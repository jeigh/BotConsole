using ANT_Managed_Library;


namespace AntPlayground
{
    public class AntHelper
    {
        private ANT_Device _device = null;
        private ANT_Channel _channel = null;
        private byte[] NETKEY = new byte[] { 0xB9, 0xA5, 0x21, 0xFB, 0xBD, 0x72, 0xC3, 0x45 };
        private PowerData _powerData = new PowerData();

        public void InitializePowerMeter()
        {
            _device = new ANT_Device();
            _device.setNetworkKey(0, NETKEY);

            _channel = _device.getChannel(0);
            _channel.assignChannel(ANT_ReferenceLibrary.ChannelType.BASE_Master_Transmit_0x10, 0);

            _channel.setChannelID(35, false, 0x0B, 0x01);
            _channel.setChannelPeriod(8192);
            _channel.setChannelFreq(57);

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