using System.Reflection;
using System.Text.Json;
using PymSpaceOrbit.Shared.Models.Game;

namespace PymSpaceOrbit.Shared.Services;

public static class PymSpaceOrbitCatalogLoader
{
    private static readonly Lazy<List<GameCardModel>> _cachedCards = new(LoadCatalogInternal);

    public static IReadOnlyList<GameCardModel> AllCards => _cachedCards.Value;

    public static readonly IReadOnlyDictionary<string, (string DisplayEn, string DisplayPtBr, string OriginalEn, string OriginalPtBr)> CardNameCatalog =
        new Dictionary<string, (string DisplayEn, string DisplayPtBr, string OriginalEn, string OriginalPtBr)>(StringComparer.OrdinalIgnoreCase)
        {
            // Deck 1: Ship Build Deck (BLD-01 .. BLD-10)
            ["BLD-01"] = (
                "Shielded Hull",
                "Casco Blindado",
                "Multilayer Carbon-Silicate & Hydrogenated Boron-Nitride (BNNT) Hull",
                "Casco Multicamadas de Carbono-Silicato e Nitreto de Boro Hidrogenado (BNNT)"
            ),
            ["BLD-02"] = (
                "Solar Power Wings",
                "Asas Solares",
                "Dual Kilopower Krusty Fission Cores + iROSA Solar Wings (220 kWe)",
                "Núcleos Duplos de Fissão Kilopower KRUSTY + Asas Solares iROSA (220 kWe)"
            ),
            ["BLD-03"] = (
                "Spacesuit Airlock",
                "Eclusa de Trajes",
                "xEMU Z-2 Orthofabric SuitPort Bay + Electrodynamic Dust Shield (EDS)",
                "Baia SuitPort de Orto-Tecido xEMU Z-2 + Escudo Eletrodinâmico de Poeira (EDS)"
            ),
            ["BLD-04"] = (
                "Space Med-Bay",
                "Hospital Espacial",
                "OSDR GeneLab Surgical Med-Bay + ARED + Cryo-Lyophilized Pantry",
                "Ala Médica Cirúrgica OSDR GeneLab + ARED + Despensa Crio-Liofilizada"
            ),
            ["BLD-05"] = (
                "Laser & 3D Lab",
                "Laser e Lab 3D",
                "DSOC Deep-Space Optical Laser + 3D CNC Titanium/Polymer Sintering Fab",
                "Laser Óptico de Espaço Profundo DSOC + Sinterização CNC 3D de Titânio/Polímero"
            ),
            ["BLD-06"] = (
                "Cryo Fuel Tank",
                "Tanque de Foguete",
                "Zero-Boil-Off Cryo-Methalox (CH4/LOX) Broad-Area Cooled Depot",
                "Depósito Criogênico Methalox (CH4/LOX) Zero-Boil-Off de Resfriamento Amplo"
            ),
            ["BLD-07"] = (
                "Origami Radiators",
                "Radiador Origami",
                "Two-Phase Pumped Ammonia Loop + Deployable Origami Radiators",
                "Circuito Bifásico Bombeado de Amônia + Radiadores Origami Desdobráveis"
            ),
            ["BLD-08"] = (
                "Space Robot Crew",
                "Robôs Espaciais",
                "Canadarm3 Dexterous Manipulator + Valkyrie R5 EVA Humanoid Twins",
                "Manipulador Destro Canadarm3 + Gêmeos Humanoides EVA Valkyrie R5"
            ),
            ["BLD-09"] = (
                "Gravity Sleep Pod",
                "Cama de Gravidade",
                "Short-Radius Counter-Rotating Sleep Centrifuge (0.38g to 0.50g)",
                "Centrífuga de Sono Contra-Rotativa de Curto Raio (0,38g a 0,50g)"
            ),
            ["BLD-10"] = (
                "Space Batteries",
                "Banco de Baterias",
                "Solid-State Li-Sulfur Battery Bank + Regenerative PEM Fuel Cells",
                "Banco de Baterias de Estado Sólido Lítio-Enxofre + Células a Combustível PEM Regenerativas"
            ),

            // Deck 2: ECLSS Properties Deck (ECL-01 .. ECL-10)
            ["ECL-01"] = (
                "Water Recycler",
                "Reciclador d'Água",
                "Marshall Water Recovery System (WPA Catalytic Reactor + UPA VCD)",
                "Sistema de Recuperação de Água Marshall (Reator Catalítico WPA + Destilação VCD UPA)"
            ),
            ["ECL-02"] = (
                "Saltwater Filter",
                "Filtro de Salmoura",
                "Brine Processor Assembly (BPA) Ionomer-Membrane Dewatering",
                "Conjunto Processador de Salmoura (BPA) por Desidratação em Membrana Ionomérica"
            ),
            ["ECL-03"] = (
                "Oxygen Generator",
                "Gerador de O2",
                "Oxygen Generation Assembly (OGA PEM) + 4-Bed Zeolite 5A CDRA",
                "Conjunto Gerador de Oxigênio (OGA PEM) + Removedor CDRA de 4 Leitos de Zeólita 5A"
            ),
            ["ECL-04"] = (
                "CO2 Fuel Maker",
                "Reator de Metano",
                "Sabatier Carbon Dioxide Reduction Assembly (Ru-Al2O3 Catalyst)",
                "Conjunto de Redução de Dióxido de Carbono Sabatier (Catalisador Ru-Al2O3)"
            ),
            ["ECL-05"] = (
                "Algae Oxygen Tank",
                "Tanque de Algas",
                "Hybrid Chlorella Vulgaris & Spirulina Photobioreactor (3,400 L)",
                "Fotobiorreator Híbrido de Chlorella Vulgaris e Spirulina (3.400 L)"
            ),
            ["ECL-06"] = (
                "UV Air Purifier",
                "Purificador UV",
                "Trace Contaminant Control (TCCS) + Silver-Ion Biocide & UV Sterilizer",
                "Controle de Contaminantes Traço (TCCS) + Biocida de Íons de Prata e Esterilizador UV"
            ),
            ["ECL-07"] = (
                "Hydrogen Recycler",
                "Reciclador de H2",
                "Methane Pyrolysis & Bosch Series Reactor (Closed Hydrogen Loop)",
                "Pirólise de Metano e Reator em Série Bosch (Ciclo Fechado de Hidrogênio)"
            ),
            ["ECL-08"] = (
                "Space Veggie Pod",
                "Horta Espacial",
                "Advanced Plant Habitat (APH) + Veggie Aeroponic Crop Module",
                "Habitat Avançado de Plantas (APH) + Módulo Agrícola Aeropônico Veggie"
            ),
            ["ECL-09"] = (
                "Cabin Air Tank",
                "Pressão da Cabine",
                "Cryogenic Nitrogen Recovery & Cabin Pressure Makeup Assembly",
                "Recuperação Criogênica de Nitrogênio e Reposição de Pressão da Cabine"
            ),
            ["ECL-10"] = (
                "Bio-Waste Tank",
                "Biogás Orgânico",
                "Anaerobic Membrane Bioreactor (AnMBR) Solid Waste Methanizer",
                "Metanizador de Resíduos Sólidos com Biorreator de Membrana Anaeróbica (AnMBR)"
            ),

            // Deck 3: Environment Countermeasure Cards (ISS, Moon, Mars)
            ["ENV-ISS-01"] = (
                "Meteoroid Shield",
                "Escudo de Meteoros",
                "Whipple Micrometeoroid & SAA Polyethylene Liner",
                "Escudo Whipple contra Micrometeoroides e Revestimento de Polietileno para a SAA"
            ),
            ["ENV-ISS-02"] = (
                "Bubble Separator",
                "Caça-Bolhas Zero-G",
                "Capillary Zero-G Acoustic Phase Separator",
                "Separador Acústico Capilar de Fases em Gravidade Zero"
            ),
            ["ENV-ISS-03"] = (
                "Zero-G Gym Suit",
                "Traje de Academia",
                "Lower-Body Negative Pressure (LBNP) + ARED Suite",
                "Pressão Negativa nos Membros Inferiores (LBNP) + Suíte ARED"
            ),
            ["ENV-ISS-04"] = (
                "Orbit Booster",
                "Propulsor Orbital",
                "SiO2 Atomic Oxygen (ATOX) Protective Coating & Reboost Thruster",
                "Revestimento Protetor SiO2 contra Oxigênio Atômico (ATOX) e Propulsor de Reboost"
            ),
            ["ENV-LUN-01"] = (
                "Moon Dust Shield",
                "Escudo Anti-Poeira",
                "Electrodynamic Regolith Dust Shield (EDS) + SuitPort",
                "Escudo Eletrodinâmico de Poeira de Regolito (EDS) + SuitPort"
            ),
            ["ENV-LUN-02"] = (
                "Crater Ice Melter",
                "Extrator de Gelo",
                "Shackleton Crater Polar Ice ISRU Electrolyzer",
                "Eletrolisador ISRU de Gelo Polar da Cratera Shackleton"
            ),
            ["ENV-LUN-03"] = (
                "Water-Wall Shelter",
                "Parede de Água",
                "Algal Water-Wall Solar Storm Shelter",
                "Abrigo de Tempestade Solar com Parede d'Água de Microalgas"
            ),
            ["ENV-LUN-04"] = (
                "Moon Rock Heater",
                "Aquecedor Lunar",
                "Regolith Thermal Mass Accumulator & Carbothermal O2 Rig",
                "Acumulador de Massa Térmica de Regolito e Extrator Carbotérmico de O2"
            ),
            ["ENV-MAR-01"] = (
                "Mars Dust Shower",
                "Ducha Anti-Tóxica",
                "Perchlorate Bio-Scrubber Enzyme Washlock (Azospira Orizae)",
                "Eclusa de Lavagem Enzimática Bio-Scrubber de Perclorato (Azospira Orizae)"
            ),
            ["ENV-MAR-02"] = (
                "Mars Air Collector",
                "Coletor de Ar",
                "MOXIE Solid-Oxide + Sabatier Atmospheric CO2 Compressor",
                "Compressor de CO2 Atmosférico de Óxido Sólido MOXIE + Sabatier"
            ),
            ["ENV-MAR-03"] = (
                "Magnetic Shield",
                "Escudo Magnético",
                "Active Toroidal GCR Magneto-Shield + Radioprotectants",
                "Magneto-Escudo Toroidal Ativo contra GCR + Radioprotetores"
            ),
            ["ENV-MAR-04"] = (
                "Mars Ice Drill",
                "Broca de Geleira",
                "Subsurface Rodwell Glacier Drill & Dust-Storm Laser Lidar",
                "Perfuratriz Glacial Subsuperficial Rodwell e Lidar a Laser para Tempestades de Poeira"
            ),

            // Deck 4: Outpost Architectural Decisions Deck (ARC-*)
            ["ARC-LSS-01"] = (
                "Moon Oxygen Plant",
                "Usina de O2 Lunar",
                "Shackleton Regolith Ice-Mining & Carbothermal Oxygen Plant",
                "Usina de Mineração de Gelo em Regolito e Oxigênio Carbotérmico em Shackleton"
            ),
            ["ARC-LSS-02"] = (
                "Mars Oxygen Dome",
                "Estufa de Marte",
                "Scaled 200x MOXIE Solid-Oxide + Chlorella Pressurized Greenhouse Dome",
                "MOXIE de Óxido Sólido Escalado 200x + Domo Estufa Pressurizado de Chlorella"
            ),
            ["ARC-SHD-01"] = (
                "Lunar Stone Dome",
                "Domo de Rocha 3D",
                "3D-Sintered Lunar Regolith Vault (45 g/cm2 Overburden) + Water Jacket",
                "Abóbada de Regolito Lunar Sinterizado em 3D (45 g/cm² de Cobertura) + Camisa d'Água"
            ),
            ["ARC-SHD-02"] = (
                "Lava Cave Base",
                "Base na Caverna",
                "Arsia Mons Lava Tube Subsurface Outpost + BNNT Inflatable Liner",
                "Posto Subsuperficial em Tubo de Lava de Arsia Mons + Revestimento Inflável BNNT"
            ),
            ["ARC-PWR-01"] = (
                "Lunar Power Grid",
                "Usina Solar Lunar",
                "Quad-Kilopower 40 kWe Fission Microgrid + Vertical Polar Solar Array",
                "Microrrede de Fissão Quad-Kilopower 40 kWe + Painel Solar Polar Vertical"
            ),
            ["ARC-PWR-02"] = (
                "Storm Power Core",
                "Gerador Blindado",
                "Dust-Tolerant Nuclear Stirling Matrix + Regenerative Methalox Fuel Cell",
                "Matriz Nuclear Stirling Tolerante à Poeira + Célula a Combustível Methalox Regenerativa"
            ),
            ["ARC-COM-01"] = (
                "Moon Relay Dish",
                "Satélites da Lua",
                "LunarNet L2 Halo Relay Constellation + Ka-Band Phased Array",
                "Constelação Relay Halo LunarNet L2 + Matriz de Fase Banda Ka"
            ),
            ["ARC-COM-02"] = (
                "Mars AI Autopilot",
                "Piloto IA Marte",
                "Areostationary DSOC Optical Relay + Onboard Quad-ML.NET Edge Autopilot",
                "Relay Óptico Areoestacionário DSOC + Piloto Automático de Borda Quad-ML.NET"
            ),

            // Deck 5: DONKI Space Weather Events Deck (SWX-DONKI-01 .. 04)
            ["SWX-DONKI-01"] = (
                "Giant Solar Flare",
                "Explosão Solar",
                "X-Class Solar Flare (X10.4 Peak Flux)",
                "Erupção Solar Classe X (Fluxo de Pico X10.4)"
            ),
            ["SWX-DONKI-02"] = (
                "Solar Storm",
                "Tempestade Solar",
                "Solar Energetic Particle (SEP) S4 Severe Proton Storm (>100 MeV)",
                "Tempestade Severa S4 de Partículas Energéticas Solares (SEP > 100 MeV)"
            ),
            ["SWX-DONKI-03"] = (
                "Solar Plasma Wave",
                "Onda de Plasma",
                "Fast Halo Coronal Mass Ejection (CME > 2,100 km/s)",
                "Ejeção de Massa Coronal (CME) Rápida em Halo (> 2.100 km/s)"
            ),
            ["SWX-DONKI-04"] = (
                "Radio Blackout",
                "Apagão de Rádio",
                "Solar Conjunction Blackout + CME Forbush Decrease",
                "Blackout de Conjunção Solar + Decréscimo de Forbush por CME"
            )
        };

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

