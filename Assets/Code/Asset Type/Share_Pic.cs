using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "ScriptableObject/Share Pic", order = 1)]
public class Share_Pic : SerializedScriptableObject
{
    public Dictionary<string, Sprite> goblin_pic = new Dictionary<string, Sprite>();
    public Dictionary<string, Sprite> human_pic = new Dictionary<string, Sprite>();
    public Dictionary<string, Sprite> bed_pic = new Dictionary<string, Sprite>();

    //    public Sprite[] Rarity_Pic = new Sprite[] { };
}