﻿//////////////////////////////////////////////////////////////////
//bl_PhotonHelper.cs
//
//This a simple base class
//to us serve as an extension of Photon.Monobehaviour default
//                   Lovatto Studio
//////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

public class bl_PhotonHelper : MonoBehaviourPun
{

    protected GameMode mGameMode = GameMode.FFA;
    private List<Player> PlayerList = new List<Player>();
    private bool GameModeDownloaded = false;

    /// <summary>
    /// 
    /// </summary>
    public string myTeam
    {
        get
        {
            string t = (string)PhotonNetwork.LocalPlayer.CustomProperties[PropertiesKeys.TeamKey];
            return t;
        }
    }

    public bool isRoomReady
    {
        get { return (PhotonNetwork.IsConnected && PhotonNetwork.InRoom); }
    }

    /// <summary>
    /// Find a player gameobject by the viewID 
    /// </summary>
    /// <returns></returns>
    public GameObject FindPlayerRoot(int view)
    {
        PhotonView m_view = PhotonView.Find(view);

        if (m_view != null)
        {
            return m_view.gameObject;
        }
        else
        {
            return null;
        }
    }
    /// <summary>
    ///  get a photonView by the viewID
    /// </summary>
    /// <param name="view"></param>
    /// <returns></returns>
    public PhotonView FindPlayerView(int view)
    {
        PhotonView m_view = PhotonView.Find(view);

        if (m_view != null)
        {
            return m_view;
        }
        else
        {
            return null;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    public PhotonView GetPhotonView(GameObject go)
    {
        PhotonView view = go.GetComponent<PhotonView>();
        if (view == null)
        {
            view = go.GetComponentInChildren<PhotonView>();
        }
        return view;
    }
    /// <summary>
    /// 
    /// </summary>
    public Transform Root
    {
        get
        {
            return transform.root;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public Transform Parent
    {
        get
        {
            return transform.parent;
        }
    }

    /// <summary>
    /// True if the PhotonView is "mine" and can be controlled by this client.
    /// </summary>
    /// <remarks>
    /// PUN has an ownership concept that defines who can control and destroy each PhotonView.
    /// True in case the owner matches the local PhotonPlayer.
    /// True if this is a scene photon view on the Master client.
    /// </remarks>
    public bool isMine
    {
        get
        {
            return photonView.IsMine;
        }
    }
    /// <summary>
    /// Get Photon.connect
    /// </summary>
    public bool isConnected
    {
        get
        {
            return PhotonNetwork.IsConnected;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public GameObject FindPhotonPlayer(Player p)
    {
        GameObject player = GameObject.Find(p.NickName);
        if (player == null)
        {
            return null;
        }
        return player;
    }

    /// <summary>
    /// Get the team of players
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public string GetTeam(Player p)
    {
        if (p == null || !isConnected)
            return null;

        return (string)p.CustomProperties[PropertiesKeys.TeamKey];
    }

    /// <summary>
    /// Get the team of players
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public Team GetTeamEnum(Player p)
    {
        if (p == null || !isConnected)
            return Team.All;

        string t = (string)p.CustomProperties[PropertiesKeys.TeamKey];

        switch (t)
        {
            case "Recon":
                return Team.Recon;
            case "Delta":
                return Team.Delta;
        }
        return Team.All;
    }

    /// <summary>
    /// Get current gamemode
    /// </summary>
    public GameMode GetGameMode
    {
        get
        {
            if (!isConnected || !PhotonNetwork.InRoom)
                return GameMode.FFA;

            if (!GameModeDownloaded)
            {
                GameMode[] result = (GameMode[])System.Enum.GetValues(typeof(GameMode));
                string gm = (string)PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.GameModeKey];
                for (int i = 0; i < result.Length; i++)
                {
                    if (result[i].ToString() == gm)
                    {
                        mGameMode = result[i];
                        GameModeDownloaded = true;
                    }
                }
            }

            return mGameMode;
        }
    }

    public string LocalName
    {
        get
        {
            if (PhotonNetwork.LocalPlayer != null && isConnected)
            {
                return PhotonNetwork.LocalPlayer.NickName;
            }
            else
            {
                return "None";
            }
        }
    }

    /// <summary>
    /// Get All Player in Room of a specific team
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public List<Player> GetPlayersInTeam(string team)
    {
        PlayerList.Clear();
        PlayerList = new List<Player>();

        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            if ((string)players[i].CustomProperties[PropertiesKeys.TeamKey] == team)
            {
                PlayerList.Add(players[i]);
            }
        }
        return PlayerList;
    }

    /// <summary>
    /// is the a one team game mode?
    /// </summary>
    public bool isOneTeamMode
    {
        get
        {
            bool b = false;
            GameMode m = GetGameMode;
            if (m == GameMode.FFA) { b = true; }
#if GR
            if (m == GameMode.GR) { b = true; }
#endif
#if LMS
            if (m == GameMode.LSM) { b = true; }
#endif
            return b;
        }
    }
}