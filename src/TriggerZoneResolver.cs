using System.Collections.Generic;
using System.Text;
using Comfort.Common;
using EFT;

namespace ClearlyPrepareScreenAddon
{
    internal static class TriggerZoneResolver
    {
        public static string ResolveActiveZones()
        {
            if (!Singleton<GameWorld>.Instantiated)
            {
                return string.Empty;
            }

            var player = Singleton<GameWorld>.Instance.MainPlayer;
            if (player == null)
            {
                return string.Empty;
            }

            try
            {
                var zones = player.TriggerZones;
                if (zones == null || zones.Count == 0)
                {
                    return string.Empty;
                }

                var names = new List<string>(zones.Count);
                for (int i = 0; i < zones.Count; i++)
                {
                    var zoneId = zones[i];
                    if (string.IsNullOrWhiteSpace(zoneId))
                    {
                        continue;
                    }

                    var localized = zoneId.Localized(null);
                    names.Add(string.IsNullOrWhiteSpace(localized) || localized == zoneId ? zoneId : localized);
                }

                if (names.Count == 0)
                {
                    return string.Empty;
                }

                var sb = new StringBuilder(128);
                for (int i = 0; i < names.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(", ");
                    }

                    sb.Append(names[i]);
                }

                return sb.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
