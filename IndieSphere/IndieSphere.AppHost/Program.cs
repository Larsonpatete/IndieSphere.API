//using Aspire.Hosting.Docker;

var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.IndieSphere_ApiService>("apiservice");

// TODO: add react frontend project

var container = builder.AddDockerfile("mycontainer", "..", "IndieSphere.ApiService/Dockerfile");


builder.Build().Run();

//"C:\Users\Larson Patete\source\repos\IndieSphereDiscovery\IndieSphere\IndieSphere.AppHost"
//"C:\Users\Larson Patete\source\repos\TypeScriptFun\IndieSphere.UI\src\App.tsx"
