using System.Threading.Tasks;
using FakeItEasy;
using MicroService.Login.Models.RepoService;
using MicroService.Login.Repo.Sql.Models;
using MicroService.Login.Repo.Sql.Services.Interfaces;
using MicroService.Login.Security.Services.Impl;
using MicroService.Login.Security.Services.Interfaces;
using Xunit;

namespace MicroService.Login.Webapi.Test
{
    public class FeeServiceTest
    {
        private ITierRepositoryService _tierRepoService;
        private IFeeService            _feeSerfvice;

        public FeeServiceTest()
        {
            _tierRepoService = A.Fake<ITierRepositoryService>();
            _feeSerfvice = new FeeService(_tierRepoService);
        }


        [Fact]
        public async Task BussniesToUserFee()
        {
            var fromUser = new User
            {
                DomainId = 1,
                TierId = 2
            };
            var toUser = new User
            {
                DomainId = null,
                TierId = 1
            };
            var ammount = 1000;

            A.CallTo(() => _tierRepoService.GetTierAsync(fromUser.TierId)).Returns(new SqlTier
            {
                Name = "Verified domain",
                TransferFee = new decimal(1),
                DepositFee = new decimal(0.5),
                WithdrawalFee = new decimal(0.5),
                Id = 2
            });
            A.CallTo(() => _tierRepoService.GetTierAsync(toUser.TierId)).Returns(new SqlTier
            {
                Name = "Standard",
                TransferFee = new decimal(3),
                DepositFee = new decimal(5),
                WithdrawalFee = new decimal(5),
                Id = 1
            });

            var fee = await _feeSerfvice.CalculateFeeForTransactionAsync(fromUser, toUser, ammount);

            Assert.Equal(10, fee);

            A.CallTo(() => _tierRepoService.GetTierAsync(fromUser.TierId)).MustHaveHappened();
            A.CallTo(() => _tierRepoService.GetTierAsync(toUser.TierId)).MustHaveHappened();
        }

        [Fact]
        public async Task UserToBussniesFee()
        {
            var toUser = new User
            {
                DomainId = 1,
                TierId = 2
            };
            var fromUser = new User
            {
                DomainId = null,
                TierId = 1
            };
            var ammount = 1000;

            A.CallTo(() => _tierRepoService.GetTierAsync(2)).Returns(new SqlTier
            {
                Name = "Verified domain",
                TransferFee = new decimal(1),
                DepositFee = new decimal(0.5),
                WithdrawalFee = new decimal(0.5),
            });
            A.CallTo(() => _tierRepoService.GetTierAsync(1)).Returns(new SqlTier
            {
                Name = "Standard",
                TransferFee = new decimal(3),
                DepositFee = new decimal(5),
                WithdrawalFee = new decimal(5),
            });

            var fee = await _feeSerfvice.CalculateFeeForTransactionAsync(fromUser, toUser, ammount);

            Assert.Equal(10, fee);

            A.CallTo(() => _tierRepoService.GetTierAsync(fromUser.TierId)).MustHaveHappened();
            A.CallTo(() => _tierRepoService.GetTierAsync(toUser.TierId)).MustHaveHappened();
        }

        [Fact]
        public async Task UserToUserFee()
        {
            var fromUser = new User
            {
                DomainId = null,
                TierId = 2
            };
            var toUser = new User
            {
                DomainId = null,
                TierId = 1
            };
            var ammount = 1000;

            A.CallTo(() => _tierRepoService.GetTierAsync(2)).Returns(new SqlTier
            {
                Name = "Premium",
                TransferFee = new decimal(2),
                DepositFee = new decimal(0.5),
                WithdrawalFee = new decimal(0.5),
            });
            A.CallTo(() => _tierRepoService.GetTierAsync(1)).Returns(new SqlTier
            {
                Name = "Standard",
                TransferFee = new decimal(3),
                DepositFee = new decimal(5),
                WithdrawalFee = new decimal(5),
            });

            var fee = await _feeSerfvice.CalculateFeeForTransactionAsync(fromUser, toUser, ammount);

            Assert.Equal(20, fee);

            A.CallTo(() => _tierRepoService.GetTierAsync(fromUser.TierId)).MustHaveHappened();
            A.CallTo(() => _tierRepoService.GetTierAsync(toUser.TierId)).MustHaveHappened();
        }

        [Fact]
        public async Task BussnieToBussniesFee()
        {
            var fromUser = new User
            {
                DomainId = 1,
                TierId = 2
            };
            var toUser = new User
            {
                DomainId = 2,
                TierId = 1
            };
            var ammount = 1000;

            A.CallTo(() => _tierRepoService.GetTierAsync(fromUser.TierId)).Returns(new SqlTier
            {
                Name = "Verified domain",
                TransferFee = new decimal(2),
                DepositFee = new decimal(0.5),
                WithdrawalFee = new decimal(0.5),
                Id = 2
            });
            A.CallTo(() => _tierRepoService.GetTierAsync(toUser.TierId)).Returns(new SqlTier
            {
                Name = "Standard",
                TransferFee = new decimal(3),
                DepositFee = new decimal(5),
                WithdrawalFee = new decimal(5),
                Id = 1
            });

            var fee = await _feeSerfvice.CalculateFeeForTransactionAsync(fromUser, toUser, ammount);

            Assert.Equal(20, fee);

            A.CallTo(() => _tierRepoService.GetTierAsync(fromUser.TierId)).MustHaveHappened();
            A.CallTo(() => _tierRepoService.GetTierAsync(toUser.TierId)).MustHaveHappened();
        }
    }
}