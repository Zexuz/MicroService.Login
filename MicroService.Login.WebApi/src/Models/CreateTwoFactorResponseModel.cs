namespace MicroService.Login.WebApi.Models
{
    public class CreateTwoFactorResponseModel
    {
        public string Base64QrCode  { get; set; }
        public string Secret        { get; set; }
        public string SecurityToken { get; set; }
    }
}