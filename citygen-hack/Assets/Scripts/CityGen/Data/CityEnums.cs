namespace CityGen.Data
{
    public enum CityFunction
    {
        Housing,
        Food,
        Water,
        Waste,
        Power,
        Air,
        Heat,
        Medical,
        Worship,
        Research,
        Military,
        Administration,
        Data,
        Commerce,
        Transit,
        Maintenance,
        Security,
        Recreation,
        Education,
        Storage,
        ResourceExtraction,
        DeathCare,
        OrganicGrowth
    }

    public enum AccessLevel
    {
        Public,
        SemiPublic,
        StaffOnly,
        Restricted,
        Military,
        Hidden,
        Forbidden
    }

    public enum HistoricalLayer
    {
        OriginalTemple,
        PilgrimageBoom,
        HospitalConversion,
        BureaucraticExpansion,
        MilitaryOccupation,
        ExtractionEra,
        OrganicGrowth,
        CollapseRepair
    }

    public enum TaxonomyKind
    {
        RiverCrossing,
        Port,
        Capital,
        TemplePilgrimage,
        HospitalHealing,
        ResourceExtraction,
        MilitaryResearch,
        Industrial,
        MarketTrade,
        Refugee,
        Religious,
        Corporate,
        OrganicMegastructure,
        IllicitLabor,
        WasteProcessing,
        ResearchMedical
    }

    public enum MacroShapeKind
    {
        PerfectCylinder,
        TaperedTower,
        StackedDiscs,
        HollowCylinder,
        SpiralTower,
        BoneColumn,
        OrganTube,
        ArcologyCylinder,
        FusedTowers,
        RingedTempleHospitalStack,
        OvergrownMegastructure
    }

    public enum RouteKind
    {
        PublicSpine,
        PublicRing,
        PublicBridge,
        ServiceCorridor,
        HiddenPassage,
        RestrictedElevator,
        CoreShaft,
        MaintenanceLadder,
        EmergencyEgress,
        WasteChute
    }

    public enum InfrastructureKind
    {
        Water,
        Food,
        Waste,
        Power,
        Air,
        Heat,
        MedicalSupply,
        Data,
        Security,
        Labor,
        DeathCare,
        OrganicNutrient
    }
}