        bool hasMappedNames = CardNameCatalog.TryGetValue(cardId, out var mappedNames);

        string originalNameEn = "";
        if (item.TryGetProperty("OriginalName", out var origEnProp)) originalNameEn = origEnProp.GetString() ?? "";
        else if (hasMappedNames) originalNameEn = mappedNames.OriginalEn;
        else if (item.TryGetProperty("Name", out var nProp)) originalNameEn = nProp.GetString() ?? "";
        else if (item.TryGetProperty("EventClass", out var ecProp)) originalNameEn = ecProp.GetString() ?? "";
        if (string.IsNullOrWhiteSpace(originalNameEn)) originalNameEn = cardId;

        string originalNamePt = "";
        if (item.TryGetProperty("OriginalNamePtBr", out var origPtProp)) originalNamePt = origPtProp.GetString() ?? "";
        else if (hasMappedNames) originalNamePt = mappedNames.OriginalPtBr;
        else if (item.TryGetProperty("NamePtBr", out var npProp)) originalNamePt = npProp.GetString() ?? "";
        else if (item.TryGetProperty("EventClassPtBr", out var ecpProp)) originalNamePt = ecpProp.GetString() ?? "";
        if (string.IsNullOrWhiteSpace(originalNamePt)) originalNamePt = originalNameEn;

