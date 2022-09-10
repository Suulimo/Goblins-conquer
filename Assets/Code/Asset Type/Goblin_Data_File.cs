using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Goblin Data File", order = 1)]
public class Goblin_Data_File : ScriptableObject
{
    public List<Goblin_Data> goblin_list;
}
