using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public Image fadePlane;
    public GameObject gameOverUI;
    public RectTransform newWaveBanner;
    public Text newWaveTitle;
    public Text newWaveEnemyCount;
    public Text scoreUI;
    public Text gameOverScoreUI;
    public RectTransform healthBar;

    Spawner spawner;
    Player player;

    private void Awake()
    {
        spawner = FindObjectOfType<Spawner>();
        spawner.OnNewWave += OnNewWave;
    }

    public void OnNewWave(int index)
    {
        string[] numbers = { "One", "Two", "Three", "Four", "Five" };
        newWaveTitle.text = "- Wave " + numbers[index - 1] + " -";

        if (spawner.enemyWaves[index - 1].infinite)
        {
            newWaveEnemyCount.text = "Enemies: INFINITE";
        }
        else
        {
            newWaveEnemyCount.text = "Enemies: " + spawner.enemyWaves[index - 1].enemyCount;
        }

        StopCoroutine("AnimateBanner");
        StartCoroutine("AnimateBanner");
    }

    IEnumerator AnimateBanner()
    {
        float percent = 0;
        float speed = 1.5f;
        float delayTime = 1f;
        int dir = 1;
        float endDelay = Time.time + 1 / speed + delayTime;

        while (percent >= 0)
        {
            percent += Time.deltaTime * speed * dir;

            if (percent >= 1)
            {
                percent = 1;

                if (Time.time > endDelay)
                {
                    dir = -1;
                }
            }

            newWaveBanner.anchoredPosition = Vector2.up * Mathf.Lerp(-370, -40, percent);

            yield return null;
        }
    }

    // Use this for initialization
    void Start ()
    {
        player = FindObjectOfType<Player>();
        player.OnDeath += OnGameOver;	
	}
	
	// Update is called once per frame
	void Update ()
    {
        scoreUI.text = ScoreKeeper.score.ToString("D8");
        float healthPercent = 0;
        if (player != null)
        {
            healthPercent = player.currentHealth / player.maxHealth;
        }

        healthBar.localScale = new Vector3(healthPercent, 1, 1);
	}

    void OnGameOver()
    {
        StartCoroutine(Fade(Color.clear, new Color(0, 0, 0, 0.84f), 1));
        gameOverScoreUI.text = scoreUI.text;
        scoreUI.gameObject.SetActive(false);
        healthBar.transform.parent.gameObject.SetActive(false);

        gameOverUI.SetActive(true);
    }

    IEnumerator Fade(Color from, Color to, float time)
    {
        float speed = 1 / time;

        float percent = 0;

        while (percent < 1)
        {
            percent += Time.deltaTime * speed;

            fadePlane.color = Color.Lerp(from, to, percent);

            yield return null;
        }
    }

    // UI Input
    public void StartNewGame()
    {
        SceneManager.LoadScene("First Game");
        ScoreKeeper.score = 0;
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
