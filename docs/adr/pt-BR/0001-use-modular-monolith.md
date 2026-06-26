[English](../0001-use-modular-monolith.md) | [Português](0001-use-modular-monolith.md)

# ADR-001: Utilizar um monólito modular

## Context
O case exige comandos, consultas, persistência relacional, projeções e processamento em background, mas possui domínio pequeno e nenhuma evidência de escala independente.

## Decision
Usar uma solução com projetos Domain, Application, Infrastructure, API e Worker. Publicar uma API e um worker.

## Alternatives Considered
Microserviços, um worker por evento e projeto único sem limites de dependência.

## Consequences
O código permanece navegável/testável sem custo distribuído. Uma divisão futura continua possível nos limites existentes.
