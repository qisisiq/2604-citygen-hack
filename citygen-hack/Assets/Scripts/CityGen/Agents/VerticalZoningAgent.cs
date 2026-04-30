using System.Collections.Generic;
using CityGen.Core;
using CityGen.Data;
using UnityEngine;

namespace CityGen.Agents
{
    [CreateAssetMenu(menuName = "CityGen/Agents/Vertical Zoning Agent", fileName = "VerticalZoningAgent")]
    public class VerticalZoningAgent : CityAgentBase
    {
        [SerializeField]
        private int floorsPerBand = 4;

        public override void Execute(CityContext context)
        {
            if (context == null)
            {
                return;
            }

            if (context.Grid == null)
            {
                context.Grid = new CityGridDefinition();
            }

            context.VerticalBands = new List<VerticalBandData>();
            int bandCount = Mathf.Max(4, context.Grid.HeightBands);

            for (int i = 0; i < bandCount; i++)
            {
                VerticalBandData band = BuildBand(i, bandCount);

                band.Id = $"band-{i:00}";
                band.MinFloor = i * Mathf.Max(1, floorsPerBand);
                band.MaxFloor = band.MinFloor + Mathf.Max(1, floorsPerBand) - 1;

                if (i > 0)
                {
                    band.ConnectedBandIds.Add($"band-{i - 1:00}");
                }

                if (i < bandCount - 1)
                {
                    band.ConnectedBandIds.Add($"band-{i + 1:00}");
                }

                if (i < context.Navigation.MajorLandmarks.Count)
                {
                    band.LandmarkIds.Add(context.Navigation.MajorLandmarks[i]);
                }

                context.VerticalBands.Add(band);
            }

            LogDecision(
                context,
                $"Generated {context.VerticalBands.Count} vertical bands.",
                "Bands were assigned official, hidden, and inherited functions using lower, middle, upper, core, and crown logic.");
        }

        private static VerticalBandData BuildBand(int index, int bandCount)
        {
            float t = bandCount <= 1 ? 0f : (float)index / (bandCount - 1);
            VerticalBandData band = new VerticalBandData();
            band.Index = index;

            if (index == 0)
            {
                band.DisplayName = "Root Cisterns";
                band.OfficialFunctions.Add(CityFunction.Water);
                band.OfficialFunctions.Add(CityFunction.Waste);
                band.HiddenFunctions.Add(CityFunction.ResourceExtraction);
                band.InheritedFunctions.Add(CityFunction.Worship);
                band.DefaultAccessLevel = AccessLevel.Restricted;
                band.DominantHistoricalLayer = HistoricalLayer.OriginalTemple;
                band.Cleanliness = 0.20f;
                band.Sacredness = 0.55f;
                band.Wealth = 0.10f;
                band.Danger = 0.80f;
                band.Surveillance = 0.60f;
                band.Description = "Foundational water, waste, and sacred remnants around the old core.";
                return band;
            }

            if (t < 0.25f)
            {
                band.DisplayName = index == 1 ? "Pilgrim Intake" : "Lower Labor Wards";
                band.OfficialFunctions.Add(CityFunction.Housing);
                band.OfficialFunctions.Add(CityFunction.Commerce);
                band.OfficialFunctions.Add(CityFunction.Medical);
                band.HiddenFunctions.Add(CityFunction.ResourceExtraction);
                band.HiddenFunctions.Add(CityFunction.Security);
                band.InheritedFunctions.Add(CityFunction.Worship);
                band.DefaultAccessLevel = AccessLevel.SemiPublic;
                band.DominantHistoricalLayer = HistoricalLayer.PilgrimageBoom;
                band.Cleanliness = 0.35f;
                band.Sacredness = 0.50f;
                band.Wealth = 0.20f;
                band.Danger = 0.55f;
                band.Surveillance = 0.65f;
                band.Description = "Arrival, quarantine, labor intake, and dense public life.";
                return band;
            }

            if (t < 0.60f)
            {
                band.DisplayName = "General Wards";
                band.OfficialFunctions.Add(CityFunction.Medical);
                band.OfficialFunctions.Add(CityFunction.Housing);
                band.OfficialFunctions.Add(CityFunction.Worship);
                band.OfficialFunctions.Add(CityFunction.Commerce);
                band.HiddenFunctions.Add(CityFunction.Security);
                band.HiddenFunctions.Add(CityFunction.Data);
                band.InheritedFunctions.Add(CityFunction.Worship);
                band.DefaultAccessLevel = AccessLevel.Public;
                band.DominantHistoricalLayer = HistoricalLayer.HospitalConversion;
                band.Cleanliness = 0.55f;
                band.Sacredness = 0.70f;
                band.Wealth = 0.45f;
                band.Danger = 0.35f;
                band.Surveillance = 0.55f;
                band.Description = "Public-facing care bands with clinics, wards, and ritualized movement.";
                return band;
            }

            if (t < 0.85f)
            {
                band.DisplayName = index == bandCount - 2 ? "Military Recovery Labs" : "Administrative Spine";
                band.OfficialFunctions.Add(CityFunction.Administration);
                band.OfficialFunctions.Add(CityFunction.Research);
                band.OfficialFunctions.Add(CityFunction.Medical);
                band.HiddenFunctions.Add(CityFunction.Military);
                band.HiddenFunctions.Add(CityFunction.Security);
                band.InheritedFunctions.Add(CityFunction.Worship);
                band.DefaultAccessLevel = AccessLevel.Restricted;
                band.DominantHistoricalLayer = HistoricalLayer.MilitaryOccupation;
                band.Cleanliness = 0.75f;
                band.Sacredness = 0.40f;
                band.Wealth = 0.80f;
                band.Danger = 0.65f;
                band.Surveillance = 0.90f;
                band.Description = "Administrative and research bands where care becomes control.";
                return band;
            }

            band.DisplayName = "Growth Crown";
            band.OfficialFunctions.Add(CityFunction.OrganicGrowth);
            band.OfficialFunctions.Add(CityFunction.Research);
            band.HiddenFunctions.Add(CityFunction.ResourceExtraction);
            band.HiddenFunctions.Add(CityFunction.Military);
            band.InheritedFunctions.Add(CityFunction.Medical);
            band.DefaultAccessLevel = AccessLevel.Forbidden;
            band.DominantHistoricalLayer = HistoricalLayer.OrganicGrowth;
            band.Cleanliness = 0.30f;
            band.Sacredness = 0.60f;
            band.Wealth = 0.85f;
            band.Danger = 0.95f;
            band.Surveillance = 0.85f;
            band.OrganicMutation = 1.00f;
            band.Description = "Unstable organic expansion where the city is no longer fully planned.";
            return band;
        }
    }
}
