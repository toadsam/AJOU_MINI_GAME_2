using UnityEngine;
using UnityEngine.SceneManagement;

namespace AjouFestival.Core
{
    public sealed class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField, Range(0f, 1f)] private float defaultMusicVolume = 0.7f;
        [SerializeField, Range(0f, 1f)] private float defaultMusicSliderVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float defaultSfxSliderVolume = 1f;

        [Header("Options UI")]
        [SerializeField] private bool showAudioOptionsUI = true;
        [SerializeField] private string optionsButtonText = "\uC635\uC158";
        [SerializeField] private string optionsTitleText = "\uC18C\uB9AC \uC124\uC815";
        [SerializeField] private string closeButtonText = "\uB2EB\uAE30";
        [SerializeField] private string musicLabelText = "\uBC30\uACBD\uC74C\uC545";
        [SerializeField] private string sfxLabelText = "\uD6A8\uACFC\uC74C";
        [SerializeField] private Vector2 optionsButtonPosition = new Vector2(14f, -14f);
        [SerializeField] private Vector2 optionsButtonSize = new Vector2(92f, 36f);
        [SerializeField] private Vector2 optionsPanelPosition = new Vector2(14f, -58f);
        [SerializeField] private Vector2 optionsPanelSize = new Vector2(280f, 164f);
        [SerializeField] private int optionsSortingOrder = 5000;
        [SerializeField] private Color optionsButtonColor = new Color(0.08f, 0.42f, 0.86f, 0.96f);
        [SerializeField] private Color optionsPanelColor = new Color(0.03f, 0.08f, 0.14f, 0.9f);
        [SerializeField] private Color optionsTextColor = Color.white;
        [SerializeField] private Color optionsAccentColor = new Color(0.2f, 0.78f, 1f, 1f);
        [SerializeField] private Sprite optionsButtonSprite;
        [SerializeField] private Sprite closeButtonSprite;
        [SerializeField] private Sprite optionsPanelSprite;

        private AudioListener fallbackListener;
        private float currentMusicBaseVolume = 0.7f;
        private float musicVolume = 1f;
        private float sfxVolume = 1f;

        private const string SceneMusicMarker = "SceneBGM";
        private const string SceneMusicMarkerAlt = "SceneMusic";
        private const string MusicVolumePrefKey = "Audio.MusicVolume";
        private const string SfxVolumePrefKey = "Audio.SfxVolume";

        public float MusicVolume => musicVolume;
        public float SfxVolume => sfxVolume;
        public bool ShowAudioOptionsUI => showAudioOptionsUI;
        public string OptionsButtonText => optionsButtonText;
        public string OptionsTitleText => optionsTitleText;
        public string CloseButtonText => closeButtonText;
        public string MusicLabelText => musicLabelText;
        public string SfxLabelText => sfxLabelText;
        public Vector2 OptionsButtonPosition => optionsButtonPosition;
        public Vector2 OptionsButtonSize => optionsButtonSize;
        public Vector2 OptionsPanelPosition => optionsPanelPosition;
        public Vector2 OptionsPanelSize => optionsPanelSize;
        public int OptionsSortingOrder => optionsSortingOrder;
        public Color OptionsButtonColor => optionsButtonColor;
        public Color OptionsPanelColor => optionsPanelColor;
        public Color OptionsTextColor => optionsTextColor;
        public Color OptionsAccentColor => optionsAccentColor;
        public Sprite OptionsButtonSprite => optionsButtonSprite;
        public Sprite CloseButtonSprite => closeButtonSprite;
        public Sprite OptionsPanelSprite => optionsPanelSprite;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            musicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumePrefKey, defaultMusicSliderVolume));
            sfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumePrefKey, defaultSfxSliderVolume));
            currentMusicBaseVolume = defaultMusicVolume;

            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
            }

            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f;
            musicSource.volume = GetEffectiveMusicVolume();

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }

            sfxSource.playOnAwake = false;
            sfxSource.volume = 1f;

            fallbackListener = GetComponent<AudioListener>();
            if (fallbackListener == null)
            {
                fallbackListener = gameObject.AddComponent<AudioListener>();
            }

            if (showAudioOptionsUI)
            {
                AudioOptionsUI optionsUI = GetComponent<AudioOptionsUI>();
                if (optionsUI == null)
                {
                    optionsUI = gameObject.AddComponent<AudioOptionsUI>();
                }

                optionsUI.Initialize(this);
            }
        }

        private void OnValidate()
        {
            defaultMusicVolume = Mathf.Clamp01(defaultMusicVolume);
            defaultMusicSliderVolume = Mathf.Clamp01(defaultMusicSliderVolume);
            defaultSfxSliderVolume = Mathf.Clamp01(defaultSfxSliderVolume);
            optionsButtonSize = ClampPositive(optionsButtonSize, new Vector2(60f, 28f));
            optionsPanelSize = ClampPositive(optionsPanelSize, new Vector2(220f, 130f));

            if (!Application.isPlaying)
            {
                return;
            }

            if (Instance == this && musicSource != null)
            {
                musicSource.volume = GetEffectiveMusicVolume();
            }

            AudioOptionsUI optionsUI = GetComponent<AudioOptionsUI>();
            if (optionsUI != null)
            {
                optionsUI.Rebuild();
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void Start()
        {
            UpdateFallbackAudioListener();
            PlaySceneMusic(SceneManager.GetActiveScene());
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            UpdateFallbackAudioListener();
            PlaySceneMusic(scene);
        }

        public static AudioManager Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            GameObject obj = new GameObject("AudioManager");
            return obj.AddComponent<AudioManager>();
        }

        public void PlayMusic(AudioClip clip)
        {
            PlayMusic(clip, defaultMusicVolume, true);
        }

        public void PlayMusic(AudioClip clip, float volume, bool loop = true)
        {
            if (clip == null || musicSource == null)
            {
                return;
            }

            currentMusicBaseVolume = Mathf.Clamp01(volume);
            if (musicSource.clip == clip && musicSource.isPlaying)
            {
                musicSource.volume = GetEffectiveMusicVolume();
                musicSource.loop = loop;
                return;
            }

            musicSource.clip = clip;
            musicSource.volume = GetEffectiveMusicVolume();
            musicSource.loop = loop;
            musicSource.Play();
        }

        public void PlaySfx(AudioClip clip)
        {
            PlaySfx(clip, 1f);
        }

        public void PlaySfx(AudioClip clip, float volume)
        {
            if (clip == null || sfxSource == null)
            {
                return;
            }

            sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume) * sfxVolume);
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(MusicVolumePrefKey, musicVolume);
            PlayerPrefs.Save();

            if (musicSource != null)
            {
                musicSource.volume = GetEffectiveMusicVolume();
            }
        }

        public void SetSfxVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(SfxVolumePrefKey, sfxVolume);
            PlayerPrefs.Save();
        }

        private float GetEffectiveMusicVolume()
        {
            return currentMusicBaseVolume * musicVolume;
        }

        private static Vector2 ClampPositive(Vector2 value, Vector2 minimum)
        {
            return new Vector2(Mathf.Max(minimum.x, value.x), Mathf.Max(minimum.y, value.y));
        }

        private void PlaySceneMusic(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return;
            }

            AudioSource sceneMusic = FindSceneMusicSource(scene);
            if (sceneMusic == null || sceneMusic.clip == null)
            {
                return;
            }

            PlayMusic(sceneMusic.clip, sceneMusic.volume, sceneMusic.loop);
            sceneMusic.Stop();
            sceneMusic.playOnAwake = false;
        }

        private AudioSource FindSceneMusicSource(Scene scene)
        {
            AudioSource firstClipSource = null;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                AudioSource found = FindSceneMusicSourceInChildren(roots[i], ref firstClipSource);
                if (found != null)
                {
                    return found;
                }
            }

            return firstClipSource;
        }

        private AudioSource FindSceneMusicSourceInChildren(GameObject root, ref AudioSource firstClipSource)
        {
            if (root == null || root == gameObject)
            {
                return null;
            }

            AudioSource[] sources = root.GetComponentsInChildren<AudioSource>(true);
            for (int i = 0; i < sources.Length; i++)
            {
                AudioSource source = sources[i];
                if (source == null || source.clip == null || source == musicSource || source == sfxSource)
                {
                    continue;
                }

                string objectName = source.gameObject.name;
                if (objectName.IndexOf(SceneMusicMarker, System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    objectName.IndexOf(SceneMusicMarkerAlt, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return source;
                }

                if (firstClipSource == null && !IsSceneSfxSourceName(objectName))
                {
                    firstClipSource = source;
                }
            }

            return null;
        }

        private static bool IsSceneSfxSourceName(string objectName)
        {
            if (string.IsNullOrWhiteSpace(objectName))
            {
                return false;
            }

            string[] sfxMarkers =
            {
                "SFX", "Sound", "Jump", "Item", "Collect", "Hit", "Obstacle", "Speed",
                "\uC810\uD504", "\uC544\uC774\uD15C", "\uC7A5\uC560\uBB3C", "\uC18D\uB3C4"
            };

            for (int i = 0; i < sfxMarkers.Length; i++)
            {
                if (objectName.IndexOf(sfxMarkers[i], System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private void UpdateFallbackAudioListener()
        {
            if (fallbackListener == null)
            {
                fallbackListener = GetComponent<AudioListener>();
                if (fallbackListener == null)
                {
                    fallbackListener = gameObject.AddComponent<AudioListener>();
                }
            }

            bool hasSceneListener = false;
            AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            for (int i = 0; i < listeners.Length; i++)
            {
                AudioListener listener = listeners[i];
                if (listener != null && listener != fallbackListener && listener.enabled)
                {
                    hasSceneListener = true;
                    break;
                }
            }

            fallbackListener.enabled = !hasSceneListener;
        }
    }
}
