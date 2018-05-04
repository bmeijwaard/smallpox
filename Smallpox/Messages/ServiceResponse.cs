namespace Smallpox.Messages
{
    public class ServiceResponse : IServiceResponse
    {
        public ServiceResponse()
        {
            Succeeded = true;
        }

        public ServiceResponse(string errorMessage)
        {
            ErrorMessage = errorMessage;
            Succeeded = string.IsNullOrWhiteSpace(errorMessage);
        }

        public string ErrorMessage { get; set; }

        public bool Succeeded { get; set; }
    }
}
