
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

partial class Program
{
    record class Joint(string Name, double X, double Y, double Z);

    static readonly ConcurrentDictionary<Guid, WebSocket> _clients = new();

    public async static Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.UseWebSockets();

        // Accept WebSocket connections.
        app.Map("/leapmotion", AddClient);

        try
        {
            // Create Leap Motion controller
            var leap = new Leap.Controller();
            leap.FrameReady += OnFrame;

            await app.RunAsync();
        }
        finally
        {
            // Stop the broadcast loop when the server shuts down.
            // In a production app, use a hosted BackgroundService instead.
        }

    }

    static async Task AddClient(HttpContext context)
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

        try
        {
            // Keep the connection open while receiving client messages.
            var buffer = new byte[1024];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    context.RequestAborted);

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

            if (socket.State == WebSocketState.Open ||
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
                    // The connection may already be closed.
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
