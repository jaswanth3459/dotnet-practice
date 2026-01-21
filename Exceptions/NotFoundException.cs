namespace EmployeeAdminPortal.Exceptions
{

    public class NotFoundException : Exception
    {
        public string ResourceName { get; set; }
        public string ResourceId { get; set; }

        public NotFoundException(string resourceName, string resourceId)
            : base($"{resourceName} with ID '{resourceId}' was not found.")
        {
            ResourceName = resourceName;
            ResourceId = resourceId;
        }
        public NotFoundException(string message) : base(message)
        {
            ResourceName = "";
            ResourceId = "";
        }
    }
}
