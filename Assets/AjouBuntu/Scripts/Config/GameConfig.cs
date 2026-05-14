using System.Collections.Generic;
using UnityEngine;

namespace AjouBuntu.Config
{
    public enum ItemKind
    {
        APlus,
        IdCard,
        Coffee,
        Attendance,
        MealTicket,
        Coupon,
        Sticker
    }

    public enum PlatformKind
    {
        StoneBridge,
        Stairs,
        Rooftop,
        LibraryShelf,
        FestivalBooth,
        BusStop
    }

    public enum ObstacleKind
    {
        AssignmentBomb,
        ExamPaper,
        LateAlarm,
        ConstructionSign
    }

    public enum PlayerAnimState
    {
        Running,
        Jump,
        Fall,
        Landing,
        Hang
    }

    public enum CoinArcProfile
    {
        Safe,
        Tight,
        Bait
    }

    [System.Serializable]
    public sealed class ItemDefinition
    {
        public ItemKind kind;
        public int score;
        public Sprite sprite;
    }

    [System.Serializable]
    public sealed class PlatformDefinition
    {
        public PlatformKind kind;
        public Sprite sprite;
        public Vector2 visualSize = new Vector2(320f, 72f);
        public Vector2 colliderSize = new Vector2(300f, 18f);
        public Vector2 colliderOffset = new Vector2(0f, 30f);
    }

    [System.Serializable]
    public sealed class ObstacleDefinition
    {
        public ObstacleKind kind;
        public Sprite sprite;
        public Vector2 colliderSize = new Vector2(48f, 48f);
    }

    [CreateAssetMenu(menuName = "AjouBuntu/Game Config", fileName = "GameConfig")]
    public sealed class GameConfig : ScriptableObject
    {
        [Header("Logical Screen")]
        public Vector2 logicalSize = new Vector2(960f, 540f);
        public Vector2 playerStartScreenPosition = new Vector2(170f, 402f);

        [Header("Runner Tuning")]
        public float gravity = 2050f;
        public float initialSpeed = 350f;
        public float maxSpeed = 780f;
        public float speedIncreasePer900Ms = 10.5f;
        public float jumpSpeed = -820f;
        public float targetDistance = 24000f;
        public float scorePerSecond = 12f;
        public float deathLineScreenY = 650f;
        public float coyoteTime = 0.11f;
        public int maxAirJumps = 1;
        public float landingDuration = 0.11f;

        [Header("Spawn")]
        public float spawnX = 1120f;
        public float despawnX = -260f;
        public float platformTopMinScreenY = 130f;
        public float platformTopMaxScreenY = 448f;
        public float easyGapMin = 110f;
        public float easyGapMax = 175f;
        public float hardGapMin = 155f;
        public float hardGapMax = 245f;
        public float platformMinWidth = 280f;
        public float platformMaxWidth = 430f;

        [Header("Features")]
        public bool wireEnabled = false;
        public bool obstacleEnabled = false;

        [Header("Art")]
        public Sprite bgCampusDay;
        public Sprite bgCampusSunset;
        public Sprite bgCampusNight;
        public Texture2D playerSpriteSheet;

        [Header("Definitions")]
        public List<ItemDefinition> items = new();
        public List<PlatformDefinition> platforms = new();
        public List<ObstacleDefinition> obstacles = new();

        public float WorldYFromScreenY(float screenY) => logicalSize.y - screenY;
        public float ScreenYFromWorldY(float worldY) => logicalSize.y - worldY;

        public ItemDefinition GetItem(ItemKind kind)
        {
            EnsureDefaults();
            return items.Find(item => item.kind == kind);
        }

        public PlatformDefinition GetPlatform(PlatformKind kind)
        {
            EnsureDefaults();
            return platforms.Find(platform => platform.kind == kind);
        }

        public void EnsureDefaults()
        {
            EnsureItem(ItemKind.APlus, 100);
            EnsureItem(ItemKind.IdCard, 50);
            EnsureItem(ItemKind.Coffee, 30);
            EnsureItem(ItemKind.Attendance, 70);
            EnsureItem(ItemKind.MealTicket, 80);
            EnsureItem(ItemKind.Coupon, 60);
            EnsureItem(ItemKind.Sticker, 40);

            EnsurePlatform(PlatformKind.StoneBridge, new Vector2(340f, 74f), new Vector2(316f, 16f), new Vector2(0f, 29f));
            EnsurePlatform(PlatformKind.Stairs, new Vector2(320f, 86f), new Vector2(290f, 18f), new Vector2(0f, 34f));
            EnsurePlatform(PlatformKind.Rooftop, new Vector2(360f, 76f), new Vector2(330f, 18f), new Vector2(0f, 31f));
            EnsurePlatform(PlatformKind.LibraryShelf, new Vector2(300f, 92f), new Vector2(280f, 18f), new Vector2(0f, 37f));
            EnsurePlatform(PlatformKind.FestivalBooth, new Vector2(350f, 96f), new Vector2(310f, 18f), new Vector2(0f, 39f));
            EnsurePlatform(PlatformKind.BusStop, new Vector2(330f, 86f), new Vector2(300f, 18f), new Vector2(0f, 34f));

            EnsureObstacle(ObstacleKind.AssignmentBomb);
            EnsureObstacle(ObstacleKind.ExamPaper);
            EnsureObstacle(ObstacleKind.LateAlarm);
            EnsureObstacle(ObstacleKind.ConstructionSign);
        }

        private void EnsureItem(ItemKind kind, int score)
        {
            if (items.Exists(item => item.kind == kind))
            {
                return;
            }

            items.Add(new ItemDefinition { kind = kind, score = score });
        }

        private void EnsurePlatform(PlatformKind kind, Vector2 visualSize, Vector2 colliderSize, Vector2 colliderOffset)
        {
            if (platforms.Exists(platform => platform.kind == kind))
            {
                return;
            }

            platforms.Add(new PlatformDefinition
            {
                kind = kind,
                visualSize = visualSize,
                colliderSize = colliderSize,
                colliderOffset = colliderOffset
            });
        }

        private void EnsureObstacle(ObstacleKind kind)
        {
            if (obstacles.Exists(obstacle => obstacle.kind == kind))
            {
                return;
            }

            obstacles.Add(new ObstacleDefinition { kind = kind });
        }
    }
}
