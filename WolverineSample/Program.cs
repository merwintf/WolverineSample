using Infra;
using Microsoft.EntityFrameworkCore;
using Pipeline;
using Wolverine;
using Wolverine.FluentValidation;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddWolverineHttp();

var cs = builder.Configuration.GetConnectionString("mysql")
         ?? "Server=localhost;Database=appdb;User Id=root;Password=root;TreatTinyAsBoolean=false;";
builder.Services.AddDbContext<AppDbContext>(o => o.UseMySql(cs, ServerVersion.AutoDetect(cs)));

builder.Host.UseWolverine(opts =>
{
    opts.UseFluentValidation();
    opts.Policies.AddMiddleware(typeof(Pipeline.EfTransactionMiddleware));
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapWolverineEndpoints(h => h.UseFluentValidationProblemDetailMiddleware());

// map your endpoints (e.g., Features.Books.Add.Endpoint.Map(app));
Features.Books.Add.Endpoint.Map(app);

app.Run();
