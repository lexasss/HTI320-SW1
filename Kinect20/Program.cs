using Microsoft.Kinect;
using System;
using System.Linq;

// Initialize variables
Body[] bodies = null;

// Initialize Kinect sensor
var kinectSensor = KinectSensor.GetDefault();
var bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader();
bodyFrameReader.FrameArrived += OnFrameArrived;

// Start body detection process
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

// The app exists here


// Functions:

void OnFrameArrived(object sender, BodyFrameArrivedEventArgs e)
{
    // Get the body frame
    using BodyFrame bodyFrame = e.FrameReference.AcquireFrame();
    if (bodyFrame == null)
        return;

    // Initialize the array when the first frame arrives
    bodies ??= new Body[bodyFrame.BodyCount];

    // Receive all bodies' data
    bodyFrame.GetAndRefreshBodyData(bodies);

    // Take the first visible body,
    var body = bodies.FirstOrDefault(b => b.IsTracked);
    if (body == null)
        return;

    // ... get the right hand tip joint,
    var rightHandTipJoint = body.Joints[JointType.HandTipRight];
    if (rightHandTipJoint.TrackingState == TrackingState.NotTracked)
        return;

    // ... and get its 3D position
    CameraSpacePoint position = rightHandTipJoint.Position;
    if (position.Z < 0)     // for some reason, the distance is negative sometimes...
        position.Z = 0.1f;  // so, lets fix it by setting a small positive value

    // Finally, print this position out
    Console.CursorTop = 3;
    Console.WriteLine($"RIGHT HAND TIP : X = {position.X:F3}, Y = {position.Y:F3}, Z = {position.Z:F3}");
}
