/////////////////////////////////////////////////////////////////////////////////
///////////////////////////////bl_RoundTime.cs///////////////////////////////////
///////////////Use this to manage time in rooms//////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////
////////////////////////////////Lovatto Studio///////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable; //Replace default Hashtables with Photon hashtables
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class bl_RoundTime : bl_MonoBehaviour
{
    public RoundStyle m_RoundStyle { get; set; }
    public int RoundDuration { get; set; }
    public float CurrentTime { get; set; }

    //private
    private const string StartTimeKey = "RoomTime";       // the name of our "start time" custom property.
    private float m_Reference;
    private int m_countdown = 10;
    public bool isFinish { get; set; }
    private bl_RoomSettings RoomSettings;
    private bl_RoomMenu RoomMenu;
    private bl_GameManager m_Manager = null;
    private Text TimeText;
    private bl_UIReferences UIReferences;
    private bool roomClose = false;
    public bool Initialized { get; set; }
    public bool Pause { get; set; }
    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        if (!PhotonNetwork.IsConnected)
        {
            bl_UtilityHelper.LoadLevel(bl_GameData.Instance.MainMenuScene);
            return;
        }
        base.Awake();
        RoomSettings = this.GetComponent<bl_RoomSettings>();
        RoomMenu = GetComponent<bl_RoomMenu>();
        m_Manager = GetComponent<bl_GameManager>();
        UIReferences = bl_UIReferences.Instance;
        TimeText = UIReferences.PlayerUI.TimeText;
        Pause = false;
    }

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
            return;
        //only master client initialized by default, other players will wait until master sync match information after they join.
        if (PhotonNetwork.IsMasterClient)
        {
            Init();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Init()
    {
        //if the match is waiting for a minimum amount of players to start
        if (m_Manager.GameMatchState == MatchState.Waiting)
        {
            bl_UIReferences.Instance.SetWaitingPlayersText(string.Format(bl_GameTexts.WaitingPlayers, PhotonNetwork.PlayerList.Length, 2), true);
            return;
        }
#if LMS
        if (GetGameMode == GameMode.LSM)return;
#endif
        GetTime(true);
    }

    /// <summary>
    /// get the current time and verify if it is correct
    /// </summary>
    public void GetTime(bool ResetReference)
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.TimeRoomKey] != null)
        {
            RoundDuration = (int)PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.TimeRoomKey];
        }
        if (PhotonNetwork.IsMasterClient)
        {
            if (ResetReference)
            {
                m_Reference = (float)PhotonNetwork.Time;

                Hashtable startTimeProp = new Hashtable();  // only use ExitGames.Client.Photon.Hashtable for Photon
                startTimeProp.Add(StartTimeKey, m_Reference);
                PhotonNetwork.CurrentRoom.SetCustomProperties(startTimeProp);
            }
        }
        else
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties[StartTimeKey] != null)
            {
                m_Reference = (float)PhotonNetwork.CurrentRoom.CustomProperties[StartTimeKey];
            }
        }
        if (!Initialized) { bl_GameManager.Instance.SetGameState(MatchState.Playing); }
        Initialized = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public void RestartTime()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.TimeRoomKey] != null)
        {
            RoundDuration = (int)PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.TimeRoomKey];
        }
        m_Reference = (float)PhotonNetwork.Time;
        if (PhotonNetwork.IsMasterClient)
        {
            Hashtable startTimeProp = new Hashtable();  // only use ExitGames.Client.Photon.Hashtable for Photon
            startTimeProp.Add(StartTimeKey, m_Reference);
            PhotonNetwork.CurrentRoom.SetCustomProperties(startTimeProp);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnFixedUpdate()
    {
        if (!Initialized || Pause)
            return;

        float t_time = RoundDuration - ((float)PhotonNetwork.Time - m_Reference);
        if (t_time > 0.0001f)
        {
            CurrentTime = t_time;
            if(CurrentTime <= 30 && !roomClose && PhotonNetwork.IsMasterClient)
            {
                roomClose = true;
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;
                Debug.Log("Close room to prevent player join");
            }
        }
        else if (t_time <= 0.001 && GetTimeServed == true)//Round Finished
        {
            CurrentTime = 0;
            FinishRound();
        }
        else//even if I do not photonnetwork.time then obtained to regain time
        {
            Refresh();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void FinishRound()
    {
        if (!PhotonNetwork.IsConnected)
            return;

        bl_EventHandler.OnRoundEndEvent();
        if (!isFinish)
        {
            isFinish = true;
            if (RoomMenu) { RoomMenu.isFinish = true; }
            if (m_Manager) { m_Manager.GameFinish = true; }
            if (m_RoundStyle == RoundStyle.OneMacht)
            {
                FindObjectOfType<bl_GameFinish>().CollectData();
            }
            UIReferences.SetCountDown(m_countdown);
            InvokeRepeating("Countdown", 1, 1);
            UIReferences.SetFinalText(m_RoundStyle, RoomSettings.GetWinnerName);
            bl_UCrosshair.Instance.Show(false);
            bl_GameManager.Instance.SetGameState(MatchState.Finishing);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        TimeControll();
    }

    /// <summary>
    /// 
    /// </summary>
    void TimeControll()
    {
        if (CurrentTime == 0) return;

        int normalSecons = 60;
        float remainingTime = Mathf.CeilToInt(CurrentTime);
        int m_Seconds = Mathf.FloorToInt(remainingTime % normalSecons);
        int m_Minutes = Mathf.FloorToInt((remainingTime / normalSecons) % normalSecons);
        string t_time = bl_UtilityHelper.GetTimeFormat(m_Minutes, m_Seconds);

        if (TimeText != null)
        {
            TimeText.text = t_time;
        }
    }

    /// <summary>
    /// with this fixed the problem of the time lag in the Photon
    /// </summary>
    void Refresh()
    {
        if (PhotonNetwork.CurrentRoom == null)
            return;

        if (PhotonNetwork.IsMasterClient)
        {
            m_Reference = (float)PhotonNetwork.Time;

            Hashtable startTimeProp = new Hashtable();  // only use ExitGames.Client.Photon.Hashtable for Photon
            startTimeProp.Add(StartTimeKey, m_Reference);
            PhotonNetwork.CurrentRoom.SetCustomProperties(startTimeProp);
        }
        else
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(StartTimeKey))
            {
                m_Reference = (float)PhotonNetwork.CurrentRoom.CustomProperties[StartTimeKey];
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void Countdown()
    {
        m_countdown--;
        UIReferences.SetCountDown(m_countdown);
        if (m_countdown <= 0)
        {
            FinishGame();
            CancelInvoke("Countdown");
            m_countdown = 10;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void FinishGame()
    {

        bl_UtilityHelper.LockCursor(false);
        if (m_RoundStyle == RoundStyle.OneMacht)
        {
            FindObjectOfType<bl_GameFinish>().Show();
        }
        if (m_RoundStyle == RoundStyle.Rounds)
        {
#if ULSP
            if (bl_DataBase.Instance != null)
            {
                Player p = PhotonNetwork.LocalPlayer;
                bl_DataBase.Instance.SaveData(p.GetPlayerScore(), p.GetKills(), p.GetDeaths());
                bl_DataBase.Instance.StopAndSaveTime();
            }
#endif
            GetTime(true);
            if (RoomSettings)
            {
                RoomSettings.ResetRoom();
            }
            isFinish = false;

            if (m_Manager)
            {
                m_Manager.GameFinish = false;
                m_Manager.DestroyPlayer(true);
            }
            if (RoomMenu != null)
            {
                RoomMenu.isFinish = false;
                RoomMenu.isPlaying = false;
                bl_UtilityHelper.LockCursor(false);
            }
            UIReferences.ResetRound();
            bl_UIReferences.Instance.OnKillCam(false);
            m_countdown = 10;
            PhotonNetwork.CurrentRoom.IsOpen = true;
            PhotonNetwork.CurrentRoom.IsVisible = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void StartNewRoundAndKeepData()
    {
        GetTime(true);
        isFinish = false;
        if (RoomMenu != null)
        {
            RoomMenu.isFinish = false;
            RoomMenu.isPlaying = false;
            bl_UtilityHelper.LockCursor(false);
        }
        m_Manager.SpawnPlayer(PhotonNetwork.LocalPlayer.GetPlayerTeam());
    }

    [PunRPC]
    void RpcStartTime(int wait)
    {
        if (bl_GameManager.Instance.LocalPlayerTeam == Team.None) return;

        bl_UIReferences.Instance.SetWaitingPlayersText("Starting Match...", true);
        Invoke("StartTime", wait);
    }

    /// <summary>
    /// 
    /// </summary>
    void StartTime()
    {
        GetTime(false);
        Pause = false;
        bl_UIReferences.Instance.SetWaitingPlayersText();
        m_Manager.SpawnPlayer(PhotonNetwork.LocalPlayer.GetPlayerTeam());
    }

    bool GetTimeServed
    {
        get
        {
            bool m_bool = false;
            if (Time.timeSinceLevelLoad > 7)
            {
                m_bool = true;
            }
            return m_bool;
        }
    }

    private static bl_RoundTime _instance;
    public static bl_RoundTime Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_RoundTime>(); }
            return _instance;
        }
    }
}