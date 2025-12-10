using Microsoft.EntityFrameworkCore;
using SIOMS.Data;
using SIOMS.Services;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IStockMovementService, StockMovementService>();

// Add Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

// Configure logging with Trace Switch
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
    logging.AddEventSourceLogger();

    // Configure trace switch
    var traceSwitch = new SourceSwitch("SIOMSLogging", "Logging for SIOMS Application")
    {
        Level = SourceLevels.Warning
    };

    logging.AddTraceSource(traceSwitch, new TextWriterTraceListener("sioms.log"));
});

// Add background service for daily reconciliation
builder.Services.AddHostedService<BackgroundReconciliationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// IMPORTANT: UseAuthentication must come before UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // This will create the database and tables if they don't exist
        bool created = context.Database.EnsureCreated();

        if (created)
        {
            Console.WriteLine("Database was created successfully.");
        }
        try
        {
            context.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
                BEGIN
                    CREATE TABLE Users (
                        UserId INT PRIMARY KEY IDENTITY(1,1),
                        FullName NVARCHAR(100) NOT NULL,
                        Email NVARCHAR(100) NOT NULL UNIQUE,
                        Password NVARCHAR(100) NOT NULL,
                        Role NVARCHAR(50) DEFAULT 'User',
                        IsActive BIT DEFAULT 1,
                        CreatedDate DATETIME DEFAULT GETDATE(),
                        LastLoginDate DATETIME NULL
                    )
                    PRINT 'Users table created successfully.'
                END
                ELSE
                BEGIN
                    PRINT 'Users table already exists.'
                END
            ");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating Users table: {ex.Message}");
        }

        Console.WriteLine("Users table created.");

        // Seed initial data
        await SeedData.InitializeAsync(context);

        // Don't initialize product subscriptions in constructor
        // We'll handle low stock alerts differently
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

app.Run();