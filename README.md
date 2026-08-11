# ProjetoBusiness

API de agendamentos multi-tenant (SaaS) construída em .NET, com lembretes automáticos enviados por WhatsApp. Pensada para clínicas, barbearias, salões e qualquer negócio que trabalhe com horário marcado — cada empresa cadastrada tem seus próprios clientes e agendamentos, totalmente isolados dos demais.

Comecei esse projeto pra sair do "CRUD básico" e treinar coisas que aparecem em sistema de verdade rodando em produção: multi-tenancy, processamento em background, integração com API externa instável, logs que realmente ajudam a debugar, e tratamento de erro centralizado.

## O que o sistema faz

Uma empresa se cadastra, cria seus clientes, e agenda horários pra eles. Um minuto depois de criar o agendamento, o cliente recebe uma mensagem de lembrete no WhatsApp — com o texto podendo ser customizado por empresa (cada uma define seu próprio template, com `{{Nome}}` e `{{Data}}` sendo substituídos automaticamente).

Todo esse fluxo é protegido por autenticação JWT, e o `EmpresaId` de cada usuário vem embutido no próprio token — então não tem como um usuário de uma empresa acessar dado de outra nem por engano ou má-fé, o dado nunca é passado "confiando" no que o front-end manda.

## Arquitetura

Segui Clean Architecture e SOLID na medida do que faz sentido pra uma API desse porte: Controllers finos, regra de negócio nos Services, e toda configuração de infraestrutura isolada em Extension Methods dentro de `Configurations/`. O `Program.cs` só orquestra a ordem de tudo, sem lógica escondida nele.

```
ProjetoBusiness/
├── ArkahBusiness.API/
│   ├── Configurations/     → setup de banco, JWT, Hangfire, Serilog, HttpClient/Polly
│   ├── Controllers/        → Auth, Cliente, Agendamento
│   ├── Data/                → AppDbContext (EF Core)
│   ├── DTOs/                → contratos de request/response
│   ├── Extensions/          → ClaimsPrincipalExtensions (pega o EmpresaId do token)
│   ├── Middlewares/         → CorrelationId e tratamento global de exceções
│   ├── Migrations/
│   ├── Models/               → Empresa, Usuario, Cliente, Agendamento
│   ├── Services/             → regra de negócio (Auth, Cliente, Agendamento, WhatsApp)
│   └── Program.cs
├── ArkahBusiness.Tests/     → xUnit + Moq + EF Core InMemory
└── docker/                  → Evolution API + Postgres + Redis
```

### Por que fiz certas escolhas

**Multi-tenant via Claim no token, não via campo no body.** Se o `EmpresaId` viesse no corpo da requisição, qualquer pessoa poderia trocar o número e tentar acessar dado de outra empresa (IDOR). Em vez disso, o token JWT já carrega essa informação assinada, e um extension method (`User.GetEmpresaId()`) extrai ela direto do usuário autenticado. O front nunca decide de qual empresa é o dado — o token decide.

**Hangfire pro envio de lembrete.** Mandar a mensagem de WhatsApp de forma síncrona, dentro da própria requisição de criar agendamento, seria um problema: se a Evolution API estivesse lenta ou fora do ar naquele instante, o usuário ficaria esperando (ou o agendamento falharia por causa de um problema que nem é dele). Com Hangfire, o agendamento é salvo, a resposta volta rápido pro usuário, e o lembrete é disparado em segundo plano um minuto depois — com direito a dashboard visual pra acompanhar os jobs.

**Polly no HttpClient da Evolution API.** Integração externa cai. Em vez de estourar exceção na primeira instabilidade de rede, o client tenta de novo automaticamente com backoff exponencial (2s, 4s, 8s) antes de desistir e logar o erro de verdade.

**Serilog com Correlation ID.** Cada requisição ganha um ID único que acompanha todos os logs gerados durante aquela chamada — do controller até o service, até uma eventual falha na Evolution API. Quando alguma coisa quebra, dá pra filtrar o log inteiro daquela requisição específica em vez de vasculhar tudo.

**Middleware de exceção centralizado.** Em vez de `try/catch` espalhado em cada controller, qualquer exceção não tratada passa por um único lugar, que decide o status code certo, loga do jeito certo (Warning pra erro de validação, Error com stack trace pra falha real) e devolve uma resposta JSON padronizada — sempre com o `correlationId` junto, pra rastrear depois.

**Nenhuma credencial no código.** Connection string, chave JWT e API key da Evolution ficam fora do repositório via User Secrets em desenvolvimento (e variável de ambiente em produção). O `appsettings.json` que vai pro Git não tem valor sensível nenhum — só a estrutura.

