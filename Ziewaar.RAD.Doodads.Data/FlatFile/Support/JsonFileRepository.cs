using System.Collections.Generic;
using System.IO;

namespace Ziewaar.RAD.Doodads.Data.FlatFile.Support;

public class JsonFileRepository
{
    public static readonly JsonFileRepository Instance = new JsonFileRepository();

    private readonly SortedList<string, JsonFile> KnownFiles = new SortedList<string, JsonFile>();
    private readonly object KnownFileLock = new object();
    public JsonFile Retrieve(string filePath)
    {
        JsonFile foundFile;
        lock (KnownFileLock)
        {
            if (!KnownFiles.TryGetValue(filePath, out foundFile))
            {
                if (!File.Exists(filePath)) File.WriteAllText(filePath, "{}");
                KnownFiles[filePath] = foundFile = new JsonFile(filePath);
            }
        }
        return foundFile;
    }
}
