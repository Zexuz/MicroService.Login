using System;

namespace MicroService.Login.Models.RepoService
{
    public class RefreshToken
    {
        public int             Id          { get; set; }
        public string          Value       { get; set; }
        public string          FromIp      { get; set; }
        public string          CountryCode { get; set; }
        public int             UserId      { get; set; }
        public DateTimeOffset  Created     { get; set; }
        public string          Browser     { get; set; }
        public string          Device      { get; set; }
        public string          Os          { get; set; }
        public bool            Valid       { get; set; }
        public DateTimeOffset? Revoked     { get; set; }
        public DateTimeOffset  LastUsed    { get; set; }
    }
}