using FilmAPI.Data;
using FilmAPI.Services;
using FilmAPI.Endpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.RateLimiting;
using Scalar.AspNetCore;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Register TMDB HTTP client service as a singleton
builder.Services.AddSingleton<ITMDBHttpClientService, TMDBHttpClientService>();
builder.Services.AddSingleton<IRequestValidationService, RequestValidationService>();

// Configure rate limiting
builder.Services.AddRateLimiter(options =>
{
	options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
	options.AddFixedWindowLimiter("TMDBPolicy", configure =>
	{
		configure.PermitLimit = 100;
		configure.Window = TimeSpan.FromMinutes(1);
		configure.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
		configure.QueueLimit = 2;
	});
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
if (builder.Environment.IsDevelopment())
{
	builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}
//accediamo alla stringa di connessione
var connectionString = builder.Configuration.GetConnectionString("FilmAPIConnection")
	?? throw new InvalidOperationException("Connection string 'FilmAPIConnection' not found.");
// Versione esplicita del server MariaDB (es. immagine Docker mariadb:lts 12.3.3).
// Si preferisce a ServerVersion.AutoDetect(connectionString) perché AutoDetect apre una connessione
// al DB già all'avvio/build dell'app, facendo fallire comandi offline come `dotnet ef` se il container è spento.
var serverVersion = new MariaDbServerVersion(new Version(12, 3, 3));
// In alternativa, se il server MariaDB è sempre raggiungibile e si vuole rilevare automaticamente la versione:
// var serverVersion = ServerVersion.AutoDetect(connectionString);
//Aggiungiamo il servizio database tramite EF Core con MariaDB
builder.Services.AddDbContext<FilmDbContext>(opt =>
{
	opt.UseMySql(connectionString, serverVersion);

	if (builder.Environment.IsDevelopment())
	{
		opt.LogTo(Console.WriteLine, LogLevel.Information)
			.EnableSensitiveDataLogging()
			.EnableDetailedErrors();
	}
});

var app = builder.Build();

// Configure the HTTP request pipeline.
//qui mettiamo i middleware
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference();
	app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

// Serve static files after Scalar

// Middleware per file statici

// 1. Configura il middleware per servire index.html dalla cartella pages alla root
app.UseDefaultFiles(new DefaultFilesOptions
{
	FileProvider = new PhysicalFileProvider(
		Path.Combine(builder.Environment.WebRootPath, "pages")
	),
	RequestPath = ""
});

// 2. Middleware per file statici in wwwroot (CSS, JS, ecc.)
app.UseStaticFiles();

// 3. Middleware di fallback per cercare file in wwwroot/pages
app.UseStaticFiles(new StaticFileOptions
{
	FileProvider = new PhysicalFileProvider(
		Path.Combine(builder.Environment.WebRootPath, "pages")
	)
});

app.UseRateLimiter();
// routing per le API
//--------------------Endpoints management--------------------
app
.MapGroup("/api")
.MapRegistaEndpoints()
.MapFilmEndpoints()
.MapProiezioniEndpoints()
.MapCinemaEndpoints()
.MapTMDBProxyEndpoints()
.WithTags("Public API");

//--------------------Endpoints management--------------------

app.Run();