
using Microsoft.EntityFrameworkCore;
using PizzaStoreV1.Data;
using PizzaStoreV1.Model;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
// 👈 si aggiunga la connection string
var connectionString = builder.Configuration.GetConnectionString("Pizzas") ?? "Data Source=DefaultPizzas.db";

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//adding services to the container
if (builder.Environment.IsDevelopment())
{
	//il servizio AddDatabaseDeveloperPageExceptionFilter andrebbe usato solo in fase di testing e non in produzione.
	builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}
// 👈 si aggiunga il DbContext per l'accesso al database
//builder.Services.AddDbContext<PizzaDb>(options => options.UseInMemoryDatabase("items"));  
// 👈 si modifichi il DbContext per l'accesso al database 
builder.Services.AddDbContext<PizzaDb>(options => options.UseSqlite(connectionString)); 
var app = builder.Build();


//app.UseHttpsRedirection();

//adding middleware for OpenAPI and Scalar UI in Development only
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference();
}
//creations of API Endpoint Routes
app.MapGet("/", () => "Hello World!");

app.MapGet("/pizzas", async (PizzaDb db) => await db.Pizzas.ToListAsync());

app.MapPost("/pizza", async (PizzaDb db, Pizza pizza) =>
{
	db.Pizzas.Add(pizza);
	await db.SaveChangesAsync();
	return Results.Created($"/pizza/{pizza.Id}", pizza);
});

app.MapGet("/pizza/{id}", async (PizzaDb db, int id) => await db.Pizzas.FindAsync(id));

app.MapPut("/pizza/{id}", async (PizzaDb db, Pizza updatePizza, int id) =>
{
	var pizza = await db.Pizzas.FindAsync(id);
	if (pizza is null) return Results.NotFound();
	pizza.Name = updatePizza.Name;
	pizza.Description = updatePizza.Description;
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


