using System;

namespace MicroService.Login.Models.RepoService
{
    public class LoginAttempt
    {
        public int            Id      { get; set; }
        public bool           Success { get; set; }
        public string         FromIp  { get; set; }
        public DateTimeOffset Date    { get; set; }
        public string         Reason  { get; set; }
        public int            UserId  { get; set; }
        public string         Browser { get; set; }
        public string         Device  { get; set; }
        public string         Os      { get; set; }
    }
}