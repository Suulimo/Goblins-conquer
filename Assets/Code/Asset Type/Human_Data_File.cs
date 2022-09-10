using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Human Data File", order = 1)]
public class Human_Data_File : ScriptableObject
{
    public List<Human_Data> female_list;
    public List<Human_Data> male_list;
}
