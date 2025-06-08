# Virtual AI Racing

## Inleiding

**Virtual AI Racing** is een VR-racespel waarbij de speler het opneemt tegen AI-tegenstanders. Deze AI-agents leren zelfstandig het circuit rijden via reinforcement learning. Door de combinatie van VR en AI ontstaat een uitdagende en meeslepende race-ervaring.

In deze tutorial leer je hoe je het project van nul opbouwt: van installatie en opzet tot het trainen van de AI-agent en het integreren in een VR-omgeving. Aan het einde heb je een werkend prototype waarin een getrainde AI tegen de speler racet in VR.

## Methoden

### Installatie

- Unity: 6000.0.36f1
- Python: 3.9.21
- ML-Agents: 1.0.0
- TensorFlow: 2.x
- Oculus Integration (Unity Asset Store): ??????????
- VSCode + Python- en C#-extensies
- Anaconda: 2.6.6

### Verloop van de simulatie

De speler komt via de VR-headset direct op een racetrack terecht. Na een countdown start de race automatisch. AI-wagens starten tegelijk met de speler. De speler bestuurt via VR-controllers (gas/rem/sturen). AI-auto’s zijn getrainde agents die reageren op de track. Na het einde van de race verschijnt de eindpositie op een virtueel scorebord.

### Observaties, acties en beloningen

**Observaties (input voor AI):**

- Positie t.o.v. racelijn
- Snelheid
- Oriëntatie op de track
- Afstand tot bochten

**Acties (output van AI):**

- Gas geven
- Remmen
- Sturen (links/rechts)

**Beloningen:**

- Positieve reward bij vooruitgang op de track
- Straf bij afwijken van track of crash
- Straf bij te lage snelheid of achterwaartse beweging

### Objecten in de simulatie

- **Speler:** bestuurt een voertuig via VR in first-person
- **AI-agent:** autonoom voertuig dat leert via reinforcement learning
- **Racetrack:** statisch circuit met bochten en rechte stukken
- **Omgeving:** basismodel (visuele feedback, geluidseffecten)

### Gedrag van objecten

- **Speler:** reageert direct op controllerinput
- **AI-agent:** anticipeert op bochten, leert racelijnen herkennen
- **Track/omgeving:** passief, geeft feedback via collisions en triggers

### Informatie uit de one-pager

- Directe start op circuit (geen menu)
- AI-agent gebaseerd op Single-Agent RL
- Interactie via VR-controllers
- Spelerservaring: snelle, meeslepende race met tegenstanders
- Geen boosts, alleen natuurlijke race-ervaring
- Einde: scorebord met finishpositie

### Afwijkingen t.o.v. one-pager (indien van toepassing)

_(Nog niet van toepassing – aanvullen indien het eindproduct afwijkt van plan)_

## Resultaten

### Tensorboard-grafieken

_(Voeg hier grafieken in zodra training is voltooid – let op: assen, titels en legenda verplicht)_

### Beschrijving van de grafieken

_(Bondige uitleg over verloop reward, loss, enz.)_

### Opvallende waarnemingen

_(Voorbeelden: traag leren bochten herkennen, overfitting op korte stukken, verbetering na extra training, enz.)_

## Conclusie

_(1 zin samenvatting van het project)_

_(Korte bespreking van resultaten – zonder cijfers)_

_(Reflectie en betekenis van de resultaten)_

_(Verbeterpunten en aanbevelingen voor de toekomst)_
