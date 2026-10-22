using System.IO.Ports;

Console.WriteLine("Smell Inspector COM port communicator is running...");

// Prompt user for COM port ID
Console.Write("Enter COM port ID: ");
var portID = Console.ReadLine();

// Port configuration and initialization
var portName = $"COM{portID}";
var port = new SerialPort()
{
    PortName = portName,
    BaudRate = 115200,
    Parity = Parity.None,
    DataBits = 8,
    StopBits = StopBits.One,
};

port.Open();

// Run a COM-port data reading loop in a parallel thread
_ = Task.Run(ReadDataInCycle);

// Instructions for users
Console.WriteLine("Press 'i' to retrieve device info, or any other key to exit...");
Console.CursorVisible = false;

// Wait for a key press
for (;;)
{
    var key = Console.ReadKey();
    Console.CursorLeft = 0;

    if (key.Key == ConsoleKey.I)        // 'i' pressed - send 'GET_INFO' command to the device
        port.WriteLine("GET_INFO");
    else                                // any other key pressed - exit the app
        break;
}

// Clean up
port.Close();
port.Dispose();

// The app exists here


void ReadDataInCycle()
{
    while (true)
    {
        Thread.Sleep(10);

        if (!port.IsOpen)
            break;

        var response = port.ReadLine();
        if (string.IsNullOrEmpty(response))
            return;

        if (response.Length > 60)
            Console.WriteLine($"{response[..60]}...");
        else
            Console.WriteLine(response);
    }
}

/*
void ListComPorts()
{
    var portNames = SerialPort.GetPortNames();
    Console.WriteLine("Available COM ports:");
    foreach (var name in portNames)
    {
        Console.WriteLine($"- {name}");
    }
}
*/