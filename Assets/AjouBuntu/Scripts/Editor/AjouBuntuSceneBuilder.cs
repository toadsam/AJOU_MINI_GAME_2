#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using AjouBuntu.Config;
using AjouBuntu.Core;
using AjouBuntu.Player;
using AjouBuntu.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AjouBuntu.Editor
{
    public static class AjouBuntuSceneBuilder
    {
        private const string Root = "Assets/AjouBuntu";
        private const string ResourcesPath = Root + "/Resources";
        private const string PrefabsPath = Root + "/Prefabs";
        private const string EditorSpritesPath = Root + "/EditorSprites";
        private const string ScenesPath = "Assets/Scenes";
        private const string ConfigPath = ResourcesPath + "/GameConfig.asset";

        [InitializeOnLoadMethod]
        private static void AutoBuildIfMissing()
        {
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    return;
                }

                if (!File.Exists($"{ScenesPath}/Boot.unity") ||
                    !File.Exists($"{ScenesPath}/Menu.unity") ||
                    !File.Exists($"{ScenesPath}/Game.unity") ||
                    !File.Exists($"{ScenesPath}/GameOver.unity") ||
                    AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigPath) == null)
                {
                    BuildAllScenes();
                }
            };
        }

        [MenuItem("AjouBuntu/Build Scenes And Config")]
        public static void BuildAllScenes()
        {
            Directory.CreateDirectory(ResourcesPath);
            Directory.CreateDirectory(PrefabsPath);
            Directory.CreateDirectory(EditorSpritesPath);
            Directory.CreateDirectory(ScenesPath);
            EnsurePlatformLayer();
            GameConfig config = BuildConfig();
            BuildPreviewSprites();
            BuildPrefabs(config);

            CreateScene("Boot", SceneRole.Boot, config);
            CreateScene("Menu", SceneRole.Menu, config);
            CreateScene("Game", SceneRole.Game, config);
            CreateScene("GameOver", SceneRole.GameOver, config);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene($"{ScenesPath}/Boot.unity", true),
                new EditorBuildSettingsScene($"{ScenesPath}/Menu.unity", true),
                new EditorBuildSettingsScene($"{ScenesPath}/Game.unity", true),
                new EditorBuildSettingsScene($"{ScenesPath}/GameOver.unity", true)
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("AjouBuntu scenes, config, and build settings generated.");
        }

        private static GameConfig BuildConfig()
        {
            GameConfig config = AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<GameConfig>();
                AssetDatabase.CreateAsset(config, ConfigPath);
            }

            config.EnsureDefaults();
            config.bgCampusDay = null;
            config.bgCampusSunset = null;
            config.bgCampusNight = null;
            config.playerSpriteSheet = null;

            AssignPlatformSprite(config, PlatformKind.StoneBridge, null);
            AssignPlatformSprite(config, PlatformKind.Stairs, null);
            AssignPlatformSprite(config, PlatformKind.Rooftop, null);
            AssignPlatformSprite(config, PlatformKind.LibraryShelf, null);
            AssignPlatformSprite(config, PlatformKind.FestivalBooth, null);
            AssignPlatformSprite(config, PlatformKind.BusStop, null);

            AssignItemSprite(config, ItemKind.APlus, null);
            AssignItemSprite(config, ItemKind.IdCard, null);
            AssignItemSprite(config, ItemKind.Coffee, null);
            AssignItemSprite(config, ItemKind.Attendance, null);
            AssignItemSprite(config, ItemKind.MealTicket, null);
            AssignItemSprite(config, ItemKind.Coupon, null);
            AssignItemSprite(config, ItemKind.Sticker, null);

            EditorUtility.SetDirty(config);
            return config;
        }

        private static void AssignPlatformSprite(GameConfig config, PlatformKind kind, string fileName)
        {
            PlatformDefinition definition = config.GetPlatform(kind);
            definition.sprite = string.IsNullOrEmpty(fileName) ? null : FindSprite(fileName);
        }

        private static void AssignItemSprite(GameConfig config, ItemKind kind, string fileName)
        {
            ItemDefinition definition = config.GetItem(kind);
            definition.sprite = string.IsNullOrEmpty(fileName) ? null : FindSprite(fileName);
        }

        private static void CreateScene(string sceneName, SceneRole role, GameConfig config)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject bootstrap = new GameObject("RuntimeBootstrap");
            RuntimeBootstrap runtimeBootstrap = bootstrap.AddComponent<RuntimeBootstrap>();
            runtimeBootstrap.SetEditorValues(role, config);

            if (role == SceneRole.Game)
            {
                BuildVisibleGameScene(config);
            }
            else if (role == SceneRole.Menu || role == SceneRole.GameOver)
            {
                BuildPreviewCamera(config);
                BuildBackgroundPreview(config);
            }

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/{sceneName}.unity");
        }

        private static void BuildVisibleGameScene(GameConfig config)
        {
            BuildPreviewCamera(config);
            BuildBackgroundPreview(config);

            new GameObject("GameManager").AddComponent<GameManager>();
            new GameObject("ScoreManager").AddComponent<ScoreManager>();

            GameObject spawner = new GameObject("PlatformSpawner_VisibleEditable");
            spawner.layer = LayerMask.NameToLayer("Platform");
            spawner.AddComponent<PlatformSpawner>();

            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/Player.prefab");
            GameObject player = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
            player.name = "Player_VisibleEditable";
            player.transform.position = new Vector3(config.playerStartScreenPosition.x, config.WorldYFromScreenY(config.playerStartScreenPosition.y), 0f);

            float[,] platforms =
            {
                { 260f, 520f, config.playerStartScreenPosition.y + 1f },
                { 760f, 330f, 420f },
                { 1180f, 340f, 390f },
                { 1600f, 320f, 430f },
                { 2020f, 360f, 405f }
            };

            for (int i = 0; i < platforms.GetLength(0); i++)
            {
                GameObject platform = CreatePlatformInstance(config, spawner.transform, i, platforms[i, 0], platforms[i, 1], platforms[i, 2]);
                if (i > 0)
                {
                    CreateCoffeeLine(config, spawner.transform, platform, platforms[i, 0], platforms[i, 1], platforms[i, 2]);
                }
            }
        }

        private static void BuildPreviewCamera(GameConfig config)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = config.logicalSize.y * 0.5f;
            camera.transform.position = new Vector3(config.logicalSize.x * 0.5f, config.logicalSize.y * 0.5f, -10f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.02f, 0.08f, 0.16f);
        }

        private static void BuildBackgroundPreview(GameConfig config)
        {
            GameObject background = new GameObject("BackgroundManager_VisiblePreview");
            background.AddComponent<BackgroundManager>();

            Sprite sky = LoadPreviewSprite("preview_sky");
            GameObject skyObject = new GameObject("SkyGradientPreview");
            skyObject.transform.SetParent(background.transform, false);
            skyObject.transform.position = new Vector3(config.logicalSize.x * 0.5f, config.logicalSize.y * 0.5f, 10f);
            SpriteRenderer skyRenderer = skyObject.AddComponent<SpriteRenderer>();
            skyRenderer.sprite = sky;
            skyRenderer.sortingOrder = -60;

            Sprite buildingSprite = LoadPreviewSprite("preview_building");
            for (int i = 0; i < 10; i++)
            {
                GameObject building = new GameObject($"CampusBuildingPreview_{i}");
                building.transform.SetParent(background.transform, false);
                float width = 80f + (i % 3) * 32f;
                float height = 85f + (i % 4) * 28f;
                building.transform.position = new Vector3(50f + i * 110f, height * 0.5f, 0f);
                building.transform.localScale = new Vector3(width / 32f, height / 32f, 1f);
                SpriteRenderer renderer = building.AddComponent<SpriteRenderer>();
                renderer.sprite = buildingSprite;
                renderer.sortingOrder = -30;
            }
        }

        private static GameObject CreatePlatformInstance(GameConfig config, Transform parent, int index, float centerX, float width, float topScreenY)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/Platform_StoneBridge.prefab");
            GameObject platform = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            platform.name = index == 0 ? "StartPlatform_Editable" : $"PlatformPreview_{index}";
            platform.transform.SetParent(parent, false);
            platform.layer = LayerMask.NameToLayer("Platform");
            platform.transform.position = new Vector3(centerX, config.WorldYFromScreenY(topScreenY), 0f);

            PlatformController controller = platform.GetComponent<PlatformController>();
            controller.SetEditorValues(PlatformKind.StoneBridge, width, topScreenY);
            controller.Initialize(config.GetPlatform(PlatformKind.StoneBridge), null, width, config.WorldYFromScreenY(topScreenY), config.despawnX);
            return platform;
        }

        private static void CreateCoffeeLine(GameConfig config, Transform parent, GameObject platform, float centerX, float width, float topScreenY)
        {
            for (int i = 0; i < 4; i++)
            {
                float x = centerX - width * 0.32f + i * 42f;
                float y = config.WorldYFromScreenY(topScreenY) + 46f;
                CreateItemInstance(config, parent, ItemKind.Coffee, new Vector3(x, y, 0f));
            }
        }

        private static void CreateItemInstance(GameConfig config, Transform parent, ItemKind kind, Vector3 position)
        {
            string prefabName = kind == ItemKind.APlus ? "Item_APlus.prefab" : "Item_Coffee.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/{prefabName}");
            GameObject item = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            item.name = $"Item_{kind}_Editable";
            item.transform.SetParent(parent, false);
            item.transform.position = position;
            ItemController controller = item.GetComponent<ItemController>();
            controller.SetEditorKind(kind);
        }

        private static void BuildPrefabs(GameConfig config)
        {
            SavePlayerPrefab(config);
            SavePlatformPrefab(config);
            SaveItemPrefab("Item_Coffee", ItemKind.Coffee, LoadPreviewSprite("preview_coffee"));
            SaveItemPrefab("Item_APlus", ItemKind.APlus, LoadPreviewSprite("preview_aplus"));
        }

        private static void SavePlayerPrefab(GameConfig config)
        {
            GameObject player = new GameObject("Player");
            SpriteRenderer renderer = player.AddComponent<SpriteRenderer>();
            renderer.sprite = LoadPreviewSprite("preview_player");
            renderer.sortingOrder = 10;

            Rigidbody2D body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 1f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            BoxCollider2D collider = player.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(42f, 78f);
            collider.offset = new Vector2(0f, 38f);

            player.AddComponent<PlayerAnimationController>();
            player.AddComponent<PlayerController>();
            PrefabUtility.SaveAsPrefabAsset(player, $"{PrefabsPath}/Player.prefab");
            Object.DestroyImmediate(player);
        }

        private static void SavePlatformPrefab(GameConfig config)
        {
            GameObject platform = new GameObject("Platform_StoneBridge");
            platform.layer = LayerMask.NameToLayer("Platform");
            SpriteRenderer renderer = platform.AddComponent<SpriteRenderer>();
            renderer.sprite = config.GetPlatform(PlatformKind.StoneBridge).sprite != null
                ? config.GetPlatform(PlatformKind.StoneBridge).sprite
                : LoadPreviewSprite("preview_platform");
            renderer.sortingOrder = 5;
            platform.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            platform.AddComponent<PlatformController>().SetEditorValues(PlatformKind.StoneBridge, 520f, 403f);

            GameObject top = new GameObject("OneWayTopCollider");
            top.layer = platform.layer;
            top.transform.SetParent(platform.transform, false);
            BoxCollider2D collider = top.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(496f, 16f);
            collider.usedByEffector = true;
            PlatformEffector2D effector = top.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.surfaceArc = 170f;

            PrefabUtility.SaveAsPrefabAsset(platform, $"{PrefabsPath}/Platform_StoneBridge.prefab");
            Object.DestroyImmediate(platform);
        }

        private static void SaveItemPrefab(string name, ItemKind kind, Sprite sprite)
        {
            GameObject item = new GameObject(name);
            SpriteRenderer renderer = item.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 8;
            CircleCollider2D collider = item.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 18f;
            item.AddComponent<ItemController>().SetEditorKind(kind);
            PrefabUtility.SaveAsPrefabAsset(item, $"{PrefabsPath}/{name}.prefab");
            Object.DestroyImmediate(item);
        }

        private static void BuildPreviewSprites()
        {
            SaveSpritePng("preview_sky", 960, 540, (x, y) => Color.Lerp(new Color(0.14f, 0.43f, 0.84f), new Color(0.68f, 0.94f, 1f), y / 539f));
            SaveSpritePng("preview_building", 32, 32, (x, y) => new Color(0.03f, 0.15f, 0.25f, 0.45f));
            SaveSpritePng("preview_platform", 256, 64, (x, y) => y > 46 ? new Color(0.64f, 0.82f, 0.92f) : new Color(0.25f, 0.43f, 0.55f));
            SaveSpritePng("preview_player", 72, 100, CreatePlayerPixel);
            SaveSpritePng("preview_coffee", 48, 48, CreateCoffeePixel);
            SaveSpritePng("preview_aplus", 48, 48, (x, y) => CirclePixel(x, y, 48, new Color(1f, 0.88f, 0.22f), new Color(1f, 1f, 0.82f)));
        }

        private static Color CreatePlayerPixel(int x, int y)
        {
            float nx = (x - 36f) / 25f;
            float ny = (y - 50f) / 43f;
            float d = nx * nx + ny * ny;
            if (d > 1f)
            {
                return Color.clear;
            }

            return d > 0.82f ? new Color(0.02f, 0.11f, 0.28f) : new Color(0.18f, 0.78f, 1f);
        }

        private static Color CreateCoffeePixel(int x, int y)
        {
            return CirclePixel(x, y, 48, new Color(0.22f, 0.86f, 1f), Color.white);
        }

        private static Color CirclePixel(int x, int y, int size, Color fill, Color edge)
        {
            Vector2 p = new Vector2(x - size * 0.5f, y - size * 0.5f);
            float distance = p.magnitude;
            if (distance > size * 0.42f)
            {
                return Color.clear;
            }

            return distance > size * 0.34f ? edge : fill;
        }

        private delegate Color PixelFactory(int x, int y);

        private static void SaveSpritePng(string name, int width, int height, PixelFactory pixelFactory)
        {
            string path = $"{EditorSpritesPath}/{name}.png";
            if (!File.Exists(path))
            {
                Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        texture.SetPixel(x, y, pixelFactory(x, y));
                    }
                }

                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
            }

            AssetDatabase.ImportAsset(path);
            ConfigureTextureImporter(path, TextureImporterType.Sprite, 1f);
        }

        private static Sprite LoadPreviewSprite(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>($"{EditorSpritesPath}/{name}.png");
        }

        private static Sprite FindSprite(string fileName)
        {
            string path = FindAssetPath(fileName);
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            ConfigureTextureImporter(path, TextureImporterType.Sprite, 1f);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Texture2D FindTexture(string fileName)
        {
            string path = FindAssetPath(fileName);
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            ConfigureTextureImporter(path, TextureImporterType.Sprite, 1f);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static string FindAssetPath(string fileName)
        {
            string bareName = Path.GetFileNameWithoutExtension(fileName);
            string[] guids = AssetDatabase.FindAssets(bareName);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileName(path).Equals(fileName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return path;
                }
            }

            return null;
        }

        private static void ConfigureTextureImporter(string path, TextureImporterType type, float pixelsPerUnit)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            bool changed = importer.textureType != type || !Mathf.Approximately(importer.spritePixelsPerUnit, pixelsPerUnit);
            importer.textureType = type;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.alphaIsTransparency = true;
            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        private static void EnsurePlatformLayer()
        {
            Object tagManagerAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0];
            SerializedObject tagManager = new SerializedObject(tagManagerAsset);
            SerializedProperty layers = tagManager.FindProperty("layers");

            for (int i = 0; i < layers.arraySize; i++)
            {
                if (layers.GetArrayElementAtIndex(i).stringValue == "Platform")
                {
                    return;
                }
            }

            for (int i = 8; i < layers.arraySize; i++)
            {
                SerializedProperty layer = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(layer.stringValue))
                {
                    layer.stringValue = "Platform";
                    tagManager.ApplyModifiedProperties();
                    return;
                }
            }
        }
    }
}
#endif
