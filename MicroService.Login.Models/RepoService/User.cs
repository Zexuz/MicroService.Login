using System;
using MicroService.Common.Core.ValueTypes.Types;

namespace MicroService.Login.Models.RepoService
{
    public class User
    {
        public int            Id              { get; set; }
        public string         Username        { get; set; }
        public string         Password        { get; set; }
        public Email          Email           { get; set; }
        public DateTimeOffset Created         { get; set; }
        public string         TwoFactorSecret { get; set; }
        public Role           Role            { get; set; }
        public bool           EmailVerified   { get; set; }
        public int            Balance         { get; set; }
        public bool           IsSuspended     { get; set; }
        public int?           DomainId        { get; set; }
        public int            TierId          { get; set; }
    }
}