using System;
using UnityEngine;

namespace CityGen.Data
{
    [Serializable]
    public class InfrastructureFlow
    {
        public string Id = string.Empty;
        public InfrastructureKind Kind = InfrastructureKind.Water;
        public string ProducerDistrictId = string.Empty;
        public string ConsumerDistrictId = string.Empty;
        public string RouteId = string.Empty;
        public float Capacity = 1.0f;
        public bool IsVisibleToPlayer;
        public bool UsesCoreShaft = true;
        public AccessLevel AccessLevel = AccessLevel.Hidden;

        [TextArea(2, 4)]
        public string Notes = string.Empty;
    }
}
