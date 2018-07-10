using System;
using System.Net;

namespace MicroService.Login.Models.RepoService
{
    public class WhitelistedIp
    {
        public int            Id        { get; set; }
        public int            UserId    { get; set; }
        public DateTimeOffset Added     { get; set; }
        public IPAddress      IpAddress { get; set; }
    }
}