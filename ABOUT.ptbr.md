# PymGameOrbit — Jogo de Cartas de Exploração Espacial e Sobrevivência Orbital
## Glossário Geral de Engenharia Aeroespacial, Biofísica & Clima Espacial (NASA & ML.NET)

O **PymGameOrbit** é um card game educativo e simulador biofísico de exploração espacial baseado em turnos. Nele, os jogadores projetam e operam uma **nave espacial e base orbital** na Estação Espacial Internacional (ISS), na Lua ou em Marte. O jogo combina a ciência de sistemas de suporte de vida regenerativos da NASA (ECLSS), monitoramento de clima espacial em tempo real (NASA DONKI) e predições estocásticas em tempo real através de um modelo **FastTree Quad-Ensemble do ML.NET** treinado com 240.000 amostras biofísicas ao longo de 36 parâmetros.

---

### 1. Casco, Energia e Estrutura (Ship Build & Chassis)
- **BNNT (Boron Nitride Nanotubes / Nanotubos de Nitreto de Boro Hidrogenados)**: Nanomaterial ultraleve e resistente com alta densidade de paragem de radiação cósmica galáctica (GCR) e prótons solares de alta energia (SEP), oferecendo blindagem superior sem o peso excessivo do chumbo ou alumínio.
- **KRUSTY (Kilopower Reactor Using Stirling Technology)**: Mini-reator nuclear de fissão de 10 a 40 kWe desenvolvido pela NASA Glenn, projetado para operar continuamente dia e noite na Lua e Marte, imune a tempestades de poeira e longas noites lunares.
- **iROSA (Roll-Out Solar Array)**: Painéis solares flexíveis e ultraleves que se desenrolam mecanicamente, proporcionando até 220 kWe de pico fotovoltaico com eficiência de massa otimizada.
- **EDS (Electrodynamic Dust Shield)**: Sistema de ondas elétricas multifásicas que repelem partículas abrasivas e eletrostaticamente carregadas de regolito lunar e marciano de visores, juntas e trajes espaciais.
- **xEMU (Exploration Extravehicular Mobility Unit)**: Traje espacial de nova geração da NASA Artemis, com maior mobilidade articular, regeneração aprimorada de CO2 e interfaces de acoplamento SuitPort externo.
- **DSOC (Deep Space Optical Communications)**: Transmissão de dados por feixes de raio laser no infravermelho próximo, permitindo taxas de transmissão 10 a 100 vezes superiores às ondas de rádio de frequência espacial convencionais.
- **Canadarm3 & Robôs Valkyrie R5**: Manipuladores robóticos avançados e robôs humanoides gêmeos operados por IA para manutenção e montagem externa autônoma sem expor a tripulação ao vácuo e à radiação.
- **ARED (Advanced Resistive Exercise Device)**: Equipamento de exercícios com cilindros a vácuo que aplica até 2.600 N de força para impedir osteopenia (perda óssea) e atrofia muscular em microgravidade.
- **Centrífuga de Sono de Curto Raio**: Sistema rotativo contra-rotativo que gera gravidade artificial entre 0,38g e 0,50g durante o repouso para mitigar a síndrome neuro-ocular associada ao voo espacial (SANS).
- **Baterias de Estado Sólido Li-Enxofre (SABERS) & Células PEM**: Armazenamento eletroquímico de alta densidade energética para fornecer energia de emergência durante eclipses e tempestades de poeira severas.

---

### 2. Suporte de Vida em Circuito Fechado (Marshall ECLSS)
- **WPA (Water Processor Assembly)**: Reator catalítico e colunas de leito multifiltrante que purificam umidade do ar e águas cinzas da cabine com taxa de recuperação nominal de 98,5%.
- **UPA (Urine Processor Assembly) & VCD (Vapor Compression Distillation)**: Destilador rotativo a vácuo para evaporação e condensação da urina humana em ambiente de microgravidade.
- **BPA (Brine Processor Assembly)**: Membranas de ionômero aquecidas que extraem a água residual retida na salmoura tóxica da UPA, elevando o fechamento do ciclo hídrico da base para até 99,2%.
- **OGA (Oxygen Generation Assembly)**: Célula de eletrólise de água com membrana de troca de prótons (PEM) que separa H2O em oxigênio respirável (O2) e hidrogênio residual (H2).
- **CDRA (Carbon Dioxide Removal Assembly)**: Sistema de dessorção térmica com 4 leitos de zeólita sintética 5A que adsorve seletivamente o CO2 expirado e regenera o ar da cabine continuamente.
- **Reator Sabatier**: Reator termoquímico com catalisador de rutênio que combina CO2 expirado e hidrogênio da eletrólise a 400°C para gerar metano (CH4) e água (H2O).
- **Reator Bosch em Série & Pirólise de Metano**: Decomposição de hidrocarbonetos para deposição de carbono sólido e recuperação de 100% do hidrogênio em ciclo fechado.
- **PBR (Fotobiorreator de Microalgas Chlorella e Spirulina)**: Culturas líquidas densas de microalgas iluminadas que consomem CO2 10x mais rápido que plantas terrestres, gerando oxigênio e biomassa comestível rica em proteínas.
- **TCCS (Trace Contaminant Control Subsystem)**: Leitos de carvão ativado e reatores de oxidação catalítica para neutralizar monóxido de carbono, etanol e voláteis orgânicos.
- **AnMBR (Biorreator de Membrana Anaeróbica)**: Processador biológico de resíduos sólidos e biomassa para metanização controlada e fertilização orgânica em aeroponia.

