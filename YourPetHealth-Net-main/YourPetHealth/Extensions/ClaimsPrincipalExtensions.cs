using System.Security.Claims;

namespace YourPetHealth.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int? GetEntidadeId(this ClaimsPrincipal user)
        {
            var valor = user.FindFirst("EntidadeId")?.Value;
            return int.TryParse(valor, out var id) ? id : null;
        }
    }
}
