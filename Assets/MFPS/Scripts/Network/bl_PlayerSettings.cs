////////////////////////////////////////////////////////////////////////////////
// bl_PlayerSettings.cs
//
// This script configures the required settings for the local and remote player
//
//                        Lovatto Studio
////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
public class bl_PlayerSettings : bl_PhotonHelper
{

    public const string LocalTag = "Player";
    public const string RemoteTag = "Remote";

    public Team m_Team = Team.All;
    public List<MonoBehaviour> RemoteOnlyScripts = new List<MonoBehaviour>();
    public List<MonoBehaviour> LocalOnlyScripts = new List<MonoBehaviour>();
    [Space(5)]
    public GameObject LocalObjects;
    public GameObject RemoteObjects;
    [Header("Player References")]
    public Camera PlayerCamera;
    public Transform FlagPosition;

    [Header("Hands Textures")]
    public HandsLocal_ m_hands;

    
    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        if (isMine || !PhotonNetwork.InRoom)
        {
            LocalPlayer();
        }
        else
        {
            RemotePlayer();
        }
    }

    /// <summary>
    /// We call this function only if we are Remote player
    /// </summary>
    public void RemotePlayer()
    {
        for (int i = 0; i < LocalOnlyScripts.Count; i++)
        {
            if (LocalOnlyScripts[i] != null)
            {
                 LocalOnlyScripts[i].enabled = false;
            }
        }
        LocalObjects.SetActive(false);
        m_Team = photonView.Owner.GetPlayerTeam();
        gameObject.tag = RemoteTag;
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        bl_EventHandler.OnRemoteActorChange(transform, true, true);

    }
    /// <summary>
    /// We call this function only if we are Local player
    /// </summary>
    public void LocalPlayer()
    {
        gameObject.name = PhotonNetwork.NickName;
        if (myTeam == Team.Delta.ToString())
        {
            m_Team = Team.Delta;
            if (m_hands.SlevesMat != null)
            {
                m_hands.SlevesMat.mainTexture = m_hands.SlevesTeam1;
            }
            if (m_hands.GlovesMat != null)
            {
                m_hands.GlovesMat.mainTexture = m_hands.GlovesTeam1;
            }
        }
        else if (myTeam == Team.Recon.ToString())
        {
            m_Team = Team.Recon;
            if (m_hands.SlevesMat != null)
            {
                m_hands.SlevesMat.mainTexture = m_hands.SlevesTeam2;
            }
            if (m_hands.GlovesMat != null)
            {
                m_hands.GlovesMat.mainTexture = m_hands.GlovesTeam2;
            }
        }
        else
        {
            m_Team = Team.All;
            if (m_hands.SlevesMat != null)
            {
                m_hands.SlevesMat.mainTexture = m_hands.SlevesTeam2;
            }
            if (m_hands.GlovesMat != null)
            {
                m_hands.GlovesMat.mainTexture = m_hands.GlovesTeam2;
            }
        }
        if (m_hands.GlovesMat != null && m_hands.GlovesMat.HasProperty("_Color")
            && m_hands.SlevesMat != null && m_hands.SlevesMat.HasProperty("_Color") && m_hands.useEffect)
        {
            StartCoroutine(StartEffect());
        }
        for (int i = 0; i < RemoteOnlyScripts.Count; i++)
        {
            if (RemoteOnlyScripts[i] != null)
            {
                RemoteOnlyScripts[i].enabled = false;
            }
        }
        RemoteObjects.SetActive(false);
        gameObject.tag = LocalTag;
#if GR
        transform.GetComponentInChildren<bl_GunManager>().isGunRace = (GetGameMode == GameMode.GR);
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDestroy()
    {
        bl_EventHandler.OnRemoteActorChange(transform, false, true);
    }

    /// <summary>
    /// produce an effect of spawn
    /// with a loop 
    /// </summary>
    /// <returns></returns>
    IEnumerator StartEffect()
    {
        int loops = 8;// number of repeats
        for (int i = 0; i < loops; i++)
        {
            yield return new WaitForSeconds(0.25f);
            m_hands.GlovesMat.color = m_hands.mBettewColor;
            m_hands.SlevesMat.color = m_hands.mBettewColor;
            yield return new WaitForSeconds(0.25f);
            m_hands.GlovesMat.color = m_hands.HandsInitColor;
            m_hands.SlevesMat.color = m_hands.HandsInitColor;

        }
    }

    public bool isLocal { get { return photonView.IsMine; } }


    [System.Serializable]
    public class HandsLocal_
    {
        public Material SlevesMat;
        public Material GlovesMat;
        [Space(5)]
        public Texture2D SlevesTeam1;
        public Texture2D GlovesTeam1;
        [Space(5)]
        public Texture2D SlevesTeam2;
        public Texture2D GlovesTeam2;
        [Space(5)]
        public bool useEffect = true;
        public Color HandsInitColor = new Color(1, 1, 1, 1);
        public Color mBettewColor = new Color(0.1f, 0.1f, 1, 1);
        public GameObject AimPositionReference;
    }
}