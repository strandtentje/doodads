namespace Ziewaar.RAD.Doodads.Python;

public class PythonEval : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<PythonEnvironmentInteraction>(out var pei) || pei == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "python env required to run python"));
            return;
        }

        var tsi = new TextSinkingInteraction(interaction);
        OnElse?.Invoke(this, tsi);
        var scriptText = tsi.ReadAllText();

        var idd = new Dictionary<string, PyObject>(
            new InteractingDefaultingDictionary(interaction, EmptyReadOnlyDictionary.Instance).ToDictionary(x => x.Key,
                ConvertToPyObject));

        using var output = pei.Environment.ExecuteExpression(scriptText, idd);
        var result = output.ToString();

        OnThen?.Invoke(this, new CommonInteraction(interaction, result));
    }

    private PyObject ConvertToPyObject(KeyValuePair<string, object> x) =>
        PyObject.From(x.Value switch
        {
            decimal numval => (double)numval,
            { } otherVal => otherVal.ToString(),
        });

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}

public class PythonMod : IService
{
    private readonly UpdatingPrimaryValue ModnameConst = new UpdatingPrimaryValue();
    private readonly UpdatingKeyValue FuncnameConst = new UpdatingKeyValue("def");
    private readonly UpdatingKeyValue ParamsConst = new UpdatingKeyValue("args");
    private string? NewMod;
    private string? NewFunc;
    private IEnumerable<string> CurrentArgs=[];
    private string? CurFunc, CurMod;
    private PyObject? CurModInst;
    private PyObject? CurDefInst;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, ModnameConst).IsRereadRequired(out string? modnamecand))
            this.NewMod = modnamecand;
        if ((constants, FuncnameConst).IsRereadRequired(out string? funccand))
            this.NewFunc = funccand;
        if ((constants, ParamsConst).IsRereadRequired(out System.Collections.IEnumerable? args))
            this.CurrentArgs = args?.OfType<object>().Select(x => x.ToString()).OfType<string>() ?? [];

        if (string.IsNullOrWhiteSpace(NewMod) || string.IsNullOrWhiteSpace(NewFunc))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "mod and def required"));
            return;
        }

        if (!interaction.TryGetClosest<PythonEnvironmentInteraction>(out var pei) || pei == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "python env required to run python"));
            return;
        }

        if (this.NewMod != this.CurMod || this.NewFunc != this.CurFunc)
        {
            using (GIL.Acquire())
            {
                GlobalLog.Instance?.Information("Switching python modules");
                this.CurModInst?.Dispose();
                this.CurDefInst?.Dispose();
                this.CurModInst = Import.ImportModule(this.NewMod);
                this.CurDefInst = CurModInst.GetAttr(this.NewFunc);
                
                this.CurFunc = NewFunc;
                this.CurMod = NewMod;
            }
        }

        if (this.CurDefInst is null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "no def"));
            return;
        }
        
        using (GIL.Acquire())
        {
            var callArguments = new List<PyObject>();

            foreach (string currentArg in CurrentArgs)
            {
                if (interaction.TryFindVariable(currentArg, out object? argval) && argval != null)
                {
                    callArguments.Add(ConvertToPyObject(argval));
                }
            }
            GlobalLog.Instance?.Information("Invoking with {num} args", callArguments.Count);
            using var ret = CurDefInst.Call(callArguments.ToArray());
            
            OnThen?.Invoke(this, new CommonInteraction(interaction, ret.As<string>()));

            foreach (PyObject pyObject in callArguments)
                pyObject.Dispose();
        }
    }

    private PyObject ConvertToPyObject(object x) =>
        PyObject.From(x switch
        {
            decimal numval => (double)numval,
            { } otherVal => otherVal.ToString(),
        });

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}