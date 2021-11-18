using FluentValidation.TestHelper;
using NSubstitute;
using RX.Nyss.Web.Features.NationalSocieties.Dto;
using RX.Nyss.Web.Features.NationalSocieties.Validation;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.NationalSocieties
{
    public class CreateNationalSocietyValidatorTester
    {
        private CreateNationalSocietyRequestDto.Validator CreateValidator { get; set; }
        private EditNationalSocietyRequestDto.Validator EditValidator { get; set; }

        public CreateNationalSocietyValidatorTester()
        {
            var validationService = Substitute.For<INationalSocietyValidationService>();
            validationService.CountryExists(1).Returns(false);
            validationService.LanguageExists(1).Returns(false);
            validationService.NameExists("Test").Returns(true);
            validationService.NameExistsToOther("Test", 1).Returns(true);
            CreateValidator = new CreateNationalSocietyRequestDto.Validator(validationService);
            EditValidator = new EditNationalSocietyRequestDto.Validator(validationService);
        }

        [Fact]
        public void Create_WhenCountryDoesntExists_ShouldHaveError()
        {
            var model = new CreateNationalSocietyRequestDto { CountryId = 1};

            var result = CreateValidator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(ns => ns.CountryId);
        }

        [Fact]
        public void Create_WhenContentLanguageDoesntExists_ShouldHaveError()
        {
            var model = new CreateNationalSocietyRequestDto { ContentLanguageId = 1 };

            var result = CreateValidator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(ns => ns.ContentLanguageId);
        }

        [Fact]
        public void Create_WhenNameExists_ShouldHaveError()
        {
            var model = new CreateNationalSocietyRequestDto { Name = "Test" };

            var result = CreateValidator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(ns => ns.Name);
        }

        [Fact]
        public void Edit_WhenCountryDoesntExist_ShouldHaveError()
        {
            var model = new EditNationalSocietyRequestDto { CountryId = 1 };

            var result = EditValidator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(ns => ns.CountryId);
        }

        [Fact]
        public void Edit_WhenContentLanguageDoesntExist_ShouldHaveError()
        {
            var model = new EditNationalSocietyRequestDto { ContentLanguageId = 1 };

            var result = EditValidator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(ns => ns.ContentLanguageId);
        }

        [Fact]
        public void Edit_WhenNameExist_ShouldHaveError()
        {
            var model = new EditNationalSocietyRequestDto
            {
                Name = "Test",
                Id = 1
            };

            var result = EditValidator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }
    }
}