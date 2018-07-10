using MicroService.Common.Core.Databases.Repository.MsSql.Interfaces;

namespace MicroService.Login.Repo.Sql.Tables
{
    internal class TransactionTable : ITable
    {
        public string TableName => "Transaction";

        public string TableContent => @"
                            (
                                Id        INT IDENTITY PRIMARY KEY NOT NULL,
                                Fee       INT                      NOT NULL,
                                NrOfCoins INT                      NOT NULL,
                                Created   DATETIMEOFFSET           NOT NULL,
                                FromUser  INT                      NOT NULL CONSTRAINT Transaction_User_Id_fk_2 REFERENCES {0}.dbo.[User],
                                ToUser    INT                      NOT NULL CONSTRAINT Transaction_User_Id_fk REFERENCES {0}.dbo.[User]
                            )          

                            CREATE INDEX Transaction_FromUser_index ON {0}.dbo.[Transaction] (FromUser)
                            CREATE INDEX Transaction_ToUser_index ON {0}.dbo.[Transaction] (ToUser)
 ";
    }
}