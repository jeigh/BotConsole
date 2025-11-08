using DataAccess;
using AntHelpers;
using System;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace AntPowerMeterReceiver.Console
{
    internal partial class Program
    {
        public class FeatureAbstraction
        {
            private const ushort deviceNumber = 24165; //25558;
            private const ushort freq = (8182*4);
            private const byte channelFreq = 57;

            private SQLiteGateway _dal;
            private AntReceiveHelper _receiver;
            
            private const float _ridersHourPower = 180f;
            private const float _targetTwentyMinutePower = 200f; //220f;
            private float _multiplier = _targetTwentyMinutePower / _ridersHourPower;

            private void PersistData(int power, int cadence) 
            {
                var config = new BespokeConfig();

                var rider = new Rider()
                {
                    RiderId = config.PimaryZwiftId,
                    CurrentWatts = (int) (_multiplier * power),
                    CurrentCadence = 90,
                    MaxIdealOneMinuteWatts = (int) (_ridersHourPower * 1.3f),
                    MaxIdealFiveMinuteWatts = (int) (_ridersHourPower * 1.2f),
                    MaxIdealTenMinuteWatts = (int) (_ridersHourPower * 1.14f),
                    MaxIdealTwentyMinuteWatts = (int) (_ridersHourPower * 1.05f),
                    MaxIdealOneHourWatts = (int) _ridersHourPower,
                    MaxWattsAboveThreshold = 00
                };

                _dal.UpsertRiderValues(rider);

                System.Console.WriteLine($"Power: {power}W, Cadence: {cadence}rpm");
            }


            public FeatureAbstraction()
            {
                _dal = new SQLiteGateway();
                _receiver = new AntReceiveHelper(PersistData);
            }

            public async Task RunAsync()
            {
                _receiver.ConfigureChannel(deviceNumber, freq, channelFreq);

                await Task.Delay(-1);
            }
        }

    }
}
