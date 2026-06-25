using System;
using EFT;
using UnityEngine;

namespace ClearlyPrepareScreenAddon
{
    /// <summary>
    /// Состояние активного финального countdown перед рейдом.
    /// </summary>
    internal static class CountdownSession
    {
        public static bool IsActive { get; private set; }
        public static DateTime GameStartTime { get; private set; }
        public static Profile ActiveProfile { get; private set; }
        public static float LastRemainingSeconds { get; private set; } = float.MaxValue;
        public static float LastAlpha { get; private set; }

        public static void Begin(Profile profile, DateTime gameStartTime)
        {
            ActiveProfile = profile;
            GameStartTime = gameStartTime;
            LastRemainingSeconds = float.MaxValue;
            LastAlpha = 0f;
            IsActive = true;
        }

        public static void End()
        {
            IsActive = false;
            ActiveProfile = null;
            LastRemainingSeconds = float.MaxValue;
            LastAlpha = 0f;
        }

        public static float GetRemainingSeconds()
        {
            if (!IsActive)
            {
                return float.MaxValue;
            }

            var span = GameStartTime - EFTDateTimeClass.Now;
            var seconds = (float)span.TotalSeconds;
            return seconds < 0f ? 0f : seconds;
        }

        public static float ComputeFadeAlpha(float remainingSeconds, float fadeStartBeforeEnd, float fadeDuration, float holdAtFullSeconds)
        {
            if (remainingSeconds <= holdAtFullSeconds)
            {
                return 1f;
            }

            if (remainingSeconds > fadeStartBeforeEnd)
            {
                return 0f;
            }

            // Плавно 0 -> 1 между (fadeStartBeforeEnd) и (holdAtFullSeconds)
            float fadeWindow = fadeStartBeforeEnd - holdAtFullSeconds;
            if (fadeWindow <= 0f)
            {
                return 1f;
            }

            float progress = 1f - ((remainingSeconds - holdAtFullSeconds) / fadeWindow);
            return Mathf.Clamp01(progress);
        }

        public static void SetRuntime(float remainingSeconds, float alpha)
        {
            LastRemainingSeconds = remainingSeconds;
            LastAlpha = alpha;
        }
    }
}
