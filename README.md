# Cirreum.Contracts

[![NuGet Version](https://img.shields.io/nuget/v/Cirreum.Contracts.svg?style=flat-square&labelColor=1F1F1F&color=003D8F)](https://www.nuget.org/packages/Cirreum.Contracts/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Cirreum.Contracts.svg?style=flat-square&labelColor=1F1F1F&color=003D8F)](https://www.nuget.org/packages/Cirreum.Contracts/)
[![GitHub Release](https://img.shields.io/github/v/release/cirreum/Cirreum.Contracts?style=flat-square&labelColor=1F1F1F&color=FF3B2E)](https://github.com/cirreum/Cirreum.Contracts/releases)
[![License](https://img.shields.io/badge/license-MIT-F2F2F2?style=flat-square&labelColor=1F1F1F)](https://github.com/cirreum/Cirreum.Contracts/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-003D8F?style=flat-square&labelColor=1F1F1F)](https://dotnet.microsoft.com/)

**The abstractions and vocabulary every Cirreum application programs against.**

## Overview

**Cirreum.Contracts** is the contract layer of the Cirreum framework — the cross-host abstractions, vocabulary, and value types a Cirreum application programs against.

Cirreum.Contracts defines:

- **Conductor — the CQRS surface** — `IDispatcher`, `IPublisher`, `IOperation` / `IOperationHandler`, `IIntercept`, the intercept contracts (HandlerPerformance, QueryCaching, Validation), builders, and settings. (Concretes — `Dispatcher`, `Publisher`, the pipeline — ship in `Cirreum.Domain`.)
- **Caching** — `ICacheService` (get-or-create + tag-based invalidation), `CacheProvider`, `CacheSettings`, `CacheExpirationSettings`
- **State** — cross-host state abstractions (`IStateContainer`, `IStateManager`, `IStatePersistence`, `IScopedNotificationState`, `ISessionState`, …) and the `Notification` shape
- **Presence** — `IUserPresenceService`, `IUserPresenceMonitor`, `UserPresence`, `PresenceStatus`
- **Profile Enrichment** — `IUserProfileEnrichmentBuilder`, `IGraphEnabledBuilder`, `IExternalGraphEnabledBuilder` — the host-agnostic builder seam any host (server, Blazor WebAssembly, …) uses to configure post-authentication profile enrichment, independent of which (or whether any) auth scheme is active; default enricher implementations ship in `Cirreum.Domain`
- **RemoteServices** — `IRemoteConnection`, connection state, `RemoteServiceOptions`
- **FileSystem + CSV** — `IFileSystem`, `ICsvFileBuilder` / `ICsvFileReader`, `CsvOptions`, `PathType`
- **Invocation** — the `HttpContext`-free invocation seam (`IInvocationContext`, `IInvocationContextAccessor`, `IInvocationConnection`, `IConnectionEstablishmentContributor`, `DisconnectInfo`)
- **Authorization pillar** — the vocabulary (`AuthorizationPolicies`, `ApplicationRoles`, `Role`, `DenyCodes`, `AuthorizationContext`) and the cross-host contracts (`IAuthorizer`, `IAuthorizationContextAccessor`, the operation / grant / resource-ACL abstractions, `[RequiresGrant]`); default implementations and FluentValidation validators live in `Cirreum.Domain`

Concerns with their own dedicated tracks (Messaging, Storage) live in their own packages and do NOT fold into Cirreum.Contracts. The Authentication event surface lives in `Cirreum.Kernel`.

## Where it fits

Cirreum.Contracts is **L2 — the contract surface**. It builds on `Cirreum.Kernel` (L1) and references the foundation peer `Cirreum.Result` as needed; it takes no other Cirreum dependency. It is a pure-contracts package: the default implementation of everything declared here ships one layer up, in `Cirreum.Domain`, along with the richer dependency tree (FluentValidation, `Microsoft.Extensions.Caching.Memory`, …) the concretes require.

## Contribution Guidelines

1. **Be conservative with new abstractions**  
   The API surface must remain stable and meaningful — Contracts is what every Cirreum app programs against; changes ripple through the entire ecosystem.

2. **Limit dependency expansion**  
   Only add foundational, version-stable dependencies. Contracts is a pure-abstractions package — concrete dependencies belong in `Cirreum.Domain`.

3. **Favor additive, non-breaking changes**  
   Breaking changes in the contract surface cascade through every dependent package and every Cirreum app. Major version bumps are rare.

4. **Include thorough unit tests**  
   All contracts and patterns should be independently testable.

5. **Document architectural decisions**  
   Context and reasoning should be clear for future maintainers.

6. **Follow .NET conventions**  
   Use established patterns from `Microsoft.Extensions.*` libraries.

## Versioning

Cirreum.Contracts follows [Semantic Versioning](https://semver.org/):

- **Major** — Breaking API changes
- **Minor** — New features, backward compatible
- **Patch** — Bug fixes, backward compatible

Given its foundational role, major version bumps are rare and carefully considered.

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

---

**Cirreum Foundation Framework**  
*Layered simplicity for modern .NET*
