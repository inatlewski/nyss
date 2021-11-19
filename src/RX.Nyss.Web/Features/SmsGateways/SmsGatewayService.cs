using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.SmsGateways.Dto;
using RX.Nyss.Web.Services;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.SmsGateways
{
    public interface ISmsGatewayService
    {
        Task<Result<GatewaySettingResponseDto>> Get(int smsGatewayId);
        Task<Result<List<GatewaySettingResponseDto>>> List(int nationalSocietyId);
        Task<Result<int>> Create(int nationalSocietyId, EditGatewaySettingRequestDto editGatewaySettingRequestDto);
        Task<Result> Edit(int smsGatewayId, EditGatewaySettingRequestDto editGatewaySettingRequestDto);
        Task<Result> Delete(int smsGatewayId);
        Task UpdateAuthorizedApiKeys();
        Task<Result> GetIotHubConnectionString(int smsGatewayId);
        Task<Result<IEnumerable<string>>> ListIotHubDevices();
    }

    public class SmsGatewayService : ISmsGatewayService
    {
        private readonly INyssContext _nyssContext;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly ISmsGatewayBlobProvider _smsGatewayBlobProvider;
        private readonly IIotHubService _iotHubService;

        public SmsGatewayService(
            INyssContext nyssContext,
            ILoggerAdapter loggerAdapter,
            ISmsGatewayBlobProvider smsGatewayBlobProvider, IIotHubService iotHubService)
        {
            _nyssContext = nyssContext;
            _loggerAdapter = loggerAdapter;
            _smsGatewayBlobProvider = smsGatewayBlobProvider;
            _iotHubService = iotHubService;
        }

        public async Task<Result<GatewaySettingResponseDto>> Get(int smsGatewayId)
        {
            // Oponeo: Get GatewaySettings
            // Select a new object of type GatewaySettingResponseDto with the following properties:
            //      Id, Name, ApiKey, GatewayType, EmailAddress, IotHubDeviceName
            //      ModemOneName - if Modems collection is not null and any element in Modems collection has ModemId == 1, then use Name property of such element
            //      ModemTwoName - if Modems collection is not null and any element in Modems collection has ModemId == 2, then use Name property of such element
            // Get first or default row base on Id equals smsGatewayId
            GatewaySettingResponseDto gatewaySetting = null;

            if (gatewaySetting == null)
            {
                return Error<GatewaySettingResponseDto>(ResultKey.NationalSociety.SmsGateway.SettingDoesNotExist);
            }

            var result = Success(gatewaySetting);

            return result;
        }

        public async Task<Result<List<GatewaySettingResponseDto>>> List(int nationalSocietyId)
        {
            // Oponeo: Get a list of GatewaySettings
            // Filter by nationalSocietyId
            // Order by Id
            // Select a new object of type GatewaySettingResponseDto with the following properties:
            //      Id, Name, ApiKey, GatewayType, IotHubDeviceName
            var gatewaySettings = new List<GatewaySettingResponseDto>();

            var result = Success(gatewaySettings);

            return result;
        }

        public async Task<Result<int>> Create(int nationalSocietyId, EditGatewaySettingRequestDto editGatewaySettingRequestDto)
        {
            try
            {
                // Oponeo: Make sure in the database there is NationalSociety with id from a parameter 
                if (true)
                {
                    return Error<int>(ResultKey.NationalSociety.SmsGateway.NationalSocietyDoesNotExist);
                }

                var gatewaySettingToAdd = new GatewaySetting
                {
                    Name = editGatewaySettingRequestDto.Name,
                    ApiKey = editGatewaySettingRequestDto.ApiKey,
                    GatewayType = editGatewaySettingRequestDto.GatewayType,
                    EmailAddress = editGatewaySettingRequestDto.EmailAddress,
                    NationalSocietyId = nationalSocietyId,
                    IotHubDeviceName = editGatewaySettingRequestDto.IotHubDeviceName
                };

                AttachGatewayModems(gatewaySettingToAdd, editGatewaySettingRequestDto);

                // Oponeo: add gatewaySettingToAdd to GatewaySettings table and save changes

                // Oponeo: fix UpdateAuthorizedApiKeys() method
                await UpdateAuthorizedApiKeys();

                return Success(gatewaySettingToAdd.Id, ResultKey.NationalSociety.SmsGateway.SuccessfullyAdded);
            }
            catch (ResultException exception)
            {
                _loggerAdapter.Debug(exception);
                return exception.GetResult<int>();
            }
        }

        public async Task<Result> Edit(int smsGatewayId, EditGatewaySettingRequestDto editGatewaySettingRequestDto)
        {
            try
            {
                // Oponeo: Get GatewaySetting to be updated base on id
                // Include information about Modems
                // Make sure database returned zero or one row
                GatewaySetting gatewaySettingToUpdate = null;

                if (gatewaySettingToUpdate == null)
                {
                    return Error(ResultKey.NationalSociety.SmsGateway.SettingDoesNotExist);
                }

                gatewaySettingToUpdate.Name = editGatewaySettingRequestDto.Name;
                gatewaySettingToUpdate.ApiKey = editGatewaySettingRequestDto.ApiKey;
                gatewaySettingToUpdate.GatewayType = editGatewaySettingRequestDto.GatewayType;
                gatewaySettingToUpdate.EmailAddress = editGatewaySettingRequestDto.EmailAddress;
                gatewaySettingToUpdate.IotHubDeviceName = editGatewaySettingRequestDto.IotHubDeviceName;

                // Oponeo: fix the methods inside EditGatewayModems()
                EditGatewayModems(gatewaySettingToUpdate, editGatewaySettingRequestDto);

                // Oponeo: save changes

                await UpdateAuthorizedApiKeys();

                return SuccessMessage(ResultKey.NationalSociety.SmsGateway.SuccessfullyUpdated);
            }
            catch (ResultException exception)
            {
                _loggerAdapter.Debug(exception);
                return exception.Result;
            }
        }

        public async Task<Result> Delete(int smsGatewayId)
        {
            try
            {
                // Oponeo: Get a GatewaySetting to be deleted base on id
                // Include information about Modems
                // Make sure database returned zero or one row
                GatewaySetting gatewaySettingToDelete = null;

                if (gatewaySettingToDelete == null)
                {
                    return Error(ResultKey.NationalSociety.SmsGateway.SettingDoesNotExist);
                }

                var modems = gatewaySettingToDelete.Modems.ToList();

                if (modems.Any())
                {
                    // Oponeo: fix the following methods
                    RemoveManagerModemReferences(modems);
                    RemoveSupervisorModemReferences(modems);
                    RemoveHeadSupervisorModemReferences(modems);
                    RemoveTechnicalAdvisorModemReferences(modems);
                    RemoveAlertRecipientModemsReferences(modems);
                }

                // Oponeo: remove gatewaySettingToDelete from GatewaySettings table and save changes

                await UpdateAuthorizedApiKeys();

                return SuccessMessage(ResultKey.NationalSociety.SmsGateway.SuccessfullyDeleted);
            }
            catch (ResultException exception)
            {
                _loggerAdapter.Debug(exception);
                return exception.Result;
            }
        }

        public async Task UpdateAuthorizedApiKeys()
        {
            // Oponeo: Get GatewaySettings
            // Ordered by NationalSocietyId and then Id
            // Select ApiKey
            // List the query
            var authorizedApiKeys = new List<string>();

            var blobContentToUpload = string.Join(Environment.NewLine, authorizedApiKeys);
            await _smsGatewayBlobProvider.UpdateApiKeys(blobContentToUpload);
        }

        public async Task<Result> GetIotHubConnectionString(int smsGatewayId)
        {
            // Oponeo: Find GatewaySettings by smsGatewayId
            GatewaySetting gatewayDevice = null;

            if (string.IsNullOrEmpty(gatewayDevice?.IotHubDeviceName))
            {
                return Error(ResultKey.NationalSociety.SmsGateway.SettingDoesNotExist);
            }

            var connectionString = await _iotHubService.GetConnectionString(gatewayDevice.IotHubDeviceName);

            return Success(connectionString);
        }

        public async Task<Result<IEnumerable<string>>> ListIotHubDevices()
        {
            var allDevices = await _iotHubService.ListDevices();

            // Oponeo: Get GatewaySettings
            // Filter out rows with IotHubDeviceName that is null or whitespace
            // Select IotHubDeviceName
            // List the query
            var takenDevices = new List<string>();

            var availableDevices = allDevices.Except(takenDevices);

            return Success(availableDevices);
        }

        private void AttachGatewayModems(GatewaySetting gatewaySetting, EditGatewaySettingRequestDto dto)
        {
            if (!string.IsNullOrEmpty(dto.ModemOneName) && !string.IsNullOrEmpty(dto.ModemTwoName))
            {
                gatewaySetting.Modems = new List<GatewayModem>
                {
                    new GatewayModem
                    {
                        ModemId = 1,
                        Name = dto.ModemOneName
                    },
                    new GatewayModem
                    {
                        ModemId = 2,
                        Name = dto.ModemTwoName
                    }
                };
            }
        }

        private void EditGatewayModems(GatewaySetting gatewaySetting, EditGatewaySettingRequestDto dto)
        {
            if (!string.IsNullOrEmpty(dto.ModemOneName) && !string.IsNullOrEmpty(dto.ModemTwoName))
            {
                if (!gatewaySetting.Modems.Any())
                {
                    AttachGatewayModems(gatewaySetting, dto);
                }
                else
                {
                    gatewaySetting.Modems.First(gm => gm.ModemId == 1).Name = dto.ModemOneName;
                    gatewaySetting.Modems.First(gm => gm.ModemId == 2).Name = dto.ModemTwoName;
                }
            }
            else
            {
                var modemsToRemove = gatewaySetting.Modems.ToList();

                if (modemsToRemove.Any())
                {
                    // Oponeo: fix the following methods:
                    RemoveAlertRecipientModemsReferences(modemsToRemove);
                    RemoveManagerModemReferences(modemsToRemove);
                    RemoveSupervisorModemReferences(modemsToRemove);
                    RemoveHeadSupervisorModemReferences(modemsToRemove);
                    RemoveTechnicalAdvisorModemReferences(modemsToRemove);

                    // Oponeo: remove modemsToRemove from GatewayModems table
                }
            }
        }

        private void RemoveManagerModemReferences(List<GatewayModem> gatewayModems)
        {
            // Oponeo: Get users
            // With Role.Manager
            // Select and cast to ManagerUser
            // Get only managers that have Modem from gatewayModems
            // List the query
            var managersConnectedToModems = new List<ManagerUser>();

            foreach (var manager in managersConnectedToModems)
            {
                manager.Modem = null;
            }
        }

        private void RemoveSupervisorModemReferences(List<GatewayModem> gatewayModems)
        {
            // Oponeo: Get users
            // With Role.Supervisor
            // Select and cast to SupervisorUser
            // Get only managers that have Modem from gatewayModems
            // List the query
            var supervisorsConnectedToModems = new List<SupervisorUser>();

            foreach (var supervisor in supervisorsConnectedToModems)
            {
                supervisor.Modem = null;
            }
        }

        private void RemoveHeadSupervisorModemReferences(List<GatewayModem> gatewayModems)
        {
            // Oponeo: Get users
            // With Role.HeadSupervisor
            // Select and cast to HeadSupervisorUser
            // Get only managers that have Modem from gatewayModems
            // List the query
            var headSupervisorsConnectedToModems = new List<HeadSupervisorUser>();

            foreach (var headSupervisor in headSupervisorsConnectedToModems)
            {
                // Oponeo: unlink (set to null) Modem from headSupervisor
            }
        }

        private void RemoveTechnicalAdvisorModemReferences(List<GatewayModem> gatewayModems)
        {
            // Get TechnicalAdvisorUserGatewayModems that contains gatewayModems
            var technicalAdvisorModemsToRemove = new List<TechnicalAdvisorUserGatewayModem>();

            // Oponeo: remove technicalAdvisorModemsToRemove from TechnicalAdvisorUserGatewayModems table
        }

        private void RemoveAlertRecipientModemsReferences(List<GatewayModem> gatewayModems)
        {
            // Oponeo: Get AlertNotificationRecipients that contains gatewayModems
            var alertRecipientsConnectedToModem = new List<AlertNotificationRecipient>();

            foreach (var alertRecipient in alertRecipientsConnectedToModem)
            {
                // Oponeo: unlink (set to null) GatewayModem from alertRecipient
            }
        }
    }
}
