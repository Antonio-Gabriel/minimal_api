using minimapApi.Data;
using minimapApi.Model;

using MiniValidation;
using Microsoft.EntityFrameworkCore;

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

app.MapPost("/fornecedor",
    async (MinimalContextDb _context, Fornecedor fornecedor) =>
    {
        if (!MiniValidator.TryValidate(fornecedor, out var errors))
            return Results.ValidationProblem(errors);

        _context!.Fornecedores!.Add(fornecedor);
        var result = await _context!.SaveChangesAsync();

        return result > 0
            // ? Results.Created($"/fornecedor/{fornecedor.Id}", fornecedor)
            ? Results.CreatedAtRoute("GetFornecedorPorId", new { id = fornecedor.Id }, fornecedor)
            : Results.BadRequest("Houve um problema ao salvar o registro");

    })
    .ProducesValidationProblem()
    .Produces<Fornecedor>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest)
    .WithName("PostFornecedor")
    .WithTags("Fornecedor");

app.MapPut("/fornecedor/{id}",
    async (Guid id, MinimalContextDb _context, Fornecedor fornecedor) =>
    {
        var fornecedorDb = await _context!.Fornecedores!.FindAsync(id);
        if (fornecedorDb == null) return Results.NotFound();

        if (!MiniValidator.TryValidate(fornecedor, out var errors))
            return Results.ValidationProblem(errors);

        _context!.Fornecedores!.Update(fornecedor);
        var result = await _context!.SaveChangesAsync();

        return result > 0
            ? Results.Ok("Fornecedor atualizado com sucesso")
            : Results.BadRequest("Houve um problema ao atualizar o registro");
    })
    .ProducesValidationProblem()
    .Produces(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status400BadRequest)
    .WithName("PutFornecedor")
    .WithTags("Fornecedor");

app.MapDelete("/fornecedor/{id}",
    async (Guid id, MinimalContextDb _context) =>
    {
        var fornecedor = await _context!.Fornecedores!.FindAsync(id);
        if (fornecedor == null) return Results.NotFound();

        _context!.Fornecedores!.Remove(fornecedor);
        var result = await _context!.SaveChangesAsync();

        return result > 0
            ? Results.Ok("Fornecedor deletado com sucesso")
            : Results.BadRequest("Houve um problema ao deletar o registro");
    })
    .Produces(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status400BadRequest)
    .WithName("DeleteFornecedor")
    .WithTags("Fornecedor");

app.Run();

