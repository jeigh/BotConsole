using System;
using System.Configuration;

namespace BotConsole
{
    public class BespokeConfig
    {

        public int PrimaryZwiftId;

        public BespokeConfig()
        {
            string primaryZwiftIdStrStr = ConfigurationManager.AppSettings["primaryZwiftId"];
            if (!int.TryParse(primaryZwiftIdStrStr, out PrimaryZwiftId))
                throw new ArgumentException("primaryZwiftId");
        }
    }
}
