using System.Runtime.Serialization;
using System.Text;
using Ziewaar.RAD.Doodads.EnumerableStreaming.Readers;
using Ziewaar.RAD.Doodads.EnumerableStreaming.StreamingMultipart;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations;

public class LengthValidatorCollection(uint minLength, uint maxLength) : IValidatingCollection
{
    private readonly List<object> BackingValues = new();

    public void Add(object value, out object transformed)
    {
        transformed = value;
        if (!IsSatisfied) return;

        if (value is ICountingEnumerator<byte> binaryEnumerator)
        {
            string tempFileWithProposedName;
            string? pathTemp = null;
            if (binaryEnumerator is ITaggedCountingEnumerator<byte> tagged && tagged.Tag is string proposedFilename &&
                !string.IsNullOrWhiteSpace(proposedFilename))
            {
                var mainTemp = Path.GetTempPath();
                var subTemp = Path.GetRandomFileName();
                pathTemp = Path.Combine(mainTemp, subTemp);
                Directory.CreateDirectory(pathTemp);
                tempFileWithProposedName = Path.Combine(pathTemp, proposedFilename);
            }
            else
            {
                tempFileWithProposedName = Path.GetTempFileName();
            }
            
            var tempFileWriter = File.Create(tempFileWithProposedName);
            var pool = ByteArrayPoolFactory.Instance.GetOrCreate(512, 4096, TimeSpan.FromSeconds(30));
            var writeBuffer = pool.Rent();
            int writeBufferCursor = 0;
            uint totalBufferThrougput = 0;
            try
            {
                while (binaryEnumerator.MoveNext())
                {
                    writeBuffer[writeBufferCursor++] = binaryEnumerator.Current;
                    totalBufferThrougput++;
                    if (totalBufferThrougput >= maxLength)
                    {
                        Reason = "file too long";
                        IsSatisfied = false;
                        return;
                    }

                    if (writeBufferCursor >= writeBuffer.Length)
                    {
                        tempFileWriter.Write(writeBuffer, 0, writeBufferCursor);
                        writeBufferCursor = 0;
                    }
                }

                if (totalBufferThrougput < minLength)
                {
                    Reason = "file too short";
                    IsSatisfied = false;
                    return;
                }

                if (writeBufferCursor > 0)
                {
                    tempFileWriter.Write(writeBuffer, 0, writeBufferCursor);
                }
            }
            finally
            {
                pool.Return(writeBuffer);
                tempFileWriter.Close();
                if (!IsSatisfied)
                {
                    File.Delete(tempFileWithProposedName);
                    if (pathTemp != null) Directory.Delete(pathTemp, true);
                }
            }
            transformed = new FileInfo(tempFileWithProposedName);
        }
        else if (value is IEnumerator<char> enumerator)
        {
            var sbl = new StringBuilder();
            while (enumerator.MoveNext())
            {
                sbl.Append(enumerator.Current);
                if (sbl.Length > maxLength)
                {
                    Reason = "too many chars";
                    IsSatisfied = false;
                    return;
                }
            }

            if (sbl.Length < minLength)
            {
                Reason = "too few chars";
                IsSatisfied = false;
                return;
            }

            transformed = sbl.ToString();
        }
        else
        {
            string asString = value.ToString() ?? "";
            if (asString.Length < minLength)
            {
                Reason = "string short";
                IsSatisfied = false;
            }

            if (asString.Length > maxLength)
            {
                Reason = "string long";
                IsSatisfied = false;
            }
            transformed = asString;
        }

        if (IsSatisfied)
            BackingValues.Add(transformed);
    }

    public bool IsSatisfied { get; private set; } = true;
    public string Reason { get; private set; } = "";
    public IEnumerable ValidItems => BackingValues;
}