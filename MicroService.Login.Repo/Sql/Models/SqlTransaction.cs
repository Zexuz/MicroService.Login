using System;
using Dapper.Contrib.Extensions;
using MicroService.Common.Core.Databases.Repository.MsSql.Impl;

namespace MicroService.Login.Repo.Sql.Models
{
    [Table("[Transaction]")]
    public class SqlTransaction : SqlEntityBase
    {
        public int            Fee       { get; set; }
        public int            NrOfCoins { get; set; }
        public DateTimeOffset Created   { get; set; }
        public int            FromUser  { get; set; }
        public int            ToUser    { get; set; }
    }
}