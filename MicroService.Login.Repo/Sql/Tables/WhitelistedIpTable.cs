using MicroService.Common.Core.Databases.Repository.MsSql.Interfaces;

namespace MicroService.Login.Repo.Sql.Tables
{
    internal class WhitelistedIpTable : ITable
    {
        public string TableName => "WhitelistedIp";

        public string TableContent => @"
                            (
                                Id         INT PRIMARY KEY IDENTITY NOT NULL,
                                Added      DATETIMEOFFSET           NOT NULL,
                                IpAddress  VARCHAR(15)              NOT NULL,
                                UserId     INT                      NOT NULL CONSTRAINT WhitelistedIp_User_Id_fk REFERENCES {0}.dbo.[User]
                            )

                            CREATE UNIQUE INDEX WhitelistedIp_Id_uindex               ON {0}.dbo.WhitelistedIp (Id)
                            CREATE UNIQUE INDEX WhitelistedIp_IpAddress_UserId_uindex ON {0}.dbo.WhitelistedIp (IpAddress, UserId)
";
    }
}