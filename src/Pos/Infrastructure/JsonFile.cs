using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Pos.Infrastructure;

public class JsonFile<T> where T : new()
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
    };

    private readonly string _path;

    public JsonFile(string directory, string fileName)
    {
        _path = Path.Combine(directory, fileName);
    }

    public T Load()
    {
        if (!File.Exists(_path))
            return new T();
        return JsonSerializer.Deserialize<T>(File.ReadAllText(_path), Options) ?? new T();
    }

    public void Save(T data)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_path))!);
        File.WriteAllText(_path, JsonSerializer.Serialize(data, Options));
    }
}
