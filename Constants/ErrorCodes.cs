namespace EmployeeAdminPortal.Constants
{
    public static class ErrorCodes
    {
        public static readonly Dictionary<string, string> ErrorMessages = new Dictionary<string, string>
        {
            // Phone errors
            { "E14001", "phone is required." },
            { "E14002", "phone is not valid." },
            { "E14003", "Phone is already exits." },

            // Name errors
            { "E14004", "name is required." },
            { "E14005", "name must be at least 3 characters long." },
            { "E14006", "name must not exceed 50 characters." },
            { "E14012", "Name is already exits." },

            // Email errors
            { "E14007", "email is required." },
            { "E14008", "email is not valid." },
            { "E14009", "Email is already exits." },

            // Salary errors
            { "E14010", "salary is required." },
            { "E14011", "salary must be greater than 0." }
        };

        public static string GetErrorMessage(string code)
        {
            return ErrorMessages.TryGetValue(code, out var message) ? message : "Unknown error.";
        }
    }
}
