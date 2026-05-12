using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost3000", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Use CORS middleware
app.UseCors("AllowLocalhost3000");

// Global exception handler middleware
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var error = new { error = exceptionHandlerPathFeature?.Error?.Message ?? "An unexpected error occurred" };
        
        await context.Response.WriteAsJsonAsync(error);
    });
});

// 404 Not Found handler (must be after UseExceptionHandler but before MapGet)
app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == StatusCodes.Status404NotFound && context.Response.ContentType == null)
    {
        context.Response.ContentType = "application/json";
        var error = new { error = "Not found" };
        await context.Response.WriteAsJsonAsync(error);
    }
});

app.MapGet("/", () => "Hello World!");
app.MapGet("/health", () => new { status = "healthy" });
app.MapGet("/error/unhandled", () =>
{
    throw new Exception("Test unhandled exception");
});

app.Run();
