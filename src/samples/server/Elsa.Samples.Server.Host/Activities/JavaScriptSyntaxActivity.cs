using System.Threading.Tasks;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Design;
using Elsa.Expressions;
using Elsa.Serialization;
using Elsa.Services;
using Elsa.Services.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace Elsa.Samples.Server.Host.Activities
{
    [Action(
        Category = "HTTP",
        DisplayName = "Test SyntaxNames.JavaScript.",
        Description = "Test SyntaxNames.JavaScript.",
        Outcomes = new[] { OutcomeNames.Done }
    )]
    public class JavaScriptSyntaxActivity : Activity
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IStringLocalizer T { get; }
        private IContentSerializer _serializer;

        public JavaScriptSyntaxActivity(IHttpContextAccessor httpContextAccessor, 
            IStringLocalizer<CustomizeSelectActivity> localizer,
            IContentSerializer serializer)
        {
            _serializer = serializer;
            _httpContextAccessor = httpContextAccessor;
            T = localizer;
        }

        [ActivityInput(
            UIHint = ActivityInputUIHints.MultiLine,
            Hint = "Test.",
            DefaultSyntax = SyntaxNames.JavaScript,
            SupportedSyntaxes = new[] { SyntaxNames.Literal, SyntaxNames.JavaScript, SyntaxNames.Liquid }
        )]
        public object Value1 { get; set; }


        protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            var aa = Value1;
            var httpContext = _httpContextAccessor.HttpContext ?? new DefaultHttpContext();
            var response = httpContext.Response;

            if (response.HasStarted)
                return Fault(T["Response has already started"]!);

            await response.WriteAsync($"Value1: {_serializer.Serialize(aa)}", context.CancellationToken);
            return Done();
        }
    }
}
