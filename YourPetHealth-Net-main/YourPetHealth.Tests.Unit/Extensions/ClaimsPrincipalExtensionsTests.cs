using System.Security.Claims;
using YourPetHealth.Extensions;
using Xunit;

namespace YourPetHealth.Tests.Unit.Extensions
{
    public class ClaimsPrincipalExtensionsTests
    {
        [Fact]
        public void GetEntidadeId_ComClaimValido_RetornaOId()
        {
            var claims = new[] { new Claim("EntidadeId", "42") };
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

            var resultado = user.GetEntidadeId();

            Assert.Equal(42, resultado);
        }

        [Fact]
        public void GetEntidadeId_SemClaim_RetornaNull()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity());

            var resultado = user.GetEntidadeId();

            Assert.Null(resultado);
        }

        [Fact]
        public void GetEntidadeId_ComClaimNaoNumerico_RetornaNull()
        {
            var claims = new[] { new Claim("EntidadeId", "abc") };
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

            var resultado = user.GetEntidadeId();

            Assert.Null(resultado);
        }
    }
}
