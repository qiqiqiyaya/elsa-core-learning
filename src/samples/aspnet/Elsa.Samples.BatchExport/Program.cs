using Elsa.Models;
using Elsa.Persistence;
using Elsa.Persistence.EntityFramework.Core.Extensions;
using Elsa.Persistence.EntityFramework.Sqlite;
using Elsa.Persistence.Specifications.WorkflowDefinitions;
using Elsa.Samples.BatchExport;
using Elsa.Serialization;
using Elsa.Services;
using System.Threading;
using Elsa;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services
    .AddElsa(options => options
        .AddHttpActivities()
        .UseEntityFrameworkPersistence(ef => ef.UseSqlite())
        .AddWorkflow<TestWorkflow>());
builder.Services.AddTransient<BatchExportFileService>();

var app = builder.Build();
await AddData(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();
app.MapGet("/test", async context =>
{
    var service = context.RequestServices.GetRequiredService<BatchExportFileService>();
    await service.ExportAllAsync();
});

app.Run();



async Task AddData(IServiceProvider provider)
{
    using (var scope = provider.CreateScope())
    {
        var scopeProvider = scope.ServiceProvider;
        var store = scopeProvider.GetRequiredService<IWorkflowDefinitionStore>();
        var specification = new ManyWorkflowDefinitionVersionIdsSpecification(new List<string>() { VersionOptions.Latest.ToString() });
        var count = await store.CountAsync(specification);

        if (count == 0)
        {
            var environment = scopeProvider.GetRequiredService<IWebHostEnvironment>();
            var workflowPublisher = scopeProvider.GetRequiredService<IWorkflowPublisher>();
            var contentSerializer = scopeProvider.GetRequiredService<IContentSerializer>();
            string path = environment.ContentRootPath + "Data\\";

            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                var stream = File.OpenRead(file);

                var json = await stream.ReadStringToEndAsync();
                var workflowDefinition = workflowPublisher.New();
                var postedModel = contentSerializer.Deserialize<WorkflowDefinition>(json);

                workflowDefinition.Activities = postedModel.Activities;
                workflowDefinition.Channel = postedModel.Channel;
                workflowDefinition.Connections = postedModel.Connections;
                workflowDefinition.Description = postedModel.Description;
                workflowDefinition.Name = postedModel.Name;
                workflowDefinition.Tag = postedModel.Tag;
                workflowDefinition.Variables = postedModel.Variables;
                workflowDefinition.ContextOptions = postedModel.ContextOptions;
                workflowDefinition.CustomAttributes = postedModel.CustomAttributes;
                workflowDefinition.DisplayName = postedModel.DisplayName;
                workflowDefinition.IsSingleton = postedModel.IsSingleton;
                workflowDefinition.DeleteCompletedInstances = postedModel.DeleteCompletedInstances;
                workflowDefinition.PersistenceBehavior = postedModel.PersistenceBehavior;
                workflowDefinition.TenantId = null;

                await workflowPublisher.SaveDraftAsync(workflowDefinition);
            }
        }
    }
}
