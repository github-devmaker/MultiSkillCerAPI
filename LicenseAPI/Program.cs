using LicenseAPI.Contexts;
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(c =>
{
    c.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(15);
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<dbSCM>();
builder.Services.AddDbContext<dbHRM>();
builder.Services.AddDbContext<dbBCS>();
builder.Services.AddDbContext<dbIoTFac2>();
builder.Services.AddCors(options => options.AddPolicy("Cors", builder =>
{
    builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader();
}));
var app = builder.Build();
app.UseCors("Cors");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
