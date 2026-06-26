[English](DECISIONS.md) | [Português](DECISIONS.pt-BR.md)

# Decisões de engenharia

## Esta arquitetura é adequada?

CQRS com MySQL e MongoDB provavelmente é excessivo para uma biblioteca comunitária pequena. Foi implementado porque o case pede escrita relacional, leitura NoSQL e sincronização. Num sistema pequeno real eu começaria com um banco relacional e separação lógica entre comandos e consultas, introduzindo MongoDB ou broker apenas após evidências de consulta, escala ou integração.

A solução é um monólito modular: uma API, um worker e limites claros. Evita framework CQRS genérico, repositórios genéricos, event sourcing, Shared Kernel e um processo por evento.

## Consistência e falha de sincronização

MySQL é autoritativo e MongoDB é eventualmente consistente. Comandos validam disponibilidade no MySQL, logo uma projeção defasada não viola a regra. Read-your-writes estrito não é garantido; comandos retornam o estado confirmado da escrita.

Mudanças e mensagens de outbox são persistidas no mesmo `SaveChanges`. Após o commit, o `BackgroundService` repete projeções, incrementa tentativas, registra o último erro e mantém mensagens esgotadas observáveis. A entrega é at-least-once. Handlers comparam a versão recebida com o MongoDB e ignoram duplicatas/antigas. O deploy inicial assume uma instância do worker; a idempotência limita efeitos duplicados, mas escalar workers horizontalmente exigiria claim, lock, lease ou broker. Consumidores independentes futuros devem adicionar Inbox Pattern.

## Local das regras

- Validadores de request/comando tratam obrigatoriedade, tamanho, faixa e IDs inválidos, retornando falhas explícitas com `Result` que produzem `400`.
- `Book` e `Loan` protegem invariantes independentemente do chamador.
- Handlers carregam agregados, chamam comportamentos, criam entidades/eventos e coordenam o caso de uso. Falhas esperadas como validação, recurso ausente, livro indisponível, devolução repetida e conflito de concorrência são representadas como valores `Result`, em vez de exceptions usadas como fluxo de controle.
- Infraestrutura possui mappings, transações, MySQL, MongoDB e despacho do outbox.

Exceptions de domínio ainda existem como guardas internas de invariantes, para impedir estado inválido se um caso de uso futuro esquecer uma pré-validação. A borda da API mapeia erros de `Result` para respostas RFC 7807, deixando falhas esperadas explícitas e exceptions para falhas inesperadas.

## Concorrência

`Book.Version` e `Loan.Version` são tokens explícitos do EF Core. Pedidos podem ler a mesma versão, mas o update usa a versão original. Um commit a avança; o outro não altera linha e vira `409 Conflict`. Não há retry automático, pois esconderia um conflito de negócio. `Book.Version` protege alterações de disponibilidade; `Loan.Version` protege alterações diretas no empréstimo, como devoluções concorrentes.

## Escolha de persistência

EF Core foi usado porque o case exige Entity Framework ou NHibernate. Sem essa restrição, minha preferência provável seria Dapper + DbUp: SQL explícito, controle de queries, menor abstração, runtime simples e migrations independentes do ORM. Não houve necessidade concreta de misturar Dapper aqui.

MySQL foi escolhido por ser permitido, familiar operacionalmente e suficiente. O campo de versão mantém a concorrência portável.

Os nomes físicos de armazenamento usam singular em `snake_case` para seguir o estilo do projeto de referência: as tabelas MySQL são `book`, `loan` e `outbox_message`; as coleções MongoDB são `book` e `loan`.

## Domínio rico versus anêmico

Entidades ricas atendem ao requisito e impedem mutações inválidas. Um modelo anêmico continua válido: regras por caso de uso facilitam inspeção, handlers explicitam o fluxo e serviços/helpers focados controlam duplicação com menos comportamento acoplado às entidades. Essa abordagem funcionou bem em outros sistemas; aqui a expectativa do avaliador e a proteção de invariantes justificam o modelo rico.

## Outbox sem broker

O outbox remove a perda de dual write com baixo custo operacional. Broker, múltiplos workers ou event sourcing adicionariam peças sem requisito atual. Se volume, ownership, escala ou consumidores exigirem, o outbox pode publicar num broker e consumidores podem manter inboxes.

## Uso de IA

IA foi usada para explorar arquitetura e gerar código, testes e documentação iniciais. Toda mudança incorporada foi revisada e é explicável. Foram rejeitados broker na v1, vários workers, repositórios/CQRS genéricos, projetos compartilhados excessivos e event sourcing.

## Trade-offs operacionais

Tentativas e atraso são configuráveis e limitados. Falhas permanecem no outbox, sem DLQ sem broker. Produção deve adicionar retenção/arquivo e métricas de pendências, idade, falhas, lag e duração. Réplicas do worker exigiriam claim/lease; o deploy inicial deliberadamente usa uma. Apenas a API aplica migrations EF Core ao iniciar; no Docker Compose, o worker espera o health check da API e não tenta migrations concorrentes.

O tipo `Result` customizado é propositalmente pequeno e local. Uma biblioteca como FluentResults não era necessária neste case, porque o projeto só precisa de sucesso/falha, código, mensagem, detalhes opcionais de validação e mapeamento HTTP na borda da API.
