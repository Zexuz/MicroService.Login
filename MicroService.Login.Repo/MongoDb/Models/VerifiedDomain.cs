using MicroService.Common.Core.Databases.Repository;

namespace MicroService.Login.Repo.MongoDb.Models
{
    public class Tier : IEntity<int>
    {
        public int Id { get; set; }

        public decimal WithdrawalFee { get; set; }
        public decimal DepositFee    { get; set; }
        public decimal TransferFee   { get; set; }
        public string  Name          { get; set; }
    }
}