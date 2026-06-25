using UnityEngine;

namespace ClearlyPrepareScreenAddon
{
    /// <summary>
    /// Если GameWorld.OnGameStarted не приходит после нуля countdown — снимаем чёрный экран.
    /// </summary>
    internal sealed class DeployWatchdogBehaviour : MonoBehaviour
    {
        private bool _armed;
        private float _releaseAtUnscaledTime;

        public void Arm(float secondsAfterZero)
        {
            if (secondsAfterZero <= 0f)
            {
                secondsAfterZero = 15f;
            }

            _armed = true;
            _releaseAtUnscaledTime = Time.unscaledTime + secondsAfterZero;
        }

        public void Disarm()
        {
            _armed = false;
        }

        private void Update()
        {
            if (!_armed || !CountdownSession.IsActive)
            {
                return;
            }

            if (CountdownSession.GetRemainingSeconds() > 0f)
            {
                return;
            }

            if (Time.unscaledTime < _releaseAtUnscaledTime)
            {
                return;
            }

            _armed = false;
            PluginCore.ForceReleaseAfterCountdown("watchdog: GameWorld.OnGameStarted timeout");
        }
    }
}
