using MicroService.Common.Core.Databases.Repository.MsSql.Interfaces;

namespace MicroService.Login.Repo.Sql.Tables
{
    internal class VerifiedDomainUserTable : ITable
    {
        public string TableName => "VerifiedDomainUser";

        public string TableContent => @"
                            (
                                Id           INT IDENTITY PRIMARY KEY NOT NULL,
                                WebsiteUrl   VARCHAR(100)             NOT NULL,
                                Created      DATETIMEOFFSET           NOT NULL,
                                OwnerId      INT                      NOT NULL CONSTRAINT VerifiedDomainUser_User_Id_fk REFERENCES {0}.dbo.[User],
                                AllowDeposit BIT                      NOT NULL,
                                Description  VARCHAR(500)                 NULL,
                                LastUpdated  DATETIMEOFFSET           NOT NULL,
                                
                            )
                            CREATE UNIQUE INDEX VerifiedDomainUser_Id_uindex         ON {0}.dbo.VerifiedDomainUser (Id)
                            CREATE UNIQUE INDEX VerifiedDomainUser_WebsiteUrl_uindex ON {0}.dbo.VerifiedDomainUser (WebsiteUrl)
";
    }
}