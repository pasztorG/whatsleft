using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Add JWT Configuration
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
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddEndpointsApiExplorer(); 
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WhatsLeftAPI", Description = "Keep track of your household finances", Version = "v1" });
});

// Add this near the top with other service registrations (before builder.Build())
builder.Services.AddCors();

// Add this near the top with other builder.Services configurations
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

// Correct order of middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WhatsLeftAPI V1");
    });
    app.UseDeveloperExceptionPage();
}

// Add CORS before authentication and authorization
app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// API Endpoints
app.MapPost("/api/auth/register", async (RegisterRequest request, ApplicationDbContext db) =>
{
    Console.WriteLine("Register endpoint hit"); // Add logging

    // Check if household with same name already exists
    var existingHousehold = await db.Households
        .FirstOrDefaultAsync(h => h.Name == request.Username);
    
    if (existingHousehold != null)
    {
        return Results.BadRequest("Username already exists");
    }

    // Create new household
    var household = new Household
    {
        Id = Guid.NewGuid().ToString(),
        Name = request.Username,
        Password = request.Password
    };

    db.Households.Add(household);
    await db.SaveChangesAsync();

    // Generate JWT token
    var claims = new[]
    {
        new Claim(ClaimTypes.Name, household.Name),
        new Claim("HouseholdId", household.Id),
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: builder.Configuration["Jwt:Issuer"],
        audience: builder.Configuration["Jwt:Audience"],
        claims: claims,
        expires: DateTime.Now.AddMinutes(30),
        signingCredentials: creds);

    return Results.Ok(new
    {
        token = new JwtSecurityTokenHandler().WriteToken(token),
        householdId = household.Id,
        householdName = household.Name
    });
});

app.MapGet("/api/financialdata", async (ApplicationDbContext db, ClaimsPrincipal user, DateTime? from, DateTime? to) =>
{
    var householdId = user.FindFirst("HouseholdId")?.Value;
    if (householdId == null) return Results.Unauthorized();

    // Convert input dates to UTC and normalize to start/end of day
    if (from.HasValue)
    {
        from = DateTime.SpecifyKind(from.Value, DateTimeKind.Utc).Date;  // Start of day
    }
    if (to.HasValue)
    {
        to = DateTime.SpecifyKind(to.Value, DateTimeKind.Utc).Date.AddDays(1).AddSeconds(-1);  // End of day
    }

    Console.WriteLine($"Querying with date range: {from} to {to}"); // Debug logging

    var query = db.FinancialData
        .Include(fd => fd.Household)
        .Include(fd => fd.Category)
        .Where(fd => fd.HouseholdId == householdId);

    // Apply date filters if provided
    if (from.HasValue)
        query = query.Where(fd => fd.Date.Date >= from.Value.Date);
    if (to.HasValue)
        query = query.Where(fd => fd.Date.Date <= to.Value.Date);

    var financialData = await query
        .Select(fd => new
        {
            fd.Id,
            fd.HouseholdId,
            fd.IsRegular,
            fd.Type,
            fd.Description,
            fd.Amount,
            fd.Date,
            fd.CreatedAt,
            fd.Household.Name,
            Category = new
            {
                fd.Category.Id,
                fd.Category.Name
            }
        })
        .ToListAsync();
    
    return Results.Ok(financialData);
})
.RequireAuthorization();

app.MapGet("/api/financialdata/{householdId}", async (string householdId, ApplicationDbContext db) =>
{
    var financialData = await db.FinancialData
        .Include(fd => fd.Household)
        .Include(fd => fd.Category)
        .Where(fd => fd.HouseholdId == householdId)
        .Select(fd => new
        {
            fd.Id,
            fd.HouseholdId,
            fd.IsRegular,
            fd.Type,
            fd.Description,
            fd.Amount,
            fd.Date,
            fd.CreatedAt,
            fd.CategoryId,
            fd.Household,
            Category = new
            {
                fd.Category.Id,
                fd.Category.Name
            }
        })
        .ToListAsync();
    
    return financialData.Any() ? Results.Ok(financialData) : Results.NotFound();
});
app.MapPost("/api/household", async (Household household, ApplicationDbContext db) => {
    db.Households.Add(household);
    await db.SaveChangesAsync();
    return Results.Created($"/api/household/{household.Id}", household);
});

