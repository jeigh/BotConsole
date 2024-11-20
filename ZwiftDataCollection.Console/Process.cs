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

        private void UpdateRiderValues(int id, int draft, float grade)
        {
            
            int additionalWatts = CalculateAdditionalWatts(draft, grade, _maxAdditionalPower, _draftDenominator, _gradeDenominator);

            System.Console.WriteLine($"Draft: {draft}, Grade {grade}, _maxAdditionalPower: {_maxAdditionalPower}, _draftDenominator: {_draftDenominator}, _gradeDenominator: {_gradeDenominator}, additionalWatts: {additionalWatts}");
            _db.UpsertRiderValues(id, additionalWatts);
        }

        public int CalculateAdditionalWatts(int draft, float grade, int maxAdditionalPower, float draftDenominator, float gradeDenominator)
        {
            var draftAddend = GetAddend(draft, maxAdditionalPower, draftDenominator);
            var gradeAddend = GetAddend(grade, maxAdditionalPower, gradeDenominator);

            int additionalWatts = Min(draftAddend + gradeAddend, maxAdditionalPower);
            return additionalWatts;
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
