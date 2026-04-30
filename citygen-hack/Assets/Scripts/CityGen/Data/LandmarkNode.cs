using System;
using System.Collections.Generic;
using UnityEngine;

namespace CityGen.Data
{
    [Serializable]
    public class LandmarkNode
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public string DistrictId = string.Empty;
        public int BandIndex;

        [TextArea(2, 4)]
        public string Description = string.Empty;

        public bool VisibleFromMultipleBands = true;
        public bool IsPrimaryNavigationAnchor = true;
        public Vector3 ApproximateLocalPosition = Vector3.zero;
        public List<string> Tags = new List<string>();
    }
}
