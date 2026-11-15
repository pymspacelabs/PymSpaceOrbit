# PymGameOrbit — Space Exploration & Orbital Survival Card Game
## Comprehensive Aerospace Engineering, Biophysics & Space Weather Glossary (NASA & ML.NET)

**PymGameOrbit** is an educational turn-based card game and biophysical space mission simulator. Players design and manage a unified spacecraft and surface outpost on the International Space Station (ISS), the Moon (Artemis Gateway & South Pole Base), or Mars (Areocentric Orbit & Surface ISRU). The simulation pairs real-world NASA regenerative Environmental Control and Life Support Systems (ECLSS), live space weather data from NASA DONKI, and a **240,000-sample, 36-feature ML.NET FastTree Quad-Ensemble model** providing real-time surrogate telemetry predictions.

---

### 1. Hull, Power & Structural Architecture (Ship Build & Chassis)
- **BNNT (Hydrogenated Boron Nitride Nanotubes)**: Ultralight, high-tensile nanomaterial with exceptional stopping power against Galactic Cosmic Rays (GCR) and high-energy Solar Energetic Protons (SEP), providing superior radiation protection without the severe mass penalty of lead or aluminum.
- **KRUSTY (Kilopower Reactor Using Stirling Technology)**: 10 to 40 kWe space-fission nuclear reactor developed by NASA Glenn, engineered for continuous day-and-night electrical power on the Moon and Mars, fully immune to dust storms and two-week lunar nights.
- **iROSA (Roll-Out Solar Array)**: Flexible, roll-out photovoltaic wings delivering up to 220 kWe of peak power with optimized specific mass and compact launch stowage.
- **EDS (Electrodynamic Dust Shield)**: Multi-phase electric wave system that actively pushes abrasive and electrostatically charged lunar and Martian regolith particles away from spacesuit visors, joints, and radiator surfaces.
- **xEMU (Exploration Extravehicular Mobility Unit)**: NASA's next-generation Artemis spacesuit featuring superior joint articulation, rapid CO2 scrubbing, and external rear SuitPort docking interfaces.
- **DSOC (Deep Space Optical Communications)**: Near-infrared laser beam telemetry transmitting data at rates 10 to 100 times higher than conventional space radio frequencies (Ka/X-band).
- **Canadarm3 & Valkyrie R5 EVA Humanoid Robots**: AI-assisted dexterous robotic arms and humanoid twin robots performing autonomous external inspection, maintenance, and structural assembly without exposing human astronauts to vacuum.
- **ARED (Advanced Resistive Exercise Device)**: Vacuum-cylinder resistance training system applying up to 2,600 N of biomechanical force to prevent microgravity bone mineral density loss and muscle atrophy.
- **Short-Radius Counter-Rotating Sleep Centrifuge**: Rotary artificial gravity platform generating 0.38g to 0.50g during sleep periods to alleviate Spaceflight-Associated Neuro-ocular Syndrome (SANS).
- **Solid-State Li-Sulfur Batteries (SABERS) & Regenerative PEM Fuel Cells**: High-specific-energy electrochemical energy storage buffer providing emergency microgrid power through solar conjunctions and planetary eclipses.

---

### 2. Closed-Loop Regenerative Life Support (Marshall ECLSS)
- **WPA (Water Processor Assembly)**: Catalytic oxidation reactor and multifiltration beds purifying cabin atmospheric humidity condensate and gray water with a 98.5% recovery rate.
- **UPA (Urine Processor Assembly) & VCD (Vapor Compression Distillation)**: Rotary distillation centrifuge that evaporates and condenses human urine in microgravity.
- **BPA (Brine Processor Assembly)**: Heated ionomer-membrane dewatering unit extracting residual potable water from toxic UPA brine, boosting overall outpost water recovery to 99.2%.
- **OGA (Oxygen Generation Assembly)**: Proton Exchange Membrane (PEM) water electrolysis stack splitting H2O into breathable oxygen (O2) and byproduct hydrogen gas (H2).
- **CDRA (Carbon Dioxide Removal Assembly)**: Thermal swing desorption system with 4 beds of synthetic 5A zeolite, continuously capturing metabolic CO2 to maintain safe cabin partial pressure.
- **Sabatier Propellant Reactor**: High-temperature catalytic reactor combining waste CO2 and electrolysis H2 over a ruthenium catalyst at 400°C to synthesize methane rocket fuel (CH4) and water (H2O).
- **Bosch Series Reactor & Methane Pyrolysis**: Closed hydrogen recovery loop converting hydrocarbon gases to solid carbon black while recycling 100% of hydrogen back into water and fuel synthesis.
- **PBR (Chlorella Vulgaris & Spirulina Photobioreactor)**: High-density illuminated liquid microalgae cultures consuming CO2 10x faster than terrestrial plants, bubbling pure oxygen and yielding protein-rich edible biomass.
- **TCCS (Trace Contaminant Control Subsystem)**: Granular activated carbon adsorbents and catalytic oxidizers destroying carbon monoxide, volatile organic compounds, and airborne contaminants.
- **AnMBR (Anaerobic Membrane Bioreactor)**: Biomass digester converting crew solid waste and crop residues into organic fertilizers and supplementary methane.

