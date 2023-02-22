using UnityEngine;

public interface SnapListener
{
    public void onSnap(Transform snapped);
    public void onUnsnap();
}
