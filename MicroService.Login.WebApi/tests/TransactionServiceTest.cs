using System.Data;
using System.Data.Common;
using FakeItEasy;
using MicroService.Common.Core.Databases.Repository.MsSql.Interfaces;
using MicroService.Common.Core.Exceptions;
using MicroService.Login.Models.RepoService;
using MicroService.Login.Repo.Sql.Services.Interfaces;
using MicroService.Login.Security.Services.Impl;
using MicroService.Login.Security.Services.Interfaces;
using Xunit;

namespace MicroService.Login.Webapi.Test
{
    public class TransactionServiceTest
    {
        private readonly TransactionService            _transactrionService;
        private readonly ITransactionRepositoryService _mongoTranscrionRepo;
        private readonly IUserRepositoryService        _userRepoService;
        private readonly IReviewRepositoryService      _reviewRepositoryService;
        private readonly ITransactionWrapper           _transactionWrapper;

        public TransactionServiceTest()
        {
            _mongoTranscrionRepo = A.Fake<ITransactionRepositoryService>();
            _reviewRepositoryService = A.Fake<IReviewRepositoryService>();


            _userRepoService = A.Fake<IUserRepositoryService>();
            var feeService = A.Fake<IFeeService>();

            var sqlTransactionFactory = A.Fake<ITransactionFactory>();

            _transactrionService = new TransactionService
            (
                _mongoTranscrionRepo,
                _userRepoService,
                _reviewRepositoryService,
                sqlTransactionFactory,
                feeService
            );

            _transactionWrapper = A.Fake<ITransactionWrapper>();
            A.CallTo(() => sqlTransactionFactory.BeginTransaction(A<IsolationLevel>._)).Returns(_transactionWrapper);
        }

        [Fact]
        public async void SendCoinsSuccess()
        {
            var ammount = 100;
            var fromUser = new User {Id = 1, Balance = 200,};
            var toUser = new User {Id = 2};


            A.CallTo(() => _userRepoService.FindAsync(fromUser.Id, A<DbTransaction>._)).Returns(fromUser);
            A.CallTo(() => _userRepoService.FindAsync(toUser.Id, A<DbTransaction>._)).Returns(toUser);

            var result = await _transactrionService.SendCoinsAsync(fromUser.Id, toUser.Id, ammount);

            A.CallTo(() => _userRepoService.FindAsync(fromUser.Id, A<DbTransaction>._)).MustHaveHappened();
            A.CallTo(() => _userRepoService.FindAsync(toUser.Id, A<DbTransaction>._)).MustHaveHappened();
            A.CallTo(() => _userRepoService.ChangeBalance(fromUser.Id, -100, A<IDbTransaction>._)).MustHaveHappened();
            A.CallTo(() => _userRepoService.ChangeBalance(toUser.Id, 100, A<IDbTransaction>._)).MustHaveHappened();
            A.CallTo(() => _mongoTranscrionRepo.InsertAsync(A<Transaction>._, A<IDbTransaction>._)).MustHaveHappened();
            A.CallTo(() => _reviewRepositoryService.InsertAsync(A<Review>._, A<IDbTransaction>._)).MustHaveHappened();
            A.CallTo(() => _transactionWrapper.Commit()).MustHaveHappened();
            A.CallTo(() => _transactionWrapper.Rollback()).MustNotHaveHappened();

            Assert.True(result);
        }

        [Theory]
        [InlineData(-99)]
        [InlineData(-50)]
        [InlineData(-1)]
        [InlineData(-0)]
        [InlineData(0)]
        public async void SendCoinsFailsNegatveAmmount(int ammount)
        {
            var fromUser = new User {Id = 1, Balance = 1000};
            var toUser = new User {Id = 2};


            A.CallTo(() => _userRepoService.FindAsync(fromUser.Id, A<DbTransaction>._)).Returns(fromUser);

            await Assert.ThrowsAnyAsync<NegativeAmmountException>(async () =>
                await _transactrionService.SendCoinsAsync(fromUser.Id, toUser.Id, ammount));
            A.CallTo(() => _transactionWrapper.Commit()).MustNotHaveHappened();
            A.CallTo(() => _transactionWrapper.Rollback()).MustHaveHappened();
        }

        [Theory]
        [InlineData(99)]
        [InlineData(50)]
        [InlineData(1)]
        [InlineData(0)]
        public async void SendCoinsFailsInsufficientFunds(int balance)
        {
            var ammount = 100;
            var fromUser = new User {Id = 1, Balance = balance};
            var toUser = new User {Id = 2};


            A.CallTo(() => _userRepoService.FindAsync(fromUser.Id, A<DbTransaction>._)).Returns(fromUser);


            await Assert.ThrowsAnyAsync<InsufficientFoundsException>(async () =>
                await _transactrionService.SendCoinsAsync(fromUser.Id, toUser.Id, ammount));
            A.CallTo(() => _transactionWrapper.Commit()).MustNotHaveHappened();
            A.CallTo(() => _transactionWrapper.Rollback()).MustHaveHappened();
        }

        [Fact]
        public async void SendCoinsFailsSuspendedSenderAccount()
        {
            var ammount = 100;
            var fromUser = new User {Id = 1, Balance = 900, IsSuspended = true};
            var toUser = new User {Id = 2};


            A.CallTo(() => _userRepoService.FindAsync(fromUser.Id, A<DbTransaction>._)).Returns(fromUser);

            await Assert.ThrowsAnyAsync<UserSuspendedException>(async () =>
                await _transactrionService.SendCoinsAsync(fromUser.Id, toUser.Id, ammount));
            A.CallTo(() => _transactionWrapper.Commit()).MustNotHaveHappened();
            A.CallTo(() => _transactionWrapper.Rollback()).MustHaveHappened();
        }

        [Fact]
        public async void SendCoinsFailsSuspendedReceiverAccount()
        {
            var ammount = 100;
            var fromUser = new User {Id = 1, Balance = 900, IsSuspended = false};
            var toUser = new User {Id = 2, IsSuspended = true};


            A.CallTo(() => _userRepoService.FindAsync(fromUser.Id, A<DbTransaction>._)).Returns(fromUser);
            A.CallTo(() => _userRepoService.FindAsync(toUser.Id, A<DbTransaction>._)).Returns(toUser);

            await Assert.ThrowsAnyAsync<UserSuspendedException>(async () =>
                await _transactrionService.SendCoinsAsync(fromUser.Id, toUser.Id, ammount));
            A.CallTo(() => _transactionWrapper.Commit()).MustNotHaveHappened();
            A.CallTo(() => _transactionWrapper.Rollback()).MustHaveHappened();
        }

        [Fact]
        public async void SendCoinsFailsReceivingAccountNotFound()
        {
            var ammount = 100;
            var fromUser = new User {Id = 1, Balance = 900, IsSuspended = false};


            A.CallTo(() => _userRepoService.FindAsync(fromUser.Id, A<DbTransaction>._)).Returns(fromUser);
            A.CallTo(() => _userRepoService.FindAsync(5, A<DbTransaction>._)).Returns<User>(null);

            await Assert.ThrowsAnyAsync<UserDoesNotExistException>(async () => await _transactrionService.SendCoinsAsync(fromUser.Id, 5, ammount));
            A.CallTo(() => _transactionWrapper.Commit()).MustNotHaveHappened();
            A.CallTo(() => _transactionWrapper.Rollback()).MustHaveHappened();
        }
    }
}