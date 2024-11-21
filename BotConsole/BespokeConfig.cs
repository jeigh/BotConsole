using System;
using System.Configuration;

namespace BotConsole
{
    public class BespokeConfig
    {

        public int PimaryZwiftId;

        public BespokeConfig()
        {
            string primaryZwiftIdStrStr = ConfigurationManager.AppSettings["primaryZwiftId"];
            if (!int.TryParse(primaryZwiftIdStrStr, out PimaryZwiftId))
                throw new ArgumentException("primaryZwiftId");
        }
    }
}
