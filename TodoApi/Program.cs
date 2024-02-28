using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TodoApi;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ToDoDbContext>();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.SwaggerDoc("v1", new OpenApiInfo
{
    Title = "To Do API",
    Description = "An ASP.NET Core Web API for managing ToDo items"
}));

builder.Services.AddCors(options =>
{
    options.AddPolicy("Policy", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
 {
     c.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API V1");
 });
}
app.UseCors("Policy");



app.MapGet("/items", async (ToDoDbContext context) => Results.Ok(await context.Items.ToListAsync()));
app.MapGet("items/{id}", async (ToDoDbContext context, int id) => await context.Items.FindAsync(id));

app.MapPost("/items", async (ToDoDbContext context, Item item) =>
{
    context.Add(item);
    await context.SaveChangesAsync();
    return Results.Ok(item);
});

app.MapPut("/items/{id}", async (ToDoDbContext context, [FromBody] Item item, int id) =>
{
    var it = await context.Items.FindAsync(id);
    if (it is null) return Results.NotFound();

    it.Name = item.Name;
    it.IsComplete = item.IsComplete;

    await context.SaveChangesAsync();
    return Results.Ok();
});

app.MapDelete("/items/{id}", async (ToDoDbContext context, int id) =>
{

    if (await context.Items.FindAsync(id) is Item item)
    {
        context.Remove(item);
        await context.SaveChangesAsync();
        return Results.Ok();
    }
    return Results.NotFound();
});

app.Run();




