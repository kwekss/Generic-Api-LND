using System;
namespace AssociationBundles.Core.Models.USSD
{
    public class UssdRequestPayload
    {
        public string DialogId { get; set; }
        public string SessionId { get; set; }
        public string BeneficiaryMsisdn { get; set; }
        public string Msisdn { get; set; }
        public string PaymentMethod { get; set; }
        public string CId { get; set; }
        public string MsisdnCategory { get; set; }
        public long ProductId { get; set; }
        public string Category { get; set; }
        public string ProductType { get; set; }
        public double Amount { get; set; }
        public string Input { get; set; }
        public string Email { get; set; }
        public string Surname { get; set; }
        public string CardNumber { get; set; }
        public string Channel { get; set; }
        public DateTime RequestTimestamp { get; set; }
        public DateData Date { get; set; }
    }

    public class DateData
    {

    }
}
