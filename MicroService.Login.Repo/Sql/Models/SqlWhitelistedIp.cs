using System;
using Dapper.Contrib.Extensions;
using MicroService.Common.Core.Databases.Repository.MsSql.Impl;

namespace MicroService.Login.Repo.Sql.Models
{
    [Table("WhiteListedIp")]
    public class SqlWhitelistedIp : SqlEntityBase
    {
        public int            UserId    { get; set; }
        public DateTimeOffset Added     { get; set; }
        public string         IpAddress { get; set; }
    }
}