using MicroService.Common.Core.Databases.Repository.MsSql.Interfaces;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("MicroService.Login.Repo.Test")]

namespace MicroService.Login.Repo.Sql.Tables
{
    internal class UserTable:ITable
    {
        public string TableName => "User";

        public string TableContent => @"
                            (
                              Id              INT IDENTITY   PRIMARY KEY,
                              Username        VARCHAR(18)    NOT NULL,
                              Password        VARCHAR(100)   NOT NULL,
                              Email           VARCHAR(100)   NOT NULL,
                              Created         DATETIMEOFFSET NOT NULL,
                              Role            INT            NOT NULL,
                              TwoFactorSecret VARCHAR(100)       NULL,
                              EmailVerified   BIT            NOT NULL,
                              IsSuspended     BIT            NOT NULL,
                            )

                            CREATE UNIQUE INDEX User_Email_uindex                        ON {0}.dbo.[User] (Email)
                            CREATE UNIQUE INDEX Users_Username_uindex                    ON {0}.dbo.[User] (Username)
                            CREATE UNIQUE NONCLUSTERED INDEX User_TwoFactorSecret_uindex ON {0}.dbo.[User] (TwoFactorSecret) WHERE TwoFactorSecret IS NOT NULL
";

    }
}