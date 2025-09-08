#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;

[Category("Printing & Formatting")]
[Title("Cache by keys, invalidate by keys")]
[Description("Uses ReadCache, WriteCache and UsingCache internally to associate OnThen's stream source with keys and validation keys for caching.")]
public class SinkCache : IService
{
    private readonly UsingSinkCache CacheChecker = new();
    private readonly ReadCache ReadCache = new();
    private readonly WriteCache WriteCache = new();
    
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public SinkCache()
    {
        CacheChecker.OnThen += (s, e) => ReadCache.Enter(new(new object()), e);
        CacheChecker.OnElse += (s, e) => WriteCache.Enter(new(new object()), e);
        WriteCache.OnThen += this.OnThen;
        CacheChecker.OnException += this.OnException;
        ReadCache.OnException += this.OnException;
        WriteCache.OnException += this.OnException;
    }
    public void Enter(StampedMap constants, IInteraction interaction) => CacheChecker.Enter(constants, interaction);
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}