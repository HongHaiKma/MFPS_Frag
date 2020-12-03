///////////////////////////////////////////////////////////////////////////////////////
// bl_BodyPart.cs
//
// This script receives the information of the damage done by another player
// place it on a gameobject containing a collider in the hierarchy of the remote player
// use "bl_BodyPartManager.cs" to automatically configure                            
//                                 Lovatto Studio
///////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

public class bl_BodyPart : MonoBehaviour {

    /// <summary>
    /// so far as the damage is multiplied in this part
    /// </summary>
    public float multiplier = 1f;
    public bl_PlayerDamageManager HealtScript;
    /// <summary>
    /// Its this the heat? if yes them is head shot :)
    /// </summary>
    public bool TakeHeatShot = false;

    /// <summary>
    /// Use this for receive damage local and sync for all other
    /// </summary>
    public void GetDamage(float damage, string t_from, DamageCause cause, Vector3 direction, int weapon_ID = 0)
    {
        float m_TotalDamage = damage + multiplier;

        DamageData e = new DamageData();
        e.Damage = m_TotalDamage;
        e.Direction = direction;
        e.Cause = cause;
        e.isHeadShot = TakeHeatShot;
        e.GunID = weapon_ID;
        e.From = t_from;

        if (HealtScript != null)
        {
            HealtScript.GetDamage(e);
        }
    }
}
