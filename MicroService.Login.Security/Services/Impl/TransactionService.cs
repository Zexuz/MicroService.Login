using System;
using System.Data;
using System.Threading.Tasks;
using MicroService.Common.Core.Databases.Repository.MsSql.Interfaces;
using MicroService.Common.Core.Exceptions;
using MicroService.Login.Models.RepoService;
using MicroService.Login.Repo.Sql.Services.Interfaces;
using MicroService.Login.Security.Services.Interfaces;
using Transaction = MicroService.Login.Models.RepoService.Transaction;

namespace MicroService.Login.Security.Services.Impl
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepositoryService _transactionRepositoryService;
        private readonly IUserRepositoryService        _userRepositoryService;
        private readonly IReviewRepositoryService      _reviewRepositoryService;
        private readonly ITransactionFactory           _transactionFactory;
        private readonly IFeeService                   _feeService;

        public TransactionService
        (
            ITransactionRepositoryService transactionRepositoryService,
            IUserRepositoryService userRepositoryService,
            IReviewRepositoryService reviewRepositoryService,
            ITransactionFactory transactionFactory,
            IFeeService feeService
        )
        {
            _transactionRepositoryService = transactionRepositoryService;
            _userRepositoryService = userRepositoryService;
            _reviewRepositoryService = reviewRepositoryService;
            _transactionFactory = transactionFactory;
            _feeService = feeService;
        }

        public async Task<bool> SendCoinsAsync(int fromUserId, int toUserId, int ammount)
        {
            //----start transaction with read lock
            using (var transactionWrapper = _transactionFactory.BeginTransaction(IsolationLevel.RepeatableRead))
            {
                var dbtransaction = transactionWrapper.Transaction;
                try
                {
                    var fromUser = await _userRepositoryService.FindAsync(fromUserId, dbtransaction);
                    var toUser = await _userRepositoryService.FindAsync(toUserId, dbtransaction);

                    var coinsFee = await _feeService.CalculateFeeForTransactionAsync(fromUser, toUser, ammount);

                    if (toUser == null)
                        throw new UserDoesNotExistException("Error message"); //TODO fix this error message

                    if (fromUser.IsSuspended)
                        throw new UserSuspendedException("Error message"); //TODO fix this error message

                    if (toUser.IsSuspended)
                        throw new UserSuspendedException("Error message"); //TODO fix this error message

                    if (fromUser.Balance < ammount)
                        throw new InsufficientFoundsException("Error message"); //TODO fix this error message

                    if (ammount <= 0)
                        throw new NegativeAmmountException("Ammount must be more then 1");

                    await _userRepositoryService.ChangeBalance(fromUserId, -ammount, dbtransaction);
                    await _userRepositoryService.ChangeBalance(toUserId, ammount, dbtransaction);

                    var transaction = new Transaction
                    {
                        Created = DateTimeOffset.Now,
                        Fee = coinsFee,
                        FromUserId = fromUserId,
                        ToUserId = toUserId,
                        NrOfCoins = ammount
                    };

                    var transactionId = await _transactionRepositoryService.InsertAsync(transaction, dbtransaction);

                    var review = new Review
                    {
                        IsPositive = true,
                        Text = null,
                        Id = transactionId,
                        Updated = DateTimeOffset.Now,
                        UserId = fromUserId,
                        Valid = true,
                    };

                    await _reviewRepositoryService.InsertAsync(review, dbtransaction);
                }
                catch (Exception)
                {
                    transactionWrapper.Rollback();
                    throw;
                }

                transactionWrapper.Commit();
            }

            return true; //TODO Return the transactionViewModel instead of a bool
        }
    }
}