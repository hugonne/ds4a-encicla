using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ds4a.EnciclaWeb.Models;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ds4a.EnciclaWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
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
        public IActionResult Map()
        {
            return View();
        }

        /// <summary>
        /// Stations View. Shows a list of station for the user to see its details and
        /// availability predictions.
        /// </summary>
        /// <param name="id">
        /// Optional parameter. If present in route, site renders details for that station.
        /// If not, site renders a list for the user to select a Station.
        /// </param>
        /// <returns></returns>
        public async Task<IActionResult> Stations(short? id, DateTime? date)
        {
            try
            {
                var client = new HttpClient();
                string response;

                //If no id is provided, get stations list and don't return a model to the view.
                if (!id.HasValue)
                {
                    response = await client.GetStringAsync($"https://localhost:5001/api/stations");
                    var stationList = JsonConvert.DeserializeObject<List<StationListViewModel>>(response);
                    ViewData["StationList"] = new SelectList(stationList, "StationId", "Name");
                    return View();
                }

                //If no date is provided, send today by default.
                if (date == null)
                {
                    date = DateTime.Today;
                }

                response = await client.GetStringAsync($"https://localhost:5001/api/stations/{id}");
                var station = JsonConvert.DeserializeObject<Station>(response);

                if (station == null || station.StationId == 0)
                {
                    return NotFound();
                }

                //This response doesn't need to be deserialized to an object, because raw JSON
                //is required by Plotly for the chart.
                response = await client.GetStringAsync($"https://localhost:5001/api/stations/{id}/dailyCapacity?date={date}");

                var stationDetails = new StationDetailsViewModel
                {
                    Station = station,
                    DailyCapacityJson = response
                };

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
