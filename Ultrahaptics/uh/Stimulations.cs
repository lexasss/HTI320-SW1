using System.Collections;
using System.Text.Json;
using System.Text.RegularExpressions;

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

public class Stimulations
{
    public Stimulation Point => _stimulations.Single(item => item.Name == "point");
    public Stimulation Vertical => _stimulations.Single(item => item.Name == "line-y");
    public Stimulation Horizontal => _stimulations.Single(item => item.Name == "line-x");
    public Stimulation Rectangle => _stimulations.Single(item => item.Name == "rect");
    public Stimulation ScanUp => _stimulations.Single(item => item.Name == "scan-up");
    public Stimulation ScanDown => _stimulations.Single(item => item.Name == "scan-down");
    public Stimulation ScanLeft => _stimulations.Single(item => item.Name == "scan-left");
    public Stimulation ScanRight => _stimulations.Single(item => item.Name == "scan-right");

    public Stimulations(System.Resources.ResourceSet resources)
    {
        foreach (DictionaryEntry entry in resources)
        {
            if (entry.Key.ToString()?.StartsWith(Stimulation.RESOURCE_PREFIX) == true)
            {
                var name = entry.Key.ToString()?.Substring(Stimulation.RESOURCE_PREFIX.Length) ?? string.Empty;
                var re = new Regex(@"(\p{Lu}[\p{Ll}0-9_]*)");
                var words = new List<string>();
                foreach (Match match in re.Matches(name))
                {
                    words.Add(match.ToString());
                }
                var stream = new MemoryStream((byte[])entry.Value!);
                try
                {
                    var stimulation = new Stimulation(string.Join("-", words).ToLower(), stream, [new Point(50, 50)]);
                    _stimulations.Add(stimulation);
                }
                catch (InvalidDataException ex)
                {
                    Console.WriteLine($"Error occurred while processing resource '{name}': {ex.Message}");
                }
            }
        }
    }

    public Stimulations(string folder)
    {
        var files = Directory.GetFiles(folder, "*.usp");
        foreach (var file in files)
        {
            try
            {
                var stimulation = new Stimulation(file, [new Point(50, 50)]);
                _stimulations.Add(stimulation);
            }
            catch (InvalidDataException ex)
            {
                Console.WriteLine($"Error occurred while processing file '{Path.GetFileName(file)}': {ex.Message}");
            }
        }
    }

    readonly List<Stimulation> _stimulations = [];
}
