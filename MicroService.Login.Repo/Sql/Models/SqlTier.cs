using Dapper.Contrib.Extensions;
using MicroService.Common.Core.Databases.Repository.MsSql.Impl;

namespace MicroService.Login.Repo.Sql.Models
{
    [Table("Tier")]
    public class SqlTier : SqlEntityBase
    {
        public decimal WithdrawalFee { get; set; }
        public decimal DepositFee    { get; set; }
        public decimal TransferFee   { get; set; }
        public string  Name          { get; set; }
    }
}