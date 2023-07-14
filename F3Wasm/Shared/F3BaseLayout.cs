using Microsoft.AspNetCore.Components;
using System.Web;

namespace F3Wasm.Layouts
{
    public class F3BaseLayout : LayoutComponentBase
    {
        public string Region { get; set; }
        public bool IsEmbed { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        protected override void OnParametersSet()
        {
            // pull out the "Region" parameter from the route data
            object region = null;
            if ((this.Body.Target as RouteView)?.RouteData.RouteValues?.TryGetValue("Region", out region) == true)
            {
                Region = region?.ToString().ToLower();
            }
        }

        protected override void OnInitialized()
        {
            // Get IsEmbed from the query string
            var uri = new Uri(NavigationManager.Uri);
            var hasIsEmbedParam = HttpUtility.ParseQueryString(uri.Query).AllKeys.Contains("IsEmbed");

            if (hasIsEmbedParam)
            {
                var isEmbedParam = HttpUtility.ParseQueryString(uri.Query)["IsEmbed"];
                bool.TryParse(isEmbedParam, out var isEmbed);
                IsEmbed = isEmbed;
            }
        }
    }
}