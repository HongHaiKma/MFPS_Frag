using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System;

public class bl_PhotonCallbacks : MonoBehaviourPunCallbacks
{
    public delegate void EEvent();

    public delegate void EPlayerPropertiesUpdate(Player target, Hashtable changedProps);
    public static EPlayerPropertiesUpdate PlayerPropertiesUpdate;

    public delegate void EPlayerLeftRoom(Player otherPlayer);
    public static EPlayerLeftRoom PlayerLeftRoom;

    public delegate void EPlayerEnteredRoom(Player newPlayer);
    public static EPlayerEnteredRoom PlayerEnteredRoom;

    public delegate void ERoomPropertiesUpdate(Hashtable propertiesThatChanged);
    public static ERoomPropertiesUpdate RoomPropertiesUpdate;

    public static EEvent LeftRoom;

    //PLAYER CALLBACKS
    public override void OnPlayerPropertiesUpdate(Player target, Hashtable changedProps)
    {
       if(PlayerPropertiesUpdate != null)
        {
            PlayerPropertiesUpdate.Invoke(target, changedProps);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PlayerLeftRoom != null)
        {
            PlayerLeftRoom.Invoke(otherPlayer);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PlayerEnteredRoom != null)
        {
            PlayerEnteredRoom.Invoke(newPlayer);
        }
    }
    //PLAYER CALLBACKS

    //ROOM CALLBACKS
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (RoomPropertiesUpdate != null)
        {
            RoomPropertiesUpdate.Invoke(propertiesThatChanged);
        }
    }
    //ROOM CALLBACKS

    //MATCHMAKING CALLBACKS
    public override void OnLeftRoom()
    {
       if(LeftRoom != null)
        {
            LeftRoom.Invoke();
        }
    }
    //MATCHMAKING CALLBACKS
}