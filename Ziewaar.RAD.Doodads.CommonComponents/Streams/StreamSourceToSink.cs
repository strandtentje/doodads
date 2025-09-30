using System.Net.Sockets;
using System.Threading;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.CommonComponents.Streams;

#nullable enable

[Category("Sourcing & Sinking")]
[Title("Pump bytes from source to sink")]
[Description("""
             So long as nothing breaks and the copy name is touched with Continue,
             this will continue pumping data from the source to the sink. For each block,
             OnThen is invoked with the byte total in register. 
             When copying stops, OnElse is hit with the total byte count in register.
             """)]
public class StreamSourceToSink : IService
{
    [PrimarySetting("Copy name to use with Continue")]
    private readonly UpdatingPrimaryValue CopyNameConstant = new();

    [NamedSetting("eof", "If a 0-length read means EOF")]
    private readonly UpdatingKeyValue ZeroIsEofConstant = new("eof");

    [NamedSetting("buffer", "Buffer size, defaults to 1024")]
    private readonly UpdatingKeyValue BufferSizeConstant = new("buffer");

    private string? CurrentCopyName;
    private bool IsZeroEof = true;
    private int BufferSize = 1024;

    [EventOccasion("When another block of data was moved; use Continue here.")]
    public event CallForInteraction? OnThen;

    [EventOccasion("After copying stopped")]
    public event CallForInteraction? OnElse;

    [EventOccasion("Likely happens when the copy name is missing, or sources/sinks were missing.")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, CopyNameConstant).IsRereadRequired(out string? copyNameCandidate))
            this.CurrentCopyName = copyNameCandidate;
        if ((constants, ZeroIsEofConstant).IsRereadRequired(() => true, out bool zeroIsEofCandidate))
            this.IsZeroEof = zeroIsEofCandidate;
        if ((constants, BufferSizeConstant).IsRereadRequired(() => 1024M, out decimal bufferSizeCandidate))
            this.BufferSize = (int)bufferSizeCandidate;

        if (this.BufferSize < 16)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Buffer size too small"));
            return;
        }

        if (this.CurrentCopyName == null || string.IsNullOrWhiteSpace(this.CurrentCopyName))
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

        byte[] buffer = new byte[this.BufferSize];

        ThreadPool.QueueUserWorkItem(_ =>
        {
            long totalCopyCount = 0;
            try
            {
                var repeatInteraction = new RepeatInteraction(this.CurrentCopyName, interaction);
                var countDict = new SortedList<string, object>() { ["currentcount"] = 0, ["totalcount"] = 0, };
                var countInteraction = new CommonInteraction(repeatInteraction, register: 0, memory: countDict);
                repeatInteraction.IsRunning = true;
                int trueReadCount = 0;
                var fsr = sourcingInteraction.SourceBuffer as IFinishSensingStream;

                while (repeatInteraction.IsRunning)
                {
                    if (fsr?.IsFinished == true)
                    {
                        try
                        {
                            sinkingInteraction.SinkBuffer.Write(buffer, 0, trueReadCount);
                            sinkingInteraction.SinkBuffer.Flush();
                        }
                        catch (Exception ex)
                        {
                            GlobalLog.Instance?.Warning(ex, "Stream EOF couldn't be forwarded anymore.");
                        }

                        break;
                    }

                    try
                    {
                        trueReadCount = sourcingInteraction.SourceBuffer.Read(buffer, 0, this.BufferSize);
                    }
                    catch (Exception ex)
                    {
                        GlobalLog.Instance?.Warning(ex, "Stream copying stopped due to broken read stream");
                        return;
                    }

                    if (trueReadCount == 0 && IsZeroEof)
                    {
                        try
                        {
                            sinkingInteraction.SinkBuffer.Write(buffer, 0, trueReadCount);
                            sinkingInteraction.SinkBuffer.Flush();
                        }
                        catch (Exception ex)
                        {
                            GlobalLog.Instance?.Warning(ex, "Stream EOF couldn't be forwarded anymore.");
                        }

                        break;
                    }

                    repeatInteraction.IsRunning = false;
                    countInteraction.Register = trueReadCount;
                    countDict["currentcount"] = trueReadCount;
                    countDict["totalcount"] = totalCopyCount + trueReadCount;
                    OnThen?.Invoke(this, countInteraction);

                    if (!repeatInteraction.IsRunning) break;

                    try
                    {
                        sinkingInteraction.SinkBuffer.Write(buffer, 0, trueReadCount);
                        sinkingInteraction.SinkBuffer.Flush();
                    }
                    catch (Exception ex)
                    {
                        GlobalLog.Instance?.Warning(ex, "Stream copying stopped due to broken write stream");
                        return;
                    }

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