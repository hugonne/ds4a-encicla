using System;

namespace Ds4a.EnciclaWeb.Models.Domain
{
    public partial class Weather
    {
        public string WeatherId { get; set; }
        public short StationId { get; set; }
        public DateTime Date { get; set; }
        public string WeatherMain { get; set; }
        public string WeatherDescription { get; set; }
        public double? MainTempKelvin { get; set; }
        public double? MainFeelsLikeKelvin { get; set; }
        public double? MainTempMinKelvin { get; set; }
        public double? MainTempMaxKelvin { get; set; }
        public int? MainPressure { get; set; }
        public int? MainHumidity { get; set; }
        public int? Visibility { get; set; }
        public double? WindSpeed { get; set; }
        public int? WindDeg { get; set; }
        public int? CloudsAll { get; set; }

        public Station Station { get; set; }
    }
}
