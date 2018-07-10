using System;
using System.Threading.Tasks;
using MicroService.Common.Core.Databases.Repository.MsSql.Impl;
using MicroService.Login.Repo.Sql.Models;
using MicroService.Login.Repo.Sql.Repositories.Implementation;
using Xunit;

namespace MicroService.Login.Repo.Test
{
    public class UserRepoTestFixture : TestFixtureBase
    {
        public UserRepository UserRepo { get; private set; }

        protected override string Database => "CoinsUser";

        public UserRepoTestFixture()
        {
            UserRepo = new UserRepository(new SqlConnectionFactory(ConnectionString));
        }
    }

    public class UserRepoTest : IClassFixture<UserRepoTestFixture>
    {
        private UserRepository _userRepo;

        public UserRepoTest(UserRepoTestFixture databaseFixture)
        {
            _userRepo = databaseFixture.UserRepo;
        }

        [Fact]
        public async Task InsertSameUsernameThorws()
        {
            await _userRepo.InsertAsync(new SqlUser()
            {
                Email = "email@13.c",
                Username = "user",
                Password = "pw",
            });

            await Assert.ThrowsAnyAsync<Exception>(async () => await _userRepo.InsertAsync(new SqlUser
            {
                Email = "email@12.c",
                Username = "user",
                Password = "pw",
            }));
        }

        [Fact]
        public async Task InsertSameEmailThorws()
        {
            await _userRepo.InsertAsync(new SqlUser
            {
                Email = "email1@a.c",
                Username = "user2",
                Password = "pw",
            });

            await Assert.ThrowsAnyAsync<Exception>(async () => await _userRepo.InsertAsync(new SqlUser
            {
                Email = "email1@a.c",
                Username = "user1",
                Password = "pw",
            }));
        }
    }
}