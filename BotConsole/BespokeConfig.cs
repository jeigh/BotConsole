using System;
using System.Configuration;

namespace BotConsole
{
    public class BespokeConfig
    {
        public ushort baseCadence;
        public int maxIdealTwentyMinuteAvgWatts;
        public int basePowerValue;
        public int maxWattsAboveThreshold;
        public int PimaryZwiftId;

        public string ZwiftPassword { get; set; }
        public string ZwiftUsername { get; set; }
        
        

        public BespokeConfig()
        {
            string baseCadenceStr = ConfigurationManager.AppSettings["baseCadence"];
            if (!ushort.TryParse(baseCadenceStr, out baseCadence))
                throw new ArgumentException("baseCadence");

            string maxIdealTwentyMinuteAvgWattsStr = ConfigurationManager.AppSettings["maxIdealTwentyMinuteAvgWatts"];
            if (!int.TryParse(maxIdealTwentyMinuteAvgWattsStr, out maxIdealTwentyMinuteAvgWatts))
                throw new ArgumentException("maxIdealTwentyMinuteAvgWatts");

            string maxWattsAboveThresholdStr = ConfigurationManager.AppSettings["maxWattsAboveThreshold"];
            if (!int.TryParse(maxWattsAboveThresholdStr, out maxWattsAboveThreshold))
                throw new ArgumentException("maxWattsAboveThreshold");

            string basePowerValueStr = ConfigurationManager.AppSettings["basePowerValue"];
            if (!int.TryParse(basePowerValueStr, out basePowerValue))
                throw new ArgumentException("basePowerValue");

            string primaryZwiftIdStrStr = ConfigurationManager.AppSettings["primaryZwiftId"];
            if (!int.TryParse(primaryZwiftIdStrStr, out PimaryZwiftId))
                throw new ArgumentException("primaryZwiftId");            

            ZwiftUsername = ConfigurationManager.AppSettings["zwiftUsername"];
            ZwiftPassword = ConfigurationManager.AppSettings["zwiftPassword"];

        }


    }
}
