using System;
using System.Collections.Generic;
using UnityEngine;

namespace CityGen.Data
{
    [Serializable]
    public class RouteEdge
    {
        public string Id = string.Empty;
        public string FromDistrictId = string.Empty;
        public string ToDistrictId = string.Empty;
        public RouteKind Kind = RouteKind.PublicRing;
        public AccessLevel AccessLevel = AccessLevel.Public;
        public bool IsBidirectional = true;
        public bool IsShortcut;
        public bool IsMajorBackbone;
        public float TravelCost = 1.0f;
        public float VerticalDeltaMeters;
        public List<string> Tags = new List<string>();

        [TextArea(2, 4)]
        public string Notes = string.Empty;
    }
}
