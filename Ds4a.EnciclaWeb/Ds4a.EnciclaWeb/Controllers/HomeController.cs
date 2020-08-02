using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ds4a.EnciclaWeb.Models;
using System.Net.Http;
using Ds4a.EnciclaWeb.Models.Domain;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ds4a.EnciclaWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string _baseUrl;

        public HomeController(ILogger<HomeController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            var http = httpContextAccessor.HttpContext.Request.IsHttps ? "https" : "http";
            _baseUrl = $"{http}://{httpContextAccessor.HttpContext.Request.Host.Value}";
        }

        /// <summary>
        /// Home. Default landing page.
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Map View. Shows a map with all stations and their status.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Map()
        {
            var client = new HttpClient();
            string response = await client.GetStringAsync($"{_baseUrl}/api/stations?includeAvailability=true");
            var stations = JsonConvert.DeserializeObject<List<Station>>(response);

            return View(stations);
        }

        /// <summary>
        /// Stations View. Shows a list of station for the user to see its details and
        /// availability predictions.
        /// </summary>
        /// <param name="id">
        /// Optional parameter. If present in route, site renders details for that station.
        /// If not, site renders a list for the user to select a Station.
        /// </param>
        /// <param name="date">Optional parameter. If not sent, data is queried for the current day.</param>
        /// <returns></returns>
        public async Task<IActionResult> Stations(short? id, DateTime? date)
        {
            try
            {
                var client = new HttpClient();
                string response;
                var stationDetails = new StationDetails();

                //If no id is provided, get stations list and don't return a model to the view.
                if (!id.HasValue)
                {
                    response = await client.GetStringAsync($"{_baseUrl}/api/stations/forList");
                    var stationList = JsonConvert.DeserializeObject<List<StationList>>(response);
                    ViewData["StationList"] = new SelectList(stationList, "StationId", "Name");
                    return View();
                }

                //If no date is provided, send today by default.
                date ??= DateTime.Today;

                #region Station Info

                response = await client.GetStringAsync($"{_baseUrl}/api/stations/{id}");
                var station = JsonConvert.DeserializeObject<Station>(response);
                if (station == null || station.StationId == 0)
                {
                    return NotFound();
                }
                stationDetails.Station = station;

                #endregion

                #region Line Chart JSON data

                //This responses don't need to be deserialized to an object, because raw JSON
                //is required by Plotly for the chart.
                response = await client.GetStringAsync($"{_baseUrl}/api/stations/{id}/dailyCapacity?date={date}");
                stationDetails.DailyCapacityJson = response;
                
                response = await client.GetStringAsync($"{_baseUrl}/api/stations/{id}/dailyPredictions?date={date}");
                stationDetails.DailyPredictionsJson = response;

                #endregion

                #region Heat Map JSON Data

                //This responses don't need to be deserialized to an object, because raw JSON
                //is required by Plotly for the chart.
                response = await client.GetStringAsync($"{_baseUrl}/api/stations/{id}/hourlyAverage");
                stationDetails.HourlyAverage = response;

                #endregion


                return View(stationDetails);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex, "GetStations");
                return View("Error");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
