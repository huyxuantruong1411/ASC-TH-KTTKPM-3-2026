using ASC.Tests.TestUtilities;
using ASC.Utilities;
using ASC.Web.Configuration;
using ASC.Web.Controllers;
using ASC.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ASC.Tests
{
    public class HomeControllerTests
    {
        private readonly Mock<IOptions<ApplicationSettings>> optionsMock;
        private readonly Mock<ILogger<HomeController>> loggerMock;

        public HomeControllerTests()
        {
            optionsMock = new Mock<IOptions<ApplicationSettings>>();
            optionsMock.Setup(ap => ap.Value).Returns(new ApplicationSettings
            {
                ApplicationTitle = "ASC"
            });

            loggerMock = new Mock<ILogger<HomeController>>();
        }

        [Fact]
        public void HomeController_Index_View_Test()
        {
            // Arrange
            var controller = new HomeController(loggerMock.Object, optionsMock.Object);

            // Khởi tạo HttpContext giả và gán FakeSession cho nó
            controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                Session = new FakeSession()
            };

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.Null(viewResult!.ViewData.Model);
            Assert.Empty(viewResult.ViewData.ModelState);

            // Xác minh xem Session đã thực sự được ghi vào chưa
            var sessionTest = controller.HttpContext.Session.GetSession<ApplicationSettings>("Test");
            Assert.NotNull(sessionTest);
        }

        [Fact]
        public void HomeController_Privacy_View_Test()
        {
            // Arrange
            var controller = new HomeController(loggerMock.Object, optionsMock.Object);

            // Act
            var result = controller.Privacy();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void HomeController_Error_View_Test()
        {
            // Arrange
            var controller = new HomeController(loggerMock.Object, optionsMock.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            // Act
            var result = controller.Error();

            // Assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.IsType<ErrorViewModel>(viewResult!.ViewData.Model);
        }
    }
}