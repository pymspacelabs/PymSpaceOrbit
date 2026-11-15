# ☄️ PymGameOrbit — Jogo de Cartas de Decisões, Sobrevivência e Exploração Espacial

[![.NET 11](https://img.shields.io/badge/.NET-11.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![MudBlazor](https://img.shields.io/badge/MudBlazor-v9.12.0-00E5FF?logo=blazor)](https://mudblazor.com/)
[![MAUI](https://img.shields.io/badge/.NET_MAUI-Hybrid-00E676?logo=dotnet)](https://learn.microsoft.com/dotnet/maui/)
[![NASA Space Apps](https://img.shields.io/badge/NASA_Space_Apps-2026-FFD700?logo=nasa)](https://www.spaceappschallenge.org/)

## 👨🏻‍🚀 Sobre o Projeto
O **PymGameOrbit** foi desenvolvido para o **NASA Space Apps Challenge 2026**, atendendo ao desafio de construir um simulador e jogo educativo de treinamento de missões espaciais voltado para crianças e jovens de **7 a 12 anos** (Junior Mission Trainer).

O jogador projeta, constrói e gerencia sua própria **Nave e Base Orbital PymGameOrbit** em três destinos emblemáticos:
- 🛰️ **Estação Espacial Internacional (ISS)** — Órbita Baixa da Terra (LEO, 414 km)
- 🌕 **Lua** — Lunar Gateway & Base Artemis no Polo Sul (Cratera Shackleton)
- 🔴 **Marte** — Órbita Areocêntrica & Operações de Superfície com ISRU

A simulação integra dados científicos reais da NASA:
- **Marshall ECLSS**: Malhas de suporte de vida em ciclo fechado (reciclagem de água WPA/BPA, eletrólise OGA, removedores de CO₂ CDRA, reatores Sabatier/Bosch e fotobiorreatores de microalgas).
- **NASA DONKI**: Telemetria em tempo real de erupções solares (Flares Classe X), tempestades de prótons (SEP) e ejeções de massa coronal (CME).
- **API de Imagens da NASA (`images-api.nasa.gov`)**: Fotografias oficiais astronômicas e de missões espaciais integradas dinamicamente a cada card.
- **Motor ML.NET FastTree Quad-Ensemble**: Rede de árvores de decisão treinada com 240.000 amostras biofísicas ao longo de 36 atributos, avaliando saúde da tripulação, confiabilidade do hardware, rendimento de combustível e dias de estadia.

---

## 🏗️ Arquitetura da Solução

O repositório é organizado em uma arquitetura compartilhada e multiplataforma com **.NET 11**:
- **`src/PymSpaceOrbit.Shared`**: Razor Class Library contendo todas as Views, Páginas, Componentes MudBlazor, Catálogo de Cards oficial (`PymSpaceOrbit_decks_catalog.json`), Serviços do Jogo (`IGameService`) e Modelo Biofísico (`IPymGameOrbitMlModelService`).
- **`src/PymSpaceOrbit`**: Aplicação **.NET MAUI Blazor Hybrid** para Android, iOS, macOS (MacCatalyst) e Windows.
- **`src/PymSpaceOrbit.Web`**: Aplicação **Blazor WebAssembly** hospedada na web e pronta para GitHub Pages.

---

## ⚡ Primeiros Passos

### Pré-requisitos
1. Clone o repositório:
   ```bash
   git clone https://github.com/PymSpaceLabs/PymSpaceOrbit.git
   cd PymSpaceOrbit
   ```
2. Instale o [.NET 11 SDK](https://dotnet.microsoft.com/download/dotnet/11.0).
3. Instale os workloads do .NET MAUI e Blazor WebAssembly:
   ```bash
   dotnet workload install maui wasm-tools
   ```

---

## 🔧 Compilação e Execução

### 1. Restaurar Dependências
Na raiz do repositório, execute:
```bash
dotnet restore
```

### 2. Compilar a Solução
```bash
dotnet build
```

### 3. Executar por Plataforma

- **Web (Blazor WebAssembly / Kestrel)**:
  ```bash
  dotnet run --project ./src/PymSpaceOrbit.Web/PymSpaceOrbit.Web.csproj
  ```

- **Windows Desktop (.NET MAUI)**:
  ```bash
  dotnet run --project ./src/PymSpaceOrbit/PymSpaceOrbit.csproj -f net11.0-windows10.0.19041.0
  ```

- **Android (.NET MAUI)**:
  ```bash
  dotnet run --project ./src/PymSpaceOrbit/PymSpaceOrbit.csproj -f net11.0-android
  ```

- **iOS (.NET MAUI — macOS)**:
  ```bash
  dotnet run --project ./src/PymSpaceOrbit/PymSpaceOrbit.csproj -f net11.0-ios
  ```

- **MacCatalyst (.NET MAUI — macOS)**:
  ```bash
  dotnet run --project ./src/PymSpaceOrbit/PymSpaceOrbit.csproj -f net11.0-maccatalyst
  ```

---

## 🎮 Telas e Funcionalidades Principais

| Tela | Rota | Descrição |
|---|---|---|
| **Menu Principal** | `/` | Banner com logo, seleção de idioma e 4 botões de ação: Jogar (modal de destino), Modo Construção, Galeria e Opções. |
| **Tabuleiro de Missão** | `/play` | **Fase 1 (Construção da Base)**: 5 slots centrais e bandeja de cartas da mão.<br>**Fases 2 e 3 (Turnos e Decisões)**: Pilhas de compra/descarte, 3 escolhas de exploração do turno em barras horizontais com trade-offs em Watts, O2, Blindagem, Radiação e Desgaste de Traje.<br>**Fase 4 (Desfechos)**: Vitória com Safe Rate-Limit ou 3 modos de Game Over. |
| **Galeria & Sobre** | `/info` | Navegação pelos 5 sets de cartas (Ship Build, ECLSS, Ambientes, Decisões de Base, DONKI Space Weather), grid 2x4 com paginação, zoom do card com imagem oficial da NASA, resumo educativo infantil (7–12 anos) e glossário técnico. |
| **Configurações & Telemetria** | `/options` | Seleção de idioma (pt-BR / en-US), slider de tripulação (1 a 16 astronautas), slider de cartas mínimas da base (3 a 8) e alternância de sincronização ao vivo da API da NASA. |

---

## 📚 Documentação Técnica Adicional
- **[ABOUT.ptbr.md](ABOUT.ptbr.md)**: Glossário detalhado em Português sobre todos os subsistemas de suporte de vida (ECLSS), tecnologias de propulsão, clima espacial DONKI e regras do motor FastTree.
- **[ABOUT.md](ABOUT.md)**: Versão completa do glossário aeroespacial e biofísico em Inglês (en-US).
