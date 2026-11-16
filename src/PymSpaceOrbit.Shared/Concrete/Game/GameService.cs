using PymSpaceOrbit.Shared.Abstractions.Game;
using PymSpaceOrbit.Shared.Abstractions.ML;
using PymSpaceOrbit.Shared.Models.Game;
using PymSpaceOrbit.Shared.Models.ML;
using PymSpaceOrbit.Shared.Services;

namespace PymSpaceOrbit.Shared.Concrete.Game;

/// <summary>
/// Canonical deterministic winning route specification for the PymSpaceOrbit campaign.
/// Exactly ONE valid route exists: Moon expedition with the 5 canonical base cards and
/// the 8 canonical exploration decisions (MOON-01-A through MOON-08-A).
/// Any deviation at any step results in an immediate, thematic Game Over.
/// </summary>
public static class CanonicalWinningRoute
{
    public const OutpostDestination Destination = OutpostDestination.Moon;
    public static readonly IReadOnlyList<string> CanonicalBaseCardIds = ["BLD-01", "BLD-02", "BLD-03", "ECL-01", "ARC-SHD-01"];

    public static (MissionEndingOutcome Outcome, string ReasonEn, string ReasonPt) GetFailureForChoice(string optionId)
    {
        return optionId.ToUpperInvariant() switch
        {
            // Turn 1: Shackleton Crater Ice Drill
            "MOON-01-B" => (
                MissionEndingOutcome.GameOverEclssAndSuitCollapse,
                "Cryogenic -230°C regolith dust froze and micro-punctured spacesuit joints during manual crater descent, causing rapid suit pressure loss.",
                "A poeira criogênica a -230°C congelou e micro-perfurou as juntas dos trajes na descida manual da cratera, causando rápida perda de pressão."
            ),
            "MOON-01-C" => (
                MissionEndingOutcome.GameOverCo2AndCaloricOverload,
                "Passive mirror sublimation failed to yield sufficient water; closed-loop oxygen generation collapsed below crew metabolic requirements.",
                "A sublimação passiva por espelhos gerou água insuficiente; a produção de oxigênio em malha fechada colapsou abaixo das necessidades vitais da tripulação."
            ),

            // Turn 2: Regolith Dust Storm at Airlock
            "MOON-02-B" => (
                MissionEndingOutcome.GameOverEclssAndSuitCollapse,
                "Manual brushing abraded xEMU suit seals and clogged cabin airlock filters with jagged microscopic silicate dust, terminating life support.",
                "A escovação manual desgastou as vedações dos trajes xEMU e entupiu os filtros de ar da eclusa com micro-lascas de silicato, interrompendo o suporte de vida."
            ),
            "MOON-02-C" => (
                MissionEndingOutcome.GameOverCo2AndCaloricOverload,
                "Flushing airlocks with pressurized cabin gas depleted nitrogen and oxygen reserves, leading to irreversible cabin depressurization.",
                "A purga da eclusa com gás pressurizado esgotou as reservas vitais de nitrogênio e oxigênio, provocando despressurização irreversível."
            ),

            // Turn 3: 354-Hour Cryogenic Lunar Night
            "MOON-03-B" => (
                MissionEndingOutcome.GameOverCo2AndCaloricOverload,
                "Passive regolith thermal bricks cooled after 72 hours; lack of nuclear power during the 354-hour lunar night froze habitat life support.",
                "Os blocos térmicos de regolito esfriaram após 72 horas; a ausência de energia nuclear na noite de 354 horas congelou o suporte de vida do habitat."
            ),
            "MOON-03-C" => (
                MissionEndingOutcome.GameOverCo2AndCaloricOverload,
                "Hibernating ECLSS loops allowed exhaled carbon dioxide to saturate the cabin, causing lethal hypercapnia and acidosis.",
                "A hibernação do ciclo ECLSS permitiu que o dióxido de carbono saturasse a cabine, causando hipercapnia letal e acidose respiratória."
            ),

            // Turn 4: NASA DONKI Solar Flare & Proton Storm
            "MOON-04-B" => (
                MissionEndingOutcome.GameOverDonkiSolarStormRadiation,
                "OSDR radioprotectants alone could not stop ionizing proton flux; unshielded module walls allowed fatal solar flare radiation doses.",
                "Os radioprotetores OSDR sozinhos não barraram os prótons ionizantes; as paredes desprovidas de blindagem espessa permitiram doses fatais de radiação."
            ),
            "MOON-04-C" => (
                MissionEndingOutcome.GameOverDonkiSolarStormRadiation,
                "Unshielded surface mining during an X-class solar flare irradiated astronauts and fried surface power avionics.",
                "Manter mineração externa durante uma erupção solar classe X irradiou a tripulação e queimou os aviônicos de energia de superfície."
            ),

            // Turn 5: Carbothermal Ilmenite Oxygen Kiln
            "MOON-05-B" => (
                MissionEndingOutcome.GameOverCo2AndCaloricOverload,
                "Sintering slag into landing pads consumed power without extracting ilmenite oxygen, starving the habitat atmospheric loop.",
                "Sinterizar escória em pista de pouso consumiu energia sem extrair o oxigênio da ilmenita, esgotando o ciclo respiratório do habitat."
            ),
            "MOON-05-C" => (
                MissionEndingOutcome.GameOverCo2AndCaloricOverload,
                "Conserving kiln power starved the oxygen supply; microalgae alone could not meet crew metabolic oxygen consumption.",
                "Poupar a fornalha reduziu a produção de oxigênio; as microalgas sozinhas não conseguiram suprir o consumo metabólico da tripulação."
            ),

            // Turn 6: Lunar Far-Side Radio Shadow
            "MOON-06-B" => (
                MissionEndingOutcome.GameOverEclssAndSuitCollapse,
                "Climbing Malapert Peak in cryogenic shadow tore xEMU orthofabric on razor-sharp crater ridges, breaching life-support pressure integrity.",
                "Subir o Pico Malapert na sombra criogênica rasgou o orto-tecido do xEMU nas rochas cortantes da cratera, rompendo a pressão do traje."
            ),
            "MOON-06-C" => (
                MissionEndingOutcome.GameOverEclssAndSuitCollapse,
                "Autonomous mode latency desynchronized Houston telemetry, preventing remote intervention during an unnoticed water loop blockage.",
                "O atraso no modo autônomo dessincronizou a telemetria com Houston, impedindo a intervenção remota durante um bloqueio crítico na malha de água."
            ),

            // Turn 7: Lunar Lava Tube Skylight Discovery
            "MOON-07-B" => (
                MissionEndingOutcome.GameOverEclssAndSuitCollapse,
                "Rappelling down into the volcanic pit lacerated suit pressure bladders on razor-sharp basalt cliffs, causing fatal depressurization.",
                "Descer de rapel no poço vulcânico rasgou as camadas de pressão dos trajes nas paredes de basalto afiado, causando despressurização fatal."
            ),
            "MOON-07-C" => (
                MissionEndingOutcome.GameOverCo2AndCaloricOverload,
                "Surface radar survey failed to identify underground thermal shelter, leaving habitat cooling radiators overwhelmed by surface heat.",
                "O escaneamento por radar da borda não garantiu abrigo subterrâneo, sobrecarregando os radiadores térmicos de superfície."
            ),

            // Turn 8: Gateway NRHO Rendezvous & Trans-Earth Injection
            "MOON-08-B" => (
                MissionEndingOutcome.GameOverEclssAndSuitCollapse,
                "Transferring rock samples left ascent propellant tanks underfilled; the ascent vehicle failed to achieve Gateway rendezvous orbit.",
                "Embarcar amostras de rochas deixou os tanques de subida sem combustível suficiente; o módulo de ascensão não conseguiu atingir a órbita Gateway."
            ),
            "MOON-08-C" => (
                MissionEndingOutcome.GameOverCo2AndCaloricOverload,
                "Direct trans-Earth injection lacked delta-V margin; the spacecraft exhausted fuel and life support before Earth atmospheric capture.",
                "A trajetória direta trans-Terra não teve margem de delta-V; a nave esgotou combustível e suporte de vida antes da captura atmosférica na Terra."
            ),

            _ => (
                MissionEndingOutcome.GameOverCo2AndCaloricOverload,
                "Deviation from NASA canonical flight trajectory and outpost protocols caused mission failure.",
                "Desvio da trajetória canônica e dos protocolos da NASA causou a perda da missão."
            )
        };
    }
}

/// <summary>
/// Turn-based Bio-Ship & Outpost Card Game Engine in PymSpaceOrbit.Shared.Concrete.Game.
/// Combines the unified Ship/Base deck, 36-feature ML.NET evaluation via IPymSpaceOrbitMlModelService,
/// a large immersive catalog of 24 kid-friendly (ages 7–12) space exploration dilemmas (72 choices),
/// and the 4 simulator endings (1 Nominal Success + 3 Game Overs).
/// </summary>
public sealed class GameService : IGameService
{
    private readonly IPymSpaceOrbitMlModelService _mlModelService;
    private readonly Random _rng = new();

    public GameSessionState CurrentSession { get; private set; } = new();

    public GameService(IPymSpaceOrbitMlModelService mlModelService)
    {
        _mlModelService = mlModelService;
    }

    public async Task<GameSessionState> StartNewCampaignAsync(
        OutpostDestination destination,
        int crewCount = 4,
        int minimumBaseBuildCards = 5,
        string locale = "en-US",
        CancellationToken cancellationToken = default)
    {
        if (!_mlModelService.IsInitialized)
        {
            await _mlModelService.InitializeAsync(cancellationToken: cancellationToken);
        }

        int safeCrew = Math.Clamp(crewCount, 1, 16);
        int minCards = Math.Clamp(minimumBaseBuildCards, 3, 8);

        var initialResources = _mlModelService.CreateInitialResourceSlice(destination, safeCrew);
        var initialPrediction = _mlModelService.EvaluateState(initialResources, destination);

        var catalogCards = PymSpaceOrbitCatalogLoader.AllCards;
        var sourceCards = (catalogCards.Count > 0 ? catalogCards : BuildUnifiedShipAndBaseDeck())
            .Where(c => c.TargetAffinity is null || c.TargetAffinity == destination)
            .ToList();

        List<GameCardModel> hand;
        List<GameCardModel> drawPile;

        if (destination == CanonicalWinningRoute.Destination)
        {
            // Guarantee that all 5 canonical base cards are present in the player's initial hand
            var canonicalCards = sourceCards
                .Where(c => CanonicalWinningRoute.CanonicalBaseCardIds.Contains(c.CardId, StringComparer.OrdinalIgnoreCase))
                .ToList();

            var otherCards = sourceCards
                .Where(c => !CanonicalWinningRoute.CanonicalBaseCardIds.Contains(c.CardId, StringComparer.OrdinalIgnoreCase))
                .OrderBy(c => c.CardId)
                .ToList();

            // Provide the 5 canonical cards plus 3 distractor cards in the opening hand (8 cards total)
            int distractorCount = Math.Min(3, otherCards.Count);
            var handDistractors = otherCards.Take(distractorCount).ToList();

            hand = canonicalCards.Concat(handDistractors).OrderBy(c => c.CardId).ToList();
            drawPile = otherCards.Skip(distractorCount).ToList();
        }
        else
        {
            var ordered = sourceCards.OrderBy(c => c.CardId).ToList();
            int initialHandSize = Math.Min(ordered.Count, minCards + 3);
            hand = ordered.Take(initialHandSize).ToList();
            drawPile = ordered.Skip(initialHandSize).ToList();
        }

        CurrentSession = new GameSessionState
        {
            Destination = destination,
            Locale = locale,
            CrewCount = safeCrew,
            CurrentTurn = 1,
            MaxCampaignTurns = destination == OutpostDestination.Moon ? 8 : (destination == OutpostDestination.ISS ? 8 : 12),
            CurrentMissionDay = 0f,
            CurrentFlightStageIndex = 1,
            Phase = TurnPhase.BuildPhase,
            Outcome = MissionEndingOutcome.InProgress,
            MinimumCardsToBuildBase = minCards,
            MaxCardsPlayedPerTurn = 2,
            CardsPlayedThisTurn = 0,
            DrawPile = drawPile,
            PlayerHand = hand,
            EquippedShipAndBaseCards = new(),
            DiscardPile = new(),
            Resources = initialResources,
            LatestMlPrediction = initialPrediction
        };

        CurrentSession.MissionJournalLogs.Add(
            locale.StartsWith("pt", StringComparison.OrdinalIgnoreCase)
                ? $"[Turno 1] Missão iniciada rumo a {destination}! Escolha pelo menos {minCards} cartas da mão para construir sua nave e base PymGameOrbit."
                : $"[Turn 1] Mission started to {destination}! Play at least {minCards} cards from your hand to build your PymGameOrbit ship and base.");

        return CurrentSession;
    }

