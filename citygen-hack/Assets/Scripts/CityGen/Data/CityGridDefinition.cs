using System;
using System.Collections.Generic;

namespace CityGen.Data
{
    [Serializable]
    public class CityGridDefinition
    {
        public float TotalHeightMeters = 1200f;
        public float OuterRadiusMeters = 280f;
        public float CoreRadiusMeters = 60f;
        public int HeightBands = 10;
        public int RadialRings = 4;
        public int AngularSectors = 12;
        public bool HasHollowAtrium = true;
        public bool HasExternalBalconies = true;
        public bool HasExpansionScars = true;
        public List<string> MajorVoids = new List<string>();
        public List<string> StructuralRibs = new List<string>();
    }
}
