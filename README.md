# YourPetHealth+ — Sistema de Gestão para Clínicas Veterinárias

Sistema web em **ASP.NET Core MVC (.NET 10)** para gestão de uma clínica veterinária: cadastro de tutores, veterinários e pets, agendamento de consultas, histórico clínico, autenticação com perfis de acesso, **telemetria de saúde pet via IoT (MongoDB)**, monitoramento (health checks, logging estruturado, tracing/métricas) e testes automatizados.

## Sumário

- [Funcionalidades](#funcionalidades)
- [Perfis de acesso](#perfis-de-acesso)
- [Arquitetura e estrutura](#arquitetura-e-estrutura)
- [Como rodar o projeto](#como-rodar-o-projeto)
- [Telemetria IoT (MongoDB)](#telemetria-iot-mongodb)
- [Monitoramento e Observabilidade](#monitoramento-e-observabilidade)
- [Testes automatizados](#testes-automatizados)
- [Solução de problemas](#solução-de-problemas)
- [Tecnologias utilizadas](#tecnologias-utilizadas)

---

## Funcionalidades

- **Cadastro unificado** (`/Auth/Register`): uma única tela de "criar conta", com escolha de perfil (Tutor de pet ou Veterinário).
- **Login** com autenticação por cookie e redirecionamento diferente por perfil. A navbar exibe **Olá, {nome}** (nome do tutor/veterinário; se não houver, usa o e-mail).
- **Tutores**: cadastram e gerenciam seus próprios pets, agendam consultas com o veterinário de sua escolha.
- **Veterinários**: visualizam todos os tutores/pets do sistema e atualizam o status das consultas que são deles (Realizada/Cancelada).
- **Histórico clínico** por pet.
- **Telemetria IoT**: recebe leituras de sensores (batimentos cardíacos, temperatura e status geral) e armazena no MongoDB.
- **Swagger/OpenAPI** disponível em ambiente de desenvolvimento para testar os endpoints da API.
- Regra de negócio: não é possível remover um pet que tenha consulta agendada.

## Perfis de acesso

| Área | Tutor (RESPONSAVEL) | Veterinário |
|---|---|---|
| Responsáveis | sem acesso | vê todos |
| Pets | só os seus (CRUD completo) | vê todos (leitura) |
| Veterinários | vê todos (leitura, pra escolher na consulta) | gerencia (CRUD) |
| Consultas | agenda para os próprios pets | atualiza status das que são dele |

## Arquitetura e estrutura

A solution é dividida em 3 projetos:

```
YourPetHealth.sln
├── YourPetHealth/                     # aplicação MVC + API
│   ├── Controllers/
│   ├── Models/                        # inclui TelemetriaPet (documento MongoDB)
│   ├── Views/
│   ├── Data/                          # AppDbContext (EF Core + Oracle)
│   ├── Services/                      # MongoDbService, GeminiService
│   ├── Migrations/
│   ├── HealthChecks/                  # health checks customizados
│   ├── Middleware/                    # correlation id
│   ├── Diagnostics/                   # ActivitySource/Meter (OpenTelemetry manual)
│   ├── wwwroot/                       # CSS, JS e arquivos estáticos
│   └── Program.cs
├── YourPetHealth.Tests.Unit/          # testes unitários (xUnit + Moq)
└── YourPetHealth.Tests.Integration/   # testes de integração (xUnit + WebApplicationFactory)
```

**Bancos de dados:** o sistema usa dois bancos. O **Oracle** guarda os dados relacionais (tutores, pets, consultas, usuários) via EF Core. O **MongoDB** guarda as leituras de telemetria, que são dados de alto volume e sem esquema fixo.

## Como rodar o projeto

### Pré-requisitos

- **.NET 10 SDK**
- Acesso a um banco **Oracle** (o projeto foi desenvolvido usando o Oracle disponibilizado pela FIAP)
- **MongoDB** (local em `localhost:27017` ou MongoDB Atlas)

### Passo a passo

```bash
git clone https://github.com/M5-SOLUTIONS/YourPetHealth-.Net.git
cd YourPetHealth-.Net
```

Configure as conexões em `YourPetHealth/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "OracleConnection": "User Id=SEU_USUARIO;Password=SUA_SENHA;Data Source=SEU_HOST:1521/SEU_SERVICO;"
  },
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "clyvo_vet_db"
  }
}
```

> **Atenção:** não envie senhas reais para o Git. Use o `appsettings.Development.json` (listado no `.gitignore`) ou o *User Secrets* (`dotnet user-secrets`) para credenciais.

Aplique as migrations e rode o projeto:

```bash
cd YourPetHealth
dotnet restore
dotnet ef database update
dotnet run
```

Acesse `http://localhost:PORTA` (a porta aparece no terminal ao rodar, na linha `Now listening on`). O Swagger fica em `http://localhost:PORTA/swagger` (apenas em ambiente `Development`).

> **Dica:** a aplicação usa a pasta do executável como raiz de conteúdo e copia a `wwwroot` para a pasta de saída no build. Por isso o CSS carrega mesmo ao abrir o `.exe` diretamente. Para publicar, use `dotnet publish -c Release`.

---

## Telemetria IoT (MongoDB)

Os sensores enviam leituras para a API, que grava os documentos na coleção `telemetria` do banco `clyvo_vet_db`.

Modelo do documento (`TelemetriaPet`):

| Campo | Tipo | Descrição |
|---|---|---|
| `Id` | string (ObjectId) | Gerado pelo MongoDB. Ignorado no Swagger na criação. |
| `PetId` | int | Identificador do pet (referência ao Oracle) |
| `BatimentosCardiacos` | double | Batimentos por minuto |
| `Temperatura` | double | Temperatura corporal |
| `StatusGeral` | string | Status geral do pet |
| `DataLeitura` | DateTime | Data/hora da leitura (UTC, padrão: agora) |

Para testar, rode a aplicação em `Development`, abra o Swagger e use os endpoints de telemetria (POST para gravar, GET para listar). Os registros podem ser conferidos no MongoDB Compass, no banco `clyvo_vet_db`, coleção `telemetria`.

---

## Monitoramento e Observabilidade

### Health Checks

Dois endpoints, sem autenticação, retornando JSON detalhado (status geral, duração de cada check, dados extras):

- **`GET /health`** — todos os checks:
  - `oracle_database`: abre conexão real com o Oracle e roda `SELECT 1 FROM DUAL`. Retorna `Healthy`, `Degraded` (acima de 1s de resposta) ou `Unhealthy`.
  - `external_service`: faz um GET num serviço externo configurável em `HealthChecks:ExternalServiceUrl` (appsettings.json).
- **`GET /health/ready`** — só os checks marcados com a tag `ready` (útil em orquestradores que separam liveness de readiness).

Na interface, o link **Health** aparece na barra de navegação (e no rodapé) e abre `/health` em nova aba, para inspecionar rapidamente o status da aplicação, do Oracle e do serviço externo.

Exemplo de resposta:
```json
{
  "status": "Healthy",
  "totalDurationMs": 145.2,
  "checks": [
    { "name": "oracle_database", "status": "Healthy", "durationMs": 98.1, "data": { "responseTimeMs": 95 } },
    { "name": "external_service", "status": "Healthy", "durationMs": 47.1 }
  ]
}
```

### Logging estruturado (Serilog)

- Saída em **console** e em **arquivo** (`YourPetHealth/logs/log-AAAAMMDD.txt`, rotação diária, mantém 14 dias).
- Cada requisição HTTP vira um único log estruturado (método, rota, status, duração).
- Todo log de uma mesma requisição carrega um **Correlation Id** (reaproveita o header `X-Correlation-Id` se o cliente mandar, ou gera um novo — devolvido também no header da resposta).
- Níveis configuráveis em `appsettings.json`, seção `Serilog:MinimumLevel`.

### Tracing e métricas (OpenTelemetry)

- Instrumentação automática de ASP.NET Core e HttpClient (spans e métricas de tempo de resposta / taxa de erro entre camadas).
- Spans manuais em pontos de negócio importantes (`Auth.Login`, `Responsaveis.Create`) via `AppDiagnostics.ActivitySource`.
- Métricas customizadas: logins bem-sucedidos/falhos, cadastros de responsável (`AppDiagnostics`, expostas via `Meter`).
- Exportador de **console** — os spans e métricas aparecem no próprio terminal onde `dotnet run` está rodando; não é necessário configurar nenhum backend externo (Jaeger/Application Insights/etc.) para ver a instrumentação funcionando.

---

## Testes automatizados

A solution tem dois projetos de teste, separados por camada:

| Projeto | O que testa | Ferramentas |
|---|---|---|
| `YourPetHealth.Tests.Unit` | Controllers isolados (regras de autorização/negócio), extensions puras, health check de serviço externo | xUnit, Moq, EF Core InMemory |
| `YourPetHealth.Tests.Integration` | Fluxo HTTP completo (registro → login → rota protegida → logout), endpoints de health check | xUnit, `WebApplicationFactory` |

Convenção de nomenclatura: `MetodoTestado_Cenario_ResultadoEsperado`. Todos seguem o padrão **AAA** (Arrange, Act, Assert), com os testes de `PetsController`, `ConsultasController` e `AuthController` reaproveitando dados semeados via **Class Fixture** (`ReferenceDataFixture`), e os testes de integração compartilhando **um único host da aplicação** entre as classes via **Collection Fixture** (`WebAppFactoryFixture` + `[Collection("Integration")]`).

### Rodando os testes

```bash
# a partir da raiz da solution (onde está o YourPetHealth.sln)
dotnet test

# só os unitários
dotnet test YourPetHealth.Tests.Unit

# só os de integração
dotnet test YourPetHealth.Tests.Integration
```

> Os testes dependem do projeto principal compilar. Se o build do `YourPetHealth` falhar, os testes acusam `CS0006` (DLL não encontrada). Corrija primeiro o erro do projeto principal.

---

## Solução de problemas

| Sintoma | Causa provável | Solução |
|---|---|---|
| `CS0101: o namespace já contém uma definição para "TelemetriaPet"` | Existem duas cópias da classe (por exemplo uma em `Controllers/`) | Mantenha uma só, em `Models/TelemetriaPet.cs`, e apague a outra |
| `CS0006: arquivo de origem ...YourPetHealth.dll não pode ser encontrado` | O projeto principal não compilou | Corrija os erros do `YourPetHealth` e recompile a solution |
| `CS0246 BsonId / MongoClient não encontrado`, `DateTime não existe` | O restore dos pacotes NuGet falhou ou não rodou | Rode `dotnet restore` e depois `dotnet build`; veja a mensagem `NU....` se falhar |
| `Não é possível iniciar o programa ...YourPetHealth.exe/.dll` | O build falhou, então o executável não foi gerado | Veja a aba **Saída** do Visual Studio e corrija o erro de build ou de restore |
| `Failed to bind to address http://127.0.0.1:5000: address already in use` (e o VS fecha com código `0xe0434352`) | A porta já está em uso, em geral por uma instância antiga aberta | Encerre o processo antigo: `Get-Process YourPetHealth \| Stop-Process -Force`; ou use outra porta: `dotnet run --urls http://localhost:5050` |
| Página abre **sem CSS** ao abrir o `.exe` | O aviso `WebRootPath was not found` indica que a `wwwroot` não está na pasta de saída | Mantenha `ContentRootPath = AppContext.BaseDirectory` no `Program.cs` e o item `<Content Update="wwwroot\**" CopyToOutputDirectory="PreserveNewest" />` no `.csproj` |
| Falhas intermitentes de build, arquivos travados em `obj/` ou `bin/` | O projeto está dentro de uma pasta sincronizada (OneDrive) | Mova o projeto para uma pasta fora do OneDrive (ex.: `C:\dev\YourPetHealth`) |
| Swagger não aparece | A aplicação está em ambiente `Production` (acontece ao abrir o `.exe` direto) | Rode com `dotnet run` ou defina `ASPNETCORE_ENVIRONMENT=Development` |

**Limpeza completa de build** (PowerShell, na pasta da solution):

```powershell
Get-ChildItem -Path . -Include bin,obj -Recurse -Directory | Remove-Item -Recurse -Force
dotnet restore
dotnet build
```

---

## Tecnologias utilizadas

- ASP.NET Core MVC (.NET 10)
- Entity Framework Core + Oracle.EntityFrameworkCore
- MongoDB.Driver (telemetria IoT)
- Swagger / Swashbuckle (OpenAPI)
- Autenticação por cookie (`Microsoft.AspNetCore.Authentication.Cookies`) + `PasswordHasher<T>`
- Serilog (console + arquivo)
- OpenTelemetry (tracing + métricas)
- Microsoft.Extensions.Diagnostics.HealthChecks
- xUnit, Moq, EF Core InMemory, `Microsoft.AspNetCore.Mvc.Testing`
- Bootstrap + CSS customizado

---

## Desenvolvido por

- Erick de Faria Gama — RM561951
- Matheus Nascimento Corregio — RM563765
- Guilherme Cintra — RM562850
- Pedro Fonseca de Almeida — RM563466
- Daniel Fonseca de Almeida — RM563045
