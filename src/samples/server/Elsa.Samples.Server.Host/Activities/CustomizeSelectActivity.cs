using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Design;
using Elsa.Expressions;
using Elsa.Metadata;
using Elsa.Samples.Server.Host.OptionsProvider;
using Elsa.Services;
using Elsa.Services.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Elsa.Samples.Server.Host.Activities
{
    [Trigger(
        Category = "HTTP",
        DisplayName = "自定义UI下拉框值，通过Response输出",
        Description = "Customize select test.",
        Outcomes = new[] { OutcomeNames.Done })
    ]
    public class CustomizeSelectActivity : Activity, IActivityPropertyOptionsProvider, IRuntimeSelectListProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IStringLocalizer T { get; }

        public CustomizeSelectActivity(IHttpContextAccessor httpContextAccessor, IStringLocalizer<CustomizeSelectActivity> localizer)
        {
            _httpContextAccessor= httpContextAccessor;
            T= localizer;
        }

        [ActivityInput(
            UIHint = ActivityInputUIHints.Dropdown,
            OptionsProvider = typeof(CustomizeSelectActivity),
            DefaultSyntax = SyntaxNames.Literal,
            SupportedSyntaxes = new[] { SyntaxNames.Literal, SyntaxNames.JavaScript, SyntaxNames.Liquid }
        )]
        public string? Value1 { get; set; }

        [ActivityInput(
            UIHint = ActivityInputUIHints.Dropdown,
            OptionsProvider = typeof(CustomizeSelectValue2OptionsProvider),
            DefaultSyntax = SyntaxNames.Literal,
            SupportedSyntaxes = new[] { SyntaxNames.Literal, SyntaxNames.JavaScript, SyntaxNames.Liquid }
        )]
        public string? Value2 { get; set; }

        public ValueTask<SelectList> GetSelectListAsync(object? context = default, CancellationToken cancellationToken = default)
        {
            var brands = new[] { "BMW", "Peugeot", "Tesla" };
            var items = brands.Select(x => new SelectListItem(x)).ToList();
            return new ValueTask<SelectList>(new SelectList(items));
        }

        public object? GetOptions(PropertyInfo property)
        {
            return new RuntimeSelectListProviderSettings(GetType(), new { Age=15 });
        }

        protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            var httpContext = _httpContextAccessor.HttpContext ?? new DefaultHttpContext(); 
            var response = httpContext.Response;

            if (response.HasStarted)
                return Fault(T["Response has already started"]!);

            await response.WriteAsync($"Value1: {Value1},Value2: {Value2} ", context.CancellationToken);
            return Done();
        }
    }
}
