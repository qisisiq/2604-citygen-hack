using System.Collections.Generic;
using CityGen.Core;
using CityGen.Data;
using UnityEngine;

namespace CityGen.Agents
{
    [CreateAssetMenu(menuName = "CityGen/Agents/Macro Shape Agent", fileName = "MacroShapeAgent")]
    public class MacroShapeAgent : CityAgentBase
    {
        [SerializeField]
        private float defaultOuterRadiusMeters = 280f;

        [SerializeField]
        private float defaultTotalHeightMeters = 1200f;

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

            context.Grid.TotalHeightMeters = Mathf.Max(300f, context.Grid.TotalHeightMeters > 0f ? context.Grid.TotalHeightMeters : defaultTotalHeightMeters);
            context.Grid.OuterRadiusMeters = Mathf.Max(80f, context.Grid.OuterRadiusMeters > 0f ? context.Grid.OuterRadiusMeters : defaultOuterRadiusMeters);
            context.Grid.HeightBands = Mathf.Max(4, context.Grid.HeightBands);
            context.Grid.RadialRings = Mathf.Max(2, context.Grid.RadialRings);
            context.Grid.AngularSectors = Mathf.Max(4, context.Grid.AngularSectors);

            context.Grid.MajorVoids = new List<string>();
            context.Grid.StructuralRibs = new List<string>();

            switch (context.MacroShape)
            {
                case MacroShapeKind.HollowCylinder:
                    context.Grid.HasHollowAtrium = true;
                    context.Grid.CoreRadiusMeters = context.Grid.OuterRadiusMeters * 0.22f;
                    context.Grid.MajorVoids.Add("Central healing atrium");
                    context.Grid.StructuralRibs.Add("Ceremonial outer buttresses");
                    break;

                case MacroShapeKind.SpiralTower:
                    context.Grid.HasHollowAtrium = true;
                    context.Grid.CoreRadiusMeters = context.Grid.OuterRadiusMeters * 0.18f;
                    context.Grid.MajorVoids.Add("Spiral processional gap");
                    context.Grid.StructuralRibs.Add("Helical support spine");
                    break;

                case MacroShapeKind.OvergrownMegastructure:
                    context.Grid.HasHollowAtrium = false;
                    context.Grid.CoreRadiusMeters = context.Grid.OuterRadiusMeters * 0.28f;
                    context.Grid.HasExpansionScars = true;
                    context.Grid.MajorVoids.Add("Collapsed growth cavity");
                    context.Grid.StructuralRibs.Add("Organic support trusses");
                    break;

                default:
                    context.Grid.HasHollowAtrium = true;
                    context.Grid.CoreRadiusMeters = context.Grid.OuterRadiusMeters * 0.20f;
                    context.Grid.MajorVoids.Add("Primary civic void");
                    context.Grid.StructuralRibs.Add("Primary structural rib set");
                    break;
            }

            context.Grid.HasExternalBalconies = true;

            LogDecision(
                context,
                $"Configured macro shape as {context.MacroShape}.",
                "Macro dimensions, core size, voids, and support ribs were normalized for a first-pass cylindrical prototype.");
        }
    }
}
