using Polly;
using Polly.Registry;
using Polly.Retry;
using Polly.Simmy;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.OpenApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddResiliencePipeline("default", static builder =>
{
    builder.AddRetry(new RetryStrategyOptions
    {
        MaxRetryAttempts = 4,
        Delay = TimeSpan.FromSeconds(1),
        BackoffType = DelayBackoffType.Exponential
    });
    builder.AddChaosFault(0.5, () => new InvalidOperationException("Chaos strategy injection!"));
});

var app = builder.Build();
app.MapOpenApi();

var logger = LoggerFactory.Create(config =>
{
    config.AddConsole();
}).CreateLogger("Program");

var users = new List<Models.User>();

app.MapGet("/user/{id}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(user);
});

app.MapGet("/users", (ResiliencePipelineProvider<string> pipelineProvider) =>
{
    List<Models.User> results = new();
    try
    {
        pipelineProvider.GetPipeline("default").Execute(() =>
        {
            logger.LogInformation("Executing pipeline...");
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred executing the pipeline.");
    }
    return Results.Ok(results);
});

app.MapPost("/user", (Models.User user) =>
{
    if (users.Any(u => u.Id == user.Id))
    {
        return Results.Conflict();
    }
    users.Add(user);
    return Results.Created($"/user/{user.Id}", user);
});

app.Run();
