using System.Collections.Generic;
using CityGen.Data;

namespace CityGen.Agents
{
    internal static class CityTaxonomyUtility
    {
        public static float GetWeight(IReadOnlyList<TaxonomyWeight> weights, TaxonomyKind kind)
        {
            if (weights == null)
            {
                return 0f;
            }

            for (int i = 0; i < weights.Count; i++)
            {
                if (weights[i] != null && weights[i].Kind == kind)
                {
                    return weights[i].Weight;
                }
            }

            return 0f;
        }

        public static TaxonomyKind GetDominantKind(IReadOnlyList<TaxonomyWeight> weights, TaxonomyKind fallback)
        {
            float bestWeight = -1f;
            TaxonomyKind bestKind = fallback;

            if (weights == null)
            {
                return bestKind;
            }

            for (int i = 0; i < weights.Count; i++)
            {
                TaxonomyWeight candidate = weights[i];
                if (candidate == null)
                {
                    continue;
                }

                if (candidate.Weight > bestWeight)
                {
                    bestWeight = candidate.Weight;
                    bestKind = candidate.Kind;
                }
            }

            return bestKind;
        }
    }
}
