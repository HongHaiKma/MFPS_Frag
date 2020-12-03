using UnityEngine;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;

public abstract class bl_FlagBase : MonoBehaviour
{


    public abstract bool CanBePickedUpBy(bl_PlayerSettings logic);

    public abstract void OnPickup(bl_PlayerSettings logic);

    PhotonView m_PhotonView;

    protected PhotonView PhotonView
    {
        get
        {
            if (m_PhotonView == null)
            {
                m_PhotonView = PhotonView.Get(this);
            }

            return m_PhotonView;
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag(bl_PlayerSettings.LocalTag))
        {
            bl_PlayerSettings logic = collider.gameObject.GetComponent<bl_PlayerSettings>();
            if (CanBePickedUpBy(logic) == true)
            {
                PickupObject(logic);
            }
        }
    }

    void PickupObject(bl_PlayerSettings logic)
    {
        PhotonView.RPC("OnPickup", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer, logic.photonView.ViewID);
    }

    [PunRPC]
    protected void OnPickup(Player m_actor, int m_view)
    {
        PhotonView view = PhotonView.Find(m_view);
        if (view != null)
        {
            bl_PlayerSettings logic = view.GetComponent<bl_PlayerSettings>();
            if (CanBePickedUpBy(logic) == true)
            {
                OnPickup(logic);
                if (PhotonNetwork.LocalPlayer == m_actor)
                {
                    bool t_send = false;//Prevent call two or more events
                    if (!t_send)
                    {
                        t_send = true;
                        Team oponentTeam;
                        if ((string)PhotonNetwork.LocalPlayer.CustomProperties[PropertiesKeys.TeamKey] == Team.Delta.ToString())
                        {
                            oponentTeam = Team.Recon;
                        }
                        else
                        {
                            oponentTeam = Team.Delta;
                        }
                        string obtainedText = string.Format(bl_GameTexts.ObtainedFlag, oponentTeam.ToString());
                        bl_EventHandler.KillEvent(PhotonNetwork.NickName, string.Empty, obtainedText, (string)PhotonNetwork.LocalPlayer.CustomProperties[PropertiesKeys.TeamKey], 777, 15);

                    }
                }
            }
        }

    }
}