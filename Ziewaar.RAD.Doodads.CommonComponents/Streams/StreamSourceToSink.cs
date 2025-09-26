using System.Threading;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.Cryptography;

#nullable enable

public class StreamSourceToSink : IService
{
    private readonly UpdatingPrimaryValue CopyNameConstant = new();
    private readonly UpdatingKeyValue ZeroIsEofConstant = new("eof");
    private string? CurrentCopyName;
    private bool IsZeroEof = true;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, CopyNameConstant).IsRereadRequired(out string? copyNameCandidate))
            this.CurrentCopyName = copyNameCandidate;
        if ((constants, ZeroIsEofConstant).IsRereadRequired(out bool zeroIsEofCandidate))
            this.IsZeroEof = zeroIsEofCandidate;

        if (string.IsNullOrWhiteSpace(this.CurrentCopyName))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "copy name required as primary constant"));
            return;
        }

        if (!interaction.TryGetClosest<ISinkingInteraction>(out var sinkingInteraction) || sinkingInteraction == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "sink interaction required"));
            return;
        }

        if (!interaction.TryGetClosest<ISourcingInteraction>(out var sourcingInteraction) ||
            sourcingInteraction == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "sourcing interaction required"));
            return;
        }

        byte[] buffer = new byte[1024];

        ThreadPool.QueueUserWorkItem(_ =>
        {
            long totalCopyCount = 0;
            try
            {
                var repeatInteraction = new RepeatInteraction(this.CurrentCopyName, interaction);
                var countInteraction = new CommonInteraction(repeatInteraction, 0);
                repeatInteraction.IsRunning = true;
                int trueReadCount = 0;
                while (repeatInteraction.IsRunning)
                {
                    trueReadCount = sourcingInteraction.SourceBuffer.Read(buffer, 0, 1024);

                    if (trueReadCount == 0 && IsZeroEof)
                        break;
                    
                    repeatInteraction.IsRunning = false;
                    countInteraction.Register = trueReadCount;
                    OnThen?.Invoke(this, countInteraction);

                    if (!repeatInteraction.IsRunning) break;

                    sinkingInteraction.SinkBuffer.Write(buffer, 0, trueReadCount);
                    totalCopyCount += trueReadCount;
                }
            }
            catch (Exception ex)
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, ex));
            }
            finally
            {
                OnElse?.Invoke(this, new CommonInteraction(interaction, totalCopyCount));
            }
        }, null);
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}