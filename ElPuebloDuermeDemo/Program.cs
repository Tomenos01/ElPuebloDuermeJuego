using ElPuebloDuermeDemo.Services;
using ElPuebloDuermeDemo.Domain;
using ElPuebloDuermeDemo.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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