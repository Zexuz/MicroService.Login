using MicroService.Common.Core.Databases.Repository.MsSql.Interfaces;

namespace MicroService.Login.Repo.Sql.Tables
{
    internal class ReviewTable : ITable
    {
        public string TableName => "Review";

        public string TableContent => @"
                            (
                                Id             INT PRIMARY KEY NOT NULL CONSTRAINT Review_Transakction_Id_fk REFERENCES {0}.dbo.[Transaction],
                                UserId         INT             NOT NULL CONSTRAINT Review_User_Id_fk  REFERENCES {0}.dbo.[User],
                                IsPositive     BIT             NOT NULL,
                                Text           VARCHAR(500)        NULL,
                                Updated        DATETIMEOFFSET  NOT NULL,
                                Valid          BIT             NOT NULL,
                            )
                            CREATE UNIQUE INDEX Review_TransactionId_uindex        ON {0}.dbo.Review (Id)
                            CREATE        INDEX Review_UserId_index                ON {0}.dbo.Review (UserId)
 ";
    }
}