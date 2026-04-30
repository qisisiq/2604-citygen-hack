using System;
using System.Collections.Generic;

namespace CityGen.Data
{
    [Serializable]
    public class NavigationPrinciples
    {
        public string MainPublicRoute = "Spiraling pilgrimage ramp";
        public string MainHiddenRoute = "Old temple stair inside the core";
        public string MainRestrictedRoute = "Military medical elevator";
        public List<string> MajorLandmarks = new List<string>();
        public List<string> WayfindingRules = new List<string>();
    }
}
