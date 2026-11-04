using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;

namespace YourPetHealth.Tests.Integration
{
    public static class AntiForgeryHelper
    {
        private static readonly Regex TokenRegex = new(
            "<input[^>]*name=\"__RequestVerificationToken\"[^>]*>",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex ValueRegex = new(
            "value=\"([^\"]+)\"",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static string ExtractToken(string html)
        {
            var inputMatch = TokenRegex.Match(html);
            if (!inputMatch.Success)
                throw new InvalidOperationException("Token anti-forgery não encontrado na página (input __RequestVerificationToken ausente).");

            var valueMatch = ValueRegex.Match(inputMatch.Value);
            if (!valueMatch.Success)
                throw new InvalidOperationException("Token anti-forgery encontrado, mas sem atributo value.");

            return valueMatch.Groups[1].Value;
        }
    }
}
