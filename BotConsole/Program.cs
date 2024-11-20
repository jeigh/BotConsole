using AntPlayground;
using System;
using System.Threading.Tasks;
using DataAccess;



namespace BotConsole
{
    public class Program
    {
        public static bool _ttMode = false;

        static async Task Main(string[] args)
        {
            var config = new BespokeConfig();

            Func<int, int> momentaryWatts = defaultValueOfZero;

            if (!_ttMode)
            {
                var dataAccess = new SQLiteGateway();
                momentaryWatts = dataAccess.GetRiderValues;
            }
            
            Bot bot = new Bot(config.baseCadence, config.maxIdealTwentyMinuteAvgWatts, config.basePowerValue, momentaryWatts, config.PimaryZwiftId);
            bot.Run();

            Console.WriteLine("Ending...");
        }

        public static int defaultValueOfZero(int RiderId)
        {
            return 0;
        }
    }
}
