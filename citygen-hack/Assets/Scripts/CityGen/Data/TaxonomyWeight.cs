using System;
using UnityEngine;

namespace CityGen.Data
{
    [Serializable]
    public class TaxonomyWeight
    {
        public TaxonomyKind Kind = TaxonomyKind.TemplePilgrimage;

        [Range(0f, 1f)]
        public float Weight = 0.25f;
    }
}
