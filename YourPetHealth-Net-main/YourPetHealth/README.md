<div align="center">

# 🐾 YourPetHealth
### Assistente Veterinário Preditivo & Telemetria IoT em Tempo Real

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=.net&logoColor=white" alt=".NET 10">
  <img src="https://img.shields.io/badge/MongoDB-4EA94B?style=for-the-badge&logo=mongodb&logoColor=white" alt="MongoDB">
  <img src="https://img.shields.io/badge/Google_Gemini-8E75B2?style=for-the-badge&logo=googlegemini&logoColor=white" alt="Google Gemini">
  <img src="https://img.shields.io/badge/Architecture-Repository_Pattern-blue?style=for-the-badge" alt="Repository Pattern">
</p>

*Monitoramento contínuo de sinais vitais e triagem preditiva alimentada por Inteligência Artificial para a saúde animal.*

</div>

---

## 📌 O Problema e a Nossa Solução

### ⚠️ O Problema
Na medicina veterinária tradicional, tutores frequentemente enfrentam dificuldade para identificar os primeiros sinais de dor ou desconforto em seus animais de estimação, já que os pets tendem a mascarar sintomas. Quando uma alteração física é notada visualmente, o quadro clínico pode estar avançado. Além disso, a falta de dados históricos de sinais vitais no momento do atendimento dificulta um diagnóstico rápido e preciso por parte do veterinário.

### 💡 A Solução YourPetHealth
O **YourPetHealth** resolve essa lacuna integrando dispositivos **IoT de telemetria contínua** a um dashboard inteligente em **.NET 10 MVC**. A plataforma captura, em tempo real, métricas vitais como **frequência cardíaca (BPM)** e **temperatura corporal (°C)**, armazenando o histórico de saúde no **MongoDB**.

Com esses dados centralizados, a solução atua em duas frentes:
1. **Prevenção Ativa**: Permite que tutores e veterinários acompanhem variações fisiológicas em tempo real antes que se tornem emergências.
2. **Triagem Inteligente**: Oferece um canal de suporte imediato para o tutor tirar dúvidas sobre o comportamento do pet.

---

## 🤖 O Agente de IA Veterinária

O coração da nossa triagem é um **Agente de IA Generativa** integrado via API do **Google Gemini** (`gemini-3.5-flash`). Diferente de um chatbot comum, o agente opera com **contexto fisiológico em tempo real**:

- **Cruzamento de Dados**: Ao receber uma dúvida ou relato de sintoma do tutor (ex: *"meu pet está apático e recusa comida"*), o agente consulta instantaneamente a última telemetria registrada no MongoDB.
- **Análise Correlacionada**: A IA correlaciona a queixa do tutor com os batimentos cardíacos e a temperatura corporal recentes para identificar possíveis anomalias (como febre ou taquicardia).
- **Orientações Preditivas e Empáticas**: Retorna respostas rápidas, humanas e estruturadas, fornecendo primeiros cuidados recomendados e sinalizando quando a busca por atendimento presencial imediato é indispensável.

---

## 🚀 Tecnologias Utilizadas

| Camada | Tecnologia / Ferramenta |
| :--- | :--- |
| **Backend / Web** | .NET 10 MVC (C#) |
| **Inteligência Artificial** | Google Gemini API (`gemini-3.5-flash`) via REST / HttpClient |
| **Banco de Dados** | MongoDB (Telemetria IoT) & Oracle DB |
| **Frontend** | Razor Views, Bootstrap 5, FontAwesome |
| **Arquitetura** | Repository Pattern, Dependency Injection, Clean Code |

---

## 🛠️ Estrutura do Projeto

    YourPetHealth/
    ├── Controllers/         # Lógica de controle de rotas e visões (Pet, Telemetria, IA)
    ├── Models/              # Entidades do domínio (Pet, TelemetriaModel, etc.)
    ├── Repositories/        # Camada de acesso a dados (MongoDB / Oracle)
    ├── Services/            # Serviços de integração (GeminiService)
    ├── Views/               # Telas Razor (.cshtml) do Dashboard
    └── appsettings.json     # Configurações de conexões e API Keys

---

## ⚙️ Como Executar o Projeto

### Pré-requisitos
- .NET 10 SDK instalado na máquina.
- Instância do **MongoDB** em execução localmente ou na nuvem.
- Chave de API ativa do **Google AI Studio**.

### Passo a Passo

1. **Clonar o repositório:**
   git clone https://github.com/seu-usuario/YourPetHealth.git
   cd YourPetHealth

2. **Configurar as credenciais:**
   No ficheiro `appsettings.json`, insira as suas chaves e strings de conexão:
   
   {
     "ConnectionStrings": {
       "MongoDb": "sua-connection-string-mongodb",
       "Oracle": "sua-connection-string-oracle"
     },
     "Gemini": {
       "ApiKey": "SUA_API_KEY_AQUI"
     }
   }

3. **Executar a aplicação:**
   dotnet run

4. **Aceder no navegador:** 
   Abra `https://localhost:5001` ou `http://localhost:5000`

---

## 👥 Equipe de Desenvolvimento

| Colaborador | RM / Matrícula |
| :--- | :---: |
| **Daniel Fonseca de Almeida** | RM 563045 |
| **Erick de Faria Gama** | RM 561951 |
| **Guilherme Batista Cintra** | RM 562850 |
| **Matheus Nascimento Corrégio** | RM 563765 |
| **Pedro Fonseca de Almeida** | RM 563466 |

---

## 🚫 Ficheiro .gitignore

    bin/
    obj/
    .vs/
    appsettings.Development.json
    *.user