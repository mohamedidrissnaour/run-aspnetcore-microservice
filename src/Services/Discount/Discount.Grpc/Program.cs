using Consul;
using Discount.Grpc.Data;
using Discount.Grpc.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddDbContext<DiscountContext>(opts =>
        opts.UseSqlite(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(cfg =>
{
    cfg.Address = new Uri("http://consul:8500"); // adresse du serveur Consul
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMigration();
app.MapGrpcService<DiscountService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");


//discovery service
//---------------------------------
var consulClient = app.Services.GetRequiredService<IConsulClient>();
var registration = new AgentServiceRegistration()
{
    ID = "discount-grpc-1", //identifiant unique du service
    Name = "discount.grpc", //nom du service
    Address = "catalog.api", // nom du container Docker pour que les autres services le voient
    Port = 8080 //port interne exposé dans Docker 
};

await consulClient.Agent.ServiceRegister(registration);
//-----------------------------------



app.Run();
