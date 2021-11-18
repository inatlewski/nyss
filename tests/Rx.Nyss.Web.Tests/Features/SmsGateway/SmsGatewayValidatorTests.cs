using FluentValidation.TestHelper;
using NSubstitute;
using RX.Nyss.Web.Features.SmsGateways.Dto;
using RX.Nyss.Web.Features.SmsGateways.Validation;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.SmsGateway
{
    public class SmsGatewayValidatorTests
    {
        private readonly CreateGatewaySettingRequestDto.CreateGatewaySettingRequestValidator _createValidator;

        private readonly EditGatewaySettingRequestDto.GatewaySettingRequestValidator _editValidator;

        public SmsGatewayValidatorTests()
        {
            var validationService = Substitute.For<ISmsGatewayValidationService>();
            validationService.ApiKeyExists("1234").Returns(true);
            validationService.ApiKeyExistsToOther(1, "1234").Returns(true);
            _createValidator = new CreateGatewaySettingRequestDto.CreateGatewaySettingRequestValidator(validationService);
            _editValidator = new EditGatewaySettingRequestDto.GatewaySettingRequestValidator(validationService);
        }

        [Fact]
        public void Create_WhenApiExists_ShouldHaveError()
        {
            var model = new CreateGatewaySettingRequestDto
            {
                Id = 1,
                ApiKey = "1234"
            };

            var result = _createValidator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(gs => gs.ApiKey);
        }

        [Fact]
        public void Create_WhenEmailIsNullAndIotHubDeviceNameIsNull_ShouldHaveError()
        {
            var model = new CreateGatewaySettingRequestDto
            {
                Id = 1,
                IotHubDeviceName = null
            };

            var result = _createValidator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(gs => gs.IotHubDeviceName);
        }

        [Fact]
        public void Create_WhenIotHubDeviceNameIsSetAndEmailIsNull_ShouldNotHaveError()
        {
            var model = new CreateGatewaySettingRequestDto
            {
                Id = 1,
                IotHubDeviceName = "iothub"
            };

            var result = _createValidator.TestValidate(model);

            result.ShouldNotHaveValidationErrorFor(gs => gs.IotHubDeviceName);
        }

        [Fact]
        public void Create_WhenIotHubDeviceNameIsNullAndEmailIsSet_ShouldNotHaveError()
        {
            var model = new CreateGatewaySettingRequestDto
            {
                Id = 1,
                EmailAddress = "test@example.com"
            };

            var result = _createValidator.TestValidate(model);

            result.ShouldNotHaveValidationErrorFor(gs => gs.EmailAddress);
        }

        [Fact]
        public void Edit_WhenApiExistsToOther_ShouldHaveError()
        {
            var model = new EditGatewaySettingRequestDto
            {
                Id = 1,
                ApiKey = "1234"
            };

            var result = _editValidator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(gs => gs.ApiKey);
        }

        [Fact]
        public void Edit_WhenEmailIsNullAndIotHubDeviceNameIsNull_ShouldHaveError()
        {
            var model = new EditGatewaySettingRequestDto
            {
                Id = 1,
                IotHubDeviceName = null
            };

            var result = _editValidator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(gs => gs.IotHubDeviceName);
        }

        [Fact]
        public void Edit_WhenIotHubDeviceNameIsSetAndEmailIsNull_ShouldNotHaveError()
        {
            var model = new EditGatewaySettingRequestDto
            {
                Id = 1,
                IotHubDeviceName = "iothub"
            };

            var result = _editValidator.TestValidate(model);

            result.ShouldNotHaveValidationErrorFor(gs => gs.IotHubDeviceName);
        }

        [Fact]
        public void Edit_WhenIotHubDeviceNameIsNullAndEmailIsSet_ShouldNotHaveError()
        {
            var model = new EditGatewaySettingRequestDto
            {
                Id = 1,
                EmailAddress = "test@example.com"
            };

            var result = _editValidator.TestValidate(model);

            result.ShouldNotHaveValidationErrorFor(gs => gs.EmailAddress);
        }
    }
}
