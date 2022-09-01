using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "ScriptableObject/Share Audio", order = 1)]
public class Share_Audio : SerializedScriptableObject
{
    public Dictionary<Sound_Id, AudioClip> Sound = new Dictionary<Sound_Id, AudioClip>();
    public Dictionary<Music_Id, AudioSource> Music = new Dictionary<Music_Id, AudioSource>();
}