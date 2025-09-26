#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;
#pragma warning disable 67

[Category("Printing & Formatting")]
[Title("Cache by keys, invalidate by keys")]
[Description("Uses ReadCache, WriteCache and UsingCache internally to associate OnThen's stream source with keys and validation keys for caching.")]
public class SinkCache : IService
{
    [NamedSetting("keys", "Memory names of cache member identity")]
    // ReSharper disable once UnusedMember.Local; this serves a documentative purpose.
    private readonly UpdatingKeyValue CacheIdentityKeysConstant = new("keys");
    [NamedSetting("validatekeys", "Memory names to validate cache contents with")]
    // ReSharper disable once UnusedMember.Local; this serves a documentative purpose.
    private readonly UpdatingKeyValue CacheValidateKeysConstant = new("validatekeys");
    private readonly UsingSinkCache CacheChecker = new();
    private readonly ReadCache ReadCache = new();
    private readonly WriteCache WriteCache = new();
    [EventOccasion("Hook up sinker to cache from here.")]
    public event CallForInteraction? OnThen;
    [NeverHappens()]
    public event CallForInteraction? OnElse;
    [EventOccasion("When the internl UsinkSinkCache, ReadCache or WriteCache caused a problem.")]
    public event CallForInteraction? OnException;

    public SinkCache()
    {
        CacheChecker.OnThen += (_, e) => ReadCache.Enter(new(new object()), e);
        CacheChecker.OnElse += (_, e) => WriteCache.Enter(new(new object()), e);
        WriteCache.OnThen += this.OnThen;
        CacheChecker.OnException += this.OnException;
        ReadCache.OnException += this.OnException;
        WriteCache.OnException += this.OnException;
    }
    public void Enter(StampedMap constants, IInteraction interaction) => CacheChecker.Enter(constants, interaction);
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}