[English](0002-use-transactional-outbox.md) | [Português](pt-BR/0002-use-transactional-outbox.md)

# ADR-002: Use a transactional outbox without a broker

## Context
MySQL writes and MongoDB projections cannot be an atomic dual write. Losing synchronization intent after a committed loan is unacceptable.

## Decision
Persist state and a state-based event in one MySQL transaction. A worker polls, retries, and applies version-idempotent MongoDB projections.

## Alternatives Considered
Direct dual writes, distributed transactions, or introducing a message broker immediately.

## Consequences
Delivery is at least once and MongoDB is eventually consistent. The outbox is observable/recoverable with little infrastructure, but polling and retention require operation.
