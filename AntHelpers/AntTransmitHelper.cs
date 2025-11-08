using ANT_Managed_Library;

namespace AntHelpers
{
    public class AntTransmitHelper
    {
        private ANT_Device _device = null;
        private ANT_Channel _channel = null;
        private PowerData _powerData = new PowerData();

        private ushort _deviceNumber;
        private ushort _periodFrequency;
        private byte _channelFrequency;

        public void InitializePowerMeter(ushort deviceNumber, ushort periodFrequency, byte channelFrequency)
        {
            _deviceNumber = deviceNumber;
            _periodFrequency = periodFrequency;
            _channelFrequency = channelFrequency;

            _device = new ANT_Device();
            _device.setNetworkKey(0, Constants.NETKEY);

            _channel = _device.getChannel(0);
            _channel.assignChannel(ANT_ReferenceLibrary.ChannelType.BASE_Master_Transmit_0x10, 0);

            _channel.setChannelID(_deviceNumber, false, Constants.DeviceTypeBicyclePower, 0x01);
            _channel.setChannelPeriod(_periodFrequency);
            _channel.setChannelFreq(_channelFrequency);

            _channel.openChannel();
        }

        public bool SendPayload(byte[] payload) =>
            _channel.sendBroadcastData(payload);

        public void CloseAndDispose()
        {
            _channel.closeChannel();
            _device.Dispose();
        }

        public byte[] BuildPayload(int power, ushort cadence)
        {
            _powerData.EventCount = (byte)((_powerData.EventCount + 1) & 0xff);
            _powerData.CumulativePower = (ushort)((_powerData.CumulativePower + power) & 0xffff);
            _powerData.InstantaneousPower = (ushort)power;
            _powerData.Cadence = cadence;

            var payload = new byte[8];

            payload[0] = 0x10;  // standard power-only message
            payload[1] = _powerData.EventCount;
            payload[2] = 0xFF;  // Pedal power not used
            payload[3] = (byte)(_powerData.Cadence ?? 0xFF);
            payload[4] = (byte)(_powerData.CumulativePower & 0xff);
            payload[5] = (byte)(_powerData.CumulativePower >> 8);
            payload[6] = (byte)(_powerData.InstantaneousPower & 0xff);
            payload[7] = (byte)(_powerData.InstantaneousPower >> 8);

            return payload;
        }
    }
}
