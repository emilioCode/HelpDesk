using HelpDesk;
using Microsoft.AspNetCore.Builder;

// The host is built with the current model (WebApplication). The configuration of the services and the order of the middlewares are
// still those of the Startup class, untouched, so the application answers exactly as it did with the previous host (WebHost).
var builder = WebApplication.CreateBuilder(args);

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();
startup.Configure(app, app.Environment);

app.Run();

// The integration tests start the application through this type.
public partial class Program
{
}
