using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TodoApiV2;
using TodoApiV2.Filters;
using TodoApiV2.Middlewares;
//creation of Web application builder
var builder = WebApplication.CreateBuilder(args);
//adding services to the container
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
if (builder.Environment.IsDevelopment())
{
	builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//creation of Web application
var app = builder.Build();

//adding middleware for OpenAPI and Scalar UI in Development only
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.MapGroup("/public/todos")
	.MapTodosApi()
	.WithTags("Public");

app.MapGroup("/private/todos")
	.MapTodosApi()
	.WithTags("Private");
//.RequireAuthorization();

//alcuni esempi di rotte con gruppi presi dalla documentazione con qualche adattamento
var all = app.MapGroup("");
var org = all.MapGroup("{org}");
var user = org.MapGroup("{user}");
//questa è una GET sulla rotta definita da /{org}/{user}/
//ad esempio effettuando una GET su /greppi-dev/malafronte/ la risposta è greppi-dev/malafronte
user.MapGet("", (string org, string user) => $"{org}/{user}");

var outer = app.MapGroup("/outer");
var inner = outer.MapGroup("/inner");

//rotte di /outer
outer.MapGet("/say-hello", () => "say hello!");
outer.MapGet("/hand-shake", () => "give a firm handshake!");
//rotte di inner, quindi in /outer/inner
inner.MapGet("/make-appointment", () => "See you tomorrow at 6pm!");
//rotte con default tag
//app.MapGet("/",()=>Console.WriteLine("Ciao mondo"));
app.MapGet("/", () =>
	{
		app.Logger.LogInformation("Endpoint with route /: delegate execution");
		
		return "Test of endpoint with filters";
	})
	.AddEndpointFilter<PerformanceFilter>()
	.AddEndpointFilter(async (efiContext, next) =>
	{
		app.Logger.LogInformation("Before inline filter 1");
		var result = await next(efiContext);
		app.Logger.LogInformation("After inline filter 1");
		return result;
	})
	.AddEndpointFilter(async (efiContext, next) =>
	{
		app.Logger.LogInformation("Before inline filter 2");
		var result = await next(efiContext);
		app.Logger.LogInformation("After inline filter 2");
		return result;
	})
	.AddEndpointFilter(async (efiContext, next) =>
	{
		app.Logger.LogInformation("Before inline filter 3");
		var result = await next(efiContext);
		app.Logger.LogInformation("After inline filter 3");
		return result;
	});
//local function
static string ColorName(string color) => $"Color specified: {color}!";
//nel caso in cui il colore richiesto sia Red viene restituito un messaggio d'errore 
//in tutti gli altri casi viene restituito il colore richiesto
app.MapGet("/colorSelector/{color}", ColorName)
	.AddEndpointFilter(async (invocationContext, next) =>
	{
		app.Logger.LogInformation("Inline filter on color before delegate execution");
		//accesso al primo argomento passato all'endpoint delegate
		var color = invocationContext.GetArgument<string>(0);

		if (color == "Red")
		{
			return Results.Problem("Red not allowed!");
		}
		var result = await next(invocationContext);
		app.Logger.LogInformation("Inline filter on color after delegate execution");
		return result;
	});

//aggiunta di custom middleware. Attenzione: l'ordine conta!
//👇 custom middleware 1
app.Use(async (context, next) =>
{
	app.Logger.LogInformation("Middleware 1: Prima della richiesta");
	await next(); // Passa al middleware successivo
	app.Logger.LogInformation("Middleware 1: Dopo la risposta");
	
});
//👇 custom middleware 2
app.Use(async (context, next) =>
{

	app.Logger.LogInformation("Middleware 2: Prima della richiesta");
	await next(); // Passa al middleware successivo
	app.Logger.LogInformation("Middleware 2: Dopo la risposta");

});
//👇 custom middleware 3
app.Use(async (context, next) =>
{

	app.Logger.LogInformation("Middleware 3: Prima della richiesta");
	app.Logger.LogInformation("Path: {context.Request.Path}", context.Request.Path);
	app.Logger.LogInformation("Method: {context.Request.Method}", context.Request.Method);
	var currentEndpoint = context.GetEndpoint();
	if (currentEndpoint is null)
	{
		await next(context);
		return;
	}

	app.Logger.LogInformation("Endpoint: {currentEndpoint.DisplayName}", currentEndpoint.DisplayName);
	await next(context); // Passa al middleware successivo
	app.Logger.LogInformation("Middleware 3: Dopo la risposta");

});

//👇 aggiunta del middleware LoggingMiddleware alla pipeline
app.UseMiddleware<LoggingMiddleware>();

app.Run();
