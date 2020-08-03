using System;
using System.Collections.Generic;
using System.Linq;
using Ds4a.EnciclaWeb.DataAccess;
using Ds4a.EnciclaWeb.Models;
using Ds4a.EnciclaWeb.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ds4a.EnciclaWeb.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class StationsController : ControllerBase
    {
        private readonly EnciclaDbContext _context;

        public StationsController(EnciclaDbContext context)
        {
            _context = context;
        }

        // GET: api/Stations
        [HttpGet]
        public IEnumerable<Station> Get(bool includeAvailability = false)
        {
            var stations = _context.Stations.Include(a => a.Zone).ToList();
            if (!includeAvailability)
            {
                return stations;
            }

            #region Current Availability

            var maxInventoryDate = _context.Inventories.Max(a => a.Date);
            var inventories = _context.Inventories
                .Where(a => 
                    a.StationId.HasValue &&
                    stations.Select(b => b.StationId).Contains(a.StationId.Value))
                .Include(a => a.Station)
                .ThenInclude(a => a.Zone)
                .Where(a => a.Date == maxInventoryDate);
            foreach (var inventory in inventories)
            {
                stations.First(a => a.StationId == inventory.Station.StationId)
                        .CurrentBikes = inventory.StationBikes ?? 0;
            }

            #endregion

            #region Predicted Availability

            var maxPredictionDate = _context.Predictions.Max(a => a.PredictDate);
            var predictions = _context.Predictions
                .Include(a => a.Inventory)
                .ThenInclude(a => a.Station)
                .ThenInclude(a => a.Zone)
                .Where(a => 
                    a.PredictDate == maxPredictionDate &&
                    a.Inventory.StationId.HasValue &&
                    stations.Select(b => b.StationId).Contains(a.Inventory.StationId.Value));
            foreach (var prediction in predictions)
            {
                stations.First(a => a.StationId == prediction.Inventory.Station.StationId)
                        .PredictedBikes = prediction.PredictBikes ?? -1;
            }

            #endregion

            return stations;
        }

        // GET: api/Stations
        [HttpGet("forList", Name = "GetForList")]
        public IEnumerable<StationList> GetForList()
        {
            return _context.Stations
                .Select(a => new StationList {StationId = a.StationId, Name = a.Name})
                .OrderBy(a => a.Name);
        }

        // GET: api/Stations/5
        [HttpGet("{id}", Name = "Get")]
        public Station Get(short id)
        {
            var station = _context.Stations
                .Include(a => a.Zone)
                .FirstOrDefault(a => a.StationId == id);

            if (station == null)
            {
                return null;
            }

            #region Current Availability

            var maxInventoryDate = _context.Inventories.Max(a => a.Date);
            var inventory = _context.Inventories
                .FirstOrDefault(a => a.StationId == id && a.Date == maxInventoryDate);
            station.CurrentBikes = inventory?.StationBikes ?? 0;

            #endregion

            #region Predicted Availability

            var maxPredictionDate = _context.Predictions.Include(a => a.Inventory).Max(a => a.PredictDate);
            var prediction = _context.Predictions
                .FirstOrDefault(a => a.Inventory.StationId == id && a.PredictDate == maxPredictionDate);
            //A value of -1 indicates that predictions for that time started, but the station hasn't been processed yet
            station.PredictedBikes = prediction?.PredictBikes ?? -1;

            #endregion
            
            return station;
        }

        [HttpGet("{id}/dailyCapacity", Name = "GetDailyCapacity")]
        //[Route("{id}/inventories")]
        public TwoDimensionalPlot<DateTime, int> GetDailyCapacity(short id, DateTime date)
        {
            var inventories = _context.Inventories
                .Where(a =>
                    a.StationId == id &&
                    a.Date.HasValue &&
                    a.StationBikes.HasValue &&
                    a.Date.Value.Date == date.Date)
                .OrderBy(a => a.Date)
                .ToList();
            return new TwoDimensionalPlot<DateTime, int>
            {
                // ReSharper disable once PossibleInvalidOperationException
                X = inventories.Select(a => a.Date.Value).ToList(),
                // ReSharper disable once PossibleInvalidOperationException
                Y = inventories.Select(a => a.StationBikes.Value).ToList(),
                Type = "scatter",
                Name = "Actual",
                Mode = "lines"
            };
        }

        [HttpGet("{id}/dailyPredictions", Name = "GetDailyPredictions")]
        //[Route("{id}/inventories")]
        public TwoDimensionalPlot<DateTime, int> GetDailyPredictions(short id, DateTime date)
        {
            var inventories = _context.Predictions
                .Include(a => a.Inventory)
                .Where(a =>
                    a.Inventory.StationId == id &&
                    a.PredictBikes.HasValue &&
                    a.PredictDate.Date == date.Date)
                .OrderBy(a => a.PredictDate)
                .ToList();
            return new TwoDimensionalPlot<DateTime, int>
            {
                X = inventories.Select(a => a.PredictDate).ToList(),
                // ReSharper disable once PossibleInvalidOperationException
                Y = inventories.Select(a => a.PredictBikes.Value).ToList(),
                Type = "scatter",
                Name = "Prediction",
                Mode = "lines+markers"
            };
        }

        [HttpGet("{id}/hourlyAverage", Name = "GetHourlyAverage")]
        public HeatMapPlot GetHourlyAverage(short id)
        {
            var hours = _context.AvgInvByStationHours.Where(a => a.StationId == id);

            var plotData = new HeatMapPlot
            {
                Y = hours.Select(a => a.Hour).OrderBy(a => a).ToList(),
                X = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" },
                Z = new List<List<double>>(),
                Type = "heatmap",
                Hoverongaps = false
            };
            foreach (var hour in hours)
            {
                plotData.Z.Add(new List<double>
                {
                    hour.Monday,
                    hour.Tuesday,
                    hour.Wednesday,
                    hour.Thursday,
                    hour.Friday,
                    hour.Saturday,
                    hour.Sunday
                });
            }
            return plotData;
        }

        [HttpGet("stationsWithoutBikesNextHour", Name = "GetStationsWithoutBikesNextHour")]
        public IEnumerable<Station> GetStationsWithoutBikesNextHour()
        {
            //Get from hourly predictions, those stations with prediction value in 0 in last hour,
            //which is always the max predicted date.
            var maxPredictionDate = _context.PredictionsOneHour.Max(a => a.PredictDate);
            var predictions = _context.PredictionsOneHour
                .Include(a => a.Inventory)
                .ThenInclude(a => a.Station)
                .ThenInclude(a => a.Zone)
                .Where(a => a.PredictDate == maxPredictionDate && a.PredictBikes == 0);

            return predictions.Select(a => a.Inventory.Station).OrderBy(a => a.Name);
        }
    }
}