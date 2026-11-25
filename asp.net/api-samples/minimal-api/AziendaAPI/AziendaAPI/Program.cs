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
var connectionString = builder.Configuration.GetConnectionString("AziendaAPIConnection")
	?? throw new InvalidOperationException("Connection string 'AziendaAPIConnection' not found.");
// Versione esplicita del server MariaDB (es. immagine Docker mariadb:lts 12.3.3).
// Si preferisce a ServerVersion.AutoDetect(connectionString) perché AutoDetect apre una connessione
// al DB già all'avvio/build dell'app, facendo fallire comandi offline come `dotnet ef` se il container è spento.
var serverVersion = new MariaDbServerVersion(new Version(12, 3, 3));
// In alternativa, se il server MariaDB è sempre raggiungibile e si vuole rilevare automaticamente la versione:
// var serverVersion = ServerVersion.AutoDetect(connectionString);
builder.Services.AddDbContext<AziendaDbContext>(opt =>
{
	opt.UseMySql(connectionString, serverVersion);

	if (builder.Environment.IsDevelopment())
	{
		// The following three options help with debugging, but should
		// not be enabled in production.
		opt.LogTo(Console.WriteLine, LogLevel.Information)
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
