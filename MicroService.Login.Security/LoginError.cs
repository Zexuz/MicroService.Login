namespace MicroService.Login.Security
{
    public enum LoginError
    {
        None,
        AccountNotFound,
        InvalidLoginDetails,
        NotValidatedEmail,
        UnkownError,
        NotWhitelistedIp,
        TwoFactorRequiered,
        TwoFactorInvalid,
    }
}