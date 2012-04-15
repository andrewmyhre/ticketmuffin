namespace GroupGiving.PayPal.Model
{
    public class ResponseError
    {
        public string ErrorId { get; set; }

        public string Domain { get; set; }

        public string SubDomain { get; set; }

        public string Severity { get; set; }

        public string Category { get; set; }

        public string Message { get; set; }

        public string Parameter { get; set; }
    }
}