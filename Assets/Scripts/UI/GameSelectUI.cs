using System;
using System.Collections.Generic;
using AjouFestival.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AjouFestival.UI
{
    [ExecuteAlways]
    public sealed class GameSelectUI : MonoBehaviour
    {
        [Header("Card Setup")]
        [SerializeField] private Transform cardRoot;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Transform[] gameCards;
        [SerializeField] private string[] fallbackCardNames =
        {
            "AjouBoontuCard",
            "BalanceWalkCard",
            "SoccerCard",
            "ComingSoonCard1",
            "ComingSoonCard2"
        };

        [Header("Carousel")]
        [SerializeField] private float cardHorizontalSpacing = 360f;
        [SerializeField] private float cardVerticalOffset = 28f;
        [SerializeField] private float selectedScale = 1.0f;
        [SerializeField] private float nearbyScale = 0.86f;
        [SerializeField] private float farScale = 0.72f;
        [SerializeField] private float selectedAlpha = 1f;
        [SerializeField] private float nearbyAlpha = 0.62f;
        [SerializeField] private float farAlpha = 0.25f;
        [SerializeField] private float sideTilt = 5f;
        [SerializeField] private float transitionDuration = 0.28f;
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Navigation Buttons")]
        [SerializeField] private Button leftNavigationButton;
        [SerializeField] private Button rightNavigationButton;
        [SerializeField] private bool createNavigationButtons = true;
        [SerializeField] private string leftNavigationText = "<";
        [SerializeField] private string rightNavigationText = ">";
        [SerializeField] private Sprite leftNavigationSprite;
        [SerializeField] private Sprite rightNavigationSprite;
        [SerializeField] private Vector2 navigationButtonSize = new Vector2(58f, 76f);
        [SerializeField] private Vector2 leftNavigationPosition = new Vector2(34f, 0f);
        [SerializeField] private Vector2 rightNavigationPosition = new Vector2(-34f, 0f);
        [SerializeField] private Color navigationButtonColor = new Color(1f, 1f, 1f, 0f);
        [SerializeField] private Color navigationTextColor = Color.white;

        [Header("SFX")]
        [SerializeField] private AudioClip cardMoveSfx;
        [SerializeField, Range(0f, 1f)] private float cardMoveSfxVolume = 1f;

        private readonly List<CardItem> cardItems = new();
        private int currentIndex;
        private Coroutine transitionRoutine;
        private Font uiFont;

        private sealed class CardItem
        {
            public string name;
            public RectTransform rect;
            public CanvasGroup canvasGroup;
            public Button startButton;
            public bool selectable;
        }

        private void Awake()
        {
            if (cardRoot == null)
            {
                cardRoot = transform;
            }

            if (mainMenuButton == null)
            {
                mainMenuButton = transform.Find("MainMenuButton")?.GetComponent<Button>();
            }
        }

        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                EnsureNavigationButtonsExist();
            }
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                ApplyNavigationButtonSettings();
            }
        }

        private void Start()
        {
            GameSessionManager.Ensure();
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            InitializeCards();
            SetupListeners();
            SetupNavigationButtons();
            SetSelectedIndex(0, true);

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(SceneLoader.LoadMainMenu);
            }
        }

        private void Update()
        {
            if (cardItems.Count == 0)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                SelectPreviousCard();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                SelectNextCard();
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            {
                TryStartCurrent();
            }
        }

        private void InitializeCards()
        {
            cardItems.Clear();

            if (gameCards != null && gameCards.Length > 0)
            {
                AddCardEntries(gameCards);
            }
            else
            {
                AddCardEntriesByName();
            }

            if (cardItems.Count == 0)
            {
                var fallback = new List<Transform>();
                for (int i = 0; i < cardRoot.childCount; i++)
                {
                    Transform child = cardRoot.GetChild(i);
                    if (child != null && child.name.IndexOf("Card", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        fallback.Add(child);
                    }
                }

                if (fallback.Count > 0)
                {
                    AddCardEntries(fallback.ToArray());
                }
            }
        }

        private void AddCardEntriesByName()
        {
            var entries = new List<Transform>();
            for (int i = 0; i < fallbackCardNames.Length; i++)
            {
                Transform card = cardRoot.Find(fallbackCardNames[i]);
                if (card != null && !entries.Contains(card))
                {
                    entries.Add(card);
                }
            }

            AddCardEntries(entries.ToArray());
        }

        private void AddCardEntries(Transform[] cards)
        {
            foreach (Transform card in cards)
            {
                if (card == null)
                {
                    continue;
                }

                RectTransform rect = card.GetComponent<RectTransform>();
                if (rect == null)
                {
                    continue;
                }

                CanvasGroup group = card.GetComponent<CanvasGroup>();
                if (group == null)
                {
                    group = card.gameObject.AddComponent<CanvasGroup>();
                }

                Button button = card.Find("StartButton")?.GetComponent<Button>();

                cardItems.Add(new CardItem
                {
                    name = card.name,
                    rect = rect,
                    canvasGroup = group,
                    startButton = button,
                    selectable = IsSelectableCard(card.name)
                });
            }
        }

        private bool IsSelectableCard(string cardName)
        {
            return cardName == "AjouBoontuCard" || cardName == "BalanceWalkCard" || cardName == "SoccerCard";
        }

        private void SetupListeners()
        {
            for (int i = 0; i < cardItems.Count; i++)
            {
                CardItem item = cardItems[i];
                if (item.startButton == null)
                {
                    continue;
                }

                int index = i;
                item.startButton.onClick.RemoveAllListeners();
                item.startButton.onClick.AddListener(() => ActivateCard(index));
            }
        }

        private void SetupNavigationButtons()
        {
            EnsureNavigationButtonsExist();

            if (Application.isPlaying)
            {
                if (leftNavigationButton != null)
                {
                    leftNavigationButton.onClick.RemoveAllListeners();
                    leftNavigationButton.onClick.AddListener(SelectPreviousCard);
                }

                if (rightNavigationButton != null)
                {
                    rightNavigationButton.onClick.RemoveAllListeners();
                    rightNavigationButton.onClick.AddListener(SelectNextCard);
                }
            }
        }

        private void EnsureNavigationButtonsExist()
        {
            if (leftNavigationButton == null)
            {
                leftNavigationButton = transform.Find("LeftNavigationButton")?.GetComponent<Button>();
            }

            if (rightNavigationButton == null)
            {
                rightNavigationButton = transform.Find("RightNavigationButton")?.GetComponent<Button>();
            }

            if (createNavigationButtons)
            {
                if (leftNavigationButton == null)
                {
                    leftNavigationButton = CreateNavigationButton("LeftNavigationButton", leftNavigationText, leftNavigationSprite, new Vector2(0f, 0.5f), leftNavigationPosition);
                }

                if (rightNavigationButton == null)
                {
                    rightNavigationButton = CreateNavigationButton("RightNavigationButton", rightNavigationText, rightNavigationSprite, new Vector2(1f, 0.5f), rightNavigationPosition);
                }
            }

            ApplyNavigationButtonSettings();
        }

        private void ApplyNavigationButtonSettings()
        {
            ApplyNavigationButtonSettings(leftNavigationButton, leftNavigationText, leftNavigationSprite, new Vector2(0f, 0.5f), leftNavigationPosition);
            ApplyNavigationButtonSettings(rightNavigationButton, rightNavigationText, rightNavigationSprite, new Vector2(1f, 0.5f), rightNavigationPosition);
        }

        private void ApplyNavigationButtonSettings(Button button, string label, Sprite sprite, Vector2 anchor, Vector2 anchoredPosition)
        {
            if (button == null)
            {
                return;
            }

            RectTransform rect = button.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = anchor;
                rect.anchorMax = anchor;
                rect.pivot = new Vector2(0.5f, 0.5f);

                if (rect.sizeDelta == Vector2.zero)
                {
                    rect.sizeDelta = navigationButtonSize;
                }

                if (rect.anchoredPosition == Vector2.zero)
                {
                    rect.anchoredPosition = anchoredPosition;
                }
            }

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = sprite;
                image.type = Image.Type.Simple;
                image.color = sprite != null && navigationButtonColor.a <= 0f ? Color.white : navigationButtonColor;
            }

            CommonButtonUI commonButton = button.GetComponent<CommonButtonUI>();
            if (commonButton != null)
            {
                commonButton.SetTintTargetGraphic(false);
            }

            Text text = button.transform.Find("Text")?.GetComponent<Text>();
            if (text != null)
            {
                text.text = label;
                text.color = navigationTextColor;
            }
        }

        private void SelectPreviousCard()
        {
            SetSelectedIndex(currentIndex - 1);
        }

        private void SelectNextCard()
        {
            SetSelectedIndex(currentIndex + 1);
        }

        private void SetSelectedIndex(int newIndex, bool immediate = false)
        {
            if (cardItems.Count == 0)
            {
                return;
            }

            int wrappedIndex = WrapIndex(newIndex);
            if (wrappedIndex == currentIndex && !immediate)
            {
                return;
            }

            currentIndex = wrappedIndex;
            if (!immediate)
            {
                PlayCardMoveSfx();
            }

            if (transitionRoutine != null)
            {
                StopCoroutine(transitionRoutine);
            }

            transitionRoutine = StartCoroutine(AnimateCards(immediate ? 0f : transitionDuration));
        }

        private int WrapIndex(int index)
        {
            int count = Mathf.Max(1, cardItems.Count);
            index %= count;
            if (index < 0)
            {
                index += count;
            }

            return index;
        }

        private void TryStartCurrent()
        {
            if (currentIndex < 0 || currentIndex >= cardItems.Count)
            {
                return;
            }

            ActivateCard(currentIndex);
        }

        private void ActivateCard(int index)
        {
            if (index != currentIndex)
            {
                SetSelectedIndex(index);
                return;
            }

            CardItem item = cardItems[index];
            if (!item.selectable)
            {
                return;
            }

            switch (item.name)
            {
                case "AjouBoontuCard":
                    SceneLoader.LoadAjouBoontu();
                    break;
                case "BalanceWalkCard":
                    SceneLoader.LoadBalanceWalk();
                    break;
                case "SoccerCard":
                    SceneLoader.LoadSoccer();
                    break;
            }
        }

        private System.Collections.IEnumerator AnimateCards(float duration)
        {
            if (duration <= 0f)
            {
                ApplyCardVisualState(1f, false);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = transitionCurve.Evaluate(Mathf.Clamp01(elapsed / duration));
                ApplyCardVisualState(t, false);
                yield return null;
            }

            ApplyCardVisualState(1f, true);
        }

        private void ApplyCardVisualState(float t, bool finalState)
        {
            if (cardItems.Count == 0)
            {
                return;
            }

            for (int i = 0; i < cardItems.Count; i++)
            {
                CardItem item = cardItems[i];
                int distance = GetWrappedDistance(i, currentIndex);
                float abs = Mathf.Abs(distance);
                float maxDistance = Mathf.Max(1f, (cardItems.Count - 1) * 0.5f);
                float scale;
                float alpha;

                if (abs <= 1f || maxDistance <= 1f)
                {
                    scale = Mathf.Lerp(selectedScale, nearbyScale, abs);
                    alpha = Mathf.Lerp(selectedAlpha, nearbyAlpha, abs);
                }
                else
                {
                    float farNormalized = Mathf.Clamp01((abs - 1f) / (maxDistance - 1f));
                    scale = Mathf.Lerp(nearbyScale, farScale, farNormalized);
                    alpha = Mathf.Lerp(nearbyAlpha, farAlpha, farNormalized);
                }

                float targetX = distance * cardHorizontalSpacing;
                float targetY = -abs * cardVerticalOffset;
                float targetRotation = -distance * sideTilt;

                Vector2 targetPosition = new Vector2(targetX, targetY);

                item.rect.anchoredPosition = Vector2.LerpUnclamped(item.rect.anchoredPosition, targetPosition, t);
                item.rect.localScale = Vector3.LerpUnclamped(item.rect.localScale, Vector3.one * scale, t);
                item.rect.localRotation = Quaternion.LerpUnclamped(
                    item.rect.localRotation,
                    Quaternion.Euler(0f, 0f, targetRotation),
                    t);

                if (item.canvasGroup != null)
                {
                    item.canvasGroup.alpha = Mathf.Lerp(item.canvasGroup.alpha, alpha, t);
                    item.canvasGroup.interactable = distance == 0 && item.selectable;
                    item.canvasGroup.blocksRaycasts = distance == 0;
                }

                if (item.startButton != null)
                {
                    item.startButton.interactable = distance == 0 && item.selectable;
                }
            }

            for (int i = 0; i < cardItems.Count; i++)
            {
                if (GetWrappedDistance(i, currentIndex) == 0)
                {
                    cardItems[i].rect.SetAsLastSibling();
                }
            }

            if (finalState)
            {
                transitionRoutine = null;
            }
        }

        private int GetWrappedDistance(int fromIndex, int targetIndex)
        {
            int distance = fromIndex - targetIndex;
            if (distance > cardItems.Count / 2)
            {
                distance -= cardItems.Count;
            }
            else if (distance < -cardItems.Count / 2)
            {
                distance += cardItems.Count;
            }

            return distance;
        }

        private void PlayCardMoveSfx()
        {
            if (cardMoveSfx == null)
            {
                return;
            }

            AudioManager.Ensure().PlaySfx(cardMoveSfx, cardMoveSfxVolume);
        }

        private Button CreateNavigationButton(string name, string label, Sprite sprite, Vector2 anchor, Vector2 anchoredPosition)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(CommonButtonUI));
            obj.transform.SetParent(transform, false);

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = navigationButtonSize;

            Image image = obj.GetComponent<Image>();
            if (sprite != null)
            {
                image.color = navigationButtonColor.a <= 0f ? Color.white : navigationButtonColor;
                image.sprite = sprite;
                image.type = Image.Type.Simple;
            }
            else
            {
                image.color = navigationButtonColor;
            }

            Button button = obj.GetComponent<Button>();
            button.targetGraphic = image;

            CommonButtonUI commonButton = obj.GetComponent<CommonButtonUI>();
            commonButton.SetTintTargetGraphic(false);

            Text text = CreateNavigationButtonText(obj.transform, label);
            text.raycastTarget = false;
            return button;
        }

        private Text CreateNavigationButtonText(Transform parent, string label)
        {
            GameObject obj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            obj.transform.SetParent(parent, false);

            Text text = obj.GetComponent<Text>();
            if (uiFont != null)
            {
                text.font = uiFont;
            }

            text.text = label;
            text.fontSize = 44;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = navigationTextColor;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 16;
            text.resizeTextMaxSize = 44;

            RectTransform rect = text.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return text;
        }
    }
}
