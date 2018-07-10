using MicroService.Common.Core.Databases.Repository.MsSql.Interfaces;

namespace MicroService.Login.Repo.Sql.Tables
{
    internal class TierTable : ITable
    {
        public string TableName => "Tier";

        public string TableContent => @"
                            (
                                Id            INT PRIMARY KEY IDENTITY NOT NULL,
                                Name          VARCHAR(20)              NOT NULL,
                                WithdrawalFee DECIMAL(10,8)            NOT NULL,
                                DepositFee    DECIMAL(10,8)            NOT NULL,
                                TransferFee   DECIMAL(10,8)            NOT NULL
                            )
                            CREATE UNIQUE INDEX Tier_Id_uindex   ON {0}.dbo.Tier (Id)
                            CREATE UNIQUE INDEX Tier_Name_uindex ON {0}.dbo.Tier (Name)
 ";
    }
}