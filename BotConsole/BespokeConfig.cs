using System;
using System.Configuration;

namespace BotConsole
{
    public class BespokeConfig
    {
        public ushort baseCadence;
        public int maxIdealTwentyMinuteAvgWatts;
        public int basePowerValue;

        public BespokeConfig()
        {
            string baseCadenceStr = ConfigurationManager.AppSettings["baseCadence"];
            if (!ushort.TryParse(baseCadenceStr, out baseCadence))
                throw new ArgumentException("baseCadence");

            string maxIdealTwentyMinuteAvgWattsStr = ConfigurationManager.AppSettings["maxIdealTwentyMinuteAvgWatts"];
            if (!int.TryParse(maxIdealTwentyMinuteAvgWattsStr, out maxIdealTwentyMinuteAvgWatts))
                throw new ArgumentException("maxIdealTwentyMinuteAvgWatts");

            string basePowerValueStr = ConfigurationManager.AppSettings["basePowerValue"];
            if (!int.TryParse(basePowerValueStr, out basePowerValue))
                throw new ArgumentException("basePowerValue");
        }
    }
}
