using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TodoApi.Endpoints;
using TodoApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
if (builder.Environment.IsDevelopment())
{
	builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}

builder.Services.AddDbContext<TodoDb>(opt =>
	opt.UseInMemoryDatabase("TodoList"));

var app = builder.Build();

// Configure the HTTP request pipeline.
//qui mettiamo i middleware
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference();
}

// Serve static files after Scalar

// Middleware per file statici

// 1. Configura il middleware per servire index.html dalla cartella root
app.UseDefaultFiles();

// 2. Middleware per file statici in wwwroot (CSS, JS, ecc.)
app.UseStaticFiles();


if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
}
//-------- Start of API management ------------
app
.MapGroup("")
.MapTodosEndpoints()
.WithTags("Public API");
//-------- End of API management ------------

//avvia l'applicazione
app.Run();


