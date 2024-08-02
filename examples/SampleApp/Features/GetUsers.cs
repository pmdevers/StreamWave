using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32.SafeHandles;
using SampleApp.Domain;
using SampleApp.Infrastructure;

namespace SampleApp.Features;

public static class GetUsers
{
    public static void MapGetSamples(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/samples", Handle);
    }

    private static Ok<List<SampleState>> Handle([FromServices] SampleDBContext context)
    {
        return TypedResults.Ok(context.Set<SampleState>().AsQueryable().ToList());
    }
}
