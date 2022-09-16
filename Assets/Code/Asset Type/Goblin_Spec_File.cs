using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Goblin Spec File", order = 1)]
public class Goblin_Spec_File : ScriptableObject
{
    public List<GCQ.Goblin_Spec> goblin_list;
}
