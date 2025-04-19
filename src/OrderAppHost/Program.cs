var builder = DistributedApplication.CreateBuilder(args);

var orderAccumulator = builder.AddProject("orderaccumulator", "../OrderAccumulator/OrderAccumulator.csproj");
var orderGenerator =  builder.AddProject("ordergenerator", "../OrderGenerator/OrderGenerator.csproj")
                .WithReference(orderAccumulator);

builder.Build().Run();