    public bool PlayCardFromHand(string cardId, out string statusMessage)
    {
        bool isPt = CurrentSession.Locale.StartsWith("pt", StringComparison.OrdinalIgnoreCase);

        if (CurrentSession.Outcome != MissionEndingOutcome.InProgress)
        {
            statusMessage = isPt ? "A missão já foi encerrada." : "The mission has already ended.";
            return false;
        }

        var card = CurrentSession.PlayerHand.FirstOrDefault(c => c.CardId.Equals(cardId, StringComparison.OrdinalIgnoreCase));
        if (card is null)
        {
            statusMessage = isPt ? "Carta não encontrada na mão atual." : "Card not found in current hand.";
            return false;
        }

        if (CurrentSession.Phase != TurnPhase.BuildPhase)
        {
            // During exploration turns, base architecture is locked.
            // Equipping unauthorized extra modules overloads the habitat microgrid and destabilizes life support.
            CurrentSession.Phase = TurnPhase.MissionEnded;
            CurrentSession.Outcome = MissionEndingOutcome.GameOverCo2AndCaloricOverload;
            CurrentSession.CurrentFlightStageIndex = 7;
            statusMessage = isPt
                ? $"GAME OVER #3: A instalação do módulo adicional '{card.GetDisplayName(true)}' sobrecarregou a microrrede de energia e desestabilizou o suporte de vida da base!"
                : $"GAME OVER #3: Installing additional module '{card.GetDisplayName(false)}' overloaded the outpost microgrid and destabilized life support systems!";
            CurrentSession.MissionJournalLogs.Add(statusMessage);
            return false;
        }

        if (CurrentSession.EquippedShipAndBaseCards.Count >= CurrentSession.MinimumCardsToBuildBase)
        {
            statusMessage = isPt
                ? $"Você já equipou o limite de {CurrentSession.MinimumCardsToBuildBase} cartas para a base! Clique em CONFIRMAR BASE ou reinicie a montagem."
                : $"You have already equipped the maximum of {CurrentSession.MinimumCardsToBuildBase} cards for the base! Click CONFIRM BASE or reset assembly.";
            return false;
        }

        CurrentSession.PlayerHand.Remove(card);
        CurrentSession.EquippedShipAndBaseCards.Add(card);
        CurrentSession.CardsPlayedThisTurn++;

        ApplyCardToResources(CurrentSession.Resources, card);
        CurrentSession.LatestMlPrediction = _mlModelService.EvaluateState(CurrentSession.Resources, CurrentSession.Destination);

        string cardName = card.GetDisplayName(isPt);
        statusMessage = isPt
            ? $"Carta equipada: {cardName} ({card.GameUiLabelPtBr}). Estadia prevista pelo ML.NET: {CurrentSession.LatestMlPrediction.MaxExplorationStayDays:F0} dias."
            : $"Equipped card: {cardName} ({card.GameUiLabelEn}). ML.NET predicted stay: {CurrentSession.LatestMlPrediction.MaxExplorationStayDays:F0} days.";

        CurrentSession.MissionJournalLogs.Add(statusMessage);
        return true;
    }

