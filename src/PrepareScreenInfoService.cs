using System.Linq;
using System.Text;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using UnityEngine;

namespace ClearlyPrepareScreenAddon
{
    internal sealed class PrepareScreenSnapshot
    {
        public string MapId = string.Empty;
        public string BotAmount = string.Empty;
        public string BotDifficulty = string.Empty;
        public string TriggerZones = string.Empty;
        public int LootItems;
        public int LootContainers;
        public bool HasWorldData;
    }

    internal static class PrepareScreenInfoService
    {
        public static PrepareScreenSnapshot Collect(Profile activeProfile)
        {
            var snapshot = new PrepareScreenSnapshot();
            var raidSettings = ResolveRaidSettings();

            if (raidSettings != null)
            {
                snapshot.MapId = raidSettings.SelectedLocation?.Id ?? raidSettings.LocationId ?? string.Empty;
                snapshot.BotAmount = raidSettings.WavesSettings.BotAmount.ToString();
                snapshot.BotDifficulty = raidSettings.WavesSettings.BotDifficulty.ToString();
            }

            if (Singleton<GameWorld>.Instantiated)
            {
                var world = Singleton<GameWorld>.Instance;
                snapshot.HasWorldData = true;
                if (!string.IsNullOrEmpty(world.LocationId))
                {
                    snapshot.MapId = world.LocationId;
                }

                snapshot.TriggerZones = TriggerZoneResolver.ResolveActiveZones();
                snapshot.LootItems = world.LootList.Count(item => item is LootItem);
                snapshot.LootContainers = world.LootList.Count(item => item is LootableContainer);
            }

            if (string.IsNullOrEmpty(snapshot.MapId))
            {
                snapshot.MapId = activeProfile?.Info?.MainProfileNickname ?? string.Empty;
            }

            return snapshot;
        }

        public static string FormatOverlay(PrepareScreenSnapshot data, float remainingSeconds)
        {
            var sb = new StringBuilder(512);
            sb.AppendLine("<b>ПОДГОТОВКА К РЕЙДУ</b>");
            sb.AppendLine();
            sb.AppendFormat("<color=#FFD966><b>До высадки: {0:F1} сек</b></color>\n", remainingSeconds);
            sb.AppendLine();
            sb.AppendLine("<color=#666666>────────────────────</color>");

            AppendSectionHeader(sb, "КАРТА");
            AppendRow(sb, "Локация", FormatMapId(data.MapId));
            AppendRow(sb, "Плотность AI", data.BotAmount);
            AppendRow(sb, "Сложность AI", data.BotDifficulty);

            if (data.HasWorldData)
            {
                AppendRow(
                    sb,
                    "TriggerZones",
                    string.IsNullOrEmpty(data.TriggerZones) ? "—" : data.TriggerZones,
                    string.IsNullOrEmpty(data.TriggerZones) ? "#888888" : "#C8C864");
            }
            else
            {
                AppendRow(sb, "TriggerZones", "ожидание карты…", "#888888");
            }

            if (!data.HasWorldData)
            {
                sb.AppendLine();
                sb.AppendLine("<color=#888888>Мир ещё загружается...</color>");
                sb.AppendLine("<color=#666666>Цифры появятся, когда карта будет готова.</color>");
                return sb.ToString();
            }

            AppendSectionHeader(sb, "ЛУТ НА КАРТЕ");
            AppendRow(sb, "Предметы на земле", data.LootItems.ToString(), "#7FD37F");
            AppendRow(sb, "Контейнеры", data.LootContainers.ToString(), "#7FD37F");

            return sb.ToString();
        }

        public static string FormatPlain(PrepareScreenSnapshot data, float remainingSeconds)
        {
            return StripRichText(FormatOverlay(data, remainingSeconds));
        }

        private static string FormatMapId(string mapId)
        {
            if (string.IsNullOrWhiteSpace(mapId))
            {
                return mapId;
            }

            var localized = (mapId + " Name").Localized(null);
            if (string.IsNullOrWhiteSpace(localized) || localized == mapId + " Name")
            {
                return mapId;
            }

            return mapId + " (" + localized + ")";
        }

        private static void AppendSectionHeader(StringBuilder sb, string title)
        {
            sb.AppendLine();
            sb.AppendFormat("<color=#BBBBBB><b>{0}</b></color>\n", title);
        }

        private static void AppendRow(StringBuilder sb, string label, string value, string valueColor = "white")
        {
            if (string.IsNullOrEmpty(value))
            {
                value = "—";
            }

            sb.AppendFormat("<color=#888888>  {0}:</color>  <color={1}>{2}</color>\n", label, valueColor, value);
        }

        private static string StripRichText(string rich)
        {
            if (string.IsNullOrEmpty(rich))
            {
                return rich;
            }

            var result = new StringBuilder(rich.Length);
            bool inTag = false;
            foreach (char c in rich)
            {
                if (c == '<')
                {
                    inTag = true;
                    continue;
                }

                if (c == '>')
                {
                    inTag = false;
                    continue;
                }

                if (!inTag)
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        private static RaidSettings ResolveRaidSettings()
        {
            try
            {
                return Object.FindObjectOfType<TarkovApplication>()?.CurrentRaidSettings;
            }
            catch
            {
                return null;
            }
        }
    }
}
