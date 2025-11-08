namespace AntHelpers
{
    public class PowerData
    {
        public byte EventCount { get; set; }
        public ushort EventTime { get; set; }
        public ushort? Cadence { get; set; }
        public ushort CumulativePower { get; set; }
        public ushort InstantaneousPower { get; set; }
    }
}
