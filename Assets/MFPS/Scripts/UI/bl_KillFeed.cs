using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

public class bl_KillFeed : bl_PhotonHelper
{
    public LocalKillDisplay m_LocalKillDisplay = LocalKillDisplay.Individual;
    [Range(1, 7)] public float IndividualShowTime = 3;
    public Color SelfColor = Color.green;
    //private
    private bl_RoomSettings setting;
    private bl_UIReferences UIReference;
    private List<LocalKillInfo> localKillsQueque = new List<LocalKillInfo>();

#if LOCALIZATION
    private int[] LocaleTextIDs = new int[] { 28,17, };
    private string[] LocaleStrings;
#endif
    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        setting = this.GetComponent<bl_RoomSettings>();
        UIReference = FindObjectOfType<bl_UIReferences>();
        if (PhotonNetwork.InRoom)
        {
#if LOCALIZATION
            LocaleStrings = bl_Localization.Instance.GetTextArray(LocaleTextIDs);
#endif
            OnJoined();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        bl_EventHandler.OnKillFeed += this.OnKillFeed;
        bl_EventHandler.OnKill += this.NewLocalKill;
        bl_PhotonCallbacks.PlayerLeftRoom += OnPhotonPlayerDisconnected;
    }
    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        bl_EventHandler.OnKillFeed -= this.OnKillFeed;
        bl_EventHandler.OnKill -= this.NewLocalKill;
        bl_PhotonCallbacks.PlayerLeftRoom -= OnPhotonPlayerDisconnected;
    }

    /// <summary>
    /// Called this when a new kill event 
    /// </summary>
    public void OnKillFeed(string t_Killer, string t_Killed, string t_HowKill, string t_team, int t_GunID, int isHeatShot)
    {
        photonView.RPC("AddNewKillFeed", RpcTarget.All, t_Killer, t_Killed, t_HowKill, t_team.ToString(), t_GunID, isHeatShot);
    }
    /// <summary>
    /// Player Joined? sync
    /// </summary>
    void OnJoined()
    {
#if LOCALIZATION
        photonView.RPC("AddNewKillFeed", RpcTarget.Others, PhotonNetwork.NickName, LocaleStrings[1], string.Empty, string.Empty, 777, 20);
#else
        photonView.RPC("AddNewKillFeed", RpcTarget.Others, PhotonNetwork.NickName, bl_GameTexts.JoinedInMatch, string.Empty, string.Empty, 777, 20);
#endif
    }

    [PunRPC]
    void AddNewKillFeed(string t_Killer, string t_Killed, string t_HowKill, string m_team, int t_GunID, int isHeatShot)
    {
        Color KillerColor = new Color(1, 1, 1, 1);
        Color KilledColor = new Color(1, 1, 1, 1);
        if (setting.m_GameMode != GameMode.FFA)
        {
            if (m_team == Team.Delta.ToString())
            {
                KillerColor = isMy(t_Killer) ? SelfColor : bl_GameData.Instance.Team1Color;
                KilledColor = isMy(t_Killed) ? SelfColor : bl_GameData.Instance.Team2Color;
            }
            else if (m_team == Team.Recon.ToString())
            {
                KillerColor = isMy(t_Killer) ? SelfColor : bl_GameData.Instance.Team2Color;
                KilledColor = isMy(t_Killed) ? SelfColor : bl_GameData.Instance.Team1Color;
            }
            else
            {
                KilledColor = Color.white;
                KillerColor = Color.white;
            }
        }

#if LOCALIZATION
        if (string.IsNullOrEmpty(t_Killer)) { t_Killer = LocaleStrings[0]; }
#else
        if (string.IsNullOrEmpty(t_Killer)) { t_Killer = bl_GameTexts.ServerMesagge; }
#endif

        KillFeed newKillFeed = new KillFeed();
        newKillFeed.Killer = t_Killer;
        newKillFeed.Killed = t_Killed;
        newKillFeed.HowKill = t_HowKill;
        newKillFeed.KilledColor = KilledColor;
        newKillFeed.KillerColor = KillerColor;
        newKillFeed.HeatShot = (isHeatShot == 1) ? true : false;
        newKillFeed.GunID = t_GunID;

        UIReference.SetKillFeed(newKillFeed);
    }

    public void OnPhotonPlayerDisconnected(Player otherPlayer)
    {
#if LOCALIZATION
        AddNewKillFeed(otherPlayer.NickName, string.Empty, bl_Localization.Instance.GetText(18), string.Empty, 777, 20);
#else
        AddNewKillFeed(otherPlayer.NickName, string.Empty, bl_GameTexts.LeftOfMatch, string.Empty, 777, 20);
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    private bool isMy(string n)
    {
        bool b = false;
        if (n == LocalName)
        {
            b = true;
        }
        return b;
    }

    /// <summary>
    /// Show a local ui when out killed other player
    /// </summary>
    private void NewLocalKill(LocalKillInfo localKill)
    {
        if (localKillsQueque.Count <= 0)
        {
            bl_UIReferences.Instance.SetLocalKillFeed(localKill, m_LocalKillDisplay);
        }
        localKillsQueque.Add(localKill);
    }

    /// <summary>
    /// 
    /// </summary>
    public void LocalDisplayDone()
    {
        localKillsQueque.RemoveAt(0);
        if(localKillsQueque.Count > 0)
        {
            bl_UIReferences.Instance.SetLocalKillFeed(localKillsQueque[0], m_LocalKillDisplay);
        }
    }

    [System.Serializable]
    public class LocalKillInfo
    {
        public string Killed;
        public string Weapon;
        public bool HeadShot = false;
        public int Score;
    }

    [System.Serializable]
    public enum LocalKillDisplay
    {
        Individual,
        Multiple,
    }
}