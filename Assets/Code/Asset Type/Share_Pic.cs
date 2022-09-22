using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Share Pic", order = 1)]
public class Share_Pic : SerializedScriptableObject
{
    public Dictionary<string, Sprite> goblin_pic = new Dictionary<string, Sprite>();
    public Dictionary<string, Sprite> human_pic = new Dictionary<string, Sprite>();
    public Dictionary<string, Sprite> bed_pic = new Dictionary<string, Sprite>();
    public Dictionary<string, Sprite> item_pic = new Dictionary<string, Sprite>();

    //    public Sprite[] Rarity_Pic = new Sprite[] { };
}