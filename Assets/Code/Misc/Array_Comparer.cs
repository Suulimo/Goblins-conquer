using System.Collections.Generic;
using UnityEngine;

public class Point_Distance_Comparer : IComparer<Collider2D>
{
    Vector2 _point;
    public Point_Distance_Comparer(Vector2 point) {
        _point = point;
    }
    // Call CaseInsensitiveComparer.Compare with the parameters reversed.
    public int Compare(Collider2D a, Collider2D b) {
        return ((Vector2)a.transform.position - _point).sqrMagnitude.CompareTo(((Vector2)b.transform.position - _point).sqrMagnitude);
    }
}

public static class Copy_Sorted_Point_Ext
{
    public static void CopyToPoint(this Collider2D[] col, int length, ref Vector2[] array) {
        for (int i = 0; i < length; i++) {
            array[i] = col[i].attachedRigidbody.position;
        }
    }
}

public class Human_List_Comparer : IComparer<GCQ.Human_Spec>
{
    // Call CaseInsensitiveComparer.Compare with the parameters reversed.
    public int Compare(GCQ.Human_Spec a, GCQ.Human_Spec b) {
        // id: CAT_008 - CAT_001
        return (a.other.beauty - b.other.beauty);
    }

}

public class Int2_Comparer : IComparer<Unity.Mathematics.int2>
{
    // Call CaseInsensitiveComparer.Compare with the parameters reversed.
    public int Compare(Unity.Mathematics.int2 a, Unity.Mathematics.int2 b) {
        int dx = (a.x - b.x);
        return (dx != 0) ? dx : a.y - b.y;
    }

}

