#if UNITY_EDITOR
using System;
using System.IO;
using AjouFestival.Core;
using AjouFestival.Games.AjouBoontu;
using AjouFestival.Games.BalanceWalk;
using AjouFestival.Games.Soccer;
using AjouFestival.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MiniGameProjectBuilder
{
    private const string ScriptsRoot = "Assets/Scripts";
    private const string PrefabsRoot = "Assets/Prefabs";
    private const string SpritesRoot = "Assets/Sprites";
    private const string ScenesRoot = "Assets/Scenes";

    [MenuItem("Tools/Ajou Festival/Create MiniGame Project Structure")]
    public static void CreateProjectStructure()
    {
        string[] folders =
        {
            "Assets/Scripts/Core", "Assets/Scripts/UI",
            "Assets/Scripts/Games/AjouBoontu", "Assets/Scripts/Games/BalanceWalk", "Assets/Scripts/Games/Soccer",
            "Assets/Prefabs/Core", "Assets/Prefabs/UI",
            "Assets/Prefabs/Games/AjouBoontu", "Assets/Prefabs/Games/BalanceWalk", "Assets/Prefabs/Games/Soccer",
            "Assets/Sprites/Placeholder", "Assets/Sprites/Chito", "Assets/Sprites/Backgrounds",
            "Assets/Sprites/Platforms", "Assets/Sprites/Items", "Assets/Sprites/Obstacles", "Assets/Sprites/UI"
        };

        foreach (string folder in folders)
        {
            Directory.CreateDirectory(folder);
        }

        AssetDatabase.Refresh();
        Debug.Log("Ajou Festival project structure created.");
    }

    [MenuItem("Tools/Ajou Festival/Create Placeholder Prefabs")]
    public static void CreatePlaceholderPrefabs()
    {
        CreateProjectStructure();
        CreatePlaceholderSprites();
        CreateCorePrefabs();
        CreateUIPrefabs();
        CreateAjouBoontuPrefabs();
        CreateBalancePrefabs();
        CreateSoccerPrefabs();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Ajou Festival placeholder prefabs created.");
    }

    [MenuItem("Tools/Ajou Festival/Create Basic Scenes")]
    public static void CreateBasicScenes()
    {
        CreatePlaceholderPrefabs();
        CreateMainMenuScene();
        CreateGameSelectScene();
        CreateAjouBoontuScene();
        CreateBalanceWalkScene();
        CreateSoccerScene();
        CreateResultScene();
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene($"{ScenesRoot}/00_MainMenu.unity", true),
            new EditorBuildSettingsScene($"{ScenesRoot}/01_GameSelect.unity", true),
            new EditorBuildSettingsScene($"{ScenesRoot}/02_AjouBoontu.unity", true),
            new EditorBuildSettingsScene($"{ScenesRoot}/03_BalanceWalk.unity", true),
            new EditorBuildSettingsScene($"{ScenesRoot}/04_OneVsOneSoccer.unity", true),
            new EditorBuildSettingsScene($"{ScenesRoot}/05_Result.unity", true)
        };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Ajou Festival scenes created.");
    }

    private static void CreateCorePrefabs()
    {
        GameObject session = new GameObject("GameSessionManager");
        session.AddComponent<GameSessionManager>();
        SavePrefab(session, "Assets/Prefabs/Core/GameSessionManager.prefab");

        GameObject audio = new GameObject("AudioManager");
        audio.AddComponent<AudioManager>();
        SavePrefab(audio, "Assets/Prefabs/Core/AudioManager.prefab");
    }

    private static void CreateUIPrefabs()
    {
        SavePrefab(CreateMainMenuCanvas(), "Assets/Prefabs/UI/Canvas_MainMenu.prefab");
        SavePrefab(CreateGameSelectCanvas(), "Assets/Prefabs/UI/Canvas_GameSelect.prefab");
        SavePrefab(CreateResultCanvas(), "Assets/Prefabs/UI/Canvas_Result.prefab");
    }

    private static void CreateAjouBoontuPrefabs()
    {
        Sprite chito = LoadSprite("Assets/Sprites/Chito/placeholder_chito.png");
        Sprite platformSprite = LoadSprite("Assets/Sprites/Platforms/placeholder_platform.png");
        Sprite itemSprite = LoadSprite("Assets/Sprites/Items/placeholder_aplus.png");
        Sprite obstacleSprite = LoadSprite("Assets/Sprites/Obstacles/placeholder_obstacle.png");

        GameObject runner = new GameObject("ChitoRunner");
        runner.transform.position = new Vector3(-5f, -1f, 0f);
        SpriteRenderer runnerRenderer = runner.AddComponent<SpriteRenderer>();
        runnerRenderer.sprite = chito;
        runnerRenderer.sortingOrder = 10;
        Rigidbody2D runnerBody = runner.AddComponent<Rigidbody2D>();
        runnerBody.gravityScale = 2.5f;
        runnerBody.freezeRotation = true;
        BoxCollider2D runnerCollider = runner.AddComponent<BoxCollider2D>();
        runnerCollider.size = new Vector2(0.8f, 1.2f);
        runner.AddComponent<ChitoRunnerController>();
        runner.AddComponent<WireActionController>();
        SavePrefab(runner, "Assets/Prefabs/Games/AjouBoontu/ChitoRunner.prefab");

        GameObject platform = new GameObject("Platform_Default");
        SpriteRenderer platformRenderer = platform.AddComponent<SpriteRenderer>();
        platformRenderer.sprite = platformSprite;
        platformRenderer.sortingOrder = 1;
        BoxCollider2D platformCollider = platform.AddComponent<BoxCollider2D>();
        platformCollider.size = new Vector2(4f, 0.55f);
        platform.AddComponent<RunnerPlatform>();
        SavePrefab(platform, "Assets/Prefabs/Games/AjouBoontu/Platform_Default.prefab");

        GameObject item = new GameObject("Item_APlus");
        SpriteRenderer itemRenderer = item.AddComponent<SpriteRenderer>();
        itemRenderer.sprite = itemSprite;
        itemRenderer.sortingOrder = 7;
        CircleCollider2D itemCollider = item.AddComponent<CircleCollider2D>();
        itemCollider.isTrigger = true;
        itemCollider.radius = 0.35f;
        item.AddComponent<RunnerItem>();
        SavePrefab(item, "Assets/Prefabs/Games/AjouBoontu/Item_APlus.prefab");

        GameObject obstacle = new GameObject("Obstacle_Default");
        SpriteRenderer obstacleRenderer = obstacle.AddComponent<SpriteRenderer>();
        obstacleRenderer.sprite = obstacleSprite;
        obstacleRenderer.sortingOrder = 6;
        BoxCollider2D obstacleCollider = obstacle.AddComponent<BoxCollider2D>();
        obstacleCollider.isTrigger = true;
        obstacleCollider.size = new Vector2(0.75f, 0.75f);
        obstacle.AddComponent<RunnerObstacle>();
        SavePrefab(obstacle, "Assets/Prefabs/Games/AjouBoontu/Obstacle_Default.prefab");
    }

    private static void CreateBalancePrefabs()
    {
        Sprite chito = LoadSprite("Assets/Sprites/Chito/placeholder_balance_chito.png");
        Sprite groundSprite = LoadSprite("Assets/Sprites/Placeholder/placeholder_ground.png");

        GameObject player = new GameObject("BalancePlayer");
        SpriteRenderer renderer = player.AddComponent<SpriteRenderer>();
        renderer.sprite = chito;
        renderer.sortingOrder = 5;
        Rigidbody2D body = player.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        CapsuleCollider2D collider = player.AddComponent<CapsuleCollider2D>();
        collider.size = new Vector2(0.8f, 3.2f);
        player.AddComponent<BalancePlayerController>();
        BalancePlayerController controller = player.GetComponent<BalancePlayerController>();
        SerializedObject serialized = new SerializedObject(controller);
        serialized.FindProperty("moveSpeed").floatValue = 4.2f;
        serialized.FindProperty("speedIncreaseRate").floatValue = 0.035f;
        serialized.FindProperty("useAutoMove").boolValue = true;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        SavePrefab(player, "Assets/Prefabs/Games/BalanceWalk/BalancePlayer.prefab");

        GameObject ground = new GameObject("BalanceGround");
        SpriteRenderer groundRenderer = ground.AddComponent<SpriteRenderer>();
        groundRenderer.sprite = groundSprite;
        groundRenderer.sortingOrder = 0;
        ground.transform.localScale = new Vector3(60f, 0.7f, 1f);
        ground.AddComponent<BoxCollider2D>();
        ground.AddComponent<BalanceGroundLoop>();
        SavePrefab(ground, "Assets/Prefabs/Games/BalanceWalk/BalanceGround.prefab");

        GameObject meter = new GameObject("BalanceMeter");
        meter.AddComponent<BalanceMeterUI>();
        SavePrefab(meter, "Assets/Prefabs/Games/BalanceWalk/BalanceMeter.prefab");
    }

    private static void CreateSoccerPrefabs()
    {
        Sprite p1Sprite = LoadSprite("Assets/Sprites/Placeholder/placeholder_p1.png");
        Sprite p2Sprite = LoadSprite("Assets/Sprites/Placeholder/placeholder_p2.png");
        Sprite ballSprite = LoadSprite("Assets/Sprites/Placeholder/placeholder_ball.png");
        Sprite goalSprite = LoadSprite("Assets/Sprites/Placeholder/placeholder_goal.png");
        Sprite fieldSprite = LoadSprite("Assets/Sprites/Backgrounds/placeholder_soccer_field.png");

        SaveSoccerPlayerPrefab("SoccerPlayer1", 1, p1Sprite, "Assets/Prefabs/Games/Soccer/SoccerPlayer1.prefab");
        SaveSoccerPlayerPrefab("SoccerPlayer2", 2, p2Sprite, "Assets/Prefabs/Games/Soccer/SoccerPlayer2.prefab");

        GameObject ball = new GameObject("SoccerBall");
        SpriteRenderer ballRenderer = ball.AddComponent<SpriteRenderer>();
        ballRenderer.sprite = ballSprite;
        ballRenderer.sortingOrder = 7;
        Rigidbody2D ballBody = ball.AddComponent<Rigidbody2D>();
        ballBody.gravityScale = 0f;
        ballBody.linearDamping = 1.2f;
        CircleCollider2D ballCollider = ball.AddComponent<CircleCollider2D>();
        ballCollider.radius = 0.35f;
        ball.AddComponent<SoccerBallController>();
        SavePrefab(ball, "Assets/Prefabs/Games/Soccer/SoccerBall.prefab");

        SaveGoalPrefab("SoccerGoalLeft", 2, goalSprite, "Assets/Prefabs/Games/Soccer/SoccerGoalLeft.prefab");
        SaveGoalPrefab("SoccerGoalRight", 1, goalSprite, "Assets/Prefabs/Games/Soccer/SoccerGoalRight.prefab");

        GameObject field = new GameObject("SoccerField");
        SpriteRenderer fieldRenderer = field.AddComponent<SpriteRenderer>();
        fieldRenderer.sprite = fieldSprite;
        fieldRenderer.sortingOrder = -10;
        field.transform.localScale = new Vector3(14f, 8f, 1f);
        SavePrefab(field, "Assets/Prefabs/Games/Soccer/SoccerField.prefab");
    }

    private static void SaveSoccerPlayerPrefab(string name, int index, Sprite sprite, string path)
    {
        GameObject player = new GameObject(name);
        SpriteRenderer renderer = player.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 6;
        Rigidbody2D body = player.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        CircleCollider2D collider = player.AddComponent<CircleCollider2D>();
        collider.radius = 0.48f;
        SoccerPlayerController controller = player.AddComponent<SoccerPlayerController>();
        SerializedObject serialized = new SerializedObject(controller);
        serialized.FindProperty("playerIndex").intValue = index;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        SavePrefab(player, path);
    }

    private static void SaveGoalPrefab(string name, int scoringPlayer, Sprite sprite, string path)
    {
        GameObject goal = new GameObject(name);
        SpriteRenderer renderer = goal.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 3;
        BoxCollider2D collider = goal.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.7f, 3f);
        SoccerGoal soccerGoal = goal.AddComponent<SoccerGoal>();
        SerializedObject serialized = new SerializedObject(soccerGoal);
        serialized.FindProperty("scoringPlayer").intValue = scoringPlayer;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        SavePrefab(goal, path);
    }

    private static void CreateMainMenuScene()
    {
        NewSceneWithBaseObjects("00_MainMenu", false);
        InstantiatePrefab("Assets/Prefabs/UI/Canvas_MainMenu.prefab");
        SaveScene("00_MainMenu");
    }

    private static void CreateGameSelectScene()
    {
        NewSceneWithBaseObjects("01_GameSelect", false);
        InstantiatePrefab("Assets/Prefabs/UI/Canvas_GameSelect.prefab");
        SaveScene("01_GameSelect");
    }

    private static void CreateResultScene()
    {
        NewSceneWithBaseObjects("05_Result", false);
        InstantiatePrefab("Assets/Prefabs/UI/Canvas_Result.prefab");
        SaveScene("05_Result");
    }

    private static void CreateAjouBoontuScene()
    {
        NewSceneWithBaseObjects("02_AjouBoontu", true);
        Camera.main.gameObject.AddComponent<RunnerCameraController>();
        Camera.main.transform.position = new Vector3(0f, 1f, -10f);

        GameObject manager = new GameObject("RunnerGameManager");
        manager.AddComponent<AjouBoontuGameManager>();
        manager.AddComponent<GlobalShortcutHandler>();

        GameObject runner = InstantiatePrefab("Assets/Prefabs/Games/AjouBoontu/ChitoRunner.prefab");
        runner.transform.position = new Vector3(-5f, -1.4f, 0f);

        GameObject start = InstantiatePrefab("Assets/Prefabs/Games/AjouBoontu/Platform_Default.prefab");
        start.name = "StartPlatform_Editable";
        start.transform.position = new Vector3(-4f, -2.5f, 0f);
        start.transform.localScale = new Vector3(2.4f, 1f, 1f);

        GameObject platformSpawner = new GameObject("PlatformSpawner");
        RunnerPlatformSpawner rps = platformSpawner.AddComponent<RunnerPlatformSpawner>();
        AddPrefabToList(rps, "platformPrefabs", LoadPrefab("Assets/Prefabs/Games/AjouBoontu/Platform_Default.prefab"));

        GameObject itemSpawner = new GameObject("ItemSpawner");
        RunnerItemSpawner ris = itemSpawner.AddComponent<RunnerItemSpawner>();
        AddPrefabToList(ris, "itemPrefabs", LoadPrefab("Assets/Prefabs/Games/AjouBoontu/Item_APlus.prefab"));

        GameObject obstacleSpawner = new GameObject("ObstacleSpawner");
        RunnerObstacleSpawner ros = obstacleSpawner.AddComponent<RunnerObstacleSpawner>();
        AddPrefabToList(ros, "obstaclePrefabs", LoadPrefab("Assets/Prefabs/Games/AjouBoontu/Obstacle_Default.prefab"));

        Canvas canvas = CreateBaseCanvas("Canvas_RunnerUI");
        canvas.gameObject.AddComponent<RunnerUI>();
        CreateTopText(canvas.transform, "ScoreText", "점수 0", new Vector2(190f, -40f), TextAnchor.MiddleLeft);
        CreateTopText(canvas.transform, "BestScoreText", "최고 0", new Vector2(-190f, -40f), TextAnchor.MiddleRight);
        CreateBottomText(canvas.transform, "HintText", "Space/클릭: 점프   길게 누르기: 와이어", new Vector2(0f, 42f));
        CreateButton(canvas.transform, "ExitButton", "선택으로", new Vector2(0f, -40f), new Vector2(160f, 46f), null);

        SaveScene("02_AjouBoontu");
    }

    private static void CreateBalanceWalkScene()
    {
        NewSceneWithBaseObjects("03_BalanceWalk", true);
        Camera.main.transform.position = new Vector3(0f, 0.5f, -10f);
        Camera.main.gameObject.AddComponent<BalanceCameraController>();

        GameObject manager = new GameObject("BalanceGameManager");
        manager.AddComponent<BalanceWalkGameManager>();
        manager.AddComponent<GlobalShortcutHandler>();

        for (int i = 0; i < 3; i++)
        {
            GameObject ground = InstantiatePrefab("Assets/Prefabs/Games/BalanceWalk/BalanceGround.prefab");
            ground.name = $"BalanceGround_{i + 1}";
            ground.transform.position = new Vector3(i * 60f, -2.6f, 0f);
        }

        GameObject player = InstantiatePrefab("Assets/Prefabs/Games/BalanceWalk/BalancePlayer.prefab");
        player.transform.position = new Vector3(0f, -0.8f, 0f);

        CreateBalanceMotionCues();

        Canvas canvas = CreateBaseCanvas("Canvas_BalanceUI");
        canvas.gameObject.AddComponent<BalanceUI>();
        CreateTopText(canvas.transform, "TimeText", "이동 거리 0.0 m", new Vector2(210f, -40f), TextAnchor.MiddleLeft);
        CreateTopText(canvas.transform, "BestText", "최고 거리 0.0 m", new Vector2(-210f, -40f), TextAnchor.MiddleRight);
        CreateText(canvas.transform, "CountdownText", "3", 82, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.58f), new Vector2(360f, 110f), Vector2.zero);
        CreateBottomText(canvas.transform, "HintText", "A/D 또는 ←/→로 균형 잡기", new Vector2(0f, 42f));
        CreateButton(canvas.transform, "ExitButton", "선택으로", new Vector2(0f, -40f), new Vector2(160f, 46f), null);
        CreateBalanceMeter(canvas.transform);

        SaveScene("03_BalanceWalk");
    }

    private static void CreateBalanceMotionCues()
    {
        Sprite laneSprite = LoadSprite("Assets/Sprites/Placeholder/placeholder_lane_mark.png");
        Sprite postSprite = LoadSprite("Assets/Sprites/Placeholder/placeholder_distance_post.png");
        Sprite campusSprite = LoadSprite("Assets/Sprites/Backgrounds/placeholder_campus_block.png");

        for (int i = 0; i < 8; i++)
        {
            CreateBalanceBackgroundBlock(i, campusSprite);
        }

        for (int i = 0; i < 24; i++)
        {
            CreateBalanceDistanceCue(i, laneSprite, postSprite);
        }
    }

    private static void CreateBalanceBackgroundBlock(int index, Sprite sprite)
    {
        GameObject block = new GameObject($"CampusParallax_{index + 1:00}");
        block.transform.position = new Vector3(-12f + index * 24f, 0.25f, 0f);
        block.transform.localScale = new Vector3(8f, 3f, 1f);
        SpriteRenderer renderer = block.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = new Color(0.58f, 0.78f, 1f, 0.35f);
        renderer.sortingOrder = -12;
        BalanceParallaxLoop loop = block.AddComponent<BalanceParallaxLoop>();
        SerializedObject serialized = new SerializedObject(loop);
        serialized.FindProperty("parallaxFactor").floatValue = 0.22f;
        serialized.FindProperty("tileWidth").floatValue = 24f;
        serialized.FindProperty("tileCount").intValue = 8;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateBalanceDistanceCue(int index, Sprite laneSprite, Sprite postSprite)
    {
        GameObject cue = new GameObject($"DistanceCue_{index + 1:00}");
        cue.transform.position = new Vector3(5f + index * 8f, 0f, 0f);

        GameObject lane = new GameObject("LaneDash");
        lane.transform.SetParent(cue.transform);
        lane.transform.localPosition = new Vector3(0f, -2.17f, 0f);
        lane.transform.localScale = new Vector3(2.8f, 1.2f, 1f);
        SpriteRenderer laneRenderer = lane.AddComponent<SpriteRenderer>();
        laneRenderer.sprite = laneSprite;
        laneRenderer.sortingOrder = 2;

        GameObject post = new GameObject("DistancePost");
        post.transform.SetParent(cue.transform);
        post.transform.localPosition = new Vector3(0f, -1.62f, 0f);
        post.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
        SpriteRenderer postRenderer = post.AddComponent<SpriteRenderer>();
        postRenderer.sprite = postSprite;
        postRenderer.sortingOrder = 3;

        GameObject labelObj = new GameObject("DistanceLabel");
        labelObj.transform.SetParent(cue.transform);
        labelObj.transform.localPosition = new Vector3(0f, -0.96f, 0f);
        TextMesh label = labelObj.AddComponent<TextMesh>();
        label.text = $"{Mathf.RoundToInt(cue.transform.position.x)}m";
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.characterSize = 0.28f;
        label.fontSize = 48;
        label.color = Color.white;
        MeshRenderer labelRenderer = labelObj.GetComponent<MeshRenderer>();
        labelRenderer.sortingOrder = 4;

        BalanceDistanceCueLoop loop = cue.AddComponent<BalanceDistanceCueLoop>();
        SerializedObject serialized = new SerializedObject(loop);
        serialized.FindProperty("distanceLabel").objectReferenceValue = label;
        serialized.FindProperty("spacing").floatValue = 8f;
        serialized.FindProperty("cueCount").intValue = 24;
        serialized.FindProperty("recycleBehind").floatValue = 16f;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateSoccerScene()
    {
        NewSceneWithBaseObjects("04_OneVsOneSoccer", true);
        Camera.main.transform.position = new Vector3(0f, 0f, -10f);

        GameObject manager = new GameObject("SoccerGameManager");
        manager.AddComponent<SoccerGameManager>();
        manager.AddComponent<GlobalShortcutHandler>();

        InstantiatePrefab("Assets/Prefabs/Games/Soccer/SoccerField.prefab");
        GameObject p1 = InstantiatePrefab("Assets/Prefabs/Games/Soccer/SoccerPlayer1.prefab");
        p1.transform.position = new Vector3(-4.5f, 0f, 0f);
        GameObject p2 = InstantiatePrefab("Assets/Prefabs/Games/Soccer/SoccerPlayer2.prefab");
        p2.transform.position = new Vector3(4.5f, 0f, 0f);
        InstantiatePrefab("Assets/Prefabs/Games/Soccer/SoccerBall.prefab");
        GameObject leftGoal = InstantiatePrefab("Assets/Prefabs/Games/Soccer/SoccerGoalLeft.prefab");
        leftGoal.transform.position = new Vector3(-7.2f, 0f, 0f);
        GameObject rightGoal = InstantiatePrefab("Assets/Prefabs/Games/Soccer/SoccerGoalRight.prefab");
        rightGoal.transform.position = new Vector3(7.2f, 0f, 0f);
        CreateSoccerWalls();

        Canvas canvas = CreateBaseCanvas("Canvas_SoccerUI");
        canvas.gameObject.AddComponent<SoccerUI>();
        CreateTopText(canvas.transform, "TimeText", "남은 시간 60", new Vector2(0f, -38f), TextAnchor.MiddleCenter);
        CreateTopText(canvas.transform, "ScoreText", "P1 0 : 0 P2", new Vector2(0f, -82f), TextAnchor.MiddleCenter);
        CreateBottomText(canvas.transform, "HintText", "P1: WASD+Space   P2: 방향키+Enter", new Vector2(0f, 42f));
        CreateButton(canvas.transform, "ExitButton", "선택으로", new Vector2(0f, -40f), new Vector2(160f, 46f), null);

        SaveScene("04_OneVsOneSoccer");
    }

    private static void NewSceneWithBaseObjects(string sceneName, bool worldScene)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject sessionPrefab = LoadPrefab("Assets/Prefabs/Core/GameSessionManager.prefab");
        if (sessionPrefab != null) PrefabUtility.InstantiatePrefab(sessionPrefab);
        GameObject audioPrefab = LoadPrefab("Assets/Prefabs/Core/AudioManager.prefab");
        if (audioPrefab != null) PrefabUtility.InstantiatePrefab(audioPrefab);

        GameObject loader = new GameObject("SceneLoader");
        loader.AddComponent<SceneLoader>();

        GameObject cameraObj = new GameObject("Main Camera");
        cameraObj.tag = "MainCamera";
        Camera camera = cameraObj.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = worldScene ? 5f : 540f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.08f, 0.24f, 0.52f);
        if (!worldScene)
        {
            camera.transform.position = new Vector3(480f, 270f, -10f);
        }

        CreateEventSystem();
        if (!worldScene)
        {
            CreateWorldSprite("MenuBackground", LoadSprite("Assets/Sprites/Backgrounds/placeholder_menu_bg.png"), new Vector3(480f, 270f, 2f), Vector3.one);
        }
    }

    private static GameObject CreateMainMenuCanvas()
    {
        Canvas canvas = CreateBaseCanvas("Canvas_MainMenu");
        canvas.gameObject.AddComponent<MainMenuUI>();
        CreateText(canvas.transform, "TitleText", "아주대학교 축제 미니게임", 54, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.69f), new Vector2(760f, 72f), Vector2.zero);
        CreateText(canvas.transform, "SubtitleText", "AU Festival Game Zone", 30, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.59f), new Vector2(680f, 52f), Vector2.zero);
        CreateButton(canvas.transform, "GameSelectButton", "게임 선택", new Vector2(0f, -45f), new Vector2(260f, 62f), null);
        CreateButton(canvas.transform, "HowToButton", "조작 방법", new Vector2(0f, -125f), new Vector2(260f, 62f), null);
        CreateButton(canvas.transform, "QuitButton", "종료", new Vector2(0f, -205f), new Vector2(260f, 62f), null);
        GameObject panel = CreatePanel(canvas.transform, "HowToPanel", new Vector2(0.5f, 0.5f), new Vector2(620f, 170f), new Vector2(0f, 96f), new Color(0.02f, 0.12f, 0.26f, 0.82f));
        CreateText(panel.transform, "HowToText", "ESC: 게임 선택으로 돌아가기\nR: 현재 게임 다시 시작\n각 게임 화면의 안내를 따라 플레이", 24, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(560f, 130f), Vector2.zero);
        panel.SetActive(false);
        return canvas.gameObject;
    }

    private static GameObject CreateGameSelectCanvas()
    {
        Canvas canvas = CreateBaseCanvas("Canvas_GameSelect");
        canvas.gameObject.AddComponent<GameSelectUI>();
        CreateText(canvas.transform, "TitleText", "게임 선택", 48, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.88f), new Vector2(420f, 64f), Vector2.zero);
        CreateGameCard(canvas.transform, "AjouBoontuCard", "아주분투", "A+를 향해 캠퍼스를 질주하라!", "Space / 마우스 클릭 / 길게 누르기", new Vector2(-310f, 40f), true);
        CreateGameCard(canvas.transform, "BalanceWalkCard", "치토 균형걷기", "넘어지지 않고 오래 버텨라!", "A/D 또는 ←/→", new Vector2(0f, 40f), true);
        CreateGameCard(canvas.transform, "SoccerCard", "아주 1대1 축구", "친구와 60초 승부!", "P1 A/D W Space / P2 Arrows Up Enter", new Vector2(310f, 40f), true);
        CreateGameCard(canvas.transform, "ComingSoonCard1", "Coming Soon", "다음 축제 게임 준비중", "-", new Vector2(-155f, -205f), false);
        CreateGameCard(canvas.transform, "ComingSoonCard2", "Coming Soon", "다음 축제 게임 준비중", "-", new Vector2(155f, -205f), false);
        CreateButton(canvas.transform, "MainMenuButton", "메인으로", new Vector2(0f, -255f), new Vector2(190f, 52f), null);
        return canvas.gameObject;
    }

    private static GameObject CreateResultCanvas()
    {
        Canvas canvas = CreateBaseCanvas("Canvas_Result");
        canvas.gameObject.AddComponent<ResultUI>();
        CreateText(canvas.transform, "GameNameText", "게임 이름", 48, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.74f), new Vector2(640f, 64f), Vector2.zero);
        CreateText(canvas.transform, "ResultMessageText", "결과 메시지", 30, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.62f), new Vector2(720f, 54f), Vector2.zero);
        CreateText(canvas.transform, "ScoreText", "최종 점수: 0", 32, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.50f), new Vector2(560f, 54f), Vector2.zero);
        CreateText(canvas.transform, "BestScoreText", "최고 점수: 0", 28, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.42f), new Vector2(560f, 48f), Vector2.zero);
        CreateButton(canvas.transform, "RetryButton", "다시하기", new Vector2(-220f, -130f), new Vector2(200f, 58f), null);
        CreateButton(canvas.transform, "GameSelectButton", "게임 선택으로", new Vector2(0f, -130f), new Vector2(220f, 58f), null);
        CreateButton(canvas.transform, "MainMenuButton", "메인으로", new Vector2(230f, -130f), new Vector2(200f, 58f), null);
        return canvas.gameObject;
    }

    private static void CreateGameCard(Transform parent, string name, string title, string desc, string control, Vector2 pos, bool active)
    {
        GameObject card = CreatePanel(parent, name, new Vector2(0.5f, 0.5f), new Vector2(285f, 190f), pos, active ? new Color(0.95f, 0.98f, 1f, 0.92f) : new Color(0.55f, 0.65f, 0.75f, 0.55f));
        CreateText(card.transform, "Title", title, 28, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.78f), new Vector2(245f, 40f), Vector2.zero, Color.black);
        CreateText(card.transform, "Description", desc, 18, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.55f), new Vector2(245f, 38f), Vector2.zero, Color.black);
        CreateText(card.transform, "Control", control, 16, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.36f), new Vector2(245f, 34f), Vector2.zero, new Color(0.05f, 0.22f, 0.44f));
        Button button = CreateButton(card.transform, "StartButton", active ? "시작" : "준비중", new Vector2(0f, -58f), new Vector2(140f, 44f), null);
        button.interactable = active;
    }

    private static Canvas CreateBaseCanvas(string name)
    {
        GameObject canvasObj = new GameObject(name);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<GraphicRaycaster>();
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(960f, 540f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    private static Text CreateText(Transform parent, string name, string text, int size, TextAnchor anchor, Vector2 anchorCenter, Vector2 rectSize, Vector2 pos)
    {
        return CreateText(parent, name, text, size, anchor, anchorCenter, rectSize, pos, Color.white);
    }

    private static Text CreateText(Transform parent, string name, string text, int size, TextAnchor anchor, Vector2 anchorCenter, Vector2 rectSize, Vector2 pos, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        Text label = obj.AddComponent<Text>();
        label.text = text;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = size;
        label.alignment = anchor;
        label.color = color;
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = 12;
        label.resizeTextMaxSize = size;
        RectTransform rect = label.rectTransform;
        rect.anchorMin = anchorCenter;
        rect.anchorMax = anchorCenter;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = rectSize;
        rect.anchoredPosition = pos;
        return label;
    }

    private static Text CreateTopText(Transform parent, string name, string text, Vector2 pos, TextAnchor anchor)
    {
        return CreateText(parent, name, text, 24, anchor, new Vector2(0.5f, 1f), new Vector2(360f, 42f), pos);
    }

    private static Text CreateBottomText(Transform parent, string name, string text, Vector2 pos)
    {
        return CreateText(parent, name, text, 22, TextAnchor.MiddleCenter, new Vector2(0.5f, 0f), new Vector2(760f, 42f), pos);
    }

    private static Button CreateButton(Transform parent, string name, string text, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction action)
    {
        GameObject obj = CreatePanel(parent, name, new Vector2(0.5f, 0.5f), size, pos, new Color(0.08f, 0.42f, 0.86f, 0.95f));
        Button button = obj.AddComponent<Button>();
        obj.AddComponent<CommonButtonUI>();
        if (action != null) button.onClick.AddListener(action);
        Text label = CreateText(obj.transform, "Text", text, 24, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), size, Vector2.zero);
        label.raycastTarget = false;
        return button;
    }

    private static GameObject CreatePanel(Transform parent, string name, Vector2 anchorCenter, Vector2 size, Vector2 pos, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        Image image = obj.AddComponent<Image>();
        image.color = color;
        RectTransform rect = image.rectTransform;
        rect.anchorMin = anchorCenter;
        rect.anchorMax = anchorCenter;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = pos;
        return obj;
    }

    private static void CreateBalanceMeter(Transform parent)
    {
        GameObject root = CreatePanel(parent, "BalanceMeterRoot", new Vector2(0.5f, 0.18f), new Vector2(260f, 72f), Vector2.zero, new Color(0.02f, 0.12f, 0.25f, 0.65f));
        root.AddComponent<BalanceMeterUI>();
        GameObject needle = CreatePanel(root.transform, "Needle", new Vector2(0.5f, 0.5f), new Vector2(10f, 66f), Vector2.zero, new Color(1f, 0.95f, 0.2f, 1f));
        SerializedObject serialized = new SerializedObject(root.GetComponent<BalanceMeterUI>());
        serialized.FindProperty("needle").objectReferenceValue = needle.GetComponent<RectTransform>();
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateSoccerWalls()
    {
        CreateWall("TopWall", new Vector3(0f, 4.2f, 0f), new Vector2(15f, 0.35f));
        CreateWall("BottomWall", new Vector3(0f, -4.2f, 0f), new Vector2(15f, 0.35f));
        CreateWall("LeftBackWall", new Vector3(-7.8f, 0f, 0f), new Vector2(0.3f, 8.2f));
        CreateWall("RightBackWall", new Vector3(7.8f, 0f, 0f), new Vector2(0.3f, 8.2f));
    }

    private static void CreateWall(string name, Vector3 position, Vector2 size)
    {
        GameObject wall = new GameObject(name);
        wall.transform.position = position;
        BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();
        collider.size = size;
    }

    private static void CreateEventSystem()
    {
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
        UnityEngine.InputSystem.UI.InputSystemUIInputModule module = eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        UnityEngine.InputSystem.InputActionAsset actions = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>("Assets/InputSystem_Actions.inputactions");
        if (actions != null)
        {
            module.actionsAsset = actions;
        }
#else
        eventSystem.AddComponent<StandaloneInputModule>();
#endif
    }

    private static GameObject CreateWorldSprite(string name, Sprite sprite, Vector3 position, Vector3 scale)
    {
        GameObject obj = new GameObject(name);
        obj.transform.position = position;
        obj.transform.localScale = scale;
        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = -20;
        return obj;
    }

    private static void CreatePlaceholderSprites()
    {
        SaveSprite("Assets/Sprites/Backgrounds/placeholder_menu_bg.png", 960, 540, (x, y) => Color.Lerp(new Color(0.07f, 0.25f, 0.58f), new Color(0.82f, 0.94f, 1f), y / 539f), 1f);
        SaveSprite("Assets/Sprites/Backgrounds/placeholder_soccer_field.png", 256, 160, (x, y) => new Color(0.16f, 0.58f, 0.34f), 32f);
        SaveSprite("Assets/Sprites/Backgrounds/placeholder_campus_block.png", 240, 120, (x, y) => y < 10 || x < 10 || x > 230 || (x > 40 && x < 68 && y > 42) || (x > 96 && x < 126 && y > 30) || (x > 158 && x < 194 && y > 52) ? new Color(0.4f, 0.72f, 1f, 0.85f) : Color.clear, 100f);
        SaveSprite("Assets/Sprites/Chito/placeholder_chito.png", 96, 96, (x, y) => CirclePixel(x, y, 96, new Color(0.2f, 0.85f, 1f), Color.white), 100f);
        SaveSprite("Assets/Sprites/Chito/placeholder_balance_chito.png", 80, 180, (x, y) => CapsulePixel(x, y, 80, 180, new Color(0.18f, 0.78f, 1f), new Color(0.03f, 0.14f, 0.28f)), 100f);
        SaveSprite("Assets/Sprites/Platforms/placeholder_platform.png", 400, 60, (x, y) => y > 42 ? new Color(0.76f, 0.9f, 1f) : new Color(0.2f, 0.38f, 0.56f), 100f);
        SaveSprite("Assets/Sprites/Items/placeholder_aplus.png", 72, 72, (x, y) => CirclePixel(x, y, 72, new Color(1f, 0.88f, 0.18f), Color.white), 100f);
        SaveSprite("Assets/Sprites/Obstacles/placeholder_obstacle.png", 72, 72, (x, y) => new Color(1f, 0.28f, 0.25f, 1f), 100f);
        SaveSprite("Assets/Sprites/Placeholder/placeholder_ground.png", 320, 40, (x, y) => new Color(0.15f, 0.32f, 0.48f), 100f);
        SaveSprite("Assets/Sprites/Placeholder/placeholder_lane_mark.png", 96, 18, (x, y) => new Color(0.72f, 0.94f, 1f, 0.95f), 100f);
        SaveSprite("Assets/Sprites/Placeholder/placeholder_distance_post.png", 28, 128, (x, y) => x < 8 || x > 20 || y > 104 ? new Color(0.9f, 0.97f, 1f, 0.95f) : new Color(0.1f, 0.45f, 0.95f, 0.9f), 100f);
        SaveSprite("Assets/Sprites/Placeholder/placeholder_p1.png", 72, 72, (x, y) => CirclePixel(x, y, 72, new Color(0.1f, 0.55f, 1f), Color.white), 100f);
        SaveSprite("Assets/Sprites/Placeholder/placeholder_p2.png", 72, 72, (x, y) => CirclePixel(x, y, 72, new Color(1f, 0.32f, 0.36f), Color.white), 100f);
        SaveSprite("Assets/Sprites/Placeholder/placeholder_ball.png", 64, 64, (x, y) => CirclePixel(x, y, 64, Color.white, Color.black), 100f);
        SaveSprite("Assets/Sprites/Placeholder/placeholder_goal.png", 48, 180, (x, y) => x < 8 || x > 40 || y < 8 || y > 172 ? new Color(1f, 1f, 1f, 0.85f) : Color.clear, 100f);
    }

    private static Color CirclePixel(int x, int y, int size, Color fill, Color edge)
    {
        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(size * 0.5f, size * 0.5f));
        if (dist > size * 0.43f) return Color.clear;
        return dist > size * 0.34f ? edge : fill;
    }

    private static Color CapsulePixel(int x, int y, int width, int height, Color fill, Color edge)
    {
        float nx = (x - width * 0.5f) / (width * 0.35f);
        float ny = (y - height * 0.5f) / (height * 0.46f);
        float d = nx * nx + ny * ny;
        if (d > 1f) return Color.clear;
        return d > 0.82f ? edge : fill;
    }

    private delegate Color PixelFunc(int x, int y);

    private static void SaveSprite(string path, int width, int height, PixelFunc func, float ppu)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                texture.SetPixel(x, y, func(x, y));
            }
        }

        texture.Apply();
        File.WriteAllBytes(path, texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = ppu;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
    }

    private static Sprite LoadSprite(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static GameObject LoadPrefab(string path)
    {
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    private static GameObject InstantiatePrefab(string path)
    {
        GameObject prefab = LoadPrefab(path);
        return prefab == null ? null : PrefabUtility.InstantiatePrefab(prefab) as GameObject;
    }

    private static void AddPrefabToList(UnityEngine.Object target, string propertyName, GameObject prefab)
    {
        if (target == null || prefab == null)
        {
            return;
        }

        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty list = serialized.FindProperty(propertyName);
        list.arraySize = 1;
        list.GetArrayElementAtIndex(0).objectReferenceValue = prefab;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SavePrefab(GameObject obj, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        PrefabUtility.SaveAsPrefabAsset(obj, path);
        UnityEngine.Object.DestroyImmediate(obj);
    }

    private static void SaveScene(string sceneName)
    {
        Directory.CreateDirectory(ScenesRoot);
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), $"{ScenesRoot}/{sceneName}.unity");
    }
}
#endif
