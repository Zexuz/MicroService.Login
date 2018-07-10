using System.Net;
using System.Threading.Tasks;
using Dapper;
using MicroService.Common.Core.Databases.Repository.MsSql.Impl;
using MicroService.Common.Core.Databases.Repository.MsSql.Interfaces;
using MicroService.Login.Repo.Sql.Models;
using MicroService.Login.Repo.Sql.Repositories.Interfaces;

namespace MicroService.Login.Repo.Sql.Repositories.Implementation
{
    public class WhitelistedIpRepository : SqlRepositoryBase<SqlWhitelistedIp>, IWhitelistedIpRepository
    {
        public WhitelistedIpRepository(ISqlConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        public async Task<bool> RemoveAsync(int userId, string ipAddress)
        {
            using (var cn = ConnectionFactory.GetNewOpenConnection())
            {
                var res = await cn.ExecuteAsync("DELETE FROM WhitelistedIp WHERE UserId =@UserId AND IpAddress =@Ip", new
                {
                    UserId = userId,
                    Ip = ipAddress
                });

                return res == 1;
            }
        }

        public async Task<SqlWhitelistedIp> FindAsync(int userId, IPAddress ipAddress)
        {
            using (var cn = ConnectionFactory.GetNewOpenConnection())
            {
                var res = await cn.QueryFirstOrDefaultAsync<SqlWhitelistedIp>("SELECT * FROM WhitelistedIp WHERE UserId =@UserId AND IpAddress =@Ip",
                    new
                    {
                        UserId = userId,
                        Ip = ipAddress.ToString()
                    });

                return res;
            }
        }
    }
}