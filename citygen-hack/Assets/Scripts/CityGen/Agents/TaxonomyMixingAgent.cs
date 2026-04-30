using System.Collections.Generic;
using CityGen.Core;
using CityGen.Data;
using UnityEngine;

namespace CityGen.Agents
{
    [CreateAssetMenu(menuName = "CityGen/Agents/Taxonomy Mixing Agent", fileName = "TaxonomyMixingAgent")]
    public class TaxonomyMixingAgent : CityAgentBase
    {
        [SerializeField]
        private bool overwriteIdentityText = true;

        public override void Execute(CityContext context)
        {
            if (context == null)
            {
                return;
            }

            if (context.Identity == null)
            {
                context.Identity = new CityIdentitySummary();
            }

            float temple = CityTaxonomyUtility.GetWeight(context.TaxonomyWeights, TaxonomyKind.TemplePilgrimage);
            float hospital = CityTaxonomyUtility.GetWeight(context.TaxonomyWeights, TaxonomyKind.HospitalHealing);
            float extraction = CityTaxonomyUtility.GetWeight(context.TaxonomyWeights, TaxonomyKind.ResourceExtraction);
            float military = CityTaxonomyUtility.GetWeight(context.TaxonomyWeights, TaxonomyKind.MilitaryResearch);
            float organic = CityTaxonomyUtility.GetWeight(context.TaxonomyWeights, TaxonomyKind.OrganicMegastructure);

            TaxonomyKind dominantKind = CityTaxonomyUtility.GetDominantKind(context.TaxonomyWeights, TaxonomyKind.TemplePilgrimage);

            if (overwriteIdentityText || string.IsNullOrWhiteSpace(context.Identity.ClaimedIdentity))
            {
                context.Identity.ClaimedIdentity = temple >= hospital
                    ? "A sacred pilgrimage city devoted to healing."
                    : "A sacred hospital city devoted to care and recovery.";
            }

            if (overwriteIdentityText || string.IsNullOrWhiteSpace(context.Identity.ActualIdentity))
            {
                context.Identity.ActualIdentity = extraction >= 0.15f
                    ? "A managed system that converts patients, labor, and biological byproducts into institutional value."
                    : "A medical bureaucracy that organizes care through surveillance and controlled movement.";
            }

            if (overwriteIdentityText || string.IsNullOrWhiteSpace(context.Identity.LegacyIdentity))
            {
                context.Identity.LegacyIdentity = "An older temple-healing complex built around ritual access to a curative anomaly.";
            }

            if (overwriteIdentityText || string.IsNullOrWhiteSpace(context.Identity.EmergentIdentity))
            {
                context.Identity.EmergentIdentity = organic >= 0.10f
                    ? "A living megastructure that grows new wards when enough suffering, prayer, and treatment waste accumulate."
                    : "A hardened institutional city that keeps absorbing new functions without resolving old contradictions.";
            }

            context.Identity.ConflictingInstitutions = new List<string>
            {
                "Temple-healer order",
                "Hospital bureaucracy",
                "Military research directorate"
            };

            if (extraction >= 0.15f)
            {
                context.Identity.ConflictingInstitutions.Add("Extraction logistics office");
            }

            context.Identity.VisualMotifs = new List<string>
            {
                "Ritual stone merged with clinical metal",
                "Balconies overlooking medical processions",
                "Pipes, reliquaries, and warning glass"
            };

            context.Identity.SocialTensions = new List<string>
            {
                "Care is inseparable from surveillance.",
                "Pilgrimage is inseparable from labor assignment.",
                "Sacred rhetoric hides bureaucratic coercion."
            };

            if (military > 0.10f)
            {
                context.Identity.SocialTensions.Add("Elite treatment zones double as military observation sites.");
            }

            LogDecision(
                context,
                $"Mixed taxonomy into a {dominantKind} dominant identity.",
                "Claimed, actual, legacy, and emergent identities were derived from the weighted taxonomy blend.");
        }
    }
}
