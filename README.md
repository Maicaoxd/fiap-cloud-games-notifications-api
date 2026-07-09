# FIAP Cloud Games - NotificationsAPI

Microsservico responsavel por consumir eventos da plataforma FIAP Cloud Games e simular o envio de notificacoes por e-mail escrevendo logs no console.

Este repositorio faz parte da Fase 2 do Tech Challenge e representa o microsservico independente de notificacoes.

## Responsabilidades

- Consumir `UserCreatedEvent` publicado pela UsersAPI.
- Simular e-mail de boas-vindas para novos usuarios.
- Futuramente consumir `PaymentProcessedEvent` publicado pela PaymentsAPI.
- Futuramente simular e-mail de confirmacao de compra quando o pagamento for aprovado.
- Expor endpoints de health para validacao local, Docker e Kubernetes.

## Tecnologias

- .NET 10
- ASP.NET Core Minimal API
- RabbitMQ
- MassTransit
- Swagger / OpenAPI
- Docker
- xUnit, NSubstitute e Shouldly

## Estrutura

```text
src/NotificationsAPI/
  Consumers/       Consumers MassTransit dos eventos de integracao.
  Contracts/       Contratos de eventos compartilhados entre microsservicos.
  Health/          Checagens de prontidao do servico.
  Options/         Options tipadas para configuracoes externas.
  Program.cs       Configuracao da API, Swagger, MassTransit e health endpoints.

tests/NotificationsAPI.Tests/
  Consumers/       Testes dos consumers.
  Health/          Testes dos health checkers.
  Options/         Testes das options.
```

## Variaveis de ambiente

| Variavel | Finalidade | Padrao local |
|---|---|---|
| `RabbitMq__Host` | Host do RabbitMQ usado pelo MassTransit. | `localhost` |
| `RabbitMq__Port` | Porta TCP usada pelo MassTransit e pelo health check do RabbitMQ. | `5672` |
| `RabbitMq__VirtualHost` | Virtual host do RabbitMQ. | `/` |
| `RabbitMq__Username` | Usuario do RabbitMQ. | `guest` |
| `RabbitMq__Password` | Senha do RabbitMQ. | `guest` |
| `ASPNETCORE_ENVIRONMENT` | Ambiente de execucao da API. | `Development` |

Configuracao local padrao em `src/NotificationsAPI/appsettings.json`:

```json
{
  "RabbitMq": {
    "Host": "localhost",
    "Port": 5672,
    "VirtualHost": "/",
    "Username": "guest",
    "Password": "guest"
  }
}
```

Observacao: os segredos deste repositorio sao apenas para desenvolvimento local academico.

## Subir dependencia local

O arquivo `docker-compose.dev.yml` sobe apenas o RabbitMQ com Management UI:

```powershell
cd C:\Projetos\FIAP\Projetos\fiap-cloud-games-notifications-api

docker compose -f docker-compose.dev.yml up -d
```

Validar container:

```powershell
docker ps
```

RabbitMQ Management:

```text
http://localhost:15672
usuario: guest
senha: guest
```

Se voce ja estiver usando o RabbitMQ do repositorio UsersAPI na porta `5672`, nao suba outro RabbitMQ ao mesmo tempo. Nesse caso, deixe apenas um broker rodando e a NotificationsAPI vai consumir dele.

## Executar a API localmente

```powershell
dotnet run --project src\NotificationsAPI\NotificationsAPI.csproj
```

URLs locais padrao:

```text
Swagger: http://localhost:5007/swagger
Health:  http://localhost:5007/health
Live:    http://localhost:5007/health/live
Ready:   http://localhost:5007/health/ready
```

O endpoint `/health/live` confirma que o processo da API esta de pe.
O endpoint `/health` e o `/health/ready` tentam abrir conexao TCP com o RabbitMQ configurado.

## Swagger

A NotificationsAPI nao possui endpoints de negocio neste momento, porque seu trabalho principal e consumir eventos do RabbitMQ. Mesmo assim, o Swagger esta habilitado para expor os endpoints operacionais, como health checks.

```text
http://localhost:5007/swagger
```

## Evento consumido

### `UserCreatedEvent`

Publicado pela UsersAPI apos o cadastro de usuario.

```json
{
  "userId": "guid",
  "name": "string",
  "email": "string",
  "createdAt": "datetime"
}
```

Quando o evento chega, o consumer `UserCreatedEventConsumer` registra no console uma mensagem simulando envio de e-mail de boas-vindas.

## Validar o fluxo com UsersAPI

1. Suba um RabbitMQ unico. Pode ser pelo `docker-compose.dev.yml` da UsersAPI ou da NotificationsAPI.
2. Rode a NotificationsAPI:

```powershell
dotnet run --project src\NotificationsAPI\NotificationsAPI.csproj
```

3. Rode a UsersAPI em outro terminal.
4. Cadastre um usuario na UsersAPI.
5. Confira o terminal da NotificationsAPI. O log esperado sera parecido com:

```text
E-mail de boas-vindas enviado para Nome (email@dominio.com). UserId: <guid>
```

No RabbitMQ Management, a fila criada pelo consumer costuma aparecer com nome baseado no consumer, por exemplo `user-created-event` ou `user-created-event-consumer`, dependendo da topologia configurada pelo MassTransit.

## Testes

Executar a suite:

```powershell
dotnet test NotificationsAPI.slnx -m:1
```

Os testes atuais cobrem:

- consumo/log do `UserCreatedEvent`;
- defaults de `RabbitMqOptions`;
- checagem de conexao TCP usada pelo health readiness.

## Docker da API

Build da imagem:

```powershell
docker build -t fiap-cloud-games-notifications-api:latest .
```

Executar a imagem apontando para um RabbitMQ rodando no host:

```powershell
docker run --rm -p 8083:8080 `
  -e RabbitMq__Host=host.docker.internal `
  -e RabbitMq__Port=5672 `
  -e RabbitMq__VirtualHost=/ `
  -e RabbitMq__Username=guest `
  -e RabbitMq__Password=guest `
  fiap-cloud-games-notifications-api:latest
```

Acessar:

```text
http://localhost:8083/swagger
http://localhost:8083/health
```

## Kubernetes

Este microsservico deve ter manifests em `k8s/` com:

- `Deployment`
- `Service`
- `ConfigMap`
- `Secret`

Comandos esperados quando os manifests forem adicionados:

```powershell
kubectl apply -f .\k8s
kubectl get pods
kubectl get services
kubectl logs deployment/notifications-api
```

No cluster, configure `RabbitMq__Host` com o nome do Service do RabbitMQ, por exemplo `rabbitmq`, e mantenha os dados sensiveis em `Secret`.

## Problemas comuns

### `/health` retorna `Unhealthy`

Verifique se o RabbitMQ esta rodando e se a porta esta correta:

```powershell
docker ps
```

Tambem confira `RabbitMq__Host` e `RabbitMq__Port`.

### Nao aparece mensagem parada na fila

Isso e normal quando o consumer esta funcionando. O RabbitMQ entrega a mensagem para a NotificationsAPI e, depois do ACK, a mensagem sai da fila. Para acompanhar o fluxo, olhe:

- aba `Queues and Streams` no RabbitMQ Management;
- contadores `Ready`, `Unacked`, `Ack` e `Deliver/get`;
- logs da NotificationsAPI.

### Erro de porta ocupada ao subir RabbitMQ

Voce provavelmente ja tem outro RabbitMQ usando `5672` ou `15672`. Pare um deles ou use apenas o broker do repositorio UsersAPI durante os testes integrados.

