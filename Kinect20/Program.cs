using Microsoft.Kinect;
using System;
using System.Linq;

Body[] bodies = null;

// Initialize Kinect sensor
var kinectSensor = KinectSensor.GetDefault();
var bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader();
bodyFrameReader.FrameArrived += OnFrameArrived;

kinectSensor.Open();

// Instructions for users
Console.WriteLine("Kinect is running...");
Console.WriteLine("Press any key to exit...");

// Wait for a key press
Console.CursorVisible = false;
Console.ReadKey();

// Cleanup
bodyFrameReader.Dispose();
kinectSensor.Close();

void OnFrameArrived(object sender, BodyFrameArrivedEventArgs e)
{
    using BodyFrame bodyFrame = e.FrameReference.AcquireFrame();
    if (bodyFrame == null)
        return;

    if (bodies == null)
        bodies = new Body[bodyFrame.BodyCount];

    // Receive all bodies' data
    bodyFrame.GetAndRefreshBodyData(bodies);

    // Take the first visible body,
    var body = bodies.FirstOrDefault(b => b.IsTracked);
    if (body == null)
        return;

    // ... and get the right hand tip joint,
    var rightHandTipJoint = body.Joints[JointType.HandTipRight];
    if (rightHandTipJoint.TrackingState == TrackingState.NotTracked)
        return;

    CameraSpacePoint position = rightHandTipJoint.Position;
    if (position.Z < 0)
        position.Z = 0.1f;

    Console.CursorTop = 3;
    Console.WriteLine($"RIGHT HAND TIP : X = {position.X:F3}, Y = {position.Y:F3}, Z = {position.Z:F3}");
}
