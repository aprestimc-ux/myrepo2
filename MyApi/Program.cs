using Microsoft.AspNetCore.Mvc;
using MyApi.Middlewares;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.RegisterHttpServices();
builder.Services.RegisterServices();

var app = builder.Build();

app.UseMiddleware<GlobalException>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.RegisterToDoItemsEndpoints(); 

app.MapGet("/products", async (NorthwindContext con) => {
    var data = 
    await con.Products
    .Select(p => new ProductDTO
    {
         Id = p.ProductId,
         Name = p.ProductName,
         Category = p.Category.CategoryName
    })
    .ToListAsync();
    return Results.Ok(data);
});



app.UseHttpsRedirection();


app.Use(async (context, next) =>
{
    await next();

    // Se l’endpoint non è trovato
    if (context.Response.StatusCode == StatusCodes.Status404NotFound && !context.Response.HasStarted)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Not Found in the fog",
            Detail = "The requested endpoint does not exist.",
            Instance = context.Request.Path
        };

        problemDetails.Extensions["TRACEID"] = traceId;

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
});

app.Run();

