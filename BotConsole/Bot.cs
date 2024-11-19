using System;
using System.Linq;
using System.Threading;

namespace AntPlayground
{
    public class Bot        
    {
        private const int twentyMinutesInSeconds = 1200;
        private const int millisecondsPerTick = 250;
        private const int ticksPerSecond = 4;

        private int _maxIdealTwentyMinuteAvgWatts;
        private int _basePowerValue;
        
        private const int queueSize = twentyMinutesInSeconds * ticksPerSecond;
        private ushort _baseCadence;

        private FixedSizeIntQueue _wattsQueue { get; set; } = new FixedSizeIntQueue(queueSize);

        public Bot(ushort baseCadence, int maxIdealTwentyMinuteAvgWatts, int basePowerValue, IAnerobicStrategy anerobicStrategy)
        {
            _baseCadence = baseCadence;
            _maxIdealTwentyMinuteAvgWatts = maxIdealTwentyMinuteAvgWatts;
            _basePowerValue = basePowerValue;
            _anerobicStrategy = anerobicStrategy;
        }

        private AntHelper _antHelper = new AntHelper();
        private readonly IAnerobicStrategy _anerobicStrategy;

        public void Run()
        {
            _antHelper.InitializePowerMeter();                     

            try
            {
                while (true)
                {
                    try
                    {
                        int power = ConditionBasePowerValue(_basePowerValue);

                        power = _anerobicStrategy.ApplyAdditionalAnerobicPower(power);

                        float currentTwentyMinuteJoules = DetermineCurrentTwentyMinuteJoules(power);
                        int twentyMinuteRemainingJoules = (int)Math.Floor(twentyMinutesInSeconds * _maxIdealTwentyMinuteAvgWatts - currentTwentyMinuteJoules);
                       
                        power = ConditionPowerToPreventCattingUp(power, twentyMinuteRemainingJoules);

                        _wattsQueue.Enqueue(power);
                        int secs = _wattsQueue.HistoryCount / ticksPerSecond;
                        float sumOfJoules = currentTwentyMinuteJoules + twentyMinuteRemainingJoules;
                        
                        Console.WriteLine($"@{secs} secs: Power: {power}W 20M Joules: {currentTwentyMinuteJoules}j + Remaining Joules: {twentyMinuteRemainingJoules} = {sumOfJoules}");

                        byte[] payload = _antHelper.BuildPayload(power, _baseCadence);

                        bool val = _antHelper.SendPayload(payload);

                        Thread.Sleep(millisecondsPerTick);
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
                _antHelper.CloseAndDispose();
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
    }
}