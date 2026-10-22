using SharpDX.DirectInput;

var TAB = new string(' ', 4);

double lx = 0, ly = 0;
double rx = 0, ry = 0;
double slider = 0;
Dictionary<string, bool> buttons = [];

// Initialize DirectInput
var directInput = new DirectInput();
var attachedGamepads = directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AttachedOnly);
if (attachedGamepads.Count == 0)
{
    Console.WriteLine("No gamepads found.");
}

// Create a gamepad object
var gamepad = new Joystick(directInput, attachedGamepads[0].InstanceGuid);
gamepad.Properties.BufferSize = 128;
gamepad.Acquire();

// Run indefinite loop
var cts = new CancellationTokenSource();
_ = Task.Run(async () => ReadDataInCycle(cts.Token));

// Instructions for users
Console.WriteLine("XBox Controller reader is running...");
Console.WriteLine("Press any key to exit...");

// Wait for a key press
Console.CursorVisible = false;
Console.ReadKey();

// Cleanup
cts.Cancel();
gamepad.Unacquire();
gamepad.Dispose();
directInput.Dispose();


async void ReadDataInCycle(CancellationToken cts)
{
    await Task.Delay(100);    // just in case, as this loop may start earlier then a device is initialized

    while (!cts.IsCancellationRequested)
    {
        try
        {
            ReadData();
        }
        finally
        {
            Thread.Sleep(10);
        }
    }
}

void ReadData()
{
    gamepad.Poll();

    var datas = gamepad.GetBufferedData();

    if (datas.Length > 0)
    {
        foreach (var data in datas)
        {
            if (data.Offset == JoystickOffset.X)
                lx = ToRelative(data.Value);
            else if (data.Offset == JoystickOffset.Y)
                ly = ToRelative(data.Value);
            else if (data.Offset == JoystickOffset.Z)
                slider = ToRelative(data.Value);
            else if (data.Offset == JoystickOffset.RotationX)
                rx = ToRelative(data.Value);
            else if (data.Offset == JoystickOffset.RotationY)
                ry = ToRelative(data.Value);
            else
            {
                var name = data.Offset.ToString();
                if (name.StartsWith("Button"))
                {
                    buttons[name] = data.Value > 0;
                }
            }
        }

        Console.CursorTop = 3;
        Console.WriteLine($"LEFT JOYSTICK  : X = {lx:F3}, Y = {ly:F3} {TAB}");
        Console.WriteLine($"RIGHT JOYSTICK : X = {rx:F3}, Y = {ry:F3} {TAB}");
        Console.WriteLine($"L2 + R2        : {slider:F3} {TAB}");

        foreach (var button in buttons)
        {
            Console.WriteLine($"{button.Key,-14} : {(button.Value ? "Pressed" : "Released")} {TAB}");
        }
    }
}

double ToRelative(int value)
{
    return (double)(value - 0x7FFF) / 0x8000;
}