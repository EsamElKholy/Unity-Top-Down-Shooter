using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public AudioClip mainTheme;
    public AudioClip menuTheme;

    string currentSceneName;

    // Use this for initialization
    void Start ()
    {
        OnLevelWasLoaded(0);
	}
	
	// Update is called once per frame
	void Update ()
    {
        
    }

    private void OnLevelWasLoaded(int level)
    {
        string newSceneName =  SceneManager.GetActiveScene().name;

        if (newSceneName != currentSceneName)
        {
            currentSceneName = newSceneName;

            Invoke("PlayMusic", 0.2f);
        }
    }

    void PlayMusic()
    {
        AudioClip clip = null;

        if (currentSceneName == "Menu")
        {
            clip = menuTheme;
        }
        else if(currentSceneName == "First Game")
        {
            clip = mainTheme;
        }

        if (clip != null)
        {
            AudioManager.instance.PlayMusic(clip, 2);

            Invoke("PlayMusic", clip.length);
        }
    }
}
