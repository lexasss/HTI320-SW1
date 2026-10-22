using System.Text;

namespace UH;

public class UspFile
{
    public enum EmitterType
    {
        AM = 0,
        TPS = 1
    }

    public enum PlayTimes
    {
        Forever = 1,
        AsSpecified = 2
    }

    public class Vector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public Vector3() { }
        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    public class FocalPoint
    {
        public float Intensity { get; set; }
        public Vector3 Position { get; set; } = new();

        public FocalPoint() { }
        public FocalPoint(float intensity, Vector3 position)
        {
            Intensity = intensity;
            Position = position;
        }
    }

    public class TimeSlice
    {
        public FocalPoint[] FocalPoints { get; set; } = [];
        public float Frequency { get; set; }
    }

    public float[] DefaultTransform { get; set; } = [];
    public bool FingerTrackingEnabled { get; set; }
    public int Fixation { get; set; }
    public bool HandTrackingEnabled { get; set; }
    public int LoopGap { get; set; }
    public int LoopTimes { get; set; }
    public PlayTimes PlaybackType { get; set; } = new();
    public int PulseLength { get; set; }
    public int StepsPerSecond { get; set; }
    public int StepsPerTimeslice { get; set; }
    public EmitterType[] SupportedEmitters { get; set; } = [];
    public TimeSlice[] TimeSlices { get; set; } = [];
    public bool YPlaneTrackingEnabled { get; set; }
    public bool ZAxisTrackingEnabled { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("USP");
        sb.AppendLine($"type: {SupportedEmitters[0]}");
        sb.AppendLine($"slice count: {TimeSlices.Length}");
        sb.AppendLine($"pulse length: {PulseLength}");
        sb.AppendLine($"steps/sec: {StepsPerSecond}");
        sb.AppendLine($"steps/slice: {StepsPerTimeslice}");
        sb.AppendLine($"loop gap: {LoopGap}");
        sb.AppendLine($"loop times: {LoopTimes}");
        sb.AppendLine($"playback: {PlaybackType}");
        sb.AppendLine($"ref: {Fixation}");
        return sb.ToString();
    }

    public float VerticalOffset => DefaultTransform[11];
}
