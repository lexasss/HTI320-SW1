Console.WriteLine("Ultrahaptics communicator is running...");

// Listing Ultrahaptics devices
var devices = UH.DeviceManager.Devices;
if (devices.Length == 0)
{
    Console.WriteLine("No Ultrahaptics devices found.");
    return;
}

foreach (var device in devices)
    Console.WriteLine($"Device: {device.ID}");

// Create a USP file player
var deviceId = devices.First().ID;
var uspPlayer = new UH.UspPlayer(deviceId);
uspPlayer.Finished += (s, e) => Console.WriteLine("Finished playing the stimulation");

// Load stimulations from the "usp" directory and play the POINT stimulation
var stimulations = new UH.StimulationCollection("usp");
var stimulation = stimulations.Point;
uspPlayer.Play(stimulation, offsetZ: 0.2f); // set focal point 20 cm above the device surface

// Instructions for users
Console.WriteLine($"Playing {stimulation.Name.ToUpper()}");
Console.WriteLine("Press any key to exit...");

// Wait for a key press
Console.CursorVisible = false;
Console.ReadKey();

// Cleanup
uspPlayer.Stop();
uspPlayer.Dispose();

// The app exists here