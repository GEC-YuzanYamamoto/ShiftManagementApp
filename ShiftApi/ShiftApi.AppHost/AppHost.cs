var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.ShiftApi_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.ShiftFrontend>("shiftfrontend");

builder.Build().Run();
