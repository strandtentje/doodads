using System.Text;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations;

public class LengthValidatorCollection(uint minLength, uint maxLength) : IValidatingCollection
{
    private readonly List<object> BackingValues = new();

    public void Add(object value, out object transformed)
    {
        transformed = value;
        if (!IsSatisfied) return;

        if (value is IEnumerator<byte> binaryEnumerator)
        {
            var tempFilename = Path.GetTempFileName();
            var tempFileWriter = File.Create(tempFilename);
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
                if (!IsSatisfied) File.Delete(tempFilename);
            }
            transformed = new FileInfo(tempFilename);
        }
        else if (value is IEnumerator<char> enumerator)
        {
            var sbl = new StringBuilder();
            while (enumerator.MoveNext())
            {
                sbl.Append(enumerator.Current);
                if (sbl.Length > maxLength)
                {
                    IsSatisfied = false;
                    return;
                }
            }

            if (sbl.Length < minLength)
            {
                IsSatisfied = false;
                return;
            }

            transformed = sbl.ToString();
        }
        else
        {
            string asString = value.ToString() ?? "";
            if (asString.Length < minLength)
                IsSatisfied = false;
            if (asString.Length > maxLength)
                IsSatisfied = false;
            transformed = asString;
        }

        if (IsSatisfied)
            BackingValues.Add(transformed);
    }

    public bool IsSatisfied { get; private set; } = true;
    public IEnumerable ValidItems => BackingValues;
}