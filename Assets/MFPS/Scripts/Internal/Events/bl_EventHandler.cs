/////////////////////////////////////////////////////////////////////////////////
//////////////////// bl_EventHandler.cs/////////////////////////////////////////
////////////////////Use this to create new internal events///////////////////////
//this helps to improve the communication of the script through delegated events/
////////////////////////////////Lovatto Studio////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System;

public class bl_EventHandler
{
    //Call all script when Fall Events
    public delegate void FallEvent(float m_amount);
    public static FallEvent OnFall;
    //Call all script when Kill Events
    public delegate void KillFeedEvent(string kl, string kd, string hw, string m_team, int gid, int hs);
    public static KillFeedEvent OnKillFeed;
    //Item EventPick Up
    public delegate void ItemsPickUp(int Amount);
    public static ItemsPickUp OnPickUp;
    //Call new Kit Air 
    public delegate void KitAir(Vector3 m_position, int type);
    public static KitAir OnKitAir;
    //Pick Up Ammo
    public delegate void AmmoKit(int bullets, int Clips, int projectiles);
    public static AmmoKit OnKitAmmo;
    //On Kill Event
    public delegate void NewKill(bl_KillFeed.LocalKillInfo localKill);
    public static NewKill OnKill;
    //On Round End
    public delegate void RoundEnd();
    public static RoundEnd OnRoundEnd;
    //small impact
    public delegate void SmallImpact();
    public static SmallImpact OnSmallImpact;
    //Receive Damage
    public delegate void GetDamage(DamageData e);
    public static GetDamage OnDamage;
    //When Local Player Death
    public delegate void LocalPlayerDeath();
    public static LocalPlayerDeath OnLocalPlayerDeath;
    //When Local Player is Instantiate
    public delegate void LocalPlayerSpawn();
    public static LocalPlayerSpawn OnLocalPlayerSpawn;
    //When Local Player is Instantiate
    public delegate void LocalPlayerShake(float amount = 0.2f, float duration = 0.4f, float intense = 0.25f, bool aim = false);
    public static LocalPlayerShake OnLocalPlayerShake;

    public delegate void EffectChange(bool chrab,bool anti,bool bloom, bool ssao, bool motionb);
    public static EffectChange OnEffectChange;

    public delegate void PickUpWeapon(bl_OnPickUpInfo e);
    public static PickUpWeapon OnPickUpGun;

    public delegate void ChangeWeapon(int GunID);
    public static ChangeWeapon OnChangeWeapon;

    public static Action<Transform, bool, bool> RemoteActorsChange;

    /// <summary>
    /// Called when the LOCAL player change of weapon
    /// </summary>
    public static void ChangeWeaponEvent(int GunID)
    {
        if (OnChangeWeapon != null)
            OnChangeWeapon(GunID);
    }

    /// <summary>
    /// Called event when recive Fall Impact
    /// </summary>
    public static void EventFall(float m_amount)
    {
        if (OnFall != null)
            OnFall(m_amount);
    }

    /// <summary>
    /// Event Called when receive a new kill feed message
    /// </summary>
    public static void KillEvent(string kl, string kd, string hw, string t_team, int gid, int hs)
    {
        if (OnKillFeed != null)
            OnKillFeed(kl, kd, hw, t_team, gid, hs);
    }
    /// <summary>
    /// Called event when pick up a med kit
    /// </summary>
    public static void PickUpEvent(int t_amount)
    {
        if (OnPickUp != null)
            OnPickUp(t_amount);
    }
    /// <summary>
    /// Called event when call a new kit 
    /// </summary>
    /// <param name="t_position">position where kit appear</param>
    /// <param name="type"></param>
    public static void KitAirEvent(Vector3 t_position, int type)
    {
        if (OnKitAir != null)
            OnKitAir(t_position, type);
    }
    /// <summary>
    /// Called Event when pick up ammo
    /// </summary>
    /// <param name="clips">number of clips</param>
    public static void OnAmmo(int bullets, int clips, int projectiles)
    {
        if (OnKitAmmo != null)
            OnKitAmmo(bullets, clips, projectiles);
    }
    /// <summary>
    /// Called this when killed a new player
    /// </summary>
    /// <param name="t_amount"></param>
    public static void OnKillEvent(bl_KillFeed.LocalKillInfo localKill)
    {
        if (OnKill != null)
            OnKill(localKill);
    }
    /// <summary>
    /// Call This when room is finish a round
    /// </summary>
    public static void OnRoundEndEvent()
    {
        if (OnRoundEnd != null)
            OnRoundEnd();
    }
    /// <summary>
    /// 
    /// </summary>
    public static void OnSmallImpactEvent()
    {
        if (OnSmallImpact != null)
            OnSmallImpact();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    public static void eDamage(DamageData e)
    {
        if (OnDamage != null)
            OnDamage(e);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    public static void PlayerLocalDeathEvent()
    {
        if (OnLocalPlayerDeath != null)
        {
            OnLocalPlayerDeath();
        }
    }

    public static void PlayerLocalSpawnEvent()
    {
        if (OnLocalPlayerSpawn != null)
        {
            OnLocalPlayerSpawn();
        }
    }

    public static void PlayerLocalShakeEvent(float amount = 0.2f, float duration = 0.4f, float intense = 0.25f, bool aim = false)
    {
        if (OnLocalPlayerShake != null)
        {
            OnLocalPlayerShake(amount, duration, intense, aim);
        }
    }

    public static void PlayerLocalSpawnEvent(bool chrab, bool anti, bool bloom, bool ssao, bool motionBlur)
    {
        if (OnEffectChange != null)
        {
            OnEffectChange(chrab, anti, bloom, ssao, motionBlur);
        }
    }

    public static void ePickUpGun(bl_OnPickUpInfo e)
    {
        if (OnPickUpGun != null)
            OnPickUpGun(e);
    }

    public static void OnRemoteActorChange(Transform actor, bool spawning, bool isRealPlayer)
    {
        if(RemoteActorsChange != null)
        {
            RemoteActorsChange.Invoke(actor, spawning, isRealPlayer);
        }
    }

    public static void SetEffectChange(bool chrab, bool anti, bool bloom, bool ssao, bool motionb)
    {
        if (OnEffectChange != null)
            OnEffectChange(chrab, anti, bloom, ssao, motionb);
    }
}