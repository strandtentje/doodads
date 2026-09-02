using Ejije.Logging;

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
    private static readonly Log
        StartedOnPrefixes = Log.Cool("Webserver started on {prefixes}"),
        AccessDenied = Log.Warn("Received {exception} while starting webserver; likely due to url acl violation"),
        OtherFailure = Log.Fail("Received {exception} while starting webserver; restarting in {ms}."),
        FixAcl = Log.Oops("Attempting to fix the acl for webserver"),
        StartingWebserverAttempt = Log.Tech("Starting webserver {attempt}"),
        FailedToStart = Log.Fail("Failed to start the server after 10 attempt; will stop trying.");

    private void StartListening()
    {
        if (CurrentState != ServerCommand.Start) throw new InvalidOperationException("Current state must be started");
        lock (ControlLock)
        {
            if (CurrentControlBox != null) throw new InvalidOperationException("A control box already exists");
            CurrentControlBox = new(new(threadCount, threadCount), new());
            foreach (string prefix in prefixes)
                CurrentControlBox.Listener.Prefixes.Add(prefix);
            bool isStarted = false;
            for (int i = 0; i < 10; i++)
            {
                Log.Post(StartingWebserverAttempt, i);
                try
                {
                    CurrentControlBox.Listener.Start();
                    Log.Post(StartedOnPrefixes, string.Join(',', prefixes));
                    isStarted = true;
                    break;
                }
                catch (HttpListenerException ex)
                {
                    if (ex.ErrorCode == 5)
                    {
                        Log.Post(AccessDenied, ex);
                        Log.Post(FixAcl);
                        UrlAccessGuarantor.WipeUrlFile();
                        UrlAccessGuarantor.EnsureUrlAcls(prefixes);
                    }
                    else
                    {
                        var timeout = Random.Shared.Next(1280, 2560);
                        Log.Post(OtherFailure, timeout);
                        Thread.Sleep(timeout);
                    }
                }
            }
            if (!isStarted)
            {
                Log.Post(FailedToStart);
                return;
            }
        }
        Task.Run(ServerLoop);
    }
    private static readonly Log
        StartingWebserverLoop = Log.Tech("Starting webserver loop");
    private void ServerLoop()
    {
        Log.Post(StartingWebserverLoop);
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
    private static readonly Log
        NoOrigin = Log.Warn("Request without originating server"),
        GetContextFail = Log.Fail("Failed to get http context due to {exception}"),
        GetContextStop = Log.Tech("Http context not gotten because the server was stopped."),
        RejectUnstarted = Log.Oops("Webserver not started; rejecting request"),
        RejectWrongInstance = Log.Oops("Wrong http listener instance; rejecting request"),
        KillStrangeFail = Log.Warn("The unexpected HTTP server that sent us a request, could not be killed. due to {exception}"),
        ResponseCloseFail = Log.Oops("Http Response couldnt be closed; it probably was already dead.");
    private void RequestContextOpened(IAsyncResult ar)
    {
        if (ar.AsyncState is not ControlBox sourceControlBox)
        {
            Log.Post(NoOrigin);
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
                Log.Post(GetContextFail, ex);
            else
                Log.Post(GetContextStop);
            return;
        }

        if (CurrentState != ServerCommand.Start)
        {
            Log.Post(RejectUnstarted);
            context.Response.StatusCode = 500;
            context.Response.Close();
            return;
        }

        if (sourceControlBox != CurrentControlBox)
        {
            Log.Post(RejectWrongInstance);
            context.Response.StatusCode = 500;
            context.Response.Close();
            try
            {
                sourceControlBox.Listener.Stop();
            }
            catch (Exception ex)
            {
                Log.Post(KillStrangeFail);
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
                Log.Post(ResponseCloseFail);
            }
        }
    }
    private static readonly Log
        StopWaitControl = Log.Tech("Stopping webserver; waiting for control lock"),
        StopDoneControl = Log.Tech("Stopping webserver; control lock ours"),
        StopWaitLoop = Log.Tech("Stopping webserver; waiting for loop stop"),
        StopDoneLoop = Log.Tech("Stopping webserver; loop stopped"),
        StopWaitControlAgain = Log.Tech("Stopping webserver; waiting for control lock again"),
        StopDoneControlEmpty = Log.Tech("Stopping webserver; control empied, webserver should be down.");
    private void StopListening()
    {
        if (CurrentState == ServerCommand.Start)
            throw new InvalidOperationException("Can't stop when state is set to started.");

        Log.Post(StopWaitControl);
        lock (ControlLock)
        {
            if (CurrentControlBox == null) throw new InvalidOperationException();
            Log.Post(StopDoneControl);
        }

        Log.Post(StopWaitLoop);
        lock (LoopLock)
        {
            Log.Post(StopDoneLoop);
        }

        Log.Post(StopWaitControlAgain);
        lock (ControlLock)
        {
            Log.Post(StopDoneControlEmpty);
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