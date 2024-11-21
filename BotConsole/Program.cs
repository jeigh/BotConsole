using AntPlayground;
using System;
using System.Threading.Tasks;
using DataAccess;
using System.Runtime.CompilerServices;



namespace BotConsole
{
    public class Program
    {
        public static bool _ttMode = false;

        static async Task Main(string[] args)
        {
            var config = new BespokeConfig();

            Func<int, Rider> riderRetrievalMethod = defaultRiderValues;

            if (!_ttMode)
            {
                var dataAccess = new SQLiteGateway();
                riderRetrievalMethod = dataAccess.GetRiderValues;
            }
            
            Bot bot = new Bot(config.PimaryZwiftId, riderRetrievalMethod);
            bot.Run();

            Console.WriteLine("Ending...");
        }

        public static Rider defaultRiderValues(int RiderId)
        {

            var defaultCriticalPower = 180f;

            return new Rider
            {
                RiderId = RiderId,
                CurrentWatts = 179,
                CurrentCadence = 90,
                MaxIdealOneMinuteWatts = (int)(defaultCriticalPower * 1.3f),
                MaxIdealFiveMinuteWatts = (int)(defaultCriticalPower * 1.2f),
                MaxIdealTenMinuteWatts = (int)(defaultCriticalPower * 1.14f),
                MaxIdealTwentyMinuteWatts = (int)(defaultCriticalPower * 1.05f),
                MaxIdealOneHourWatts = (int)defaultCriticalPower,
                MaxWattsAboveThreshold = 50
            };
        }
    }
}
