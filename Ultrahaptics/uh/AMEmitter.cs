using System.Text;
using Ultrahaptics;

namespace UH;

public class AMEmitter : Emitter
{
    public AMEmitter(string deviceID, UspFile usp) : base(deviceID, usp)
    {
        _emitter = new AmplitudeModulationEmitter(deviceID);

        _timer = new HighResolutionTimer(GetInterval());
        _timer.Elapsed += new EventHandler<HighResolutionTimerElapsedEventArgs>(OnTimer);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine(_usp.ToString());
        sb.AppendLine("Emitter");
        sb.AppendLine("<no data>");
        return sb.ToString();
    }

    public override void Dispose()
    {
        _emitter.Dispose();

        GC.SuppressFinalize(this);
    }

    public override bool Start()
    {
        base.Start();

        OnTimer(this, new HighResolutionTimerElapsedEventArgs(0));

        _timer.Interval = GetInterval();
        _timer.Start();

        return _timer.IsRunning;
    }

    public override bool Stop()
    {
        _timer.Stop();
        _emitter.stop();

        return !_timer.IsRunning;
    }

    public override bool Pause()
    {
        _emitter.update([]);
        return base.Pause();
    }

    public override void SetVerticalOffset(float offsetZ)
    {
        base.SetVerticalOffset(offsetZ);

        _dampedOffsetZ = _dampedOffsetZ * 0.2f + _offsetZ * 0.8f;
    }

    public override void SetHorizontalOffset(float offsetX, float offsetY)
    {
        base.SetHorizontalOffset(offsetX, offsetY);

        _dampedOffsetX = _dampedOffsetX * 0.2f + _offsetX * 0.8f;
        _dampedOffsetY = _dampedOffsetY * 0.2f + _offsetY * 0.8f;
    }

    #region Internal

    readonly AmplitudeModulationEmitter _emitter;
    readonly HighResolutionTimer _timer;

    void OnTimer(object? sender, HighResolutionTimerElapsedEventArgs args)
    {
        if (_locations.Length == 0)
        {
            return;
        }

        var points = new List<AmplitudeModulationControlPoint>();

        var slice = _usp.TimeSlices[_sliceIndex];
        foreach (var point in slice.FocalPoints)
        {
            var pos = point.Position;
            var transformedPos = new Vector3(pos.X + _dampedOffsetX, pos.Y + _dampedOffsetY, pos.Z + _dampedOffsetZ);
            points.Add(new AmplitudeModulationControlPoint(Transform(_usp.DefaultTransform, transformedPos), point.Intensity, slice.Frequency));
        }

        _sliceIndex = (_sliceIndex + 1) % _usp.TimeSlices.Length;

        if (!_isPaused)
        {
            _emitter.update(points);
        }

        if (_sliceIndex == 0)
        {
            if (_usp.LoopGap == 0)
            {
                _playedTimes++;

                if (_usp.PlaybackType == UspFile.PlayTimes.AsSpecified && _playedTimes == _usp.LoopTimes)
                {
                    Stop();
                    OnStopped(EventArgs.Empty);
                    return;
                }
            }
        }
    }

    float GetInterval()
    {
        float interval; // ms
        if (_usp.LoopGap == 0)
        {
            interval = 1000f * _usp.StepsPerTimeslice / _usp.StepsPerSecond;
        }
        else
        {
            interval = _usp.PulseLength;
        }

        return interval;
    }

    #endregion
}