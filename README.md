# financial-exposure

## Sobre o Projeto

**Financial Exposure** é uma solução para **cálculo e monitoramento de exposição financeira em tempo real**.  
Seu objetivo é acompanhar limites de exposição por ativo com base em ordens enviadas, simulando parte do ambiente de um sistema de negociação.

A aplicação recebe como **input** uma interface web (via Blazor) onde é possível **enviar ordens**. Essas ordens são processadas por um serviço que calcula a exposição financeira e determina, por exemplo, se o envio ultrapassa os limites internos configurados. As ordens de compra aumentam a exposição e as de venda diminuem a exposição.

Você pode acompanhar o limite de exposição através de uma API disponível no serviço `OrderGenerator`.

Este repositório contém a solução `FinancialExposure`, que consiste em composta por três projetos:

- **`OrderGenerator`**: Serviço implementado em Blazor que contém a interface para envio de ordens.
- **`OrderAccumulator`**: Serviço utilizado para calcular a exposição financeira das ordens enviadas + API para acompanhamento de exposição financeira. 
- **`OrderAppHost`**: Serviço em .NET Aspire que contém ferramentas para gerir os serviços desta solução. 

## Dependências

Certifique-se de que as seguintes dependências estejam instaladas em sua máquina:

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)


## Como executar na máquina local

1. Clone o repositório:

   ```bash
   git clone https://github.com/flavioalbuquerque/financial-exposure.git
   cd financial-exposure

2. Restaure as dependências do projeto:
   ```
   dotnet restore src/FinancialExposure.sln
   ```

3. Execute o `OrderAppHost` para iniciar os serviços:
   ```
   dotnet run --project src/OrderAppHost
   ```

## Como iniciar o orquestrador da solução
O `OrderAppHost` utiliza o .NET Aspire para gerenciar os serviços desta solução. Aqui estão os passos para usar o Aspire:

1. Certifique-se de que o `OrderAppHost` está em execução:
   ```
   dotnet run --project src/OrderAppHost
   ```

2. Acesse o painel do .NET Aspire no navegador. Por padrão, ele estará disponível no seguinte endereço:
   ```
   http://localhost:15219
   ```

3. No painel do .NET Aspire, você pode:
- **Gerenciar serviços**: Iniciar, parar ou reiniciar os serviços da solução;
- **Monitorar logs**: Visualizar logs em tempo real;
- **Testar endpoints**: Usar ferramentas integradas para testar os endpoints da API.

   Painel do .NET Aspire
   ![Painel do .NET Aspire](./docs/images/aspire-recursos.png)

   Exemplo de monitoramento de log
   ![Painel do .NET Aspire](./docs/images/aspire-order-accumulator-logs.png)

## Testando a interface para envio de ordens
A interface `OrderGenerator` estará disponível no seguinte endereço:
```
http://localhost:5079
```

Tela para envio de ordem
![Tela para envio de ordem](./docs/images/order-generator-enviar-ordem.png)

Ordem enviada com sucesso
![Tela para envio de ordem](./docs/images/order-generator-ordem-enviada-com-sucesso.png)

## Testando a API
A API do `OrderAccumulator` expõe documentação interativa através de:
- **Swagger**: 
  ```
  http://localhost:5219/swagger
  ```
  ![Swagger](./docs/images/order-accumulator-swagger.png)
  
- **Scalar**:
  ```
  http://localhost:5219/scalar
  ````
  ![Scalar](./docs/images/order-accumulator-scalar.png)
  
- **ReDoc**:
  ```
  http://localhost:5219/redoc
  ```
  ![ReDoc](./docs/images/order-accumulator-redoc.png)

## Regras do serviço

### Regras
Ao receber uma nova ordem:
- O `OrderAccumulator` calcula a exposição financeira total;
- Se o envio ultrapassar o limite configurado, a ordem é rejeitada;
- Caso contrário, a ordem é aceita e armazenada para futuros cálculos.

Cálculo de exposição financeira:


**Exposição Financeira** = ∑ (preço × quantidade executada) das ordens de **compra** - ∑ (preço × quantidade executada) das ordens de **venda**

### Diagramas
Abaixo está o diagrama de sequência que ilustra o fluxo de envio de ordens:

```mermaid
sequenceDiagram
    title Envio de Ordem

    actor Cliente
    participant Generator as OrderGenerator
    participant Accumulator as OrderAccumulator

    Cliente ->> Generator: Envia ordem
    note right of Generator: Utiliza o protocolo FIX 4.4
    Generator->> Accumulator: FIX: NewOrderSingle

    Accumulator->>Accumulator: Calcula exposição financeira do ativo <BR/>considerando a Ordem enviada
    
    alt Ordem ultrapassa o limite de exposição
        Accumulator->>Generator: FIX: ExecType = Rejected
        Generator->> Cliente: Ordem rejeitada
    else Ordem dentro do limite de exposição
        Accumulator->>Accumulator: Armazena Ordem para utilizar <BR/>nos próximos cálculos <BR/>de exposição financeira
        Accumulator->>Generator: FIX: ExecType = New
        Generator->>Cliente: Ordem aceita
    end
```

## Melhorias Futuras
Funcionalidades planejadas para as próximas versões da solução:

- [ ] **Testes automatizados**  
  Inclusão de testes unitários para todas as classes de serviço e testes de integração para garantir a estabilidade do sistema.
  Observação: Priorizei os testes unitários do serviço `OrderAccumulator.ExposureService`

- [ ] **Implementar suporte completo a HTTPS**  
  Configurar os serviços para utilizar HTTPS por padrão, garantindo comunicação segura entre os componentes da solução.
  Inclui ajustes nas configurações de `applicationUrl` e a remoção da dependência da variável `ASPIRE_ALLOW_UNSECURED_TRANSPORT` para ambientes locais.

- [ ] **API OrderAccumulator: Desacoplar model do retorno da API**  
  Separar o modelo de domínio utilizado internamente (ex: entidade `Order`) dos modelos de resposta (DTOs) expostos pela API.

- [ ] **Dashboard de exposição agregada**  
  Interface visual para acompanhar a exposição financeira total em tempo real.

- [ ] **Dashboard de ordens**  
  Interface visual para acompanhar as ordens por símbolo.

- [ ] **Containerizar aplicações**  
  Criar imagens Docker para cada projeto da solução (`OrderGenerator`, `OrderAccumulator` e `OrderAppHost`) com o objetivo de padronizar a execução em diferentes ambientes para facilitar a execução local, testes, deploy em nuvem e integração com orquestradores como Kubernetes ou Docker Compose.

- [ ] **Persistência em banco de dados**  
  Armazenar ordens em um banco relacional para análise posterior.

- [ ] **Integração com mensageria**  
  Substituir chamadas diretas por mensagens assíncronas, aumentando a escalabilidade.
  Esta melhoria também consiste em isolar o serviço `OrderGenerator` para que seja capaz de receber requisições de diferentes clients.

- [ ] **Suporte a múltiplos ativos e múltiplos clientes**  
  Separar cálculos de exposição por cliente e por ativo para maior granularidade.

