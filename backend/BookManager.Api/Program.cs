using System.Text.Json.Serialization;
using BookManager.Api.ExceptionHandling;
using BookManager.Api.Hubs;
using BookManager.Application;
using BookManager.Application.Events;
using BookManager.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

const string BrowserClientsPolicy = "BrowserClients";

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SupportNonNullableReferenceTypes();
    options.NonNullableReferenceTypesAsRequired();
});
builder.Services.AddOpenApi();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddSignalR(options => options.StatefulReconnectBufferSize = 100_000);
builder.Services.AddSingleton<IBookEventNotifier, SignalRBookEventNotifier>();

if (allowedOrigins.Length > 0)
{
    builder.Services.AddCors(options =>
        options.AddPolicy(
            BrowserClientsPolicy,
            // SignalR needs AllowCredentials, which cannot be combined with AllowAnyOrigin.
            policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()
        )
    );
}

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

if (allowedOrigins.Length > 0)
{
    app.UseCors(BrowserClientsPolicy);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.MapHub<BookEventsHub>("/hubs/book-events", options => options.AllowStatefulReconnects = true);

app.Run();
