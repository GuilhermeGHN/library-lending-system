[English](0003-use-optimistic-concurrency.md) | [Português](pt-BR/0003-use-optimistic-concurrency.md)

# ADR-003: Use optimistic concurrency for aggregate updates

## Context
Concurrent requests must not borrow the same last available copy or update the same loan from stale state. The solution should not depend on a database-specific row-version type.

## Decision
Use explicit `Book.Version` and `Loan.Version` fields as EF Core concurrency tokens and map stale writes to `409 Conflict` without automatic retry.

## Alternatives Considered
Pessimistic row locks, serializable transactions, database-specific version types, and silent retries.

## Consequences
Normal operations avoid long locks and remain portable. Callers must handle a visible conflict and can retry after reading fresh state.
