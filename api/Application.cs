var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

var app = builder.Build();

// Add middlewares 
app.MapControllers();

// Run the application
app.Run();
