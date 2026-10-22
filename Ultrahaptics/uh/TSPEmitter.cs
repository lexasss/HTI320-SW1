using System.Text;
using Ultrahaptics;

namespace UH;

public class TSPEmitter : Emitter
{
    public TSPEmitter(string deviceID, UspFile usp) : base(deviceID, usp)
    {
        _emitter = new TimePointStreamingEmitter(deviceID);
        _emitter.setEmissionCallback(OnEmission, null);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine(_usp.ToString());
        sb.AppendLine("Emitter");
        sb.AppendLine($"samples/cycle: {_samplesInCycle}");
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

        _isActive = _emitter.start();
        return _isActive;
    }

    public override bool Stop()
    {
        _isActive = false;
        return _emitter.stop();
    }

    public override void SetLocations(Point[] locations)
    {
        if (_isActive)
        {
            _emitter.stop();
            _emitter.reset();
        }

        lock (_positions)
        {
            base.SetLocations(locations);
            ConstructDataPoints();
        }

        if (_isActive)
        {
            _isActive = _emitter.start();
        }
    }

    public override void ClearLocations()
    {
        if (_isActive)
        {
            _emitter.stop();
            _emitter.reset();
        }

        lock (_positions)
        {
            base.ClearLocations();
            ConstructDataPoints();
        }

        if (_isActive)
        {
            _isActive = _emitter.start();
        }
    }

    public void SaveDebugInfo(string filename)
    {
        using var writer = new StreamWriter(filename);

        foreach (var row in _intensities)
        {
            var sb = new StringBuilder();
            foreach (var col in row)
            {
                sb.Append(col);
            }
            writer.WriteLine(sb.ToString());
        }
        writer.Flush();
    }

    #region Internal

    readonly TimePointStreamingEmitter _emitter;

    bool _isActive = false;

    int _samplesInCycle = 0;

    Vector3[][] _positions = [[]];
    float[][] _intensities = [[]];

    void ConstructDataPoints()
    {
        uint pointCount = (uint)_locations.Length; // (uint)_usp.timeSlices[0].focalPoints.Length;
        if (pointCount == 0)
        {
            return;
        }

        uint sampleRate = _emitter.setMaximumControlPointCount(pointCount);

        float samplesPerSlice = (float)sampleRate / _usp.StepsPerSecond * _usp.StepsPerTimeslice;
        _samplesInCycle = (int)(_usp.TimeSlices.Length * samplesPerSlice);

        _positions = new Vector3[_samplesInCycle][];
        _intensities = new float[_samplesInCycle][];

        for (uint si = 0; si < _samplesInCycle; si++)
        {
            _positions[si] = new Vector3[pointCount];
            _intensities[si] = new float[pointCount];

            int sliceIndex = (int)(si / samplesPerSlice);
            var point = _usp.TimeSlices[sliceIndex].FocalPoints[0];

            for (int pi = 0; pi < pointCount; pi++)
            {
                var offset = _locations[pi];
                var position = new Vector3(point.Position.X + offset.X, point.Position.Y + offset.Y, point.Position.Z);
                _positions[si][pi] = Transform(_usp.DefaultTransform, position);
                _intensities[si][pi] = point.Intensity;
            }
        }
    }

    void OnEmission(TimePointStreamingEmitter emitter, OutputInterval interval, TimePoint deadline, object user_obj)
    {
        if (_locations.Length == 0)
        {
            return;
        }

        _dampedOffsetX = _dampedOffsetX * 0.95f + _offsetX * 0.05f;
        _dampedOffsetY = _dampedOffsetY * 0.6f + _offsetY * 0.4f;
        _dampedOffsetZ = _dampedOffsetZ * 0.6f + _offsetZ * 0.4f;

        if (!_isPaused)
        {
            lock (_positions)
            {
                foreach (var tpoint in interval)
                {
                    for (int pi = 0; pi < tpoint.count(); ++pi)
                    {
                        var point = tpoint.persistentControlPoint(pi);
                        Vector3 pos = _positions[_sliceIndex][pi];

                        point.setPosition(new Vector3(pos.x + _dampedOffsetX, pos.y + _dampedOffsetY, pos.z + _dampedOffsetZ));
                        point.setIntensity(_intensities[_sliceIndex][pi]);
                        point.enable();
                    }

                    _sliceIndex = (_sliceIndex + 1) % _samplesInCycle;

                    if (_sliceIndex == 0)
                    {
                        _playedTimes++;
                        if (_usp.PlaybackType == UspFile.PlayTimes.AsSpecified && _playedTimes == _usp.LoopTimes)
                        {
                            Stop();
                            OnStopped(EventArgs.Empty);
                            break;
                        }
                    }
                }
            }
        }
    }

    #endregion
}
