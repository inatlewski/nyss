﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Data.Queries;
using RX.Nyss.Web.Features.GlobalCoordinators.Dto;
using RX.Nyss.Web.Services;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.GlobalCoordinators
{
    public interface IGlobalCoordinatorService
    {
        Task<Result> Create(CreateGlobalCoordinatorRequestDto dto);
        Task<Result> Edit(EditGlobalCoordinatorRequestDto dto);
        Task<Result<GetGlobalCoordinatorResponseDto>> Get(int id);
        Task<Result<List<GetGlobalCoordinatorResponseDto>>> List();
        Task<Result> Delete(int id);
    }

    public class GlobalCoordinatorService : IGlobalCoordinatorService
    {
        private const string EnglishLanguageCode = "en";
        private readonly INyssContext _dataContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly IVerificationEmailService _verificationEmailService;
        private readonly IDeleteUserService _deleteUserService;

        public GlobalCoordinatorService(
            IIdentityUserRegistrationService identityUserRegistrationService,
            INyssContext dataContext,
            ILoggerAdapter loggerAdapter, IVerificationEmailService verificationEmailService, IDeleteUserService deleteUserService)
        {
            _identityUserRegistrationService = identityUserRegistrationService;
            _dataContext = dataContext;
            _loggerAdapter = loggerAdapter;
            _verificationEmailService = verificationEmailService;
            _deleteUserService = deleteUserService;
        }

        public async Task<Result> Create(CreateGlobalCoordinatorRequestDto dto)
        {
            try
            {
                string securityStamp;
                GlobalCoordinatorUser user;
                using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var identityUser = await _identityUserRegistrationService.CreateIdentityUser(dto.Email, Role.GlobalCoordinator);
                    securityStamp = await _identityUserRegistrationService.GenerateEmailVerification(identityUser.Email);

                    // Oponeo: get ApplicationLanguage by LanguageCode equals EnglishLanguageCode
                    // Make sure database returned zero or one row
                    ApplicationLanguage defaultUserApplicationLanguage = null;

                    user = new GlobalCoordinatorUser
                    {
                        IdentityUserId = identityUser.Id,
                        EmailAddress = identityUser.Email,
                        Name = dto.Name,
                        PhoneNumber = dto.PhoneNumber,
                        AdditionalPhoneNumber = dto.AdditionalPhoneNumber,
                        Organization = dto.Organization,
                        Role = Role.GlobalCoordinator,
                        ApplicationLanguage = defaultUserApplicationLanguage
                    };

                    // Oponeo: add a user and save changes

                    transactionScope.Complete();
                }

                await _verificationEmailService.SendVerificationEmail(user, securityStamp);

                return Success(ResultKey.User.Registration.Success);
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        public async Task<Result> Edit(EditGlobalCoordinatorRequestDto dto)
        {
            // Oponeo: Get Users
            // Apply extension method FilterAvailable() that gets only not-deleted users
            // Make sure database returned zero or one row filtering by user id and Role GlobalCoordinator
            User globalCoordinator = null;

            if (globalCoordinator == null)
            {
                _loggerAdapter.Debug($"Global coordinator with id {dto.Id} was not found");
                return Error(ResultKey.User.Common.UserNotFound);
            }

            // Oponeo: Set properties Name, PhoneNumber, AdditionalPhoneNumber, Organization and save changes

            return Success();
        }

        public async Task<Result<GetGlobalCoordinatorResponseDto>> Get(int id)
        {
            // Oponeo: Get Users
            // Apply extension method FilterAvailable() that gets only not-deleted users
            // Filter by user id and Role GlobalCoordinator
            // Select a new object of type GetGlobalCoordinatorResponseDto with the following properties:
            //      Id, Name, Email, PhoneNumber, AdditionalPhoneNumber, Organization
            // Make sure database returned zero or one row
            GetGlobalCoordinatorResponseDto globalCoordinator = null;

            if (globalCoordinator == null)
            {
                _loggerAdapter.Debug($"Global coordinator with id {id} was not found");
                return Error<GetGlobalCoordinatorResponseDto>(ResultKey.User.Common.UserNotFound);
            }

            return Success(globalCoordinator);
        }

        public async Task<Result<List<GetGlobalCoordinatorResponseDto>>> List()
        {
            // Oponeo: Get Users
            // Apply extension method FilterAvailable() that gets only not-deleted users
            // Filter by Role GlobalCoordinator
            // Select a new object of type GetGlobalCoordinatorResponseDto with the following properties:
            //      Id, Name, Email, PhoneNumber, AdditionalPhoneNumber, Organization
            // Order by Name
            // List query
            var globalCoordinators = new List<GetGlobalCoordinatorResponseDto>();

            return Success(globalCoordinators);
        }

        public async Task<Result> Delete(int id)
        {
            try
            {
                using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    // Oponeo: Get Users
                    // Apply extension method FilterAvailable() that gets only not-deleted users
                    // Get first row or null base on id
                    User globalCoordinator = null;

                    if (globalCoordinator == null)
                    {
                        _loggerAdapter.Debug($"Global coordinator with id {id} was not found");
                        throw new ResultException(ResultKey.User.Common.UserNotFound);
                    }

                    await _deleteUserService.EnsureCanDeleteUser(id, Role.GlobalCoordinator);

                    // Oponeo: Remove globalCoordinator from Users table and save changes

                    await _identityUserRegistrationService.DeleteIdentityUser(globalCoordinator.IdentityUserId);

                    transactionScope.Complete();
                }

                return Success();
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }
    }
}
