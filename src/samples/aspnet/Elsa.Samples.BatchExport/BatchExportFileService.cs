using Elsa.Models;
using Elsa.Persistence;
using Elsa.Persistence.Specifications;
using Elsa.Persistence.Specifications.WorkflowDefinitions;
using Elsa.Serialization;
using Humanizer;
using System.IO.Compression;

namespace Elsa.Samples.BatchExport
{
    public class BatchExportFileService
    {
        private readonly IWorkflowDefinitionStore _store;
        private readonly IWebHostEnvironment _environment;
        private readonly IContentSerializer _contentSerializer;

        public BatchExportFileService(IWorkflowDefinitionStore store,
            IWebHostEnvironment environment,
            IContentSerializer contentSerializer)
        {
            _store = store;
            _environment = environment;
            _contentSerializer = contentSerializer;
        }

        public async Task ExportAllAsync()
        {
            var specification = GetSpecification(null, VersionOptions.Latest)
                .And(new TenantSpecification<WorkflowDefinition>(null));

            var orderBySpecification = OrderBySpecification
                .OrderBy<WorkflowDefinition>(x => x.CreatedAt!, SortDirection.Descending);
            var items = await _store
                .FindManyAsync(specification, orderBySpecification, Paging.Page(0, 10000));

            var workflowDefinitions = items as WorkflowDefinition[] ?? items.ToArray();
            if (!workflowDefinitions.Any())
            {
                return;
            }

            string directory = Guid.NewGuid().ToString();
            string basePath = _environment.ContentRootPath + "WorkFlow\\";
            string jsonFileDirectory = basePath + directory;
            if (!Directory.Exists(jsonFileDirectory))
            {
                Directory.CreateDirectory(jsonFileDirectory);
            }

            foreach (var item in workflowDefinitions)
            {
                var hasWorkflowName = !string.IsNullOrWhiteSpace(item.Name);
                var workflowName = hasWorkflowName ? item.Name!.Trim() : item.DefinitionId;

                var fileName = hasWorkflowName
                    ? $"{workflowName.Underscore().Dasherize().ToLowerInvariant()}.json"
                    : $"workflow-definition-{workflowName.Underscore().Dasherize().ToLowerInvariant()}.json";

                var json = _contentSerializer.Serialize(item);
                await File.WriteAllTextAsync(jsonFileDirectory + "\\" + fileName, json);
            }

            ZipFile.CreateFromDirectory(jsonFileDirectory, basePath + directory + ".zip");
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
