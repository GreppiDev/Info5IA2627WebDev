
using System.ComponentModel;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using PizzaStoreV3.Data;
using PizzaStoreV3.Model;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
// 👇 si aggiunga la connection string
var connectionString = builder.Configuration.GetConnectionString("Pizzas") ?? "Data Source=DefaultPizzas.db";

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// builder.Services.AddOpenApi("internal", options => 
// {
// 	options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
// }); // Document name is internal

//adding services to the container
if (builder.Environment.IsDevelopment())
{
	//il servizio AddDatabaseDeveloperPageExceptionFilter andrebbe usato solo in fase di testing e non in produzione.
	builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}
builder.Services.AddProblemDetails();
// 👇 si aggiunga il DbContext per l'accesso al database
//builder.Services.AddDbContext<PizzaDb>(options => options.UseInMemoryDatabase("items"));  
// 👇 si modifichi il DbContext per l'accesso al database 
builder.Services.AddDbContext<PizzaDb>(options => options.UseSqlite(connectionString));
var app = builder.Build();


//app.UseHttpsRedirection();

//adding middleware for OpenAPI and Scalar UI in Development only
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference();
}

//Adds a middleware to the pipeline that will catch exceptions, log them, and re-execute
//the request in an alternate pipeline. The request will not be re-executed if
//the response has already started.
// app.UseExceptionHandler(exceptionHandlerApp
// 	=> exceptionHandlerApp.Run(async context
// 		=> await Results.Problem()
// 					 .ExecuteAsync(context)));
// //Adds a StatusCodePages middleware with the specified handler that checks for responses with 
// //status codes between 400 and 599 that do not have a body.
// app.UseStatusCodePages(async statusCodeContext
// 	=> await Results.Problem(statusCode: statusCodeContext.HttpContext.Response.StatusCode)
// 				 .ExecuteAsync(statusCodeContext.HttpContext));

app.MapGet("/exception", () =>
{
	throw new InvalidOperationException("Sample Exception");
});


//creations of API Endpoint Routes
app.MapGet("/", () => "Hello World!");

//in questo caso non cambia nulla nel documento di OpenAPI prodotto
//perché il return type verrebbe inferito automaticamente
//app.MapGet("/pizzas", async (PizzaDb db) => await db.Pizzas.ToListAsync()).Produces<IList<Pizza>>();

//in questo caso non cambia nulla nel documento di OpenAPI prodotto
//perché il return type verrebbe inferito automaticamente
//app.MapGet("/pizzas", [ProducesResponseType<List<Pizza>>(200)] async (PizzaDb db) => await db.Pizzas.ToListAsync());

//in questo caso non cambia nulla nel documento di OpenAPI prodotto
//perché il return type verrebbe inferito automaticamente
app.MapGet("/pizzas", async (PizzaDb db) => {
	
	var pizzas =await db.Pizzas.ToListAsync();
	return TypedResults.Ok(pizzas);
	});

var pizzaGroup = app.MapGroup("pizza");
pizzaGroup.MapPost("/", async (PizzaDb db,[Description("The pizza to post") ] Pizza pizza) =>
{
	db.Pizzas.Add(pizza);
	await db.SaveChangesAsync();
	return Results.Created($"/pizza/{pizza.Id}", pizza);
});
// app.MapGet("/pizza/{id}", async Task<Results<Ok<Pizza>, NotFound>> (int id, PizzaDb db) =>
// {
// 	return await db.Pizzas.FindAsync(id)
// 			is Pizza pizza
// 				? TypedResults.Ok(pizza)
// 				: TypedResults.NotFound();
// });
app.MapGet("/pizza/{id}", async (int id, PizzaDb db) =>
	{
		if(id<0) 
		{
				return Results.BadRequest();
		}
		return	await db.Pizzas.FindAsync(id)
		is Pizza pizza
		? Results.Ok(pizza)
		: Results.NotFound();
	})
   .Produces<Pizza>(StatusCodes.Status200OK)
   .Produces(StatusCodes.Status404NotFound)
   .Produces(StatusCodes.Status400BadRequest);
//pizzaGroup.MapGet("/{id}", async (PizzaDb db, int id) => await db.Pizzas.FindAsync(id));

pizzaGroup.MapPut("/{id}", async (PizzaDb db, Pizza updatePizza, int id) =>
{
	var pizza = await db.Pizzas.FindAsync(id);
	if (pizza is null) return Results.NotFound();
	pizza.Name = updatePizza.Name;
	pizza.Description = updatePizza.Description;
	await db.SaveChangesAsync();
	return Results.NoContent();
});
pizzaGroup.MapDelete("/{id}", async (PizzaDb db, int id) =>
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

app.MapGet("/extension-methods", () => "Hello world!")
  .WithSummary("This is a summary.")
  .WithDescription(
			"""
			## PizzaStore API

			Restituisce informazioni sullo stato del servizio **PizzaStore**.

			- Supporta risposte in formato `text/plain`
			- Documentazione generata con **OpenAPI** e visualizzata in **Scalar**
			""");
//👇 i metadati summary e Description inseriti sull'Endpoint /attributes 
//sono presenti sia nella documentazione di OpenAPI che in quella di Swagger
//perché è stato usato il metodo di estensione WithOpenApi
// app.MapGet("/attributes",
//   [EndpointSummary("This is a summary.")]
// [EndpointDescription("This is a description.")]
// () => "Hello world!").WithOpenApi();

//WithTags oppure Tags permettono di inserire i tag sia in OpenAPI che in Swagger
// app.MapGet("/extension-methods", () => "Hello world!")
//   .WithTags("todos", "projects");
// app.MapGet("/attributes",[Tags("todos", "projects")]() => "Hello world!");

//👇 per assicurare coerenza tra OpenAPI e Swagger è opportuno aggiungere .WithOpenApi()
// app.MapGet("/extension-methods", () => "Hello world!").WithName("FromExtensionMethods").WithOpenApi();
// app.MapGet("/attributes",[EndpointName("FromAttributes")]() => "Hello world!").WithOpenApi();


app.MapGet("/attributes",([Description("This is a description.")] string name) => "Hello world!");
app.Run();


