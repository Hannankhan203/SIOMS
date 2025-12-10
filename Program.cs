using Microsoft.EntityFrameworkCore;
using SIOMS.Data;
using SIOMS.Services;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IStockMovementService, StockMovementService>();

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