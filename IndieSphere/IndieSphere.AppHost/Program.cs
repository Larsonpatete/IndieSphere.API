var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.IndieSphere_ApiService>("apiservice");

// TODO: add react frontend project
builder.Build().Run();
