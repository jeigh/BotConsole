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
        private float _gradeDenominator = 5f;

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

        private void UpdateRiderValues(int id, int draft, ThreeDimensionalPoint current, ThreeDimensionalPoint previous, float speedInKm)
        {
            (int additionalWatts, float grade) = CalculateAdditionalWatts(draft, current, previous, _maxAdditionalPower, _draftDenominator, _gradeDenominator);

            //todo: retrieve these from config eventually
            var defaultCriticalPower = 180f;
            var baseWatts = 150;
            
            
            if (grade <= -4 && speedInKm >= 58)
            {
                baseWatts = 0;
                additionalWatts = 0;
            }

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
                MaxWattsAboveThreshold = 00
            };

            var stringifiedRider = $"Id: {rider.RiderId}, " +
                $"W: {baseWatts}+{additionalWatts}, " +
                $"S: {speedInKm}, " +
                $"D: {draft}, " +
                $"G: {(int) grade} " +
                $"CC: {rider.CurrentCadence}, " +
                $"M1: {rider.MaxIdealOneMinuteWatts}, " +
                $"M5: {rider.MaxIdealFiveMinuteWatts}, " +
                $"M10: {rider.MaxIdealTenMinuteWatts}, " +
                $"M20: {rider.MaxIdealTwentyMinuteWatts}, " +
                $"M60: {rider.MaxIdealOneHourWatts}, " +
                $"MT: {rider.MaxWattsAboveThreshold}";

            System.Console.WriteLine(stringifiedRider);

            _db.UpsertRiderValues(rider);
        }

        

        public (int, float) CalculateAdditionalWatts(int draft, ThreeDimensionalPoint current, ThreeDimensionalPoint previous, int maxAdditionalPower, float draftDenominator, float elevationChangeDenominator)
        {
            float elevationChange = current.Z - previous.Z;
            float horizontalChange = (float) Math.Sqrt(Math.Pow(current.X - previous.X, 2) + Math.Pow(current.Y - previous.Y, 2));
            
            float grade = 100 * elevationChange / horizontalChange;

            var draftAddend = GetDraftAddend(draft, maxAdditionalPower, draftDenominator);
            var gradeAddend = GetGradeAddend(grade, maxAdditionalPower, elevationChangeDenominator);
           

            return (new List<int> { draftAddend + gradeAddend, maxAdditionalPower }.Min(), grade);
        }

        public int GetGradeAddend(float currentValue, float maxAdditionalPower, float gradeDenominator)
        {
            float weight = maxAdditionalPower / gradeDenominator;
            
            if (currentValue > gradeDenominator) currentValue = gradeDenominator;
            if (currentValue > 0) return (int)(currentValue * weight);

            else return 0;
        }

        public int GetDraftAddend(float currentValue, float maxAdditionalPower, float gradeDenominator)
        {
            float weight = maxAdditionalPower / gradeDenominator;
            
            if (currentValue > gradeDenominator) currentValue = gradeDenominator;
            if (currentValue > 0) return (int)(currentValue * weight);

            else return 0;
        }



        private int Min(int a, int b) => a < b ? a : b;


    }




}
