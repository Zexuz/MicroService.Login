using System.Net;
using MicroService.Common.Core.Models;

namespace MicroService.Login.Security
{
    public class ConnectionInfo
    {
        public BrowserInfo BrowserInfo { get; set; }
        public IPAddress   IpAddress   { get; set; }
    }
}