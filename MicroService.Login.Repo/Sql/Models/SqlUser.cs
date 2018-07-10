using System;
using Dapper.Contrib.Extensions;
using MicroService.Common.Core.Databases.Repository.MsSql.Impl;
using MicroService.Login.Models;

namespace MicroService.Login.Repo.Sql.Models
{
    [Table("[User]")]
    public class SqlUser : SqlEntityBase
    {
        public string         Username        { get; set; }
        public string         Password        { get; set; }
        public string         Email           { get; set; }
        public DateTimeOffset Created         { get; set; }
        public string         TwoFactorSecret { get; set; }
        public Role           Role            { get; set; }
        public bool           EmailVerified   { get; set; }
        public int            Balance         { get; set; }
        public bool           IsSuspended     { get; set; }
        public int?           DomainId        { get; set; }
        [Computed]
        public int            TierId          { get; set; }
    }
}