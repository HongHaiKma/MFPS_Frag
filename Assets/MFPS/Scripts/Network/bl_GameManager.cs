/////////////////////////////////////////////////////////////////////////////////
//////////////////////////////bl_GameManager.cs//////////////////////////////////
/////////////////place this in a scene for Spawn Players in Room/////////////////
/////////////////////////////////////////////////////////////////////////////////
////////////////////////////////Briner Games/////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable; //Replace default Hashtables with Photon hashtables
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class bl_GameManager : bl_PhotonHelper, IInRoomCallbacks, IConnectionCallbacks {

    public static int m_view = -1;
    public static bool isAlive = false;
    public static int SuicideCount = 0;
    public static bool Joined = false;
    public int Headshots { get; set; }
    [HideInInspector]
    public GameObject OurPlayer;
    [Header("References")]

    /// <summary>
    /// Camera Preview
    /// </summary>
    public Camera m_RoomCamera;
    public bool DrawSpawnPoints = true;
    public Mesh SpawnPointPlayerGizmo;
    [HideInInspector]public List<Transform> AllSpawnPoints = new List<Transform>();
    private List<Transform> ReconSpawnPoint = new List<Transform>();
    private List<Transform> DeltaSpawnPoint = new List<Transform>();
    private int currentReconSpawnPoint = 0;
    private int currentDeltaSpawnPoint = 0;
    /// <summary>
    /// List with all Players in Current Room
    /// </summary>
    public List<Player> connectedPlayerList = new List<Player>();
    private bool EnterInGamePlay = false;
    [HideInInspector] public bool GameFinish = false;
    [HideInInspector] public List<SceneActors> OthersActorsInScene = new List<SceneActors>();
    private SceneActors LocalActor = new SceneActors();

    public MatchState GameMatchState;
    public Team LocalPlayerTeam { get; set; }
    private int WaitingPlayersAmount = 1;
    private float StartPlayTime;
    private Camera cameraRender = null;
    public Camera CameraRendered
    {
        get
        {
            if(cameraRender == null)
            {
               // Debug.Log("Not Camera has been setup.");
                return Camera.current;
            }
            return cameraRender;
        }
        set
        {
            if(cameraRender != null && cameraRender.isActiveAndEnabled)
            {
                //if the current render over the set camera, keep it as renderer camera
                if (cameraRender.depth >= value.depth) return;
            }
            cameraRender = value;
        }
    }
#if UMM
    private Canvas MiniMapCanvas = null;
#endif

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        PhotonNetwork.AddCallbackTarget(this);
        if (!PhotonNetwork.IsConnected)
            return;

        PhotonNetwork.IsMessageQueueRunning = true;
        Joined = false;
        SuicideCount = 0;
        StartPlayTime = Time.time;
        LocalActor.isRealPlayer = true;
        bl_UCrosshair.Instance.Show(false);
#if UMM
        MiniMapCanvas = FindObjectOfType<bl_MiniMap>().m_Canvas;
        MiniMapCanvas.enabled = false;
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        bl_EventHandler.RemoteActorsChange += OnRemoteActorChange;
        bl_EventHandler.OnRoundEnd += OnGameFinish;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        bl_EventHandler.RemoteActorsChange -= OnRemoteActorChange;
        bl_EventHandler.OnRoundEnd -= OnGameFinish;
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    /// <summary>
    /// Spawn Player Function
    /// </summary>
    /// <param name="t_team"></param>
    public void SpawnPlayer(Team t_team)
    {
        if (!bl_RoomMenu.Instance.SpectatorMode)
        {
            if (OurPlayer != null)
            {
                PhotonNetwork.Destroy(OurPlayer);
            }
            if (!GameFinish)
            {
                Hashtable PlayerTeam = new Hashtable();
                PlayerTeam.Add(PropertiesKeys.TeamKey, t_team.ToString());
                PhotonNetwork.LocalPlayer.SetCustomProperties(PlayerTeam, null);
                LocalPlayerTeam = t_team;

                //spawn the player model
#if !PSELECTOR
                SpawnPlayerModel(t_team);
                m_RoomCamera.gameObject.SetActive(false);
                StartCoroutine(bl_UIReferences.Instance.FinalFade(false, false, 0));
                bl_UtilityHelper.LockCursor(true);
                if (!Joined) { StartPlayTime = Time.time; }
                Joined = true;

#if UMM
    MiniMapCanvas.enabled = true;
#endif
#else
                bl_PlayerSelector ps = FindObjectOfType<bl_PlayerSelector>();
                if (ps.IsSelected && !ps.isChangeOfTeam)
                {
                    ps.SpawnSelected();
                }
                else
                {
                    ps.OpenSelection(t_team);
                }
#endif
            }
            else
            {
                m_RoomCamera.gameObject.SetActive(true);
            }
        }
        else
        {
            this.GetComponent<bl_RoomMenu>().WaitForSpectator = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SpawnPlayerIfAlreadyInstanced()
    {
        if (OurPlayer == null)
            return;

        Team t = PhotonNetwork.LocalPlayer.GetPlayerTeam();
        SpawnPlayer(t);
    }

    /// <summary>
    /// If Player exist, them destroy
    /// </summary>
    public void DestroyPlayer(bool ActiveCamera)
    {
        if (OurPlayer != null)
        {
            PhotonNetwork.Destroy(OurPlayer);
        }
        m_RoomCamera.gameObject.SetActive(ActiveCamera);
    } 

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public void SpawnPlayerModel(Team t_team)
    {
        Vector3 pos;
        Quaternion rot;
        if (t_team == Team.Recon)
        {
            GetSpawn(ReconSpawnPoint.ToArray(),out pos,out rot);
            OurPlayer = PhotonNetwork.Instantiate(bl_GameData.Instance.Player1.name, pos, rot, 0);
        }
        else if (t_team == Team.Delta)
        {
            GetSpawn(DeltaSpawnPoint.ToArray(), out pos, out rot);
            OurPlayer = PhotonNetwork.Instantiate(bl_GameData.Instance.Player2.name, pos, rot, 0);
        }
        else
        {
            GetSpawn(AllSpawnPoints.ToArray(), out pos, out rot);
            OurPlayer = PhotonNetwork.Instantiate(bl_GameData.Instance.Player1.name, pos, rot, 0);
        }
        LocalActor.Actor = OurPlayer.transform;
        LocalActor.ActorView = OurPlayer.GetComponent<PhotonView>();
        EnterInGamePlay = true;
        bl_EventHandler.PlayerLocalSpawnEvent();
        bl_UCrosshair.Instance.Show(true);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnGameFinish()
    {

    }

#if PSELECTOR
     /// <summary>
    /// 
    /// </summary>
    public void SpawnSelectedPlayer(MFPS.PlayerSelector.bl_PlayerSelectorInfo info,Team t_team)
    {
        Vector3 pos;
        Quaternion rot;
        if (t_team == Team.Recon)
        {
            GetSpawn(ReconSpawnPoint.ToArray(), out pos, out rot);
            OurPlayer = PhotonNetwork.Instantiate(info.Prefab.name, pos, rot, 0);
        }
        else if (t_team == Team.Delta)
        {
            GetSpawn(DeltaSpawnPoint.ToArray(), out pos, out rot);
            OurPlayer = PhotonNetwork.Instantiate(info.Prefab.name, pos, rot, 0);
        }
        else
        {
            GetSpawn(AllSpawnPoints.ToArray(), out pos, out rot);
            OurPlayer = PhotonNetwork.Instantiate(info.Prefab.name, pos, rot, 0);
        }
        LocalActor.Actor = OurPlayer.transform;
        LocalActor.ActorView = OurPlayer.GetComponent<PhotonView>();
        this.GetComponent<bl_ChatRoom>().Refresh();
        m_RoomCamera.gameObject.SetActive(false);
        StartCoroutine(bl_UIReferences.Instance.FinalFade(false, false, 0));
        bl_UtilityHelper.LockCursor(true);
     if (!Joined) { StartPlayTime = Time.time; }
        Joined = true;

        EnterInGamePlay = true;
        bl_EventHandler.PlayerLocalSpawnEvent();
        bl_UCrosshair.Instance.Show(true);

#if UMM
    MiniMapCanvas.enabled = true;
#endif
    }
#endif

    /// <summary>
    /// 
    /// </summary>
    public bool WaitForPlayers(int MinPlayers)
    {
        if (isOneTeamMode)
        {
            if (PhotonNetwork.PlayerList.Length >= MinPlayers) return false;
        }
        else
        {
            if (PhotonNetwork.PlayerList.Length >= MinPlayers)
            {
                if (bl_RoomMenu.Instance.GetPlayerInDeltaCount > 0 && bl_RoomMenu.Instance.GetPlayerInReconCount > 0)
                {
                    return false;
                }
            }
        }
        WaitingPlayersAmount = MinPlayers;
        SetGameState(MatchState.Waiting);
        return true;
    }

    public float PlayedTime
    {
        get
        {
            return Time.time - StartPlayTime;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void GetSpawn(Transform[] list, out Vector3 position, out Quaternion Rotation)
    {
       int random = Random.Range(0, list.Length);
       Vector3 s = Random.insideUnitSphere * list[random].GetComponent<bl_SpawnPoint>().SpawnSpace;
       position = list[random].position + new Vector3(s.x, 0.55f, s.z);
       Rotation = list[random].rotation;
    }

    /// <summary>
    /// 
    /// </summary>
    public void RegisterSpawnPoint(bl_SpawnPoint point)
    {
        switch (point.m_Team)
        {
            case Team.Delta:
                DeltaSpawnPoint.Add(point.transform);
                break;
            case Team.Recon:
                ReconSpawnPoint.Add(point.transform);
                break;
        }
        AllSpawnPoints.Add(point.transform);
    }

    public Transform GetAnSpawnPoint
    {
        get { return AllSpawnPoints[Random.Range(0, AllSpawnPoints.Count)]; }
    }

    public Transform GetAnTeamSpawnPoint(Team team, bool sequencial = false)
    {
        if (team == Team.Recon)
        {
            if (sequencial)
            {
                currentReconSpawnPoint = (currentReconSpawnPoint + 1) % ReconSpawnPoint.Count;
                return ReconSpawnPoint[currentReconSpawnPoint];
            }
            return ReconSpawnPoint[Random.Range(0, ReconSpawnPoint.Count)];
        }
        else if (team == Team.Delta)
        {
            if (sequencial)
            {
                currentDeltaSpawnPoint = (currentDeltaSpawnPoint + 1) % DeltaSpawnPoint.Count;
                return DeltaSpawnPoint[currentDeltaSpawnPoint];
            }
            return DeltaSpawnPoint[Random.Range(0, DeltaSpawnPoint.Count)];
        }
        else
        {
            if (sequencial)
            {
                currentReconSpawnPoint = (currentReconSpawnPoint + 1) % AllSpawnPoints.Count;
                return AllSpawnPoints[currentReconSpawnPoint];
            }
            return AllSpawnPoints[Random.Range(0, AllSpawnPoints.Count)];
        }
    }

    /// <summary>
    /// This is a event callback
    /// here is caching all 'actors' in the scene (players and bots)
    /// </summary>
    public void OnRemoteActorChange(Transform trans, bool spawning, bool isRealPlayer)
    {
        if (OthersActorsInScene.Exists(x => x.Name == trans.name))
        {
            int id = OthersActorsInScene.FindIndex(x => x.Name == trans.name);
            if (spawning)
            {
                OthersActorsInScene[id].Actor = trans;
                OthersActorsInScene[id].ActorView = trans.GetComponent<PhotonView>();
                OthersActorsInScene[id].isAlive = true;
            }
            else
            {
                OthersActorsInScene[id].isAlive = false;
            }
        }
        else
        {
            if (spawning)
            {
                SceneActors sa = new SceneActors();
                sa.Actor = trans;
                sa.ActorView = trans.GetComponent<PhotonView>();
                sa.Name = trans.name;
                sa.isRealPlayer = isRealPlayer;
                sa.isAlive = true;
                OthersActorsInScene.Add(sa);
            }
        }
    }

    /// <summary>
    /// Find a player or bot by their PhotonView ID
    /// </summary>
    /// <returns></returns>
    public Transform FindActor(int ViewID)
    {
        for (int i = 0; i < OthersActorsInScene.Count; i++)
        {
            if(OthersActorsInScene[i].ActorView != null && OthersActorsInScene[i].ActorView.ViewID == ViewID) 
            {
                return OthersActorsInScene[i].Actor;
            }
        }
        if(OurPlayer != null && OurPlayer.GetPhotonView().ViewID == ViewID) { return OurPlayer.transform; }
        return null;
    }

    /// <summary>
    /// Find a player or bot by their PhotonPlayer
    /// </summary>
    /// <returns></returns>
    public Transform FindActor(Player player)
    {
        for (int i = 0; i < OthersActorsInScene.Count; i++)
        {
            if (OthersActorsInScene[i].ActorView != null && OthersActorsInScene[i].ActorView.Owner.ActorNumber == player.ActorNumber)
            {
                return OthersActorsInScene[i].Actor;
            }
        }
        if (OurPlayer != null && OurPlayer.GetPhotonView().Owner.ActorNumber == player.ActorNumber) { return OurPlayer.transform; }
        return null;
    }

    /// <summary>
    /// Find a player or bot by their PhotonPlayer
    /// </summary>
    /// <returns></returns>
    public SceneActors FindActor(string actorName)
    {
        for (int i = 0; i < OthersActorsInScene.Count; i++)
        {
            if (OthersActorsInScene[i].ActorView != null && OthersActorsInScene[i].Actor.name == actorName)
            {
                return OthersActorsInScene[i];
            }
        }
        if (OurPlayer != null && OurPlayer.GetPhotonView().Owner.NickName == actorName) { return LocalActor; }
        return null;
    }

    #region PUN

    [PunRPC]
    void RPCSyncGame(MatchState state)
    {
        Debug.Log("Game sync by master, match state: " + state.ToString());
        GameMatchState = state;
        if (!PhotonNetwork.IsMasterClient)
        {
            bl_RoundTime.Instance.Init();
        }
    }

    public void SetGameState(MatchState state)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        photonView.RPC("RPCMatchState", RpcTarget.All, state);
    }

    [PunRPC]
    void RPCMatchState(MatchState state)
    {
        GameMatchState = state;
    }

    /// <summary>
    /// 
    /// </summary>
    void CheckPlayersInMatch()
    {
        //if still waiting
        if (!bl_RoundTime.Instance.Initialized && GameMatchState == MatchState.Waiting)
        {
            bool ready = false;
            if (isOneTeamMode)
            {
                ready = PhotonNetwork.PlayerList.Length >= WaitingPlayersAmount;
            }
            else
            {
                //if the minimum amount of players are in the game
                if (PhotonNetwork.PlayerList.Length >= WaitingPlayersAmount)
                {
                    //and they are split in both teams
                    if (bl_RoomMenu.Instance.GetPlayerInDeltaCount > 0 && bl_RoomMenu.Instance.GetPlayerInReconCount > 0)
                    {
                        //we are ready to start
                        ready = true;
                    }
                    else
                    {
                        //otherwise wait until player split in both teams
#if LOCALIZATION
                        bl_UIReferences.Instance.SetWaitingPlayersText(bl_Localization.Instance.GetText(128), true);
#else
                        bl_UIReferences.Instance.SetWaitingPlayersText("Waiting for balance team players", true);
#endif
                        return;
                    }
                }
            }
            if (ready)//all needed players in game
            {
                //master set the call to start the match
                if (PhotonNetwork.IsMasterClient)
                {
                    bl_RoundTime.Instance.GetTime(true);
                    photonView.RPC("RpcStartTime", RpcTarget.AllBuffered, 3);
                }
#if ELIM
               else if (GetGameMode == GameMode.ELIM)
                {
                    bl_Elimination.Instance.StartRound();
                }
#endif
                SetGameState(MatchState.Starting);
            }
            else
            {
                bl_UIReferences.Instance.SetWaitingPlayersText(string.Format(bl_GameTexts.WaitingPlayers, PhotonNetwork.PlayerList.Length, 2), true);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnApplicationQuit()
    {
        PhotonNetwork.Disconnect();
    }

    //PLAYER EVENTS
    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Player connected: " + newPlayer.NickName);
        if (PhotonNetwork.IsMasterClient)
        {
            //master sync the require match info to be sure all players have the same info at the start
            photonView.RPC("RPCSyncGame", newPlayer, GameMatchState);
        }
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Player disconnected: " + otherPlayer.NickName);
    }

    public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
      
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        //when a player has join to a team
        if (changedProps.ContainsKey(PropertiesKeys.TeamKey))
        {
            //make sure has join to a team
            if ((string)changedProps[PropertiesKeys.TeamKey] != Team.None.ToString())
            {
                CheckPlayersInMatch();
            }
            else
            {
                if (GameMatchState == MatchState.Waiting)
                {
                    bl_UIReferences.Instance.SetWaitingPlayersText(string.Format(bl_GameTexts.WaitingPlayers, PhotonNetwork.PlayerList.Length, WaitingPlayersAmount), true);
                }
            }
        }
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("The old masterclient left, we have a new masterclient: " + newMasterClient.NickName);
        this.GetComponent<bl_ChatRoom>().AddLine("We have a new masterclient: " + newMasterClient.NickName);
    }

    public void OnConnected()
    {
      
    }

    public void OnConnectedToMaster()
    {
       
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Clean up a bit after server quit, cause: " + cause.ToString());
        PhotonNetwork.IsMessageQueueRunning = false;
        bl_UtilityHelper.LoadLevel(bl_GameData.Instance.OnDisconnectScene);
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {
     
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
       
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {
       
    }
#endregion

    public bool isEnterinGamePlay
    {
        get
        {
            return EnterInGamePlay;
        }
        set
        {
            EnterInGamePlay = value;
        }
    }

    [System.Serializable]
    public class SceneActors
    {
        public string Name;
        public Transform Actor;
        public PhotonView ActorView;
        public bool isRealPlayer = true;
        public bool isAlive = true;
    }

    private static bl_GameManager _instance;
    public static bl_GameManager Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_GameManager>(); }
            return _instance;
        }
    }
}		