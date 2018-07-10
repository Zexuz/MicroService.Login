namespace MicroService.Login.Security.Models
{
    public class CreateUserReponse
    {
        public bool   Success { get; set; }
        public string Error   { get; set; }

        public CreateUserReponse(bool success, string error = "Something went wrong, please try again later")
        {
            Success = success;
            Error = error;
        }
    }
}