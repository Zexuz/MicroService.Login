using System;
using Dapper.Contrib.Extensions;
using MicroService.Common.Core.Databases.Repository.MsSql.Impl;

namespace MicroService.Login.Repo.Sql.Models
{
    [Table("Review")]
    public class SqlReview : SqlEntityBase
    {
        [ExplicitKey]
        public override int Id { get; set; }

        public int            UserId     { get; set; }
        public bool           IsPositive { get; set; }
        public string         Text       { get; set; }
        public DateTimeOffset Updated    { get; set; }
        public bool           Valid      { get; set; }
    }
}