// Add this before app.Run()
app.MapPost("/api/auth/login", async (LoginRequest loginRequest, ApplicationDbContext db) =>
{
    // Find the household by name instead of ID
    var household = await db.Households
        .FirstOrDefaultAsync(h => h.Name == loginRequest.Username);
    
    if (household != null && loginRequest.Password == household.Password) // In production, use proper password hashing
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, household.Name),
            new Claim("HouseholdId", household.Id),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: builder.Configuration["Jwt:Issuer"],
            audience: builder.Configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds);

        return Results.Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            householdId = household.Id,
            householdName = household.Name
        });
    }

    return Results.Unauthorized();
});

app.MapPost("/api/financialdata", async (FinancialDataRequest request, ApplicationDbContext db, ClaimsPrincipal user) =>
{
    var householdId = user.FindFirst("HouseholdId")?.Value;
    if (householdId == null) return Results.Unauthorized();

    // Find or create category
    var category = await db.Categories
        .FirstOrDefaultAsync(c => c.Name == request.CategoryName);
    
    if (category == null)
    {
        category = new Category 
        { 
            Id = Guid.NewGuid().ToString(),
            Name = request.CategoryName 
        };
        db.Categories.Add(category);
    }

    var financialData = new FinancialData
    {
        Id = Guid.NewGuid().ToString(),
        HouseholdId = householdId,
        IsRegular = request.IsRegular,
        Type = request.Type,
        Description = request.Description,
        Amount = request.Amount,
        Date = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc),
        CategoryId = category.Id,
        CreatedAt = DateTime.UtcNow
    };

    db.FinancialData.Add(financialData);
    await db.SaveChangesAsync();

    return Results.Created($"/api/financialdata/{financialData.Id}", financialData);
})
.RequireAuthorization();

app.MapDelete("/api/financialdata/{id}", async (string id, ApplicationDbContext db, ClaimsPrincipal user) =>
{
    var householdId = user.FindFirst("HouseholdId")?.Value;
    if (householdId == null) return Results.Unauthorized();

    var financialData = await db.FinancialData
        .FirstOrDefaultAsync(fd => fd.Id == id && fd.HouseholdId == householdId);

    if (financialData == null) return Results.NotFound();

    db.FinancialData.Remove(financialData);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.RequireAuthorization();

app.MapPut("/api/financialdata/{id}", async (string id, FinancialDataRequest request, ApplicationDbContext db, ClaimsPrincipal user) =>
{
    var householdId = user.FindFirst("HouseholdId")?.Value;
    if (householdId == null) return Results.Unauthorized();

    var financialData = await db.FinancialData
        .FirstOrDefaultAsync(fd => fd.Id == id && fd.HouseholdId == householdId);

    if (financialData == null) return Results.NotFound();

    // Find or create category
    var category = await db.Categories
        .FirstOrDefaultAsync(c => c.Name == request.CategoryName);
    
    if (category == null)
    {
        category = new Category 
        { 
            Id = Guid.NewGuid().ToString(),
            Name = request.CategoryName 
        };
        db.Categories.Add(category);
    }

    // Update the financial data
    financialData.IsRegular = request.IsRegular;
    financialData.Type = request.Type;
    financialData.Description = request.Description;
    financialData.Amount = request.Amount;
    financialData.Date = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc);
    financialData.CategoryId = category.Id;

    await db.SaveChangesAsync();
    return Results.Ok(financialData);
})
.RequireAuthorization();

app.Run();

// Don't forget to add this class
public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class FinancialDataRequest
{
    public bool IsRegular { get; set; }
    public FinancialType Type { get; set; }
    public string Description { get; set; }
    public int Amount { get; set; }
    public DateTime Date { get; set; }
    public string CategoryName { get; set; }
}

public class RegisterRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}