        string displayNameEn = "";
        if (item.TryGetProperty("DisplayName", out var dispEnProp)) displayNameEn = dispEnProp.GetString() ?? "";
        else if (hasMappedNames) displayNameEn = mappedNames.DisplayEn;
        else if (item.TryGetProperty("Name", out var dnProp)) displayNameEn = dnProp.GetString() ?? "";
        if (string.IsNullOrWhiteSpace(displayNameEn)) displayNameEn = originalNameEn;

        string displayNamePt = "";
        if (item.TryGetProperty("DisplayNamePtBr", out var dispPtProp)) displayNamePt = dispPtProp.GetString() ?? "";
        else if (hasMappedNames) displayNamePt = mappedNames.DisplayPtBr;
        else if (item.TryGetProperty("NamePtBr", out var dnpProp)) displayNamePt = dnpProp.GetString() ?? "";
        if (string.IsNullOrWhiteSpace(displayNamePt)) displayNamePt = displayNameEn;

        string uiEn = item.TryGetProperty("GameUiLabel", out var uiProp) ? uiProp.GetString() ?? "" : "";
        string uiPt = item.TryGetProperty("GameUiLabelPtBr", out var uipProp) ? uipProp.GetString() ?? "" : uiEn;

        string kidEn = "";
        if (item.TryGetProperty("KidSummaryEn", out var ksEn)) kidEn = ksEn.GetString() ?? "";
        else if (item.TryGetProperty("TacticalCountermeasureRule", out var tcm)) kidEn = tcm.GetString() ?? "";
        else if (item.TryGetProperty("ArchitecturalRuleSummary", out var ars)) kidEn = ars.GetString() ?? "";
        else if (item.TryGetProperty("Effect", out var eff)) kidEn = eff.GetString() ?? "";
        else if (item.TryGetProperty("SpaceWeatherHazardDescription", out var swd)) kidEn = swd.GetString() ?? "";
        if (string.IsNullOrWhiteSpace(kidEn)) kidEn = originalNameEn;

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
            displayNameEn,
            displayNamePt,
            originalNameEn,
            originalNamePt,
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
