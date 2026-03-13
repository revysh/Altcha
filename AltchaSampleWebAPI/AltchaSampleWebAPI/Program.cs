using Ixnas.AltchaNet;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("DisableCORS", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

byte[] secretKey = Encoding.UTF8.GetBytes("1234567890123456789012345678901234567890123456789012345678901234");
string apiSecret = "sec_your_private_secret_here";
builder.Services.AddSingleton(sp =>
{
    return Altcha.CreateServiceBuilder()
        .UseSha256(secretKey)
        .UseInMemoryStore()
        //.SetComplexity(50000, 3000000)
        .Build();
});
builder.Services.AddMemoryCache();
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("DisableCORS");
app.UseAuthorization();

app.MapControllers();

app.Run();
