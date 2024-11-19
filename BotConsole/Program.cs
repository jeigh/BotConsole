using AntPlayground;
using System;
using System.Threading.Tasks;

namespace BotConsole
{
    public class ThreadSafeProperties
    {

        private static int _draft = 0;
        private static float _grade = 0f;
        private static object _draftLock = new object();
        private static object _gradeLock = new object();

        public int GetDraft()
        {
            lock (_draftLock)
            {
                return _draft;
            }

        }

        public void SetDraft(int value)
        {
            lock (_draftLock)
            {
                // average current value in with previous value
                _draft = (_draft + value) / 2;
            }
        }

        public float GetGrade()
        {
            lock (_gradeLock)
            {
                return _grade;
            }

        }

        public void SetGrade(float value)
        {
            lock (_gradeLock)
            {
                // average current value in with previous value
                _grade = (_grade + value) / 2;
            }
        }
    }


    public class Program
    {
        public static bool _ttMode = false;

        static async Task Main(string[] args)
        {
            var config = new BespokeConfig();
            var props = new ThreadSafeProperties();
            var anerobicStrategy = new IdealAnerobicStrategy(props.GetDraft, props.GetGrade, config.maxWattsAboveThreshold);
            
            if (!_ttMode)
            {
                var zwiftAPI = new ZwiftAPI();
                await zwiftAPI.Authenticate(config.ZwiftUsername, config.ZwiftPassword);

                Task.Run(() => zwiftAPI.LoopPlayerStateRetrieval(config.PimaryZwiftId,  props.SetDraft));
            }

            Bot bot = new Bot(config.baseCadence, config.maxIdealTwentyMinuteAvgWatts, config.basePowerValue, anerobicStrategy);
            bot.Run();

            Console.WriteLine("ewww");
        }
    }
}
