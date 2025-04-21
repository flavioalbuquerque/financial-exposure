using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["AppHost:BrowserToken"] = "",
});

var orderAccumulator = builder.AddProject("orderaccumulator", "../OrderAccumulator/OrderAccumulator.csproj");
var orderGenerator =  builder.AddProject("ordergenerator", "../OrderGenerator/OrderGenerator.csproj")
                .WithReference(orderAccumulator);

builder.Build().Run();