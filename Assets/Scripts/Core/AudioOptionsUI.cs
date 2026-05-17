using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AjouFestival.Core
{
    public sealed class AudioOptionsUI : MonoBehaviour
    {
        private AudioManager audioManager;
        private GameObject root;
        private GameObject panel;
        private Text musicValueText;
        private Text sfxValueText;
        private Slider musicSlider;
        private Slider sfxSlider;
        private Font uiFont;

        public void Initialize(AudioManager manager)
        {
            audioManager = manager;
            if (audioManager != null && !audioManager.ShowAudioOptionsUI)
            {
                DestroyRoot();
                return;
            }

            if (root == null)
            {
                BuildUI();
            }

            SyncSliders();
        }

        public void Rebuild()
        {
            if (audioManager == null)
            {
                audioManager = GetComponent<AudioManager>();
            }

            DestroyRoot();
            if (audioManager != null && !audioManager.ShowAudioOptionsUI)
            {
                return;
            }

            BuildUI();
            SyncSliders();
        }

        private void Awake()
        {
            if (audioManager == null)
            {
                audioManager = GetComponent<AudioManager>();
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void Start()
        {
            EnsureEventSystem();
            if (root == null)
            {
                BuildUI();
            }

            SyncSliders();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureEventSystem();
        }

        private void BuildUI()
        {
            if (audioManager != null && !audioManager.ShowAudioOptionsUI)
            {
                return;
            }

            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            root = new GameObject("AudioOptionsCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);

            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = audioManager != null ? audioManager.OptionsSortingOrder : 5000;

            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(960f, 540f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            Button toggleButton = CreateButton(
                root.transform,
                "OptionsButton",
                audioManager != null ? audioManager.OptionsButtonText : "\uC635\uC158",
                audioManager != null ? audioManager.OptionsButtonPosition : new Vector2(14f, -14f),
                audioManager != null ? audioManager.OptionsButtonSize : new Vector2(92f, 36f));
            toggleButton.onClick.AddListener(TogglePanel);

            Vector2 panelSize = audioManager != null ? audioManager.OptionsPanelSize : new Vector2(280f, 164f);
            panel = CreatePanel(
                root.transform,
                "OptionsPanel",
                audioManager != null ? audioManager.OptionsPanelPosition : new Vector2(14f, -58f),
                panelSize,
                audioManager != null ? audioManager.OptionsPanelColor : new Color(0.03f, 0.08f, 0.14f, 0.9f));
            ApplyImageSprite(panel.GetComponent<Image>(), audioManager != null ? audioManager.OptionsPanelSprite : null);

            CreateText(panel.transform, "Title", audioManager != null ? audioManager.OptionsTitleText : "\uC18C\uB9AC \uC124\uC815", 22, TextAnchor.MiddleLeft, new Vector2(16f, -16f), new Vector2(160f, 28f), GetTextColor());

            Button closeButton = CreateButton(panel.transform, "CloseButton", audioManager != null ? audioManager.CloseButtonText : "\uB2EB\uAE30", new Vector2(panelSize.x - 74f, -14f), new Vector2(58f, 30f));
            ApplyImageSprite(closeButton.targetGraphic as Image, audioManager != null ? audioManager.CloseButtonSprite : null);
            closeButton.onClick.AddListener(() => panel.SetActive(false));

            CreateSliderRow(panel.transform, "Music", audioManager != null ? audioManager.MusicLabelText : "\uBC30\uACBD\uC74C\uC545", new Vector2(16f, -58f), out musicSlider, out musicValueText);
            CreateSliderRow(panel.transform, "Sfx", audioManager != null ? audioManager.SfxLabelText : "\uD6A8\uACFC\uC74C", new Vector2(16f, -110f), out sfxSlider, out sfxValueText);

            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            panel.SetActive(false);
        }

        private void SyncSliders()
        {
            if (audioManager == null)
            {
                return;
            }

            if (musicSlider != null)
            {
                musicSlider.SetValueWithoutNotify(audioManager.MusicVolume);
                SetValueText(musicValueText, audioManager.MusicVolume);
            }

            if (sfxSlider != null)
            {
                sfxSlider.SetValueWithoutNotify(audioManager.SfxVolume);
                SetValueText(sfxValueText, audioManager.SfxVolume);
            }
        }

        private void TogglePanel()
        {
            if (panel != null)
            {
                panel.SetActive(!panel.activeSelf);
            }
        }

        private void OnMusicVolumeChanged(float value)
        {
            audioManager?.SetMusicVolume(value);
            SetValueText(musicValueText, value);
        }

        private void OnSfxVolumeChanged(float value)
        {
            audioManager?.SetSfxVolume(value);
            SetValueText(sfxValueText, value);
        }

        private static void SetValueText(Text label, float value)
        {
            if (label != null)
            {
                label.text = $"{Mathf.RoundToInt(value * 100f)}%";
            }
        }

        private void CreateSliderRow(Transform parent, string name, string label, Vector2 position, out Slider slider, out Text valueText)
        {
            float panelWidth = audioManager != null ? audioManager.OptionsPanelSize.x : 280f;
            float sliderWidth = Mathf.Max(80f, panelWidth - 174f);
            CreateText(parent, $"{name}Label", label, 18, TextAnchor.MiddleLeft, position, new Vector2(90f, 28f), GetTextColor());
            valueText = CreateText(parent, $"{name}Value", "100%", 16, TextAnchor.MiddleRight, new Vector2(panelWidth - 60f, position.y), new Vector2(44f, 28f), new Color(0.82f, 0.92f, 1f, 1f));
            slider = CreateSlider(parent, $"{name}Slider", new Vector2(position.x + 94f, position.y - 2f), new Vector2(sliderWidth, 24f));
        }

        private Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject obj = CreatePanel(parent, name, anchoredPosition, size, audioManager != null ? audioManager.OptionsButtonColor : new Color(0.08f, 0.42f, 0.86f, 0.96f));
            Button button = obj.AddComponent<Button>();
            button.targetGraphic = obj.GetComponent<Image>();
            ApplyImageSprite(button.targetGraphic as Image, ResolveButtonSprite(name));

            Text text = CreateText(obj.transform, "Text", label, 18, TextAnchor.MiddleCenter, Vector2.zero, size, GetTextColor());
            text.raycastTarget = false;
            return button;
        }

        private Slider CreateSlider(Transform parent, string name, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject sliderObj = new GameObject(name, typeof(RectTransform), typeof(Slider));
            sliderObj.transform.SetParent(parent, false);
            RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0f, 1f);
            sliderRect.anchorMax = new Vector2(0f, 1f);
            sliderRect.pivot = new Vector2(0f, 1f);
            sliderRect.anchoredPosition = anchoredPosition;
            sliderRect.sizeDelta = size;

            GameObject background = CreatePanel(sliderObj.transform, "Background", Vector2.zero, new Vector2(size.x, 8f), new Color(0.16f, 0.22f, 0.3f, 1f));
            RectTransform backgroundRect = background.GetComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0f, 0.5f);
            backgroundRect.anchorMax = new Vector2(1f, 0.5f);
            backgroundRect.pivot = new Vector2(0.5f, 0.5f);
            backgroundRect.anchoredPosition = Vector2.zero;
            backgroundRect.sizeDelta = new Vector2(0f, 8f);

            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0.5f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.5f);
            fillAreaRect.pivot = new Vector2(0.5f, 0.5f);
            fillAreaRect.anchoredPosition = Vector2.zero;
            fillAreaRect.sizeDelta = new Vector2(-12f, 8f);

            GameObject fill = CreatePanel(fillArea.transform, "Fill", Vector2.zero, Vector2.zero, audioManager != null ? audioManager.OptionsAccentColor : new Color(0.2f, 0.78f, 1f, 1f));
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(sliderObj.transform, false);
            RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(6f, 0f);
            handleAreaRect.offsetMax = new Vector2(-6f, 0f);

            GameObject handle = CreatePanel(handleArea.transform, "Handle", Vector2.zero, new Vector2(14f, 20f), Color.white);

            Slider slider = sliderObj.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            slider.fillRect = fillRect;
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.direction = Slider.Direction.LeftToRight;
            return slider;
        }

        private Text CreateText(Transform parent, string name, string text, int fontSize, TextAnchor alignment, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            obj.transform.SetParent(parent, false);

            Text label = obj.GetComponent<Text>();
            if (uiFont != null)
            {
                label.font = uiFont;
            }

            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = color;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 10;
            label.resizeTextMaxSize = fontSize;

            RectTransform rect = label.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return label;
        }

        private GameObject CreatePanel(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            obj.transform.SetParent(parent, false);

            Image image = obj.GetComponent<Image>();
            image.color = color;

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return obj;
        }

        private Color GetTextColor()
        {
            return audioManager != null ? audioManager.OptionsTextColor : Color.white;
        }

        private Sprite ResolveButtonSprite(string buttonName)
        {
            if (audioManager == null)
            {
                return null;
            }

            if (buttonName == "CloseButton")
            {
                return audioManager.CloseButtonSprite;
            }

            return audioManager.OptionsButtonSprite;
        }

        private static void ApplyImageSprite(Image image, Sprite sprite)
        {
            if (image == null || sprite == null)
            {
                return;
            }

            image.sprite = sprite;
            image.type = Image.Type.Sliced;
        }

        private void DestroyRoot()
        {
            if (root == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(root);
            }
            else
            {
                DestroyImmediate(root);
            }

            root = null;
            panel = null;
            musicValueText = null;
            sfxValueText = null;
            musicSlider = null;
            sfxSlider = null;
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem");
            DontDestroyOnLoad(eventSystem);
            eventSystem.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            eventSystem.AddComponent<StandaloneInputModule>();
#endif
        }
    }
}
