using System.Threading;
using System.Threading.Tasks;

namespace AntPowerMeterReceiver.Console
{
    internal partial class Program
    {
        static async Task Main(string[] args)
        {
            await Task.Delay(15000);

            FeatureAbstraction feature = new FeatureAbstraction();
            await feature.RunAsync();
        }
    }
}
