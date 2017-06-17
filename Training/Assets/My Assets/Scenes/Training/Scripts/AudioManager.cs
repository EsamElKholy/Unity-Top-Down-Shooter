using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public enum AudioChannel { Master, SFX, Music };

    public float masterVolumePercent { get; private set; }
    public float sfxVolumePercent { get; private set; }
    public float musicVolumePercent { get; private set; }

    AudioSource[] musicSources;
    AudioSource sound2DSource;
    int activeMusicSourceIndex;

    public static AudioManager instance;

    Transform playerT;
    Transform audioListener;

    SoundLibrary library;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            masterVolumePercent = 1f;
            sfxVolumePercent = 1;
            musicVolumePercent = 1f;

           // DontDestroyOnLoad(gameObject);

            musicSources = new AudioSource[2];

            for (int i = 0; i < musicSources.Length; i++)
            {
                GameObject newMusicSource = new GameObject("Music Source " + (i + 1));
                musicSources[i] = newMusicSource.AddComponent<AudioSource>();
                newMusicSource.transform.SetParent(transform);
            }

            GameObject newsound2DSource = new GameObject("Sound 2D Source");
            sound2DSource = newsound2DSource.AddComponent<AudioSource>();
            newsound2DSource.transform.SetParent(transform);

            instance = this;

            library = GetComponent<SoundLibrary>();

            if (FindObjectOfType<Player>() != null)
            {
                playerT = FindObjectOfType<Player>().transform;
            }

            audioListener = FindObjectOfType<AudioListener>().transform;
        }

        masterVolumePercent = PlayerPrefs.GetFloat("master vol", 1);
        sfxVolumePercent = PlayerPrefs.GetFloat("sfx vol", 1);
        musicVolumePercent = PlayerPrefs.GetFloat("music vol", 1);
    }

    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (playerT != null)
        {
            audioListener.position = playerT.position;
        }
	}

    public void SetVolume(float volumePercent, AudioChannel channel)
    {
        switch (channel)
        {
            case AudioChannel.Master:
                masterVolumePercent = volumePercent;
                break;
            case AudioChannel.SFX:
                sfxVolumePercent = volumePercent;
                break;
            case AudioChannel.Music:
                musicVolumePercent = volumePercent;
                break;
        }

        musicSources[0].volume = musicVolumePercent * masterVolumePercent;
        musicSources[1].volume = musicVolumePercent * masterVolumePercent;

        PlayerPrefs.SetFloat("master vol", masterVolumePercent);
        PlayerPrefs.SetFloat("sfx vol", sfxVolumePercent);
        PlayerPrefs.SetFloat("music vol", musicVolumePercent);

        PlayerPrefs.Save();
    }

    public void PlaySound(AudioClip clip, Vector3 pos)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, pos, sfxVolumePercent * masterVolumePercent);
        }
    }

    public void PlaySound(string clipName, Vector3 pos)
    {
        PlaySound(library.GetClipFromName(clipName), pos);
    }

    public void PlaySound2D(string clipName)
    {
        sound2DSource.PlayOneShot(library.GetClipFromName(clipName), sfxVolumePercent * masterVolumePercent);
    }

    public void PlayMusic(AudioClip clip, float fadeDuration = 1)
    {
        activeMusicSourceIndex = 1 - activeMusicSourceIndex;
        musicSources[activeMusicSourceIndex].clip = clip;
       // musicSources[activeMusicSourceIndex].loop = true;
        musicSources[activeMusicSourceIndex].Play();

        StartCoroutine(AnimateMusicCrossfade(fadeDuration));
    }

    IEnumerator AnimateMusicCrossfade(float fadeDuration)
    {
        float percent = 0;
        float fadeSpeed = 1 / fadeDuration;

        while (percent < 1)
        {
            percent += Time.deltaTime * fadeSpeed;

            musicSources[activeMusicSourceIndex].volume = Mathf.Lerp(0, musicVolumePercent * masterVolumePercent, percent); 
            musicSources[1 - activeMusicSourceIndex].volume = Mathf.Lerp(musicVolumePercent * masterVolumePercent, 0, percent);

            yield return null;
        }
    }
}
