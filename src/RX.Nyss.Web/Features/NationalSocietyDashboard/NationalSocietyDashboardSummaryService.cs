using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.Common.Extensions;
using RX.Nyss.Web.Features.NationalSocietyDashboard.Dto;
using RX.Nyss.Web.Features.Reports;
using RX.Nyss.Web.Services.ReportsDashboard;

namespace RX.Nyss.Web.Features.NationalSocietyDashboard
{
    public interface INationalSocietyDashboardSummaryService
    {
        Task<NationalSocietySummaryResponseDto> GetData(ReportsFilter filters);
    }

    public class NationalSocietyDashboardSummaryService : INationalSocietyDashboardSummaryService
    {
        private readonly IReportService _reportService;

        private readonly INyssContext _nyssContext;

        private readonly IReportsDashboardSummaryService _reportsDashboardSummaryService;

        public NationalSocietyDashboardSummaryService(
            IReportService reportService,
            INyssContext nyssContext,
            IReportsDashboardSummaryService reportsDashboardSummaryService)
        {
            _reportService = reportService;
            _nyssContext = nyssContext;
            _reportsDashboardSummaryService = reportsDashboardSummaryService;
        }

        public async Task<NationalSocietySummaryResponseDto> GetData(ReportsFilter filters)
        {
            if (!filters.NationalSocietyId.HasValue)
            {
                throw new InvalidOperationException("NationalSocietyId was not supplied");
            }

            var nationalSocietyId = filters.NationalSocietyId.Value;

            var dashboardReports = _reportService.GetDashboardHealthRiskEventReportsQuery(filters);
            var rawReportsWithDataCollector = _reportService.GetRawReportsWithDataCollectorQuery(filters);

            // Oponeo: Get NationalSociety by id
            // Select a new anonymous object with property activeDataCollectorCount that counts unique Id of DataCollectors using a query rawReportsWithDataCollector
            // Then select a new object of type NationalSocietySummaryResponseDto with the following properties:
            //      KeptReportCount - sum of ReportedCaseCount with report status Accepted using a query dashboardReports
            //      DismissedReportCount - sum of ReportedCaseCount with report status Rejected using a query dashboardReports
            //      NotCrossCheckedReportCount - sum of ReportedCaseCount with report status New, Pending or Closed using a query dashboardReports
            //      TotalReportCount - sum of all ReportedCaseCount using a query dashboardReports
            //      ActiveDataCollectorCount
            //      DataCollectionPointSummary - use a query provided by _reportsDashboardSummaryService.DataCollectionPointsSummary with a parameter dashboardReports
            //      AlertsSummary - use a query provided by _reportsDashboardSummaryService.AlertsSummary with a parameter filters
            //      NumberOfDistricts - count a unique Village District using a query rawReportsWithDataCollector
            //      NumberOfVillages - count a unique Village using a query rawReportsWithDataCollector
            // Return first row from the database or null
            return null;
        }
    }
}
