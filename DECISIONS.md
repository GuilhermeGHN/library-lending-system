[English](DECISIONS.md) | [Português](DECISIONS.pt-BR.md)

# Engineering decisions

## Is this architecture suitable?

CQRS with MySQL and MongoDB is probably excessive for a small community library. It is implemented because the case explicitly asks for a relational write model, NoSQL reads, and synchronization. For a real small system I would begin with one relational database and logical command/query separation, then introduce MongoDB or a broker only after measured query, scale, or integration needs appear.

The solution is a modular monolith: one API, one worker, and clear dependency boundaries. It avoids a generic CQRS framework, generic repositories, event sourcing, shared-kernel projects, and one process per event.

## Consistency and synchronization failure

MySQL is authoritative and MongoDB is eventually consistent. Commands validate availability in MySQL, so a stale projection cannot violate the core rule. Strict read-your-writes is not guaranteed; command responses return committed write-side data instead.

Domain changes and outbox messages are persisted by the same `SaveChanges` transaction. After commit, a `BackgroundService` retries failed projections, increments attempts, records the last error, and leaves exhausted messages observable. Delivery is at least once. Projection handlers compare the incoming aggregate version with MongoDB and ignore duplicates or older events. The initial deployment assumes one worker instance; idempotency limits duplicate effects, but horizontal worker scaling would require claim/lock/lease behavior or a broker. A future independent/broker consumer should add an Inbox Pattern.

## Rule placement

- Request/command validators handle required values, length, ranges, and malformed identifiers, returning explicit `Result` failures that produce `400`.
- `Book` and `Loan` protect invariants regardless of the caller.
- Handlers load aggregates, invoke behavior, create entities/events, and coordinate a use case. Expected failures such as validation errors, missing resources, unavailable books, repeated returns, and concurrency conflicts are represented as `Result` values instead of being used as control-flow exceptions.
- Infrastructure owns EF mappings, transactions, MySQL, MongoDB, and outbox dispatch.

Domain exceptions still exist as internal invariant guards, so entities cannot enter invalid state if a future use case forgets a pre-check. The API boundary maps `Result` errors to RFC 7807 responses, keeping expected failures explicit while leaving exceptions for unexpected failures.

## Concurrency

`Book.Version` and `Loan.Version` are explicit EF Core concurrency tokens. Concurrent requests may read the same version, but the SQL update includes the original version. One commit advances it; the other affects no row and becomes `409 Conflict`. The operation is intentionally not retried because a retry could hide a meaningful business conflict. `Book.Version` protects availability changes; `Loan.Version` protects direct loan updates such as concurrent returns.

## Persistence choice

EF Core is used because the case explicitly requests Entity Framework or NHibernate. Without that restriction, my likely preference would be Dapper + DbUp: explicit SQL, greater query control, lower abstraction, simpler runtime behavior, and migrations independent from the ORM. No real need justified mixing Dapper into this version.

MySQL was selected because it is allowed, operationally familiar, and the domain needs no vendor-specific PostgreSQL feature. The explicit version field keeps concurrency semantics portable.

Physical storage names use singular `snake_case` to match the reference codebase style: MySQL tables are `book`, `loan`, and `outbox_message`; MongoDB collections are `book` and `loan`.

## Rich versus anemic domain model

Rich entities were chosen to satisfy the requirement and make accidental invalid mutations impossible. An anemic model is still a valid alternative: rules grouped by use case can make flows easy to inspect, handlers can remain explicit, and focused services/helpers can control duplication with less behavior coupled to entities. That style has worked well in previous systems; here the evaluator's expectation and invariant-protection goal justify rich behavior.

## Outbox without a broker

The outbox removes dual-write loss at low operational cost. A broker, multiple workers, or event sourcing would add moving parts without solving a current requirement. If throughput, independent ownership, scaling, or new consumers demand it, the outbox can become a publisher to a broker and downstream consumers can own inboxes.

## AI usage

AI was used to explore architecture and generate initial code, tests, and documentation. Every incorporated change was reviewed against the constraints and remains explainable. Suggestions rejected as unnecessary included a broker in v1, multiple deployable workers, generic repositories, a generic CQRS framework, shared abstraction projects, and event sourcing.

## Operational trade-offs

Retry count and delay are configurable and bounded. Failed events stay in the outbox as the initial operational control surface; there is no dead-letter queue without a broker. A production evolution should add retention/archive policy and metrics for pending count, oldest age, failure count, lag, and processing duration. Multiple worker replicas would require a portable claim/lease strategy; the initial deployment deliberately runs one. Only the API applies EF Core migrations at startup; the worker waits for the API health check under Docker Compose and does not attempt concurrent migrations.

The custom `Result` type is intentionally small and local. A library such as FluentResults was unnecessary for this case because the project only needs success/failure, a code, a message, optional validation details, and HTTP mapping at the API boundary.
