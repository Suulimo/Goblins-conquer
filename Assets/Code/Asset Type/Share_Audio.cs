using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Share Audio", order = 1)]
public class Share_Audio : SerializedScriptableObject
{
    public Dictionary<GCQ.Sound_Id, AudioClip> Sound = new Dictionary<GCQ.Sound_Id, AudioClip>();
    public Dictionary<GCQ.Music_Id, AudioSource> Music = new Dictionary<GCQ.Music_Id, AudioSource>();
}