---

### 3. Decisões Arquiteturais e Habitats de Superfície
- **Sinterização de Regolito Lunar por Micro-ondas / Laser 3D**: Derretimento robótico da poeira da Lua para erguer abóbadas espessas com sobrecarga de 45 g/cm², bloqueando 95% da radiação ambiente e impactos de micrometeoroides.
- **Tubos de Lava em Arsia Mons (Marte)**: Cavernas vulcânicas naturais de basalto maciço que oferecem proteção natural superior a 68 g/cm² contra radiação e variações térmicas extremas.
- **MOXIE (Mars Oxygen ISRU Experiment)**: Usina de eletrólise de óxido sólido que aquece a atmosfera marciana de CO2 a 800°C para produzir oxigênio puro e monóxido de carbono.
- **Mineração de Gelo na Cratera Shackleton (Lua)**: Extração de gelo volátil nas crateras permanentemente sombreadas do Polo Sul Lunar para purificação de água potável e eletrólise de combustíveis criogênicos (Methalox/Hydrolox).
- **Relay Óptico LunarNet L2 e Areoestacionário DSOC**: Constelações de satélites em pontos de Lagrange e órbitas sincrônicas para comunicação laser contínua entre a Terra e as bases planetárias.

---

### 4. Clima Espacial & Eventos NASA DONKI
- **DONKI (Database of Notifications, Knowledge, Information)**: Centro de telemetria e inteligência espacial em tempo real da NASA para monitoramento de clima espacial heliosférico.
- **Solar Flares Classe X**: As maiores e mais intensas erupções solares magnéticas, capazes de provocar blecautes de rádio em HF e fortes descargas eletrostáticas em naves e postos avançados.
- **SEP (Solar Energetic Particle Events)**: Tempestades de prótons de alta energia (>100 MeV) acelerados por ondas de choque coronais, exigindo abrigo em câmaras hidráulicas de tempestade.
- **CME (Coronal Mass Ejection)**: Ejeções de massa coronal com bilhões de toneladas de plasma magnetizado que viajam a velocidades de 1.000 a mais de 2.500 km/s.
- **Decréscimo de Forbush**: Redução temporária na contagem de raios cósmicos galácticos decorrente da blindagem magnética dinâmica criada pela passagem de uma CME.

---

### 5. Motor ML.NET Quad-Ensemble & Mecânicas de Gameplay
- **Arquitetura FastTree Quad-Ensemble**: Quatro regressores Gradient Boosted Trees do ML.NET treinados com 240.000 amostras e avaliados em tempo real com base em 36 atributos físicos:
  1. **Fuel Yield Rate**: Taxa de rendimento na síntese de propelente metano para retorno seguro.
  2. **Max Exploration Stay Days**: Previsão de dias máximos de estadia na superfície antes de exaustão de insumos ou desgaste de trajes.
  3. **Crew Health Survival Probability**: Probabilidade estocástica de sobrevivência e manutenção da saúde física/psicológica da tripulação.
  4. **System Reliability Index**: Confiabilidade integrada dos sistemas de energia, suporte de vida e integridade de casco.
- **Fase de Construção (BuildPhase)**: Seleção inicial de no mínimo 5 cartas da mão para equipar a Nave-Bioma antes de iniciar os turnos de exploração.
- **Fase de Turnos e Decisões (CardPlay & ExplorationDecision)**: Execução de jogadas com até 2 cartas por turno e resolução de dilemas espaciais que afetam Watts, Oxigênio, Blindagem, Radiação e Desgaste de Trajes.
- **Desfechos do Simulador**:
  - `SuccessNominalReturn`: Vitória completa após 9 estágios com telemetria dentro dos limites do protocolo Safe Rate-Limit.
  - `GameOverEclssAndSuitCollapse`: Colapso do ciclo hídrico WPA/BPA e fadiga de materiais dos trajes xEMU.
  - `GameOverDonkiSolarStormRadiation`: Sobrecarga de radiação ionizante por tempestade solar extrema Classe X do NASA DONKI.
  - `GameOverCo2AndCaloricOverload`: Acidose respiratória por saturação do CDRA e esgotamento calórico dos suprimentos vitais.
