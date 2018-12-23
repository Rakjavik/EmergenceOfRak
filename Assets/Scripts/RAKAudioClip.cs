using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RAKAudioClip
{
    public AudioClip audioClip { get; set; }

    public RAKAudioClip(AudioClip audioClip)
    {
        this.audioClip = audioClip;
    }

    public static RAKAudioClip[] toRakClips(AudioClip[] clips)
    {
        List<RAKAudioClip> rakList = new List<RAKAudioClip>();
        foreach (AudioClip clip in clips)
        {
            RAKAudioClip rakClip = new RAKAudioClip(clip);
            rakList.Add(rakClip);
        }
        return rakList.ToArray();
    }
}
