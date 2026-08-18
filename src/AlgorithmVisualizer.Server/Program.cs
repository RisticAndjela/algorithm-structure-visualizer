using AlgorithmVisualizer.Server.Persistence;

const string LearningUserCookie = "asv-learning-user";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<LearningStateDatabase>();

var app = builder.Build();

var database = app.Services.GetRequiredService<LearningStateDatabase>();
await database.InitializeAsync();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();

app.MapGet("/api/learning-state", async (
    HttpContext context,
    LearningStateDatabase store,
    CancellationToken cancellationToken) =>
{
    var userId = GetOrCreateLearningUserId(context);
    var state = await store.GetAllAsync(userId, cancellationToken);
    return Results.Ok(state);
});

app.MapPut("/api/learning-state", async (
    HttpContext context,
    LearningStateWriteRequest request,
    LearningStateDatabase store,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Key) || request.Key.Length > 200)
    {
        return Results.BadRequest("A state key between 1 and 200 characters is required.");
    }

    if (request.Value is null || request.Value.Length > 65_536)
    {
        return Results.BadRequest("A state value up to 64 KiB is required.");
    }

    var userId = GetOrCreateLearningUserId(context);
    await store.UpsertAsync(userId, request.Key, request.Value, cancellationToken);
    return Results.NoContent();
});

app.MapDelete("/api/learning-state/{key}", async (
    HttpContext context,
    string key,
    LearningStateDatabase store,
    CancellationToken cancellationToken) =>
{
    var userId = GetOrCreateLearningUserId(context);
    await store.DeleteAsync(userId, key, cancellationToken);
    return Results.NoContent();
});

app.MapDelete("/api/learning-state", async (
    HttpContext context,
    LearningStateDatabase store,
    CancellationToken cancellationToken) =>
{
    var userId = GetOrCreateLearningUserId(context);
    await store.ClearAsync(userId, cancellationToken);
    return Results.NoContent();
});

app.MapFallbackToFile("index.html");

app.Run();

static string GetOrCreateLearningUserId(HttpContext context)
{
    if (context.Request.Cookies.TryGetValue(LearningUserCookie, out var existing) &&
        Guid.TryParseExact(existing, "N", out var existingId))
    {
        return existingId.ToString("N");
    }

    var userId = Guid.NewGuid().ToString("N");
    context.Response.Cookies.Append(
        LearningUserCookie,
        userId,
        new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Secure = context.Request.IsHttps,
            Expires = DateTimeOffset.UtcNow.AddYears(2),
            Path = "/"
        });

    return userId;
}

internal sealed record LearningStateWriteRequest(string Key, string Value);
