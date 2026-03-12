# PopaDin

Aplicação de finanças pessoais para controle de receitas, despesas, orçamentos e alertas financeiros.

> **Este projeto está em desenvolvimento ativo.** Funcionalidades podem mudar ou estar incompletas.

## Arquitetura

O PopaDin é composto por uma API principal e microserviços event-driven que se comunicam via Azure Service Bus:

```
┌────────────┐       ┌──────────────┐       ┌───────────────────┐
│  Frontend  │──────▶│  Backend API │──────▶│  Azure Service Bus │
│  React     │◀──────│  .NET 8      │       └─────┬─────────┬───┘
└────────────┘       └──────────────┘             │         │
                                                  ▼         ▼
                                          ┌──────────┐ ┌──────────┐
                                          │  Alert   │ │  Export  │
                                          │  Service │ │  Service │
                                          └──────────┘ └──────────┘
```

- **Backend API** — REST API com autenticação JWT, Clean Architecture (Domain, Service, Infra, IoC, API)
- **AlertService** — Avalia regras de alerta (saldo baixo, orçamento estourado) e envia notificações por email
- **ExportService** — Gera relatórios PDF dos registros financeiros e armazena no Azure Blob Storage

## Tecnologias

### Backend
- .NET 8 / C#
- SQL Server (usuários, orçamentos, tags)
- MongoDB (registros financeiros, alertas, dashboard)
- Redis (cache)
- Azure Service Bus (mensageria)
- Azure Blob Storage (armazenamento de PDFs)
- JWT Bearer Authentication

### Microserviços
- .NET 8 Worker Services
- MailKit (envio de emails)
- QuestPDF (geração de relatórios)

### Frontend
- React 19 + TypeScript
- Tailwind CSS
- React Query (TanStack)
- React Hook Form + Zod
- Vite

## Como rodar

### Pré-requisitos

- [Docker](https://www.docker.com/) e Docker Compose instalados
- Variáveis de ambiente configuradas (veja abaixo)

### Variáveis de ambiente

Crie um arquivo `.env` na raiz do projeto:

```env
# SQL Server
SA_PASSWORD=SuaSenhaForte123!

# Azure Service Bus
SERVICEBUS_CONNECTION_STRING=sua_connection_string

# Azure Blob Storage
BLOB_STORAGE_CONNECTION_STRING=sua_connection_string

# JWT
JWT_SECRET=seu_secret

# SMTP (AlertService)
SMTP_HOST=smtp.exemplo.com
SMTP_PORT=587
SMTP_USERNAME=seu_email
SMTP_PASSWORD=sua_senha
SMTP_SENDER_EMAIL=noreply@popadin.com
```

### Subindo o projeto

```bash
docker compose up --build
```

Isso sobe todos os serviços:

| Serviço | Porta |
|---------|-------|
| Frontend | [localhost:3000](http://localhost:3000) |
| Backend API | [localhost:5285](http://localhost:5285) |
| SQL Server | 1433 |
| MongoDB | 27017 |
| Redis | 6379 |

Os microserviços (AlertService, ExportService) não expõem porta — comunicam-se internamente via Service Bus.
