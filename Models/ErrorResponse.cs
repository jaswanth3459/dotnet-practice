namespace EmployeeAdminPortal.Models
{
    public class ErrorResponse
    {
        public string Message { get; set; }
        public List<ErrorDetail> Errors { get; set; }
    }

    public class ErrorDetail
    {
        public string Element { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public string Value { get; set; }
        public string Location { get; set; }
    }
}
