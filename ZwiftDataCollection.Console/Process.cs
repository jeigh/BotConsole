using ZwiftClassLibrary;
using ZwiftDataCollectionAgent.Console.DataAccess;

namespace ZwiftDataCollectionAgent.Console
{

    public class Process
    {
        private SQLiteGateway _db = new();
        private ZwiftAPI _zwift = new();

        private int _maxAdditionalPower = 50;

        private float _draftDenominator = 100f;
        private float _elevationChangeDenominator = 50f;

        private readonly IBespokeConfig _config;

        public Process(IBespokeConfig config)
        {
            _config = config;
        }

        public async Task Run()
        {
            await _zwift.Authenticate(_config.zwiftUsername, _config.zwiftPassword);
            await _zwift.LoopPlayerStateRetrieval(_config.zwiftId, UpdateRiderValues);

            System.Console.WriteLine("end.");
        }

        private void UpdateRiderValues(int id, int draft, float currentZ, float previousZ)
        {
            int additionalWatts = CalculateAdditionalWatts(draft, currentZ, previousZ, _maxAdditionalPower, _draftDenominator, _elevationChangeDenominator);

            //todo: retrieve these from config eventually
            var defaultCriticalPower = 185f;
            var baseWatts = 165;

            var rider = new Rider()
            {
                RiderId = id,
                CurrentWatts = baseWatts + additionalWatts,
                CurrentCadence = 90,
                MaxIdealOneMinuteWatts = (int) (defaultCriticalPower * 1.3f),
                MaxIdealFiveMinuteWatts = (int) (defaultCriticalPower * 1.2f),
                MaxIdealTenMinuteWatts = (int) (defaultCriticalPower * 1.14f),
                MaxIdealTwentyMinuteWatts = (int) (defaultCriticalPower * 1.05f),
                MaxIdealOneHourWatts = (int) defaultCriticalPower,
                MaxWattsAboveThreshold = 50
            };

            var stringifiedRider = $"Id: {rider.RiderId}, CW: {rider.CurrentWatts}, D: {draft}, (C-P)Z:{currentZ-previousZ} CC: {rider.CurrentCadence}, M1: {rider.MaxIdealOneMinuteWatts}, M5: {rider.MaxIdealFiveMinuteWatts}, M10: {rider.MaxIdealTenMinuteWatts}, M20: {rider.MaxIdealTwentyMinuteWatts}, M60: {rider.MaxIdealOneHourWatts}, MT: {rider.MaxWattsAboveThreshold}";
            System.Console.WriteLine(stringifiedRider);

            _db.UpsertRiderValues(rider);
        }

        public int CalculateAdditionalWatts(int draft, float currentZ, float previousZ, int maxAdditionalPower, float draftDenominator, float elevationChangeDenominator)
        {
            var elevationChange = currentZ - previousZ;

            var draftAddend = GetAddend(draft, maxAdditionalPower, draftDenominator);
            var gradeAddend = GetAddend(elevationChange, maxAdditionalPower, elevationChangeDenominator);

            return new List<int> { draftAddend + gradeAddend, maxAdditionalPower }.Min();
        }

        public int GetAddend(float currentValue, float maxAdditionalPower, float gradeDenominator)
        {
            float weight = maxAdditionalPower / gradeDenominator;
            
            if (currentValue > gradeDenominator) currentValue = gradeDenominator;
            if (currentValue > 0) return (int)(currentValue * weight);

            else return 0;
        }



        private int Min(int a, int b) => a < b ? a : b;


    }




}
