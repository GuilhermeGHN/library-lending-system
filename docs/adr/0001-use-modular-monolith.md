[English](0001-use-modular-monolith.md) | [Português](pt-BR/0001-use-modular-monolith.md)

# ADR-001: Use a modular monolith

## Context
The case needs commands, queries, relational persistence, projections, and background processing, but has one small domain and no independent scaling evidence.

## Decision
Use one solution with explicit Domain, Application, Infrastructure, API, and Worker projects. Deploy one API and one worker.

## Alternatives Considered
Microservices, one worker per event, and a single project without dependency boundaries.

## Consequences
The code remains navigable and independently testable without distributed ownership/operations. A future split remains possible at existing boundaries.
