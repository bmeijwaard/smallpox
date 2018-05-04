namespace Smallpox.Messages
{
    public interface IServiceResponse
    {
        bool Succeeded { get; }
        string ErrorMessage { get; }
    }
}
