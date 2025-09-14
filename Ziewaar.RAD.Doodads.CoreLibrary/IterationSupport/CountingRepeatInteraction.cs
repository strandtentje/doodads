#nullable enable
namespace Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;
public class CountingRepeatInteraction : RepeatInteraction
{
    public CountingRepeatInteraction(string repeatName, IInteraction parent) : base(repeatName, parent) =>
        Memory = new SwitchingDictionary(
            [repeatName], key => key == repeatName ? RepeatCounter : throw new KeyNotFoundException());
    public decimal RepeatCounter { get; private set; }
    public bool ContinueRepeating()
    {
        if (IsRunning)
        {
            this.RepeatCounter++;
            IsRunning = false;
            return true;
        }
        else
        {
            return false;
        }
    }
    public override IReadOnlyDictionary<string, object> Memory { get; }
}
public static class RepeatInteractionExtensions
{
    public static void RepeatInto<TType>(
        this (object sender, string name, IEnumerable<TType> items) repeat,
        IInteraction offset,
        CallForInteraction? doneBranch,
        CallForInteraction? itemBranch,
        Func<TType, object>? getRegister = null,
        Func<TType, (string key, object value)[]>? getMemory = null)
    {
        using (var enumerator = repeat.items.GetEnumerator())
        {
            var ri = new CountingRepeatInteraction(repeat.name, offset);
            ri.IsRunning = true;
            while (enumerator.MoveNext() && ri.ContinueRepeating())
            {
                switch ((getRegister, getMemory))
                {
                    case (Func<TType, object> actualGetRegister, null):
                        itemBranch?.Invoke(repeat.sender, new CommonInteraction(
                            ri, register: actualGetRegister(enumerator.Current)));
                        break;
                    case (null, Func<TType, (string key, object value)[]> actualGetMemory):
                        itemBranch?.Invoke(repeat.sender, new CommonInteraction(
                            ri, memory: actualGetMemory(enumerator.Current).ToDictionary(x => x.key, x => x.value)));
                        break;
                    case (Func<TType, object> actualGetRegister,
                        Func<TType, (string key, object value)[]> actualGetMemory):
                        itemBranch?.Invoke(repeat.sender, new CommonInteraction(
                            ri, memory: actualGetMemory(enumerator.Current).ToDictionary(x => x.key, x => x.value),
                            register: actualGetRegister(enumerator.Current)));
                        break;
                    default:
                        itemBranch?.Invoke(repeat.sender, ri);
                        break;
                }
            }
            doneBranch?.Invoke(repeat.sender, ri);
        }
    }
}