---

### 3. Surface Outpost Architecture & In-Situ Resource Utilization (ISRU)
- **3D-Sintered Lunar Regolith Vaults**: Robotic microwave and laser melting of Moon dust creating thick protective masonry vaults with 45 g/cm² of overburden, deflecting 95% of space radiation and micrometeoroids.
- **Arsia Mons Lava Tube Subsurface Outpost (Mars)**: Natural basalt volcanic caverns offering over 68 g/cm² of innate geological shielding against ionizing radiation, extreme thermal swings, and high-velocity dust storms.
- **MOXIE (Mars Oxygen ISRU Experiment)**: Solid-oxide electrolysis unit heating Martian CO2 to 800°C to extract breathable O2 and industrial propellants directly from the local Martian atmosphere.
- **Shackleton Crater Polar Ice Mining (Moon)**: Autonomous thermal mining of cryogenic volatiles in permanently shadowed regions of the lunar south pole for drinking water and Hydrolox/Methalox propulsion.
- **LunarNet L2 & Areostationary DSOC Laser Relays**: Lagrange point and synchronous communications constellations delivering high-bandwidth low-latency links between planetary outposts and Earth mission control.

---

### 4. Heliophysics & Space Weather Hazards (NASA DONKI)
- **DONKI (Database of Notifications, Knowledge, Information)**: NASA's comprehensive real-time space weather telemetry clearinghouse tracking flares, radiation storms, and coronal mass ejections across the solar system.
- **X-Class Solar Flares**: The most energetic magnetic eruptions from the Sun, generating immediate high-frequency radio blackouts and intense electrostatic transients.
- **SEP (Solar Energetic Particle) Events**: High-energy solar proton showers (>100 MeV) accelerated by coronal shock fronts, requiring immediate crew retreat to water-wall storm shelters.
- **CME (Coronal Mass Ejection)**: Giant bubbles of magnetized solar plasma traveling at velocities between 1,000 and 2,500 km/s, triggering magnetospheric storms and induced currents.
- **Forbush Decrease**: Temporary decline in galactic cosmic ray flux caused by the sweeping magnetic sheath of an interplanetary CME.

---

### 5. ML.NET FastTree Quad-Ensemble Engine & Gameplay Mechanics
- **Quad-Ensemble Architecture**: Four gradient-boosted decision tree regressors trained on 240,000 biophysical simulation runs and evaluating 36 input telemetry features in real time:
  1. **Fuel Yield Rate**: Predicted efficiency of local methane rocket propellant synthesis for the return transit.
  2. **Max Exploration Stay Days**: Dynamic estimate of maximum surface mission days before consumable reserves or suit integrity are depleted.
  3. **Crew Health Survival Probability**: Stochastic index tracking physical health, radiation dose accumulation, nutrition, and psychological morale.
  4. **System Reliability Index**: Combined hardware reliability across power systems, thermal loops, life support, and hull integrity.
- **Phase 1: Bio-Ship & Outpost Assembly (BuildPhase)**: Selecting and equipping the minimum required cards (default 5) from the starting hand before surface exploration begins.
- **Phases 2 & 3: Turn Exploration & Dilemmas (CardPlay & ExplorationDecision)**: Playing up to 2 cards per turn and making high-stakes decisions balancing Watts, Oxygen liters, Radiation dose, Shielding grams, and Suit wear.
- **Phase 4: Mission Simulator Outcomes (MissionEnded)**:
  - `SuccessNominalReturn`: 9 mission flight stages completed, touchdown achieved within the Safe Rate-Limit protocol, with high Science Discovery points.
  - `GameOverEclssAndSuitCollapse`: Catastrophic loss of the WPA/BPA water recycling loop and abrasive wear failure of xEMU EVA suits.
  - `GameOverDonkiSolarStormRadiation`: Critical ionizing radiation overload from an unmitigated NASA DONKI X-Class solar proton storm.
  - `GameOverCo2AndCaloricOverload`: Severe metabolic acidosis from CDRA zeolite failure and depletion of vital oxygen and caloric reserves.
