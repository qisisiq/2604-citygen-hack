using System;
using System.Collections.Generic;
using UnityEngine;

namespace CityGen.Data
{
    [Serializable]
    public class DistrictNode
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public int MinFloor;
        public int MaxFloor;
        public int RadialRing;
        public int AngularSectorStart;
        public int AngularSectorEnd;
        public List<CityFunction> OfficialFunctions = new List<CityFunction>();
        public List<CityFunction> HiddenFunctions = new List<CityFunction>();
        public List<CityFunction> InheritedFunctions = new List<CityFunction>();
        public List<string> Tags = new List<string>();
        public AccessLevel AccessLevel = AccessLevel.Public;
        public HistoricalLayer HistoricalLayer = HistoricalLayer.OriginalTemple;

        [Range(0f, 1f)]
        public float Cleanliness = 0.5f;

        [Range(0f, 1f)]
        public float Sacredness = 0.5f;

        [Range(0f, 1f)]
        public float Danger = 0.5f;

        [Range(0f, 1f)]
        public float Wealth = 0.5f;

        [Range(0f, 1f)]
        public float Surveillance = 0.5f;

        [Range(0f, 1f)]
        public float OrganicMutation = 0.0f;

        public string PopulationProfile = string.Empty;
        public string OfficialRole = string.Empty;
        public string HiddenRole = string.Empty;
        public string InheritedRole = string.Empty;
        public List<string> VisualMotifs = new List<string>();
        public List<string> SoundMotifs = new List<string>();
        public List<string> SmellMotifs = new List<string>();
        public List<string> MainRouteIds = new List<string>();
        public List<string> HiddenRouteIds = new List<string>();
        public List<string> LandmarkIds = new List<string>();
        public List<string> DependencyIds = new List<string>();
        public List<string> NarrativeHooks = new List<string>();
    }
}
