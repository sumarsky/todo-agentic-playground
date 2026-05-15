using Microsoft.AspNetCore.Diagnostics;
using BackendApi.Application;
using BackendApi.Infrastructure;
using BackendApi.Application.UseCases;
using BackendApi.Contracts;
using BackendApi.Mappers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost3000", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000", "http://127.0.0.1:3000", "http://localhost:5173")
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

// Todo Endpoints
app.MapPost("/todos", (TodoCreateRequest request, CreateTodoUseCase useCase) =>
{
    try
    {
        var todo = useCase.Execute(request.Title);
        return Results.Created($"/todos/{todo.Id}", TodoMapper.ToResponse(todo));
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new ErrorResponse(400, ex.Message));
    }
}).WithName("CreateTodo");

app.MapGet("/todos", (ListTodosUseCase useCase, bool? completed, string? search) =>
{
    var todos = useCase.Execute(completed, search);
    return Results.Ok(TodoMapper.ToResponses(todos));
}).WithName("ListTodos");

app.MapPut("/todos/{id}/title", (Guid id, TodoUpdateTitleRequest request, UpdateTitleUseCase useCase) =>
{
    try
    {
        var todo = useCase.Execute(id, request.Title);
        if (todo == null)
            return Results.NotFound(new ErrorResponse(404, "Todo not found"));
        return Results.Ok(TodoMapper.ToResponse(todo));
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new ErrorResponse(400, ex.Message));
    }
}).WithName("UpdateTodoTitle");

app.MapPut("/todos/{id}/toggle", (Guid id, ToggleCompletedUseCase useCase) =>
{
    var todo = useCase.Execute(id);
    if (todo == null)
        return Results.NotFound(new ErrorResponse(404, "Todo not found"));
    return Results.Ok(TodoMapper.ToResponse(todo));
}).WithName("ToggleTodoCompleted");

app.MapDelete("/todos/{id}", (Guid id, DeleteTodoUseCase useCase) =>
{
    useCase.Execute(id);
    return Results.NoContent();
}).WithName("DeleteTodo");

app.MapDelete("/todos", (HttpContext context, BulkDeleteTodoUseCase useCase) =>
{
    var ids = context.Request.Query["ids"]
        .SelectMany(x => (x ?? string.Empty).Split(','))
        .Where(x => Guid.TryParse(x.Trim(), out _))
        .Select(x => Guid.Parse(x.Trim()))
        .ToList();

    if (ids.Count == 0)
        return Results.BadRequest(new ErrorResponse(400, "No valid IDs provided"));

    useCase.Execute(ids);
    return Results.NoContent();
}).WithName("BulkDeleteTodos");

app.Run();
