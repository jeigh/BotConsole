using Microsoft.Extensions.Configuration;

namespace ZwiftDataCollectionAgent.Console
{
    public interface IBespokeConfig
    {
        int zwiftId { get; set; }
        string zwiftPassword { get; set; }
        string zwiftUsername { get; set; }
    }

    public class BespokeConfig : IBespokeConfig
    {
        public string zwiftPassword { get; set; } = "";
        public string zwiftUsername { get; set; } = "";
        public int zwiftId { get; set; }
    }




}
