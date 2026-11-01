# Dentus Clinic — API

Backend do sistema de gerenciamento da clínica odontológica Dentu's Clinic.

ASP.NET Core · .NET 10 · Entity Framework Core 10 · SQL Server

Atende três clientes: o sistema web da clínica, o aplicativo do paciente e o Swagger.

## Arquitetura

```text
api/
├── Controllers/     # Endpoints HTTP e autorização por perfil
├── Services/        # Regras de negócio
├── Repositories/    # Acesso ao banco
├── Data/            # AppDbContext e DataSeeder
├── Models/          # Entidades
├── DTOs/            # Request e Response
├── Migrations/      # Esquema do banco, triggers e procedure
├── Middleware/      # Tratamento central de erros
├── Attributes/      # Validações próprias (CPF, datas, cargo)
└── Enums/
```

O fluxo é sempre **Controller → Service → Repository → DbContext**. Controller não fala com o banco, e regra de negócio não mora no controller.

Toda resposta segue o mesmo envelope:

```json
{ "sucesso": true, "mensagem": "...", "dados": { } }
```

## Como rodar

### Pré-requisitos

- .NET SDK 10
- SQL Server acessível (o projeto usa um contêiner Docker local)

### Configuração

As credenciais ficam no `.env`, que **não é versionado**. Copie o modelo e preencha:

```bash
cp .env.example .env
```

| Variável | Para quê |
|---|---|
| `DB_CONNECTION_STRING` | conexão com o SQL Server |
| `JWT_SECRET_KEY` | chave que assina os tokens |
| `FIREBASE_CREDENTIALS_PATH` | caminho do JSON da conta de serviço do Firebase |

O `FIREBASE_CREDENTIALS_PATH` é opcional: sem ele a API sobe normalmente e as notificações vão para o log em vez do celular. Útil para quem só mexe no sistema web.

### Subir

```bash
dotnet run --urls http://0.0.0.0:5081
```

O `--urls http://0.0.0.0:5081` é necessário **quando o aplicativo mobile for usado**. Por padrão a API escuta só em `localhost`, e `localhost` no celular é o próprio celular — o app nunca alcança o seu computador. Para mexer só no sistema web, `dotnet run` basta.

Ao subir, a API aplica as migrations pendentes e popula o banco com dados de exemplo.

Swagger em `http://localhost:5081/swagger`, apenas em ambiente de desenvolvimento. Faça login, copie o token e cole no botão **Authorize** — só o token, sem escrever "Bearer".

### Usuários de exemplo

Criados pelo `DataSeeder` na primeira execução:

| Perfil | E-mail | Senha |
|---|---|---|
| Administrador | `admin@dentusclinic.com` | `Admin@123` |
| Dentista | `ana.souza@dentusclinic.com` | `Dentista@123` |
| Recepcionista | `julia.mendes@dentusclinic.com` | `Secretaria@123` |

Pacientes não têm senha fixa: ao serem cadastrados recebem uma senha temporária de 6 dígitos, que aparece na tela de confirmação do cadastro.

## Banco de dados

O esquema é gerenciado **somente por migrations**. Nunca altere o banco pela mão.

```bash
dotnet ef migrations add NomeDaMigration
dotnet ef database update
```

Se o build falhar reclamando que o arquivo está em uso, é a própria API rodando — pare ela antes.

### Regras que vivem no banco

Três triggers e uma procedure, criados por migration:

| Objeto | O que garante |
|---|---|
| `trg_ImpedirConflitoHorarioDentista` | um dentista não tem duas consultas no mesmo horário |
| `trg_ImpedirAlteracaoConsultaRealizada` | consulta encerrada não pode ser alterada |
| `trg_CancelamentoAutomaticoRecusa` | recusa do paciente cancela a consulta |
| `sp_ConfirmarPresenca` | confirma ou recusa a presença, validando o estado da consulta |

As mesmas regras existem na camada de serviço. Elas estão no banco para valerem também fora da aplicação: acesso direto, script de manutenção ou integração futura.

Por causa desses triggers, a entidade `Consulta` declara `HasTrigger` no mapeamento. Sem isso o EF Core usa cláusula `OUTPUT` nas gravações, e o SQL Server a proíbe em tabela com trigger.

### Prontuário é append-only

Plano de tratamento não é editado. Editar cria uma **nova versão**, ligada à anterior por `PlanoAnteriorId`, e só a mais recente tem `VersaoAtual = true`. As listagens devolvem apenas a versão atual; o histórico completo sai em `GET /api/planos/{id}/historico`.

Cada versão guarda o dentista que a registrou, em `IdDentista`. Não existe exclusão de plano.

## Notificações

O paciente é avisado 24 horas antes da consulta e precisa confirmar ou recusar. Enquanto não responder, o aviso é reenviado de hora em hora, até o horário da consulta.

Como funciona:

- Uma rotina em segundo plano roda a cada 5 minutos e cuida dos reenvios e das consultas que entram na janela de 24h.
- Ao agendar uma consulta que já está dentro da janela, o aviso sai na hora, sem esperar a rotina.
- O envio usa o Firebase. Sem a credencial configurada, cai na implementação que só escreve no log.
- Token recusado em definitivo pelo Firebase — aplicativo desinstalado, token de outro projeto — desativa o dispositivo, para não tentar para sempre.

Para testar, agende uma consulta com horário entre 1 e 24 horas a partir de agora.

## Perfis de acesso

Os endpoints são restritos por perfil, vindo do token JWT (válido por 8 horas).

| Perfil | Acesso |
|---|---|
| `ADMINISTRADOR` | funcionários, serviços, especialidades |
| `SECRETARIA` | agenda, pacientes, consultas |
| `DENTISTA` | agenda própria, prontuários, atendimentos |
| `PACIENTE` | apenas `/api/paciente-app/*`, sempre sobre os próprios dados |

Nos endpoints do aplicativo o id do paciente vem **sempre do token**, nunca do corpo da requisição — assim ninguém consegue ler ou alterar o cadastro de outra pessoa.

Enquanto o paciente não trocar a senha temporária, a API recusa todos os endpoints do app, menos o de troca de senha.

## Observações

- O e-mail mora só em `Logins`, onde é a credencial de acesso. Alterar o e-mail de um paciente altera o login dele.
- `Consultas` tem duas colunas de situação: `Status` é o andamento na clínica (`Agendada`, `Aguardando`, `Em Consulta`, `Encerrada`, `Cancelada`) e `StatusConfirmacao` é a resposta do paciente (`Pendente`, `Confirmada`, `Recusada`).
- Erros de regra de negócio são lançados como `InvalidOperationException` e viram HTTP 400 no middleware, com a mensagem preservada.
- Mensagens de `RAISERROR` dos triggers também chegam ao cliente com o texto original.
