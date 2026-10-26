using System.Text.Json;

namespace UH;

public class Stimulation
{
    public static readonly string RESOURCE_PREFIX = "sim";
    public string Name { get; private set; }
    public UspFile Usp { get; private set; }
    public Point[] Locations { get; private set; }
    
    public Stimulation(string filename, Point[] locations)
    {
        var p = filename.Split('\\', '/').Last().Split('.');             // remove path
        if (p.Length > 1)
        {
            p = p.Where((item, i) => i < (p.Length - 1)).ToArray();     // remove the extension
        }
        Name = string.Join(".", p);                                     // collapse back to the filename

        Locations = locations;

        Usp = Read(new StreamReader(filename));
    }

    public Stimulation(string name, Stream stream, Point[] locations)
    {
        Name = name;
        Locations = locations;
        Usp = Read(new StreamReader(stream));
    }

    public override string ToString()
    {
        return Name;
    }

    static UspFile Read(StreamReader reader)
    {
        var json = reader.ReadToEnd();
        var result = JsonSerializer.Deserialize<UspFile>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        if (result == null || result.SupportedEmitters.Length == 0)
        {
            throw new InvalidDataException("USP file does not contain any supported emitters.");
        }
        return result;
    }
}
