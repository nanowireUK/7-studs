using System;

namespace SevenStuds.Models
{
    public class BankruptcyEvent
    {
        public DateTimeOffset OccurredAt_UTC { get; set; }
        public bool IsBecauseOfLeaving { get; set; }
        public string BankruptPlayerName { get; set; }
        public BankruptcyEvent(string argBankruptPlayerName, bool argIsBecauseOfLeaving)
        {
            this.OccurredAt_UTC = DateTimeOffset.Now;
            this.BankruptPlayerName = argBankruptPlayerName;
            this.IsBecauseOfLeaving = argIsBecauseOfLeaving;
        }
    }
}