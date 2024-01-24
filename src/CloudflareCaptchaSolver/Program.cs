using CloudflareCaptchaSolver;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCaptchaSwaggerGen();
builder.Services.AddAuthenticationManager();
builder.Services.AddBrowserManager(c => builder.Configuration.Bind("BrowserManager", c));
builder.Services.AddCommandManager(c => builder.Configuration.Bind("CommandManager", c));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapCaptchaEndpoints();

app.Run();
