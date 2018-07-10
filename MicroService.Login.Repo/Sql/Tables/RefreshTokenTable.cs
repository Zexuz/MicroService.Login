using MicroService.Common.Core.Databases.Repository.MsSql.Interfaces;

namespace MicroService.Login.Repo.Sql.Tables
{
    internal class RefreshTokenTable : ITable
    {
        public string TableName => "RefreshToken";

        public string TableContent => @"
                            (
                                Id          INT IDENTITY PRIMARY KEY NOT NULL,
                                Value       VARCHAR(100)             NOT NULL,
                                FromIp      VARCHAR(15)              NOT NULL,
                                CountryCode VARCHAR(6)               NOT NULL,                
                                Created     DATETIMEOFFSET           NOT NULL,
                                LastUsed    DATETIMEOFFSET           NOT NULL,
                                Browser     VARCHAR(50)              NOT NULL,
                                Device      VARCHAR(50)              NOT NULL,
                                Os          VARCHAR(50)              NOT NULL,
                                Valid       BIT                      NOT NULL,
                                Revoked     DATETIMEOFFSET               NULL,
                                UserId      INT                      NOT NULL CONSTRAINT RefreshToken_User_Id_fk REFERENCES {0}.dbo.[User]
                            )
                            CREATE UNIQUE INDEX RefreshToken_Id_uindex    ON {0}.dbo.RefreshToken (Id)
                            CREATE UNIQUE INDEX RefreshToken_Value_uindex ON {0}.dbo.RefreshToken (Value)
 ";
    }
}