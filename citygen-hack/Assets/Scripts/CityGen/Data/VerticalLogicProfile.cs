using System;
using UnityEngine;

namespace CityGen.Data
{
    [Serializable]
    public class VerticalLogicProfile
    {
        [TextArea(2, 4)]
        public string LowerLevels = "Waste, labor, quarantine, and forgotten temple ruins.";

        [TextArea(2, 4)]
        public string MiddleLevels = "Public hospital functions, markets, housing, and worship spaces.";

        [TextArea(2, 4)]
        public string UpperLevels = "Administration, elite treatment, research, and military control.";

        [TextArea(2, 4)]
        public string Core = "Utilities, ancient machinery, restricted circulation, and extraction systems.";

        [TextArea(2, 4)]
        public string OuterShell = "Public balconies, markets, housing, shrines, and visible city life.";

        [TextArea(2, 4)]
        public string Crown = "Organic growth and unstable expansion.";

        [TextArea(2, 4)]
        public string PublicPrivateGradient = "Outer shell and middle bands are more public; upper core is more restricted.";

        [TextArea(2, 4)]
        public string CleanDirtyGradient = "Ceremonial and elite spaces skew cleaner; lower utility bands and extraction spaces skew dirtier.";

        [TextArea(2, 4)]
        public string SacredProfaneGradient = "Sacred symbolism radiates from the old core and persists even in repurposed medical spaces.";
    }
}
