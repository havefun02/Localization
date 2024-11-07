using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Globalization;

namespace Localization
{
    public class Inspect 
    {
        private const int MaxCultureFallbackDepth = 5;

        private readonly RequestDelegate _next;
        private readonly RequestLocalizationOptions _options;
        private readonly ILogger _logger;
        public Inspect(RequestDelegate next, RequestLocalizationOptions options, ILoggerFactory loggerFactory)
        {
            _next = next;
            _options = options; 
            _logger= loggerFactory.CreateLogger<Inspect>();
        }
        public async Task Invoke(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var requestCulture = _options.DefaultRequestCulture;

            IRequestCultureProvider? winningProvider = null;

            if (_options.RequestCultureProviders != null)
            {
                foreach (RouteDataRequestCultureProvider provider in _options.RequestCultureProviders)
                {
                    var providerResultCulture = await provider.DetermineProviderCultureResult(context);


                    if (providerResultCulture == null)
                    {
                        continue;
                    }
                    var cultures = providerResultCulture.Cultures;
                    var uiCultures = providerResultCulture.UICultures;

                    CultureInfo? cultureInfo = null;
                    CultureInfo? uiCultureInfo = null;
                    if (_options.SupportedCultures != null)
                    {
                        cultureInfo = GetCultureInfo(
                            cultures,
                            _options.SupportedCultures,
                            _options.FallBackToParentCultures);



                    }

                    if (_options.SupportedUICultures != null)
                    {
                        uiCultureInfo = GetCultureInfo(
                            uiCultures,
                            _options.SupportedUICultures,
                            _options.FallBackToParentUICultures);
                    }

                    if (cultureInfo == null && uiCultureInfo == null)
                    {
                        continue;
                    }

                    cultureInfo ??= _options.DefaultRequestCulture.Culture;
                    uiCultureInfo ??= _options.DefaultRequestCulture.UICulture;

                    var result = new RequestCulture(cultureInfo, uiCultureInfo);
                    requestCulture = result;
                    winningProvider = provider;
                    break;
                }
            }

            context.Features.Set<IRequestCultureFeature>(new RequestCultureFeature(requestCulture, winningProvider));

            SetCurrentThreadCulture(requestCulture);

            if (_options.ApplyCurrentCultureToResponseHeaders)
            {
                var headers = context.Response.Headers;
                headers.ContentLanguage = requestCulture.UICulture.Name;
            }

            await _next(context);
        }

        private static void SetCurrentThreadCulture(RequestCulture requestCulture)
        {
            CultureInfo.CurrentCulture = requestCulture.Culture;
            CultureInfo.CurrentUICulture = requestCulture.UICulture;
        }

        private static CultureInfo? GetCultureInfo(
            IList<StringSegment> cultureNames,
            IList<CultureInfo> supportedCultures,
            bool fallbackToParentCultures)
        {
            foreach (var cultureName in cultureNames)
            {
                // Allow empty string values as they map to InvariantCulture, whereas null culture values will throw in
                // the CultureInfo ctor
                if (cultureName != null)
                {
                    var cultureInfo = GetCultureInfo(cultureName, supportedCultures, fallbackToParentCultures, currentDepth: 0);
                    if (cultureInfo != null)
                    {
                        return cultureInfo;
                    }
                }
            }

            return null;
        }

        private static CultureInfo? GetCultureInfo(
            StringSegment cultureName,
            IList<CultureInfo>? supportedCultures,
            bool fallbackToParentCultures,
            int currentDepth)
        {
            // If the cultureName is an empty string there
            // is no chance we can resolve the culture info.
            if (cultureName.Equals(string.Empty))
            {
                return null;
            }

            var culture = GetCultureInfo(cultureName, supportedCultures);

            if (culture == null && fallbackToParentCultures && currentDepth < MaxCultureFallbackDepth)
            {
                try
                {
                    culture = CultureInfo.GetCultureInfo(cultureName.ToString());

                    culture = GetCultureInfo(culture.Parent.Name, supportedCultures, fallbackToParentCultures, currentDepth + 1);
                }
                catch (CultureNotFoundException)
                {
                }
            }

            return culture;
        }

        private static CultureInfo? GetCultureInfo(StringSegment name, IList<CultureInfo>? supportedCultures)
        {
            // Allow only known culture names as this API is called with input from users (HTTP requests) and
            // creating CultureInfo objects is expensive and we don't want it to throw either.
            if (name == null || supportedCultures == null)
            {
                return null;
            }
            var culture = supportedCultures.FirstOrDefault(
                supportedCulture => {
                    return StringSegment.Equals(supportedCulture.Name, name, StringComparison.OrdinalIgnoreCase);
                    });

            if (culture == null)
            {
                return null;
            }

            return CultureInfo.ReadOnly(culture);
        }

    }
}
