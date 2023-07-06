using Microsoft.EntityFrameworkCore;
using PizzaStore.Data;
using PizzaStore.Models;

var builder = WebApplication.CreateBuilder(args);
// var connectionString = builder.Configuration.GetConnectionString("SQLite") ?? "Data Source=Pizzas.db";
// var connectionString = builder.Configuration.GetConnectionString("MSSQL");
// var connectionString = builder.Configuration.GetConnectionString("Postgresql");

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// builder.Services.AddDbContext<PizzaDb>(options => options.UseInMemoryDatabase("items"));
// builder.Services.AddSqlite<PizzaDb>(connectionString);
// builder.Services.AddDbContext<PizzaDb>(optionsAction=>optionsAction.UseSqlServer(connectionString));
// builder.Services.AddDbContext<PizzaDb>(options=>options.UseNpgsql(connectionString));
string defaultConnectionString = "Server=localhost;Port=5432;Database=PizzaDb;User ID=postgres;Password=xxxxx";
builder.Services.AddDbContext<PizzaDb>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgresql") ?? defaultConnectionString)
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/", () => "Hello World");
app.MapGet("/pizzas", async (PizzaDb db) => await db.Pizzas.ToListAsync());
app.MapPost("/pizza", async (PizzaDb db, Pizza pizza) =>
{
    await db.Pizzas.AddAsync(pizza);
    await db.SaveChangesAsync();
    return Results.Created($"/pizza/{pizza.Id}", pizza);
});
app.MapGet("/pizza/{id}", async (PizzaDb db, int id) => await db.Pizzas.FindAsync(id));
app.MapPut("/pizza/{id}", async (PizzaDb db, Pizza updatepizza, int id) =>
{
    var pizza = await db.Pizzas.FindAsync(id);
    if (pizza is null) return Results.NotFound();
    pizza.Name = updatepizza.Name;
    pizza.Description = updatepizza.Description;
    await db.SaveChangesAsync();
    return Results.NoContent();
});
app.MapDelete("/pizza/{id}", async (PizzaDb db, int id) =>
{
    var pizza = await db.Pizzas.FindAsync(id);
    if (pizza is null)
    {
        return Results.NotFound();
    }
    db.Pizzas.Remove(pizza);
    await db.SaveChangesAsync();
    return Results.Ok();
});
app.Run();
