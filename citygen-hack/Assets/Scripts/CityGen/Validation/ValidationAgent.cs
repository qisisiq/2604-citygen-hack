using System.Collections.Generic;
using CityGen.Core;
using CityGen.Data;
using UnityEngine;

namespace CityGen.Validation
{
    [CreateAssetMenu(menuName = "CityGen/Agents/Validation Agent", fileName = "ValidationAgent")]
    public class ValidationAgent : CityAgentBase
    {
        [SerializeField]
        private bool requireDistricts = false;

        public override void Execute(CityContext context)
        {
            if (context == null)
            {
                return;
            }

            context.ValidationIssues.Clear();

            ValidateGrid(context);
            ValidateNavigation(context);
            ValidateVerticalBands(context);

            if (requireDistricts && context.Districts.Count == 0)
            {
                context.AddValidationIssue("No districts have been generated yet.");
            }

            string summary = context.ValidationIssues.Count == 0
                ? "Validation passed with no issues."
                : $"Validation found {context.ValidationIssues.Count} issue(s).";

            LogDecision(
                context,
                summary,
                "Validation checked grid dimensions, navigation anchors, and first-pass vertical zoning coverage.");
        }

        private static void ValidateGrid(CityContext context)
        {
            if (context.Grid == null)
            {
                context.AddValidationIssue("City grid is missing.");
                return;
            }

            if (context.Grid.HeightBands < 4)
            {
                context.AddValidationIssue("City grid has fewer than 4 height bands.");
            }

            if (context.Grid.RadialRings < 2)
            {
                context.AddValidationIssue("City grid has fewer than 2 radial rings.");
            }

            if (context.Grid.AngularSectors < 4)
            {
                context.AddValidationIssue("City grid has fewer than 4 angular sectors.");
            }
        }

        private static void ValidateNavigation(CityContext context)
        {
            if (context.Navigation == null)
            {
                context.AddValidationIssue("Navigation principles are missing.");
                return;
            }

            if (string.IsNullOrWhiteSpace(context.Navigation.MainPublicRoute))
            {
                context.AddValidationIssue("Main public route is missing.");
            }

            if (string.IsNullOrWhiteSpace(context.Navigation.MainHiddenRoute))
            {
                context.AddValidationIssue("Main hidden route is missing.");
            }

            if (context.Navigation.MajorLandmarks.Count < 3)
            {
                context.AddValidationIssue("Fewer than 3 major landmarks are defined.");
            }
        }

        private static void ValidateVerticalBands(CityContext context)
        {
            if (context.VerticalBands == null || context.VerticalBands.Count == 0)
            {
                context.AddValidationIssue("No vertical bands have been generated.");
                return;
            }

            if (context.VerticalBands.Count != context.Grid.HeightBands)
            {
                context.AddValidationIssue("Vertical band count does not match the grid height band count.");
            }

            bool hasMedical = false;
            bool hasWorship = false;
            bool hasWaste = false;
            bool hasTopAccess = false;
            HashSet<HistoricalLayer> layers = new HashSet<HistoricalLayer>();

            for (int i = 0; i < context.VerticalBands.Count; i++)
            {
                VerticalBandData band = context.VerticalBands[i];
                if (band == null)
                {
                    context.AddValidationIssue($"Band {i} is null.");
                    continue;
                }

                layers.Add(band.DominantHistoricalLayer);

                if (band.OfficialFunctions.Contains(CityFunction.Medical))
                {
                    hasMedical = true;
                }

                if (band.OfficialFunctions.Contains(CityFunction.Worship) || band.InheritedFunctions.Contains(CityFunction.Worship))
                {
                    hasWorship = true;
                }

                if (band.OfficialFunctions.Contains(CityFunction.Waste) || band.HiddenFunctions.Contains(CityFunction.ResourceExtraction))
                {
                    hasWaste = true;
                }

                if (i == context.VerticalBands.Count - 1 && band.DefaultAccessLevel == AccessLevel.Forbidden)
                {
                    hasTopAccess = true;
                }

                if (i > 0 && !band.ConnectedBandIds.Contains(context.VerticalBands[i - 1].Id))
                {
                    context.AddValidationIssue($"Band {band.Id} is missing a downward connection.");
                }

                if (i < context.VerticalBands.Count - 1 && !band.ConnectedBandIds.Contains(context.VerticalBands[i + 1].Id))
                {
                    context.AddValidationIssue($"Band {band.Id} is missing an upward connection.");
                }
            }

            if (!hasMedical)
            {
                context.AddValidationIssue("No vertical band provides medical function.");
            }

            if (!hasWorship)
            {
                context.AddValidationIssue("No vertical band preserves worship or sacred inheritance.");
            }

            if (!hasWaste)
            {
                context.AddValidationIssue("No vertical band covers waste or extraction logic.");
            }

            if (!hasTopAccess)
            {
                context.AddValidationIssue("Top band is not marked as highly restricted or forbidden.");
            }

            if (layers.Count < 3)
            {
                context.AddValidationIssue("Fewer than 3 historical layers are visible across the vertical bands.");
            }
        }
    }
}
