[English](../0002-use-transactional-outbox.md) | [Português](0002-use-transactional-outbox.md)

# ADR-002: Utilizar outbox transacional sem broker

## Context
Escritas MySQL e projeções MongoDB não formam um dual write atômico. Não se pode perder a intenção de sincronização após confirmar um empréstimo.

## Decision
Persistir estado e evento state-based numa transação MySQL. Um worker consulta, repete e aplica projeções MongoDB idempotentes por versão.

## Alternatives Considered
Dual write direto, transação distribuída ou broker imediato.

## Consequences
A entrega é at-least-once e o MongoDB eventualmente consistente. O outbox é recuperável com pouca infraestrutura, mas polling e retenção precisam de operação.
