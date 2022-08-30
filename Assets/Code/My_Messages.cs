using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DragTo
{
    public Slot_Type slot_type;
    public int slot_id;
}

public struct DragToCancel
{
}

public struct DragBegin
{
    public Slot_Type slot_type;
    public int slot_id;
}

public struct DragEnd
{
    public Slot_Type slot_type;
    public int slot_id;
}

public struct DragCancel
{
}