using Microsoft.AspNetCore.Components;

namespace F3Wasm.Layouts
{
    public class F3BaseLayout : LayoutComponentBase
    {
        public string Region { get; set; }

        protected override void OnParametersSet()
        {
            // pull out the "Region" parameter from the route data
            object region = null;
            if ((this.Body.Target as RouteView)?.RouteData.RouteValues?.TryGetValue("Region", out region) == true)
            {
                Region = region?.ToString().ToLower();
            }
        }
    }
}