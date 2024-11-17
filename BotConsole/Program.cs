using AntPlayground;
using System;

namespace BotConsole
{

    public class Program
    {

        [STAThread]
        static void Main(string[] args)
        {
            var config = new BespokeConfig();

            Bot bot = new Bot(config.baseCadence, config.maxIdealTwentyMinuteAvgWatts, config.basePowerValue);

            bot.Run();
        }
    }
}
