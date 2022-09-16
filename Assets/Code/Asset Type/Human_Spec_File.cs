using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Human Spec File", order = 1)]
public class Human_Spec_File : ScriptableObject
{
    public List<GCQ.Human_Spec> female_list;
    public List<GCQ.Human_Spec> male_list;
}
