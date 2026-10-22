using System.Text;

namespace UH;

public class DeviceDescriptor(string id, Ultrahaptics.DeviceInfo deviceInfo)
{
    public string ID => id;
    public Ultrahaptics.DeviceInfo DeviceInfo => deviceInfo;

    public string DeficeInfoText
    {
        get
        {
            var trConfig = DeviceInfo.getTransducerConfiguration();

            var sb = new StringBuilder();

            sb.AppendLine($"- depth: {DeviceInfo.arrayDepth()}");
            sb.AppendLine($"- width: {DeviceInfo.arrayWidth()}");
            sb.AppendLine($"- firmware: {DeviceInfo.getFirmwareVersion()}");
            sb.AppendLine($"- model ID: {DeviceInfo.getModelIdentifier()}");
            sb.AppendLine($"- serial number: {DeviceInfo.getSerialNumber()}");
            sb.AppendLine($"- transducer config:");
            sb.AppendLine($"  - size: {trConfig.size()}");
            sb.AppendLine($"  - position: {trConfig.data().x_position}, {trConfig.data().y_position}, {trConfig.data().z_position}");
            sb.AppendLine($"  - up: {trConfig.data().x_upvector}, {trConfig.data().y_upvector}, {trConfig.data().z_upvector}");
            sb.AppendLine($"- transducer layout: {DeviceInfo.getTransducerLayoutName()}");
            sb.AppendLine($"- supports:");
            sb.AppendLine($"  - Amplitude Modulation: {DeviceInfo.hasSupportFor(Ultrahaptics.DeviceFeatures.AMPLITUDE_MODULATION)}");
            sb.AppendLine($"  - Timepoint Streaming: {DeviceInfo.hasSupportFor(Ultrahaptics.DeviceFeatures.TIMEPOINT_STREAMING)}");

            return sb.ToString();
        }
    }

    public override string ToString()
    {
        return DeviceInfo.getModelDescription();
    }
}

public class DeviceManager
{
    public static DeviceDescriptor[] Devices
    {
        get
        {
            if (_devices == null)
            {
                _devices = [];
                foreach (var deviceID in _uh.getDeviceIdentifiers())
                {
                    var di = _uh.getDeviceInfoByIdentifier(deviceID);
                    _devices.Add(new DeviceDescriptor(deviceID, di));
                }
            }

            return _devices.ToArray();
        }
    }

    public static DeviceDescriptor[] Update()
    {
        _devices = null;
        return Devices;
    }

    #region Internal

    static List<DeviceDescriptor>? _devices;
    static readonly Ultrahaptics.UltrahapticsLibrary _uh = new();

    #endregion
}
