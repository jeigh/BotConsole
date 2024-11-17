using System;
using System.Linq;
using System.Threading;
using ANT_Managed_Library;


namespace AntPlayground
{
    public class Bot
    {
        private const int twentyMinutesInSeconds = 1200;
        private const int ticksPerSecond = 4;
        
        private int _maxIdealTwentyMinuteAvgWatts;
        private int _basePowerValue;
        
        private const int queueSize = twentyMinutesInSeconds * ticksPerSecond;
        private ushort _baseCadence;

        private int last { get; set; } = 0;
        private int startingTime { get; set; }
        private FixedSizeIntQueue _wattsQueue { get; set; } = new FixedSizeIntQueue(queueSize);

        private byte[] NETKEY = new byte[] { 0xB9, 0xA5, 0x21, 0xFB, 0xBD, 0x72, 0xC3, 0x45 };
        private ANT_Device _device = null;
        private ANT_Channel _channel = null;

        public Bot(ushort baseCadence, int maxIdealTwentyMinuteAvgWatts, int basePowerValue)
        {
            _baseCadence = baseCadence;
            _maxIdealTwentyMinuteAvgWatts = maxIdealTwentyMinuteAvgWatts;
            _basePowerValue = basePowerValue;
        }

        public void Run()
        {
            InitializeAnt();
            
            PowerData _powerData = new PowerData();

            try
            {
                while (true)
                {
                    try
                    {
                        int power = ConditionBasePowerValue(_basePowerValue);

                        float currentTwentyMinuteJoules = DetermineCurrentTwentyMinuteJoules(power);
                        int twentyMinuteRemainingJoules = (int)Math.Floor(twentyMinutesInSeconds * _maxIdealTwentyMinuteAvgWatts - currentTwentyMinuteJoules);
                       
                        power = ConditionPowerToPreventCattingUp(power, twentyMinuteRemainingJoules);

                        _wattsQueue.Enqueue(power);
                        int secs = _wattsQueue.HistoryCount / 4;
                        float sumOfJoules = currentTwentyMinuteJoules + twentyMinuteRemainingJoules;
                        
                        Console.WriteLine($"@{secs} secs: Power: {power}W 20M Joules: {currentTwentyMinuteJoules}j + Remaining Joules: {twentyMinuteRemainingJoules} = {sumOfJoules}");

                        byte[] payload = CalculatePayload(power, _powerData);

                        bool val = _channel.sendBroadcastData(payload);

                        Thread.Sleep(250);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                        break;
                    }
                }

            }
            finally
            {
                _channel.closeChannel();
                _device.Dispose();
            }
        }

        private float DetermineCurrentTwentyMinuteJoules(int power)
        {
            var tempArray = _wattsQueue.ToArray();
            tempArray = tempArray.Skip(1).ToArray();
            tempArray.Append(power);

            float currentTwentyMinuteJoules = tempArray.Sum() / 4;
            return currentTwentyMinuteJoules;
        }

        private void InitializeAnt()
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

        private static int ConditionBasePowerValue(int basePowerValue)
        {
            int randomnumber = new Random().Next(-10, 10);
            int power = basePowerValue + randomnumber;
            if (power < 30) power = 0;
            return power;
        }

        private int ConditionPowerToPreventCattingUp(int power, int remainingJoules)
        {
            if (remainingJoules <= 2000) 
                power = power * 2 / 3;
            
            if (remainingJoules <= 100) 
                power = power / 2;

            if (remainingJoules <= power)
                power = 0;

            return power;
        }

        private byte[] CalculatePayload(int power, PowerData _powerData)
        {
            _powerData.EventCount = (byte)((_powerData.EventCount + 1) & 0xff);
            _powerData.CumulativePower = (ushort)((_powerData.CumulativePower + power) & 0xffff);
            _powerData.InstantaneousPower = (ushort)power;
            _powerData.Cadence = _baseCadence;

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