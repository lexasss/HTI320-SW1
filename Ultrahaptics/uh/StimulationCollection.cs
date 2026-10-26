using System.Collections;
using System.Text.RegularExpressions;

namespace UH;

public class StimulationCollection
{
    public Stimulation Point => _stimulations.Single(item => item.Name == "point");
    public Stimulation LineY => _stimulations.Single(item => item.Name == "line-y");
    public Stimulation LineX => _stimulations.Single(item => item.Name == "line-x");
    public Stimulation Rectangle => _stimulations.Single(item => item.Name == "rect");
    public Stimulation ScanUp => _stimulations.Single(item => item.Name == "scan-up");
    public Stimulation ScanDown => _stimulations.Single(item => item.Name == "scan-down");
    public Stimulation ScanLeft => _stimulations.Single(item => item.Name == "scan-left");
    public Stimulation ScanRight => _stimulations.Single(item => item.Name == "scan-right");

    public StimulationCollection(System.Resources.ResourceSet resources)
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

    public StimulationCollection(string folder)
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
