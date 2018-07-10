using MicroService.Common.Core.Databases.Repository.MsSql.Interfaces;

namespace MicroService.Login.Repo.Sql.Tables
{
    internal class LoginAttemtTable : ITable
    {
        public string TableName => "LoginAttempt";

        public string TableContent => @"
                            (
                                Id      INT IDENTITY PRIMARY KEY,
                                Browser VARCHAR(50)    NOT NULL,
                                Device  VARCHAR(50)    NOT NULL,
                                Os      VARCHAR(50)    NOT NULL,
                                Success BIT            NOT NULL,
                                FromIp  VARCHAR(15)    NOT NULL,
                                Date    DATETIMEOFFSET NOT NULL,
                                Reason  VARCHAR(50)    NOT NULL,
                                UserId  INT            NOT NULL CONSTRAINT LoginAttempt_User_Id_fk REFERENCES {0}.dbo.[User]
                            )
                            
                            CREATE INDEX LoginAttempt_Id_index ON {0}.dbo.LoginAttempt (Id)
 ";
    }
}