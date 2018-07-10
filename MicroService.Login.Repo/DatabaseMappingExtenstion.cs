using System.Net;
using MicroService.Common.Core.ValueTypes.Types;
using MicroService.Login.Models.RepoService;

namespace MicroService.Login.Repo
{
    internal static class DatabaseMappingExtenstion
    {
        public static Repo.Sql.Models.SqlUser ToDatabase(this User user)
        {
            if (user == null)
                return null;

            return new Repo.Sql.Models.SqlUser
            {
                Id = user.Id,
                Created = user.Created,
                Email = user.Email.Value,
                Password = user.Password,
                Role = user.Role,
                Username = user.Username,
                TwoFactorSecret = user.TwoFactorSecret,
                EmailVerified = user.EmailVerified,
                Balance = user.Balance,
                DomainId = user.DomainId,
                IsSuspended = user.IsSuspended,
                TierId = user.TierId
            };
        }


        public static User FromDatabase(this Repo.Sql.Models.SqlUser user)
        {
            if (user == null)
                return null;

            return new User
            {
                Id = user.Id,
                Created = user.Created,
                Email = new Email(user.Email),
                Password = user.Password,
                Role = user.Role,
                Username = user.Username,
                TwoFactorSecret = user.TwoFactorSecret,
                EmailVerified = user.EmailVerified,
                Balance = user.Balance,
                DomainId = user.DomainId,
                IsSuspended = user.IsSuspended,
                TierId = user.TierId
            };
        }

        public static Repo.Sql.Models.SqlWhitelistedIp ToDatabase(this WhitelistedIp whitelistedIp)
        {
            if (whitelistedIp == null)
                return null;

            return new Repo.Sql.Models.SqlWhitelistedIp
            {
                Id = whitelistedIp.Id,
                Added = whitelistedIp.Added,
                IpAddress = whitelistedIp.IpAddress.ToString(),
                UserId = whitelistedIp.UserId
            };
        }

        public static WhitelistedIp FromDatabase(this Repo.Sql.Models.SqlWhitelistedIp sqlWhitelistedIp)
        {
            if (sqlWhitelistedIp == null)
                return null;

            return new WhitelistedIp
            {
                Id = sqlWhitelistedIp.Id,
                Added = sqlWhitelistedIp.Added,
                IpAddress = IPAddress.Parse(sqlWhitelistedIp.IpAddress),
                UserId = sqlWhitelistedIp.UserId
            };
        }


        public static Repo.Sql.Models.SqlLoginAttempt ToDatabase(this LoginAttempt loginAttempt)
        {
            if (loginAttempt == null)
                return null;

            return new Repo.Sql.Models.SqlLoginAttempt
            {
                Id = loginAttempt.Id,
                Browser = loginAttempt.Browser,
                Date = loginAttempt.Date,
                Device = loginAttempt.Device,
                FromIp = loginAttempt.FromIp,
                Os = loginAttempt.Os,
                Reason = loginAttempt.Reason,
                Success = loginAttempt.Success,
                UserId = loginAttempt.UserId,
            };
        }

        public static LoginAttempt FromDatabase(this Repo.Sql.Models.SqlLoginAttempt loginAttempt)
        {
            if (loginAttempt == null)
                return null;

            return new LoginAttempt
            {
                Id = loginAttempt.Id,
                Browser = loginAttempt.Browser,
                Date = loginAttempt.Date,
                Device = loginAttempt.Device,
                FromIp = loginAttempt.FromIp,
                Os = loginAttempt.Os,
                Reason = loginAttempt.Reason,
                Success = loginAttempt.Success,
                UserId = loginAttempt.UserId,
            };
        }

        public static Repo.Sql.Models.SqlRefreshToken ToDatabase(this RefreshToken refreshToken)
        {
            if (refreshToken == null)
                return null;

            return new Repo.Sql.Models.SqlRefreshToken
            {
                Id = refreshToken.Id,
                Browser = refreshToken.Browser,
                Device = refreshToken.Device,
                FromIp = refreshToken.FromIp,
                Os = refreshToken.Os,
                UserId = refreshToken.UserId,
                CountryCode = refreshToken.CountryCode,
                Created = refreshToken.Created,
                Revoked = refreshToken.Revoked,
                Valid = refreshToken.Valid,
                Value = refreshToken.Value,
                LastUsed = refreshToken.LastUsed
            };
        }

