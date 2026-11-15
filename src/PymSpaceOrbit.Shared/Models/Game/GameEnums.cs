namespace PymSpaceOrbit.Shared.Models.Game;

public enum OutpostDestination
{
    ISS = 1,
    Moon = 2,
    Mars = 3
}

public enum TurnPhase
{
    BuildPhase = 1,
    CardPlayPhase = 2,
    ExplorationDecisionPhase = 3,
    MissionEnded = 4
}

public enum MissionEndingOutcome
{
    InProgress = 0,
    SuccessNominalReturn = 1,
    GameOverEclssAndSuitCollapse = 2,
    GameOverDonkiSolarStormRadiation = 3,
    GameOverCo2AndCaloricOverload = 4
}

public enum CardSlotCategory
{
    ShipAndBaseHull = 1,
    PowerAndThermal = 2,
    MarshallEclssLoop = 3,
    OutpostArchitecture = 4,
    EnvironmentAndSuits = 5,
    DonkiSpaceWeatherCounter = 6
}
