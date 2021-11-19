using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Data.Queries;
using RX.Nyss.Web.Features.HealthRisks.Dto;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.HealthRisks
{
    public interface IHealthRiskService
    {
        Task<Result<IEnumerable<HealthRiskListItemResponseDto>>> List();
        Task<Result<HealthRiskResponseDto>> Get(int id);
        Task<Result> Create(HealthRiskRequestDto healthRiskRequestDto);
        Task<Result> Edit(int id, HealthRiskRequestDto healthRiskRequestDto);
        Task<Result> Delete(int id);
    }

    public class HealthRiskService : IHealthRiskService
    {
        private readonly INyssContext _nyssContext;
        private readonly IAuthorizationService _authorizationService;

        public HealthRiskService(INyssContext nyssContext, IAuthorizationService authorizationService)
        {
            _nyssContext = nyssContext;
            _authorizationService = authorizationService;
        }

        public async Task<Result<IEnumerable<HealthRiskListItemResponseDto>>> List()
        {
            var userName = _authorizationService.GetCurrentUserName();

            // Oponeo: Get a languageCode for current user.
            // Apply Users.FilterAvailable()
            // Filter by user's EmailAddress
            // Select LanguageCode available in ApplicationLanguage
            // Get single value with a fallback to "en"
            var languageCode = "";

            // Oponeo: Get HealthRisks
            // Select information needed to return HealthRiskListItemResponseDto
            // In the dto, return Name as:
            //     Health risk LanguageContents filtered by ContentLanguage.LanguageCode equals languageCode
            //     Select Name
            //     Get first value
            // Order by HealthRiskCode
            var healthRisks = new List<HealthRiskListItemResponseDto>();

            return Success<IEnumerable<HealthRiskListItemResponseDto>>(healthRisks);
        }

        public async Task<Result<HealthRiskResponseDto>> Get(int id)
        {
            // Oponeo: Get HealthRisks
            // Filter by id
            // Select a new object of type HealthRiskResponseDto with the following properties:
            //      Id, HealthRiskCode, HealthRiskType
            //      AlertRuleCountThreshold - healthRisk.AlertRule.CountThreshold if healthRisk.AlertRule is not null. Otherwise (int?)null.
            //      AlertRuleDaysThreshold - healthRisk.AlertRule.DaysThreshold if healthRisk.AlertRule is not null. Otherwise null.
            //      AlertRuleKilometersThreshold - healthRisk.AlertRule.KilometersThreshold if healthRisk.AlertRule is not null. Otherwise null.
            //      LanguageContent - from healthRisk.LanguageContents select a new object of type HealthRiskLanguageContentDto with the following properties:
            //          LanguageId - ContentLanguageId
            //          CaseDefinition, FeedbackMessage, Name
            // Make sure the database returned zero or one row
            HealthRiskResponseDto healthRiskResponse = null;

            if (healthRiskResponse == null)
            {
                return Error<HealthRiskResponseDto>(ResultKey.HealthRisk.HealthRiskNotFound);
            }

            return Success(healthRiskResponse);
        }

        public async Task<Result> Create(HealthRiskRequestDto healthRiskRequestDto)
        {
            // Oponeo: Make sure there is no HealthRisk with HealthRiskCode provided in the dto
            if (false)
            {
                return Error(ResultKey.HealthRisk.HealthRiskNumberAlreadyExists);
            }

            var languageContentIds = healthRiskRequestDto.LanguageContent.Select(lc => lc.LanguageId).ToArray();

            // Oponeo: Get ContentLanguages filtered by id available in languageContentIds (use Contains() method)
            // Convert to dictionary (use ToDictionaryAsync() method, key = Id, value = Content Language)
            var contentLanguages = new Dictionary<int, ContentLanguage>();

            var healthRisk = new HealthRisk
            {
                HealthRiskType = healthRiskRequestDto.HealthRiskType,
                HealthRiskCode = healthRiskRequestDto.HealthRiskCode,
                LanguageContents = healthRiskRequestDto.LanguageContent.Select(lc => new HealthRiskLanguageContent
                {
                    Name = lc.Name,
                    FeedbackMessage = lc.FeedbackMessage,
                    CaseDefinition = lc.CaseDefinition,
                    ContentLanguage = contentLanguages[lc.LanguageId]
                }).ToList(),
                AlertRule = healthRiskRequestDto.AlertRuleCountThreshold.HasValue
                    ? new AlertRule
                    {
                        CountThreshold = healthRiskRequestDto.AlertRuleCountThreshold.Value,
                        DaysThreshold = healthRiskRequestDto.AlertRuleDaysThreshold,
                        KilometersThreshold = healthRiskRequestDto.AlertRuleKilometersThreshold
                    }
                    : null
            };

            // Oponeo: Add healthRisk and save changes

            return SuccessMessage(ResultKey.HealthRisk.Create.CreationSuccess);
        }

        public async Task<Result> Edit(int id, HealthRiskRequestDto healthRiskRequestDto)
        {
            // Oponeo: find a health risk to edit base on id
            // Include AlertRule and LanguageContents and LanguageContents.ContentLanguage (use ThenInclude())
            // Make sure the database returned zero or one health risk
            HealthRisk healthRisk = null;

            if (healthRisk == null)
            {
                return Error(ResultKey.HealthRisk.HealthRiskNotFound);
            }

            // Oponeo: in the database, try to find a health risk with the same HealthRiskCode and with different id that provided as a parameter (use AnyAsync())
            if (false)
            {
                return Error(ResultKey.HealthRisk.HealthRiskNumberAlreadyExists);
            }

            // Oponeo: update values of HealthRiskCode and HealthRiskType base on information from DTO

            if (healthRiskRequestDto.AlertRuleCountThreshold.HasValue)
            {
                // Oponeo: Create a new object healthRisk.AlertRule
                // Set values of CountThreshold, DaysThreshold and KilometersThreshold
            }
            else
            {
                if (healthRisk.AlertRule != null)
                {
                    // Oponeo: Remove healthRisk.AlertRule from AlertRules table
                }

                healthRisk.AlertRule = null;
            }

            foreach (var languageContentDto in healthRiskRequestDto.LanguageContent)
            {
                var languageContent = healthRisk.LanguageContents.SingleOrDefault(lc => lc.ContentLanguage?.Id == languageContentDto.LanguageId)
                    ?? CreateNewLanguageContent(healthRisk, languageContentDto.LanguageId);

                // Oponeo: update FeedbackMessage, CaseDefinition, and Name of languageContent, base on information from DTO
            }

            // Oponeo: Save changes

            return SuccessMessage(ResultKey.HealthRisk.Edit.EditSuccess);
        }

        public async Task<Result> Delete(int id)
        {
            // Oponeo: find a health risk to be deleted base on id
            // Include AlertRule and LanguageContents
            // Make sure there is only one health risk with such id
            HealthRisk healthRisk = null;

            if (healthRisk == null)
            {
                return Error(ResultKey.HealthRisk.HealthRiskNotFound);
            }

            // Oponeo: fix HealthRiskContainsReports() method
            if (await HealthRiskContainsReports(id))
            {
                return Error(ResultKey.HealthRisk.HealthRiskContainsReports);
            }

            if (healthRisk.AlertRule != null)
            {
                // Oponeo: remove healthRisk.AlertRule from AlertRules table
            }

            // Oponeo: remove healthRisk from HealthRisks table and save changes

            return SuccessMessage(ResultKey.HealthRisk.Remove.RemoveSuccess);
        }

        private static bool CodeOrNameWasChanged(HealthRiskRequestDto healthRiskRequestDto, HealthRisk healthRisk) =>
            healthRiskRequestDto.HealthRiskCode != healthRisk.HealthRiskCode ||
            healthRiskRequestDto.LanguageContent.Any(lcDto =>
                healthRisk.LanguageContents.Any(lc =>
                    lc.ContentLanguage.Id == lcDto.LanguageId && !string.IsNullOrEmpty(lc.Name)) &&
                lcDto.Name != healthRisk.LanguageContents.Single(lc => lc.ContentLanguage.Id == lcDto.LanguageId).Name);

        private async Task<bool> HealthRiskContainsReports(int healthRiskId) =>
            // Oponeo: in the database, check if there is any ProjectHealthRisk with such healthRiskId and with any Report which is not a training report
            false;

        private HealthRiskLanguageContent CreateNewLanguageContent(HealthRisk healthRisk, int languageId)
        {
            var newLanguageContent = new HealthRiskLanguageContent
            {
                HealthRisk = healthRisk,
                ContentLanguageId = languageId
            };

            healthRisk.LanguageContents.Add(newLanguageContent);

            return newLanguageContent;
        }
    }
}
