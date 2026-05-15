using System;
using System.Collections.Generic;
using AjouFestival.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AjouFestival.UI
{
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

        private readonly List<CardItem> cardItems = new();
        private int currentIndex;
        private Coroutine transitionRoutine;

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

        private void Start()
        {
            GameSessionManager.Ensure();

            InitializeCards();
            SetupListeners();
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
                SetSelectedIndex(currentIndex - 1);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                SetSelectedIndex(currentIndex + 1);
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
    }
}
