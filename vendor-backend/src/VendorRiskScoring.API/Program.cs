var builder = WebApplication.CreateBuilder(args);

// Log
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Services
builder.Services.AddControllers();

const string myAllowSpecificOrigins = "AllowSpecificOrigin";
builder.Services.AddCors(options =>
{
    options.AddPolicy(myAllowSpecificOrigins,
        policy => policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<VendorCreateCommandValidator>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<VendorCreateCommandHandler>()
);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Vendor Risk Scoring API", Version = "v1" });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});

// HealthCheck
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

var riskMatrixPath = Path.Combine(AppContext.BaseDirectory, "RiskFactorMatrix.json");

builder.Configuration.AddJsonFile(
    riskMatrixPath,
    optional: false,
    reloadOnChange: true
);

// Options binding
builder.Services.Configure<RiskFactorMatrix>(
    builder.Configuration.GetSection("RiskFactorMatrix")
);

// Application & Infrastructure
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Vendor Risk API v1");
        options.DocumentTitle = "Vendor Risk Scoring Documentation";
    });
}


// Global exception middleware
app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors(myAllowSpecificOrigins);

app.UseAuthorization();
app.MapControllers();

// HealthCheck
app.MapHealthChecks("/health");

// Seeder
await SeedData.InitializeAsync(app.Services);

// Uygulama kapanırken Serilog buffer flush olacak
try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}