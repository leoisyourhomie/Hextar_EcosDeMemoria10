using UnityEngine;

public class WorldTrackerObject : MonoBehaviour, IARTarget
{
    private void OnValidate()
    {
        this.transform.hideFlags = HideFlags.NotEditable;
    }

    private void OnDrawGizmosSelected()
    {
        this.transform.position = Vector3.zero;
        this.transform.rotation = Quaternion.identity;
        this.transform.localScale = Vector3.one;
    }
}
