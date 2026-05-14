using AjouBuntu.Config;
using AjouBuntu.Player;
using AjouBuntu.UI;
using AjouBuntu.World;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace AjouBuntu.Core
{
    public enum SceneRole
    {
        Boot,
        Menu,
        Game,
        GameOver
    }

    public sealed class RuntimeBootstrap : MonoBehaviour
    {
        [SerializeField] private SceneRole sceneRole;
        [SerializeField] private GameConfig config;

        private void Awake()
        {
            if (config == null)
            {
                config = Resources.Load<GameConfig>("GameConfig");
            }

            if (config == null)
            {
                config = ScriptableObject.CreateInstance<GameConfig>();
            }

            config.EnsureDefaults();

            switch (sceneRole)
            {
                case SceneRole.Boot:
                    SceneTransitionManager.LoadMenu();
                    break;
                case SceneRole.Menu:
                    BuildMenu();
                    break;
                case SceneRole.Game:
                    BuildGame();
                    break;
                case SceneRole.GameOver:
                    BuildGameOver();
                    break;
            }
        }

        private void BuildMenu()
        {
            SetupCamera();
            EnsureEventSystem();
            BackgroundManager background = FindFirstObjectByType<BackgroundManager>();
            if (background == null)
            {
                background = new GameObject("BackgroundManager").AddComponent<BackgroundManager>();
            }

            background.Initialize(config, true);
            Canvas canvas = UiFactory.CreateCanvas("MenuCanvas");
            new GameObject("MenuController").AddComponent<MenuController>().Build(canvas);
        }

        private void BuildGame()
        {
            GameSessionResult.Clear();
            SetupCamera();
            EnsureEventSystem();
            Physics2D.gravity = new Vector2(0f, -config.gravity);

            int platformLayer = LayerMask.NameToLayer("Platform");
            if (platformLayer < 0)
            {
                platformLayer = 8;
            }

            BackgroundManager background = FindFirstObjectByType<BackgroundManager>();
            if (background == null)
            {
                background = new GameObject("BackgroundManager").AddComponent<BackgroundManager>();
            }

            background.Initialize(config, false);

            Canvas canvas = UiFactory.CreateCanvas("HudCanvas");
            UIManager ui = new GameObject("UIManager").AddComponent<UIManager>();
            ui.BuildHud(canvas);

            InputManager input = new GameObject("InputManager").AddComponent<InputManager>();

            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player == null)
            {
                GameObject playerObject = new GameObject("Player");
                playerObject.transform.position = new Vector3(config.playerStartScreenPosition.x, config.WorldYFromScreenY(config.playerStartScreenPosition.y), 0f);
                SpriteRenderer renderer = playerObject.AddComponent<SpriteRenderer>();
                renderer.sortingOrder = 10;
                playerObject.AddComponent<Rigidbody2D>();
                playerObject.AddComponent<BoxCollider2D>();
                playerObject.AddComponent<PlayerAnimationController>();
                player = playerObject.AddComponent<PlayerController>();
            }

            ScoreManager score = FindFirstObjectByType<ScoreManager>();
            if (score == null)
            {
                score = new GameObject("ScoreManager").AddComponent<ScoreManager>();
            }

            GameManager game = FindFirstObjectByType<GameManager>();
            if (game == null)
            {
                game = new GameObject("GameManager").AddComponent<GameManager>();
            }

            PlatformSpawner spawner = FindFirstObjectByType<PlatformSpawner>();
            if (spawner == null)
            {
                spawner = new GameObject("PlatformSpawner").AddComponent<PlatformSpawner>();
            }

            score.Initialize(config, ui);
            player.Initialize(config, input, 1 << platformLayer);
            spawner.Initialize(config, game, score, platformLayer);
            game.Initialize(config, player, score, ui, spawner, background);

            WireSystemStub wire = new GameObject("WireSystem_Disabled").AddComponent<WireSystemStub>();
            wire.Initialize(config.wireEnabled);
            ObstacleSystemStub obstacles = new GameObject("ObstacleSystem_Disabled").AddComponent<ObstacleSystemStub>();
            obstacles.Initialize(config.obstacleEnabled);
        }

        private void BuildGameOver()
        {
            SetupCamera();
            EnsureEventSystem();
            BackgroundManager background = FindFirstObjectByType<BackgroundManager>();
            if (background == null)
            {
                background = new GameObject("BackgroundManager").AddComponent<BackgroundManager>();
            }

            background.Initialize(config, true);
            Canvas canvas = UiFactory.CreateCanvas("GameOverCanvas");
            new GameObject("GameOverController").AddComponent<GameOverController>().Build(canvas);
        }

        private void SetupCamera()
        {
            Camera existing = Camera.main;
            if (existing == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                existing = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
            }

            existing.orthographic = true;
            existing.orthographicSize = config.logicalSize.y * 0.5f;
            existing.transform.position = new Vector3(config.logicalSize.x * 0.5f, config.logicalSize.y * 0.5f, -10f);
            existing.clearFlags = CameraClearFlags.SolidColor;
            existing.backgroundColor = new Color(0.02f, 0.08f, 0.16f);
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            UnityEngine.InputSystem.UI.InputSystemUIInputModule module = eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            UnityEngine.InputSystem.InputActionAsset actions = Resources.Load<UnityEngine.InputSystem.InputActionAsset>("InputSystem_Actions");
            if (actions != null)
            {
                module.actionsAsset = actions;
            }
#else
            eventSystem.AddComponent<StandaloneInputModule>();
#endif
        }

#if UNITY_EDITOR
        public void SetEditorValues(SceneRole role, GameConfig gameConfig)
        {
            sceneRole = role;
            config = gameConfig;
        }
#endif
    }
}
