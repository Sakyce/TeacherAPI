using BepInEx.Configuration;

namespace NullTeacher
{
    public class NullConfiguration
    {
        public static ConfigEntry<bool> ReplaceNullWithBaldloon { get; internal set; }
        public static ConfigEntry<int> SpawnWeight { get; internal set; }
        public static ConfigEntry<int> InfiniteFloorsFrequency { get; internal set; }

        internal static void Setup()
        {
            SpawnWeight = NullTeacherPlugin.Instance.Config.Bind(
                "Null", "SpawnWeight", 20, 
                "More it is higher, more there is a chance of him spawning. (Defaults to 100. For comparison, Baldi weight is 100) (Requires Restart)"
            );
            ReplaceNullWithBaldloon = NullTeacherPlugin.Instance.Config.Bind(
                "Null", "Baldloon", false,
                "Replaces null with a Baldloon. (Defaults to false) (Requires Restart)"
            );
            InfiniteFloorsFrequency = NullTeacherPlugin.Instance.Config.Bind(
                "Null", "NullFloorFrequency", 5,
                "Every n floors, Null will appear. (Defaults to 5) (Requires Null.Floor, EndlessFloors and Restart) (Requires Restart)"
            );
        }
    }
}
