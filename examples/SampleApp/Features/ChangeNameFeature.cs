
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SampleApp.Domain;
using StreamWave;

namespace SampleApp.Features;

public static class ChangeNameFeature
{
    public static void MapChangeName(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/{id}/{firstname}/{lastname}", Handle);
    }

    private static async Task<Results<Ok<SampleState>, BadRequest<ValidationMessage[]>>> Handle(
        [FromServices] IAggregateManager<SampleState, Guid> manager,
        [FromRoute] Guid id,
        [FromRoute] string firstname,
        [FromRoute] string lastname
        )
    {
        var aggregate = await manager.LoadAsync(id);

        await aggregate.ChangeName(firstname, lastname);
        
        if (!aggregate.IsValid)
        {
            return TypedResults.BadRequest(aggregate.Messages);
        }

        await manager.SaveAsync(aggregate);

        return TypedResults.Ok(aggregate.State);
    }

    public static Task ChangeName(this IAggregate<SampleState, Guid> aggregate, string firstname, string lastname) 
        => aggregate.ApplyAsync(new NameChanged(firstname, lastname));    

    public static IAggregateBuilder<SampleState, Guid> HandleChangeName(this IAggregateBuilder<SampleState, Guid> builder)
        => builder.WithApplier<NameChanged>((s, e) => {
             s.Lastname = e.Lastname;
             s.Firstname = e.Firstname;
             return s;
         });
}

public record NameChanged(string Firstname, string Lastname);
