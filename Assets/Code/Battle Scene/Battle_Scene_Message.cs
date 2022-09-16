using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public struct Drag_To
{
    public GCQ.Slot_Type slot_type;
    public int2 slot_id;
}

public struct Drag_To_Cancel
{
}

public struct Drag_Begin
{
    public GCQ.Slot_Type slot_type;
    public int2 slot_id;
}

public struct Drag_End
{
    public GCQ.Slot_Type slot_type;
    public int2 slot_id;
}

public struct Drag_Cancel
{
}
