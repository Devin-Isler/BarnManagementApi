// Barn Management API - Main Program File
// This file sets up all services needed for farm, animal and product management

using BarnManagementApi.Data;
using Microsoft.EntityFrameworkCore;
using BarnManagementApi.Mapping;
using BarnManagementApi.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BarnManagementApi.Services;
using Serilog;
using Serilog.Events;
using System.IdentityModel.Tokens.Jwt;

// Create web application
var builder = WebApplication.CreateBuilder(args);

// Setup logging
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration)
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) 
        .MinimumLevel.Information()
        .Enrich.FromLogContext() 
        .WriteTo.Console() // Write to console
        .WriteTo.File("Logs/BarnLog.txt", rollingInterval: RollingInterval.Day) // Write to daily files
);

// Add API services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Setup API documentation with JWT authentication
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo {Title = "BarnManagement Api", Version = "v1"});
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                },
                Scheme = "Oauth2",
                Name = JwtBearerDefaults.AuthenticationScheme,
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

// Additional Swagger setup
builder.Services.AddSwaggerGen();

// Setup database connections
builder.Services.AddDbContext<BarnDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BarnConnection"))
);
builder.Services.AddDbContext<BarnAuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BarnAuthConnection")));


// Setup object mapping between DTOs and models
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Register data access repositories
builder.Services.AddScoped<IFarmRepository, SQLFarmRepository>();
builder.Services.AddScoped<IAnimalRepository, SQLAnimalRepository>();
builder.Services.AddScoped<IProductRepository, SQLProductRepository>();
builder.Services.AddScoped<IUserRepository, SQLUserRepository>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();

// Register background services for automated tasks
builder.Services.AddHostedService<ProductService>(); // Auto-generate products from animals
builder.Services.AddHostedService<AnimalServices>(); // Handle animal lifecycle events

// Setup user authentication and management
builder.Services.AddIdentityCore<IdentityUser>()
.AddRoles<IdentityRole>()
.AddTokenProvider<DataProtectorTokenProvider<IdentityUser>>("Farm")
.AddEntityFrameworkStores<BarnAuthDbContext>()
.AddDefaultTokenProviders();
// Configure password requirements (simplified for demo)
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireLowercase = false;
    options.Password.RequiredUniqueChars = 0;
});

// Setup JWT token authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT signing key is not configured (Jwt:Key)."))),
        ClockSkew = TimeSpan.Zero
    };
    options.SaveToken = true;
    
    // Add custom token validation to check blacklist
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnMessageReceived = async context =>
        {
            var tokenRepository = context.HttpContext.RequestServices.GetRequiredService<ITokenRepository>();
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            
            if (!string.IsNullOrEmpty(token))
            {
                // Check if the token is blacklisted
                if (tokenRepository.IsTokenBlacklisted(token))
                {
                    context.Fail("Token has been invalidated.");
                    return;
                }
                
                // Also check if the user is blacklisted by parsing the token
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadJwtToken(token);
                    var userId = jsonToken.Claims.FirstOrDefault(x => x.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    
                    if (!string.IsNullOrEmpty(userId) && tokenRepository.IsUserBlacklisted(userId))
                    {
                        context.Fail("User has been logged out.");
                        return;
                    }
                }
                catch
                {
                    // If token parsing fails, let the normal JWT validation handle it
                }
            }
            
            await Task.CompletedTask;
        }
    };
});

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); 
    app.UseSwaggerUI(); 
}

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "Handled {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
});

app.UseHttpsRedirection();


app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
