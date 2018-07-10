using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MicroService.Common.Core.Databases.Repository.MsSql.Impl;
using MicroService.Common.Core.Databases.Repository.MsSql.Interfaces;
using MicroService.Login.Models.RepoService;
using MicroService.Login.Repo.Sql.Models;
using MicroService.Login.Repo.Sql.Repositories.Interfaces;
using MicroService.Login.Repo.Sql.Services.Interfaces;

namespace MicroService.Login.Repo.Sql.Repositories.Implementation
{
    public class TransactionRepository : SqlRepositoryBase<SqlTransaction>, ITransactionRepository
    {
        public TransactionRepository(ISqlConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        public async Task<SqlPaginationResult<Transaction>> GetAsync(int userId, int nrOfItems, int skip)
        {
            var queryFind = new SqlQuery
            {
                Query =
                    "SELECT * FROM [Transaction] WHERE (FromUser=@userId OR ToUser=@userId) ORDER BY Created OFFSET (@skip) ROWS FETCH NEXT (@nrOfItems) ROWS ONLY",
                Data = new {NrOfItems = nrOfItems, UserId = userId, Skip = skip}
            };

            var queryCount = new SqlQuery
            {
                Query = "SELECT Count(*) FROM [Transaction] WHERE (FromUser=@userId OR ToUser=@userId)",
                Data = new {UserId = userId}
            };

            return await Execute(queryFind, queryCount);
        }

        public async Task<SqlPaginationResult<Transaction>> GetSentAsync(int userId, int nrOfItems, int skip)
        {
            var queryFind = new SqlQuery
            {
                Query = "SELECT * FROM [Transaction] WHERE FromUser=@userId ORDER BY Created OFFSET (@skip) ROWS FETCH NEXT (@nrOfItems) ROWS ONLY",
                Data = new {NrOfItems = nrOfItems, UserId = userId, Skip = skip}
            };

            var queryCount = new SqlQuery
            {
                Query = "SELECT Count(*) FROM [Transaction] WHERE FromUser=@userId",
                Data = new {UserId = userId}
            };

            return await Execute(queryFind, queryCount);
        }

        public async Task<SqlPaginationResult<Transaction>> GetReceivedAsync(int userId, int nrOfItems, int skip)
        {
            var queryFind = new SqlQuery
            {
                Query = "SELECT * FROM [Transaction] WHERE ToUser=@userId ORDER BY Created OFFSET (@skip) ROWS FETCH NEXT (@nrOfItems) ROWS ONLY",
                Data = new {NrOfItems = nrOfItems, UserId = userId, Skip = skip}
            };

            var queryCount = new SqlQuery
            {
                Query = "SELECT Count(*) FROM [Transaction] WHERE ToUser=@userId",
                Data = new {UserId = userId}
            };

            return await Execute(queryFind, queryCount);
        }

        private async Task<SqlPaginationResult<Transaction>> Execute(SqlQuery queryFind, SqlQuery queryCount)
        {
            IEnumerable<SqlTransaction> data;
            long count;
            using (var cn = ConnectionFactory.GetNewOpenConnection())
            {
                data = await cn.QueryAsync<SqlTransaction>(queryFind.Query, queryFind.Data);
            }

            using (var cn = ConnectionFactory.GetNewOpenConnection())
            {
                count = await cn.ExecuteScalarAsync<long>(queryCount.Query, queryCount.Data);
            }

            return new SqlPaginationResult<Transaction>
            {
                Data = data.Select(transaction => transaction.FromDatabase()).ToList(),
                TotalCount = count
            };
        }

        private struct SqlQuery
        {
            public string Query { get; set; }
            public object Data  { get; set; }
        }
    }
}