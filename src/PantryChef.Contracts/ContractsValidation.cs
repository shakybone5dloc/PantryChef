using Microsoft.Extensions.DependencyInjection;

namespace PantryChef.Contracts;

public static class ContractsValidation
{
    // .NET 10's validation source generator only covers types in the assembly that calls AddValidation().
    // Calling it HERE generates validation metadata for this library's [ValidatableType] records.
    public static IServiceCollection AddContractsValidation(this IServiceCollection services) =>
        services.AddValidation();
}