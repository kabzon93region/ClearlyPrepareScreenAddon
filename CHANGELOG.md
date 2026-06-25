# Changelog — ClearlyPrepareScreenAddon

## [1.1.0] — 2026-06-12

### Removed
- С экрана высадки убраны ошибочно добавленные секции DEBUG — МЕСТНОСТИ, ИГРОКИ, БОТЫ (перенесены в RaidIntelOverlay)
- Остаются: карта, точка входа, лут, таймер

## [1.0.9] — 2026-06-21

### Added (debug)
- Overlay: **DEBUG — МЕСТНОСТИ** — координаты, ближайшая зона по SpawnPointParams, TriggerZones
- Список всех зон инфильтрации текущей локации (ключ + локализация + число spawn points)
- Секции **ИГРОКИ (N)** и **БОТЫ НА КАРТЕ (N)** с подсчётом USEC/BEAR/дикие + загрузка ботов

### Fixed
- Точка входа: чтение `GameController.InfiltrationPoint` на Fika до появления `CoopGame` в Singleton

## [1.0.8] — 2026-06-12

### Fixed
- **Точка входа (Fika countdown):** `FikaBackendUtils.ClientType` вместо только `CoopGame` — на экране высадки больше не «Старое депо» из лобби до назначения спавна
- Пока Fika не назначил точку: «Определяется…» (SamePlace) или «Случайная точка (группа)»

## [1.0.7] — 2026-06-21

### Release / docs
- **Автономный мод** — полный форк ClearlyPrepareScreen, оригинал **не нужен**
- INSTALL/UNINSTALL: при обновлении удалять только `BepInEx/plugins/ClearlyPrepareScreenAddon/`
- Не путать с оригиналом (`GUID eft.hiddenhiraigi.clearpreraidscreen`, другая папка/DLL)

## [1.0.6] — 2026-06-21

### Fixed
- **Точка входа (Fika coop):** на экране высадки больше не показывается устаревший `profile.EntryPoint` (напр. всегда «Старое депо» на Лесу)
- Пока Fika не назначил `GameController.InfiltrationPoint`: «Случайная точка (группа)» или «Определяется…»
- После назначения спавна Fika — реальная зона из `InfiltrationPoint` / `SpawnPoint`

## [1.0.5] — 2026-06-15

### Fixed
- **Точка входа:** не показывается устаревший `EntryPoint` с другой карты (напр. «Старое депо» / `Old Station` с Леса на Таможне)
- Проверка ключа инфильтрации по `SpawnPointParams` текущей локации из `RaidSettings`
- Убран кэш первого значения — overlay пересчитывает точку входа на каждом тике

### Changed
- Подпись «Зона инфильтрации» → **«Точка входа»**
- Fika, режим «разные точки»: подсказка «Случайная точка (группа)», пока нет данных от хоста

## [1.0.4] — 2026-06-14

### Changed
- Карта: `bigmap (Таможня)` — id + русское имя в скобках
- Строка «Локация спавна» → **«Зона инфильтрации»** (то, что показывает «Таможня» на Customs — имя зоны, не баг)

## [1.0.3] — 2026-06-14

### Fixed
- **Локация спавна:** больше не показывает `SamePlace` (enum `PlayersSpawnPlace`)
- Берётся реальная точка инфильтрации: Fika `GameController.SpawnPoint/InfiltrationPoint`, `Player.SpawnPoint`, локализация как в UI карты (`EntryPointView`)

### Changed
- Локализованное имя карты в overlay (`{mapId} Name`)

## [1.0.0] — 2026-06-12

### Added
- Форк ClearlyPrepareScreen: скрытие EnvironmentUI на финальном countdown
- Плавный fade `PreloaderUI.SetBlackImageAlpha` 0→1 за 2 сек (начало за 3 сек до конца)
- Удержание alpha=1 последнюю секунду перед штатным пробуждением
- IMGUI overlay: лут, контейнеры, игроки/боты USEC/BEAR/дикие, загрузка ботов
- Конфиг BepInEx: fade и overlay
