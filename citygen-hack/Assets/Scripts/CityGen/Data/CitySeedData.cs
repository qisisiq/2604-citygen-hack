using System;
using System.Collections.Generic;

namespace CityGen.Data
{
    [Serializable]
    public class CitySeedData
    {
        public string CityName = "The Ascending Ward";
        public int Seed = 2604;
        public MacroShapeKind MacroShape = MacroShapeKind.HollowCylinder;
        public CityGridDefinition Grid = new CityGridDefinition();
        public CityOriginData Origin = new CityOriginData();
        public CityIdentitySummary Identity = new CityIdentitySummary();
        public VerticalLogicProfile VerticalLogic = new VerticalLogicProfile();
        public NavigationPrinciples Navigation = new NavigationPrinciples();
        public List<TaxonomyWeight> TaxonomyWeights = new List<TaxonomyWeight>();
        public List<string> DesignPriorities = new List<string>();
        public List<string> AvoidRules = new List<string>();
    }
}
