using System;
using MicroService.Common.Core.Databases.Repository.MsSql.Impl;
using MicroService.Login.Repo.Sql.Tables;

namespace MicroService.Login.Repo.Test
{
    public abstract class TestFixtureBase : IDisposable
    {
        private readonly string _connectionStringMaster;
        protected abstract string Database { get; }

        protected string ConnectionString => $"Server=localhost;Database={Database};Trusted_Connection=True;";

        protected TestFixtureBase()
        {
            _connectionStringMaster = $"Server=localhost;Database=master;Trusted_Connection=True;";

            using (var tableHelper = new TableHelper(new SqlConnectionFactory(_connectionStringMaster)))
            {
                if (tableHelper.DoesDatabaseExists(Database).Result)
                    tableHelper.DropDatabase(Database).Wait();
            }

            using (var tableHelper = new TableHelper(new SqlConnectionFactory(_connectionStringMaster)))
            {
                tableHelper.ValidateTableExists(new UserTable(), Database).Wait();
            }
        }

        public void Dispose()
        {
            using (var tableHelper = new TableHelper(new SqlConnectionFactory(_connectionStringMaster)))
            {
                try
                {
                    if (tableHelper.DoesDatabaseExists(Database).Result)
                        tableHelper.DropDatabase(Database).Wait();
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }
    }
}