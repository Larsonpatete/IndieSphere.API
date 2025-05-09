var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.IndieSphere_ApiService>("apiservice");

builder.AddProject<Projects.IndieSphere_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
