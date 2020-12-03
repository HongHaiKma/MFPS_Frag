using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using Photon.Pun;
using Photon.Realtime;

public class bl_Lobby : bl_PhotonHelper, IConnectionCallbacks, ILobbyCallbacks, IMatchmakingCallbacks
{
    private string playerName;
    private string hostName; //Name of room
    public int MainMenuWindow = 2; // 2 = search room window
    [Header("Photon")]
    [SerializeField] private string RoomNamePrefix = "LovattoRoom {0}";
    [SerializeField] private string PlayerNamePrefix = "Guest {0}";
    public SeverRegionCode DefaultServer = SeverRegionCode.usw;
    public bool ShowPhotonStatistics;

    [Header("Room Options")]
    //Max players in game
    public int[] maxPlayers = new int[] { 6, 2, 4, 8 };
    private int players;
    //Room Time in seconds
    public int[] RoomTime = new int[] { 600, 300, 900, 1200 };
    private int r_Time;
    //Room Max Kills
    public int[] RoomKills = new int[] { 50, 100, 150, 200 };
    private int CurrentMaxKills = 0;
    //Room Max Ping
    public int[] MaxPing = new int[] { 100, 200, 500, 1000 };
    private int CurrentMaxPing = 0;
#if BDGM || ELIM
    public int[] BDRounds = new int[] { 5, 3, 7, 9 };
    private int CurrentBDRound;
#endif

    private bl_GameData.GameModesEnabled[] GameModes;
    private int CurrentGameMode = 0;

    [Header("Audio")]
    [SerializeField] private AudioClip BackgroundSound;
    [SerializeField, Range(0, 1)] private float MaxBackgroundVolume = 0.3f;

    [Header("References")]
    public List<GameObject> MenusUI = new List<GameObject>();
    public GameObject FadeImage;
    [SerializeField] private Text PlayerNameText = null;
    [SerializeField] private Text PlayerCoinsText = null;
    [SerializeField] private GameObject RoomInfoPrefab;
    [SerializeField] private GameObject PhotonStaticticsUI;
    [SerializeField] private GameObject PhotonGamePrefab;
    [SerializeField] private GameObject EnterPasswordUI;
    [SerializeField] private GameObject PingKickMessageUI;
    [SerializeField] private GameObject AFKKickMessageUI;
    [SerializeField] private GameObject SeekingMatchUI;
    [SerializeField] private Transform RoomListPanel;
    [SerializeField] private CanvasGroup CanvasGroupRoot = null;
    [SerializeField] private Text MaxPlayerText = null;
    [SerializeField] private Text RoundTimeText = null;
    [SerializeField] private Text GameModeText = null;
    [SerializeField] private Text MaxKillsText = null;
    [SerializeField] private Text MaxPingText = null;
    [SerializeField] private Text MapNameText = null;
    [SerializeField] private Text BDRoundText;
    [SerializeField] private Text PasswordLogText = null;
    [SerializeField] private Text LoadingScreenText;
    [SerializeField] private Text NoRoomText;
    [SerializeField] private Slider VolumeSlider;
    [SerializeField] private Slider BackgroundVolumeSlider;
    [SerializeField] private Slider SensitivitySlider;
    [SerializeField] private Slider AimSensitivitySlider;
    [SerializeField] private Slider WeaponFOVSlider;
    [SerializeField] private Dropdown QualityDropdown;
    [SerializeField] private Dropdown AnisoDropdown;
    [SerializeField] private Image MapPreviewImage = null;
    public Image LevelIcon;
    [SerializeField] private InputField PlayerNameField = null;
    [SerializeField] private InputField RoomNameField = null;
    [SerializeField] private InputField PassWordField;
    [SerializeField] private Toggle MuteVoiceToggle;
    [SerializeField] private Toggle PushToTalkToogle;
    [SerializeField] private Toggle InvertMouseXToogle;
    [SerializeField] private Toggle InvertMouseYToogle;
    [SerializeField] private Toggle FrameRateToggle;
    [SerializeField] private Toggle WithBotsToggle;
    [SerializeField] private Dropdown ServersDropdown;
    [SerializeField] private GameObject KickMessageUI;
    [SerializeField] private GameObject DisconnectCauseUI;
    [SerializeField] private CanvasGroup LoadingScreen;
    [SerializeField] private CanvasGroup BlackScreen;
    public GameObject MaxPingMessageUI;
    [SerializeField] private Button[] ClassButtons;
    public GameObject[] OptionsWindows;
    public GameObject[] AddonsButtons;
    public LevelUI m_LevelUI;
    [SerializeField] private AnimationCurve FadeCurve;

    //OPTIONS
    private int m_currentQuality = 3;
    private float m_volume = 1.0f;
    private float BackgroundVolume = 0.3f;
    private float m_sensitive = 15;
    private float AimSensitivity = 7;
    private bool FrameRate = false;
    private bool imh;
    private bool imv;
    private bool MuteVoice;
    private bool PushToTalk;
    private int m_stropic = 0;
    private bool GamePerRounds = false;
    private bool AutoTeamSelection = false;
    private bool FriendlyFire = false;
    private float alpha = 2.0f;

