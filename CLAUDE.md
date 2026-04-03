# Blade of the Spade — Project Context

> Tento soubor čte Claude na začátku každé session.
> Aktualizuj sekci "Aktuální stav" na konci každého pracovního dne.

---

## O hře

**Žánr:** Relaxační ASMR simulátor / detektivní puzzle
**Platforma:** PC Steam (Unity, pouze PC, žádný WebGL ani mobil)
**Koncept:** Hráč je detektorář který hledá kovové artefakty na lokalitách. Sbírá a kombinuje indicie v hypotézy o tom, kde a co by se dalo nalézt. Na základě hypotéz získává přístup k lokalitám a provádí detektorový průzkum.
**Nálada:** Klidná, badatelská, postupné odkrývání tajemství
**Svět:** Pseudorealistický — fiktivní svět ale reálné historické artefakty
**Monetizace:** Zdarma na Steamu + Patreon (patreon.com/oldoakden)
**Struktura obsahu:** Epizodická — sezóny = nové regiony

---

## Architektura — 5 hlavních mechanik

### 1. Mapa (MapaPanel)
- Polygony = lokality (locked / available / hypothesis_sent / unlocked)
- Přístupnost závisí na úrovni Prestiže hráče a odeslaných hypotézách
- Klik na polygon → info panel (název, hint, stav)
- Tlačítko přesunu do lokace → načte LocationScene

### 2. Badatelna (ResearchPanel) — právě se řeší
- **Levá část:** knihovna indicií, záložky: Vše / Dokumenty / Mapy / Poznámky / Artefakty
- **Střední část:** 6 slotů pro kombinování indicií (hráč neví kolik potřebuje zaplnit)
- **Pravá část:** interaktivní mapa s lokalitami (záložky Mapa1/Mapa2/Mapa3), info o lokalitě
- Drag & drop: indicie je unikátní fyzický objekt — zmizí ze zdroje, lze vrátit zpět
- Progress bar pod sloty (vypínatelný GameObject) — globální, přepočítá se při změně lokace
- Správná kombinace → tlačítko Odeslat → EventBus.HypothesisBuilt → zpráva od AÚ
- Sloty jsou globální (jedna sada 6 slotů pro celou hru, přetrvávají při přepnutí lokace)
- Indicie jsou mapově specifické (availableInMapIDs) + společné pro více map

### 3. Sbírka (CollectionPanel)
- Lazy loading — 3D modely se načítají jen při kliknutí na předmět, pak se uvolní
- Seznam nalezených předmětů jako ikony (grid)
- 3D pohled na předmět (rotace + zoom)
- Identifikace: Materiál / Období / Typ → body prestiže
- Reportování předmětu → EventBus.ArtifactReported → zpráva od AÚ
- Odloženo na později (kostra hry má prioritu)

### 4. Komunikace (CommunicationPanel)
- Emailový klient, dvoupanelový layout
- Záložky: Nové zprávy / Čeká na zpracování / Zpracováno
- Logika přesouvání: přečtená + requiresAction → Čeká; akce splněna → Zpracováno
- Akční tlačítko "Potvrzuji" ve zprávě → MessageActionHandler.Execute(actionID)
- ESC + tlačítko CLOSE → návrat na NavigationPanel
- Zprávy od: PhDr. Jana Horáková (AÚ), Tomáš Novák (kamarád), Knihovna AÚ

### 5. Statistiky (StatisticsPanel)
- Zatím placeholder, bude obsahovat přehled hráčových dat a prestiže

---

## Scény

```
InitScene (index 0)
  → GameStateManager.Initialize() → načte save → přesměruje na WorkroomScene

WorkroomScene (index 1) — HLAVNÍ MENU
  → 3D scéna hráčovy pracovny (stůl, laptop, vitríny, krb)
  → UI overlay: Nová hra / Pokračovat / Nastavení / Konec
  → MainMenuUI.cs

MainGameScene (index 2) — HERNÍ UI
  → Všechny panely jako UI GameObjecty (jen jeden viditelný najednou)
  → NavigationPanel → CommunicationPanel / ResearchPanel / CollectionPanel / MapPanel / StatisticsPanel
  → MainGameUI.cs řídí zobrazování panelů

LocationScene_C3 (index 3) — DETEKTOR
  → Hotová scéna s detektorovým průzkumem
  → Pohled shora na cívku, zelené světlo = intenzita signálu
```

---

## Gameplay smyčka (onboarding)

```
Start → KOMUNIKACE (kamarádův mail = vstup do světa)
  ↓
SBÍRKA → bronzová sekera → identifikace → reportování → Poznatek 01
  ↓
KOMUNIKACE → Zpráva 02 od AÚ (po reportování nálezu)
  ↓
BADATELNA → přetažení indicií (Kronika + Poznatek01 + Kniha Typů) → odeslání hypotézy
  ↓
KOMUNIKACE → Zpráva 03 od AÚ → tlačítko Potvrzuji → odemčení C3
  ↓
MAPA → polygon C3 odemčen → přesun do lokace
  ↓
DETEKTOR → nález → reportování → nové indicie → další hypotézy
```

