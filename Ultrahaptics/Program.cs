Console.WriteLine("Ultrahaptics communicator is running...");

// Listing Ultrahaptics devices
var devices = UH.DeviceManager.Devices;
if (devices.Length == 0)
{
    Console.WriteLine("No Ultrahaptics devices found.");
    return;
}
else
{
    foreach (var device in devices)
    {
        Console.WriteLine($"Device: {device.ID}");
    }
}

// Load stimulations from the "usp" directory
var stimulations = new UH.Stimulations("usp");
var stimulation = stimulations.Point;

// Create a USP file player
var deviceId = devices.First().ID;
var uhPlayer = new UH.UspPlayer(deviceId);
uhPlayer.Finished += (s, e) => Console.WriteLine("Finished playing the stimulation");
uhPlayer.Play(stimulation, offsetZ: 0.2f);

// Instructions for users
Console.WriteLine($"Playing {stimulation.Name.ToUpper()}");
Console.WriteLine("Press any key to exit...");

// Wait for a key press
Console.CursorVisible = false;
Console.ReadKey();

// Cleanup
uhPlayer.Stop();
uhPlayer.Dispose();

// The app exists here