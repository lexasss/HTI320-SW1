// Constants
var TAB = new string(' ', 4);

// Create and start the EyeX host
var host = new EyeXFramework.EyeXHost();
host.Start();

// Get the connected device URL
using var etLib = new Tobii.Gaze.Core.EyeTrackerCoreLibrary();
Uri url = etLib.GetConnectedEyeTracker();
if (url == null)
{
    Console.WriteLine("Found no devices");
    return;
}

// Create the gaze data stream
var gazeStream = host.CreateGazePointDataStream(Tobii.EyeX.Framework.GazePointDataMode.LightlyFiltered);
gazeStream.Next += OnGazePoint;

// Instructions for users
Console.WriteLine("Tobii EyeX tracker is running...");
Console.WriteLine("Press any key to exit...");

// Wait for a key press
Console.CursorVisible = false;
Console.ReadKey();

// Cleanup
gazeStream.Dispose();
host.Dispose();

// The app exists here


// Functions:

void OnGazePoint(object? sender, EyeXFramework.GazePointEventArgs gazePoint)
{
    Console.CursorTop = 3;
    Console.WriteLine($"GAZE   : X = {gazePoint.X:F0}, Y = {gazePoint.Y:F0} {TAB}");
}
