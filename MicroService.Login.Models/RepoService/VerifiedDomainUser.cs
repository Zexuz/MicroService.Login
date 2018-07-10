using System;

namespace MicroService.Login.Models.RepoService
{
    public class VerifiedDomainUser
    {
        public int            Id           { get; set; }
        public string         WebsiteUrl   { get; set; }
        public DateTimeOffset Created      { get; set; }
        public string         Description  { get; set; }
        public int            OwnerId      { get; set; }
        public bool           AllowDeposit { get; set; }
        public DateTimeOffset LastUpdated  { get; set; }
    }
}