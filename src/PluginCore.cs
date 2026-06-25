using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using EFT;
using EFT.UI;
using EFT.UI.Matchmaker;
using HarmonyLib;
using UnityEngine;

namespace ClearlyPrepareScreenAddon
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
    public sealed class PluginCore : BaseUnityPlugin
    {
        internal static EnvironmentUI CachedEnvironmentUi;
        internal static PluginCore Instance { get; private set; }

        internal ConfigEntry<bool> EnableRaidInfoOverlay;
        internal ConfigEntry<float> FadeStartBeforeEndSeconds;
        internal ConfigEntry<float> FadeDurationSeconds;
        internal ConfigEntry<float> HoldBlackBeforeEndSeconds;
        internal ConfigEntry<float> OverlayRefreshIntervalSeconds;

        private RaidInfoOverlayBehaviour _overlay;
        private DeployWatchdogBehaviour _deployWatchdog;
        private bool _worldDataLogged;
        private bool _zeroCrossed;

        internal ConfigEntry<float> DeployWatchdogSeconds;

        private void Awake()
        {
            Instance = this;
            BindConfig();
            _overlay = gameObject.AddComponent<RaidInfoOverlayBehaviour>();
            _deployWatchdog = gameObject.AddComponent<DeployWatchdogBehaviour>();

            var harmony = new Harmony(PluginInfo.GUID);
            harmony.PatchAll(typeof(PluginCore).Assembly);
            Logger.LogInfo($"{PluginInfo.NAME} v{PluginInfo.VERSION} loaded");
        }

        private void BindConfig()
        {
            EnableRaidInfoOverlay = Config.Bind(
                "Raid Info",
                "Enable Overlay",
                true,
                "Показывать виджет с информацией о рейде на экране countdown");

            FadeStartBeforeEndSeconds = Config.Bind(
                "Fade",
                "Start Before End Seconds",
                3f,
                "За сколько секунд до конца countdown начинать затемнение");

            FadeDurationSeconds = Config.Bind(
                "Fade",
                "Fade Duration Seconds",
                2f,
                "Длительность плавного fade 0->1 (сек). По умолчанию: 3с до конца -> 1с до конца = 2с fade");

            HoldBlackBeforeEndSeconds = Config.Bind(
                "Fade",
                "Hold Black Before End Seconds",
                1f,
                "Сколько секунд до конца держать alpha=1 перед штатным пробуждением");

            OverlayRefreshIntervalSeconds = Config.Bind(
                "Raid Info",
                "Refresh Interval Seconds",
                0.25f,
                "Как часто обновлять статистику рейда на overlay");

            DeployWatchdogSeconds = Config.Bind(
                "Fade",
                "Deploy Watchdog Seconds",
                20f,
                "Через сколько секунд после нуля countdown принудительно убрать чёрный экран, если высадка зависла");
        }

        internal void TickOverlay(float remainingSeconds)
        {
            // Обновление перенесено в RaidInfoOverlayBehaviour.Update — надёжнее на всём countdown.
            if (!EnableRaidInfoOverlay.Value || !CountdownSession.IsActive)
            {
                _overlay?.SetVisible(false);
            }
        }

        internal void LogOverlaySnapshot(string reason, float remainingSeconds)
        {
            if (!EnableRaidInfoOverlay.Value)
            {
                return;
            }

            var snapshot = PrepareScreenInfoService.Collect(CountdownSession.ActiveProfile);
            Logger.LogInfo($"[RAID_INFO] {reason}\n{PrepareScreenInfoService.FormatPlain(snapshot, remainingSeconds)}");
        }

        internal static void HideEnvironmentUi()
        {
            try
            {
                var envUi = Object.FindObjectOfType<EnvironmentUI>();
                if (envUi != null && envUi.gameObject.activeSelf)
                {
                    CachedEnvironmentUi = envUi;
                    CachedEnvironmentUi.gameObject.SetActive(false);
                }
            }
            catch (System.Exception ex)
            {
                Instance?.Logger.LogError($"[ENV_UI] hide error: {ex.Message}");
            }
        }

        internal static void RestoreEnvironmentUi()
        {
            try
            {
                if (CachedEnvironmentUi != null && !CachedEnvironmentUi.gameObject.activeSelf)
                {
                    CachedEnvironmentUi.gameObject.SetActive(true);
                }
            }
            catch (System.Exception ex)
            {
                Instance?.Logger.LogError($"[ENV_UI] restore error: {ex.Message}");
            }
        }

        internal static void OnCountdownStarted(Profile profile, System.DateTime gameStartTime)
        {
            CountdownSession.Begin(profile, gameStartTime);
            if (Instance != null)
            {
                Instance._worldDataLogged = false;
            }
            BlackScreenFadeHelper.ResetFade();
            HideEnvironmentUi();
            Instance?.Logger.LogInfo($"[COUNTDOWN] started, ends at {gameStartTime:O}");
            Instance?.LogOverlaySnapshot("Старт countdown", CountdownSession.GetRemainingSeconds());
        }

        internal static void OnCountdownTick(float remainingSeconds)
        {
            if (!CountdownSession.IsActive)
            {
                return;
            }

            var plugin = Instance;
            if (plugin == null)
            {
                return;
            }

            float fadeStart = plugin.FadeStartBeforeEndSeconds.Value;
            float hold = plugin.HoldBlackBeforeEndSeconds.Value;
            float fadeDuration = plugin.FadeDurationSeconds.Value;

            // fadeStart=3, hold=1 => fade window = 2 sec (как в ТЗ)
            if (fadeDuration > 0f && fadeStart > hold)
            {
                fadeStart = hold + fadeDuration;
            }

            float alpha = CountdownSession.ComputeFadeAlpha(remainingSeconds, fadeStart, fadeStart - hold, hold);
            CountdownSession.SetRuntime(remainingSeconds, alpha);
            BlackScreenFadeHelper.ApplyAlpha(alpha);
            plugin.TickOverlay(remainingSeconds);

            if (!plugin._worldDataLogged)
            {
                var snap = PrepareScreenInfoService.Collect(CountdownSession.ActiveProfile);
                if (snap.HasWorldData)
                {
                    plugin._worldDataLogged = true;
                    plugin.LogOverlaySnapshot("Карта загружена", remainingSeconds);
                }
            }
        }

        internal static void OnCountdownEnded()
        {
            CountdownSession.End();
            Instance?._overlay?.SetVisible(false);
            RestoreEnvironmentUi();
        }

        [HarmonyPatch(typeof(MatchmakerFinalCountdown), nameof(MatchmakerFinalCountdown.Show), typeof(Profile), typeof(System.DateTime))]
        private static class MatchmakerFinalCountdownShowPatch
        {
            [HarmonyPostfix]
            private static void Postfix(Profile activeProfile, System.DateTime gameStartTime)
            {
                OnCountdownStarted(activeProfile, gameStartTime);
            }
        }

        [HarmonyPatch(typeof(MatchmakerFinalCountdown), nameof(MatchmakerFinalCountdown.Update))]
        private static class MatchmakerFinalCountdownUpdatePatch
        {
            [HarmonyPostfix]
            private static void Postfix()
            {
                if (!CountdownSession.IsActive)
                {
                    return;
                }

                float remaining = CountdownSession.GetRemainingSeconds();
                OnCountdownTick(remaining);

                if (remaining <= 0f)
                {
                    BlackScreenFadeHelper.ApplyAlpha(1f);
                }
            }
        }

        [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.OnGameStarted))]
        private static class GameWorldOnGameStartedPatch
        {
            [HarmonyPostfix]
            private static void Postfix()
            {
                OnCountdownEnded();
            }
        }
    }
}
