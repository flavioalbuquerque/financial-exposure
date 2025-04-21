# financial-exposure

Este repositório contém 3 projetos:

- `OrderGenerator`: Serviço implementado em Blazor que contém a tela para envio de ordens
- `OrderAccumulator`: Long running service utilizado para calcular a exposição financeira das ordens enviadas
- `OrderAppHost`: Serviço em .Net Aspire que contém ferramentas para gerir os demais serviços

## Instruções para uso
Executar o `OrderAppHost`: `dotnet run --project src/OrderAppHost`


## Diagramas

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
    
    alt Ordem ultrapassa o limite interno
        Accumulator->>Generator: FIX: ExecType = Rejected
        Generator->> Cliente: Ordem rejeitada
    else Ordem dentro do limite
        Accumulator->>Accumulator: Armazena Ordem para utilizar <BR/>nos próximos cálculos <BR/>de exposição financeira
        Accumulator->>Generator: FIX: ExecType = New
        Generator->>Cliente: Ordem aceita
    end
```
