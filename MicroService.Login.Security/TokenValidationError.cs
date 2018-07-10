namespace MicroService.Login.Security
{
    public enum TokenValidationError
    {
        InvalidOrExpiered,
        InvalidSecret,
        InvalidValidationType,
        InvalidIp,
    }
}