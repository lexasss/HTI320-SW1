
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

partial class Program
{
    record class Joint(string Name, double X, double Y, double Z);

    static readonly ConcurrentDictionary<Guid, WebSocket> _clients = new();
    static IHostApplicationLifetime _lifetime = default!;

    public async static Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.UseWebSockets();
        app.Map("/leapmotion", HandleClient);

        _lifetime = app.Lifetime;

        // Create Leap Motion controller
        var leap = new Leap.Controller();
        leap.FrameReady += OnFrame;

        try
        {
            // Run the app: the execution stop here until Ctrl+C is pressed
            await app.RunAsync();
        }
        finally
        {
            leap.Dispose();
        }
    }

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
                // Do not wait for a WebSocket close handshake during shutdown.
                socket.Abort();
            }
            else if (socket.State == WebSocketState.Open ||
                     socket.State == WebSocketState.CloseReceived)
            {
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

    static async void OnFrame(object? sender, Leap.FrameEventArgs args)
    {
        var frame = args.frame;

        if (frame.Hands.Count > 0)
        {
            var hand = frame.Hands[0];
            var palmPos = hand.PalmPosition;

            List<Joint> joints = [
                new Joint("Palm", palmPos.x, palmPos.y, palmPos.z),
            ];

            foreach (var finger in hand.Fingers)
            {
                var pos = finger.TipPosition;
                joints.Add(new Joint(finger.Type.ToString(), pos.x, pos.y, pos.z));
            }

            var json = JsonSerializer.Serialize(joints);
            var message = Encoding.UTF8.GetBytes(json);

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
