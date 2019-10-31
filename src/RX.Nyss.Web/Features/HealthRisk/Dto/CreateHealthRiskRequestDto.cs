using System.Collections.Generic;
using FluentValidation;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.HealthRisk
{
    public class CreateHealthRiskRequestDto
    {
        public int HealthRiskCode { get; set; }

        public string Name { get; set; }

        public HealthRiskType HealthRiskType { get; set; }

        public int AlertRuleCountThreshold { get; set; }

        public int AlertRuleHoursThreshold { get; set; }

        public int AlertRuleMetersThreshold { get; set; }
        public List<HealthRiskDescriptions> Descriptions { get; set; }

        public class Validator : AbstractValidator<CreateHealthRiskRequestDto>
        {
            public Validator()
            {
                RuleFor(hr => hr.Name).NotEmpty().MaximumLength(100);
                RuleFor(hr => hr.HealthRiskCode).GreaterThan(0);
                RuleFor(hr => hr.HealthRiskType).IsInEnum();
                RuleFor(hr => hr.Descriptions).NotEmpty();
                RuleForEach(hr => hr.Descriptions).SetValidator(new HealthRiskDescriptions.Validator());
            }
        }
    }
}
