using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Elsa.Attributes;
using Elsa.Design;
using Elsa.Expressions;
using Elsa.Metadata;
using Elsa.Services;

namespace Elsa.Samples.Server.Host.Activities
{
    [Trigger(
        Category = "HTTP",
        DisplayName = "CustomizeSelect",
        Description = "Customize select test.",
        Outcomes = new[] { OutcomeNames.Done })
    ]
    public class CustomizeSelectActivity : Activity, IActivityPropertyOptionsProvider, IRuntimeSelectListProvider
    {
        [ActivityInput(
            UIHint = ActivityInputUIHints.Dropdown,
            OptionsProvider = typeof(CustomizeSelectActivity),
            DefaultSyntax = SyntaxNames.Literal,
            SupportedSyntaxes = new[] { SyntaxNames.Literal, SyntaxNames.JavaScript, SyntaxNames.Liquid }
        )]
        public string? Value { get; set; }

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
    }
}
