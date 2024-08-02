using Microsoft.EntityFrameworkCore;
using SampleApp.Domain;
using SampleApp.Features;
using SampleApp.Infrastructure;
using StreamWave;
using StreamWave.EntityFramework;
using StreamWave.Extensions;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SampleDBContext>(x =>
{    
    x.UseInMemoryDatabase("test");
}, ServiceLifetime.Transient);

builder.Services.AddSingleton(JsonSerializerOptions.Default);
builder.Services.AddScoped<IEventSerializer, DefaultSerializer>();

builder.Services
    .AddAggregate<SampleState, Guid>((id) => new SampleState { Id = id })
    .WithEntityFramework<SampleDBContext, SampleState, Guid>()
    .HandleChangeName();

var app = builder.Build();

app.MapGetSamples();
app.MapChangeName();

await app.RunAsync();
