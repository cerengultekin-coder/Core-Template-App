🧱 CoreApp (.NET 9 Clean Architecture Boilerplate)
CoreApp is a reusable, production-ready boilerplate built with .NET 9 and Clean Architecture principles.
It provides a solid foundation for building secure, modular, and scalable applications using modern best practices like CQRS, MediatR, FluentValidation, and plug-and-play infrastructure layers.

🚀 Features

✅ Clean Architecture (Domain → Application → Infrastructure → WebAPI)

✅ CQRS with MediatR

✅ FluentValidation integration

✅ MediatR Pipeline Behaviors (Validation, Logging, Exception, Performance, Authorization)

✅ Authentication module (JWT, RefreshToken, Claims)

✅ AI Service Abstraction with support for:

    🌐 OpenRouter (cloud-based LLM)

    💻 Ollama (local open-source LLM inference)

✅ Config-driven service resolution (Strategy Pattern)

✅ Modular, testable structure with feature-based branching

---

## 📁 Project Structure

src/
├── CoreApp.Domain             # Domain models and core contracts
├── CoreApp.Application        # CQRS, DTOs, Interfaces, Business Rules
├── CoreApp.Infrastructure     # EF Core, AuthService, AI Integrations, etc.
├── CoreApp.WebAPI             # RESTful API setup, middleware
├── CoreApp.Tests              # Unit + Integration tests


---

🧠 Modular AI Integration (OpenRouter + Ollama)
This project supports dynamic AI service selection using a strategy resolver pattern. Choose your provider via config:

AI Providers Supported:
Provider	Description	Model Example
OpenRouter	Cloud-based, supports many models	mistralai/mistral-7b-instruct
Ollama	Local inference, open-source models	mistral, llama3, etc.

🔧 Configuration Example:

"AiSettings": {
  "Provider": "OpenRouter" // or "Ollama"
},
"OpenAI": {
  "ApiKey": "sk-xxx"
}


🧩 Key Components:

IAIService: Common interface for all AI services

OpenRouterAiService: Talks to https://openrouter.ai/api/v1

OllamaAiService: Talks to http://localhost:11434/api/generate

AiServiceResolver: Dynamically resolves which service to use

AiRequestOptions: Optional settings like context, temperature, etc.

PromptTextCommand: CQRS command for AI prompts

✅ No code change needed to switch providers – just update your config!

📦 Tech Stack

| Concern         | Tech                                     |
| --------------- | ---------------------------------------- |
| Runtime         | .NET 9                                   |
| CQRS / Mediator | MediatR                                  |
| Validation      | FluentValidation                         |
| Auth            | JWT, Refresh Tokens, Claims              |
| ORM             | EF Core (SQL Server, PostgreSQL planned) |
| AI              | OpenRouter, Ollama                       |
| Design Patterns | Strategy, Clean Architecture, SOLID      |


## 🧩 Implemented Modules

### 📌 Domain Layer
- `BaseEntity`, `IEntity`
- `User`, `Role`, `RefreshToken` entities

### 📌 Application Layer
- Auth DTOs: `RegisterRequest`, `LoginRequest`, `AuthResponse`
- CQRS: `RegisterCommand`, `LoginCommand` + their handlers & validators
- Interface: `IAuthService`
- ✅ MediatR Pipeline Behaviors:
  - `ValidationBehavior`
  - `UnhandledExceptionBehavior`
  - `PerformanceBehavior`
  - `LoggingBehavior`
  - `AuthorizationBehavior`
  - `TransactionBehavior`

---

## 📦 Tech Stack

| Area              | Technology                                      |
|-------------------|--------------------------------------------------|
| Language/Runtime  | .NET 9                                           |
| CQRS / Mediator   | MediatR                                          |
| Validation        | FluentValidation                                 |
| Auth (planned)    | JWT, Refresh Token, Claims-based Authorization   |
| ORM (planned)     | EF Core + PostgreSQL or SQL Server               |
| Logging (planned) | Serilog                                          |

---

## 🔁 Git Branching Strategy

| Branch                          | Description                                  |
|----------------------------------|----------------------------------------------|
| `develop`                       | Main development branch                      |
| `feature/domain-model`          | Entities: User, Role, Token                  |
| `feature/application-auth`      | Auth CQRS, DTOs, Handlers                    |
| `feature/application-pipeline-behaviors` | Core MediatR behaviors                     |
| `feature/application-extra-behaviors`    | Logging, Authorization, Transaction         |

All branches were merged via PRs using semantic commit messages.

---

## 🛠️ How to Use

```bash
# clone and switch to develop
git clone https://github.com/your-username/CoreApp.git
cd CoreApp
git checkout develop

# build & run
dotnet build
dotnet run --project src/CoreApp.WebAPI

# run tests
dotnet test

```

📮 AI Usage Example
Send a prompt:

![image](https://github.com/user-attachments/assets/3b533369-3db4-4af7-bc47-068a57f57757)

![image](https://github.com/user-attachments/assets/0210f05b-7ef9-40cb-b6c6-c802a79666bc)

🙌 Contributions
You’re welcome to open issues or submit PRs!
Please follow the clean architecture principles and keep layers decoupled.



