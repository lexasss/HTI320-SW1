namespace UH;

public struct Point(float x, float y)
{
    public float X = x;
    public float Y = y;
}

public abstract class Emitter : IDisposable
{
    public UspFile Usp => _usp;

    public event EventHandler Stopped = delegate { };

    public Emitter(string deviceID, UspFile usp)
    {
        _deviceID = deviceID;
        _usp = usp;
        _usp.LoopGap = 0;
    }

    public abstract void Dispose();

    public virtual bool Start()
    {
        _sliceIndex = 0;
        _playedTimes = 0;

        return true;
    }

    public virtual bool Stop()
    {
        return true;
    }

    public virtual bool Pause()
    {
        var result = !_isPaused;
        _isPaused = true;
        return result;
    }

    public virtual bool Resume()
    {
        var result = _isPaused;
        _isPaused = false;
        return result;
    }

    public virtual void SetLocations(Point[] locations)
    {
        // We get values in the range 0-100
        // We have to transform it to offset from the center in meters,
        // where 50 means no offset
        // Since the device has about 16.5 cm in size, then 0 should 
        // result in -0.08m offset, and 100 in 0.08m offset
        _locations = locations
            .Select(item => new Point(0.08f * (item.X - 50) / 50, 0.08f * (item.Y - 50) / 50))
            .ToArray();
    }

    public virtual void ClearLocations()
    {
        _locations = [];
    }

    public virtual void SetVerticalOffset(float offsetZ)
    {
        _offsetZ = offsetZ;
    }

    public virtual void SetHorizontalOffset(float offsetX, float offsetY)
    {
        _offsetX = offsetX;
        _offsetY = offsetY;
    }

    #region Internal

    protected string _deviceID;
    protected UspFile _usp;

    protected Point[] _locations = [];

    protected float _offsetX = 0f;
    protected float _offsetY = 0f;
    protected float _offsetZ = 0f;
    protected float _dampedOffsetX = 0;
    protected float _dampedOffsetY = 0;
    protected float _dampedOffsetZ = 0;
    protected bool _isPaused = false;

    protected int _sliceIndex = 0;
    protected int _playedTimes = 0;

    protected static Ultrahaptics.Vector3 Transform(float[] matrix, Ultrahaptics.Vector3 vector)
    {
        return new Ultrahaptics.Vector3(
            matrix[0] * vector.x + matrix[1] * vector.y + matrix[2] * vector.z + matrix[3],
            matrix[4] * vector.x + matrix[5] * vector.y + matrix[6] * vector.z + matrix[7],
            matrix[8] * vector.x + matrix[9] * vector.y + matrix[10] * vector.z + matrix[11]
        );
    }

    protected virtual void OnStopped(EventArgs e)
    {
        Stopped(this, e);
    }

    #endregion
}
