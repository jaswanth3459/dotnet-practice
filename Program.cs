using EmployeeAdminPortal.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value.Errors.Count > 0)
                .SelectMany(e => e.Value.Errors.Select(x => x.ErrorMessage))
                .ToList();

            return new BadRequestObjectResult(new { errors });
        };
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var cosmosConnectionString = builder.Configuration.GetConnectionString("CosmosConnection");
var cosmosDatabaseName = builder.Configuration["CosmosDb:DatabaseName"] ?? "EmployeeDb";

builder.Services.AddDbContext<ApplicationDbContex>(options =>
    options.UseCosmos(
        cosmosConnectionString!,
        databaseName: cosmosDatabaseName
    )
    .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
    .EnableDetailedErrors(builder.Environment.IsDevelopment())
);

builder.Services.AddSingleton<Container>(sp =>
{
    var cosmosClientOptions = new CosmosClientOptions
    {
        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    };

    var cosmosClient = new CosmosClient(cosmosConnectionString, cosmosClientOptions);
    var database = cosmosClient.GetDatabase(cosmosDatabaseName);
    var containerName = builder.Configuration["CosmosDb:Containers:Employees"] ?? "Employees";
    return database.GetContainer(containerName);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContex>();
    await dbContext.Database.EnsureCreatedAsync();
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
