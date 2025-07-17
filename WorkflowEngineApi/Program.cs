using WorkflowEngineApi.Models;
using WorkflowEngineApi.Services;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var engine = new WorkflowEngine();

app.MapPost("/workflows", (WorkflowDefinition def) =>
{
    try
    {
        var created = engine.CreateDefinition(def);
        return Results.Created($"/workflows/{created.Id}", created);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapGet("/workflows/{id}", (string id) =>
{
    try
    {
        var def = engine.GetDefinition(id);
        return Results.Ok(def);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(ex.Message);
    }
});

app.MapPost("/workflows/{definitionId}/instances", (string definitionId) =>
{
    try
    {
        var instance = engine.StartInstance(definitionId);
        return Results.Created($"/instances/{instance.Id}", instance);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapGet("/instances/{id}", (string id) =>
{
    try
    {
        var inst = engine.GetInstance(id);
        return Results.Ok(inst);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(ex.Message);
    }
});

app.MapPost("/instances/{instanceId}/actions/{actionId}", (string instanceId, string actionId) =>
{
    try
    {
        var inst = engine.ExecuteAction(instanceId, actionId);
        return Results.Ok(inst);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.Run();
