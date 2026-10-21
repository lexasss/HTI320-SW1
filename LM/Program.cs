var TAB = new string(' ', 4);

var leap = new Leap.Controller();
leap.FrameReady += OnFrame;

Console.WriteLine("Leap Motion tracker is running...");
Console.WriteLine("Press any key to exit...");

Console.CursorVisible = false;
Console.ReadKey();

void OnFrame(object? sender, Leap.FrameEventArgs args)
{
    var frame = args.frame;

    if (frame.Hands.Count > 0)
    {
        var hand = frame.Hands[0];
        Console.CursorTop = 3;

        var palmPos = hand.PalmPosition;
        Console.WriteLine($"PALM   : {palmPos.x:F0}, {palmPos.y:F0}, {palmPos.z:F0} {TAB}");

        foreach (var finger in hand.Fingers)
        {
            var fingerName = finger.Type.ToString().Split('_')[1];
            Console.WriteLine($"{fingerName,-7}: {finger.TipPosition.x:F0}, {finger.TipPosition.y:F0}, {finger.TipPosition.z:F0} {TAB}");
        }
    }
}
