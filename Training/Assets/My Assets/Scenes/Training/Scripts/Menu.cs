using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject optionsMenu;
    public Slider[] volumeSliders;
    public Toggle[] resolutionToggles;
    public Toggle fullscreenToggle;
    public int[] screenWidths;
    int activeResolutionIndex;

	// Use this for initialization
	void Start ()
    {
        activeResolutionIndex = PlayerPrefs.GetInt("screen res index", activeResolutionIndex);
        bool isFullscreen = (PlayerPrefs.GetInt("fullscreen") == 1) ? true : false;

        volumeSliders[0].value = AudioManager.instance.masterVolumePercent;
        volumeSliders[1].value = AudioManager.instance.sfxVolumePercent;
        volumeSliders[2].value = AudioManager.instance.musicVolumePercent;

        for (int i = 0; i < resolutionToggles.Length; i++)
        {
            resolutionToggles[i].isOn = i == activeResolutionIndex;
        }

        SetFullScreen(isFullscreen);
        fullscreenToggle.isOn = isFullscreen;
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public void Play()
    {
        SceneManager.LoadScene("First Game");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void OptionsMenu()
    {
        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }

    public void MainMenu()
    {
        mainMenu.SetActive(true);
        optionsMenu.SetActive(false);
    }

    public void SetScreenResolution(int i)
    {
        if (resolutionToggles[i].isOn)
        {
            Screen.SetResolution(screenWidths[i], (screenWidths[i] * 9) / 16, false);
            activeResolutionIndex = i;

            PlayerPrefs.SetInt("screen res index", i);
            PlayerPrefs.Save();
        }
    }

    public void SetFullScreen(bool fullscreen)
    {
        for (int i = 0; i < resolutionToggles.Length; i++)
        {
            resolutionToggles[i].interactable = !fullscreen;
        }

        if (fullscreen)
        {
            Resolution[] allResolutions = Screen.resolutions;

            Resolution maxRes = allResolutions[allResolutions.Length - 1];

            Screen.SetResolution(maxRes.width, maxRes.height, true);
        }
        else
        {
            SetScreenResolution(activeResolutionIndex);
        }

        PlayerPrefs.SetInt("fullscreen", (fullscreen ? 1 : 0));
        PlayerPrefs.Save();

        fullscreenToggle.isOn = fullscreen;
    }

    public void SetMasterVolume(float value)
    {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Master);
    }

    public void SetSFXVolume(float value)
    {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.SFX);
    }

    public void SetMusicVolume(float value)
    {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Music);
    }
}
