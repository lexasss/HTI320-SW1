var TAB = new string(' ', 4);

// Create Leap Motion controller
var leap = new Leap.Controller();
leap.FrameReady += OnFrame;

// Instructions for users
Console.WriteLine("Leap Motion tracker is running...");
Console.WriteLine("Press any key to exit...");

// Wait for a key press
Console.CursorVisible = false;
Console.ReadKey();

// The app exists here

void OnFrame(object? sender, Leap.FrameEventArgs args)
{
    var frame = args.frame;

    if (frame.Hands.Count > 0)
    {
        var hand = frame.Hands[0];
        var palmPos = hand.PalmPosition;

        Console.CursorTop = 3;
        Console.WriteLine($"PALM   : X = {palmPos.x:F0}, Y = {palmPos.y:F0}, Z = {palmPos.z:F0} {TAB}");

        foreach (var finger in hand.Fingers)
        {
            var fingerName = finger.Type.ToString().Split('_')[1];
            Console.WriteLine($"{fingerName,-7}: X = {finger.TipPosition.x:F0}, Y = {finger.TipPosition.y:F0}, Z = {finger.TipPosition.z:F0} {TAB}");
        }
    }
}
