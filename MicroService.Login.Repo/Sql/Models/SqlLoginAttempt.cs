using System;
using Dapper.Contrib.Extensions;
using MicroService.Common.Core.Databases.Repository.MsSql.Impl;

namespace MicroService.Login.Repo.Sql.Models
{
    [Table("LoginAttempt")]
    public class SqlLoginAttempt : SqlEntityBase
    {
        public bool           Success { get; set; }
        public string         FromIp  { get; set; }
        public DateTimeOffset Date    { get; set; }
        public string         Reason  { get; set; }
        public int            UserId  { get; set; }
        public string         Browser { get; set; }
        public string         Device  { get; set; }
        public string         Os      { get; set; }
    }
}