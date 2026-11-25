using AziendaAPI.Data;
using AziendaAPI.Endpoints;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
//adding services to the container
if (builder.Environment.IsDevelopment())
{
	//il servizio AddDatabaseDeveloperPageExceptionFilter andrebbe usato solo in fase di testing e non in produzione.
	builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}
var connectionString = builder.Configuration.GetConnectionString("AziendaAPIConnection");
builder.Services.AddDbContext<AziendaDbContext>(option => option.UseSqlServer(connectionString));
var app = builder.Build();
//app.UseHttpsRedirection();

//adding middleware for OpenAPI and Scalar UI in Development only
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference();
}

using (var serviceScope = app.Services.CreateScope())

{
	var context = serviceScope.ServiceProvider.GetRequiredService<AziendaDbContext>();
	context.Database.Migrate();
    //vedere IN ALTERNATIVA al comando context.Database.Migrate() anche
    //context.Database.EnsureCreated();

}

//app.UseHttpsRedirection();
//app.MapGet("/", () => "Hello World!");
//I metodi seguenti verranno implementati in classi di estensione (descritte più avanti)
app.MapAziendaEndpoints();
app.MapProdottoEndpoints();
app.MapSviluppatoreEndpoints();
app.MapSviluppaProdottoEndpoints();

app.Run();
