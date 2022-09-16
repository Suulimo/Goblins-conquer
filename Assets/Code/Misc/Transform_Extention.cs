using UnityEngine;

public static class Transform_Extension
{
    public static void SetLocalPosX(this Transform transform, float val) {
        transform.localPosition = new Vector3(val, transform.localPosition.y, transform.localPosition.z);
    }
    public static void SetLocalPosY(this Transform transform, float val) {
        transform.localPosition = new Vector3(transform.localPosition.x, val, transform.localPosition.z);
    }
    public static void SetLocalPosZ(this Transform transform, float val) {
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, val);
    }
    public static void SetLocalScaleX(this Transform transform, float val) {
        transform.localScale = new Vector3(val, transform.localScale.y, transform.localScale.z);
    }
    public static void SetLocalScaleY(this Transform transform, float val) {
        transform.localScale = new Vector3(transform.localScale.x, val, transform.localScale.z);
    }
    public static void SetLocalScaleZ(this Transform transform, float val) {
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, val);
    }

}

public static class RectTransform_Extension
{
    public static void SetAnchorPosX(this RectTransform transform, float val) {
        transform.anchoredPosition = new Vector2(val, transform.anchoredPosition.y);
    }
    public static void SetAnchorPosY(this RectTransform transform, float val) {
        transform.anchoredPosition = new Vector2(transform.anchoredPosition.x, val);
    }
}
