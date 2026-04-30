using System;
using System.Collections.Generic;
using UnityEngine;

namespace CityGen.Data
{
    [Serializable]
    public class VerticalBandData
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public int Index;
        public int MinFloor;
        public int MaxFloor;
        public List<CityFunction> OfficialFunctions = new List<CityFunction>();
        public List<CityFunction> HiddenFunctions = new List<CityFunction>();
        public List<CityFunction> InheritedFunctions = new List<CityFunction>();
        public AccessLevel DefaultAccessLevel = AccessLevel.Public;
        public HistoricalLayer DominantHistoricalLayer = HistoricalLayer.OriginalTemple;

        [Range(0f, 1f)]
        public float Sacredness = 0.5f;

        [Range(0f, 1f)]
        public float Cleanliness = 0.5f;

        [Range(0f, 1f)]
        public float Wealth = 0.5f;

        [Range(0f, 1f)]
        public float Danger = 0.5f;

        [Range(0f, 1f)]
        public float Surveillance = 0.5f;

        [Range(0f, 1f)]
        public float OrganicMutation = 0.0f;

        [TextArea(2, 4)]
        public string Description = string.Empty;

        public List<string> LandmarkIds = new List<string>();
        public List<string> ConnectedBandIds = new List<string>();
    }
}
