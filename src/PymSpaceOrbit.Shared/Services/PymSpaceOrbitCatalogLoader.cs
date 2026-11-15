using System.Reflection;
using System.Text.Json;
using PymSpaceOrbit.Shared.Models.Game;

namespace PymSpaceOrbit.Shared.Services;

public static class PymSpaceOrbitCatalogLoader
{
    private static readonly Lazy<List<GameCardModel>> _cachedCards = new(LoadCatalogInternal);

    public static IReadOnlyList<GameCardModel> AllCards => _cachedCards.Value;

    private static List<GameCardModel> LoadCatalogInternal()
    {
        // 1. Try reading directly from the Shared project assets directory
        string[] candidatePaths = [
            Path.Combine(AppContext.BaseDirectory, "_content", "PymSpaceOrbit.Shared", "assets", "PymSpaceOrbit_decks_catalog.json"),
            Path.Combine(AppContext.BaseDirectory, "wwwroot", "assets", "PymSpaceOrbit_decks_catalog.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "src", "PymSpaceOrbit.Shared", "wwwroot", "assets", "PymSpaceOrbit_decks_catalog.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "PymSpaceOrbit.Shared", "wwwroot", "assets", "PymSpaceOrbit_decks_catalog.json")
        ];

        foreach (var p in candidatePaths)
        {
            try
            {
                if (File.Exists(p))
                {
                    using var stream = File.OpenRead(p);
                    using var doc = JsonDocument.Parse(stream);
                    var cards = ParseDoc(doc);
                    if (cards.Count > 0) return cards;
                }
            }
            catch { }
        }

        // 2. Try the EmbeddedResource from PymSpaceOrbit.Shared assembly
        try
        {
            var asm = typeof(GameCardModel).Assembly;
            using var stream = asm.GetManifestResourceStream("PymSpaceOrbit_decks_catalog.json");
            if (stream is not null)
            {
                using var doc = JsonDocument.Parse(stream);
                var cards = ParseDoc(doc);
                if (cards.Count > 0) return cards;
            }
        }
        catch { }

        return new List<GameCardModel>();
    }

    public static List<GameCardModel> ParseDoc(JsonDocument doc)
    {
        var result = new List<GameCardModel>();
        var root = doc.RootElement;

        if (root.TryGetProperty("ShipBuildDeck", out var shipDeck))
        {
            foreach (var item in shipDeck.EnumerateArray())
                result.Add(ParseCardElement(item, CardSlotCategory.ShipAndBaseHull, null));
        }

        if (root.TryGetProperty("EclssPropertiesDeck", out var eclssDeck))
        {
            foreach (var item in eclssDeck.EnumerateArray())
                result.Add(ParseCardElement(item, CardSlotCategory.MarshallEclssLoop, null));
        }

        if (root.TryGetProperty("EnvironmentDecks", out var envDecks))
        {
            foreach (var envItem in envDecks.EnumerateArray())
            {
                OutpostDestination? dest = null;
                if (envItem.TryGetProperty("TargetId", out var targetIdProp))
                {
                    string target = targetIdProp.GetString() ?? "";
                    if (target.Equals("ISS", StringComparison.OrdinalIgnoreCase)) dest = OutpostDestination.ISS;
                    else if (target.Equals("Moon", StringComparison.OrdinalIgnoreCase)) dest = OutpostDestination.Moon;
                    else if (target.Equals("Mars", StringComparison.OrdinalIgnoreCase)) dest = OutpostDestination.Mars;
                }

                if (envItem.TryGetProperty("EnvironmentCountermeasureCards", out var envCards))
                {
                    foreach (var cardItem in envCards.EnumerateArray())
                        result.Add(ParseCardElement(cardItem, CardSlotCategory.EnvironmentAndSuits, dest));
                }
            }
        }

        if (root.TryGetProperty("OutpostArchitecturalDecisionsDeck", out var arcDeck))
        {
            foreach (var item in arcDeck.EnumerateArray())
            {
                OutpostDestination? dest = null;
                if (item.TryGetProperty("OutpostEnvironment", out var envProp))
                {
                    string env = envProp.GetString() ?? "";
                    if (env.Contains("Moon", StringComparison.OrdinalIgnoreCase)) dest = OutpostDestination.Moon;
                    else if (env.Contains("Mars", StringComparison.OrdinalIgnoreCase)) dest = OutpostDestination.Mars;
                }
                result.Add(ParseCardElement(item, CardSlotCategory.OutpostArchitecture, dest));
            }
        }

        if (root.TryGetProperty("DonkiSpaceWeatherEventsDeck", out var swxDeck))
        {
            foreach (var item in swxDeck.EnumerateArray())
                result.Add(ParseCardElement(item, CardSlotCategory.DonkiSpaceWeatherCounter, null));
        }

        return result;
    }

