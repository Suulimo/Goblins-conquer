using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * ¦³Ãö Game State
 */

public struct Play_Speed
{
    public float time_scale;
}


public struct Drag_To
{
    public Slot_Type slot_type;
    public int slot_id;
}

public struct Drag_To_Cancel
{
}

public struct Drag_Begin
{
    public Slot_Type slot_type;
    public int slot_id;
}

public struct Drag_End
{
    public Slot_Type slot_type;
    public int slot_id;
}

public struct Drag_Cancel
{
}

public struct Human_Spawned
{
    public int slot_id;
    public Human_Data human_data;
}

public struct Goblin_Spawned
{
    public int slot_id;
    public Goblin_Data goblin_data;
}

public struct Bed_Spawned
{
    public int slot_id;
    public Human_Data human_data;
}