## Stack

.NET · Entity Framework Core (SQL Server) · Hangfire · Serilog · Polly · JWT Bearer · BCrypt.Net · Swagger + Scalar · xUnit + Moq · Docker (Evolution API, PostgreSQL, Redis)

## Rodando o projeto

### Pré-requisitos

- .NET SDK
- SQL Server (local ou em container)
- Docker, pra subir a infraestrutura de WhatsApp

### 1. Clone e configure os segredos

```bash
git clone https://github.com/SEU-USUARIO/ProjetoBusiness.git
cd ProjetoBusiness/ArkahBusiness.API

dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=ArkahBusinessDb;User Id=sa;Password=SUA_SENHA;TrustServerCertificate=True;"
dotnet user-secrets set "JwtSettings:SecretKey" "uma-chave-de-pelo-menos-32-caracteres"
dotnet user-secrets set "EvolutionAPI:ApiKey" "sua-api-key-da-evolution"
```

(o `AppSettingsExample.json` mostra todos os campos que precisam ser preenchidos, caso queira conferir)

### 2. Suba o WhatsApp (Docker)

```bash
cd ../docker
cp .env.example .env
# edita o .env com suas senhas antes de continuar
docker-compose up -d
```

Isso sobe três containers: `evolution-api` (o gateway que conversa com o WhatsApp), `evolution-postgres` (banco próprio da Evolution) e `evolution-redis` (cache de sessão).

### 3. Aplique as migrations e rode

```bash
cd ../ArkahBusiness.API
dotnet ef database update
dotnet run
```

A API sobe com um usuário administrador já criado automaticamente (seed), pra facilitar o primeiro teste. A documentação interativa fica em `/swagger` ou `/scalar`.

## Testando pela primeira vez

A API inteira (exceto login) é protegida por JWT, então a ordem importa:

**1.** Faça login em `POST /api/auth/login`:
```json
{
  "email": "admin@arkah.com",
  "senha": "Admin@123"
}
```

**2.** Copia o `token` que voltou na resposta. No Swagger/Scalar, clica em **Authorize** e cola assim, com `Bearer` na frente e um espaço:
```
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```
Sem isso, todo endpoint protegido retorna 401. Esse passo vale pra criar cliente, listar cliente, criar agendamento e listar agendamento — se o token expirar (8h), é só repetir o login.

**3.** Cria um cliente em `POST /api/cliente`. O telefone deve ir com DDI e DDD juntos, sem símbolo (ex: `5533999999999`), porque é esse número que a Evolution API vai usar pra mandar a mensagem.

**4.** Cria um agendamento em `POST /api/agendamento`, usando o `clienteId` que voltou no passo anterior. Atenção ao campo `dataHora` — precisa estar em ISO 8601 (`2026-08-15T14:30:00`). Mandar no formato `15/08/2026 14:30` dá erro de validação antes mesmo de chegar no controller.

Um minuto depois de criar o agendamento, o Hangfire dispara o lembrete pro WhatsApp do cliente (desde que a instância da Evolution API esteja conectada — ver abaixo).

## Conectando o WhatsApp

A Evolution API precisa de uma instância pareada com um número de WhatsApp real via QR Code antes de conseguir mandar mensagem. Com os containers do Docker no ar:

```http
POST http://localhost:8080/instance/create
apikey: SUA_API_KEY
Content-Type: application/json

{
  "instanceName": "nome_da_instancia",
  "qrcode": true,
  "integration": "WHATSAPP-BAILEYS"
}
```

```http
GET http://localhost:8080/instance/connect/nome_da_instancia
apikey: SUA_API_KEY
```

A segunda chamada retorna um QR Code — escaneia com o WhatsApp que vai enviar os lembretes. A `apikey` usada aqui, no `.env` do Docker e no User Secrets da API precisam ser todas a mesma. O `instanceName` também precisa bater com o que está configurado em `EvolutionAPI:InstanceName`.

## Acompanhando os jobs

O Hangfire tem um dashboard em `/hangfire` pra visualizar os jobs de lembrete agendados, em execução ou com falha. Em ambiente de desenvolvimento ele fica aberto sem autenticação extra pra não atrapalhar o dia a dia; fora de desenvolvimento, só usuário autenticado consegue acessar.

## Testes

```bash
cd ArkahBusiness.Tests
dotnet test
```

Os testes usam banco em memória (EF Core InMemory) e mocks via Moq, então rodam isolados sem depender de SQL Server ou da Evolution API de verdade.
