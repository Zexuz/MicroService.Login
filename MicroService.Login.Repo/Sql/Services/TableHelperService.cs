using System.Threading.Tasks;
using MicroService.Common.Core.Databases.Repository.MsSql.Interfaces;
using MicroService.Login.Repo.Sql.Tables;

namespace MicroService.Login.Repo.Sql.Services
{
    public class MigrationService
    {
        private readonly ITableHelper _tableHelper;

        public MigrationService(ITableHelper tableHelper)
        {
            _tableHelper = tableHelper;
        }

        public async Task CleanDatabase(string databaseName)
        {
            if (await _tableHelper.DoesDatabaseExists(databaseName))
                await _tableHelper.DropDatabase(databaseName);
            
            await _tableHelper.ValidateTableExists(new TierTable(), databaseName);
            await _tableHelper.ValidateTableExists(new UserTable(), databaseName);
            await _tableHelper.ValidateTableExists(new WhitelistedIpTable(), databaseName);
            await _tableHelper.ValidateTableExists(new RefreshTokenTable(), databaseName);
            await _tableHelper.ValidateTableExists(new LoginAttemtTable(), databaseName);
            await _tableHelper.ValidateTableExists(new TransactionTable(), databaseName);
            await _tableHelper.ValidateTableExists(new ReviewTable(), databaseName);
            await _tableHelper.ValidateTableExists(new VerifiedDomainUserTable(), databaseName);

            await _tableHelper.AddConstrainst(new UserTable(), databaseName);
        }
    }
}