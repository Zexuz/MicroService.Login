using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroService.Login.Models.RepoService;
using MicroService.Login.Models.ViewModel;
using MicroService.Login.Repo;
using MicroService.Login.Repo.Sql.Services.Interfaces;
using MicroService.Login.Security.Services.Interfaces;

namespace MicroService.Login.Security.Services.Impl
{
    public class TransactionLookupService : ITransactionLookupService
    {
        private          ITransactionRepositoryService _transactionRepositoryService;
        private readonly IUserLookupService            _userLookupService;
        private readonly IReviewRepositoryService      _reviewRepositoryService;
        private          int                           _pageSize;

        public TransactionLookupService
        (
            ITransactionRepositoryService transactionRepositoryService,
            IUserLookupService userLookupService,
            IReviewRepositoryService reviewRepositoryService
        )
        {
            _transactionRepositoryService = transactionRepositoryService;
            _userLookupService = userLookupService;
            _reviewRepositoryService = reviewRepositoryService;

            _pageSize = 10;
        }

        public async Task<PaginatedItemsModel<TransactionViewModel>> GetAsync(int userId, int page)
        {
            var result = await _transactionRepositoryService.GetAsync(userId, _pageSize, _pageSize * page);
            var transViewModels = await CreateTransactionViewModels(userId, result);

            return new PaginatedItemsModel<TransactionViewModel>(page, _pageSize, result.TotalCount, transViewModels);
        }

        public async Task<PaginatedItemsModel<TransactionViewModel>> GetSentAsync(int userId, int page)
        {
            var result = await _transactionRepositoryService.GetSentAsync(userId, _pageSize, _pageSize * page);
            var transViewModels = await CreateTransactionViewModels(userId, result);

            return new PaginatedItemsModel<TransactionViewModel>(page, _pageSize, result.TotalCount, transViewModels);
        }

        public async Task<PaginatedItemsModel<TransactionViewModel>> GetReceivedAsync(int userId, int page)
        {
            var result = await _transactionRepositoryService.GetReceivedAsync(userId, _pageSize, _pageSize * page);

            var transViewModels = await CreateTransactionViewModels(userId, result);

            return new PaginatedItemsModel<TransactionViewModel>(page, _pageSize, result.TotalCount, transViewModels);
        }

        private async Task<List<TransactionViewModel>> CreateTransactionViewModels(int userId, SqlPaginationResult<Transaction> result)
        {
            var transacrionIds = result.Data.Select(transaction => transaction.Id);

            var reviews = await _reviewRepositoryService.GetAsync(transacrionIds);


            var userSent = result.Data.Select(transaction => transaction.ToUserId);
            var userReceived = result.Data.Select(transaction => transaction.FromUserId);

            var uniqueUserIds = userSent.Concat(userReceived).Distinct().ToList();
            var uniqueUsers = await _userLookupService.FindAsync(uniqueUserIds);

            var transViewModels = new List<TransactionViewModel>();

            foreach (var transaction in result.Data)
            {
                var isSender = transaction.FromUserId == userId;

                var id = isSender ? transaction.ToUserId : transaction.FromUserId;
                var userViewModel = uniqueUsers.First(model => model.Id == id);
                var user = new TransactionViewModel.User
                {
                    Username = userViewModel.Username,
                    DomainName = userViewModel.VerifiedDomain?.Website.Host
                };

                var reviewModel = reviews.First(review => review.Id == transaction.Id);

                var item = new TransactionViewModel
                {
                    Created = transaction.Created,
                    Fee = transaction.Fee,
                    NrOfCoins = transaction.NrOfCoins,
                    AmISender = isSender,
                    Partner = user,
                    Review = new ReviewViewModel
                    {
                        Id = reviewModel.Id,
                        IsPositive = reviewModel.IsPositive,
                        LastUpdated = reviewModel.Updated,
                        IsMy = isSender,
                        Text = reviewModel.Text
                    }
                };

                transViewModels.Add(item);
            }

            return transViewModels;
        }
    }
}