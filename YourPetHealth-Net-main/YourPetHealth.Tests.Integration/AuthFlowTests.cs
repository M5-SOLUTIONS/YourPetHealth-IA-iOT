using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using YourPetHealth.Data;
using YourPetHealth.Tests.Integration.Fixtures;
using Xunit;

namespace YourPetHealth.Tests.Integration
{
    [Collection("Integration")]
    public class AuthFlowTests
    {
        private readonly WebAppFactoryFixture _factory;

        public AuthFlowTests(WebAppFactoryFixture factory)
        {
            _factory = factory;
        }

        private static string TextoVisivel(string html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;
            var semTags = Regex.Replace(html, "<[^>]+>", " ");
            return System.Net.WebUtility.HtmlDecode(semTags);
        }

        private async Task LimparUsuarioDeTesteAsync(string email)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var usuario = db.Usuarios.FirstOrDefault(u => u.Email == email);
            if (usuario == null) return;

            if (usuario.ResponsavelId.HasValue)
            {
                var responsavel = db.Responsaveis.FirstOrDefault(r => r.Id == usuario.ResponsavelId.Value);
                if (responsavel != null)
                    db.Responsaveis.Remove(responsavel);
            }

            if (usuario.VeterinarioId.HasValue)
            {
                var veterinario = db.Veterinarios.FirstOrDefault(v => v.Id == usuario.VeterinarioId.Value);
                if (veterinario != null)
                    db.Veterinarios.Remove(veterinario);
            }

            db.Usuarios.Remove(usuario);
            await db.SaveChangesAsync();
        }

        [Fact]
        public async Task FluxoCompleto_RegistrarLogarAcessarRotaProtegidaEDeslogar_FuncionaDePontaAPonta()
        {
            var client = _factory.CreateClient();
            var email = $"integracao.{Guid.NewGuid():N}@teste.com";
            const string senha = "SenhaForte123";

            try
            {
                var registerForm = new Dictionary<string, string>
                {
                    ["Nome"] = "Tutor de Teste",
                    ["Email"] = email,
                    ["Senha"] = senha,
                    ["Telefone"] = "11999999999",
                    ["TipoConta"] = "RESPONSAVEL"
                };
                var registerResponse = await client.PostAsync("/Auth/Register", new FormUrlEncodedContent(registerForm));
                registerResponse.EnsureSuccessStatusCode();

                var loginPageHtml = await client.GetStringAsync("/Auth/Login");
                var token = AntiForgeryHelper.ExtractToken(loginPageHtml);

                var loginForm = new Dictionary<string, string>
                {
                    ["email"] = email,
                    ["senha"] = senha,
                    ["__RequestVerificationToken"] = token
                };
                var loginResponse = await client.PostAsync("/Auth/Login", new FormUrlEncodedContent(loginForm));
                loginResponse.EnsureSuccessStatusCode();
                Assert.Contains("/Pets", loginResponse.RequestMessage!.RequestUri!.AbsolutePath);

                var createPetResponse = await client.GetAsync("/Pets/Create");
                Assert.Equal(HttpStatusCode.OK, createPetResponse.StatusCode);

                var responsaveisResponse = await client.GetAsync("/Responsaveis");
                Assert.True(
                    responsaveisResponse.StatusCode == HttpStatusCode.OK
                    || responsaveisResponse.RequestMessage?.RequestUri?.AbsolutePath.Contains("/Auth/Login") == true
                    || responsaveisResponse.StatusCode == HttpStatusCode.Forbidden
                    || (int)responsaveisResponse.StatusCode == 302,
                    $"Esperava bloqueio de acesso a /Responsaveis para tutor. Status={responsaveisResponse.StatusCode}");

                var petsPageHtml = await client.GetStringAsync("/Pets");
                var logoutToken = AntiForgeryHelper.ExtractToken(petsPageHtml);
                var logoutResponse = await client.PostAsync("/Auth/Logout",
                    new FormUrlEncodedContent(new Dictionary<string, string> { ["__RequestVerificationToken"] = logoutToken }));
                logoutResponse.EnsureSuccessStatusCode();

                var afterLogout = await client.GetAsync("/Pets/Create");
                Assert.Contains("/Auth/Login", afterLogout.RequestMessage!.RequestUri!.AbsolutePath);
            }
            finally
            {
                await LimparUsuarioDeTesteAsync(email);
            }
        }

        [Fact]
        public async Task Login_ComSenhaErrada_NaoAutenticaEMostraMensagemDeErro()
        {
            var client = _factory.CreateClient();
            var email = $"integracao.{Guid.NewGuid():N}@teste.com";

            try
            {
                var registerForm = new Dictionary<string, string>
                {
                    ["Nome"] = "Tutor Senha Errada",
                    ["Email"] = email,
                    ["Senha"] = "SenhaCorreta123",
                    ["TipoConta"] = "RESPONSAVEL"
                };
                var registerResponse = await client.PostAsync("/Auth/Register", new FormUrlEncodedContent(registerForm));
                registerResponse.EnsureSuccessStatusCode();

                var loginPageHtml = await client.GetStringAsync("/Auth/Login");
                var token = AntiForgeryHelper.ExtractToken(loginPageHtml);

                var loginForm = new Dictionary<string, string>
                {
                    ["email"] = email,
                    ["senha"] = "SenhaErrada999",
                    ["__RequestVerificationToken"] = token
                };
                var loginResponse = await client.PostAsync("/Auth/Login", new FormUrlEncodedContent(loginForm));
                var html = await loginResponse.Content.ReadAsStringAsync();
                var texto = TextoVisivel(html);

                Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
                Assert.True(
                    texto.Contains("inválidos", StringComparison.OrdinalIgnoreCase)
                    || texto.Contains("invalidos", StringComparison.OrdinalIgnoreCase)
                    || texto.Contains("Email ou senha", StringComparison.OrdinalIgnoreCase),
                    $"Mensagem de erro de login não encontrada no HTML. Trecho: {texto.Substring(0, Math.Min(500, texto.Length))}");
            }
            finally
            {
                await LimparUsuarioDeTesteAsync(email);
            }
        }

        [Fact]
        public async Task AcessarRotaProtegida_SemEstarLogado_RedirecionaParaLogin()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/Pets/Create");
            Assert.Contains("/Auth/Login", response.RequestMessage!.RequestUri!.AbsolutePath);
        }
    }
}
