using System.Text.Json.Serialization;
using Journal.Api.GrpcServices;
using Journal.Api.Pipelines;
using Journal.Domain.DataStorage;
using Journal.Infra;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000, o => o.Protocols = HttpProtocols.Http1);
    options.ListenLocalhost(5001, o => o.Protocols = HttpProtocols.Http2);
});

builder.Services.AddControllers()
       .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
builder.Services.AddGrpc(options => { options.Interceptors.Add<GrpcExceptionHandler>(); });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
                             {
                                 Title = "Journal API",
                                 Version = "v1"
                             });
});
builder.Services.Register();

WebApplication app = builder.Build();
MigrateDb(app);

app.UseMiddleware<ApiExceptionHandler>();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Journal API V1");
    options.RoutePrefix = "";
});

app.MapGrpcService<AuditLogService>();
app.MapControllers();

app.Run();

void MigrateDb(IHost host)
{
    using IServiceScope scope = host.Services.CreateScope();
    var dbMigrationEngine = scope.ServiceProvider.GetRequiredService<IDbMigrationEngine>();
    dbMigrationEngine.Migrate();
}