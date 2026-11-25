# Utilizzo dei database in ASP.NET con EF Core

- [Utilizzo dei database in ASP.NET con EF Core](#utilizzo-dei-database-in-aspnet-con-ef-core)
	- [Progetto PizzaStore v1](#progetto-pizzastore-v1)
		- [Utilizzo del database InMemory](#utilizzo-del-database-inmemory)
		- [Utilizzo del database Sqlite](#utilizzo-del-database-sqlite)
		- [Uso del database MySQL/MariaDb - PizzaStore v2](#uso-del-database-mysqlmariadb---pizzastore-v2)

## Progetto PizzaStore v1

Si crei un progetto di Minimal ASP.NET Core in uno dei modi visti in precedenza, ad esempio, con il `C# Dev Kit`.

Si aggiungano i pacchetti (dalla shell posizionata sulla cartella del progetto):

```ps1
# Aggiunta di Pacchetti NuGet
 dotnet add package Microsoft.EntityFrameworkCore.InMemory
 dotnet add package Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore
 dotnet add package Microsoft.AspNetCore.OpenApi
 dotnet add package Scalar.AspNetCore
```

Si noti che, nel caso in cui si utilizzi il template `web`, ossia il template per applicazioni ASP.NET Core vuoto, occorre esplicitamente installare anche il pacchetto `Microsoft.AspNetCore.OpenApi`; nel caso si utilizzi il template `webapi`, il pacchetto per il supporto ad `OpenApi` è già installato.

Si modifichi il codice prodotto dal template come segue:

```cs
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//adding services to the container
if (builder.Environment.IsDevelopment())
{
	builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}

var app = builder.Build();

app.UseHttpsRedirection();

//adding middleware for OpenAPI and Scalar UI in Development only
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference();
}

//creations of API Endpoint Routes
app.MapGet("/", () => "Hello World!");
app.Run();
```

Si modifichi il file `launchSettings.json` come segue, con le impostazioni per far partire il browser direttamente sulle pagine di Scalar:

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "http://localhost:5028",
      "launchUrl": "scalar",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "https://localhost:7293;http://localhost:5028",
      "launchUrl": "scalar",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

### Utilizzo del database InMemory

Si crei il file `Pizza.cs` nella cartella `Model`, con il seguente contenuto:

```cs
//file Pizza.cs
namespace PizzaStoreV1.Model;
public class Pizza
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}
```

Si aggiunga la classe `PizzaDb` nella cartella `Data`:

```cs
using Microsoft.EntityFrameworkCore;

namespace PizzaStoreV1.Data;


// class PizzaDb : DbContext
// {
// 	public PizzaDb(DbContextOptions options) : base(options) { }
// 	public DbSet<Pizza> Pizzas => Set<Pizza>();
// }

//use Primary Constructor: https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/primary-constructors
class PizzaDb(DbContextOptions options) : DbContext(options)
{
	public DbSet<Pizza> Pizzas => Set<Pizza>();
}
```

Si modifichi il file `Program.cs` come segue:

```cs
using Microsoft.EntityFrameworkCore;
using PizzaStoreV1.Model;
using PizzaStoreV1.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//adding services to the container
if (builder.Environment.IsDevelopment())
{
	builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}

builder.Services.AddDbContext<PizzaDb>(options => options.UseInMemoryDatabase("items")); // 👈 si aggiunga il DbContext per l'accesso al database 

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
```

Si testi il programma con Scalar per verificarne il corretto funzionamento.

### Utilizzo del database Sqlite

Si modifichi l’applicazione, usando `SQLite` come Data Provider di `EntityFramework`. Per l'utilizzo di `SQLite` in combinazione con `EntityFramework`, occorre installare i seguenti pacchetti:

```ps1
# Aggiunta di Pacchetti NuGet
 dotnet add package Microsoft.EntityFrameworkCore.Sqlite
 dotnet add package Microsoft.EntityFrameworkCore.Design
```

Nel file `Program.cs` si modifichi il codice con le istruzioni indicate di seguito:

```cs
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
```

Si modifichi il file `appsettings.json`, con i campi per la connection string:

```json
{
  "ConnectionStrings": {
    "Pizzas": "Data Source=Pizzas.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

Si modifichi il file `PizzaDb.cs` per aggiungere il codice che popola il database in fase di migrazione. Questa parte non è strettamente necessaria, ma in questo caso è stata aggiunta al solo scopo di avere il database già popolato alla partenza dell'applicazione.

```cs
using Microsoft.EntityFrameworkCore;
using PizzaStoreV1.Model;

namespace PizzaStoreV1.Data;


// class PizzaDb : DbContext
// {
// 	public PizzaDb(DbContextOptions options) : base(options) { }
// 	public DbSet<Pizza> Pizzas { get; set; }=null!;
// }

//use Primary Constructor: https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/primary-constructors
class PizzaDb(DbContextOptions options) : DbContext(options)
{
	public DbSet<Pizza> Pizzas { get; set; }=null!;
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Pizza>().HasData(
				new Pizza { Id = 1, Name = "Montemagno", Description = "Pizza shaped like a great mountain" },
				new Pizza { Id = 2, Name = "The Galloway", Description = "Pizza shaped like a submarine, silent but deadly" },
				new Pizza { Id = 3, Name = "The Noring", Description = "Pizza shaped like a Viking helmet, where's the mead" }
			);
	}

}
```

:memo: Per operare con `EntityFrameworkCore` da command line, occorre aver installato il pacchetto `dotnet-ef`, descritto nella [documentazione Microsoft](https://learn.microsoft.com/en-us/ef/core/cli/dotnet):

```ps1
dotnet tool install --global dotnet-ef
# nel caso in cui occorra installare una versione specifica, ad esempio la 11.0.0
dotnet tool install --global dotnet-ef --version 11.0.0
# nel caso in cui occorra effettuare un aggiornamento di dotnet-ef
dotnet tool update --global dotnet-ef
# nel caso in cui occorra effettuare la rimozione di dotnet-ef
dotnet tool uninstall dotnet-ef --global
```

Dopo aver salvato tutti i file (in VS Code è presente la funzione `Auto Save`), si effettui una migrazione e un aggiornamento del database.

```ps1
# Shell posizionata sulla cartella del progetto
dotnet ef migrations add InitialMigrate
dotnet ef database update  
```

A questo punto è possibile far partire nuovamente il progetto, per verificare che funzioni tutto.

### Uso del database MySQL/MariaDb - PizzaStore v2

Si crei una nuova versione del progetto `PizzaStore`, denominato `PizzaStoreV2`, all'interno della stessa soluzione. Per l'aggiunta del progetto alla soluzione si può procedere con le funzionalità di `C# Dev Kit`, oppure con i comandi della `.NET CLI`, come visto in precedenza. In entrambi i casi il template da utilizzare è `webapi`.

> :memo: **Passaggio da Pomelo a Microting (`.NET 10+` e `.NET 11`):** il pacchetto storico `Pomelo.EntityFrameworkCore.MySql` si è fermato alla versione `9.0.0` (compatibile solo con EF Core 9). La manutenzione ufficiale è proseguita con gli stessi autori, stessa licenza MIT e stesso repository ([`microting/Pomelo.EntityFrameworkCore.MySql`](https://github.com/microting/Pomelo.EntityFrameworkCore.MySql)) sotto il nuovo nome **`Microting.EntityFrameworkCore.MySql`**. Il namespace e le API C# (`UseMySql(...)`, `MariaDbServerVersion`, `ServerVersion`) sono **identici**:
>
> - **`.NET 11` (`net11.0`)**: `Microting.EntityFrameworkCore.MySql` `11.0.0` (porta transitivamente `MySqlConnector` `3.0.0`, senza avvisi `NU1902`).
> - **`Microsoft.EntityFrameworkCore.Design`**: va sempre allineato alla stessa major (`11.x`) del provider e configurato con `PrivateAssets="all"` (impostato di default con `dotnet add package`).
> - **Pacchetti opzionali**: esistono solo se servono colonne JSON (`Microting.EntityFrameworkCore.MySql.Json.Microsoft` / `.Json.Newtonsoft`) o tipi geografici (`Microting.EntityFrameworkCore.MySql.NetTopologySuite`).

Si installino (dalla shell posizionata sulla cartella del nuovo progetto) i pacchetti per `.NET 11`:

```ps1
# 👇 serve per avere informazioni diagnostiche per EntityFrameworkCore
# è usato quando si utilizzano i middleware come:
#   app.UseDeveloperExceptionPage();
#   app.UseDatabaseErrorPage();
dotnet add package Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore

# 👇 serve per il supporto a OpenApi (se si usa il template webapi è già installato)
dotnet add package Microsoft.AspNetCore.OpenApi

# 👇 serve per la visualizzazione interattiva della documentazione OpenAPI con Scalar
dotnet add package Scalar.AspNetCore

# 👇 serve per effettuare il Design, ossia il progetto del database tramite migrazioni (deve avere la stessa major del provider)
dotnet add package Microsoft.EntityFrameworkCore.Design

# 👇 provider EF Core per MySQL / MariaDB (continuazione ufficiale di Pomelo per .NET 10/11)
# https://github.com/microting/Pomelo.EntityFrameworkCore.MySql
# https://learn.microsoft.com/en-us/ef/core/providers/?tabs=dotnet-core-cli
dotnet add package Microting.EntityFrameworkCore.MySql
```

> :memo: **Nota su Docker e MariaDB:** avviando il database con `mariadb:lts` in Docker, è possibile verificare la versione esatta del server con `docker exec <nome_container> mariadb --version` (es. `12.3.3-MariaDB`) da riportare poi in `new MariaDbServerVersion(...)`.

Si copi il contenuto di `Program.cs` e delle cartelle `Model` e `Data` nel progetto `PizzaStoreV2`, modificando opportunamente i namespace, con il nome del nuovo progetto. A questo punto, occorre modificare la parte di codice relativa al database provider e sostituire le istruzioni relative a `Sqlite`, con quelle relative a `MariaDb`. Di seguito si riportano i file del progetto con le modifiche apportate.

File `appsettings.json`

```json
{
  "ConnectionStrings": {
    "PizzasV2": "Server=localhost;port=3306;Database=pizza_store;User Id=root;Password=root;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

```cs
//file PizzaDb.cs
using Microsoft.EntityFrameworkCore;
using PizzaStoreV2.Model;

namespace PizzaStoreV2.Data;
// class PizzaDb : DbContext
// {
// 	public PizzaDb(DbContextOptions options) : base(options) { }
// 	public DbSet<Pizza> Pizzas => Set<Pizza>();
// }

//use Primary Constructor: https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/primary-constructors
class PizzaDb(DbContextOptions options) : DbContext(options)
{
	public DbSet<Pizza> Pizzas { get; set; } = null!;
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Pizza>().HasData(
				new Pizza { Id = 1, Name = "Montemagno", Description = "Pizza shaped like a great mountain" },
				new Pizza { Id = 2, Name = "The Galloway", Description = "Pizza shaped like a submarine, silent but deadly" },
				new Pizza { Id = 3, Name = "The Noring", Description = "Pizza shaped like a Viking helmet, where's the mead" }
			);
	}

}
```

```cs
// file Pizza.cs
namespace PizzaStoreV2.Model;
public class Pizza
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}
```

```cs
//file Program.cs
using Microsoft.EntityFrameworkCore;
using PizzaStoreV2.Model;
using PizzaStoreV2.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
// 👇 si aggiunga la connection string
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

// 👇 versione esplicita del server MariaDB (es. immagine Docker mariadb:lts 12.3.3).
// Si preferisce a ServerVersion.AutoDetect(connectionString) perché AutoDetect apre una connessione al DB
// già durante builder.Build(), facendo fallire `dotnet ef migrations add` o `--no-connect` se il container è spento.
var serverVersion = new MariaDbServerVersion(new Version(12, 3, 3));
// In alternativa, se il server MariaDB è sempre raggiungibile e si vuole rilevare automaticamente la versione:
// var serverVersion = ServerVersion.AutoDetect(connectionString);

// 👇 configurazione del provider per EF Core nel caso di MariaDb con il pacchetto Microting
builder.Services.AddDbContext<PizzaDb>(dbContextOptions =>
{
	dbContextOptions.UseMySql(connectionString, serverVersion);

	if (builder.Environment.IsDevelopment())
	{
		// Opzioni utili in sviluppo per il debug delle query SQL, da non abilitare in produzione
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
```
