using IndieSphere.Application;
using IndieSphere.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

var connectionString = builder.Configuration.GetConnectionString("IndieSphereDb");

builder.Services
    .AddProblemDetails()
    .AddOpenApi()
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(connectionString)
    .AddControllers()
    ;

// Add JWT validation for Google tokens
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://accounts.google.com";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = "accounts.google.com",
            ValidAudience = builder.Configuration["Authentication:Google:ClientId"],
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorUI", policy =>
    {
        policy.WithOrigins("https://localhost:7233")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.MapDefaultEndpoints();
app.MapControllers();

app.UseAuthentication();
app.UseAuthorization();
app.UseCors("BlazorUI");

app.Run();
