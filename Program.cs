using Microsoft.EntityFrameworkCore;
using minimapApi.Data;
using minimapApi.Model;

var builder = WebApplication.CreateBuilder(args);

string mysqlConnection = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContextPool<MinimalContextDb>(
    options => options.UseMySql(mysqlConnection, ServerVersion.AutoDetect(mysqlConnection))
);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/", () => "Minimal api").WithTags("Start");
app.MapGet("/fornecedores",
    async (MinimalContextDb _context) => await _context!.Fornecedores!.ToListAsync()
)
.WithName("GetFornecedor")
.WithTags("Fornecedor");

app.MapGet("/fornecedor/{id}",
    async (Guid id, MinimalContextDb _context) =>
        await _context!.Fornecedores!.FindAsync(id)
            is Fornecedor fornecedor
                ? Results.Ok(fornecedor)
                : Results.NotFound())
    .Produces<Fornecedor>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("GetFornecedorPorId")
    .WithTags("Fornecedor");


app.Run();

