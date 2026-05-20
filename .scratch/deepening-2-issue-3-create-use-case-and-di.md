# Issue: Create CalculateMetricsUseCase & DI Registration

## Parent

Deepening 2: Hoist Metrics Logic to Use Case

## What to build

Create the use case that orchestrates log retrieval and metrics calculation, plus the dependency injection wiring to register it.

1. **`CalculateMetricsUseCase`**: A concrete class (not an interface) that:
   - Receives `ILogStore` and `IMetricsCalculator` via constructor
   - Exposes an `Execute(TimeSpan window, CancellationToken ct = default)` method that:
     - Calculates `since = DateTime.UtcNow - window`
     - Queries logs via `ILogStore.QueryAsync(new LogFilter { Since = since }, ct)`
     - Aggregates them via `IMetricsCalculator.Calculate(logs)`
     - Returns the aggregated metrics
   - No HTTP concerns; works with domain types only

2. **`AddApplication()` extension**: A static extension method on `IServiceCollection` in `BackendApi.Application/Extensions/ServiceCollectionExtensions.cs` that:
   - Registers `CalculateMetricsUseCase` as `Scoped`
   - Registers `IMetricsCalculator` as `Singleton` mapped to `DefaultMetricsCalculator`
   - Returns the collection for chaining

## Acceptance criteria

- [ ] `BackendApi.Application/UseCases/CalculateMetricsUseCase.cs` exists with correct constructor and `Execute` method signature
- [ ] `BackendApi.Application/Extensions/ServiceCollectionExtensions.cs` exists with `AddApplication()` method
- [ ] `AddApplication()` registers both use case and calculator with correct lifetimes
- [ ] `CalculateMetricsUseCase` calculates `since` correctly: `DateTime.UtcNow - window`
- [ ] `Execute` returns the result of `_calculator.Calculate()` without modification
- [ ] Unit tests in `BackendApi.Tests/Application/CalculateMetricsUseCaseTests.cs` cover: successful query and aggregation, cancellation token propagation, empty result handling, with real `DefaultMetricsCalculator` and mocked `ILogStore`
- [ ] Compilation succeeds

## Blocked by

deepening-2-issue-2-implement-default-metrics-calculator.md
