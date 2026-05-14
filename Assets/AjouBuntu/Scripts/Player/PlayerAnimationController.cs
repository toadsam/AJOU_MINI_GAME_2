using System.Collections.Generic;
using AjouBuntu.Config;
using AjouBuntu.Core;
using UnityEngine;

namespace AjouBuntu.Player
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PlayerAnimationController : MonoBehaviour
    {
        [SerializeField] private float runFrameRate = 12f;

        private readonly Dictionary<PlayerAnimState, Sprite[]> frames = new();
        private SpriteRenderer spriteRenderer;
        private PlayerAnimState state;
        private float frameTimer;
        private int frameIndex;

        public void Initialize(GameConfig config)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            frames.Clear();

            if (config != null && config.playerSpriteSheet != null)
            {
                BuildFromSpriteSheet(config.playerSpriteSheet);
            }

            if (frames.Count == 0)
            {
                BuildFallbackFrames();
            }

            SetState(PlayerAnimState.Running, true);
        }

        private void Update()
        {
            if (state != PlayerAnimState.Running && state != PlayerAnimState.Hang)
            {
                return;
            }

            Sprite[] activeFrames = frames[state];
            if (activeFrames.Length <= 1)
            {
                return;
            }

            frameTimer += Time.deltaTime;
            if (frameTimer >= 1f / runFrameRate)
            {
                frameTimer = 0f;
                frameIndex = (frameIndex + 1) % activeFrames.Length;
                spriteRenderer.sprite = activeFrames[frameIndex];
            }
        }

        public void SetState(PlayerAnimState nextState, bool force = false)
        {
            if (!force && state == nextState)
            {
                return;
            }

            state = nextState;
            frameIndex = 0;
            frameTimer = 0f;
            if (frames.TryGetValue(state, out Sprite[] activeFrames) && activeFrames.Length > 0)
            {
                spriteRenderer.sprite = activeFrames[0];
            }
        }

        private void BuildFromSpriteSheet(Texture2D sheet)
        {
            const float ppu = 3f;
            Rect[] runRects =
            {
                new(50, 55, 250, 300),
                new(287, 55, 250, 300),
                new(520, 55, 250, 300),
                new(750, 55, 250, 300),
                new(968, 55, 250, 300),
                new(1180, 55, 250, 300)
            };

            frames[PlayerAnimState.Running] = BuildFrames(sheet, runRects, ppu);
            frames[PlayerAnimState.Jump] = BuildFrames(sheet, new[] { new Rect(208, 400, 250, 300) }, ppu);
            frames[PlayerAnimState.Fall] = BuildFrames(sheet, new[] { new Rect(718, 400, 250, 300) }, ppu);
            frames[PlayerAnimState.Landing] = BuildFrames(sheet, new[] { new Rect(962, 400, 250, 300) }, ppu);
            frames[PlayerAnimState.Hang] = BuildFrames(sheet, new[] { new Rect(350, 735, 340, 280), new Rect(715, 735, 370, 280) }, ppu);
        }

        private static Sprite[] BuildFrames(Texture2D sheet, Rect[] sourceRects, float pixelsPerUnit)
        {
            Sprite[] result = new Sprite[sourceRects.Length];
            for (int i = 0; i < sourceRects.Length; i++)
            {
                Rect source = sourceRects[i];
                Rect unityRect = new Rect(source.x, sheet.height - source.y - source.height, source.width, source.height);
                result[i] = Sprite.Create(sheet, unityRect, new Vector2(0.5f, 0.15f), pixelsPerUnit);
                result[i].name = $"chito_{i}";
            }

            return result;
        }

        private void BuildFallbackFrames()
        {
            Sprite runA = RuntimeSpriteFactory.CreateCapsuleSprite(new Color(0.18f, 0.78f, 1f), new Color(0.02f, 0.11f, 0.28f), 72, 100);
            Sprite runB = RuntimeSpriteFactory.CreateCapsuleSprite(new Color(0.35f, 0.92f, 1f), new Color(0.02f, 0.11f, 0.28f), 72, 100);
            Sprite air = RuntimeSpriteFactory.CreateCapsuleSprite(new Color(1f, 0.92f, 0.42f), new Color(0.02f, 0.11f, 0.28f), 72, 100);
            frames[PlayerAnimState.Running] = new[] { runA, runB };
            frames[PlayerAnimState.Jump] = new[] { air };
            frames[PlayerAnimState.Fall] = new[] { air };
            frames[PlayerAnimState.Landing] = new[] { runB };
            frames[PlayerAnimState.Hang] = new[] { air };
        }
    }
}