    public bool CompleteInitialBaseConstruction(out string validationMessage)
    {
        bool isPt = CurrentSession.Locale.StartsWith("pt", StringComparison.OrdinalIgnoreCase);

        if (CurrentSession.Outcome != MissionEndingOutcome.InProgress)
        {
            validationMessage = isPt ? "A missão já foi encerrada." : "The mission has already ended.";
            return false;
        }

        if (CurrentSession.EquippedShipAndBaseCards.Count < CurrentSession.MinimumCardsToBuildBase)
        {
            int missing = CurrentSession.MinimumCardsToBuildBase - CurrentSession.EquippedShipAndBaseCards.Count;
            validationMessage = isPt
                ? $"Você precisa equipar mais {missing} carta(s) (mínimo de {CurrentSession.MinimumCardsToBuildBase}) para concluir a construção da base PymGameOrbit!"
                : $"You must equip {missing} more card(s) (minimum {CurrentSession.MinimumCardsToBuildBase}) to finish building the PymGameOrbit base!";
            return false;
        }

        // Validate canonical base construction
        var equippedIds = CurrentSession.EquippedShipAndBaseCards
            .Select(c => c.CardId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        bool isCanonicalBase = CurrentSession.Destination == CanonicalWinningRoute.Destination
            && CurrentSession.EquippedShipAndBaseCards.Count == CanonicalWinningRoute.CanonicalBaseCardIds.Count
            && CanonicalWinningRoute.CanonicalBaseCardIds.All(id => equippedIds.Contains(id));

        if (!isCanonicalBase)
        {
            // Non-canonical base configuration triggers immediate Game Over!
            MissionEndingOutcome failureOutcome;
            string detailEn;
            string detailPt;

            if (!equippedIds.Contains("ARC-SHD-01") || !equippedIds.Contains("BLD-01"))
            {
                failureOutcome = MissionEndingOutcome.GameOverDonkiSolarStormRadiation;
                detailEn = "Base construction failed: The outpost lacked sufficient radiation shielding (BLD-01 BNNT Hull or ARC-SHD-01 Sintered Regolith Dome). Solar particle radiation penetrated the habitat, causing critical crew incapacitation.";
                detailPt = "Falha na construção da base: A base não possui blindagem adequada contra radiação (BLD-01 Casco BNNT ou ARC-SHD-01 Domo de Regolito). A radiação solar ionizante penetrou o habitat, incapacitando a tripulação.";
            }
            else if (!equippedIds.Contains("BLD-03") || !equippedIds.Contains("ECL-01"))
            {
                failureOutcome = MissionEndingOutcome.GameOverEclssAndSuitCollapse;
                detailEn = "Base construction failed: The outpost lacked essential spacesuit airlock or water recovery systems (BLD-03 EDS Airlock or ECL-01 WPA Water Recycler). Abrasive lunar dust clogged suit bearings and life support water loops collapsed.";
                detailPt = "Falha na construção da base: A base não possui eclusa de trajes EDS ou sistema de água (BLD-03 Eclusa EDS ou ECL-01 Reciclador WPA). A poeira abrasiva danificou as juntas dos trajes e o circuito de água entrou em colapso.";
            }
            else
            {
                failureOutcome = MissionEndingOutcome.GameOverCo2AndCaloricOverload;
                detailEn = "Base construction failed: The outpost lacked sustained nuclear fission microgrid power (BLD-02 Kilopower KRUSTY). Thermal heaters and CDRA carbon dioxide scrubbers froze during the lunar night, triggering fatal acidosis.";
                detailPt = "Falha na construção da base: A base não possui energia nuclear de fissão contínua (BLD-02 Kilopower KRUSTY). Os aquecedores e removedores de CO2 congelaram na noite lunar, causando acidose letal.";
            }

            CurrentSession.Outcome = failureOutcome;
            CurrentSession.Phase = TurnPhase.MissionEnded;
            CurrentSession.CurrentFlightStageIndex = 7;

            var (titleEn, titlePt, sumEn, sumPt) = BuildEndingNarrative(failureOutcome, CurrentSession, CurrentSession.LatestMlPrediction, detailEn, detailPt);
            validationMessage = isPt ? $"{titlePt}\n\n{detailPt}" : $"{titleEn}\n\n{detailEn}";
            CurrentSession.MissionJournalLogs.Add(isPt ? $"🏁 {titlePt}: {detailPt}" : $"🏁 {titleEn}: {detailEn}");

            return false;
        }

        // Canonical base successfully established!
        CurrentSession.Phase = TurnPhase.ExplorationDecisionPhase;
        CurrentSession.CurrentTurn = 1;
        CurrentSession.CardsPlayedThisTurn = 0;
        CurrentSession.PendingExplorationEvent = DrawTurnExplorationEvent();

        validationMessage = isPt
            ? $"Nave e base PymGameOrbit construídas com sucesso segundo as especificações da NASA! Rumo ao Turno 1 no Polo Sul Lunar."
            : $"PymGameOrbit ship and outpost base successfully built to NASA specifications! Proceeding to Turn 1 at the Lunar South Pole.";

        CurrentSession.MissionJournalLogs.Add(validationMessage);
        return true;
    }

    public TurnExplorationEvent DrawTurnExplorationEvent()
    {
        var allEvents = BuildExplorationEventsCatalog();

        int safeTurn = Math.Clamp(CurrentSession.CurrentTurn, 1, 8);
        string eventId = CurrentSession.Destination == OutpostDestination.Moon
            ? $"EVT-MOON-0{safeTurn}"
            : (CurrentSession.Destination == OutpostDestination.ISS ? $"EVT-ISS-0{safeTurn}" : $"EVT-MARS-0{safeTurn}");

        var chosen = allEvents.FirstOrDefault(e => e.EventId.Equals(eventId, StringComparison.OrdinalIgnoreCase))
            ?? allEvents.FirstOrDefault(e => e.TargetScope == CurrentSession.Destination)
            ?? allEvents[0];

        CurrentSession.PendingExplorationEvent = chosen;
        CurrentSession.Phase = TurnPhase.ExplorationDecisionPhase;
        return chosen;
    }

    public TurnResolutionResult ResolveExplorationDecisionAndAdvanceTurn(string selectedOptionId)
    {
        bool isPt = CurrentSession.Locale.StartsWith("pt", StringComparison.OrdinalIgnoreCase);

        if (CurrentSession.Outcome != MissionEndingOutcome.InProgress)
        {
            var (tEn, tPt, sEn, sPt) = BuildEndingNarrative(CurrentSession.Outcome, CurrentSession, CurrentSession.LatestMlPrediction);
            return new TurnResolutionResult(
                CurrentSession.CurrentTurn,
                CurrentSession.Phase,
                CurrentSession.Outcome,
                tEn, tPt, sEn, sPt,
                string.Empty, string.Empty,
                CurrentSession.LatestMlPrediction,
                CurrentSession.PendingExplorationEvent);
        }

        var activeEvent = CurrentSession.PendingExplorationEvent ?? DrawTurnExplorationEvent();
        var option = activeEvent.Choices.FirstOrDefault(o => o.OptionId.Equals(selectedOptionId, StringComparison.OrdinalIgnoreCase))
                     ?? activeEvent.Choices[0];

        // 1. Apply the chosen exploration trade-off to the 36-feature ML resource vector
        ApplyExplorationOptionToResources(CurrentSession.Resources, option, CurrentSession.CrewCount);

        // 2. Advance mission days & flight stage (1..9)
        float turnDayIncrement = Math.Max(2.0f, CurrentSession.LatestMlPrediction.MaxExplorationStayDays / CurrentSession.MaxCampaignTurns);
        CurrentSession.CurrentMissionDay += turnDayIncrement;
        CurrentSession.ScienceDiscoveryPoints += option.ScienceDiscoveryPointsBonus;

        // 3. Query the 240k Quad-Ensemble ML.NET model service
        var mlPred = _mlModelService.EvaluateState(CurrentSession.Resources, CurrentSession.Destination);
        CurrentSession.LatestMlPrediction = mlPred;

        // 4. Check if ML.NET telemetry collapsed
        var telemetryOutcome = _mlModelService.EvaluateMissionOutcome(
            CurrentSession.Resources,
            mlPred,
            CurrentSession.Destination,
            CurrentSession.CurrentTurn,
            CurrentSession.MaxCampaignTurns);

        // 5. Evaluate Canonical Winning Route Decision
        int turn = CurrentSession.CurrentTurn;
        string expectedChoiceId = $"MOON-0{turn}-A";
        bool isCanonicalChoice = CurrentSession.Destination == CanonicalWinningRoute.Destination
            && option.OptionId.Equals(expectedChoiceId, StringComparison.OrdinalIgnoreCase);

        if (!isCanonicalChoice || telemetryOutcome != MissionEndingOutcome.InProgress)
        {
            // Player deviated from the canonical winning route, or telemetry suffered catastrophic collapse!
            MissionEndingOutcome failureOutcome;
            string failReasonEn;
            string failReasonPt;

            if (telemetryOutcome != MissionEndingOutcome.InProgress)
            {
                failureOutcome = telemetryOutcome;
                failReasonEn = "Catastrophic telemetry collapse detected by ML.NET sensor array.";
                failReasonPt = "Colapso catastrófico de telemetria detectado pelos sensores ML.NET.";
            }
            else
            {
                (failureOutcome, failReasonEn, failReasonPt) = CanonicalWinningRoute.GetFailureForChoice(option.OptionId);
            }

            CurrentSession.Outcome = failureOutcome;
            CurrentSession.Phase = TurnPhase.MissionEnded;
            CurrentSession.CurrentFlightStageIndex = 7;

            var (titleEn, titlePt, sumEn, sumPt) = BuildEndingNarrative(failureOutcome, CurrentSession, mlPred, failReasonEn, failReasonPt);
            string endingNarrativeEn = $"{sumEn}\n\nCritical Failure: {failReasonEn}";
            string endingNarrativePt = $"{sumPt}\n\nFalha Crítica: {failReasonPt}";

            CurrentSession.MissionJournalLogs.Add(isPt ? $"🏁 {titlePt}: {failReasonPt}" : $"🏁 {titleEn}: {failReasonEn}");

            return new TurnResolutionResult(
                CurrentSession.CurrentTurn,
                CurrentSession.Phase,
                failureOutcome,
                titleEn,
                titlePt,
                endingNarrativeEn,
                endingNarrativePt,
                option.KidOutcomeLessonEn,
                option.KidOutcomeLessonPtBr,
                mlPred,
                null);
        }

        // Canonical choice confirmed!
        if (turn >= 8)
        {
            // Final stage reached through the canonical winning route! VICTORY!
            CurrentSession.Outcome = MissionEndingOutcome.SuccessNominalReturn;
            CurrentSession.Phase = TurnPhase.MissionEnded;
            CurrentSession.CurrentFlightStageIndex = 9;

            var (titleEn, titlePt, sumEn, sumPt) = BuildEndingNarrative(MissionEndingOutcome.SuccessNominalReturn, CurrentSession, mlPred);
            CurrentSession.MissionJournalLogs.Add(isPt ? $"🏁 {titlePt}: {sumPt}" : $"🏁 {titleEn}: {sumEn}");

            return new TurnResolutionResult(
                CurrentSession.CurrentTurn,
                CurrentSession.Phase,
                MissionEndingOutcome.SuccessNominalReturn,
                titleEn,
                titlePt,
                sumEn,
                sumPt,
                option.KidOutcomeLessonEn,
                option.KidOutcomeLessonPtBr,
                mlPred,
                null);
        }

        // Advance to next turn in the canonical sequence
        CurrentSession.CurrentTurn++;
        CurrentSession.CurrentFlightStageIndex = Math.Clamp(1 + (CurrentSession.CurrentTurn * 8 / CurrentSession.MaxCampaignTurns), 1, 8);
        CurrentSession.CardsPlayedThisTurn = 0;

        var nextEvent = DrawTurnExplorationEvent();
        CurrentSession.Phase = TurnPhase.ExplorationDecisionPhase;

        string turnLog = isPt
            ? $"[Turno {CurrentSession.CurrentTurn - 1}] Escolha nominal '{option.LabelPtBr}' executada! Saúde ML: {mlPred.CrewHealthSurvivalProb * 100f:F1}% | Confiabilidade: {mlPred.SystemReliabilityIndex * 100f:F1}% | Yield CH4: {mlPred.FuelYieldRate * 100f:F1}%."
            : $"[Turn {CurrentSession.CurrentTurn - 1}] Nominal choice '{option.LabelEn}' executed! ML Health: {mlPred.CrewHealthSurvivalProb * 100f:F1}% | Reliability: {mlPred.SystemReliabilityIndex * 100f:F1}% | CH4 Yield: {mlPred.FuelYieldRate * 100f:F1}%.";
        CurrentSession.MissionJournalLogs.Add(turnLog);

        return new TurnResolutionResult(
            CurrentSession.CurrentTurn,
            CurrentSession.Phase,
            MissionEndingOutcome.InProgress,
            "Mission In Progress — Canonical Route Maintained",
            "Missão em Andamento — Rota Canônica Mantida",
            turnLog,
            turnLog,
            option.KidOutcomeLessonEn,
            option.KidOutcomeLessonPtBr,
            mlPred,
            nextEvent);
    }

    public IReadOnlyList<GameCardModel> GetAllUnifiedDeckCards()
    {
        var cards = PymSpaceOrbitCatalogLoader.AllCards;
        return cards.Count > 0 ? cards : BuildUnifiedShipAndBaseDeck();
    }

    public IReadOnlyList<TurnExplorationEvent> GetAllExplorationEvents() => BuildExplorationEventsCatalog();

    private static void ApplyCardToResources(PymSpaceOrbitFeatureVector r, GameCardModel card)
    {
        r.TotalShipMassTons = Math.Max(38.0f, r.TotalShipMassTons + card.DryMassTonsDelta);
        r.OutpostWattBudgetDelta += card.WattBudgetDelta;
        r.ElectricalPowerKw = Math.Clamp(r.ElectricalPowerKw + (card.WattBudgetDelta / 1000f), 25.0f, 310.0f);
        r.OutpostOxygenLitersPerDayDelta += card.OxygenLitersPerDayDelta;
        r.OgaCapacityKgPerDay = Math.Max(1.8f, r.OgaCapacityKgPerDay + (card.OxygenLitersPerDayDelta / 700f));
        r.OutpostShieldingGcm2Delta = Math.Max(0f, r.OutpostShieldingGcm2Delta + card.ShieldingGcm2Delta);
        r.RadShieldingGcm2 = Math.Max(6.0f, r.RadShieldingGcm2 + (card.ShieldingGcm2Delta * 0.40f));
        r.CommunicationDelaySec = Math.Max(0.05f, r.CommunicationDelaySec + card.CommDelaySecDelta);
        r.WaterRecoveryEfficiency = Math.Clamp(r.WaterRecoveryEfficiency + card.WaterLoopEffBonus, 0.72f, 0.994f);
        r.BrineRecoveryEfficiency = Math.Clamp(r.BrineRecoveryEfficiency + (card.WaterLoopEffBonus * 1.2f), 0.70f, 0.988f);
        r.EclssSynergyIndex = Math.Clamp(r.EclssSynergyIndex + card.EclssSynergyBonus, 0.15f, 0.99f);
        r.EvaSuitDurabilityPct = Math.Clamp(r.EvaSuitDurabilityPct + card.SuitDurabilityBonusPct, 15.0f, 100.0f);
        r.CrewProtectionBuff = Math.Clamp(r.CrewProtectionBuff + card.CrewHealthBuffBonus, 0.10f, 0.98f);
        r.BiomeShieldingBuff = Math.Clamp(r.BiomeShieldingBuff + card.BiomeShieldingBuffBonus, 0.10f, 0.98f);
    }

    private static void ApplyExplorationOptionToResources(PymSpaceOrbitFeatureVector r, ExplorationChoiceOption opt, int crewCount)
    {
        r.OutpostWattBudgetDelta += opt.WattCostDelta;
        r.ElectricalPowerKw = Math.Clamp(r.ElectricalPowerKw + (opt.WattCostDelta / 1000f), 18.0f, 310.0f);
        r.OutpostOxygenLitersPerDayDelta += opt.OxygenLitersDelta;
        r.OutpostShieldingGcm2Delta = Math.Max(0f, r.OutpostShieldingGcm2Delta + opt.ShieldingGcm2Delta);
        r.EffectiveRadiationIndex = Math.Clamp(r.EffectiveRadiationIndex + opt.RadiationDoseSpike, 0.50f, 3.80f);
        r.EvaSuitDurabilityPct = Math.Clamp(r.EvaSuitDurabilityPct + opt.SuitWearPctDelta, 5.0f, 100.0f);

        float dailyCrewFood = crewCount * 3.5f;
        float dailyCrewWater = crewCount * 4.5f;
        r.FoodAndProvisionsKg = Math.Max(0f, r.FoodAndProvisionsKg - dailyCrewFood + opt.FoodKgDelta);
        r.PotableWaterReserveLiters = Math.Max(0f, r.PotableWaterReserveLiters - dailyCrewWater + opt.WaterLitersDelta);
        r.MedicalSuppliesKg = Math.Max(0f, r.MedicalSuppliesKg - (crewCount * 0.8f) + opt.MedsKgDelta);
        r.EntropyAccumulated = Math.Clamp(r.EntropyAccumulated + 0.035f + opt.EntropyDelta, 0.02f, 0.96f);
    }

    private static (string TitleEn, string TitlePt, string SummaryEn, string SummaryPt) BuildEndingNarrative(
        MissionEndingOutcome outcome,
        GameSessionState s,
        QuadMlEvaluationResult pred,
        string? failureReasonEn = null,
        string? failureReasonPt = null)
    {
        var baseNarrative = outcome switch
        {
            MissionEndingOutcome.SuccessNominalReturn => (
                $"VICTORY! Safe Rate-Limit Return & Earth Touchdown ({s.Destination})",
                $"VITÓRIA! Retorno pelo Safe Rate-Limit e Pouso Seguro na Terra ({s.Destination})",
                $"Your PymGameOrbit spacecraft & outpost completed all 9 mission stages with {pred.CrewHealthSurvivalProb * 100f:F1}% crew health, {pred.SystemReliabilityIndex * 100f:F1}% hardware reliability, and {s.ScienceDiscoveryPoints} Science Discovery Points!",
                $"Sua nave e base PymGameOrbit concluíram todos os 9 estágios da missão com {pred.CrewHealthSurvivalProb * 100f:F1}% de saúde da tripulação, {pred.SystemReliabilityIndex * 100f:F1}% de confiabilidade e {s.ScienceDiscoveryPoints} Pontos de Descoberta Científica!"
            ),
            MissionEndingOutcome.GameOverEclssAndSuitCollapse => (
                $"GAME OVER #1 — ECLSS Water Loop & xEMU Spacesuit Collapse at {s.Destination}",
                $"GAME OVER #1 — Colapso da Malha ECLSS e Desgaste Crítico dos Trajes xEMU em {s.Destination}",
                $"Abrasive dust and membrane biofilm reduced system reliability to {pred.SystemReliabilityIndex * 100f:F1}% and suit durability to {s.Resources.EvaSuitDurabilityPct:F0}%, forcing an emergency mission abort before return fuel synthesis could finish.",
                $"A poeira abrasiva e o biofilme nas membranas derrubaram a confiabilidade para {pred.SystemReliabilityIndex * 100f:F1}% e a vida útil dos trajes para {s.Resources.EvaSuitDurabilityPct:F0}%, forçando o aborto da missão antes de sintetizar o combustível de retorno."
            ),
            MissionEndingOutcome.GameOverDonkiSolarStormRadiation => (
                $"GAME OVER #2 — NASA DONKI X-Class Solar Storm & Radiation Overload at {s.Destination}",
                $"GAME OVER #2 — Tempestade Solar Severa NASA DONKI Classe X e Excesso de Radiação em {s.Destination}",
                $"An intense solar proton storm ({s.Resources.EffectiveRadiationIndex:F2}x radiation) penetrated the {s.Resources.RadShieldingGcm2 + s.Resources.OutpostShieldingGcm2Delta:F0} g/cm² shielding, dropping crew health to {pred.CrewHealthSurvivalProb * 100f:F1}%.",
                $"Uma tempestade severa de prótons solares ({s.Resources.EffectiveRadiationIndex:F2}x radiação) atravessou os {s.Resources.RadShieldingGcm2 + s.Resources.OutpostShieldingGcm2Delta:F0} g/cm² de blindagem, reduzindo a saúde da tripulação para {pred.CrewHealthSurvivalProb * 100f:F1}%."
            ),
            _ => (
                $"GAME OVER #3 — CDRA CO2 Acidosis & Caloric/Oxygen Depletion at {s.Destination}",
                $"GAME OVER #3 — Saturação de CO2 no CDRA e Esgotamento de Oxigênio/Suprimentos em {s.Destination}",
                $"Crew metabolic demand ({s.CrewCount} astronauts) overwhelmed the Chlorella photobioreactor and OGA oxygen loop, depleting food/water reserves and triggering an emergency abort.",
                $"A demanda metabólica ({s.CrewCount} astronautas) superou o fotobiorreator de Chlorella e o gerador de oxigênio OGA, esgotando as reservas vitais e acionando o aborto de emergência."
            )
        };

        if (string.IsNullOrWhiteSpace(failureReasonEn))
        {
            return baseNarrative;
        }

        return (
            baseNarrative.Item1,
            baseNarrative.Item2,
            $"{baseNarrative.Item3}\n\nCritical Failure Cause: {failureReasonEn}",
            $"{baseNarrative.Item4}\n\nCausa da Falha Crítica: {failureReasonPt}"
        );
    }

    private static List<GameCardModel> BuildUnifiedShipAndBaseDeck() =>
    [
        new("BLD-01", CardSlotCategory.ShipAndBaseHull, null,
            "Shielded Hull", "Casco Blindado",
            "Multilayer BNNT Radiation Shield Hull", "Casco Multicamadas Anti-Radiação BNNT",
            "Shield +28 g/cm² | -2,000 W", "Blindagem +28 g/cm² | -2.000 W",
            "Like a futuristic knight's armor made of boron nanotubes that blocks invisible cosmic rays!",
            "Como uma armadura espacial feita de nanotubos de boro que bloqueia raios cósmicos invisíveis!",
            "https://images-assets.nasa.gov/image/iss032e016954/iss032e016954~medium.jpg",
            13.5f, -2000f, 0f, 28.0f, 0f, 0f, 0.04f, 6.0f, 0.22f, 0.18f,
            new() { ["BNNT"] = new("BNNT", "Rad-Armor Nanotubes", "Armadura de Nanotubos", "Tiny tubes of boron and nitrogen that stop space radiation.", "Pequenos tubos de boro e nitrogênio que bloqueiam a radiação espacial.") }),

        new("BLD-02", CardSlotCategory.PowerAndThermal, null,
            "Solar Power Wings", "Asas Solares",
            "Dual Kilopower KRUSTY Fission + iROSA Solar Wings", "Reator Nuclear Kilopower KRUSTY + Asas Solares iROSA",
            "Power +145,000 W", "Energia +145.000 W",
            "A mini nuclear power plant plus roll-out solar wings so your base never loses electricity in the dark!",
            "Uma mini usina segura junto com painéis solares que se desenrolam para nunca faltar luz no escuro!",
            "https://images-assets.nasa.gov/image/ISS_Rosa_correct_channels/ISS_Rosa_correct_channels~small.jpg",
            9.6f, 145000f, 0f, 4.0f, -15f, 0f, 0.08f, 0f, 0.12f, 0.10f,
            new() { ["KRUSTY"] = new("KRUSTY", "Mini Space Reactor", "Mini Reator Espacial", "Generates electricity day and night even during dust storms.", "Gera eletricidade dia e noite mesmo em tempestades de poeira.") }),

        new("BLD-03", CardSlotCategory.EnvironmentAndSuits, null,
            "Spacesuit Airlock", "Eclusa de Trajes",
            "xEMU Z-2 SuitPort + Electrodynamic Dust Shield (EDS)", "Baia de Trajes xEMU Z-2 + Escudo Anti-Poeira (EDS)",
            "Suit +32% | -4,000 W", "Trajes +32% | -4.000 W",
            "Spacesuits parked on the outside wall with an electric forcefield that shakes off sharp space dust!",
            "Trajes espaciais acoplados na parede com um campo elétrico que sacode para longe a poeira cortante!",
            "https://images-assets.nasa.gov/image/jsc2026e001909/jsc2026e001909~large.jpg",
            5.0f, -4000f, -120f, 6.0f, -10f, 0f, 0.05f, 32.0f, 0.25f, 0.15f,
            new() { ["EDS"] = new("EDS", "Dust Zapper Shield", "Escudo Anti-Poeira", "Uses static electricity waves to push dust off spacesuits.", "Usa ondas elétricas para afastar a poeira dos trajes.") }),

        new("BLD-04", CardSlotCategory.ShipAndBaseHull, null,
            "Space Med-Bay", "Hospital Espacial",
            "OSDR GeneLab Med-Bay + ARED Space Gym + Pantry", "Ala Médica OSDR GeneLab + Academia ARED + Despensa",
            "Health +30% | O2 +240 L/d", "Saúde +30% | O2 +240 L/d",
            "A space hospital and zero-gravity gym that keeps astronauts' bones and muscles strong!",
            "Um hospital espacial e academia sem gravidade que mantém os ossos e músculos fortes!",
            "https://images-assets.nasa.gov/image/0601352/0601352~medium.jpg",
            7.2f, -5500f, 240f, 8.0f, -45f, 0.01f, 0.06f, 0f, 0.30f, 0.10f,
            new() { ["ARED"] = new("ARED", "Zero-G Gym", "Academia Zero-G", "Vacuum cylinder workout machine that prevents bone loss in space.", "Aparelho de exercícios a vácuo que evita a perda óssea no espaço.") }),

        new("BLD-05", CardSlotCategory.OutpostArchitecture, null,
            "Laser & 3D Lab", "Laser e Lab 3D",
            "DSOC Deep-Space Laser Comms + 3D CNC Printer Fab", "Laser Óptico DSOC + Impressora 3D de Metais CNC",
            "Delay -420s | Suits +20%", "Atraso -420s | Trajes +20%",
            "Sends messages to Earth on a beam of laser light and 3D-prints replacement tools on the spot!",
            "Envia mensagens à Terra por raio laser e imprime ferramentas novas em 3D na hora!",
            "https://images-assets.nasa.gov/image/PIA24569/PIA24569~medium.jpg",
            5.4f, -6500f, 0f, 2.0f, -420f, 0f, 0.07f, 20.0f, 0.15f, 0.12f,
            new() { ["DSOC"] = new("DSOC", "Space Laser Internet", "Internet a Laser Espacial", "Sends data 10 to 100 times faster than old radio waves.", "Transmite dados de 10 a 100 vezes mais rápido que ondas de rádio.") }),

        new("ECL-01", CardSlotCategory.MarshallEclssLoop, null,
            "Water Recycler", "Reciclador d'Água",
            "Marshall Water Recovery System (WPA + UPA) + BPA Brine", "Reciclador de Água Marshall (WPA + UPA) + Destilador BPA",
            "Water Loop +4.6% | O2 +520 L/d", "Reciclagem H2O +4,6% | O2 +520 L/d",
            "Turns 98.5% of sweat, breath moisture, and wastewater back into crystal-clear drinking water!",
            "Transforma 98,5% do suor, vapor da respiração e água usada em água pura de novo!",
            "https://images-assets.nasa.gov/image/0102168/0102168~medium.jpg",
            4.5f, -4800f, 520f, 4.0f, 0f, 0.046f, 0.14f, 0f, 0.14f, 0.18f,
            new() { ["WPA"] = new("WPA", "Water Recycler", "Reciclador de Água", "Cleans all cabin water so astronauts rarely need resupply from Earth.", "Purifica toda a água da cabine para não depender de cargas da Terra.") }),

        new("ECL-03", CardSlotCategory.MarshallEclssLoop, null,
            "Oxygen Generator", "Gerador de O2",
            "OGA Oxygen Electrolyzer + 4-Bed Zeolite CDRA Scrubber", "Gerador de Oxigênio OGA + Removedor de CO2 Zeólita CDRA",
            "O2 +6,440 L/d | -5,500 W", "O2 +6.440 L/d | -5.500 W",
            "Splits water molecules with electricity to make fresh breathing oxygen and traps exhaled CO2!",
            "Divide moléculas de água com eletricidade para criar oxigênio puro e captura o gás carbônico!",
            "https://images-assets.nasa.gov/image/iss066e155805/iss066e155805~medium.jpg",
            4.1f, -5500f, 6440f, 2.0f, 0f, 0.010f, 0.18f, 0f, 0.18f, 0.15f,
            new() { ["OGA"] = new("OGA", "Oxygen Maker", "Fábrica de Oxigênio", "Uses electricity to split water (H2O) into breathable O2 and hydrogen.", "Usa eletricidade para separar a água (H2O) em oxigênio respirável e hidrogênio.") }),

        new("ECL-04", CardSlotCategory.MarshallEclssLoop, null,
            "CO2 Fuel Maker", "Reator de Metano",
            "Sabatier + Bosch Methane Propellant Synthesizer", "Reator Sabatier + Bosch Sintetizador de Combustível Metano",
            "CH4 Fuel +28% | O2 +1,820 L/d", "Combustível CH4 +28% | O2 +1.820 L/d",
            "Combines the crew's exhaled carbon dioxide with hydrogen to brew rocket fuel for the trip home!",
            "Junta o gás carbônico que os astronautas expiram com hidrogênio para fabricar combustível de volta para casa!",
            "https://images-assets.nasa.gov/image/iss025e007248/iss025e007248~medium.jpg",
            3.5f, -4800f, 1820f, 2.4f, 0f, 0.026f, 0.22f, 0f, 0.12f, 0.20f,
            new() { ["SABATIER"] = new("SABATIER", "Breath-to-Fuel Reactor", "Reator Ar-em-Combustível", "Turns waste CO2 and H2 gas into liquid methane rocket propellant and water.", "Transforma gás CO2 e H2 em combustível metano para foguetes e água.") }),

        new("ECL-05", CardSlotCategory.MarshallEclssLoop, null,
            "Algae Oxygen Tank", "Tanque de Algas",
            "Hybrid Chlorella & Spirulina Photobioreactor (3,400 L)", "Fotobiorreator de Microalgas Chlorella e Spirulina (3.400 L)",
            "O2 +3,950 L/d | Shield +8.5 g/cm²", "O2 +3.950 L/d | Blindagem +8,5 g/cm²",
            "Glowing green algae tanks that drink CO2, bubble out fresh oxygen, and make superfood snacks!",
            "Tanques verdes iluminados de microalgas que absorvem CO2, soltam oxigênio puro e produzem superalimento!",
            "https://images-assets.nasa.gov/image/iss059e114346/iss059e114346~medium.jpg",
            6.2f, -6000f, 3950f, 8.5f, 0f, 0.018f, 0.24f, 0f, 0.20f, 0.25f,
            new() { ["PBR"] = new("PBR", "Living Algae Garden", "Jardim Vivo de Algas", "Algae grow 10x faster than land plants while making oxygen and food.", "As microalgas crescem 10x mais rápido que plantas terrestres gerando oxigênio e comida.") }),

        new("ARC-SHD-01", CardSlotCategory.OutpostArchitecture, OutpostDestination.Moon,
            "Lunar Stone Dome", "Domo de Rocha 3D",
            "3D-Sintered Lunar Regolith Vault (+45 g/cm² Shielding)", "Abóbada de Regolito Lunar Impressa em 3D (+45 g/cm²)",
            "Shield +45 g/cm² | -6,200 W", "Blindagem +45 g/cm² | -6.200 W",
            "Robots melt Moon dirt with microwaves to print a thick stone igloo over your base!",
            "Robôs derretem a poeira da Lua com micro-ondas para imprimir um iglu de pedra superprotetor sobre a base!",
            "https://images-assets.nasa.gov/image/MSFC-201900567/MSFC-201900567~medium.jpg",
            2.0f, -6200f, -150f, 45.0f, 5f, 0f, 0.08f, 10.0f, 0.32f, 0.30f,
            new() { ["REGOLITH"] = new("REGOLITH", "Baked Moon Dirt Dome", "Domo de Solo Lunar", "1.5 meters of sintered lunar soil stops 94% of solar proton storms.", "1,5 metro de solo lunar fundido bloqueia 94% das tempestades de prótons solares.") }),

        new("ARC-SHD-02", CardSlotCategory.OutpostArchitecture, OutpostDestination.Mars,
            "Lava Cave Base", "Base na Caverna",
            "Arsia Mons Lava Tube Underground Base (+68 g/cm²)", "Base Subterrânea em Tubo de Lava de Arsia Mons (+68 g/cm²)",
            "Shield +68 g/cm² | Delay +45s", "Blindagem +68 g/cm² | Atraso +45s",
            "Builds the Mars base inside an ancient volcanic cave so thick rock blocks cosmic radiation!",
            "Constrói a base de Marte dentro de uma caverna vulcânica antiga onde a rocha grossa bloqueia a radiação!",
            "https://images-assets.nasa.gov/image/PIA13387/PIA13387~medium.jpg",
            2.5f, -4100f, 220f, 68.0f, 45f, 0f, 0.10f, 14.0f, 0.38f, 0.35f,
            new() { ["LAVATUBE"] = new("LAVATUBE", "Volcanic Cave Shelter", "Caverna Vulcânica", "Natural underground tunnels on Mars that shield against radiation and micrometeoroids.", "Túneis subterrâneos naturais em Marte que protegem contra radiação e meteoritos.") }),

        new("ARC-LSS-02", CardSlotCategory.OutpostArchitecture, OutpostDestination.Mars,
            "Mars Oxygen Dome", "Estufa de Marte",
            "Scaled 200x MOXIE Mars Atmospheric O2 Factory", "Usina de Oxigênio Atmosférico MOXIE 200x em Marte",
            "O2 +7,800 L/d | -14,500 W", "O2 +7.800 L/d | -14.500 W",
            "Breathes in Mars' thin carbon dioxide air and bakes it at 800°C to pour out pure oxygen!",
            "Aspira o ar fino de gás carbônico de Marte e o aquece a 800°C para liberar oxigênio puro!",
            "https://images-assets.nasa.gov/image/PIA23302/PIA23302~medium.jpg",
            4.8f, -14500f, 7800f, 12.5f, 0f, 0.015f, 0.20f, 0f, 0.24f, 0.22f,
            new() { ["MOXIE"] = new("MOXIE", "Mars Air-to-Oxygen Box", "Conversor de Ar de Marte", "Tested on NASA's Perseverance rover to make oxygen directly from Martian CO2.", "Testado no rover Perseverance da NASA para criar oxigênio direto do ar de Marte.") }),

        new("SWX-DONKI-01", CardSlotCategory.DonkiSpaceWeatherCounter, null,
            "Giant Solar Flare", "Explosão Solar",
            "Algal Water-Wall Storm Shelter & Edge-AI Autopilot", "Abrigo Hidráulico de Microalgas e Piloto Edge-AI DONKI",
            "Shield +24 g/cm² | Delay -380s", "Blindagem +24 g/cm² | Atraso -380s",
            "Pumps water and algae around the sleeping quarters when NASA DONKI spots a giant solar flare!",
            "Bombeia água e microalgas ao redor dos dormitórios quando o NASA DONKI detecta uma erupção solar gigante!",
            "https://images-assets.nasa.gov/image/GSFC_20171208_Archive_e000934/GSFC_20171208_Archive_e000934~medium.jpg",
            3.2f, -3200f, 480f, 24.0f, -380f, 0.012f, 0.12f, 12.0f, 0.35f, 0.28f,
            new() { ["DONKI"] = new("DONKI", "Space Weather Radar", "Radar de Clima Espacial", "NASA's early-warning system for solar flares and proton storms.", "Sistema de alerta antecipado da NASA para tempestades e erupções solares.") })
    ];

    /// <summary>
    /// 24 rich, educational turn-based exploration events (8 for ISS, 8 for Moon, 8 for Mars = 72 interactive choices)
    /// teaching ages 7–12 real space science while trading Watts, O2 Liters, Shielding Grams, Suit Wear, and Radiation.
    /// </summary>
    private static List<TurnExplorationEvent> BuildExplorationEventsCatalog() =>
    [
        // ==================== ISS EVENTS (8 Events) ====================
        new("EVT-ISS-01", OutpostDestination.ISS, 1,
            "South Atlantic Anomaly (SAA) Proton Pass", "Passagem pela Anomalia do Atlântico Sul (SAA)",
            "The space station is flying over South America where Earth's magnetic shield dips low! High-energy protons are buzzing outside.",
            "A estação espacial está sobrevoando o Brasil e o Atlântico Sul, onde o escudo magnético da Terra é mais baixo! Prótons velozes estão lá fora.",
            "Did you know? The South Atlantic Anomaly is a real dip in Earth's magnetic field where satellites and astronauts get extra radiation!",
            "Você sabia? A Anomalia Magnética do Atlântico Sul fica bem sobre o Brasil e exige blindagem extra nos satélites e na ISS!",
            "NASA TLE API (NORAD 25544) & DONKI RBE",
            [
                new("ISS-01-A", "Route +3,500 W to Active Magnetic Coils & Stay Inside", "Alocar +3.500 W nas Bobinas Magnéticas e Ficar Dentro",
                    "Smart move! Spending electrical Watts protected the crew and algae from proton hits.",
                    "Ótima escolha! Usar Watts elétricos protegeu a tripulação e as algas contra os prótons.",
                    "Magnetic Shield", -3500f, 120f, 8.0f, -0.10f, 0f, 0f, 0f, 0f, -0.02f, 15),
                new("ISS-01-B", "Perform Quick External Camera EVA Anyway", "Sair em Caminhada Espacial (EVA) Mesmo Assim",
                    "Whoa! You got great photos, but the spacesuits took radiation and micrometeoroid wear!",
                    "Uau! As fotos ficaram incríveis, mas os trajes sofreram desgaste e a tripulação recebeu radiação!",
                    "xEMU Suit", -1000f, -380f, 0f, 0.35f, -14.0f, 0f, 0f, -18f, 0.04f, 25),
                new("ISS-01-C", "Stack Polyethylene Water Bags Around Sleeping Pods", "Empilhar Bolsas de Água nos Dormitórios",
                    "Water is full of hydrogen atoms, which are super good at stopping space protons without using electricity!",
                    "A água é cheia de átomos de hidrogênio, que são excelentes para frear prótons sem gastar eletricidade!",
                    "Water Wall", 0f, 0f, 12.0f, -0.15f, 0f, 0f, -80f, 0f, -0.01f, 20)
            ]),

        new("EVT-ISS-02", OutpostDestination.ISS, 2,
            "Microgravity Capillary Bubble Clog in Chlorella Tank", "Bolha de Ar Presa no Tanque de Microalgas em Gravidade Zero",
            "In zero gravity, bubbles don't float up! A giant oxygen bubble is blocking light and nutrients inside the green algae bioreactor.",
            "Na gravidade zero, as bolhas não sobem! Uma bolha gigante de oxigênio está bloqueando a luz e os nutrientes no biorreator de algas.",
            "In space, warm air and gas bubbles stay stuck in liquid unless pumps or acoustic waves push them!",
            "No espaço, bolhas de gás não flutuam sozinhas na água; os cientistas usam ondas sonoras ou tubos capilares para separá-las!",
            "NASA OSDR OSD-521 & Capillary Flow Experiment",
            [
                new("ISS-02-A", "Pulse Acoustic Phase Separator (-2,200 W)", "Ativar Separador Acústico de Bolhas (-2.200 W)",
                    "Sound waves nudged the bubble into the O2 tank! Your crew gained +1,400 L of fresh oxygen!",
                    "Ondas sonoras empurraram a bolha para o tanque de O2! A tripulação ganhou +1.400 L de oxigênio puro!",
                    "Acoustic Separator", -2200f, 1400f, 0f, 0f, 0f, 60f, 0f, 0f, -0.04f, 20),
                new("ISS-02-B", "Spin the Temporary Centrifuge Module (-4,500 W)", "Girar a Centrífuga para Criar Gravidade Artificial (-4.500 W)",
                    "Artificial gravity pushed the liquid outward and freed the bubble while exercising the crew's bones!",
                    "A gravidade artificial separou o líquido do gás e ainda fortaleceu os ossos dos astronautas!",
                    "Centrifuge", -4500f, 1800f, 0f, -0.05f, 0f, 80f, 0f, 15f, -0.05f, 25),
                new("ISS-02-C", "Manually Vent the Line (Loses Water & O2)", "Purgar a Tubulação Manualmente (Perde Água e O2)",
                    "The clog cleared without spending Watts, but some precious water and oxygen escaped into the airlock.",
                    "O entupimento saiu sem gastar Watts, mas um pouco de água e oxigênio escapou pela válvula.",
                    "Manual Valve", 500f, -450f, 0f, 0f, -3.0f, 0f, -120f, 0f, 0.03f, 10)
            ]),

        new("EVT-ISS-03", OutpostDestination.ISS, 3,
            "Orbital Debris Cloud Tracked by NORAD 25544", "Nuvem de Detritos Orbitais Detectada pelo Radar NORAD",
            "A paint fleck travelling at 28,000 km/h is heading toward the solar wings! You have minutes to decide.",
            "Um fragmento viajando a 28.000 km/h está vindo na direção dos painéis solares! Você tem poucos minutos para decidir.",
            "At orbital speeds (7.6 km/s), even a tiny millimeter-sized pebble hits like a bowling ball at 100 km/h!",
            "Na velocidade orbital (28.000 km/h), até um grãozinho de tinta tem a força de impacto de uma bola de boliche!",
            "NASA TLE API (NORAD 25544) & JSC Orbital Debris Program",
            [
                new("ISS-03-A", "Fire Methalox Reboost Thrusters (DAM Avoidance Burn)", "Acionar Propulsores para Manobra de Desvio (Reboost)",
                    "The station climbed 2 km higher and dodged the debris completely!",
                    "A estação subiu 2 km de órbita e desviou completamente dos detritos!",
                    "Reboost Thruster", -3000f, -200f, 4.0f, 0f, 0f, 0f, 0f, 0f, -0.02f, 25),
                new("ISS-03-B", "Feather iROSA Solar Wings & Rely on Whipple Bumpers", "Recolher Painéis Solares e Confiar no Escudo Whipple",
                    "The outer Whipple bumper shattered the micro-debris harmlessly, though solar power dipped briefly.",
                    "A camada externa do escudo Whipple pulverizou o fragmento sem furar o casco, reduzindo só um pouco a energia.",
                    "Whipple Shield", -5500f, 0f, 6.0f, 0f, -4.0f, 0f, 0f, 0f, 0.01f, 20),
                new("ISS-03-C", "Send Valkyrie R5 Robot Outside with Patch Kit", "Enviar Robô Humanoide Valkyrie R5 com Kit de Reparo",
                    "Valkyrie patched the solar blanket without risking a human astronaut's spacesuit!",
                    "O robô Valkyrie consertou o painel solar sem desgastar o traje de nenhum astronauta humano!",
                    "Valkyrie R5", -4200f, 300f, 2.0f, 0f, 5.0f, 0f, 0f, 0f, -0.03f, 30)
            ]),

        new("EVT-ISS-04", OutpostDestination.ISS, 4,
            "Atomic Oxygen (ATOX) Erosion on Radiator Seals", "Erosão por Oxigênio Atômico (ATOX) nas Juntas do Radiador",
            "Single oxygen atoms in the upper atmosphere are chewing away the outer thermal coating of your ammonia radiators!",
            "Átomos isolados de oxigênio no topo da atmosfera estão corroendo a pintura térmica dos radiadores da nave!",
            "High above Earth, ultraviolet sunlight splits O2 into single 'atomic oxygen' atoms that rust plastics and metals fast!",
            "A 400 km de altura, o Sol divide o oxigênio em átomos soltos super-reativos que desgastam polímeros rapidamente!",
            "NASA Glenn MISSE Atomic Oxygen Experiment",
            [
                new("ISS-04-A", "Apply SiO2 Ceramic Coating via EVA (-5% Suit, -2,800 W)", "Aplicar Revestimento Cerâmico SiO2 em EVA (-5% Traje)",
                    "Glass-like silica coating stops atomic oxygen cold! Thermal radiators are back to 100%.",
                    "O revestimento de sílica vítrea bloqueia o oxigênio atômico! Os radiadores voltaram a 100%.",
                    "SiO2 Coating", -2800f, -220f, 4.0f, 0.05f, -5.0f, 0f, 0f, 0f, -0.05f, 25),
                new("ISS-04-B", "Switch to Backup Internal Water Evaporator", "Usar Evaporador Interno de Água Reserva",
                    "Saved power and suits, but vented 180 Liters of water into space as steam to cool the cabin.",
                    "Economizou energia e trajes, mas gastou 180 litros de água evaporada para resfriar a cabine.",
                    "Water Cooling", 1000f, -150f, 0f, 0f, 0f, 0f, -180f, 0f, 0.02f, 15),
                new("ISS-04-C", "3D-Print Titanium Clamp Rings in CNC Fab (-4,800 W)", "Imprimir Anéis de Titânio na Impressora 3D CNC (-4.800 W)",
                    "Onboard manufacturing built brand-new corrosion-proof seals!",
                    "A manufatura espacial imprimiu juntas novas de titânio à prova de corrosão!",
                    "3D CNC Fab", -4800f, 100f, 3.0f, 0f, 4.0f, 0f, 0f, 0f, -0.04f, 25)
            ]),

        new("EVT-ISS-05", OutpostDestination.ISS, 5,
            "SANS Eye-Pressure Alert During Zero-G Medical Check", "Alerta de Pressão Ocular (SANS) no Exame Médico em Gravidade Zero",
            "Without gravity pulling blood toward their feet, two astronauts report blurry vision as fluid shifts toward their heads!",
            "Sem a gravidade puxando o sangue para os pés, dois astronautas relataram visão turva pelo acúmulo de líquidos na cabeça!",
            "On Earth, gravity pulls fluids down to our legs. In space, fluids float upward, slightly flattening the back of the eyeball!",
            "Na Terra, a gravidade puxa os líquidos para as pernas. No espaço, eles sobem para a cabeça e pressionam o fundo dos olhos!",
            "NASA-STD-3001 & OSDR Crew Health SANS Study",
            [
                new("ISS-05-A", "Activate Lower-Body Negative Pressure (LBNP) Suit (-2,400 W)", "Ativar Câmara de Pressão Negativa nas Pernas LBNP (-2.400 W)",
                    "Gentle suction around the legs drew fluid back down just like Earth's gravity! Crew health boosted!",
                    "A sucção suave nas pernas puxou os fluidos de volta como a gravidade da Terra! Saúde restaurada!",
                    "LBNP Chamber", -2400f, 150f, 2.0f, -0.08f, 0f, 0f, 0f, 20f, -0.03f, 25),
                new("ISS-05-B", "Double Daily ARED Resistive Workout & Vitamin B12", "Dobrar o Treino no ARED e Suplementos OSDR",
                    "Heavy squats and B-vitamins protected bones and eyes, though the crew breathed extra O2 and ate more food!",
                    "O treino intenso e vitaminas protegeram ossos e olhos, mas a tripulação consumiu mais oxigênio e comida!",
                    "ARED Workout", -1500f, -320f, 0f, -0.05f, 0f, -90f, -60f, 10f, -0.02f, 20),
                new("ISS-05-C", "Run Short-Radius Sleep Centrifuge at 0.42g (-5,500 W)", "Ligar Centrífuga de Sono a 0,42g (-5.500 W)",
                    "Sleeping in gentle artificial gravity completely solved fluid shift and bone loss!",
                    "Dormir sob gravidade artificial suave resolveu o deslocamento de fluidos e a perda óssea!",
                    "Sleep Centrifuge", -5500f, 200f, 4.0f, -0.12f, 0f, 0f, 0f, 35f, -0.04f, 30)
            ]),

        new("EVT-ISS-06", OutpostDestination.ISS, 6,
            "Veggie Aeroponic Red Romaine Lettuce Harvest", "Colheita de Alface Espacial na Câmara Aeropônica Veggie",
            "Your Veggie plant pillow chamber is ready for harvest! How should the crew use the mature red romaine crop?",
            "A câmara botânica Veggie está pronta para a colheita! Como a tripulação deve aproveitar as plantas maduras?",
            "Plants in space use red and blue LED lights and grow roots in misted pillows because water would float away in soil!",
            "Plantas no espaço crescem sob LEDs vermelhos e azuis em travesseiros úmidos, pois a água flutuaria se fosse regada normal!",
            "NASA Kennedy Veggie & Advanced Plant Habitat (APH)",
            [
                new("ISS-06-A", "Feast on Fresh Salad & Save Seeds for Next Crop", "Servir Salada Fresca à Tripulação e Guardar Sementes",
                    "Fresh vitamins boosted crew morale and added +240 kg of effective food autonomy!",
                    "Vitaminas frescas elevaram a saúde da tripulação e somaram +240 kg de autonomia alimentar!",
                    "Veggie Harvest", -1800f, 650f, 0f, -0.06f, 0f, 240f, -40f, 25f, -0.02f, 25),
                new("ISS-06-B", "Expand LED Photoperiod to Maximize O2 Generation (-4,200 W)", "Aumentar Iluminação LED para Maximizar Oxigênio (-4.200 W)",
                    "Photosynthesis went into overdrive, pumping +1,950 Liters of O2/day into the cabin!",
                    "A fotossíntese acelerou ao máximo, injetando +1.950 litros de O2/dia na cabine!",
                    "APH LEDs", -4200f, 1950f, 2.0f, 0f, 0f, 110f, -70f, 0f, -0.03f, 25),
                new("ISS-06-C", "Feed Plant Trim Biomass into AnMBR Methanizer", "Enviar Restos Vegetais ao Biorreator de Metano AnMBR",
                    "Anaerobic bacteria digested the stems into extra methane rocket fuel for the return burn!",
                    "Bactérias digeriram os talos vegetais gerando metano extra para os motores de retorno!",
                    "AnMBR Digester", -2000f, 400f, 0f, 0f, 0f, 50f, 60f, 0f, -0.04f, 20)
            ]),

        new("EVT-ISS-07", OutpostDestination.ISS, 7,
            "Brine Processor (BPA) Salt Crystal Buildup", "Cristais de Sal Acumulados no Processador de Salmoura (BPA)",
            "After weeks of recycling urine into pure water, concentrated salt crystals are slowing down the BPA ionomer membrane!",
            "Após semanas reciclando água em ciclo fechado, cristais de sal estão reduzindo o fluxo na membrana do BPA!",
            "Closing the water loop from 93% to 98.5% means squeezing the last drops of water out of super-salty brine!",
            "Subir a reciclagem de água de 93% para 98,5% exige extrair até as últimas gotas de água da salmoura concentrada!",
            "NASA ISS Brine Processor Assembly & Marshall WRS",
            [
                new("ISS-07-A", "Swap BPA Bladder & Run Silver-Ion Flush (-2,600 W)", "Trocar Bolsa do BPA e Lavar com Íons de Prata (-2.600 W)",
                    "Membrane entropy dropped sharply and water recovery jumped back above 98.5%!",
                    "A entropia das membranas caiu e a reciclagem de água voltou para mais de 98,5%!",
                    "BPA + TCCS", -2600f, 350f, 0f, 0f, 0f, 0f, 220f, 0f, -0.08f, 25),
                new("ISS-07-B", "Bypass BPA and Run WPA Only (Saves Power, Loses Water)", "Desligar BPA e Operar Apenas o WPA (Economiza Watts)",
                    "Saved +2,500 Watts, but water loop efficiency dropped and 160 L of water remained trapped in brine.",
                    "Economizou +2.500 Watts, mas a reciclagem caiu e 160 L de água ficaram retidos na salmoura.",
                    "WPA Only", 2500f, -200f, 0f, 0f, 0f, 0f, -160f, 0f, 0.05f, 10),
                new("ISS-07-C", "Harvest Brine Minerals as Chlorella Trace Fertilizer", "Usar Sais Minerais Filtrados como Nutriente para as Algas",
                    "Clever recycling! Filtered nitrogen and phosphorus boosted microalgae O2 output by +1,100 L/d!",
                    "Reciclagem genial! O nitrogênio e o fósforo filtrados aumentaram o O2 das algas em +1.100 L/dia!",
                    "Bio-Loop", -3100f, 1100f, 0f, 0f, 0f, 90f, 120f, 0f, -0.05f, 30)
            ]),

        new("EVT-ISS-08", OutpostDestination.ISS, 8,
            "Final Sabatier Methalox Reserve Check Before Deorbit", "Verificação Final do Combustível Bio-Metano Antes do Retorno",
            "Mission Control in Houston asks for a final check of your Sabatier CH4 tanks and heatshield before atmospheric reentry.",
            "O Controle da Missão pede a checagem final dos tanques de metano do Sabatier e do escudo térmico antes da reentrada.",
            "Returning from orbit requires slowing down from 28,000 km/h using retro-rockets and a glowing heatshield!",
            "Para voltar do espaço é preciso frear de 28.000 km/h usando retrofoguetes e um escudo térmico resistente a 1.600°C!",
            "NASA NTRS Integrated ECLSS & Propulsion",
            [
                new("ISS-08-A", "Run Sabatier + Bosch Overdrive for Maximum CH4 Margin (-5,200 W)", "Operar Sabatier + Bosch no Máximo para Reserva Extra (-5.200 W)",
                    "Propellant tanks topped off well above the Safe Rate-Limit floor!",
                    "Tanques de propelente cheios muito acima do limite seguro de retorno!",
                    "Sabatier + Bosch", -5200f, 900f, 2.0f, 0f, 0f, 0f, 80f, 0f, -0.04f, 30),
                new("ISS-08-B", "Inspect Thermal Protection Tiles with Canadarm3 (-2,000 W)", "Inspecionar Escudo Térmico com Braço Robótico Canadarm3 (-2.000 W)",
                    "Laser scan confirmed zero cracks on the heatshield! System reliability maxed out.",
                    "O escaneamento a laser confirmou zero trincas no escudo térmico! Confiabilidade máxima.",
                    "Canadarm3", -2000f, 200f, 4.0f, 0f, 6.0f, 0f, 0f, 0f, -0.05f, 25),
                new("ISS-08-C", "Execute Immediate Fast-Track Deorbit Burn", "Executar Queima Imediata de Reentrada Rápida",
                    "Fast return saved consumables and brought the crew home ahead of schedule!",
                    "O retorno rápido economizou mantimentos e trouxe a tripulação para casa antes do prazo!",
                    "Fast Return", -1000f, 0f, 0f, 0f, 0f, 120f, 100f, 0f, 0f, 20)
            ]),

        // ==================== MOON OUTPOST EVENTS (8 Events) ====================
        new("EVT-MOON-01", OutpostDestination.Moon, 1,
            "Shackleton Crater Permanent Shadow Ice Drill", "Perfuração de Gelo na Sombra Eterna da Cratera Shackleton",
            "At the Lunar South Pole, sunlight never reaches the bottom of Shackleton Crater (-230°C), hiding ancient water ice!",
            "No Polo Sul da Lua, a luz do Sol nunca toca o fundo da Cratera Shackleton (-230°C), onde há milhões de toneladas de gelo!",
            "Lunar polar craters have been dark for 2 billion years, trapping water ice from ancient comets!",
            "As crateras polares da Lua estão no escuro há 2 bilhões de anos, preservando gelo trazido por cometas antigos!",
            "NASA Artemis III South Pole ISRU & Lunar Reconnaissance Orbiter",
            [
                new("MOON-01-A", "Deploy Heated Core Drill & Electrolyzer (-7,500 W)", "Acionar Perfuratriz Térmica e Eletrolisador (-7.500 W)",
                    "Jackpot! Melted lunar ice added +450 L of water and +2,800 L of breathable O2/day!",
                    "Sucesso total! O gelo lunar derretido rendeu +450 L de água e +2.800 L de oxigênio por dia!",
                    "ISRU Ice Drill", -7500f, 2800f, 6.0f, 0f, -6.0f, 0f, 450f, 0f, -0.04f, 35),
                new("MOON-01-B", "Send Astronauts on Manual Ice-Core EVA (-12% Suit)", "Enviar Astronautas em EVA Manual na Cratera (-12% Traje)",
                    "Collected pure ice crystals using only 1,800 W, but cryogenic -230°C dust stiffened the xEMU suit joints!",
                    "Coletou cristais puros gastando apenas 1.800 W, mas o frio de -230°C desgastou as juntas dos trajes xEMU!",
                    "xEMU EVA", -1800f, 1400f, 2.0f, 0.08f, -12.0f, 0f, 280f, 0f, 0.02f, 25),
                new("MOON-01-C", "Use Mirrors on Crater Rim to Sublimate Ice Passively", "Usar Espelhos na Borda da Cratera para Aquecer o Gelo com Luz Solar",
                    "Reflected sunlight warmed the ice tent with low power draw, yielding steady water and O2!",
                    "A luz solar refletida aqueceu a tenda coletora gastando pouca energia e gerando água e O2!",
                    "Solar Concentrator", -1200f, 1600f, 4.0f, 0f, -3.0f, 0f, 240f, 0f, -0.02f, 30)
            ]),

        new("EVT-MOON-02", OutpostDestination.Moon, 2,
            "Electrostatic Lunar Regolith Dust Storm at Airlock", "Nuvem Eletrostática de Poeira Lunar na Eclusa da Base",
            "After a moonwalk, razor-sharp glass-like Moon dust is clinging to the xEMU suits by static electricity!",
            "Após uma caminhada lunar, a poeira da Lua — cortante como vidro moído — grudou nos trajes por eletricidade estática!",
            "Because the Moon has no wind or water to smooth rocks out, lunar dust is jagged like microscopic broken glass!",
            "Como na Lua não há vento nem água para arredondar os grãos, a poeira lunar tem pontas afiadas como micro-cacos de vidro!",
            "NASA TechPort #182369 (Lunar Weather Array) & Kennedy EDS",
            [
                new("MOON-02-A", "Pulse Electrodynamic Dust Shield (EDS) & Dock via SuitPort (-2,400 W)", "Ativar Escudo Eletrodinâmico (EDS) e Entrar via SuitPort (-2.400 W)",
                    "98% of the dust flew right off the suits and the astronauts crawled in through the back hatch clean!",
                    "98% da poeira desgrudou na hora e os astronautas entraram pela escotilha traseira sem sujar a base!",
                    "EDS + SuitPort", -2400f, 200f, 2.0f, 0f, 8.0f, 0f, 0f, 0f, -0.06f, 30),
                new("MOON-02-B", "Brush Suits Manually Inside Airlock (High Suit & Filter Wear)", "Escovar os Trajes Manualmente na Eclusa (Desgasta Trajes e Filtros)",
                    "Saved electricity, but microscopic silicate dust scratched suit bearings and clogged ECLSS air filters!",
                    "Economizou energia, mas a poeira fina arranhou os rolamentos dos trajes e sujou os filtros do ECLSS!",
                    "Manual Brush", 0f, -450f, 0f, 0f, -15.0f, 0f, 0f, -15f, 0.08f, 10),
                new("MOON-02-C", "Flush Airlock with Compressed N2/O2 Curtain (-1,500 W, -350 L O2)", "Soprar Jato de Gás N2/O2 na Eclusa (-1.500 W, -350 L O2)",
                    "Blew the dust outside before opening the inner door, trading 350 Liters of gas to protect the suits.",
                    "Soprou a poeira para fora antes de abrir a porta interna, trocando 350 L de gás pela conservação dos trajes.",
                    "Gas Curtain", -1500f, -350f, 0f, 0f, 3.0f, 0f, 0f, 0f, -0.02f, 20)
            ]),

        new("EVT-MOON-03", OutpostDestination.Moon, 3,
            "354-Hour Cryogenic Lunar Night Begins (-180°C)", "Início da Longa Noite Lunar de 14 Dias Terrestres (-180°C)",
            "The Sun just set below the lunar horizon! For the next 14 Earth days, temperatures will plunge to -180°C.",
            "O Sol acabou de se pôr no horizonte da Lua! Pelos próximos 14 dias terrestres, a temperatura cairá para -180°C.",
            "One day on the Moon lasts ~29.5 Earth days: 14 days of blazing sunlight followed by 14 days of freezing darkness!",
            "Um dia completo na Lua dura quase um mês terrestre: são 14 dias de Sol forte seguidos por 14 dias de noite congelante!",
            "NASA Glenn Fission Surface Power & Regolith Thermal Mass",
            [
                new("MOON-03-A", "Throttle Up Kilopower KRUSTY Fission Microgrid (+18,000 W)", "Acelerar Microrrede de Fissão Nuclear Kilopower (+18.000 W)",
                    "Nuclear fission doesn't need sunlight! The base stayed warm and Chlorella LEDs shone bright all night!",
                    "A fissão nuclear não depende do Sol! A base ficou aquecida e os LEDs das algas brilharam a noite toda!",
                    "Kilopower Fission", 18000f, 950f, 4.0f, 0f, 0f, 0f, 0f, 0f, -0.04f, 35),
                new("MOON-03-B", "Release Heat from Sintered Regolith Thermal Bricks (-1,200 W)", "Liberar Calor Acumulado nos Blocos Térmicos de Regolito (-1.200 W)",
                    "Solar heat stored in melted Moon rock kept the cabin cozy while saving battery power!",
                    "O calor do Sol guardado em blocos de rocha lunar manteve a cabine aquecida economizando as baterias!",
                    "Thermal Bricks", -1200f, 300f, 6.0f, 0f, 0f, 0f, 0f, 0f, -0.02f, 25),
                new("MOON-03-C", "Hibernate Non-Essential ECLSS Loops (Saves Power, Raises Entropy)", "Hibernar Metade do ECLSS para Economizar Bateria",
                    "Survived the night on batteries alone, but cold pipes and paused scrubbers raised CO2 and entropy.",
                    "Atravessou a noite usando só baterias, mas o desligamento parcial elevou o CO2 e a entropia.",
                    "Battery Saver", 4000f, -900f, 0f, 0f, -4.0f, -60f, 0f, 0f, 0.07f, 15)
            ]),

        new("EVT-MOON-04", OutpostDestination.Moon, 4,
            "NASA DONKI Alert: X-Class Solar Flare & Proton Storm (SEP)", "Alerta NASA DONKI: Erupção Solar Classe X e Tempestade de Prótons",
            "The Moon has no atmosphere or global magnetic field! NASA's DONKI satellite just detected a fast proton storm heading your way.",
            "A Lua não tem atmosfera nem campo magnético global! O sistema DONKI da NASA acaba de detectar uma tempestade solar vindo aí.",
            "Outside Earth's magnetic bubble, astronauts need at least 40 g/cm² of water, plastic, or rock shielding during solar storms!",
            "Fora do escudo magnético da Terra, os astronautas precisam de pelo menos 40 g/cm² de rocha, água ou BNNT durante tempestades solares!",
            "NASA DONKI SEP / FLR API (https://api.nasa.gov/DONKI/SEP)",
            [
                new("MOON-04-A", "Retreat Inside 45 g/cm² Sintered Regolith Vault & Water Wall (-3,800 W)", "Recolher Tripulação à Abóbada de Regolito (45 g/cm²) e Parede d'Água",
                    "The thick lunar stone arch and Chlorella water jackets absorbed 96% of the solar protons!",
                    "A abóbada de rocha lunar e as paredes de água das algas absorveram 96% dos prótons solares!",
                    "Regolith Vault + Water Wall", -3800f, 400f, 22.0f, -0.25f, 0f, 0f, 0f, 15f, -0.03f, 35),
                new("MOON-04-B", "Administer OSDR Radioprotectant Meds & Stay in Standard Module", "Usar Medicamentos Radioprotetores OSDR no Módulo Padrão",
                    "Antioxidant radioprotectants helped repair DNA, consuming medical supplies while shielding was moderate.",
                    "Os radioprotetores ajudaram a proteger as células, consumindo estoque médico sob blindagem moderada.",
                    "OSDR Med-Bay", -1500f, 0f, 6.0f, 0.15f, 0f, 0f, 0f, -35f, 0.02f, 20),
                new("MOON-04-C", "Keep Surface Mining Rigs Running During Storm (High Risk!)", "Manter Mineração Externa Funcionando Durante a Tempestade (Alto Risco!)",
                    "Mined extra oxygen, but solar protons damaged solar cells (-9,000 W) and spiked radiation exposure!",
                    "Extraiu oxigênio extra, mas os prótons solares danificaram painéis (-9.000 W) e elevaram a radiação!",
                    "Unshielded Ops", -9000f, 1200f, -8.0f, 0.55f, -10.0f, 0f, 0f, -45f, 0.06f, 10)
            ]),

        new("EVT-MOON-05", OutpostDestination.Moon, 5,
            "Carbothermal Ilmenite (FeTiO3) Oxygen Kiln Run", "Fornalha Carbotérmica de Ilmenita Lunar para Produzir Oxigênio",
            "Lunar soil is 40% oxygen locked inside iron-titanium minerals (ilmenite)! Should we bake the soil at 1,600°C to free the O2?",
            "O solo da Lua tem 40% de oxigênio preso dentro de minerais como a ilmenita! Vamos aquecer o solo a 1.600°C para extrair O2?",
            "Even where there is no ice, Moon rocks are full of oxygen atoms bonded to iron, titanium, and silicon!",
            "Mesmo onde não há gelo, as rochas lunares são riquíssimas em átomos de oxigênio ligados a ferro, titânio e silício!",
            "NASA Kennedy Carbothermal Regolith Reduction (CaRD)",
            [
                new("MOON-05-A", "Run Carbothermal Methane Kiln (-8,200 W -> +4,600 L O2/d)", "Operar Fornalha Carbotérmica (-8.200 W -> +4.600 L O2/d)",
                    "Baked ilmenite released +4,600 L O2/day and left behind iron-titanium slag for +14 g/cm² radiation bricks!",
                    "A ilmenita aquecida liberou +4.600 L O2/dia e gerou blocos metálicos para +14 g/cm² de blindagem!",
                    "CaRD Reactor", -8200f, 4600f, 14.0f, 0f, 0f, 0f, 0f, 0f, -0.03f, 35),
                new("MOON-05-B", "Sinter Slag Bricks into Rover Landing Pad (-4,000 W)", "Imprimir Pista de Pouso Anti-Poeira com a Escória (-4.000 W)",
                    "Paving the landing pad stopped rocket plumes from kicking up abrasive dust onto the base!",
                    "Pavimentar a área de pouso impediu que os foguetes levantassem poeira abrasiva sobre os trajes e radiadores!",
                    "3D Sintering", -4000f, 500f, 10.0f, 0f, 9.0f, 0f, 0f, 0f, -0.05f, 30),
                new("MOON-05-C", "Conserve Kiln Power for Chlorella Bioreactor LEDs", "Poupar Energia da Fornalha e Priorizar o Fotobiorreator de Algas",
                    "Balanced approach: kept electrical consumption low while algae produced steady O2 and food.",
                    "Escolha equilibrada: manteve o consumo elétrico baixo enquanto as algas geraram O2 e comida.",
                    "Chlorella PBR", -1500f, 1500f, 2.0f, 0f, 0f, 120f, 0f, 0f, -0.02f, 20)
            ]),

        new("EVT-MOON-06", OutpostDestination.Moon, 6,
            "Lunar Far-Side Radio Shadow & LunaNet L2 Relay", "Sombra de Rádio na Cratera Polar e Satélite Relay LunaNet L2",
            "A mountain ridge at the Lunar South Pole is blocking direct radio line-of-sight to Earth!",
            "Uma cadeia de montanhas no Polo Sul da Lua está bloqueando a visão direta das antenas para a Terra!",
            "At the Moon's South Pole, Earth sits very low on the horizon, so crater walls can block direct radio signals!",
            "No Polo Sul lunar, a Terra fica bem baixinha no horizonte, então as paredes das crateras podem bloquear o sinal direto!",
            "NASA LunaNet Interoperability & LCRD Optical Relay",
            [
                new("MOON-06-A", "Lock Ka-Band Phased Array onto L2 Halo Relay (-3,200 W)", "Conectar Antena Banda Ka ao Satélite Relay Halo L2 (-3.200 W)",
                    "Signal bounced over the crater wall via the L2 relay satellite, cutting comms delay to 1.28 seconds!",
                    "O sinal contornou a cratera pelo satélite em órbita Halo L2, mantendo o atraso em apenas 1,28 segundos!",
                    "LunaNet Relay", -3200f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, -0.04f, 30),
                new("MOON-06-B", "Climb Malapert Peak in EVA to Plant Optical Repeater (-8% Suit)", "Subir o Pico Malapert em EVA para Instalar Repetidor Laser (-8% Traje)",
                    "The mountain peak has near-eternal sunlight and a clear view of Earth! High science reward!",
                    "O topo da montanha tem luz solar quase eterna e visão limpa da Terra! Grande conquista científica!",
                    "Optical Repeater", -1000f, -250f, 0f, 0.05f, -8.0f, 0f, 0f, 0f, -0.03f, 35),
                new("MOON-06-C", "Operate on Local Autonomous Mode Until Orbit Pass", "Operar em Modo Autônomo Local até a Passagem Orbital",
                    "Saved power and suit wear, though temporary communication latency increased.",
                    "Economizou energia e trajes, embora o atraso de comunicação tenha subido temporariamente.",
                    "Edge AI", 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.02f, 15)
            ]),

        new("EVT-MOON-07", OutpostDestination.Moon, 7,
            "Lunar Lava Tube Skylight Discovery at Mare Ingenii", "Descoberta de Claraboia de Tubo de Lava Lunar",
            "Your drone camera spotted a 60-meter pit leading into an ancient underground lunar lava tube!",
            "O drone da base encontrou uma abertura de 60 metros que leva a uma caverna vulcânica subterrânea na Lua!",
            "Underground lunar lava tubes stay a steady -20°C instead of swinging from +120°C to -180°C on the surface!",
            "Dentro das cavernas lunares a temperatura fica estável perto de -20°C, fugindo dos extremos de +120°C a -180°C da superfície!",
            "NASA GRAIL & LRO Lunar Pit Camera",
            [
                new("MOON-07-A", "Lower Valkyrie R5 Robot on Tether into the Pit (-3,500 W)", "Descer o Robô Valkyrie R5 por Cabo na Caverna (-3.500 W)",
                    "Valkyrie mapped a huge radiation-proof cavern (+25 g/cm² natural ceiling) without risking astronaut suits!",
                    "O robô Valkyrie mapeou um abrigo natural (+25 g/cm² de rocha no teto) sem arriscar os trajes dos astronautas!",
                    "Valkyrie R5", -3500f, 200f, 25.0f, -0.10f, 4.0f, 0f, 0f, 0f, -0.03f, 35),
                new("MOON-07-B", "Rappel Down with Astronaut Geologist Team (-11% Suit)", "Descer de Rapel com Equipe de Astronautas Geólogos (-11% Traje)",
                    "Collected pristine volcanic glass beads containing ancient solar wind helium and water!",
                    "Coletou esferas de vidro vulcânico intactas contendo água e hélio antigos! Descoberta máxima!",
                    "xEMU Rappel", -1500f, -300f, 18.0f, -0.05f, -11.0f, 0f, 150f, 0f, 0.01f, 40),
                new("MOON-07-C", "Scan from Rim with Ground-Penetrating Radar (-1,800 W)", "Escanear da Borda com Radar de Penetração no Solo (-1.800 W)",
                    "Safe 3D radar map completed with minimal resource expenditure.",
                    "Mapeamento 3D seguro concluído com gasto mínimo de recursos.",
                    "GPR Radar", -1800f, 0f, 5.0f, 0f, 0f, 0f, 0f, 0f, -0.01f, 20)
            ]),

        new("EVT-MOON-08", OutpostDestination.Moon, 8,
            "Gateway NRHO Rendezvous & Trans-Earth Injection Prep", "Acoplamento na Órbita NRHO Gateway e Preparo de Retorno à Terra",
            "It is time to verify your bio-methane propellant reserve and launch from the Lunar Outpost back toward Earth!",
            "Chegou a hora de conferir a reserva de combustível bio-metano e decolar do Outpost Lunar de volta para a Terra!",
            "From the Moon, it takes only about 3.5 days to coast back to Earth before atmospheric reentry!",
            "Saindo da Lua, a viagem de volta até a atmosfera da Terra leva cerca de 3 dias e meio!",
            "NASA JPL Horizons COMMAND='301' & Artemis NRHO",
            [
                new("MOON-08-A", "Cryo-Pump Sabatier Methalox + Polar LOX into Ascent Tanks (-6,000 W)", "Bombear Metano Sabatier + Oxigênio Líquido Polar nos Tanques (-6.000 W)",
                    "Ascent tanks filled above the 24% Safe Rate-Limit floor! Smooth lunar liftoff!",
                    "Tanques abastecidos acima do limite seguro de 24%! Decolagem lunar perfeita!",
                    "Cryo Depot", -6000f, 800f, 4.0f, 0f, 0f, 0f, 100f, 0f, -0.05f, 35),
                new("MOON-08-B", "Transfer Lunar Rock Samples & Recycle Excess Brine (-2,500 W)", "Embarcar Amostras Lunares e Reciclar Água Final no BPA (-2.500 W)",
                    "Lightened ship dry mass and maximized potable water for the 3.5-day return cruise!",
                    "Otimizou o peso da nave e garantiu água pura abundante para os 3,5 dias de volta!",
                    "BPA + WRS", -2500f, 400f, 2.0f, 0f, 2.0f, 80f, 200f, 0f, -0.04f, 30),
                new("MOON-08-C", "Launch Immediate Direct Return Trajectory", "Executar Trajetória Direta Imediata para a Terra",
                    "Engines fired on schedule for a fast trans-Earth injection!",
                    "Motores acionados no horário para uma injeção trans-Terra rápida e segura!",
                    "Direct Burn", -1200f, 0f, 0f, 0f, 0f, 50f, 50f, 0f, 0f, 25)
            ]),

        // ==================== MARS OUTPOST EVENTS (8 Events) ====================
        new("EVT-MARS-01", OutpostDestination.Mars, 1,
            "Toxic Martian Perchlorate (ClO4-) Dust in Airlock", "Poeira Tóxica de Perclorato Marciano (ClO4-) na Eclusa",
            "Martian soil contains 0.6% perchlorate salts that are toxic to human thyroids and Chlorella algae! Dust is entering the airlock.",
            "O solo de Marte contém sais de perclorato que são tóxicos para a tireoide humana e para as algas Chlorella! Há poeira na eclusa.",
            "NASA's Phoenix lander discovered perchlorates on Mars—but special bacteria can eat perchlorates and turn them into oxygen!",
            "A sonda Phoenix da NASA descobriu percloratos em Marte — mas bactérias especiais conseguem decompor esse sal em oxigênio puro!",
            "NASA InSight Weather & Perchlorate Bio-Scrubber Research",
            [
                new("MARS-01-A", "Activate Azospira Enzyme Washlock & EDS Shield (-3,400 W)", "Ativar Lavagem Enzimática Azospira e Escudo EDS (-3.400 W)",
                    "Enzyme bacteria neutralized 99.8% of the perchlorates and released +950 L of bonus oxygen!",
                    "As bactérias enzimáticas neutralizaram 99,8% dos percloratos e ainda liberaram +950 L de oxigênio bônus!",
                    "Bio-Scrubber", -3400f, 950f, 4.0f, 0f, 6.0f, 0f, 80f, 15f, -0.06f, 35),
                new("MARS-01-B", "Rinse Suits with Potable Water Reserve (-220 L Water)", "Lavar os Trajes com Água Potável da Reserva (-220 L Água)",
                    "Washed the toxic dust away without using extra Watts, but spent 220 Liters of clean water.",
                    "Removeu a poeira tóxica sem gastar Watts extras, mas consumiu 220 litros de água limpa.",
                    "Water Rinse", -500f, 0f, 0f, 0f, 2.0f, 0f, -220f, 0f, 0.01f, 20),
                new("MARS-01-C", "Dry-Brush Suits Only (Risks Biome & Crew Contamination)", "Apenas Escovar a Seco (Risco de Contaminar Algas e Cabine)",
                    "Saved water and power, but trace perchlorates slowed Chlorella O2 production and increased entropy!",
                    "Economizou água e energia, mas traços de perclorato reduziram o O2 das algas e subiram a entropia!",
                    "Dry Brush", 0f, -750f, 0f, 0.12f, -10.0f, -50f, 0f, -25f, 0.08f, 10)
            ]),

        new("EVT-MARS-02", OutpostDestination.Mars, 2,
            "Global Martian Dust Storm (Tau Opacity > 2.2)", "Tempestade Global de Poeira em Marte (Opacidade Tau > 2,2)",
            "A planet-wide dust storm is blotting out the Sun! Solar irradiance dropped from 43% to under 8% for weeks.",
            "Uma tempestade gigante de poeira cobriu o céu de Marte! A luz solar caiu de 43% para menos de 8% por várias semanas.",
            "Mars receives less than half as much sunlight as Earth, and global dust storms can dim the Martian sky for months!",
            "Marte já recebe menos da metade da luz solar da Terra, e tempestades globais de poeira podem escurecer o céu por meses!",
            "NASA InSight Weather Service API & Curiosity REMS",
            [
                new("MARS-02-A", "Switch to Kilopower / Nuclear Stirling Grid (+24,000 W)", "Operar Matriz Nuclear Kilopower / Stirling (+24.000 W)",
                    "Nuclear power completely ignored the dust storm! Outpost life support and MOXIE ran at 100%!",
                    "A energia nuclear ignorou a tempestade de poeira! O suporte à vida e o MOXIE rodaram a 100%!",
                    "Nuclear Stirling", 24000f, 1200f, 5.0f, 0f, 0f, 0f, 0f, 0f, -0.04f, 35),
                new("MARS-02-B", "Burn Methalox in Regenerative Fuel Cells (+14,000 W, -480 L O2/d)", "Usar Célula a Combustível Regenerativa (+14.000 W, -480 L O2/d)",
                    "Kept the base warm and powered by reacting stored oxygen and methane, producing +180 L of water!",
                    "Manteve a base aquecida reagindo oxigênio e metano, gerando +180 L de água pura como subproduto!",
                    "PEM Fuel Cell", 14000f, -480f, 2.0f, 0f, 0f, 0f, 180f, 0f, -0.02f, 25),
                new("MARS-02-C", "Send Crew into Dust Storm to Wipe Solar Panels (-14% Suit)", "Enviar Astronautas na Tempestade para Limpar Painéis Solares (-14% Traje)",
                    "Recovered some solar power, but windblown Martian dust heavily abraded the spacesuit visors and joints!",
                    "Recuperou parte da luz solar, mas o vento com poeira desgastou bastante os visores e juntas dos trajes!",
                    "Manual Wipe", 3500f, -350f, 0f, 0.05f, -14.0f, 0f, 0f, 0f, 0.04f, 15)
            ]),

        new("EVT-MARS-03", OutpostDestination.Mars, 3,
            "200x MOXIE Atmospheric CO2 Intake Freezing", "Congelamento na Entrada de CO2 Atmosférico do MOXIE 200x",
            "At -63°C and 745 Pascals of pressure, dry-ice frost is forming on the intake filter of your MOXIE oxygen plant!",
            "A -63°C e 745 Pascals de pressão, gelo seco (CO2 congelado) está se formando no filtro de entrada da usina de oxigênio MOXIE!",
            "Mars' air is 95.3% carbon dioxide! In extreme cold, CO2 freezes directly into dry-ice snow at -125°C!",
            "O ar de Marte tem 95,3% de gás carbônico! No frio intenso, o CO2 congela diretamente virando gelo seco!",
            "NASA TechPort Perseverance MOXIE ISRU",
            [
                new("MARS-03-A", "Route Kilopower Waste Heat Loop to MOXIE Intake (-4,500 W)", "Canalizar Calor Residual do Reator para o Filtro do MOXIE (-4.500 W)",
                    "Sublimated the dry ice and boosted MOXIE output by +3,600 Liters of breathable O2/day!",
                    "Derreteu o gelo seco e aumentou a produção do MOXIE em +3.600 litros de oxigênio por dia!",
                    "Thermal Loop + MOXIE", -4500f, 3600f, 2.0f, 0f, 0f, 0f, 0f, 0f, -0.05f, 35),
                new("MARS-03-B", "Compress Sublimated CO2 Directly into Sabatier + Chlorella PBR (-6,200 W)", "Enviar o CO2 Puro para o Reator Sabatier e para as Algas (-6.200 W)",
                    "Turned the frost into rocket fuel and algae food, yielding +2,400 L O2/d and +150 kg biomass!",
                    "Transformou o CO2 em combustível de foguete e alimento para as algas, gerando +2.400 L O2/dia e +150 kg de biomassa!",
                    "Sabatier + PBR", -6200f, 2400f, 4.0f, 0f, 0f, 150f, 90f, 0f, -0.04f, 35),
                new("MARS-03-C", "Chip Frost Off Manually on EVA (-9% Suit)", "Quebrar o Gelo Seco Manualmente em EVA (-9% Traje)",
                    "Cleared the intake without extra electrical draw, but cold CO2 frost wore down the EVA gloves.",
                    "Limpou a entrada sem gastar eletricidade extra, mas o frio intenso desgastou as luvas dos trajes.",
                    "Manual EVA", -800f, 1100f, 0f, 0.04f, -9.0f, 0f, 0f, 0f, 0.02f, 20)
            ]),

        new("EVT-MARS-04", OutpostDestination.Mars, 4,
            "22-Minute Mars Solar Conjunction Communication Blackout", "Blackout de Conjunção Solar: Marte Atrás do Sol (22 Min de Atraso)",
            "Mars and Earth are on opposite sides of the Sun! Radio signals are blocked by the Sun's hot corona for 14 days.",
            "Marte e a Terra estão em lados opostos do Sol! A coroa solar bloqueia os sinais de rádio com a Terra por 14 dias.",
            "Because light travels at 300,000 km/s, a radio message between Earth and Mars takes from 4 to 24 minutes each way!",
            "Mesmo viajando na velocidade da luz (300.000 km/s), uma mensagem entre a Terra e Marte leva de 4 a 24 minutos só de ida!",
            "NASA JPL Deep Space Network & DONKI IPS",
            [
                new("MARS-04-A", "Activate Onboard Quad-Ensemble ML.NET Edge Autopilot (-3,800 W)", "Ativar Piloto Automático Local Quad-Ensemble ML.NET (-3.800 W)",
                    "Your onboard AI balanced O2, water, and Sabatier fuel locally with zero need for Earth Mission Control!",
                    "A IA de bordo equilibrou oxigênio, água e combustível Sabatier sozinha sem depender do controle da Terra!",
                    "Edge-AI Autopilot", -3800f, 800f, 6.0f, -0.08f, 4.0f, 0f, 0f, 0f, -0.06f, 40),
                new("MARS-04-B", "Bounce DSOC Optical Laser via Relay Orbiter (-6,500 W)", "Retransmitir Laser Óptico DSOC por Sonda Triangular (-6.500 W)",
                    "Laser relay bypassed the solar corona and kept high-speed medical telemetry online!",
                    "O feixe a laser contornou a coroa solar por satélite relay e manteve a telemetria médica ativa!",
                    "DSOC Laser", -6500f, 200f, 2.0f, -0.05f, 0f, 0f, 0f, 25f, -0.04f, 35),
                new("MARS-04-C", "Wait Out the Blackout on Manual Crew Schedules", "Aguardar o Fim do Blackout com Controle Manual da Tripulação",
                    "Saved electrical power, but manual valve timing increased ECLSS filter entropy and crew fatigue.",
                    "Economizou eletricidade, mas o controle manual de válvulas aumentou a entropia do ECLSS e o cansaço.",
                    "Manual Watch", 0f, -400f, 0f, 0.05f, -4.0f, -70f, -50f, -15f, 0.06f, 15)
            ]),

        new("EVT-MARS-05", OutpostDestination.Mars, 5,
            "Subsurface Rodwell Glacier Ice Pocket at Arcadia Planitia", "Lençol de Gelo Glacial Subterrâneo em Arcadia Planitia",
            "Ground radar just located a sheet of pure water ice buried only 3 meters beneath the Martian plains!",
            "O radar da base localizou uma camada espessa de gelo de água pura a apenas 3 metros abaixo do solo marciano!",
            "NASA's SWIM project mapped huge buried glaciers on Mars that could fill Lake Superior with water!",
            "O projeto SWIM da NASA mapeou geleiras subterrâneas gigantes em Marte capazes de abastecer bases inteiras!",
            "NASA Mars Subsurface Water Ice Mapping (SWIM)",
            [
                new("MARS-05-A", "Melt a Subsurface Rodwell Water Well (-5,800 W)", "Derreter Poço Subterrâneo Rodwell e Bombear Água (-5.800 W)",
                    "Pumped +650 Liters of ultra-pure glacial water into the base and boosted OGA oxygen by +2,200 L/d!",
                    "Bombeou +650 litros de água glacial pura para a base e aumentou o oxigênio do OGA em +2.200 L/dia!",
                    "Rodwell Drill", -5800f, 2200f, 10.0f, 0f, -4.0f, 0f, 650f, 0f, -0.05f, 35),
                new("MARS-05-B", "Pump Glacial Water into Habitat Roof Shield Dome (-3,600 W)", "Bombear Água Glacial para a Cúpula de Blindagem do Teto (-3.600 W)",
                    "A 1-meter ice ceiling over the habitat added +28 g/cm² of cosmic ray shielding while letting sunlight in!",
                    "Um teto de gelo translúcido sobre o habitat adicionou +28 g/cm² de proteção contra radiação deixando a luz passar!",
                    "Ice Dome Shield", -3600f, 600f, 28.0f, -0.18f, 0f, 0f, 300f, 0f, -0.03f, 35),
                new("MARS-05-C", "Extract Core Samples for Astrobiology Lab (-1,500 W)", "Extrair Cilindros de Gelo para Pesquisa de Vida Microbiana (-1.500 W)",
                    "Preserved ancient Martian ice layers for science, earning huge discovery points!",
                    "Preservou camadas antigas de gelo marciano para análise astrobiológica, ganhando muitos pontos!",
                    "Science Drill", -1500f, 300f, 2.0f, 0f, -5.0f, 0f, 180f, 0f, -0.01f, 45)
            ]),

        new("EVT-MARS-06", OutpostDestination.Mars, 6,
            "Galactic Cosmic Ray (GCR) Heavy-Ion Flux Peak", "Pico de Raios Cósmicos Galácticos (GCR) de Íons Pesados",
            "High-energy iron nuclei from distant supernovas are sleeting through Martian thin air! How will you protect the crew?",
            "Partículas supervelozes vindas de supernovas distantes estão atravessando a fina atmosfera de Marte! Como proteger a equipe?",
            "Galactic Cosmic Rays come from outside our solar system and travel at nearly the speed of light!",
            "Os Raios Cósmicos Galácticos vêm de fora do Sistema Solar e viajam quase na velocidade da luz!",
            "NASA OSDR GeneLab Deep-Space Radiation & Curiosity RAD",
            [
                new("MARS-06-A", "Move Crew Quarters into Arsia Mons Lava Tube (+35 g/cm²)", "Transferir Alojamentos para o Tubo de Lava (+35 g/cm²)",
                    "Natural volcanic basalt rock blocked the heavy ions completely! Crew health stayed near 100%!",
                    "A rocha basáltica natural bloqueou os íons pesados! A saúde da tripulação permaneceu excelente!",
                    "Lava Tube", -3000f, 0f, 35.0f, -0.30f, 0f, 0f, 0f, 15f, -0.03f, 35),
                new("MARS-06-B", "Energize Active Toroidal Magneto-Shield (-8,500 W)", "Energizar Escudo Magnético Toroidal Ativo (-8.500 W)",
                    "A mini artificial magnetosphere deflected the cosmic rays around the habitat!",
                    "Uma mini magnetosfera artificial desviou os raios cósmicos ao redor do habitat!",
                    "Toroidal Shield", -8500f, 0f, 26.0f, -0.25f, 0f, 0f, 0f, 10f, -0.02f, 30),
                new("MARS-06-C", "Dispense OSDR Antioxidant & Spirulina Diet (-2,000 W)", "Fornecer Dieta Rica em Spirulina e Radioprotetores OSDR (-2.000 W)",
                    "Biological countermeasures repaired cellular oxidative stress while using minimal electrical power.",
                    "Contramedidas biológicas protegeram as células com baixo consumo de energia elétrica.",
                    "OSDR + Spirulina", -2000f, 600f, 6.0f, -0.10f, 0f, 100f, 0f, -20f, -0.01f, 25)
            ]),

        new("EVT-MARS-07", OutpostDestination.Mars, 7,
            "Jezero Ancient River Delta Fossil Micro-Stromatolite Search", "Exploração de Fósseis Microbianos no Antigo Delta do Rio Jezero",
            "Billions of years ago, Jezero Crater was a lake fed by a river! Your rover found layered carbonate rocks 4 km away.",
            "Há bilhões de anos, a Cratera Jezero era um lago alimentado por um rio! O rover encontrou rochas carbonáticas a 4 km da base.",
            "NASA's Perseverance rover explores Jezero Crater because ancient lake mud is the best place to preserve signs of past life!",
            "O rover Perseverance explora a Cratera Jezero porque antigos fundos de lagos são ideais para preservar sinais de vida antiga!",
            "NASA Mars 2020 Perseverance & JPL Horizons COMMAND='499'",
            [
                new("MARS-07-A", "Drive Pressurized Rover Powered by Methalox Fuel Cell (-4,200 W)", "Ir com Rover Pressurizado a Célula Methalox (-4.200 W)",
                    "Explored in shirt-sleeves inside the pressurized rover! Zero suit wear and historic science samples!",
                    "Explorou com segurança dentro do rover pressurizado! Zero desgaste de trajes e amostras históricas!",
                    "Pressurized Rover", -4200f, -250f, 8.0f, 0f, 0f, -40f, 60f, 0f, -0.02f, 45),
                new("MARS-07-B", "Long-Range xEMU Walking Sortie + UV Raman Laser (-13% Suit)", "Caminhada EVA de Longa Distância com Laser UV (-13% Traje)",
                    "Found organic carbon rings in the rock, though the 8 km walk on rusty rocks wore down the xEMU boots!",
                    "Encontrou anéis de carbono orgânico na rocha, embora a caminhada de 8 km tenha desgastado as botas xEMU!",
                    "xEMU Sortie", -1200f, -480f, 0f, 0.08f, -13.0f, -60f, -50f, 0f, 0.03f, 40),
                new("MARS-07-C", "Teleoperate Valkyrie R5 + Ingenuity-Class Hexacopter (-2,800 W)", "Teleoperar Robô Valkyrie R5 e Drone Hexacóptero (-2.800 W)",
                    "Aerial scout + humanoid robot brought the rock cores right to the airlock door!",
                    "O drone aéreo e o robô humanoide trouxeram as amostras até a porta da eclusa sem arriscar ninguém!",
                    "Valkyrie + Drone", -2800f, 150f, 2.0f, 0f, 5.0f, 0f, 0f, 0f, -0.03f, 35)
            ]),

        new("EVT-MARS-08", OutpostDestination.Mars, 8,
            "Mars Ascent & 180-Day Earth Return Methalox Ignition", "Ignição de Subida de Marte e Cruzeiro de 180 Dias de Volta à Terra",
            "Your Sabatier and MOXIE plants have brewed tons of liquid methane (CH4) and liquid oxygen (LOX) from Mars' air and ice!",
            "Suas usinas Sabatier e MOXIE fabricaram toneladas de metano líquido (CH4) e oxigênio líquido (LOX) usando o ar e o gelo de Marte!",
            "Making return rocket fuel on Mars (ISRU) saves launching 30+ tons of fuel all the way from Earth!",
            "Fabricar o combustível de volta em Marte (ISRU) economiza mais de 30 toneladas de peso no lançamento da Terra!",
            "NASA NTRS Mass Economy for Integrated ECLSS and Propulsion",
            [
                new("MARS-08-A", "Full Zero-Boil-Off Cryo-Methalox Load + Chlorella Overdrive (-7,000 W)", "Abastecimento Total Methalox Zero-Boil-Off + Algas no Máximo (-7.000 W)",
                    "Locked in >34% fuel reserve and +3,500 L O2/d for the 6-month cruise back to Earth!",
                    "Garantiu >34% de reserva de combustível e +3.500 L O2/dia para os 6 meses de viagem de volta à Terra!",
                    "Cryo Methalox + PBR", -7000f, 3500f, 10.0f, -0.10f, 0f, 220f, 250f, 20f, -0.06f, 45),
                new("MARS-08-B", "Balance Sleep Centrifuge (0.42g) & WPA/BPA Water Loop (-4,500 W)", "Equilibrar Centrífuga de Sono (0,42g) e Ciclo de Água WPA/BPA (-4.500 W)",
                    "Protected astronaut bones and muscles for the 180-day return flight and Earth landing!",
                    "Protegeu os ossos e músculos dos astronautas para os 180 dias de retorno e pouso na Terra!",
                    "Centrifuge + WRS", -4500f, 1200f, 6.0f, -0.12f, 4.0f, 100f, 320f, 30f, -0.05f, 40),
                new("MARS-08-C", "Execute Hohmann Minimum-Energy Return Burn (-1,800 W)", "Executar Queima de Retorno de Energia Mínima Hohmann (-1.800 W)",
                    "Precision navigation by JPL Horizons brought the PymGameOrbit spacecraft safely onto the Earth return corridor!",
                    "A navegação de precisão pelas efemérides JPL Horizons colocou a nave PymGameOrbit na rota segura para a Terra!",
                    "Hohmann Transfer", -1800f, 500f, 2.0f, 0f, 0f, 60f, 80f, 0f, -0.02f, 35)
            ])
    ];
}
