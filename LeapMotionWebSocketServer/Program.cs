
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

// All c# code must be within some class
partial class Program
{
    // Data structure (to be used to pack data to send to WebSocket clients)
    record class Joint(string Name, double X, double Y, double Z);

    // Long-living variables
    static readonly ConcurrentDictionary<Guid, WebSocket> _clients = new();
    static IHostApplicationLifetime _lifetime = default!;

    // Program entry point
    public async static Task Main(string[] args)
    {
        // Create a web application with WebSocket server
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.UseWebSockets();
        app.Map("/leapmotion", HandleClient);

        _lifetime = app.Lifetime;

        // Create Leap Motion controller
        var leap = new Leap.Controller();
        leap.FrameReady += OnFrame;

        // Run the app untill Ctrl+C is pressed
        try
        {
            await app.RunAsync();
        }
        finally
        {
            leap.Dispose();
        }
    }

    // Accepts client connection,
    // receives for any possible data (though ignores it) in a loop
    // until the client disconnects or the app shuts down,
    // and finally closes the connection
    static async Task HandleClient(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = 400;
            return;
        }

        using var socket = await context.WebSockets.AcceptWebSocketAsync();
        var id = Guid.NewGuid();

        _clients[id] = socket;
        Console.WriteLine($"Client connected: {id}");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(
            context.RequestAborted,
            _lifetime.ApplicationStopping);

        try
        {
            // Keep the connection open while receiving client messages.
            // (the connection will end immediately if the receiving functionality is missing)
            var buffer = new byte[1024];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    cts.Token);

                if (result.MessageType == WebSocketMessageType.Close)
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            // Client disconnected or request was cancelled.
        }
        catch (WebSocketException)
        {
            // Connection was closed unexpectedly.
        }
        finally
        {
            _clients.TryRemove(id, out _);

            if (_lifetime.ApplicationStopping.IsCancellationRequested)
            {
                // Terminate any connection upon shutdown
                socket.Abort();
            }
            else if (socket.State == WebSocketState.Open ||
                     socket.State == WebSocketState.CloseReceived)
            {
                // Close the connection gracefully of there was some error
                try
                {
                    await socket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Connection closed",
                        CancellationToken.None);
                }
                catch (WebSocketException)
                {
                    socket.Abort();
                }
            }

            Console.WriteLine($"Client disconnected: {id}");
        }
    }

    // Called when new hand data is available
    static async void OnFrame(object? sender, Leap.FrameEventArgs args)
    {
        var frame = args.frame;

        if (frame.Hands.Count > 0)
        {
            // Get hand data
            var hand = frame.Hands[0];
            var palmPos = hand.PalmPosition;

            // Create a list of joints to be sent to clients
            List<Joint> joints = [
                new Joint("Palm", palmPos.x, palmPos.y, palmPos.z),
            ];

            foreach (var finger in hand.Fingers)
            {
                var pos = finger.TipPosition;
                joints.Add(new Joint(finger.Type.ToString(), pos.x, pos.y, pos.z));
            }

            // Pack the list as JSON
            var json = JsonSerializer.Serialize(joints);
            var message = Encoding.UTF8.GetBytes(json);

            // Send the joints data to clients
            foreach (var (id, socket) in _clients)
            {
                if (socket.State != WebSocketState.Open)
                    continue;

                try
                {
                    await socket.SendAsync(
                        new ArraySegment<byte>(message),
                        WebSocketMessageType.Text,
                        endOfMessage: true,
                        CancellationToken.None);
                }
                catch (WebSocketException)
                {
                    // The connection handler will clean up the client.
                }
            }
        }
    }
}
