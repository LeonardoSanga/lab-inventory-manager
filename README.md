<div align="center">

# 🤖 Robotics Lab Inventory

**Sistema web de gestão de inventário, estoque e empréstimos para laboratórios de robótica**

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET_Core-MVC-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/aspnet/core/mvc/overview)
[![Entity Framework Core](https://img.shields.io/badge/EF_Core-9-6DB33F)](https://learn.microsoft.com/ef/core/)
[![MySQL](https://img.shields.io/badge/MySQL-8.4-4479A1?logo=mysql&logoColor=white)](https://www.mysql.com/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)](https://docs.docker.com/compose/)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-7952B3?logo=bootstrap&logoColor=white)](https://getbootstrap.com/)

</div>

---

## 📌 Sobre o projeto

O **Robotics Lab Inventory** foi desenvolvido para o Laboratório de Robótica da faculdade com o objetivo de
substituir o controle manual (planilhas e anotações) por um sistema centralizado, rastreável e acessível pela rede local.

Ele permite controlar **quais componentes o laboratório possui, quantos estão disponíveis, quem pegou emprestado,
o que foi devolvido ou danificado**, e quem fez cada alteração no sistema.

O sistema está **em produção** no servidor local da faculdade, rodando em containers Docker.

## ✨ Funcionalidades

| Módulo | O que faz |
|---|---|
| 🔐 **Autenticação** | Login com sessão via cookie, senhas com hash BCrypt e controle de acesso por perfil (Administrador / Usuário). |
| 📦 **Itens** | Cadastro de componentes (Arduinos, sensores, motores etc.), com tipo, marca e descrição. |
| 🗃️ **Estoque** | Quantidade total e disponível por item, atualizada automaticamente pelas movimentações e empréstimos. |
| 🔄 **Movimentações** | Registro de entradas e baixas de estoque com observação e data. |
| 🤝 **Empréstimos** | Empréstimo de itens para alunos, com finalidade (Ensino, Pesquisa ou Extensão), devolução parcial e registro de itens danificados. |
| 📊 **Relatórios** | Situação do inventário e empréstimos pendentes, com **exportação para CSV** (compatível com Excel). |
| 👥 **Usuários** | Gerenciamento de usuários do sistema, restrito a administradores. |
| 🕵️ **Auditoria** | Log de todas as ações relevantes: quem fez, o quê e quando. |

## 🛠️ Tecnologias

**Back-end**
- **C# / .NET 9**: versão mais recente do .NET
- **ASP.NET Core MVC**: aplicação web com renderização no servidor (Razor Views)
- **Entity Framework Core 9**: ORM com abordagem **Code-First** e **Migrations**
- **Pomelo.EntityFrameworkCore.MySql**: provider do EF Core para MySQL
- **BCrypt.Net-Next**: hash seguro de senhas

**Banco de dados**
- **MySQL 8.4 (LTS)**

**Front-end**
- **Razor Views** + **Bootstrap 5** + **jQuery** + **Font Awesome**
- Todas as bibliotecas vêm com o projeto, então ele funciona sem internet (em redes internas/isoladas).

**Infraestrutura / DevOps**
- **Docker** com **multi-stage build** (imagem final contém só o runtime, sem SDK)
- **Docker Compose** orquestrando aplicação + banco, com *healthcheck* e volumes persistentes
- Configuração por **variáveis de ambiente** (`.env`), seguindo os princípios do **12-Factor App**

## 🏛️ Arquitetura e padrões de projeto

### MVC (Model-View-Controller)
Separação clara entre **Models** (entidades de domínio), **Views** (Razor) e **Controllers** (fluxo das requisições).

### Repository Pattern
Todo o acesso a dados fica em repositórios (`Data/`), cada um com sua **interface**
(`IItemRepository`, `IEmprestimoRepository`, ...). Os controllers não conhecem o EF Core diretamente, o que:
- desacopla a regra de negócio da persistência;
- facilita testes (os repositórios podem ser substituídos por *mocks*);
- concentra as consultas em um único lugar.

### Injeção de Dependência (DI)
Repositórios e `DbContext` são registrados no container nativo do ASP.NET Core (`AddScoped`) e injetados
via construtor, seguindo o **Princípio da Inversão de Dependência (o "D" do SOLID)**.

### ViewModels
As views recebem **ViewModels** específicos (`ViewModels/`), e não as entidades do banco diretamente.
Assim, a tela recebe apenas os dados de que precisa e o modelo de domínio fica protegido.

### Code-First com Migrations
O esquema do banco é gerado a partir das classes C#. As **migrations** versionam a evolução do banco junto
com o código e são **aplicadas automaticamente** na inicialização da aplicação.

### Soft Delete
Registros não são apagados fisicamente: são marcados como inativos (`Status = "I"`). Isso preserva o histórico
de empréstimos e movimentações e a integridade referencial.

### Auditoria
Cada operação relevante gera um registro em `LogsAuditoria` com o usuário responsável, a ação, os detalhes
e a data/hora. Isso dá rastreabilidade completa.

### Programação assíncrona
Acesso a dados com `async/await`, liberando threads do servidor durante operações de I/O.

## 🔒 Segurança

- Senhas armazenadas com **BCrypt** (hash + salt), nunca em texto puro
- **Autenticação por cookie** (`HttpOnly`, `SameSite=Lax`) com expiração
- **Autorização baseada em papéis (RBAC)**: `[Authorize]` em todos os módulos e `[Authorize(Roles = "Administrador")]` na gestão de usuários
- Usuários inativos são bloqueados no login
- **Chaves de Data Protection persistidas** em volume, para que as sessões continuem válidas após reinícios do container
- **Segredos fora do código**: credenciais vêm de variáveis de ambiente, e o `.env` não é versionado
- Container da aplicação roda com **usuário não-root**
- O banco de dados **não é exposto** fora da rede interna do Docker
- Páginas de erro amigáveis em produção, sem stack trace exposto

## 🚀 Como executar

### Deploy com Docker

**Pré-requisito:** [Docker](https://docs.docker.com/get-docker/) com Docker Compose v2.

```bash
# 1. Clone o repositório
git clone https://github.com/<seu-usuario>/robotics-lab-inventory.git
cd robotics-lab-inventory

# 2. Crie o arquivo de configuração e defina as senhas
cp .env.example .env
#    (edite o .env; use apenas letras, números, "-" e "_" nas senhas)

# 3. Suba a aplicação e o banco
docker compose up -d --build

# 4. Acompanhe a inicialização
docker compose logs -f app
```

Na primeira execução, a aplicação aguarda o MySQL ficar pronto, **cria o banco via migrations** e
**cria o usuário administrador** definido no `.env`.

Acesse **http://localhost:8080** e entre com `ADMIN_LOGIN` / `ADMIN_SENHA`.

#### Variáveis de ambiente (`.env`)

| Variável | Descrição | Padrão |
|---|---|---|
| `APP_PORT` | Porta em que o sistema fica acessível | `8080` |
| `MYSQL_DATABASE` | Nome do banco | `LabRoboticaDB` |
| `MYSQL_USER` / `MYSQL_PASSWORD` | Usuário e senha usados pela aplicação | — |
| `MYSQL_ROOT_PASSWORD` | Senha root do MySQL | — |
| `ADMIN_LOGIN` / `ADMIN_SENHA` | Administrador criado na primeira inicialização | `admin` / — |

> ⚠️ As variáveis `MYSQL_*` são lidas **somente na criação do volume** do banco.

#### Comandos úteis

```bash
docker compose ps                 # status dos containers
docker compose logs -f app        # logs da aplicação
docker compose up -d --build      # atualizar após mudanças no código
docker compose down               # parar (os dados são mantidos)
docker compose down -v            # parar e APAGAR todos os dados
```

## 👤 Autor

Desenvolvido por **[Leonardo Minguini Sanga]([https://github.com/<seu-usuario>](https://www.linkedin.com/in/leonardo-minguini-sanga-386336224/))** para o Laboratório de Robótica do *Instituto Ferderal de São Paulo - Campus Votuporanga*.

[![LinkedIn](https://img.shields.io/badge/LinkedIn-0A66C2?logo=linkedin&logoColor=white)](https://www.linkedin.com/in/leonardo-minguini-sanga-386336224/)
