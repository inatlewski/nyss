using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.NationalSocieties.Access;
using RX.Nyss.Web.Features.NationalSocieties.Dto;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.NationalSocieties
{
    public interface INationalSocietyService
    {
        Task<Result<List<NationalSocietyListResponseDto>>> List();

        Task<Result<NationalSocietyResponseDto>> Get(int id);

        Task<Result> Create(CreateNationalSocietyRequestDto nationalSociety);

        Task<Result> Edit(int nationalSocietyId, EditNationalSocietyRequestDto nationalSociety);

        Task<IEnumerable<HealthRiskDto>> GetHealthRiskNames(int nationalSocietyId, bool excludeActivity);

        Task<Result> Reopen(int nationalSocietyId);
    }

    public class NationalSocietyService : INationalSocietyService
    {
        private readonly INyssContext _nyssContext;

        private readonly INationalSocietyAccessService _nationalSocietyAccessService;

        private readonly ILoggerAdapter _loggerAdapter;

        private readonly IAuthorizationService _authorizationService;

        public NationalSocietyService(
            INyssContext context,
            INationalSocietyAccessService nationalSocietyAccessService,
            ILoggerAdapter loggerAdapter,
            IAuthorizationService authorizationService)
        {
            _nyssContext = context;
            _nationalSocietyAccessService = nationalSocietyAccessService;
            _loggerAdapter = loggerAdapter;
            _authorizationService = authorizationService;
        }

        public async Task<Result<List<NationalSocietyListResponseDto>>> List()
        {
            // Oponeo: fix GetNationalSocietiesQuery()
            // Include information about HeadManager in DefaultOrganization of national society
            // Include information about PendingHeadManager in DefaultOrganization of national society
            // Select a new object of type NationalSocietyListResponseDto with the following properties:
            //      Id
            //      ContentLanguage - DisplayName of ContentLanguage
            //      Name
            //      Country - Name of Country
            //      StartDate
            //      IsArchived
            //      HeadManagers - string that contains Names of HeadManagers available in Organizations (if HeadManager is not null), separated by ","
            //      TechnicalAdvisor - string that contains Names of Users with Role TechnicalAdvisor available in NationalSocietyUsers, separated by ","
            //      Coordinator - string that contains Names of Users with Role Coordinator available in NationalSocietyUsers, separated by ","
            // Order everything by Name of the national society
            // List the query
            var list = new List<NationalSocietyListResponseDto>();

            return Success(list);
        }

        public async Task<Result<NationalSocietyResponseDto>> Get(int id)
        {
            var currentUserName = _authorizationService.GetCurrentUserName();

            // Oponeo: Get NationalSociety
            // Select a new object of type NationalSocietyResponseDto wit the following properties:
            //      Id
            //      ContentLanguageId
            //      ContentLanguageName - DisplayName of ContentLanguage
            //      Name
            //      CountryId
            //      CountryName
            //      IsCurrentUserHeadManager - a flag that indicates if in the Organizations of the national society there is any with HeadManager with EmailAddress equals currentUserName
            //      IsArchived
            //      HasCoordinator - a flag that indicates if in the NationalSocietyUsers there is any user with Role Coordinator
            // Return first row or null base on id
            NationalSocietyResponseDto nationalSociety = null;

            return nationalSociety != null
                ? Success(nationalSociety)
                : Error<NationalSocietyResponseDto>(ResultKey.NationalSociety.NotFound);
        }

        public async Task<Result> Create(CreateNationalSocietyRequestDto dto)
        {
            // Oponeo: fix GetLanguageById() and GetCountryById() methods
            var nationalSociety = new NationalSociety
            {
                Name = dto.Name,
                ContentLanguage = await GetLanguageById(dto.ContentLanguageId),
                Country = await GetCountryById(dto.CountryId),
                IsArchived = false,
                StartDate = DateTime.UtcNow
            };

            // Oponeo: Add nationalSociety and save changes

            // Oponeo: Set DefaultOrganization of national society as a new object of type Organization with the following parameters:
            //      dto.InitialOrganizationName as Name
            //      nationalSociety as NationalSociety
            // Save changes

            _loggerAdapter.Info($"A national society {nationalSociety} was created");
            return Success(nationalSociety.Id);
        }

        public async Task<Result> Edit(int nationalSocietyId, EditNationalSocietyRequestDto dto)
        {
            var currentUser = await _authorizationService.GetCurrentUser();

            // Oponeo: Get NationalSocieties base on nationalSocietyId
            // Select anonymous object with the following properties:
            //      NationalSociety,
            //      CurrentUserOrganizationId - NationalSocietyUsers where User is currentUser and select zero or one OrganizationId
            //      HasCoordinator - a flag that indicates if NationalSocietyUsers collection contains any User with Role Role.Coordinator
            // Make sure database returned exactly one row
            var nationalSocietyData = new
                {
                    NationalSociety = (NationalSociety)null,
                    CurrentUserOrganizationId = 0,
                    HasCoordinator = false
                };

            var nationalSociety = nationalSocietyData.NationalSociety;

            if (nationalSociety.IsArchived)
            {
                return Error(ResultKey.NationalSociety.Edit.CannotEditArchivedNationalSociety);
            }

            if (nationalSocietyData.HasCoordinator && !_authorizationService.IsCurrentUserInAnyRole(Role.Administrator, Role.Coordinator))
            {
                return Error(ResultKey.UnexpectedError);
            }

            nationalSociety.Name = dto.Name;
            // Oponeo: fix GetLanguageById() and GetCountryById() methods
            nationalSociety.ContentLanguage = await GetLanguageById(dto.ContentLanguageId);
            nationalSociety.Country = await GetCountryById(dto.CountryId);

            // Oponeo: save changes

            return SuccessMessage(ResultKey.NationalSociety.Edit.Success);
        }

        public async Task<IEnumerable<HealthRiskDto>> GetHealthRiskNames(int nationalSocietyId, bool excludeActivity) =>
            // Oponeo: Get ProjectHealthRisks
            // By nationalSocietyId of the Project
            // Apply a filter that checks if excludeActivity is false or HealthRiskType of ProjectHealthRisks is HealthRiskType.Activity
            // Select a new object of type HealthRiskDto with the following properties:
            //      Id set to HealthRiskId
            //      Name set to:
            //          From the HealthRisk's LanguageContents where ContentLanguage.Id equals Project.NationalSociety.ContentLanguage.Id
            //          Select Name
            //          Get the first row or null
            // Remove duplicated rows (use Distinct() method)
            // Order by Name
            // List the query
            new List<HealthRiskDto>();

        public async Task<Result> Reopen(int nationalSocietyId)
        {
            // Oponeo: Find NationalSocieties by id
            NationalSociety nationalSociety = null;

            if (nationalSociety == null)
            {
                return Error(ResultKey.NationalSociety.NotFound);
            }

            // Oponeo: Set IsArchived flag to false and save changes

            return SuccessMessage(ResultKey.NationalSociety.Archive.ReopenSuccess);
        }

        public async Task<ContentLanguage> GetLanguageById(int id) =>
            // Oponeo: Find ContentLanguages by id
            null;

        public async Task<Country> GetCountryById(int id) =>
            // Oponeo: Find Country by id
            null;

        private IQueryable<NationalSociety> GetNationalSocietiesQuery()
        {
            if (_nationalSocietyAccessService.HasCurrentUserAccessToAllNationalSocieties())
            {
                return _nyssContext.NationalSocieties;
            }

            var userName = _authorizationService.GetCurrentUserName();

            // Oponeo: return NationalSocieties that have any NationalSocietyUser with User EmailAddress equal to userName
            return Enumerable.Empty<NationalSociety>().AsQueryable();
        }
    }
}
