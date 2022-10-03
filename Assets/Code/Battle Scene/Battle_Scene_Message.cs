using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public struct Drag_Begin_Msg
{
    public GCQ.Slot_Type slot_type;
    public int2 slot_id;
    public bool cursor_diff;
}

public struct Drag_End_Msg
{
    public GCQ.Slot_Type slot_type;
    public int2 slot_id;
}

public struct Selection_Done_Msg
{
    public GCQ.Slot_Type slot_type;
    public int2 slot_id;
    public bool cursor_diff;
}

