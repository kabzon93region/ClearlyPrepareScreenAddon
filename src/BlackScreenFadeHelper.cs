using System.Reflection;
using EFT.UI;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace ClearlyPrepareScreenAddon
{
    internal static class BlackScreenFadeHelper
    {
        private static FieldInfo _blackImageField;

        public static void ResetFade()
        {
            ApplyAlpha(0f);
        }

        public static void ApplyAlpha(float alpha)
        {
            if (!MonoBehaviourSingleton<PreloaderUI>.Instantiated)
            {
                return;
            }

            var preloader = MonoBehaviourSingleton<PreloaderUI>.Instance;
            alpha = Mathf.Clamp01(alpha);

            if (alpha > 0f)
            {
                EnsureBlackImageActive(preloader);
            }

            preloader.SetBlackImageAlpha(alpha);
        }

        private static void EnsureBlackImageActive(PreloaderUI preloader)
        {
            _blackImageField ??= AccessTools.Field(typeof(PreloaderUI), "_overlapBlackImage");
            if (_blackImageField?.GetValue(preloader) is Image image && image != null && !image.gameObject.activeSelf)
            {
                image.gameObject.SetActive(true);
            }
        }
    }
}
