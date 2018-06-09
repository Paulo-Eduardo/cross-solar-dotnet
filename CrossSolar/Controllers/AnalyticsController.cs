using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrossSolar.Domain;
using CrossSolar.Models;
using CrossSolar.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrossSolar.Controllers
{
    [Route("panel")]
    public class AnalyticsController : Controller
    {
        private readonly IAnalyticsRepository _analyticsRepository;

        private readonly IPanelRepository _panelRepository;

        public AnalyticsController(IAnalyticsRepository analyticsRepository, IPanelRepository panelRepository)
        {
            _analyticsRepository = analyticsRepository;
            _panelRepository = panelRepository;
        }

        // GET panel/XXXX1111YYYY2222/analytics
        [HttpGet("{serial}/[controller]")]
        public async Task<IActionResult> Get([FromRoute] string serial)
        {
            var analytics = _analyticsRepository.Query()
                .Where(x => x.PanelId.Equals(serial)).ToList();

            var result = new OneHourElectricityListModel
            {
                OneHourElectricitys = analytics.Select(c => new OneHourElectricityModel
                {
                    Id = c.Id,
                    KiloWatt = c.KiloWatt,
                    DateTime = c.DateTime
                })
            };

            return Ok(result);
        }

        // GET panel/XXXX1111YYYY2222/analytics/day
        [HttpGet("{serial}/[controller]/day")]
        public async Task<IActionResult> DayResults([FromRoute] string serial)
        {
            var result = new List<OneDayElectricityModel>();

            var analytics = _analyticsRepository.Query()
                .Where(x => x.PanelId.Equals(serial)).OrderBy(x => x.DateTime).ToList();

            if (analytics.Count() < 0)
                return BadRequest();

            var date = analytics.Min(x => x.DateTime);

            var analyticsForDay = analytics.Where(x => x.DateTime.DayOfYear == date.DayOfYear).ToList();

            while(analyticsForDay.Count() > 0)
            {
                if (date.DayOfYear == DateTime.Now.DayOfYear)
                    break;
                result.Add(new OneDayElectricityModel()
                {
                    Average = analyticsForDay.Average(x => x.KiloWatt),
                    Maximum = analyticsForDay.Max(x => x.KiloWatt),
                    Minimum = analyticsForDay.Min(x => x.KiloWatt),
                    Sum = analytics.Sum(x => x.KiloWatt),
                    DateTime = date
                });
                date = date.AddDays(1);
                analyticsForDay = analytics.Where(x => x.DateTime.DayOfYear == date.DayOfYear).ToList();
            }

            return Ok(result);
        }

        // POST panel/XXXX1111YYYY2222/analytics
        [HttpPost("{panelId}/[controller]")]
        public async Task<IActionResult> Post([FromRoute] string panelId, [FromBody] OneHourElectricityModel value)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var oneHourElectricityContent = new OneHourElectricity
            {
                PanelId = panelId,
                KiloWatt = value.KiloWatt,
                DateTime = DateTime.UtcNow
            };

            await _analyticsRepository.InsertAsync(oneHourElectricityContent);

            var result = new OneHourElectricityModel
            {
                Id = oneHourElectricityContent.Id,
                KiloWatt = oneHourElectricityContent.KiloWatt,
                DateTime = oneHourElectricityContent.DateTime
            };

            return Created($"panel/{panelId}/analytics/{result.Id}", result);
        }
    }
}