    private List<GameObject> CacheRoomList = new List<GameObject>();
    private int CurrentScene = 0;
    private bl_LobbyChat Chat;
    private string CachePlayerName = "";
    private AudioSource BackSource;
    private RoomInfo checkingRoom;
    private int PendingRegion = -1;
    private bool FirstConnectionMade = false;
    private bool AppQuit = false;
#if ULSP
    private bl_DataBase DataBase;
#endif

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
#if ULSP
        DataBase = FindObjectOfType<bl_DataBase>();
        if (DataBase == null && bl_LoginProDataBase.Instance.ForceLoginScene)
        {
            bl_UtilityHelper.LoadLevel("Login");
            return;
        }
#endif
        PhotonNetwork.AddCallbackTarget(this);
        PhotonNetwork.UseRpcMonoBehaviourCache = true;
        bl_UtilityHelper.LockCursor(false);
        StartCoroutine(StartFade());
        Chat = GetComponent<bl_LobbyChat>();
        hostName = string.Format(RoomNamePrefix, Random.Range(10, 999));
        RoomNameField.text = hostName;
        if (FindObjectOfType<bl_PhotonGame>() == null) { Instantiate(PhotonGamePrefab); }
        if (BackgroundSound != null)
        {
            BackSource = GetComponent<AudioSource>();
            if (BackSource == null)
            {
                BackSource = gameObject.AddComponent<AudioSource>();
            }
            BackSource.clip = BackgroundSound;
            BackSource.volume = 0;
            BackSource.playOnAwake = false;
            BackSource.loop = true;
        }
        DisableAllWindows();
        if (bl_GameData.isDataCached)
        {
            SetUpGameModes();
            LoadSettings();
            SetUpUI();
        }
        if (BackgroundSound != null) { StartCoroutine(FadeAudioBack(true)); }
        FadeImage.SetActive(false);
        if (PhotonStaticticsUI != null) { PhotonStaticticsUI.SetActive(ShowPhotonStatistics); }
#if LOCALIZATION
        bl_Localization.Instance.OnLanguageChange += OnLanguageChange;
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    void SetUpGameModes()
    {
        List<bl_GameData.GameModesEnabled> gm = new List<bl_GameData.GameModesEnabled>();
        for (int i = 0; i < bl_GameData.Instance.m_GameModesAvailables.Count; i++)
        {
            if (bl_GameData.Instance.m_GameModesAvailables[i].isEnabled)
            {
                gm.Add(bl_GameData.Instance.m_GameModesAvailables[i]);
            }
        }
        GameModes = gm.ToArray();
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
#if LOCALIZATION
        bl_Localization.Instance.OnLanguageChange -= OnLanguageChange;
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public void ConnectPhoton()
    {
        // the following line checks if this client was just created (and not yet online). if so, we connect
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.AuthValues = new AuthenticationValues(PhotonNetwork.NickName);
            if (PendingRegion == -1)
            {
                PhotonNetwork.GameVersion = bl_GameData.Instance.GameVersion;
                PhotonNetwork.ConnectUsingSettings();
            }
            else
            {
                FirstConnectionMade = true;
                ChangeServerCloud(PendingRegion);
            }
#if LOCALIZATION
            LoadingScreenText.text = bl_Localization.Instance.GetText(40);
#else
            LoadingScreenText.text = bl_GameTexts.ConnectingToGameServer;
#endif
            StartCoroutine(ShowLoadingScreen());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnApplicationQuit()
    {
        AppQuit = true;
        bl_GameData.isDataCached = false;
        Disconect();
    }

    /// <summary>
    /// 
    /// </summary>
    void FixedUpdate()
    {
        if (PhotonNetwork.IsConnected && bl_GameData.isDataCached)
        {
            PlayerNameText.text = PhotonNetwork.NickName;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void ServerList(List<RoomInfo> roomList)
    {
        //Removed old list
        if (CacheRoomList.Count > 0)
        {
            foreach (GameObject g in CacheRoomList)
            {
                Destroy(g);
            }
            CacheRoomList.Clear();
        }
        //Update List
        RoomInfo[] ri = roomList.ToArray();
        if (ri.Length > 0)
        {
            NoRoomText.text = string.Empty;
            for (int i = 0; i < ri.Length; i++)
            {
                GameObject r = Instantiate(RoomInfoPrefab) as GameObject;
                CacheRoomList.Add(r);
                r.GetComponent<bl_RoomInfo>().GetInfo(ri[i]);
                r.transform.SetParent(RoomListPanel, false);
            }

        }
        else
        {
#if LOCALIZATION
            NoRoomText.text = bl_Localization.Instance.GetText("norooms");
#else

            NoRoomText.text = bl_GameTexts.NoRoomsCreated;
#endif
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void EnterName(InputField field = null)
    {
        if (field == null || string.IsNullOrEmpty(field.text))
            return;

        CachePlayerName = field.text;
        int check = bl_GameData.Instance.CheckPlayerName(CachePlayerName);
        if (check == 1)
        {
            MenusUI[9].SetActive(true);
            return;
        }
        else if (check == 2)
        {
            field.text = string.Empty;
            return;
        }

        playerName = CachePlayerName;
        playerName = playerName.Replace("\n", "");
        PlayerPrefs.SetString(PropertiesKeys.PlayerName, playerName);
        StartCoroutine(Fade(LobbyState.MainMenu));

        PhotonNetwork.NickName = playerName;
        ConnectPhoton();
        //load the user coins
        //NOTE: Coins are store locally, so is highly recommended to store in a database, you can use ULogin for it.
#if !ULSP
        bl_GameData.Instance.VirtualCoins.LoadCoins(PhotonNetwork.NickName);
        PlayerCoinsText.text = bl_GameData.Instance.VirtualCoins.UserCoins.ToString();
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public void EnterPassword(InputField field = null)
    {
        if (field == null || string.IsNullOrEmpty(field.text))
            return;

        string pass = field.text;
        if (bl_GameData.Instance.CheckPasswordUse(CachePlayerName, pass))
        {
            playerName = CachePlayerName;
            playerName = playerName.Replace("\n", "");
            StartCoroutine(Fade(LobbyState.MainMenu));

            PhotonNetwork.NickName = playerName;
            ConnectPhoton();
        }
        else
        {
            field.text = string.Empty;
        }
    }
    public void ClosePassField() { MenusUI[9].SetActive(false); }


    private bool isSeekingMatch = false;
    public void AutoMatch()
    {
        if (isSeekingMatch)
            return;

        isSeekingMatch = true;
        StartCoroutine(AutoMatchIE());
    }

    IEnumerator AutoMatchIE()
    {
        //active the search match UI
        SeekingMatchUI.SetActive(true);
        yield return new WaitForSeconds(3);
        PhotonNetwork.JoinRandomRoom();
        isSeekingMatch = false;
        SeekingMatchUI.SetActive(false);
    }

    public void OnNoRoomsToJoin(short returnCode, string message)
    {
        Debug.Log("No games to join found on matchmaking, creating one.");

        int propsCount = 11;
        string roomName = string.Format("[PUBLIC] {0}{1}", PhotonNetwork.NickName.Substring(0, 2), Random.Range(0, 9999));
        int scid = Random.Range(0, bl_GameData.Instance.AllScenes.Count);
        int maxPlayersRandom = Random.Range(0, maxPlayers.Length);
        int timeRandom = Random.Range(0, RoomTime.Length);
        int killsRandom = Random.Range(0, RoomKills.Length);
        int modeRandom = Random.Range(0, GameModes.Length);

        ExitGames.Client.Photon.Hashtable roomOption = new ExitGames.Client.Photon.Hashtable();
        roomOption[PropertiesKeys.TimeRoomKey] = RoomTime[timeRandom];
        roomOption[PropertiesKeys.GameModeKey] = GameModes[modeRandom].m_GameMode.ToString();
        roomOption[PropertiesKeys.SceneNameKey] = bl_GameData.Instance.AllScenes[scid].RealSceneName;
        roomOption[PropertiesKeys.RoomRoundKey] = RoundStyle.OneMacht;
        roomOption[PropertiesKeys.TeamSelectionKey] = false;
        roomOption[PropertiesKeys.CustomSceneName] = bl_GameData.Instance.AllScenes[scid].ShowName;
        roomOption[PropertiesKeys.RoomMaxKills] = RoomKills[killsRandom];
        roomOption[PropertiesKeys.RoomFriendlyFire] = false;
        roomOption[PropertiesKeys.MaxPing] = MaxPing[CurrentMaxPing];
        roomOption[PropertiesKeys.RoomPassworld] = string.Empty;
        roomOption[PropertiesKeys.WithBotsKey] = (GameModes[CurrentGameMode].m_GameMode == GameMode.FFA || GameModes[CurrentGameMode].m_GameMode == GameMode.TDM) ? true : false;
#if BDGM || ELIM
        roomOption[PropertiesKeys.NumberOfRounds] = BDRounds[CurrentBDRound];
         propsCount = 12;
#endif

        string[] properties = new string[propsCount];
        properties[0] = PropertiesKeys.TimeRoomKey;
        properties[1] = PropertiesKeys.GameModeKey;
        properties[2] = PropertiesKeys.SceneNameKey;
        properties[3] = PropertiesKeys.RoomRoundKey;
        properties[4] = PropertiesKeys.TeamSelectionKey;
        properties[5] = PropertiesKeys.CustomSceneName;
        properties[6] = PropertiesKeys.RoomMaxKills;
        properties[7] = PropertiesKeys.RoomFriendlyFire;
        properties[8] = PropertiesKeys.MaxPing;
        properties[9] = PropertiesKeys.RoomPassworld;
        properties[10] = PropertiesKeys.WithBotsKey;
#if BDGM || ELIM
        properties[11] = PropertiesKeys.NumberOfRounds;
#endif

        PhotonNetwork.CreateRoom(roomName, new RoomOptions()
        {
            MaxPlayers = (byte)maxPlayers[maxPlayersRandom],
            IsVisible = true,
            IsOpen = true,
            CustomRoomProperties = roomOption,
            CleanupCacheOnLeave = true,
            CustomRoomPropertiesForLobby = properties

        }, null);
        FadeImage.SetActive(true);
        FadeImage.GetComponent<Animator>().speed = 2;
        if (BackSource != null) { StartCoroutine(FadeAudioBack(false)); }
    }


    /// <summary>
    /// 
    /// </summary>
    public void CreateRoom()
    {
        if (Chat != null && Chat.isConnected()) { Chat.Disconnect(); }
        int propsCount = 11;
        PhotonNetwork.NickName = playerName;
        //Save Room properties for load in room
        ExitGames.Client.Photon.Hashtable roomOption = new ExitGames.Client.Photon.Hashtable();
        roomOption[PropertiesKeys.TimeRoomKey] = RoomTime[r_Time];
        roomOption[PropertiesKeys.GameModeKey] = GameModes[CurrentGameMode].m_GameMode.ToString();
        roomOption[PropertiesKeys.SceneNameKey] = bl_GameData.Instance.AllScenes[CurrentScene].RealSceneName;
        roomOption[PropertiesKeys.RoomRoundKey] = GamePerRounds ? RoundStyle.Rounds : RoundStyle.OneMacht;
        roomOption[PropertiesKeys.TeamSelectionKey] = AutoTeamSelection;
        roomOption[PropertiesKeys.CustomSceneName] = bl_GameData.Instance.AllScenes[CurrentScene].ShowName;
        roomOption[PropertiesKeys.RoomMaxKills] = RoomKills[CurrentMaxKills];
        roomOption[PropertiesKeys.RoomFriendlyFire] = FriendlyFire;
        roomOption[PropertiesKeys.MaxPing] = MaxPing[CurrentMaxPing];
        roomOption[PropertiesKeys.RoomPassworld] = PassWordField.text;
        roomOption[PropertiesKeys.WithBotsKey] = (GameModes[CurrentGameMode].m_GameMode == GameMode.FFA || GameModes[CurrentGameMode].m_GameMode == GameMode.TDM) ? WithBotsToggle.isOn : false;
#if BDGM || ELIM
        roomOption[PropertiesKeys.NumberOfRounds] = BDRounds[CurrentBDRound];
         propsCount = 12;
#endif

        string[] properties = new string[propsCount];
        properties[0] = PropertiesKeys.TimeRoomKey;
        properties[1] = PropertiesKeys.GameModeKey;
        properties[2] = PropertiesKeys.SceneNameKey;
        properties[3] = PropertiesKeys.RoomRoundKey;
        properties[4] = PropertiesKeys.TeamSelectionKey;
        properties[5] = PropertiesKeys.CustomSceneName;
        properties[6] = PropertiesKeys.RoomMaxKills;
        properties[7] = PropertiesKeys.RoomFriendlyFire;
        properties[8] = PropertiesKeys.MaxPing;
        properties[9] = PropertiesKeys.RoomPassworld;
        properties[10] = PropertiesKeys.WithBotsKey;
#if BDGM || ELIM
        properties[11] = PropertiesKeys.NumberOfRounds;
#endif

        PhotonNetwork.CreateRoom(hostName, new RoomOptions()
        {
            MaxPlayers = (byte)maxPlayers[players],
            IsVisible = true,
            IsOpen = true,
            CustomRoomProperties = roomOption,
            CleanupCacheOnLeave = true,
            CustomRoomPropertiesForLobby = properties,
            PublishUserId = true,
            EmptyRoomTtl = 0,
        }, null);
        FadeImage.SetActive(true);
        FadeImage.GetComponent<Animator>().speed = 2;
        if (BackSource != null) { StartCoroutine(FadeAudioBack(false)); }
    }


    #region UGUI

    public void CheckRoomPassword(RoomInfo room)
    {
        checkingRoom = room;
        EnterPasswordUI.SetActive(true);
    }

    public void OnEnterPassworld(InputField pass)
    {
        if (checkingRoom == null)
        {
            Debug.Log("Checking room is not assigned more!");
            return;
        }

        if ((string)checkingRoom.CustomProperties[PropertiesKeys.RoomPassworld] == pass.text && checkingRoom.PlayerCount < checkingRoom.MaxPlayers)
        {
            if (PhotonNetwork.GetPing() < (int)checkingRoom.CustomProperties[PropertiesKeys.MaxPing])
            {
                FadeImage.SetActive(true);
                FadeImage.GetComponent<Animator>().speed = 2;
                if (checkingRoom.PlayerCount < checkingRoom.MaxPlayers)
                {
                    PhotonNetwork.JoinRoom(checkingRoom.Name);
                }
            }
        }
        else
        {
            PasswordLogText.text = "Wrong room password";
        }
    }

    /// <summary>
    /// For button can call this 
    /// </summary>
    /// <param name="id"></param>
    public void ChangeWindow(int id)
    {
        ChangeWindow(id, -1);
    }

    public void OnChangeClass(int Class)
    {
        foreach (Button b in ClassButtons) { b.interactable = true; }
        ClassButtons[Class].interactable = false;
        PlayerClass p = (PlayerClass)Class;
        p.SavePlayerClass();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public void ChangeWindow(int id, int id2)
    {
        StartCoroutine(Fade(LobbyState.MainMenu, 3f));
        for (int i = 0; i < MenusUI.Count; i++)
        {
            if (i == id || i == id2)
            {
                if (MenusUI[i] != null)
                    MenusUI[i].SetActive(true);
            }
            else
            {
                if (i != 1)//1 = main menu buttons
                {
                    if (MenusUI[i] != null)
                    {
                        MenusUI[i].SetActive(false);
                    }
                }
                if (id == 8)
                {
                    MenusUI[1].SetActive(false);
                }
            }
        }
    }

    void DisableAllWindows()
    {
        foreach (GameObject item in MenusUI)
        {
            if (item == null) continue;
            item.SetActive(false);
        }
    }

    public void ChangeOptionsWindow(int id)
    {
        foreach (GameObject g in OptionsWindows) { g.SetActive(false); }
        OptionsWindows[id].SetActive(true);
    }

    public void Disconect()
    {
        if (Chat != null && Chat.isConnected()) { Chat.Disconnect(); }
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }

    void DelayDisconnect() { PhotonNetwork.Disconnect(); }
    /// <summary>
    /// 
    /// </summary>
    private bool serverchangeRequested = false;
    public void ChangeServerCloud(int id)
    {
        if (PhotonNetwork.IsConnected && FirstConnectionMade)
        {
            serverchangeRequested = true;
            ChangeWindow(3);
#if LOCALIZATION
            LoadingScreenText.text = bl_Localization.Instance.GetText(40);
#else
            LoadingScreenText.text = bl_GameTexts.ConnectingToGameServer;
#endif
            StartCoroutine(ShowLoadingScreen());
            MenusUI[1].SetActive(false);
            PendingRegion = id;
            Invoke("DelayDisconnect", 0.2f);
            return;
        }
        if (!string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime))
        {
            if (!FirstConnectionMade)
            {
                PendingRegion = id;
                serverchangeRequested = true;
                return;
            }
            serverchangeRequested = false;
            SeverRegionCode code = SeverRegionCode.usw;
            if (id > 3) { id++; }
            code = (SeverRegionCode)id;
            PhotonNetwork.NetworkingClient.AppId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime;
            PhotonNetwork.ConnectToRegion(code.ToString());
            PlayerPrefs.SetString(PropertiesKeys.PreferredRegion, code.ToString());
        }
        else
        {
            Debug.LogWarning("Need your AppId for change server, please add it in PhotonServerSettings");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="level"></param>
    public void LoadLocalLevel(string level)
    {
        Disconect();
        bl_UtilityHelper.LoadLevel(level);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="plus"></param>
    public void ChangeMaxPlayer(bool plus)
    {
        if (plus)
        {
            if (players < maxPlayers.Length)
            {
                players++;
                if (players > (maxPlayers.Length - 1)) players = 0;

            }
        }
        else
        {
            if (players < maxPlayers.Length)
            {
                players--;
                if (players < 0) players = maxPlayers.Length - 1;
            }
        }
#if LOCALIZATION
        MaxPlayerText.text = string.Format("{0} {1}", maxPlayers[players], bl_Localization.Instance.GetTextPlural("player"));
#else
        MaxPlayerText.text = string.Format("{0} {1}", maxPlayers[players], bl_GameTexts.Players);
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="plus"></param>
    public void ChangeRoundTime(bool plus)
    {
        if (!plus)
        {
            if (r_Time < RoomTime.Length)
            {
                r_Time--;
                if (r_Time < 0)
                {
                    r_Time = RoomTime.Length - 1;

                }
            }
        }
        else
        {
            if (r_Time < RoomTime.Length)
            {
                r_Time++;
                if (r_Time > (RoomTime.Length - 1))
                {
                    r_Time = 0;

                }

            }
        }
#if LOCALIZATION
        string rtl = string.Format(" {0}", bl_Localization.Instance.GetTextPlural("minute"));
        RoundTimeText.text = (RoomTime[r_Time] / 60) + rtl;
#else
        RoundTimeText.text = (RoomTime[r_Time] / 60) + " Minutes";
#endif
    }


    public void ChangeBDRounds(bool plus)
    {
#if BDGM || ELIM
        if (plus)
        {
            if (CurrentBDRound < BDRounds.Length)
            {
                CurrentBDRound++;
                if (CurrentBDRound > (BDRounds.Length - 1)) CurrentBDRound = 0;

            }
        }
        else
        {
            if (CurrentBDRound < BDRounds.Length)
            {
                CurrentBDRound--;
                if (CurrentBDRound < 0) CurrentBDRound = BDRounds.Length - 1;
            }
        }
        BDRoundText.text = BDRounds[CurrentBDRound] + " Rounds";
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public void ChangeMaxKills(bool plus)
    {
        if (!plus)
        {
            if (CurrentMaxKills < RoomKills.Length)
            {
                CurrentMaxKills--;
                if (CurrentMaxKills < 0)
                {
                    CurrentMaxKills = RoomKills.Length - 1;

                }
            }
        }
        else
        {
            if (CurrentMaxKills < RoomKills.Length)
            {
                CurrentMaxKills++;
                if (CurrentMaxKills > (RoomKills.Length - 1))
                {
                    CurrentMaxKills = 0;

                }

            }
        }
#if LOCALIZATION
        string rtl = string.Format(" {0}", bl_Localization.Instance.GetTextPlural("kill"));
        MaxKillsText.text = (RoomKills[CurrentMaxKills]) + rtl;
#else
        MaxKillsText.text = (RoomKills[CurrentMaxKills]) + " Kills";
#endif
    }

    public void ChangeMaxPing(bool fowr)
    {
        if (fowr)
        {
            if (CurrentMaxPing >= MaxPing.Length - 1) { CurrentMaxPing = 0; }
            else
            {
                CurrentMaxPing = CurrentMaxPing + 1 % MaxPing.Length;
            }
        }
        else
        {
            if (CurrentMaxPing > 0) { CurrentMaxPing = CurrentMaxPing - 1 % MaxPing.Length; }
            else { CurrentMaxPing = MaxPing.Length - 1; }
        }
#if LOCALIZATION
        string f = (MaxPing[CurrentMaxPing] == 0) ? bl_Localization.Instance.GetText(25) : "{0} ms";
        MaxPingText.text = string.Format(f, MaxPing[CurrentMaxPing]);
#else
        string f = (MaxPing[CurrentMaxPing] == 0) ? bl_GameTexts.NoLimit : "{0} ms";
        MaxPingText.text = string.Format(f, MaxPing[CurrentMaxPing]);
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public void ChangeGameMode(bool plus)
    {
        if (plus)
        {
            if (CurrentGameMode < GameModes.Length)
            {
                CurrentGameMode++;
                if (CurrentGameMode > (GameModes.Length - 1))
                {
                    CurrentGameMode = 0;
                }
            }
        }
        else
        {
            if (CurrentGameMode < GameModes.Length)
            {
                CurrentGameMode--;
                if (CurrentGameMode < 0)
                {
                    CurrentGameMode = GameModes.Length - 1;

                }
            }
        }
        WithBotsToggle.gameObject.SetActive(GameModes[CurrentGameMode].m_GameMode == GameMode.FFA || GameModes[CurrentGameMode].m_GameMode == GameMode.TDM);
#if LOCALIZATION
        GameModeText.text = bl_Localization.Instance.GetText(GameModes[CurrentGameMode].m_GameMode.ToString());
#else        
        GameModeText.text = GameModes[CurrentGameMode].m_GameMode.GetName();
#endif
        bool r = false;
#if ELIM
      if(GameModes[CurrentGameMode] == GameMode.ELIM.ToString()) { r = true; }
#endif
#if BDGM
      if(GameModes[CurrentGameMode] == GameMode.SND.ToString()) { r = true; }
#endif
        AddonsButtons[3].SetActive(r);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ChangeMap(bool plus)
    {
        List<bl_GameData.SceneInfo> m_scenes = bl_GameData.Instance.AllScenes;
        if (!plus)
        {
            if (CurrentScene < m_scenes.Count)
            {
                CurrentScene--;
                if (CurrentScene < 0)
                {
                    CurrentScene = m_scenes.Count - 1;

                }
            }
        }
        else
        {
            if (CurrentScene < m_scenes.Count)
            {
                CurrentScene++;
                if (CurrentScene > (m_scenes.Count - 1))
                {
                    CurrentScene = 0;
                }
            }
        }
        MapNameText.text = m_scenes[CurrentScene].ShowName;
        MapPreviewImage.sprite = m_scenes[CurrentScene].Preview;
    }

    public void OnChangeQuality(int id)
    {
        QualitySettings.SetQualityLevel(id, true);
        m_currentQuality = id;
    }

    public void OnChangeAniso(int id)
    {
        if (id == 0)
        {
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
        }
        else if (id == 1)
        {
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
        }
        else
        {
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
        }
        m_stropic = id;
    }

    /// <summary>
    /// 
    /// </summary>
    public void QuitGame(bool b)
    {
        if (b)
        {
            if (Chat != null && Chat.isConnected()) { Chat.Disconnect(); }
            Application.Quit();
            Debug.Log("Game Exit, this only work in standalone version");
        }
        else
        {
            StartCoroutine(Fade(LobbyState.MainMenu, 3.2f));
            ChangeWindow(2, 1);
        }
    }

    public void CloseKickMessage()
    {
        KickMessageUI.SetActive(false);
        MaxPingMessageUI.SetActive(false);
        PingKickMessageUI.SetActive(false);
        bl_PhotonGame.Instance.hasPingKick = false;
    }

    public void ChangeAutoTeamSelection(bool b) { AutoTeamSelection = b; }
    public void ChangeFriendlyFire(bool b) { FriendlyFire = b; }
    public void ChangeGamePerRound(bool b) { GamePerRounds = b; }
    public void ChangeRoomName(string t) { hostName = t; }
    public void ChangeVolume(float v) { m_volume = v; }
    public void ChangeSensitivity(float s) { m_sensitive = s; }
    public void ChangeAimSensitivity(float s) { AimSensitivity = s; }
    public void ChangeWeaponFov(float s) { m_WeaponFov = Mathf.FloorToInt(s); }
    public void OnChangeFrameRate(bool b) { FrameRate = b; }
    public void OnChangeIMV(bool b) { imv = b; }
    public void OnChangeIMH(bool b) { imh = b; }
    public void OnChangeMuteVoice(bool b) { MuteVoice = b; }
    public void OnChangePushToTalk(bool b) { PushToTalk = b; }
    public void OnBackgroundVolume(float v) { BackgroundVolume = v; BackSource.volume = v; }

    /// <summary>
    /// 
    /// </summary>
    void SetUpUI()
    {
#if LOCALIZATION
        MaxPlayerText.text = string.Format("{0} {1}", maxPlayers[players], bl_Localization.Instance.GetTextPlural("player"));
        RoundTimeText.text = (RoomTime[r_Time] / 60) + string.Format(" {0}", bl_Localization.Instance.GetTextPlural("minute")); 
        MaxKillsText.text = (RoomKills[CurrentMaxKills]) + string.Format(" {0}", bl_Localization.Instance.GetTextPlural("kill")); 
#else
        MaxPlayerText.text = string.Format("{0} {1}", maxPlayers[players], bl_GameTexts.Players);
        RoundTimeText.text = (RoomTime[r_Time] / 60) + " Minutes";
        MaxKillsText.text = (RoomKills[CurrentMaxKills]) + " Kills";
#endif
        GameModeText.text = GameModes[CurrentGameMode].m_GameMode.GetName();
        MapNameText.text = bl_GameData.Instance.AllScenes[CurrentScene].RealSceneName;
        MapPreviewImage.sprite = bl_GameData.Instance.AllScenes[CurrentScene].Preview;
        SensitivitySlider.value = m_sensitive;
        AimSensitivitySlider.value = AimSensitivity;
        BackgroundVolumeSlider.maxValue = MaxBackgroundVolume;
        BackgroundVolumeSlider.value = BackgroundVolume;
        VolumeSlider.value = m_volume;
        QualityDropdown.ClearOptions();
        List<Dropdown.OptionData> od = new List<Dropdown.OptionData>();
        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            Dropdown.OptionData data = new Dropdown.OptionData();
            data.text = QualitySettings.names[i].ToUpper();
            od.Add(data);
        }
        WithBotsToggle.gameObject.SetActive(true);
        QualityDropdown.AddOptions(od);
        QualityDropdown.value = m_currentQuality;
        PlayerClass p = PlayerClass.Assault; p = p.GetSavePlayerClass();
        OnChangeClass((int)p);
        MaxPingText.text = string.Format("{0} ms", MaxPing[CurrentMaxPing]);
        InvertMouseXToogle.isOn = imh;
        InvertMouseYToogle.isOn = imv;
        PushToTalkToogle.isOn = PushToTalk;
        MuteVoiceToggle.isOn = MuteVoice;
        FrameRateToggle.isOn = FrameRate;
        OnChangeQuality(m_currentQuality);
        OnChangeAniso(m_stropic);
#if BDGM || ELIM
        BDRoundText.text = BDRounds[CurrentBDRound] + " Rounds";
#else
        BDRoundText.transform.parent.gameObject.SetActive(false);
#endif
#if LM
        LevelIcon.gameObject.SetActive(true);
        LevelIcon.sprite = bl_LevelManager.Instance.GetLevel().Icon;
#else
        LevelIcon.gameObject.SetActive(false);
#endif
#if ULSP && CLANS
        AddonsButtons[7].SetActive(true);
#endif
#if SHOP
        AddonsButtons[9].SetActive(true);
#endif
        SetRegionDropdown();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Save()
    {
        PlayerPrefs.SetInt(PropertiesKeys.Quality, m_currentQuality);
        PlayerPrefs.SetInt(PropertiesKeys.Aniso, m_stropic);
        PlayerPrefs.SetFloat(PropertiesKeys.Volume, m_volume);
        PlayerPrefs.SetFloat(PropertiesKeys.BackgroundVolume, BackgroundVolume);
        PlayerPrefs.SetFloat(PropertiesKeys.Sensitivity, m_sensitive);
        PlayerPrefs.SetFloat(PropertiesKeys.SensitivityAim, AimSensitivity);
        PlayerPrefs.SetInt(PropertiesKeys.WeaponFov, m_WeaponFov);
        PlayerPrefs.SetInt(PropertiesKeys.Quality, m_currentQuality);
        PlayerPrefs.SetInt(PropertiesKeys.Aniso, m_stropic);
        PlayerPrefs.SetInt(PropertiesKeys.MuteVoiceChat, MuteVoice ? 1 : 0);
        PlayerPrefs.SetInt(PropertiesKeys.PushToTalk, PushToTalk ? 1 : 0);
        PlayerPrefs.SetInt(PropertiesKeys.FrameRate, FrameRate ? 1 : 0);
        PlayerPrefs.SetInt(PropertiesKeys.InvertMouseHorizontal, imh ? 1 : 0);
        PlayerPrefs.SetInt(PropertiesKeys.InvertMouseVertical, imv ? 1 : 0);
    }

    public void ResetSettings()
    {
        LoadSettings();
        SetUpUI();
    }

    void SetRegionDropdown()
    {
        string key = PlayerPrefs.GetString(PropertiesKeys.PreferredRegion, DefaultServer.ToString());
        string[] Regions = Enum.GetNames(typeof(SeverRegionCode));
        for (int i = 0; i < Regions.Length; i++)
        {
            if (key == Regions[i])
            {
                int id = i;
                if (id > 4) { id--; }
                ServersDropdown.value = id;
                break;
            }
        }
        ServersDropdown.RefreshShownValue();
    }

    public void UpdateCoinsText()
    {
#if ULSP
        if (DataBase != null && !DataBase.isGuest)
        {
            PlayerCoinsText.text = DataBase.LocalUser.Coins.ToString();
        }
#else
            bl_GameData.Instance.VirtualCoins.LoadCoins(PhotonNetwork.NickName);
            PlayerCoinsText.text = bl_GameData.Instance.VirtualCoins.UserCoins.ToString();
#endif
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    void GetPlayerName()
    {
        if (string.IsNullOrEmpty(PhotonNetwork.NickName))
        {
#if ULSP
            if (DataBase != null && !DataBase.isGuest)
            {
                playerName = DataBase.LocalUser.NickName;
                PhotonNetwork.NickName = playerName;
                PlayerCoinsText.text = DataBase.LocalUser.Coins.ToString();
                GoToMainMenu();
            }
            else
            {
                GeneratePlayerName();
            }
#else
            GeneratePlayerName();
#endif
        }
        else
        {
#if ULSP
            if (DataBase != null && !DataBase.isGuest)
            {
                PlayerCoinsText.text = DataBase.LocalUser.Coins.ToString();
            }
#else
            bl_GameData.Instance.VirtualCoins.LoadCoins(PhotonNetwork.NickName);
            PlayerCoinsText.text = bl_GameData.Instance.VirtualCoins.UserCoins.ToString();
#endif
            playerName = PhotonNetwork.NickName;
            GoToMainMenu();
        }
    }

    void GeneratePlayerName()
    {
        if (!PlayerPrefs.HasKey(PropertiesKeys.PlayerName) || !bl_GameData.Instance.RememberPlayerName)
        {
            playerName = string.Format(PlayerNamePrefix, Random.Range(1, 9999));
        }
        else if (bl_GameData.Instance.RememberPlayerName)
        {
            playerName = PlayerPrefs.GetString(PropertiesKeys.PlayerName, string.Format(PlayerNamePrefix, Random.Range(1, 9999)));
        }
        PlayerNameField.text = playerName;
        PhotonNetwork.NickName = playerName;
        ChangeWindow(0);
    }

    /// <summary>
    /// 
    /// </summary>
    void GoToMainMenu()
    {
        StartCoroutine(Fade(LobbyState.MainMenu, 1.2f));
        if (!PhotonNetwork.IsConnected)
        {
            ConnectPhoton();
        }
        else
        {
            if (!PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby();
            }
            else
            {
                ChangeWindow(MainMenuWindow, 1);
                if (Chat != null && !Chat.isConnected() && bl_GameData.Instance.UseLobbyChat) { Chat.Connect(bl_GameData.Instance.GameVersion); }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private IEnumerator MoveToGameScene()
    {
        //Wait for check
        if (Chat != null && Chat.isConnected()) { Chat.Disconnect(); }
        while (!PhotonNetwork.InRoom)
        {
            yield return null;
        }
        PhotonNetwork.IsMessageQueueRunning = false;
        bl_UtilityHelper.LoadLevel((string)PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.SceneNameKey]);
    }
    /// <summary>
    /// 
    /// </summary>
	AudioSource PlayAudioClip(AudioClip clip, Vector3 position, float volume)
    {
        GameObject go = new GameObject("One shot audio");
        go.transform.position = position;
        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.Play();
        Destroy(go, clip.length);
        return source;
    }

    /// <summary>
    /// 
    /// </summary>
    public void ShowLevelList()
    {
#if LM
        bl_LevelPreview lp = FindObjectOfType<bl_LevelPreview>();
        if (lp != null)
        {
            lp.ShowList();
        }
#endif
    }

    public void ShowBuyCoins()
    {
#if SHOP
        bl_ShopManager.Instance.BuyCoinsWindow.SetActive(true);
#else
        Debug.Log("Require shop addon.");
#endif
    }

    IEnumerator StartFade()
    {
        BlackScreen.alpha = 1;
        LoadingScreen.gameObject.SetActive(true);
#if LOCALIZATION
        LoadingScreenText.text = bl_Localization.Instance.GetText(39);
#else
        LoadingScreenText.text = bl_GameTexts.LoadingLocalContent;
#endif
        if (!bl_GameData.isDataCached)
        {
            yield return StartCoroutine(bl_GameData.AsyncLoadData());
            yield return new WaitForEndOfFrame();
            if (bl_GameData.Instance.Lock60FPS)
            {
                Application.targetFrameRate = 60;
            }
            else { Application.targetFrameRate = 0; }
            SetUpGameModes();
            LoadSettings();
            SetUpUI();
        }
        float d = 1;
        yield return new WaitForSeconds(0.5f);
        while (d > 0)
        {
            d -= Time.deltaTime / 1;
            BlackScreen.alpha = FadeCurve.Evaluate(d);
            yield return new WaitForEndOfFrame();
        }
        yield return StartCoroutine(ShowLoadingScreen(true, 2));
        GetPlayerName();
    }

    IEnumerator ShowLoadingScreen(bool autoHide = false, float showTime = 2)
    {
        LoadingScreen.gameObject.SetActive(true);
        LoadingScreen.alpha = 1;
        Animator bottomAnim = LoadingScreen.GetComponentInChildren<Animator>();
        bottomAnim.SetBool("show", true);
        bottomAnim.Play("show", 0, 0);
        if (autoHide)
        {
            yield return new WaitForSeconds(showTime);
            float d = 1;
            bottomAnim.SetBool("show", false);
            while (d > 0)
            {
                d -= Time.deltaTime / 0.5f;
                LoadingScreen.alpha = FadeCurve.Evaluate(d);
                yield return new WaitForEndOfFrame();
            }
            LoadingScreen.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator Fade(LobbyState t_state, float t = 2.0f)
    {
        alpha = 0.0f;
        while (alpha < t)
        {
            alpha += Time.deltaTime * 2;
            CanvasGroupRoot.alpha = alpha;
            yield return null;
        }
    }

    IEnumerator FadeAudioBack(bool up)
    {
        if (up)
        {
            BackSource.Play();
            while (BackSource.volume < BackgroundVolume)
            {
                BackSource.volume += Time.deltaTime * 0.01f;
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            while (BackSource.volume > 0)
            {
                BackSource.volume -= Time.deltaTime * 0.5f;
                yield return new WaitForEndOfFrame();
            }
        }
    }

    // ROOMLIST
    /// <summary>
    /// 
    /// </summary>
    void LoadSettings()
    {
        m_currentQuality = PlayerPrefs.GetInt(PropertiesKeys.Quality, bl_GameData.Instance.DefaultSettings.DefaultQualityLevel);
        m_stropic = PlayerPrefs.GetInt(PropertiesKeys.Aniso, bl_GameData.Instance.DefaultSettings.DefaultAnisoTropic);
        m_volume = PlayerPrefs.GetFloat(PropertiesKeys.Volume, bl_GameData.Instance.DefaultSettings.DefaultVolume);
        BackgroundVolume = PlayerPrefs.GetFloat(PropertiesKeys.BackgroundVolume, MaxBackgroundVolume);
        BackSource.volume = BackgroundVolume;
        AudioListener.volume = m_volume;
        m_sensitive = PlayerPrefs.GetFloat(PropertiesKeys.Sensitivity, bl_GameData.Instance.DefaultSettings.DefaultSensitivity);
        AimSensitivity = PlayerPrefs.GetFloat(PropertiesKeys.SensitivityAim, bl_GameData.Instance.DefaultSettings.DefaultSensitivityAim);
        m_WeaponFov = PlayerPrefs.GetInt(PropertiesKeys.WeaponFov, bl_GameData.Instance.DefaultSettings.DefaultWeaponFoV);
        FrameRate = (PlayerPrefs.GetInt(PropertiesKeys.FrameRate, bl_GameData.Instance.DefaultSettings.DefaultShowFrameRate ? 1 : 0) == 1);
        imv = (PlayerPrefs.GetInt(PropertiesKeys.InvertMouseVertical, 0) == 1);
        imh = (PlayerPrefs.GetInt(PropertiesKeys.InvertMouseHorizontal, 0) == 1);
        MuteVoice = (PlayerPrefs.GetInt(PropertiesKeys.MuteVoiceChat, 0) == 1);
        PushToTalk = (PlayerPrefs.GetInt(PropertiesKeys.PushToTalk, 0) == 1);
        bool km = PlayerPrefs.GetInt(PropertiesKeys.KickKey, 0) == 1;
        KickMessageUI.SetActive(km);
        PingKickMessageUI.SetActive(bl_PhotonGame.Instance.hasPingKick);
        PlayerPrefs.SetInt(PropertiesKeys.KickKey, 0);
        if (bl_PhotonGame.Instance.hasAFKKick) { AFKKickMessageUI.SetActive(true); bl_PhotonGame.Instance.hasAFKKick = false; }
#if LM
        if (bl_LevelManager.Instance.isNewLevel)
        {
            LevelInfo info = bl_LevelManager.Instance.GetLevel();
            m_LevelUI.Icon.sprite = info.Icon;
            m_LevelUI.LevelNameText.text = info.Name;
            m_LevelUI.Root.SetActive(true);
            bl_LevelManager.Instance.Refresh();
        }
        bl_LevelManager.Instance.GetInfo();
#endif
#if ULSP
        if (DataBase != null && DataBase.isLogged)
        {
            bl_GameData.Instance.VirtualCoins.UserCoins = DataBase.LocalUser.Coins;
        }
#endif
#if CUSTOMIZER
        AddonsButtons[2].SetActive(true);
#endif
    }


    void OnLanguageChange(Dictionary<string, string> lang)
    {
#if LOCALIZATION
        NoRoomText.text = bl_Localization.Instance.GetText("norooms");
        MaxPlayerText.text = string.Format("{0} {1}", maxPlayers[players], bl_Localization.Instance.GetTextPlural("player"));
        RoundTimeText.text = (RoomTime[r_Time] / 60) + string.Format(" {0}", bl_Localization.Instance.GetTextPlural("minute"));
        MaxKillsText.text = (RoomKills[CurrentMaxKills]) + string.Format(" {0}", bl_Localization.Instance.GetTextPlural("kill"));
#endif
    }

    public void OnConnected()
    {
        FirstConnectionMade = true;
        Debug.Log("Server connection established to: " + PhotonNetwork.CloudRegion);
    }

    public void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        if (cause == DisconnectCause.DisconnectByClientLogic)
        {
            if (AppQuit)
            {
                Debug.Log("Disconnect from Server!");
                return;
            }
            if (PendingRegion == -1)
            {
                Debug.Log("Disconnect from cloud!");
            }
            else if (serverchangeRequested)
            {
                Debug.Log("Changing server!");
                ChangeServerCloud(PendingRegion);
            }
            else
            {
                Debug.Log("Disconnect from Server.");
            }
        }
        else
        {
            FadeImage.SetActive(false);
#if LOCALIZATION
            DisconnectCauseUI.GetComponentInChildren<Text>().text = string.Format(bl_Localization.Instance.GetText(41), cause.ToString());
#else
            DisconnectCauseUI.GetComponentInChildren<Text>().text = string.Format(bl_GameTexts.DisconnectCause, cause.ToString());
#endif
            DisconnectCauseUI.SetActive(true);
            Debug.LogWarning("Failed to connect to server, cause: " + cause);
        }
    }

    public void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("We received a new room list, total rooms: " + roomList.Count);
        ServerList(roomList);
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {

    }

    public void OnJoinedLobby()
    {
        Debug.Log("Player joined to the lobby: " + PhotonNetwork.LocalPlayer.UserId);
        StartCoroutine(Fade(LobbyState.MainMenu));
        ChangeWindow(MainMenuWindow, 1);
        if (PendingRegion != -1) { }
        StartCoroutine(ShowLoadingScreen(true, 2));
        if (Chat != null && !Chat.isConnected() && bl_GameData.Instance.UseLobbyChat) { Chat.Connect(bl_GameData.Instance.GameVersion); }
    }

    public void OnJoinedRoom()
    {
        Debug.Log("We have joined a room.");
        StartCoroutine(MoveToGameScene());
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {

    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {

    }

    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {

    }

    public void OnLeftLobby()
    {

    }

    public void OnFriendListUpdate(List<FriendInfo> friendList)
    {

    }

    public void OnCreatedRoom()
    {

    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {

    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {

    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
        OnNoRoomsToJoin(returnCode, message);
    }

    public void OnLeftRoom()
    {

    }

    private int m_WeaponFov;
    [Serializable]
    public class LevelUI
    {
        public GameObject Root;
        public Image Icon;
        public Text LevelNameText;
    }
}