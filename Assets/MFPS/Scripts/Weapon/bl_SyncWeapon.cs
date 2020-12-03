/////////////////////////////////////////////////////////////////////////////////
//////////////////////////////bl_SyncWeapon.cs///////////////////////////////////
///////////////use this to synchronize when the gun is firing////////////////////
//
////////////////////////////////Lovatto Studio////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

public class bl_SyncWeapon : MonoBehaviour
{

    private bl_PlayerSync m_sync;

    void Awake()
    {
        m_sync = transform.root.GetComponent<bl_PlayerSync>();
    }

    /// <summary>
    /// Sync Fire 
    /// </summary>
    public void Firing(GunType m_type, Vector3 hitPosition)
    {
        if (m_sync)
        {
            m_sync.IsFire(m_type, hitPosition);
        }
    }

    /// <summary>
    /// Sync Fire 
    /// </summary>
    public void FiringGrenade(float m_spread, Vector3 position, Quaternion rot, Vector3 angular)
    {
        if (m_sync)
        {
            m_sync.IsFireGrenade(m_spread, position, rot, angular);
        }
    }

    public void SyncOffAmmoGrenade(bool active)
    {
        m_sync.SetActiveGrenade(active);
    }

    public void SetBlockWeapon(bool isBlock)
    {
        m_sync.SetWeaponBlocked(isBlock);
    }
}