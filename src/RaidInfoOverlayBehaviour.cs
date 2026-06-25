using UnityEngine;

namespace ClearlyPrepareScreenAddon
{
    internal sealed class RaidInfoOverlayBehaviour : MonoBehaviour
    {
        private GUIStyle _boxStyle;
        private string _content = string.Empty;
        private bool _visible;
        private float _refreshTimer;

        public void SetContent(string content)
        {
            _content = content ?? string.Empty;
        }

        public void SetVisible(bool visible)
        {
            _visible = visible;
        }

        private void Update()
        {
            if (!CountdownSession.IsActive || PluginCore.Instance == null)
            {
                return;
            }

            if (!PluginCore.Instance.EnableRaidInfoOverlay.Value)
            {
                SetVisible(false);
                return;
            }

            _refreshTimer -= Time.deltaTime;
            if (_refreshTimer > 0f)
            {
                SetVisible(true);
                return;
            }

            float interval = Mathf.Max(0.05f, PluginCore.Instance.OverlayRefreshIntervalSeconds.Value);
            _refreshTimer = interval;

            float remaining = CountdownSession.GetRemainingSeconds();
            var snapshot = PrepareScreenInfoService.Collect(CountdownSession.ActiveProfile);
            SetContent(PrepareScreenInfoService.FormatOverlay(snapshot, remaining));
            SetVisible(true);
        }

        private void OnGUI()
        {
            if (!_visible || string.IsNullOrEmpty(_content))
            {
                return;
            }

            EnsureStyle();

            // Поверх чёрного экрана PreloaderUI
            GUI.depth = -10000;

            var content = new GUIContent(_content);
            var size = _boxStyle.CalcSize(content);
            float width = Mathf.Max(520f, size.x + 24f);
            float height = size.y + 16f;
            float x = 24f;
            float y = Screen.height - height - 48f;

            var rect = new Rect(x, y, width, height);
            GUI.Box(rect, _content, _boxStyle);
        }

        private void EnsureStyle()
        {
            if (_boxStyle != null)
            {
                return;
            }

            _boxStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 15,
                richText = true,
                wordWrap = true,
                padding = new RectOffset(12, 12, 10, 10)
            };
        }
    }
}
