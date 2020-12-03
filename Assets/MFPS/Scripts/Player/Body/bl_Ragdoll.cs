using UnityEngine;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using System;

[Obsolete("This script is not longer use, local ragdoll is handle in bl_BodyPartManager.cs")]
public class bl_Ragdoll : MonoBehaviour
{
    [Header("Settings")]
    public float m_ForceFactor = 1f;
    [Header("References")]
    public bl_KillCam KillCam;
    public Transform RightHand;

    private Rigidbody[] m_Rigidbodies;
    private Vector3 m_velocity = Vector3.zero;
    private bl_GameManager m_manager;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        m_manager = bl_GameManager.Instance;
        this.Init();
    }

    protected void Init()
    {
        m_Rigidbodies = this.transform.GetComponentsInChildren<Rigidbody>();
        ChangeRagdoll(true);
    }

    public void ChangeRagdoll(bool m)
    {
        foreach (Rigidbody rigidbody in this.m_Rigidbodies)
        {
            rigidbody.isKinematic = !m;
            if (m)
            {
                rigidbody.AddForce((Time.deltaTime <= 0f) ? Vector3.zero : (((m_velocity / Time.deltaTime) * this.m_ForceFactor)), ForceMode.Impulse);
            }
        }
    }

    public void RespawnAfter(string killer, Transform netRoot, Player view, int gundID, DamageCause cause)
    {
        KillCam.enabled = true;
        KillCam.SetTarget(view, cause, killer);

        bl_UIReferences.Instance.OnKillCam(true, killer, gundID);
        if (RightHand != null && netRoot != null)
        {
            Vector3 RootPos = netRoot.localPosition;
            netRoot.parent = RightHand;
            netRoot.localPosition = RootPos;
        }
#if ELIM
       if(bl_RoomSettings.Instance.m_GameMode == GameMode.ELIM)
        {
            bl_Elimination.Instance.OnLocalDeath(KillCam);
            bl_UIReferences.Instance.OnKillCam(false);
            Destroy(gameObject, 5);
            return;
        }
#endif
        StartCoroutine(Wait(bl_GameData.Instance.PlayerRespawnTime));
    }

    IEnumerator Wait(float t_time)
    {
        float t = t_time / 3;
        yield return new WaitForSeconds(t * 2);
        if (!bl_RoomMenu.Instance.isFinish)
        {
            StartCoroutine(bl_UIReferences.Instance.FinalFade(true, false));
        }
        yield return new WaitForSeconds(t);

        m_manager.SpawnPlayer(PhotonNetwork.LocalPlayer.GetPlayerTeam());
        Destroy(KillCam.gameObject);
        bl_UIReferences.Instance.OnKillCam(false);
        Destroy(gameObject);
    }

    public void GetVelocity(Vector3 m_vel)
    {
        m_velocity = m_vel;
    }
}