using System;
using System.Collections.Generic;
using UnityEngine;

namespace CityGen.Data
{
    [Serializable]
    public class CityOriginData
    {
        [TextArea(2, 5)]
        public string FoundingReason = "A healing shrine formed around a vertical anomaly.";

        public string FoundingFunction = "Healing pilgrimage";
        public string FoundingInstitution = "Temple-healer order";
        public string CatalystEntity = "Ancient healing machine";
        public string FirstSettlementLocation = "Outer shell sanctuary ring";
        public string OriginalCenter = "Sacred spring inside the old core";

        [TextArea(2, 5)]
        public string OriginalCirculationLogic = "Pilgrims climbed a ceremonial route while attendants moved through hidden core stairs.";

        public List<string> OldestSurvivingStructures = new List<string>();
    }
}
