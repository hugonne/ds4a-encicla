using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ds4a.EnciclaWeb.DataAccess;
using Ds4a.EnciclaWeb.Models;
using Microsoft.AspNetCore.Http;
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
        public IEnumerable<Station> Get()
        {
            return _context.Stations.OrderBy(a => a.Name);
        }

        // GET: api/Stations/5
        [HttpGet("{id}", Name = "Get")]
        public Station Get(short id)
        {
            var station = _context.Stations
                    .Include(a => a.Zone)
                    //.Include(a => a.Weather)
                    .FirstOrDefault(a => a.StationId == id);
            return station;
        }

        [HttpGet("{id}/dailyCapacity", Name = "GetInventories")]
        //[Route("{id}/inventories")]
        public List<TwoDimensionalPlotViewModel<DateTime, int>> GetDailyCapacity(short id, DateTime date)
        {
            var inventories = _context.Inventories
                .Where(a =>
                    a.StationId == id &&
                    a.Date.HasValue &&
                    a.StationBikes.HasValue &&
                    a.Date.Value.Date == date.Date)
                .OrderBy(a => a.Date)
                .ToList();
            return new List<TwoDimensionalPlotViewModel<DateTime, int>>()
            {
                new TwoDimensionalPlotViewModel<DateTime, int>
                {
                    X = inventories.Select(a => a.Date.Value).ToList(),
                    Y = inventories.Select(a => a.StationBikes.Value).ToList(),
                    Type = "scatter"
                }
            };
        }
    }
}
