using System;

namespace MicroService.Login.Models.ViewModel
{
    public class UserViewModel
    {
        public int            Id          { get; set; }
        public string         Username    { get; set; }
        public DateTimeOffset MemberSince { get; set; }
    }
}