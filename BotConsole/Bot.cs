using DataAccess;
using System;
using System.Linq;
using System.Threading;

namespace AntPlayground
{
    public class Bot        
    {
        private const int millisecondsPerSecond = 1000;
        private const int twentyMinutesInSeconds = 1200;
        
        //todo: make this configurable from within the database
        private int _ticksPerSecond = 4;
        private int _queueSize;
        private int _millisecondsPerTick;
        private FixedSizeIntQueue _wattsQueue;
        private Func<int, Rider> _retrieveRider;
        private int _riderId;

        public Bot(int riderId, Func<int, Rider> retrieveRider)
        {
            _queueSize = twentyMinutesInSeconds * _ticksPerSecond;
            _wattsQueue = new FixedSizeIntQueue(_queueSize);
            _millisecondsPerTick = 250;
            _retrieveRider = retrieveRider;
            _ticksPerSecond = millisecondsPerSecond / _millisecondsPerTick;
            _riderId = riderId;
        }

        private AntHelper _antHelper = new AntHelper();


        public void Run()
        {
            _antHelper.InitializePowerMeter();                     

            try
            {
                while (true)
                {
                    try
                    {
                        var rider = _retrieveRider(_riderId);
                        int power = ConditionBasePowerValue(rider.CurrentWatts);

                        float currentTwentyMinuteJoules = DetermineCurrentTwentyMinuteJoules(power);
                        int twentyMinuteRemainingJoules = (int)Math.Floor(twentyMinutesInSeconds * rider.MaxIdealTwentyMinuteWatts - currentTwentyMinuteJoules);
                       
                        power = ConditionPowerToPreventCattingUp(power, twentyMinuteRemainingJoules);

                        _wattsQueue.Enqueue(power);
                        int secs = _wattsQueue.HistoryCount / _ticksPerSecond;
                        float sumOfJoules = currentTwentyMinuteJoules + twentyMinuteRemainingJoules;
                        
                        Console.WriteLine($"@{secs} secs: Power: {power}W 20M Joules: {currentTwentyMinuteJoules}j + Remaining Joules: {twentyMinuteRemainingJoules} = {sumOfJoules}");

                        byte[] payload = _antHelper.BuildPayload(power, (ushort) rider.CurrentCadence);
                        bool val = _antHelper.SendPayload(payload);
                        Thread.Sleep(_millisecondsPerTick);
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