using Microsoft.EntityFrameworkCore;
using PizzaStoreV2.Model;
using PizzaStoreV2.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
// 👇 si aggiunga la connection string
//var connectionString = builder.Configuration.GetConnectionString("Pizzas") ?? "Data Source=DefaultPizzas.db";
var connectionString = builder.Configuration.GetConnectionString("PizzasV2")
	?? throw new InvalidOperationException("Connection string 'PizzasV2' not found.");

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//adding services to the container
if (builder.Environment.IsDevelopment())
{
	//il servizio AddDatabaseDeveloperPageExceptionFilter andrebbe usato solo in fase di testing e non in produzione.
	builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}
// 👇 si aggiunga il DbContext per l'accesso al database
//builder.Services.AddDbContext<PizzaDb>(options => options.UseInMemoryDatabase("items"));  
// 👇 si modifichi il DbContext per l'accesso al database 
//builder.Services.AddDbContext<PizzaDb>(options => options.UseSqlite(connectionString));

// Versione esplicita del server MariaDB (es. immagine Docker mariadb:lts 12.3.3).
// Si preferisce a ServerVersion.AutoDetect(connectionString) perché AutoDetect apre una connessione
// al DB già all'avvio/build dell'app, facendo fallire comandi offline come `dotnet ef` se il container è spento.
var serverVersion = new MariaDbServerVersion(new Version(12, 3, 3));
// In alternativa, se il server MariaDB è sempre raggiungibile e si vuole rilevare automaticamente la versione:
// var serverVersion = ServerVersion.AutoDetect(connectionString);
// 👇 configurazione del provider per EF Core nel caso di MariaDb con il pacchetto Microting
builder.Services.AddDbContext<PizzaDb>(dbContextOptions =>
{
	dbContextOptions.UseMySql(connectionString, serverVersion);

	if (builder.Environment.IsDevelopment())
	{
		// The following three options help with debugging, but should
		// not be enabled in production.
		dbContextOptions
			.LogTo(Console.WriteLine, LogLevel.Information)
			.EnableSensitiveDataLogging()
			.EnableDetailedErrors();
	}
});

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
//AsNoTracking(), disabilita il tracking 
//The change tracker will not track any of the entities that are returned from a LINQ query. If the entity instances are modified, this will not be detected by the change tracker and DbContext.SaveChanges() will not persist those changes to the database.

//Disabling change tracking is useful for read-only scenarios because it avoids the overhead of setting up change tracking for each entity instance. You should not disable change tracking if you want to manipulate entity instances and persist those changes to the database using DbContext.SaveChanges().
//							È utile in contesti di sola lettura 👇
app.MapGet("/pizzas", async (PizzaDb db) => await db.Pizzas.AsNoTracking().ToListAsync());


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