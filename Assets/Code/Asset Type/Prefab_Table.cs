using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "ScriptableObject/Prefab Table", order = 1)]
public class Prefab_Table : ScriptableObject
{
    [System.Serializable]
    public class Prefab_Key
    {
        public string key;
        public int init_pool_capacity;
    }

    public List<Prefab_Key> sheet;

}
