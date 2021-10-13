using System;
using RX.Nyss.Common.Utils;

namespace RX.Nyss.Web.Features.DataCollectors.DataContracts
{
    public class RawReportDataForExport
    {
        public bool IsValid { get; set; }
        public EpiDate EpiDate { get; set; }
        public DateTime ReceivedAt { get; set; }
    }
}
