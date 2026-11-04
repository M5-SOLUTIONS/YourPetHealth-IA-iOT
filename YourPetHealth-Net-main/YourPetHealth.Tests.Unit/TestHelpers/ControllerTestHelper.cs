using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace YourPetHealth.Tests.Unit.TestHelpers
{
    public static class ControllerTestHelper
    {
        public static void SetUser(Controller controller, string perfil, int? entidadeId)
        {
            var claims = new List<Claim>();
            if (!string.IsNullOrEmpty(perfil))
                claims.Add(new Claim(ClaimTypes.Role, perfil));
            if (entidadeId.HasValue)
                claims.Add(new Claim("EntidadeId", entidadeId.Value.ToString()));

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock
                .Setup(a => a.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);
            authServiceMock
                .Setup(a => a.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.IsAny<UrlActionContext>()))
                .Returns((UrlActionContext ctx) => $"/{ctx.Controller}/{ctx.Action}");

            var urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
            urlHelperFactoryMock
                .Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                .Returns(urlHelperMock.Object);

            var services = new ServiceCollection();
            services.AddSingleton(authServiceMock.Object);
            services.AddSingleton<IUrlHelperFactory>(urlHelperFactoryMock.Object);

            var httpContext = new DefaultHttpContext
            {
                User = principal,
                RequestServices = services.BuildServiceProvider()
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
                RouteData = new RouteData(),
                ActionDescriptor = new ControllerActionDescriptor
                {
                    ControllerName = controller.GetType().Name.Replace("Controller", ""),
                    ActionName = "Index"
                }
            };

            controller.Url = urlHelperMock.Object;

            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        }
    }
}
