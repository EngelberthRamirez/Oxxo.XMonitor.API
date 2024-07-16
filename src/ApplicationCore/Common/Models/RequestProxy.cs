namespace ApplicationCore.Common.Models
{
    public class RequestParameters : ICloneable
    {
        public Guid Id { get; set; }

        public string CashRegisterIP { get; set; }

        public string[] Parameters { get; set; }

        public string Data { get; set; }

        public string SrClient { get; set; }

        public string SrHost { get; set; }

        public bool SecureConnection { get; set; }

        public object Clone() => MemberwiseClone();
    }
}