        public static RefreshToken FromDatabase(this Repo.Sql.Models.SqlRefreshToken refreshToken)
        {
            if (refreshToken == null)
                return null;

            return new RefreshToken
            {
                Id = refreshToken.Id,
                Browser = refreshToken.Browser,
                Device = refreshToken.Device,
                FromIp = refreshToken.FromIp,
                Os = refreshToken.Os,
                UserId = refreshToken.UserId,
                CountryCode = refreshToken.CountryCode,
                Created = refreshToken.Created,
                Revoked = refreshToken.Revoked,
                Valid = refreshToken.Valid,
                Value = refreshToken.Value,
                LastUsed = refreshToken.LastUsed
            };
        }

        public static Repo.Sql.Models.SqlVerifiedDomainUser ToDatabase(this VerifiedDomainUser verifiedDomainUser)
        {
            if (verifiedDomainUser == null)
                return null;

            return new Repo.Sql.Models.SqlVerifiedDomainUser
            {
                AllowDeposit = verifiedDomainUser.AllowDeposit,
                Created = verifiedDomainUser.Created,
                Description = verifiedDomainUser.Description,
                Id = verifiedDomainUser.Id,
                OwnerId = verifiedDomainUser.OwnerId,
                WebsiteUrl = verifiedDomainUser.WebsiteUrl,
                LastUpdated = verifiedDomainUser.LastUpdated
            };
        }

        public static VerifiedDomainUser FromDatabase(this Repo.Sql.Models.SqlVerifiedDomainUser verifiedDomainUser)
        {
            if (verifiedDomainUser == null)
                return null;

            return new VerifiedDomainUser
            {
                AllowDeposit = verifiedDomainUser.AllowDeposit,
                Created = verifiedDomainUser.Created,
                Description = verifiedDomainUser.Description,
                Id = verifiedDomainUser.Id,
                OwnerId = verifiedDomainUser.OwnerId,
                WebsiteUrl = verifiedDomainUser.WebsiteUrl,
                LastUpdated = verifiedDomainUser.LastUpdated
            };
        }

        public static Repo.Sql.Models.SqlTransaction ToDatabase(this Transaction transaction)
        {
            if (transaction == null)
                return null;

            return new Repo.Sql.Models.SqlTransaction
            {
                Created = transaction.Created,
                Fee = transaction.Fee,
                FromUser = transaction.FromUserId,
                ToUser = transaction.ToUserId,
                Id = transaction.Id,
                NrOfCoins = transaction.NrOfCoins
            };
        }

        public static Transaction FromDatabase(this Repo.Sql.Models.SqlTransaction transaction)
        {
            if (transaction == null)
                return null;

            return new Transaction
            {
                Created = transaction.Created,
                Fee = transaction.Fee,
                FromUserId = transaction.FromUser,
                ToUserId = transaction.ToUser,
                Id = transaction.Id,
                NrOfCoins = transaction.NrOfCoins
            };
        }

        public static Repo.Sql.Models.SqlReview ToDatabase(this Review review)
        {
            if (review == null)
                return null;

            return new Repo.Sql.Models.SqlReview
            {
                Id = review.Id,
                IsPositive = review.IsPositive,
                Text = review.Text,
                Updated = review.Updated,
                UserId = review.UserId,
                Valid = review.Valid,
            };
        }

        public static Review FromDatabase(this Repo.Sql.Models.SqlReview review)
        {
            if (review == null)
                return null;

            return new Review
            {
                Id = review.Id,
                IsPositive = review.IsPositive,
                Text = review.Text,
                Updated = review.Updated,
                UserId = review.UserId,
                Valid = review.Valid,
            };
        }
    }
}