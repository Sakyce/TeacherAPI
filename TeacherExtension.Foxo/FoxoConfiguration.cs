using BepInEx.Configuration;

namespace TeacherExtension.Foxo
{
    // It's recommended to at least add Bepinex config to let users tweak your teacher settings.
    // Most of the time, it will be about changing weights.
    public class FoxoConfiguration
    {
        public static ConfigEntry<int> Weight { get; internal set; }
        public static ConfigEntry<int> FoxFloor { get; internal set; }

        /// <summary>
        /// Triggered when launching the mod, mostly to setup BepInEx config bindings.
        /// </summary>
        internal static void Setup()
        {
            Weight = FoxoPlugin.Instance.Config.Bind(
                "Foxo",
                "Weight",
                100,
                "More it is higher, more there is a chance of him spawning. (Defaults to 100. For comparison, Baldi weight is 100) (Requires Restart)"
            );
            FoxFloor = FoxoPlugin.Instance.Config.Bind(
                "Foxo",
                "FOXFloorFrequency",
                10,
                "Every n floors, DarkFoxo will appear. (Defaults to 10) (Requires Foxo.WrathFloor, EndlessFloors and Restart)"
            );
        }
    }
}
