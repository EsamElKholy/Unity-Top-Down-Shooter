using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundLibrary : MonoBehaviour
{
    public SoundGroup[] soundGroups;

    Dictionary<string, AudioClip[]> groupDictionary = new Dictionary<string, AudioClip[]>();

    private void Awake()
    {
        foreach (SoundGroup group in soundGroups)
        {
            groupDictionary.Add(group.groupID, group.group);
        }
    }

    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public AudioClip GetClipFromName(string name)
    {
        if (groupDictionary.ContainsKey(name))
        {
            AudioClip[] clips = groupDictionary[name];

            return clips[Random.Range(0, clips.Length)];
        }

        return null;
    }

    [System.Serializable]
    public class SoundGroup
    {
        public string groupID;
        public AudioClip[] group;
    }
}
