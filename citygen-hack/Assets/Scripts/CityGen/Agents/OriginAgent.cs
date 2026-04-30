using System;
using System.Collections.Generic;
using CityGen.Core;
using CityGen.Data;
using UnityEngine;

namespace CityGen.Agents
{
    [CreateAssetMenu(menuName = "CityGen/Agents/Origin Agent", fileName = "OriginAgent")]
    public class OriginAgent : CityAgentBase
    {
        [SerializeField]
        private bool overwriteExistingValues = true;

        public override void Execute(CityContext context)
        {
            if (context == null)
            {
                return;
            }

            if (context.Origin == null)
            {
                context.Origin = new CityOriginData();
            }

            float temple = CityTaxonomyUtility.GetWeight(context.TaxonomyWeights, TaxonomyKind.TemplePilgrimage);
            float hospital = CityTaxonomyUtility.GetWeight(context.TaxonomyWeights, TaxonomyKind.HospitalHealing);
            float military = CityTaxonomyUtility.GetWeight(context.TaxonomyWeights, TaxonomyKind.MilitaryResearch);
            float organic = CityTaxonomyUtility.GetWeight(context.TaxonomyWeights, TaxonomyKind.OrganicMegastructure);

            System.Random random = new System.Random(context.Seed + 101);
            string catalystEntity = ChooseCatalyst(random, temple, hospital, military, organic);
            string foundingInstitution = temple >= military ? "Temple-healer order" : "Medical wardens";
            string originalCenter = temple >= organic ? "Sacred spring inside the old core" : "Biological reactor chamber inside the core";

            if (overwriteExistingValues || string.IsNullOrWhiteSpace(context.Origin.CatalystEntity))
            {
                context.Origin.CatalystEntity = catalystEntity;
            }

            if (overwriteExistingValues || string.IsNullOrWhiteSpace(context.Origin.FoundingInstitution))
            {
                context.Origin.FoundingInstitution = foundingInstitution;
            }

            if (overwriteExistingValues || string.IsNullOrWhiteSpace(context.Origin.FoundingFunction))
            {
                context.Origin.FoundingFunction = hospital >= temple ? "Healing pilgrimage and early care" : "Sacred pilgrimage and ritual healing";
            }

            if (overwriteExistingValues || string.IsNullOrWhiteSpace(context.Origin.OriginalCenter))
            {
                context.Origin.OriginalCenter = originalCenter;
            }

            if (overwriteExistingValues || string.IsNullOrWhiteSpace(context.Origin.FirstSettlementLocation))
            {
                context.Origin.FirstSettlementLocation = "Outer shell sanctuary ring around the first healing site";
            }

            if (overwriteExistingValues || string.IsNullOrWhiteSpace(context.Origin.OriginalCirculationLogic))
            {
                context.Origin.OriginalCirculationLogic = "Pilgrims climbed ceremonial ramps while attendants used hidden core stairs and service shafts.";
            }

            if (overwriteExistingValues || string.IsNullOrWhiteSpace(context.Origin.FoundingReason))
            {
                context.Origin.FoundingReason =
                    $"The city formed around a {context.Origin.CatalystEntity.ToLowerInvariant()}, drawing pilgrims, caretakers, and later institutions that converted ritual healing into managed medicine.";
            }

            if (overwriteExistingValues || context.Origin.OldestSurvivingStructures.Count == 0)
            {
                context.Origin.OldestSurvivingStructures = new List<string>
                {
                    "Old temple stair",
                    "Spring or reactor chamber",
                    "Processional hall",
                    "Reliquary vault"
                };
            }

            LogDecision(
                context,
                $"Set city origin around {context.Origin.CatalystEntity}.",
                "Origin was derived from the strongest healing, temple, military, and organic taxonomy weights.");
        }

        private static string ChooseCatalyst(System.Random random, float temple, float hospital, float military, float organic)
        {
            string[] templeOptions =
            {
                "Sacred spring",
                "Holy wound",
                "Pilgrim reliquary"
            };

            string[] healingOptions =
            {
                "Ancient healing machine",
                "Radiation anomaly",
                "Regenerative medical vault"
            };

            string[] militaryOptions =
            {
                "Containment reactor",
                "Prototype bio-reactor",
                "Strategic med-tech core"
            };

            string[] organicOptions =
            {
                "Alien organ",
                "Living biological reactor",
                "Self-growing tissue chamber"
            };

            if (organic > 0.20f)
            {
                return organicOptions[random.Next(organicOptions.Length)];
            }

            if (military > temple && military > hospital)
            {
                return militaryOptions[random.Next(militaryOptions.Length)];
            }

            if (hospital >= temple)
            {
                return healingOptions[random.Next(healingOptions.Length)];
            }

            return templeOptions[random.Next(templeOptions.Length)];
        }
    }
}
