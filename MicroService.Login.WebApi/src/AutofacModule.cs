using System;
using Autofac;
using Microsoft.Extensions.Configuration;
using MicroService.Common.Core.Databases.Repository;
using MicroService.Common.Core.Databases.Repository.MongoDb;
using MicroService.Common.Core.Databases.Repository.MsSql.Impl;
using MicroService.Common.Core.Databases.Repository.MsSql.Interfaces;
using MicroService.Common.Core.Managers;
using MicroService.Common.Core.Services.impl;
using MicroService.Common.Core.Services.Interfaces;
using MicroService.Common.Core.Web;
using MicroService.Login.Repo.Sql.Repositories.Implementation;
using MicroService.Login.Repo.Sql.Repositories.Interfaces;
using MicroService.Login.Repo.Sql.Services.Implementation;
using MicroService.Login.Repo.Sql.Services.Interfaces;
using MicroService.Login.Security.Services.Impl;
using MicroService.Login.Security.Services.Interfaces;

namespace MicroService.Login.WebApi
{
    public class AutofacModule : Module
    {
        private readonly IConfiguration _configuration;

        public AutofacModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            #region Services

            var gmailAppPassword = _configuration.GetSection("GoogleEmail:AppPassword").Value;
            var gamilUsername = _configuration.GetSection("GoogleEmail:UserName").Value;


            builder.RegisterType<UserService>().As<IUserService>();

            builder.RegisterType<PasswordHashService>().As<IPasswordHashService>();
            builder.RegisterType<HashService>().As<IHashService>();
            builder.RegisterType<EmailService>().As<IEmailService>();
            builder.RegisterType<TokenValidationService>().As<ITokenValidationService>();
            builder.RegisterType<UserLookupService>().As<IUserLookupService>();

            builder.Register(c => new GmailSenderService(new GmailSettings(gmailAppPassword, gamilUsername))).As<IEmailSenderService>();

            #endregion

            #region HTTP

            //====================Http
            var jwtSecret = _configuration["JWT:Secret"];
            var jwtDefaultLifespan = TimeSpan.FromSeconds(double.Parse(_configuration["JWT:LifetimeInSec"]));


            builder.Register(c => new JsonWebTokenService(jwtSecret, jwtDefaultLifespan, c.Resolve<IHashService>())).As<IJsonWebTokenService>();

            builder.RegisterType<HttpRequestParser>().As<IHttpRequestParser>();

            #endregion

            #region MongoDb

            var mongoConnectionString = _configuration["MongoDB:ConnectionString"];
            var mongoDatabaseName = _configuration["MongoDB:Database"];

            builder.Register(c => new MongoDbConnectionFacotry(mongoConnectionString, mongoDatabaseName)).As<IMongoDbConnectionFacotry>();

            builder.RegisterGeneric(typeof(MongoRepository<,>)).As(typeof(IRepository<,>));

            #endregion

            #region Managers

            builder.RegisterType<TwoFactorAuthenticatorManager>().As<ITwoFactorAuthenticatorManager>().SingleInstance();

            #endregion

            #region SQL

            //====================SQL
            var msSqlConStringForMicroService = _configuration["MSSQL:ConnectionString:MicroService"];
//            var msSqlConStringForMaster = _configuration["MSSQL:ConnectionString:Master"];

            builder.Register(c => new SqlConnectionFactory(msSqlConStringForMicroService)).As<ISqlConnectionFactory>();
            builder.RegisterType<TransactionFactory>().As<ITransactionFactory>();
//            builder.Register(c => new SqlConnectionFactory(msSqlConStringForMicroService)).As<IConnectionForMicroService>();
//            builder.Register(c => new SqlConnectionFactory(msSqlConStringForMaster)).As<IConnectionForMaster>();

            builder.RegisterType<UserRepository>().As<IUserRepository>();
            builder.RegisterType<WhitelistedIpRepository>().As<IWhitelistedIpRepository>();
            builder.RegisterType<LoginAttemptRepository>().As<ILoginAttemptRepository>();
            builder.RegisterType<RefreshTokenRepository>().As<IRefreshTokenRepository>();

            builder.RegisterType<UserRepositoryService>().As<IUserRepositoryService>();
            builder.RegisterType<WhitelistedIpRepositoryService>().As<IWhitelistedIpRepositoryService>();
            builder.RegisterType<LoginAttemptsRepositoryService>().As<ILoginAttemptsRepositoryService>();
            builder.RegisterType<RefreshTokenRepositoryService>().As<IRefreshTokenRepositoryService>();

            #endregion
        }
    }
}