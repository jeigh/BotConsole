using ANT_Managed_Library;
using System;
using static ANT_Managed_Library.ANT_ReferenceLibrary;

namespace AntHelpers
{

    public class AntReceiveHelper : IDisposable
    {
        public ANT_Device _device = null;
        public ANT_Channel _channel = null;
        private Action<int, int> _persistData;
        
        public AntReceiveHelper(Action<int, int> persistData)
        {
            _persistData = persistData;
            
            _device = new ANT_Device();
            _device.setNetworkKey(0, Constants.NETKEY);
            _device.deviceResponse += new ANT_Device.dDeviceResponseHandler(DeviceResponse);
        }

        private void DeviceResponse(ANT_Response response) =>
            Console.WriteLine($"Device Response Received: {(ANTMessageID)response.messageContents[1]}");


        private ushort _deviceNumber;
        private ushort _freq;
        private byte _channelFreq;
        private int _dropoutValue = 180;

        public void ConfigureChannel(ushort deviceNumber, ushort freq, byte channelFreq)
        {
            _deviceNumber = deviceNumber;
            _freq = freq;
            _channelFreq = channelFreq;

            _channel = _device.getChannel(0);
            _channel.assignChannel(ChannelType.BASE_Slave_Receive_0x00, 0, 50);
            _channel.setChannelID(_deviceNumber, false, Constants.DeviceTypeBicyclePower, 0); 
            _channel.setChannelPeriod(_freq); 
            _channel.setChannelFreq(_channelFreq); 
            _channel.setChannelSearchTimeout(12); 

            // Register the channel response handler
            _channel.channelResponse += new dChannelResponseHandler(ChannelResponse);

            // Open the channel
            _channel.openChannel();
        }

        private void ChannelResponse(ANT_Response response)
        {
            if (response.responseID == (byte) ANT_ReferenceLibrary.ANTMessageID.BROADCAST_DATA_0x4E)
            {
                try
                {
                    byte[] data = response.getDataPayload();
                    ProcessPowerMeterData(data, _persistData);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                var messageId = response.getMessageID();
                if (messageId == ANTMessageID.EVENT_0x01)  // channel message
                {
                    var channelMessage = response.getChannelEventCode();                  
                    switch (channelMessage)
                    {
                        case (ANTEventID.EVENT_CHANNEL_CLOSED_0x07):
                            Console.Write("Reconnecting... ");
                            ConfigureChannel(_deviceNumber, _freq, _channelFreq);
                            break;
                        
                        case ANTEventID.EVENT_RX_FAIL_0x02:
                            //Console.WriteLine($"DROPOUT.  Sending {_dropoutValue} watts instead.");
                            //_persistData(_dropoutValue, 90);
                            break;


                        default:
                            Console.WriteLine($"Channel Message Received: {channelMessage}");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine($"Channel Message Received: {messageId}");
                }
                
            }
            

        }
        private void ProcessPowerMeterData(byte[] data, Action<int, int> persistData)
        {
            if (persistData == null) persistData = DefaultProcessPowerMeterData;

            int dataPageNumber = data[0];

            



            if (dataPageNumber == 0x10)
            {
                int eventCount = data[1];
                int pedalPower = data[2];
                int cadence = data[3];
                int cumulativePower = data[4] | (data[5] << 8);
                int instantaneousPower = (data[6] | (data[7] << 8));

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"DPN: {dataPageNumber}, EC: {eventCount}, PP: {pedalPower}, C: {cadence}, CP: {cumulativePower}, IP: {instantaneousPower}");
                Console.ResetColor();

                persistData(instantaneousPower, cadence);
            } else 
                Console.WriteLine($"Data Page Number: {dataPageNumber}");

        }

        private void DefaultProcessPowerMeterData(int power, int cadence) =>
            Console.WriteLine($"Power: {power} watts, Cadence: {cadence} rpm.");


        public void Dispose()
        {
            _channel?.Dispose();
            _device?.Dispose();
        }
    }
}
