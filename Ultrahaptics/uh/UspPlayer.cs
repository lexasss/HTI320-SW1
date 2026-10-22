namespace UH;

public class UspPlayer(string deviceID) : IDisposable
{
    public string DeviceID { get; } = deviceID;
    public bool HasEmitter => _emitter != null;
    public bool IsPlaying { get; private set; } = false;

    public event EventHandler? Finished;

    public bool Play()
    {
        IsPlaying = _emitter?.Start() == true;
        return IsPlaying;
    }

    public bool Play(Stimulation stimulation, float offsetX = 0, float offsetY = 0, float offsetZ = 0)
    {
        if (SetStimulation(stimulation))
        {
            SetFocusPlane(offsetZ);
            SetHorizontalOffset(offsetX, offsetY);
            return Play();
        }

        return IsPlaying;
    }

    public bool Stop()
    {
        if (IsPlaying)
        {
            IsPlaying = false;
            _stimulation = null;

            return _emitter?.Stop() == true;
        }
        return false;
    }

    public void SetFocusPlane(float aVerticalOffset)
    {
        _emitter?.SetVerticalOffset(aVerticalOffset - _emitter.Usp.VerticalOffset);
    }

    public void SetHorizontalOffset(float aOffsetX, float aOffsetY)
    {
        _emitter?.SetHorizontalOffset(aOffsetX, aOffsetY);
    }

    public void Dispose()
    {
        _stimulation = null;

        _emitter?.Dispose();
        _emitter = null;

        GC.SuppressFinalize(this);
    }

    #region Internal

    Emitter? _emitter = null;
    Stimulation? _stimulation = null;

    private bool SetStimulation(Stimulation stimulation)
    {
        if (_stimulation == stimulation)
        {
            return false;
        }

        if (_emitter != null)
        {
            _emitter.Stop();
            _emitter.Dispose();
            _emitter = null;
        }

        IsPlaying = false;

        _stimulation = stimulation;

        var usp = stimulation.Usp;

        Emitter emitter = usp.SupportedEmitters[0] == 0
            ? new AMEmitter(DeviceID, usp)
            : new TSPEmitter(DeviceID, usp);
        emitter.Stopped += (s, e) =>
        {
            IsPlaying = false;
            Finished?.Invoke(s, e);
        };
        emitter.SetLocations(stimulation.Locations);
        _emitter = emitter;

        return true;
    }

    #endregion
}
