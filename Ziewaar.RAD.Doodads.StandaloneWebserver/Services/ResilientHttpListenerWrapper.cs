namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
#pragma warning disable 67
public class ResilientHttpListenerWrapper(string[] prefixes, int threadCount) : IControlCommandReceiver<ServerCommand>
{
    private readonly Lock ControlLock = new(), LoopLock = new();
    private ControlBox? CurrentControlBox = null;
    public ServerCommand CurrentState { get; private set; } = ServerCommand.None;
    public event EventHandler<HttpListenerContext>? NewContext;
    public event EventHandler<Exception>? Fatality;
    public void GiveCommand(ServerCommand command)
    {
        switch (command)
        {
            case ServerCommand.Start when CurrentState != ServerCommand.Start:
                CurrentState = ServerCommand.Start;
                StartListening();
                break;
            case ServerCommand.Stop when CurrentState != ServerCommand.Stop:
                CurrentState = ServerCommand.Stop;
                StopListening();
                break;
        }
    }
    private void StartListening()
    {
        if (CurrentState != ServerCommand.Start) throw new InvalidOperationException("Current state must be started");
        lock (ControlLock)
        {
            if (CurrentControlBox != null) throw new InvalidOperationException("A control box already exists");
            CurrentControlBox = new(new(threadCount, threadCount), new());
            foreach (string prefix in prefixes)
                CurrentControlBox.Listener.Prefixes.Add(prefix);
            CurrentControlBox.Listener.Start();
        }
        Task.Run(ServerLoop);
    }
    private void ServerLoop()
    {
        GlobalLog.Instance?.Information("Starting webserver loop");
        lock (LoopLock)
        {
            while (true)
            {
                lock (ControlLock)
                {
                    if (CurrentControlBox == null || CurrentState != ServerCommand.Start)
                        break;
                    if (CurrentControlBox?.Slots.WaitOne(100) == true)
                        CurrentControlBox?.Listener.BeginGetContext(RequestContextOpened, CurrentControlBox!);
                }
            }
        }

        lock (ControlLock)
        {
            CurrentControlBox?.Listener.Stop();
            CurrentControlBox?.Slots.Dispose();
        }
    }
    private void RequestContextOpened(IAsyncResult ar)
    {
        if (ar.AsyncState is not ControlBox sourceControlBox)
        {
            GlobalLog.Instance?.Warning("Request without originating server.");
            return;
        }

        HttpListenerContext context;
        try
        {
            context = sourceControlBox.Listener.EndGetContext(ar);
            sourceControlBox.Slots.Release();
        }
        catch (Exception ex)
        {
            if (CurrentState == ServerCommand.Start)
                GlobalLog.Instance?.Error(ex, "When trying to get the http context");
            else
                GlobalLog.Instance?.Information("Stopped waiting for context due to server stop.");
            return;
        }

        if (CurrentState != ServerCommand.Start)
        {
            GlobalLog.Instance?.Warning("Rejecting request while server not started");
            context.Response.StatusCode = 500;
            context.Response.Close();
            return;
        }

        if (sourceControlBox != CurrentControlBox)
        {
            GlobalLog.Instance?.Warning("Rejecting request from wrong listener instance");
            context.Response.StatusCode = 500;
            context.Response.Close();
            try
            {
                sourceControlBox.Listener.Stop();
            }
            catch (Exception ex)
            {
                GlobalLog.Instance?.Error(ex, "Attempted to kill server behind strange request but failed.");
            }

            return;
        }

        try
        {
            NewContext?.Invoke(this, context);
        }
        catch (Exception ex)
        {
            Fatality?.Invoke(this, ex);
        }
        finally
        {
            try
            {
                context.Response.Close();
            }
            catch (Exception ex)
            {
                GlobalLog.Instance?.Error(ex, "Couldn't Close Response");
            }
        }
    }
    private void StopListening()
    {
        if (CurrentState == ServerCommand.Start)
            throw new InvalidOperationException("Can't stop when state is set to started.");

        GlobalLog.Instance?.Information("Stopping webserver; waiting for control to become available.");
        lock (ControlLock)
        {
            if (CurrentControlBox == null) throw new InvalidOperationException();
            GlobalLog.Instance?.Information("Stopping webserver; control available.");
        }

        GlobalLog.Instance?.Information("Stopping webserver; waiting for loop to exit.");
        lock (LoopLock)
        {
            GlobalLog.Instance?.Information("Stopping webserver; loop ended.");
        }

        GlobalLog.Instance?.Information("Stopping webserver; waiting for control to become available again");
        lock (ControlLock)
        {
            GlobalLog.Instance?.Information("Stopping webserver; control emptied.");
            CurrentControlBox = null;
        }
    }
    public void Dispose()
    {
        CurrentState = ServerCommand.Stop;
        StopListening();
        NewContext = null;
        Fatality = null;
    }
}