using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrossSolar.Controllers;
using CrossSolar.Domain;
using CrossSolar.Models;
using CrossSolar.Repository;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CrossSolar.Tests.Controller
{
    public class AnalyticsControllerTests
    {
        public AnalyticsControllerTests()
        {
            _analyticsController = new AnalyticsController(_analyticsRepository.Object, _panelRepository.Object);
        }

        private readonly AnalyticsController _analyticsController;

        private readonly Mock<IAnalyticsRepository> _analyticsRepository = new Mock<IAnalyticsRepository>();
        private readonly Mock<IPanelRepository> _panelRepository = new Mock<IPanelRepository>();

        [Fact]
        public async Task Get_ReturnsOKResult()
        {
            // Arrange
            var panel = new Panel
            {
                Id = 1,
                Brand = "Areva",
                Latitude = 12.345678,
                Longitude = 98.7655432,
                Serial = "AAAA1111BBBB2222"
            };

            _panelRepository.Setup(x => x.Query()).Returns(new Panel[1] { panel }.AsQueryable());

            // Act
            var result = await _analyticsController.Get(panel.Serial);

            // Assert
            Assert.NotNull(result);

            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task DayResults_ReturnsOK()
        {
            // Arrange
            var panel = new Panel
            {
                Id = 1,
                Brand = "Areva",
                Latitude = 12.345678,
                Longitude = 98.7655432,
                Serial = "AAAA1111BBBB2222"
            };

            OneHourElectricity oneHourElectricity = new OneHourElectricity()
            {
                Id = 1,
                DateTime = DateTime.Now.AddDays(-1),
                KiloWatt = 200,
                PanelId = "AAAA1111BBBB2222"
            };
            _analyticsRepository.Setup(x => x.Query()).Returns(new OneHourElectricity[1] { oneHourElectricity }.AsQueryable());

            // Act
            var result = await _analyticsController.DayResults(panel.Serial);

            // Assert
            Assert.NotNull(result);

            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult.Value);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task Post_CallInsert()
        {
            // Arrange
            OneHourElectricityModel oneHourElectricityModel = new OneHourElectricityModel()
            {
                DateTime = DateTime.Now,
                KiloWatt = 100
            };

            // Act
            var result = await _analyticsController.Post("AAAA1111BBBB2222", oneHourElectricityModel);

            // Assert
            _analyticsRepository.Verify(x => x.InsertAsync(It.IsAny<OneHourElectricity>()), Times.AtLeastOnce());
        }
    }
}
