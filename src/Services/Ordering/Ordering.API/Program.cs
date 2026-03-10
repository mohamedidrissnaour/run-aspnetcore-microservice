using Consul;
using Ordering.API;
using Ordering.Application;
using Ordering.Infrastructure;
using Ordering.Infrastructure.Data.Extensions;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(cfg =>
{
    cfg.Address = new Uri("http://consul:8500"); // adresse du serveur Consul
}));

// Add services to the container.
builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration)
    .AddApiServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseApiServices();

if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
}


//discovery service
//---------------------------------
var consulClient = app.Services.GetRequiredService<IConsulClient>();
var registration = new AgentServiceRegistration()
{
    ID = "ordering-api-1", //identifiant unique du service
    Name = "ordering.api", //nom du service
    Address = "ordering.api", // nom du container Docker pour que les autres services le voient
    Port = 8080 //port interne exposé dans Docker 
};

await consulClient.Agent.ServiceRegister(registration);
//-----------------------------------


app.Run();
