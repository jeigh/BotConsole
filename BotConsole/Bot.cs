using AntHelpers;
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
        
        
        private int _ticksPerSecond;
        
        private int _queueSize;
        private int _millisecondsPerTick;
        private FixedSizeIntQueue _wattsQueue;
        private Func<int, Rider> _retrieveRider;
        private int _riderId;
        private DateTime _backoffUntil = DateTime.MinValue;
        private float _backoffMultiplier = 2f / 3f;
        private float _freqPerSecond = 8193f * 4f;

        public Bot(int riderId, Func<int, Rider> retrieveRider)
        {
            //todo: make this configurable from within the database    
            _ticksPerSecond = 1;

            _queueSize = twentyMinutesInSeconds * _ticksPerSecond;
            _wattsQueue = new FixedSizeIntQueue(_queueSize);
            _millisecondsPerTick = millisecondsPerSecond / _ticksPerSecond;
            _retrieveRider = retrieveRider;
            
            
            _riderId = riderId;
        }

        private AntTransmitHelper _antHelper = new AntTransmitHelper();


        public void Run()
        {
            _antHelper.InitializePowerMeter(25, (ushort) (_freqPerSecond / _ticksPerSecond), 57);

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
                       
                        power = ConditionPowerToPreventCattingUp(power, twentyMinuteRemainingJoules, DateTime.Now);

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

        

        public int ConditionPowerToPreventCattingUp(int power, int remainingJoules, DateTime currentTime)
        {
            if (remainingJoules <= power)
            {
                // power is at immediate risk...  reduce intensity to zero and back off for 15 secs
                _backoffUntil = currentTime + TimeSpan.FromSeconds(15);
                power = 0;
            }

            else if (remainingJoules <= 100)
            {
                // power is at risk...  reduce intensity by half and back off for 10 secs
                _backoffUntil = currentTime + TimeSpan.FromSeconds(10);
                power = power / 2;
            }

            else if (remainingJoules <= 1000)
            {
                // power may be at risk soon...  reduce intensity by a third and back off for 5 secs
                _backoffUntil = currentTime + TimeSpan.FromSeconds(5);
                power = (int) (power * _backoffMultiplier);
            }

            else if (currentTime < _backoffUntil)
            {
                // power is not currently at risk, but recently exceeded threshold...  hold back just a little longer
                power = power * 2 / 3;
            }

            return power;
        }
    }
}