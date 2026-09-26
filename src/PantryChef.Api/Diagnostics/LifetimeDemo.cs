namespace PantryChef.Api.Diagnostics;

public sealed class SingletonOp { public Guid Id { get; } = Guid.NewGuid(); }
public sealed class  ScopedOp   { public Guid Id { get; } = Guid.NewGuid(); }
public sealed class TransientOp { public Guid Id { get; } = Guid.NewGuid(); }

// A "primary contructor" (C# 12): the parameters are injected by DI
public sealed class LifetimeReporter(SingletonOp s, ScopedOp sc, TransientOp t)
{
    public object Report() => new { Singleton = s.Id, Scoped = sc.Id, Transient = t.Id };
}