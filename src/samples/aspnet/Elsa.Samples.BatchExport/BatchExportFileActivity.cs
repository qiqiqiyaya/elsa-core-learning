using System.IO.Compression;
using Elsa.ActivityResults;
using Elsa.Persistence;
using Elsa.Persistence.Specifications.WorkflowDefinitions;
using Elsa.Persistence.Specifications;
using Elsa.Services;
using Elsa.Services.Models;
using System.Threading;
using Elsa.Models;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Humanizer;
using Elsa.Serialization;

namespace Elsa.Samples.BatchExport
{
    public class BatchExportFileActivity : Activity
    {
        public BatchExportFileActivity()
        {

        }

        protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            
            var store = context.ServiceProvider.GetRequiredService<IWorkflowDefinitionStore>();
            var specification = GetSpecification(null, VersionOptions.Latest)
                .And(new TenantSpecification<WorkflowDefinition>(null));

            var orderBySpecification = OrderBySpecification
                .OrderBy<WorkflowDefinition>(x => x.CreatedAt!, SortDirection.Descending);
            var items = await store
                .FindManyAsync(specification, orderBySpecification, Paging.Page(0, 10000), context.CancellationToken);

            var environment = context.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
            var contentSerializer = context.ServiceProvider.GetRequiredService<IContentSerializer>();

            string directory = Guid.NewGuid().ToString();
            string basePath = environment.ContentRootPath + "WorkFlow/";
            string jsonFileDirectory = basePath + directory;
            if (!Directory.Exists(jsonFileDirectory))
            {
                Directory.CreateDirectory(jsonFileDirectory);
            }

            foreach (var item in items)
            {
                var hasWorkflowName = !string.IsNullOrWhiteSpace(item.Name);
                var workflowName = hasWorkflowName ? item.Name!.Trim() : item.DefinitionId;

                var fileName = hasWorkflowName
                    ? $"{workflowName.Underscore().Dasherize().ToLowerInvariant()}.json"
                    : $"workflow-definition-{workflowName.Underscore().Dasherize().ToLowerInvariant()}.json";

                var json = contentSerializer.Serialize(item);
                await File.WriteAllTextAsync(jsonFileDirectory + "/" + fileName, json);
            }

            ZipFile.CreateFromDirectory(jsonFileDirectory, basePath + directory + ".zip");

            return Done();
        }

        private Specification<WorkflowDefinition> GetSpecification(string? ids, VersionOptions version)
        {
            if (string.IsNullOrWhiteSpace(ids))
                return new VersionOptionsSpecification(version);

            var splitIds = ids.Split(',', StringSplitOptions.RemoveEmptyEntries);
            return new ManyWorkflowDefinitionIdsSpecification(splitIds, version);
        }
    }
}
