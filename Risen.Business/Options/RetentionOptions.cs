using System;

namespace Risen.Business.Options
{
    public class RetentionOptions
    {
        public int TransactionRetentionDays { get; set; } = 365; // default keep 1 year
        public int IntervalMinutes { get; set; } = 60; // run hourly
        public int BatchSize { get; set; } = 1000;
    }
}
