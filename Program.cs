using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.Text;
using TaskManagement.API.Repositories;
using TaskManagement.API.Services;
using TaskManagement.API.Hubs;
using TaskManagement.API.Configurations;
using FluentValidation;
using FluentValidation.AspNetCore;
using TaskManagement.API.Helpers.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuration bindings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDB"));

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<LoginDtoValidator>();

// MongoDB Configuration
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017";
    return new MongoClient(connectionString);
});

builder.Services.AddScoped(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var databaseName = builder.Configuration["MongoDB:DatabaseName"] ?? "TaskManagementDB";
    return client.GetDatabase(databaseName);
});

// Repository Dependencies
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IWorkItemRepository, WorkItemRepository>();
builder.Services.AddScoped<IWorkItemLogRepository, WorkItemLogRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// Service Dependencies
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IWorkItemService, WorkItemService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "your-super-secret-key-that-is-at-least-32-characters-long";
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Custom middleware
app.UseMiddleware<TaskManagement.API.Middlewares.ErrorHandlingMiddleware>();
app.UseMiddleware<TaskManagement.API.Middlewares.ValidationMiddleware>();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Custom JWT middleware (optional, since we're using built-in JWT)
// app.UseMiddleware<TaskManagement.API.Middlewares.JwtAuthenticationMiddleware>();

app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");

app.Run();