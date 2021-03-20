namespace NetAsyncSpider.Core.Scheduler.Component
{
    public class BloomFilterOptions
    {
        public double FalsePositiveProbability { get; set; }
        public int ExpectedInsertions { get; set; }
    }
}