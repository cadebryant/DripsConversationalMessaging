var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.DripsConversationalMessaging_Server>("dripsconversationalmessaging-server");

builder.Build().Run();
