# VisaFlow — Backend API

Sistema de gestão de processos migratórios (vistos, passaportes, legalizações).

---

## Stack

- **ASP.NET Core 8** — Web API
- **Entity Framework Core 8** — ORM
- **SQL Server** — Base de dados
- **JWT** — Autenticação
- **BCrypt** — Hash de passwords
- **Swagger** — Documentação interactiva da API

---

## Estrutura do projecto

```
VisaFlow/
├── Controllers/
│   ├── AuthController.cs
│   ├── ClientesController.cs
│   ├── ProcessosController.cs
│   ├── PagamentosController.cs
│   ├── DocumentosController.cs
│   └── DashboardController.cs
├── Models/
│   ├── Cliente.cs
│   ├── Processo.cs
│   ├── Pagamento.cs
│   ├── Documento.cs
│   └── Usuario.cs
├── DTOs/
│   └── Dtos.cs
├── Services/
│   ├── ClienteService.cs
│   ├── ProcessoService.cs
│   ├── FinanceiroService.cs
│   ├── DocumentoService.cs
│   ├── AuthService.cs
│   └── DashboardService.cs
├── Data/
│   └── AppDbContext.cs
├── Program.cs
├── appsettings.json
└── VisaFlow.csproj
```

---

## Setup rápido

### 1. Instalar pacotes

```bash
dotnet restore
```

### 2. Configurar a connection string

Edita `appsettings.json`:

```json
"ConnectionStrings": {
  "Default": "Server=SEU_SERVIDOR;Database=VisaFlowDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### 3. Criar a base de dados

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. Arrancar

```bash
dotnet run
```

A API fica disponível em: `https://localhost:5001`

Swagger: `https://localhost:5001/swagger`

---

## Credenciais iniciais (seed)

| Campo | Valor |
|-------|-------|
| Email | admin@visaflow.ao |
| Password | Admin@1234 |

> **Importante:** Muda a password após o primeiro login.

---

## Endpoints principais

### Auth
| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/auth/login` | Login — retorna token JWT |
| GET | `/api/auth/me` | Dados do utilizador autenticado |
| POST | `/api/auth/alterar-password` | Alterar password |

### Clientes
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/clientes` | Listar (filtros: search, activo) |
| GET | `/api/clientes/{id}` | Detalhe |
| POST | `/api/clientes` | Criar |
| PUT | `/api/clientes/{id}` | Actualizar |
| DELETE | `/api/clientes/{id}` | Desactivar (Admin) |

### Processos
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/processos` | Listar (filtros: clienteId, estado, tipo) |
| GET | `/api/processos/{id}` | Detalhe com resumo financeiro e docs |
| POST | `/api/processos` | Criar |
| PUT | `/api/processos/{id}` | Actualizar |
| PATCH | `/api/processos/{id}/estado` | Alterar só o estado |
| DELETE | `/api/processos/{id}` | Eliminar (Admin) |

### Pagamentos (parciais)
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/pagamentos/resumo/{processoId}` | Resumo: total, pago, saldo, histórico |
| GET | `/api/pagamentos/processo/{processoId}` | Lista de pagamentos |
| POST | `/api/pagamentos` | Registar pagamento (valida saldo) |
| DELETE | `/api/pagamentos/{id}` | Eliminar (Admin) |
| GET | `/api/pagamentos/dashboard` | Dashboard financeiro (Admin) |

### Documentos
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/documentos/processo/{processoId}` | Listar documentos |
| GET | `/api/documentos/checklist/{tipoProcesso}` | Checklist obrigatório por tipo |
| POST | `/api/documentos/upload/{processoId}` | Upload (multipart/form-data) |
| PATCH | `/api/documentos/{id}/status` | Validar ou rejeitar documento |
| GET | `/api/documentos/download/{id}` | Download |
| DELETE | `/api/documentos/{id}` | Eliminar |

### Dashboard
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/dashboard` | Métricas gerais do sistema |

---

## Tipos de processo suportados

- `VistoTrabalho`
- `VistoEstudo`
- `Passaporte`
- `Legalizacao`
- `ReagrupamentoFamiliar`

## Estados de processo

- `Pendente` → `EmAnalise` → `Agendado` → `Aprovado` → `Concluido`
- `Rejeitado` (em qualquer fase)

## Estados de pagamento (calculados automaticamente)

- `Pendente` — não pagou nada
- `Parcial` — pagou uma parte
- `Pago` — saldo = 0

---

## Segurança

- Todos os endpoints requerem token JWT no header: `Authorization: Bearer {token}`
- Endpoints de eliminação requerem role `Admin`
- Upload limitado a 10 MB por ficheiro
- Formatos aceites: PDF, JPG, PNG, DOCX

---

## Variáveis de ambiente (produção)

```bash
ConnectionStrings__Default="Server=...;Database=VisaFlowDB;..."
Jwt__Key="chave-muito-segura-minimo-32-caracteres"
Jwt__Issuer="VisaFlow"
Jwt__Audience="VisaFlowApp"
```
