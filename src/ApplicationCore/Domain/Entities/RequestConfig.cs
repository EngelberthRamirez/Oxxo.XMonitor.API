namespace ApplicationCore.Domain.Entities
{
    public class RequestConfig
    {
        public int Id { get; set; }
        public int RequestTypeId { get; set; }
        public string Name { get; set; }
        public string UrlFormat { get; set; }
        public string MethodName { get; set; }
        public string ReturnType { get; set; }
        public string SendType { get; set; }
        public string HttpMethod { get; set; }
        public bool IsPublic { get; set; }
        public Guid Guid { get; set; }
    }
}
