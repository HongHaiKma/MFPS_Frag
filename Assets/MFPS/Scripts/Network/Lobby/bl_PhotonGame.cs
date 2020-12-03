using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable; //Replace default Hashtables with Photon hashtables
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class bl_PhotonGame : bl_PhotonHelper
{

    [HideInInspector] public bool hasPingKick = false;
    public bool hasAFKKick { get; set; }
    static readonly RaiseEventOptions EventsAll = new RaiseEventOptions();

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        EventsAll.Receivers = ReceiverGroup.All;
        PhotonNetwork.NetworkingClient.EventReceived += OnEventCustom;
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnEventCustom(EventData data)
    {
        switch (data.Code)
        {
            case PropertiesKeys.KickPlayerEvent:
                OnKick();
                break;
            case 1:

                break;
        }
    }

    public void OnPingKick()
    {
        hasPingKick = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public void KickPlayer(Player p)
    {
        SendOptions so = new SendOptions();
        PhotonNetwork.RaiseEvent(PropertiesKeys.KickPlayerEvent, null, new RaiseEventOptions() { TargetActors = new int[] { p.ActorNumber } }, so);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnKick()
    {
        if (PhotonNetwork.InRoom)
        {
            PlayerPrefs.SetInt(PropertiesKeys.KickKey, 1);
            PhotonNetwork.LeaveRoom();
        }
    }

    private static bl_PhotonGame _instance;
    public static bl_PhotonGame Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_PhotonGame>(); }
            return _instance;
        }
    }
}