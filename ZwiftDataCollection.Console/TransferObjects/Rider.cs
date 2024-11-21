namespace ZwiftClassLibrary
{
    public class Rider
    {
        public int RiderId { get; set; }
        public int CurrentWatts { get; set; }
        public int CurrentCadence { get; set; }
        public int MaxIdealOneMinuteWatts { get; set; }
        public int MaxIdealFiveMinuteWatts { get; set; }
        public int MaxIdealTenMinuteWatts { get; set; }
        public int MaxIdealTwentyMinuteWatts { get; set; }
        public int MaxIdealOneHourWatts { get; set; }
        public int MaxWattsAboveThreshold { get; set; }
    }
}