using System.Net;
using Elsa.Activities.Http;
using Elsa.Builders;
using Elsa.Services.Models;

namespace Elsa.Samples.BatchExport
{
    public class TestWorkflow : IWorkflow
    {
        public void Build(IWorkflowBuilder builder)
        {
            builder
                .HttpEndpoint("/test")
                .WriteHttpResponse(_ => HttpStatusCode.OK, GenerateSomeHtml, _ => "text/html");
        }

        private string GenerateSomeHtml(ActivityExecutionContext context)
        {
            //var query = (IQueryCollection)context.Input!; // The output of the ReadQueryString activity will be available as input to this one.
            //var items = /*query!.Select(x => $"<li>{x.Key}: {x.Value}</li>")*/;

            return $"<ul>313323232</ul>";
        }
    }
}
