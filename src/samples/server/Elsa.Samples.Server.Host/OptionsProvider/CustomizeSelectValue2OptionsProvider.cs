using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Elsa.Design;
using Elsa.Metadata;

namespace Elsa.Samples.Server.Host.OptionsProvider
{
    public class CustomizeSelectValue2OptionsProvider: IActivityPropertyOptionsProvider, IRuntimeSelectListProvider
    {
        public object? GetOptions(PropertyInfo property)
        {
            return new RuntimeSelectListProviderSettings(GetType(), new
            {
                Age = 12345
            });
        }

        public ValueTask<SelectList> GetSelectListAsync(object? context = default, CancellationToken cancellationToken = default)
        {
            var brands = new[] { "111", "222", "333" };
            var items = brands.Select(x => new SelectListItem(x)).ToList();
            return new ValueTask<SelectList>(new SelectList(items));
        }
    }
}
