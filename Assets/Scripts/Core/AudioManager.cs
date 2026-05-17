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

        private AudioListener fallbackListener;

        private const string SceneMusicMarker = "SceneBGM";
        private const string SceneMusicMarkerAlt = "SceneMusic";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
            }

            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f;
            musicSource.volume = defaultMusicVolume;

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }

            sfxSource.playOnAwake = false;

            fallbackListener = GetComponent<AudioListener>();
            if (fallbackListener == null)
            {
                fallbackListener = gameObject.AddComponent<AudioListener>();
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

            float clampedVolume = Mathf.Clamp01(volume);
            if (musicSource.clip == clip && musicSource.isPlaying)
            {
                musicSource.volume = clampedVolume;
                musicSource.loop = loop;
                return;
            }

            musicSource.clip = clip;
            musicSource.volume = clampedVolume;
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

            sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
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
