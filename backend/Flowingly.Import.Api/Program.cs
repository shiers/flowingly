using Flowingly.Import.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- CORS: allow frontend dev server and production container ---
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",  // Vite dev server
                "http://localhost:8080"   // Nginx production container
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// --- Domain services ---
builder.Services.AddScoped<IMarkupParser, MarkupParser>();
builder.Services.AddScoped<IImportValidator, ImportValidator>();
builder.Services.AddScoped<ITaxCalculator, TaxCalculator>();
builder.Services.AddScoped<IWorkflowInsightBuilder, WorkflowInsightBuilder>();
builder.Services.AddScoped<IImportApplicationService, ImportApplicationService>();

var app = builder.Build();

// --- Middleware ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapControllers();

app.Run();

// Expose Program for integration test project access
public partial class Program { }
