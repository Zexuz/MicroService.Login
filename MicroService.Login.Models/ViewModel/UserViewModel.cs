using System;

namespace MicroService.Login.Models.ViewModel
{
    public class UserViewModel
    {
        public int                         Id                { get; set; }
        public string                      Username          { get; set; }
        public DateTimeOffset              MemberSince       { get; set; }
        public bool                        HasVerifiedDomain => VerifiedDomain != null;
        public VerifiedDomainUserViewModel VerifiedDomain    { get; set; }
    }

    public class VerifiedDomainUserViewModel
    {
        public Uri            Website      { get; set; }
        public DateTimeOffset Created      { get; set; }
        public string         Description  { get; set; }
        public DateTimeOffset LastUpdated  { get; set; }
        public bool           AllowDeposit { get; set; }
    }
}