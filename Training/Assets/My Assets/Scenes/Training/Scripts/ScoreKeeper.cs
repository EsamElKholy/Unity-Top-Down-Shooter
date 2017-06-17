using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
    public static int score;// { get; private set; }
    float lastEnemyKilled;
    int streakCount;
    float streakExpiration = 1;

	// Use this for initialization
	void Start ()
    {
        Enemy.OnDeathStatic += OnEnemyKilled;
        FindObjectOfType<Player>().OnDeath += OnPlayerDeath;
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    void OnEnemyKilled()
    {
        if (Time.time < lastEnemyKilled + streakExpiration)
        {
            streakCount++;
        }
        else
        {
            streakCount = 0;
        }

        lastEnemyKilled = Time.time;

        score += 5 + (int)(Mathf.Clamp(Mathf.Pow(2, streakCount), 0, 200));
        score = (int)Mathf.Clamp(score, 0, 99999999);
    }

    void OnPlayerDeath()
    {
        Enemy.OnDeathStatic -= OnEnemyKilled;
    }
}
