using System;
using Dapper.Contrib.Extensions;
using MicroService.Common.Core.Databases.Repository.MsSql.Impl;

namespace MicroService.Login.Repo.Sql.Models
{
    [Table("VerifiedDomainUser")]
    public class SqlVerifiedDomainUser : SqlEntityBase
    {
        public string         WebsiteUrl   { get; set; }
        public DateTimeOffset Created      { get; set; }
        public string         Description  { get; set; }
        public int            OwnerId      { get; set; }
        public bool           AllowDeposit { get; set; }
        public DateTimeOffset LastUpdated  { get; set; }
    }
}