---

## Technické informace

**Engine:** Unity 6.3 LTS (C#)
**Cesta k projektu:** `d:\Work\!Personal\Unity Projects\Blade of the Spade`
**GitHub:** github.com/OldOakDen/Blade-of-the-Spade
**Jazyk komentářů a promptů:** Čeština
**AI coding:** Claude Code (VS Code extension)
**Project management:** ClickUp (workspace: 9017941927, list: 901712010655, folder: 90177504209)

### Klíčové skripty

| Skript | Účel |
|--------|------|
| `Core/GameStateManager.cs` | Singleton, DontDestroyOnLoad, JSON save (Newtonsoft.Json) |
| `Core/SceneController.cs` | Přechody mezi scénami, automatický save před přechodem |
| `Core/EventBus.cs` | Statická třída, eventy mezi systémy |
| `UI/NotificationManager.cs` | Singleton, vykřičníky u sekcí, reaguje na EventBus |
| `UI/MainMenuUI.cs` | Nová hra / Pokračovat / Nastavení / Konec |
| `UI/MainGameUI.cs` | Navigace mezi panely, zobrazení/skrytí |
| `UI/KomunikaceUI.cs` | Emailový klient, záložky, akční tlačítka |
| `UI/MessageActionHandler.cs` | Zpracování actionID ze zpráv |
| `UI/BadatelnaUI.cs` | Hlavní controller Badatelny |
| `UI/ClueItemUI.cs` | Drag & drop item v knihovně indicií |
| `UI/ClueSlotUI.cs` | Slot na dedukční desce |
| `UI/LibraryDropZone.cs` | Drop zóna pro vrácení indicie zpět do knihovny |
| `Data/ClueData.cs` | ScriptableObject pro jednu indicii |
| `Data/ClueDatabase.cs` | SO databáze všech indicií |
| `Data/LocationData.cs` | SO pro lokalitu (včetně hypotézy a requiredClues) |
| `Data/LocationDatabase.cs` | SO databáze lokalit |
| `Data/MessageData.cs` | SO pro jednu zprávu (včetně requiresAction, actionID) |
| `Data/MessageDatabase.cs` | SO databáze zpráv |

### ScriptableObject struktura

```
ArtifactData SO (zatím neimplementováno, plánováno)
  → artifactID, type, material, period, modelPrefab, icon, basePoints
  → creditAuthor, creditSource, creditLicense
  → GetMaterialSoundKey()

ClueData SO
  → clueID
  → clueNameKey (string, klíč do CluesTable), clueContentKey (string, klíč do CluesTable)
  → ClueType enum: Document / Map / Note / Artifact
  → isAvailableByDefault, availableInMapIDs, icon

LocationData SO
  → locationID, mapID, requiredPrestigeLevel
  → locationNameKey, hintTextKey, hypothesisNameKey, hypothesisDescKey
    (string klíče, tabulka LocationsTable — nastavit v Inspektoru ručně)
  → List<ClueData> requiredClues

MessageData SO
  → messageID
  → senderName, subject, body, actionButtonLabel (LocalizedString, tabulka Messages)
  → isUnlockedByDefault, requiresAction, actionID
```

### Lokalizace — jak to funguje

- `ClueData` a `LocationData` používají **plain string klíče** (zadávají se ručně v Inspektoru)
  - Zobrazení: `LocalizationSettings.StringDatabase.GetLocalizedString("CluesTable", key)`
- `MessageData` používá **LocalizedString** (tabulka se nastavuje přes picker v Inspektoru)
  - Zobrazení: `localizedString.GetLocalizedString()`
- Tabulky: `GameTable` (UI), `Messages` (zprávy), `CluesTable` (indicie), `LocationsTable` (lokality)

### GameState (JSON save)

```json
{
  "gameStarted": true,
  "prestigePoints": 0,
  "messages": {"msg_friend_01": true},
  "foundArtifacts": {},
  "hypotheses": {"hyp_bronze_settlement": true},
  "polygons": {"C3": "unlocked"},
  "mapMarkers": [],
  "processedActions": ["update_map_C3"],
  "clueSlots": ["clue_kronika_01", null, null, null, null, null],
  "unlockedClues": ["clue_kronika_blatna_01"],
  "activeLocationID": "loc_C3"
}
```

Save path: `Application.persistentDataPath/save.json`
(`C:\Users\pavel\AppData\LocalLow\Old Oak Den\Blade of the Spade\save.json`)

### Lokalizační tabulky

| Tabulka | Obsah |
|---------|-------|
| `GameTable` | UI texty, tlačítka, obecné |
| `Messages` | Texty zpráv (sender, subject, body, actionButton) |
| `CluesTable` | Názvy a obsah indicií |
| `LocationsTable` | Názvy lokalit, hinty, hypotézy |

### Architektonické vzory
- ScriptableObjects pro veškerá herní data (indicie, lokace, zprávy, artefakty)
- EventBus (statický) pro komunikaci mezi systémy
- GameStateManager (singleton, DontDestroyOnLoad) jako jediný zdroj pravdy
- JSON save přes Newtonsoft.Json do Application.persistentDataPath
- UI panely jako GameObjecty v jedné scéně (SetActive), ne samostatné scény

---

## Konvence a coding style

- **Jazyk komentářů:** Čeština
- **Naming:** PascalCase pro třídy a metody, camelCase pro privátní proměnné
- **Serializace:** `[SerializeField]` místo public polí
- Priorita: čistý a udržitelný kód, ne rychlý hack
- Vždy komentuj v češtině

---

## Aktuální stav

**Naposledy upraveno:** 3. dubna 2026

### Co se právě řeší
- Testování Badatelny v Unity — ověřit, že drag & drop a mapa lokalit fungují po opravách
- Přiřadit `clueItemPrefab` na každý ClueSlotUI v Inspektoru (stejný prefab jako v BadatelnaUI)
- Přiřadit `badatelna` referenci na LibraryDropZone GameObject

### Nedávno dokončeno
- GameStateManager, SceneController, EventBus, NotificationManager
- MainMenuUI, MainGameUI (ESC z panelu → GoBack, ESC z NavigationPanel → GoToWorkroom)
- KomunikaceUI (záložky, dvoupanelový layout, akční tlačítka, MessageActionHandler)
- ClueData, ClueDatabase, LocationData, LocationDatabase SO (plain string klíče, ne LocalizedString)
- BadatelnaUI — knihovna, sloty, progress bar, seznam lokalit, odeslání hypotézy, záložky map
- ClueItemUI — drag & drop z knihovny, proxy správná velikost a pozice (RectTransformUtility)
- ClueSlotUI — drop z knihovny i ze slotu (swap), drag ze slotu s proxy = clueItemPrefab (ne slot sám)
- LibraryDropZone — vrácení indicie ze slotu zpět do knihovny
- Opraveny 4 bugy v Badatelně (viz Otevřené bugy)
- Lokalizační tabulky: GameTable, Messages, CluesTable, LocationsTable
- ScriptableObjecty: 3 indicie pro C3, LocationData C3, ClueDatabase, LocationDatabase

### Nejvyšší priorita
1. Otestovat kompletní průchod Badatelnou: indicie → progress bar → odeslání hypotézy
2. Propojit Komunikaci s Badatelnou (Zpráva 03 → odemčení C3 → NotificationManager)
3. Kostra MapaPanel — polygony, stavy, odemčení C3
4. Propojení EventBus: HypothesisBuilt → Komunikace → MessageActionHandler → polygon unlock
5. Kostra Sbírky (odloženo)

---

## TODO

### Vysoká priorita
- [ ] Otestovat kompletní průchod onboardingem end-to-end
- [ ] MapaPanel — polygony, odemčení C3 po zprávě 03
- [ ] Propojení všech sekcí přes EventBus

### Střední priorita
- [ ] ArtifactData SO + konsolidace DetectTarget.cs
- [ ] Sbírka — lazy loading 3D modelů, identifikace
- [ ] Systém Prestiže (ovlivňuje přístup k lokalitám)
- [ ] Statistiky panel

### Nízká priorita / nápady
- [ ] WorkroomScene — 3D pracovna (placeholder nahradit skutečnou scénou)
- [ ] DLC mapy (Mapa2, Mapa3)
- [ ] Multiplayer časové výzvy
- [ ] Community obsah (hráčem vytvořené hypotézy)

---

## Otevřené bugy / problémy

| Bug | Popis | Stav |
|-----|-------|------|
| BadatelnaUI | Mapa lokalit se nenačítá — LocationButtonPrefab se neinstantiuje | Opraveno (Debug.Log přidán pro diagnostiku, null checks) |
| ClueItemUI | Drag proxy má špatnou velikost a pozici | Opraveno (RectTransformUtility + sizeDelta z originálu) |
| ClueSlotUI | Slot nezobrazí indicii vizuálně po dropu (emptyState/filledState) | Opraveno (UpdateVisual volána ze SetClue) |
| ClueSlotUI | Při dragu ze slotu se zobrazuje rámeček slotu místo indicie | Opraveno (proxy = Instantiate(clueItemPrefab) + SetVisuals) |

---

## Poznámky pro Claude

- Solo developer, hobby indie projekt
- Priorita: funkční kostra → pak obsah a data
- Při návrhu UI respektuj existující design (drag & drop indicie, 6 slotů, záložky)
- Hra propojuje všech 5 mechanik — při změně jedné mysli na dopad na ostatní
- Vždy komentuj v češtině
- Claude Code (VS Code extension) má přímý přístup k souborům — preferuj ho před GitHub API
- GitHub API selhává kvůli rate limitům a bezpečnostním pravidlům
- ClickUp: tvorba dokumentů nefunguje (Denied), tvorba tasků a komentářů funguje
