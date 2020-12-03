using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_CameraRay : bl_MonoBehaviour
{
    public int CheckFrameRate = 5;
    [Range(0.1f, 10)] public float DistanceCheck = 2;
    public LayerMask DetectLayers;

    private int currentFrame = 0;
    private RaycastHit RayHit;
    private bl_GunPickUp gunPickup = null;
    public bool Checking { get; set; }
    private List<byte> activers = new List<byte>();

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (!Checking) { currentFrame = 0; return; }

        if(currentFrame == 0)
        {
            Fire();
        }
        currentFrame = (currentFrame + 1) % CheckFrameRate;
    }

    /// <summary>
    /// 
    /// </summary>
    void Fire()
    {
        Ray r = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(r, out RayHit, DistanceCheck, DetectLayers, QueryTriggerInteraction.Ignore))
        {
            OnHit();
        }
        else
        {
            if (gunPickup != null) { gunPickup.FocusThis(false); gunPickup = null; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnHit()
    {
        bl_GunPickUp gp = RayHit.transform.GetComponent<bl_GunPickUp>();
        if (gp != null)
        {
            if (gunPickup != null && gunPickup != gp)
            {
                gunPickup.FocusThis(false);
            }
            gunPickup = gp;
            gunPickup.FocusThis(true);
        }
        else
        {
            if (gunPickup != null) { gunPickup.FocusThis(false); gunPickup = null; }
        }
    }

    public void SetActiver(bool add, byte id)
    {
        if (add)
        {
            if (!activers.Contains(id))
            {
                activers.Add(id);
            }
            Checking = true;
        }
        else
        {
            if (activers.Contains(id))
            {
                activers.Remove(id);
            }
            if (activers.Count <= 0) Checking = false;
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawRay(transform.position, transform.forward * DistanceCheck);
    }
}