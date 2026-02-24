var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddHttpClient("ApiClient", client =>
{
    var apiBase = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5000";
    client.BaseAddress = new Uri(apiBase);
    client.Timeout = TimeSpan.FromSeconds(300);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.MapRazorPages();

app.Run();
