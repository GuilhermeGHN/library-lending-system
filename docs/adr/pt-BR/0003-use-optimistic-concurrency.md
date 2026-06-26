[English](../0003-use-optimistic-concurrency.md) | [Português](0003-use-optimistic-concurrency.md)

# ADR-003: Utilizar concorrência otimista para atualizações de agregados

## Contexto

Pedidos concorrentes não podem emprestar o mesmo último exemplar ou atualizar o mesmo empréstimo a partir de estado defasado. A solução não deve depender de tipo de versão específico do banco.

## Decisão

Usar `Book.Version` e `Loan.Version` como tokens do EF Core e mapear escrita defasada para `409 Conflict`, sem retry automático.

## Alternativas Consideradas

Locks pessimistas, transações serializáveis, versões específicas do banco e retries silenciosos.

## Consequências

Operações normais evitam locks longos e são portáveis. Clientes tratam o conflito e podem repetir após ler estado novo.
