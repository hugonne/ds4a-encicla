using System;
using Ds4a.EnciclaWeb.Models.Domain;

namespace Ds4a.EnciclaWeb.Models
{
    public class StationDetails
    {
        public Station Station { get; set; }
        public string DailyCapacityJson { get; set; }
        public string DailyPredictionsJson { get; set; }
        public string HourlyAverage { get; set; }
    }
}
