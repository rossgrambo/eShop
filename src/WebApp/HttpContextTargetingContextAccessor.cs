// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
namespace eShop.WebApp
{
    using Microsoft.FeatureManagement.FeatureFilters;

    /// <summary>
    /// Provides an implementation of <see cref="ITargetingContextAccessor"/> that creates a targeting context using info from the current HTTP request.
    /// </summary>
    public class HttpContextTargetingContextAccessor : ITargetingContextAccessor
    {
        private const string TargetingContextLookup = "HttpContextTargetingContextAccessor.TargetingContext";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextTargetingContextAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public ValueTask<TargetingContext> GetContextAsync()
        {
            HttpContext? httpContext = _httpContextAccessor.HttpContext;

            //
            // Try cache lookup
            if (httpContext != null && httpContext.Items.TryGetValue(TargetingContextLookup, out object? value))
            {
                if (value != null)
                {
                    return new ValueTask<TargetingContext>((TargetingContext)value);
                }
            }

            //
            // Grab username from cookie
            string username = httpContext?.User?.Identity?.Name?.ToLower() ?? "";

            var groups = new List<string>();

            //
            // Build targeting context based on user info
            var targetingContext = new TargetingContext
            {
                UserId = username,
                Groups = groups
            };

            //
            // Cache for subsequent lookup
            if (httpContext != null)
            {
                httpContext.Items[TargetingContextLookup] = targetingContext;
            }

            return new ValueTask<TargetingContext>(targetingContext);
        }
    }
}
