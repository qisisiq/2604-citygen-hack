using System.Collections.Generic;

namespace CityGen.Data
{
    public static class CitySeedFactory
    {
        public static CitySeedData CreateAscendingWard()
        {
            CitySeedData seed = new CitySeedData();
            seed.CityName = "The Ascending Ward";
            seed.Seed = 2604;
            seed.MacroShape = MacroShapeKind.HollowCylinder;

            seed.Grid.TotalHeightMeters = 1200f;
            seed.Grid.OuterRadiusMeters = 280f;
            seed.Grid.CoreRadiusMeters = 60f;
            seed.Grid.HeightBands = 10;
            seed.Grid.RadialRings = 4;
            seed.Grid.AngularSectors = 12;
            seed.Grid.HasHollowAtrium = true;
            seed.Grid.HasExternalBalconies = true;
            seed.Grid.HasExpansionScars = true;

            seed.Origin.FoundingReason = "A sacred healing city formed around a vertical anomaly that drew pilgrims seeking cures.";
            seed.Origin.FoundingFunction = "Healing pilgrimage";
            seed.Origin.FoundingInstitution = "Temple-healer order";
            seed.Origin.CatalystEntity = "Ancient healing machine";
            seed.Origin.FirstSettlementLocation = "Outer shell sanctuary ring";
            seed.Origin.OriginalCenter = "Sacred spring inside the old core";
            seed.Origin.OriginalCirculationLogic = "Pilgrims climbed ceremonial ramps while attendants and priests used hidden core stairs.";
            seed.Origin.OldestSurvivingStructures = new List<string>
            {
                "Old temple stair",
                "Sacred cistern",
                "Processional hall",
                "Reliquary vault"
            };

            seed.Identity.ClaimedIdentity = "A sacred hospital city where pilgrims become residents while seeking healing.";
            seed.Identity.ActualIdentity = "A military-medical extraction system that harvests biological, emotional, and cognitive outputs from patients.";
            seed.Identity.LegacyIdentity = "A temple city built around ritual healing and managed pilgrimage.";
            seed.Identity.EmergentIdentity = "A living organism that expands in response to suffering, prayer, labor, and treatment waste.";
            seed.Identity.ConflictingInstitutions = new List<string>
            {
                "Temple-healer order",
                "Hospital bureaucracy",
                "Military research directorate",
                "Extraction logistics office"
            };

            seed.VerticalLogic.LowerLevels = "Waste, labor, quarantine, and forgotten temple ruins.";
            seed.VerticalLogic.MiddleLevels = "Public hospital, markets, resident wards, and worship spaces.";
            seed.VerticalLogic.UpperLevels = "Administration, elite treatment, research, and military control.";
            seed.VerticalLogic.Core = "Utilities, ancient healing machine, and hidden extraction systems.";
            seed.VerticalLogic.OuterShell = "Public balconies, markets, housing, and visible city life.";
            seed.VerticalLogic.Crown = "Organic growth and unstable expansion.";

            seed.Navigation.MainPublicRoute = "Spiraling pilgrimage ramp";
            seed.Navigation.MainHiddenRoute = "Old temple stair inside the core";
            seed.Navigation.MainRestrictedRoute = "Military medical elevator";
            seed.Navigation.MajorLandmarks = new List<string>
            {
                "The Healing Atrium",
                "The Votive Surgery Theater",
                "The Root Cistern",
                "The Military Recovery Garden",
                "The Growth Crown"
            };
            seed.Navigation.WayfindingRules = new List<string>
            {
                "Up means authority, purity, research, and danger.",
                "Down means waste, labor, and forgotten history.",
                "Outer shell means public life.",
                "Core means infrastructure and secrets."
            };

            seed.TaxonomyWeights = new List<TaxonomyWeight>
            {
                new TaxonomyWeight { Kind = TaxonomyKind.TemplePilgrimage, Weight = 0.25f },
                new TaxonomyWeight { Kind = TaxonomyKind.HospitalHealing, Weight = 0.30f },
                new TaxonomyWeight { Kind = TaxonomyKind.ResourceExtraction, Weight = 0.20f },
                new TaxonomyWeight { Kind = TaxonomyKind.MilitaryResearch, Weight = 0.15f },
                new TaxonomyWeight { Kind = TaxonomyKind.OrganicMegastructure, Weight = 0.10f }
            };

            seed.DesignPriorities = new List<string>
            {
                "Coherence over randomness",
                "Walkability over abstract correctness",
                "Strong district identity",
                "Clear player navigation",
                "Vertical drama",
                "Layered history"
            };

            seed.AvoidRules = new List<string>
            {
                "Random skyscraper placement",
                "Generic cyberpunk city blocks",
                "Pure aesthetic generation without infrastructure",
                "Overcomplicated simulation before the graph works"
            };

            return seed;
        }
    }
}