    private static GameCardModel ParseCardElement(JsonElement item, CardSlotCategory category, OutpostDestination? destination)
    {
        string cardId = item.TryGetProperty("CardId", out var idProp) ? idProp.GetString() ?? "" : "";

        string nameEn = "";
        if (item.TryGetProperty("Name", out var nProp)) nameEn = nProp.GetString() ?? "";
        else if (item.TryGetProperty("EventClass", out var ecProp)) nameEn = ecProp.GetString() ?? "";
        if (string.IsNullOrWhiteSpace(nameEn)) nameEn = cardId;

        string namePt = "";
        if (item.TryGetProperty("NamePtBr", out var npProp)) namePt = npProp.GetString() ?? "";
        else if (item.TryGetProperty("EventClassPtBr", out var ecpProp)) namePt = ecpProp.GetString() ?? "";
        if (string.IsNullOrWhiteSpace(namePt)) namePt = nameEn;

        string uiEn = item.TryGetProperty("GameUiLabel", out var uiProp) ? uiProp.GetString() ?? "" : "";
        string uiPt = item.TryGetProperty("GameUiLabelPtBr", out var uipProp) ? uipProp.GetString() ?? "" : uiEn;

        string kidEn = "";
        if (item.TryGetProperty("KidSummaryEn", out var ksEn)) kidEn = ksEn.GetString() ?? "";
        else if (item.TryGetProperty("TacticalCountermeasureRule", out var tcm)) kidEn = tcm.GetString() ?? "";
        else if (item.TryGetProperty("ArchitecturalRuleSummary", out var ars)) kidEn = ars.GetString() ?? "";
        else if (item.TryGetProperty("Effect", out var eff)) kidEn = eff.GetString() ?? "";
        else if (item.TryGetProperty("SpaceWeatherHazardDescription", out var swd)) kidEn = swd.GetString() ?? "";
        if (string.IsNullOrWhiteSpace(kidEn)) kidEn = nameEn;

        string kidPt = "";
        if (item.TryGetProperty("KidSummaryPtBr", out var ksPt)) kidPt = ksPt.GetString() ?? "";
        else if (item.TryGetProperty("TacticalCountermeasureRulePtBr", out var tcmp)) kidPt = tcmp.GetString() ?? "";
        else if (item.TryGetProperty("ArchitecturalRuleSummaryPtBr", out var arsp)) kidPt = arsp.GetString() ?? "";
        else if (item.TryGetProperty("EffectPtBr", out var effp)) kidPt = effp.GetString() ?? "";
        else if (item.TryGetProperty("SpaceWeatherHazardDescriptionPtBr", out var swdp)) kidPt = swdp.GetString() ?? "";
        if (string.IsNullOrWhiteSpace(kidPt)) kidPt = kidEn;

        string nasaAsset = item.TryGetProperty("NasaImageAssetUrl", out var naProp) ? naProp.GetString() ?? "" : "";
        string nasaApi = item.TryGetProperty("NasaImageApiUrl", out var napiProp) ? napiProp.GetString() ?? "" : "";

        float mass = item.TryGetProperty("DryMassTons", out var dm) ? dm.GetSingle() : 3.0f;
        float watt = item.TryGetProperty("WattOfPowerCost", out var wp) ? wp.GetSingle() : 0f;
        if (watt > 0 && item.TryGetProperty("ElectricalPowerKwDelta", out var epk) && epk.GetSingle() < 0) watt = -watt;
        else if (watt > 0 && item.TryGetProperty("ElectricalPowerKwDraw", out var epkd) && epkd.GetSingle() > 0) watt = -watt;

        float o2 = item.TryGetProperty("LiterOfOxygenPerDayNet", out var o2Prop) ? o2Prop.GetString() is not null ? 0f : o2Prop.GetSingle() : 0f;
        float shield = 0f;
        if (item.TryGetProperty("GramOfRadiationShieldingGcm2", out var shProp)) shield = shProp.GetSingle();
        else if (item.TryGetProperty("RadiationShieldingGcm2", out var rshProp)) shield = rshProp.GetSingle();

        float delay = item.TryGetProperty("CommunicationDelayMitigationSec", out var dProp) ? dProp.GetSingle() : 0f;

        float waterLoop = item.TryGetProperty("WaterRecoveryEfficiencyBonus", out var wrProp) ? wrProp.GetSingle() : 0f;
        float eclssSynergy = item.TryGetProperty("O2GenerationEfficiencyBonus", out var o2bProp) ? o2bProp.GetSingle() : 0.05f;
        float suitDurability = item.TryGetProperty("EvaSuitDurabilityBonusPct", out var sdProp) ? sdProp.GetSingle() : 0f;
        float crewBuff = item.TryGetProperty("CrewProtectionBuff", out var cpProp) ? cpProp.GetSingle() : 0.10f;
        float biomeBuff = item.TryGetProperty("BiomeShieldingBuff", out var bmProp) ? bmProp.GetSingle() : 0.10f;

        var glossary = new Dictionary<string, TechnicalTermNote>();
        if (item.TryGetProperty("TechnicalGlossary", out var tgProp) && tgProp.ValueKind == JsonValueKind.Object)
        {
            foreach (var termObj in tgProp.EnumerateObject())
            {
                string termKey = termObj.Name;
                var termVal = termObj.Value;
                string termEn = termVal.TryGetProperty("GameUiLabel", out var tuProp) ? tuProp.GetString() ?? termKey : termKey;
                string termPt = termVal.TryGetProperty("GameUiLabelPtBr", out var tupProp) ? tupProp.GetString() ?? termEn : termEn;
                string meanEn = termVal.TryGetProperty("MeaningEn", out var meProp) ? meProp.GetString() ?? "" : "";
                string meanPt = termVal.TryGetProperty("MeaningPtBr", out var mpProp) ? mpProp.GetString() ?? meanEn : meanEn;

                glossary[termKey] = new TechnicalTermNote(termKey, termEn, termPt, meanEn, meanPt);
            }
        }

        return new GameCardModel(
            cardId,
            category,
            destination,
            nameEn,
            namePt,
            uiEn,
            uiPt,
            kidEn,
            kidPt,
            nasaAsset,
            mass,
            watt,
            o2,
            shield,
            delay,
            waterLoop,
            eclssSynergy,
            suitDurability,
            crewBuff,
            biomeBuff,
            glossary,
            nasaApi
        );
    }
}
