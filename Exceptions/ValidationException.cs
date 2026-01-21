using EmployeeAdminPortal.Models;

namespace EmployeeAdminPortal.Exceptions
{

    public class ValidationException : Exception
    {
        public List<ErrorDetail> Errors { get; set; }
        public ValidationException(List<ErrorDetail> errors)
            : base("Validation failed.")
        {
            Errors = errors;
        }
        public ValidationException(string fieldName, string errorCode, string errorMessage, string value)
            : base(errorMessage)
        {
            Errors = new List<ErrorDetail>
            {
                new ErrorDetail
                {
                    Element = fieldName,
                    Code = errorCode,
                    Message = errorMessage,
                    Value = value,
                    Location = "body"
                }
            };
        }
    }
}
