﻿using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.TechnicalAdvisor.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.TechnicalAdvisor
{
    public interface ITechnicalAdvisorService
    {
        Task<Result> CreateTechnicalAdvisor(int nationalSocietyId, CreateTechnicalAdvisorRequestDto createTechnicalAdvisorRequestDto);
        Task<Result<GetTechnicalAdvisorResponseDto>> GetTechnicalAdvisor(int technicalAdvisorId);
        Task<Result> UpdateTechnicalAdvisor(int technicalAdvisorId, EditTechnicalAdvisorRequestDto editTechnicalAdvisorRequestDto);
        Task<Result> DeleteTechnicalAdvisor(int TechnicalAdvisorId);
    }

    public class TechnicalAdvisorService : ITechnicalAdvisorService
    {
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssContext _dataContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;
        private readonly INationalSocietyUserService _nationalSocietyUserService;
        private readonly IVerificationEmailService _verificationEmailService;

        public TechnicalAdvisorService(IIdentityUserRegistrationService identityUserRegistrationService, INationalSocietyUserService nationalSocietyUserService, INyssContext dataContext, ILoggerAdapter loggerAdapter, IVerificationEmailService verificationEmailService)
        {
            _identityUserRegistrationService = identityUserRegistrationService;
            _dataContext = dataContext;
            _loggerAdapter = loggerAdapter;
            _nationalSocietyUserService = nationalSocietyUserService;
            _verificationEmailService = verificationEmailService;
        }

        public async Task<Result> CreateTechnicalAdvisor(int nationalSocietyId, CreateTechnicalAdvisorRequestDto createTechnicalAdvisorRequestDto)
        {
            try
            {
                string securityStamp;
                TechnicalAdvisorUser user;
                using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var identityUser = await _identityUserRegistrationService.CreateIdentityUser(createTechnicalAdvisorRequestDto.Email, Role.TechnicalAdvisor);
                    securityStamp = await _identityUserRegistrationService.GenerateEmailVerification(identityUser.Email);
                    user = await CreateTechnicalAdvisorUser(identityUser, nationalSocietyId, createTechnicalAdvisorRequestDto);

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

        private async Task<TechnicalAdvisorUser> CreateTechnicalAdvisorUser(IdentityUser identityUser, int nationalSocietyId, CreateTechnicalAdvisorRequestDto createTechnicalAdvisorRequestDto)
        {
            var nationalSociety = await _dataContext.NationalSocieties.Include(ns => ns.ContentLanguage)
                .SingleOrDefaultAsync(ns => ns.Id == nationalSocietyId);

            if (nationalSociety == null)
            {
                throw new ResultException(ResultKey.User.Registration.NationalSocietyDoesNotExist);
            }

            var defaultUserApplicationLanguage = await _dataContext.ApplicationLanguages
                .SingleOrDefaultAsync(al => al.LanguageCode == nationalSociety.ContentLanguage.LanguageCode);

            var user = new TechnicalAdvisorUser
            {
                IdentityUserId = identityUser.Id,
                EmailAddress = identityUser.Email,
                Name = createTechnicalAdvisorRequestDto.Name,
                PhoneNumber = createTechnicalAdvisorRequestDto.PhoneNumber,
                AdditionalPhoneNumber = createTechnicalAdvisorRequestDto.AdditionalPhoneNumber,
                Organization = createTechnicalAdvisorRequestDto.Organization,
                ApplicationLanguage = defaultUserApplicationLanguage,
            };

            var userNationalSociety = CreateUserNationalSocietyReference(nationalSociety, user);

            await _dataContext.AddAsync(userNationalSociety);
            await _dataContext.SaveChangesAsync();
            return user;
        }

        private UserNationalSociety CreateUserNationalSocietyReference(Nyss.Data.Models.NationalSociety nationalSociety, Nyss.Data.Models.User user) =>
            new UserNationalSociety
            {
                NationalSociety = nationalSociety,
                User = user
            };

        public async Task<Result<GetTechnicalAdvisorResponseDto>> GetTechnicalAdvisor(int nationalSocietyUserId)
        {
            var technicalAdvisor = await _dataContext.Users
                .OfType<TechnicalAdvisorUser>()
                .Where(u => u.Id == nationalSocietyUserId)
                .Select(u => new GetTechnicalAdvisorResponseDto()
                {
                    Id = u.Id,
                    Name = u.Name,
                    Role = u.Role,
                    Email = u.EmailAddress,
                    PhoneNumber = u.PhoneNumber,
                    AdditionalPhoneNumber = u.AdditionalPhoneNumber,
                    Organization = u.Organization,
                })
                .SingleOrDefaultAsync();

            if (technicalAdvisor == null)
            {
                _loggerAdapter.Debug($"Technical advisor with id {nationalSocietyUserId} was not found");
                return Error<GetTechnicalAdvisorResponseDto>(ResultKey.User.Common.UserNotFound);
            }

            return new Result<GetTechnicalAdvisorResponseDto>(technicalAdvisor, true);
        }

        public async Task<Result> UpdateTechnicalAdvisor(int technicalAdvisorId, EditTechnicalAdvisorRequestDto editTechnicalAdvisorRequestDto)
        {
            try
            {
                var user = await _nationalSocietyUserService.GetNationalSocietyUser<TechnicalAdvisorUser>(technicalAdvisorId);

                user.Name = editTechnicalAdvisorRequestDto.Name;
                user.PhoneNumber = editTechnicalAdvisorRequestDto.PhoneNumber;
                user.Organization = editTechnicalAdvisorRequestDto.Organization;

                await _dataContext.SaveChangesAsync();
                return Success();
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        public Task<Result> DeleteTechnicalAdvisor(int technicalAdvisorId) =>
            _nationalSocietyUserService.DeleteUser<TechnicalAdvisorUser>(technicalAdvisorId);
    }
}

