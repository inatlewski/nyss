namespace RX.Nyss.Web.Features.DataCollector.Dto
{
    public class ProjectBasicDataResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public NationalSocietyIdDto NationalSociety { get; set; }

        public class NationalSocietyIdDto
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string CountryName { get; set; }
        }
    }
}
