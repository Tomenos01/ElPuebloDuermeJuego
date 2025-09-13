using ElPuebloDuermeDemo.Services;
using ElPuebloDuermeDemo.Domain;
using ElPuebloDuermeDemo.SignalR;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "3.0.4",
        Title = "El Pueblo Duerme API",
        Description = "API backend para el juego El Pueblo Duerme"
    });
});
    
// SignalR
builder.Services.AddSignalR();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Dependency Injection
// Domain Services
builder.Services.AddScoped<IActionResolver, ActionResolver>();
builder.Services.AddScoped<IPhaseManager, PhaseManager>();
builder.Services.AddScoped<IGameRulesValidator, GameRulesValidator>();

// Application Services
builder.Services.AddScoped<IGameService, GameService>(); // Cambiado de Singleton a Scoped para evitar el error de dependencias
builder.Services.AddSingleton<IRoleAssignmentService, RoleAssignmentService>();
builder.Services.AddScoped<IVoteService, VoteService>();
builder.Services.AddScoped<IMessagingService, MessagingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// SignalR Hubs
app.MapHub<GameHub>("/gameHub");
app.MapHub<ChatHub>("/chatHub");

app.Run();