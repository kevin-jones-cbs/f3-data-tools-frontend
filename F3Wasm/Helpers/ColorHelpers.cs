using F3Core;

namespace F3Wasm.Data
{
    public static class ColorHelpers
    {
        public static string GetAoHex(Ao ao)
        {
            return ao.DayOfWeek switch
            {
                DayOfWeek.Monday => "#ff5b72",
                DayOfWeek.Tuesday => "#9370f0",
                DayOfWeek.Wednesday => "#32b0e5",
                DayOfWeek.Thursday => "#fdb00d",
                DayOfWeek.Friday => "#00b994",
                DayOfWeek.Saturday => "#25b808",
                DayOfWeek.Sunday => "#f875c0",
                _ => "#ffffff",
            };
        }
    }
}