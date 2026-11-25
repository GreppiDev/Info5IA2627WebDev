using Microsoft.Extensions.FileProviders;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


var app = builder.Build();

// Configure the HTTP request pipeline.
//qui mettiamo i middleware
if (app.Environment.IsDevelopment())
{
    //permette di ottenere una rotta dove vedere la documentazione delle API secondo lo standard OpenAPI
    app.MapOpenApi();
    app.MapScalarApiReference();

    app.UseDeveloperExceptionPage();
}

//app.UseHttpsRedirection();

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

// Add this before app.Run()
app.MapGet("/api/config", (IConfiguration config) =>
{
    return Results.Ok(new
    {
        backendUrl = config["BackendApi:BaseUrl"]
    });
});

app.Run();


