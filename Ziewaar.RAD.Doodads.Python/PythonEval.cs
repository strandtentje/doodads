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
                x => PyObject.From(x.Value)));

        using var output = pei.Environment.ExecuteExpression(scriptText, idd);
        var result = output.ToString();
        
        OnThen?.Invoke(this, new CommonInteraction(interaction, result));
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}