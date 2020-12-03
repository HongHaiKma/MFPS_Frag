/////////////////////////////////////////////////////////////////////////////////
////////////////////////////////bl_RoomMenu.cs///////////////////////////////////
/////////////////place this in a scene for handling menus of room////////////////
/////////////////////////////////////////////////////////////////////////////////
///////////////////////////////Lovatto Studio////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

public class bl_RoomMenu : bl_MonoBehaviour
{
    [HideInInspector]
    public bool isPlaying = false;
    [HideInInspector]
    public float m_sensitive = 2.0f,SensitivityAim;
    [HideInInspector]public int WeaponCameraFog = 60;
    [HideInInspector]
    public bool ShowWarningPing = false;
    [HideInInspector]
    public List<Player> FFAPlayerSort = new List<Player>();
    [HideInInspector]
    public string PlayerStar = "";
    [HideInInspector]
    public bool showMenu = true;
    [HideInInspector]
    public bool isFinish = false;
    [HideInInspector]
    public bool SpectatorMode, WaitForSpectator = false;
    /// <summary>
    /// Reference of player class select
    /// </summary>
    public static PlayerClass PlayerClass = PlayerClass.Assault;

    [HideInInspector] public bool AutoTeamSelection = false;
    [Header("Inputs")]
    public KeyCode ScoreboardKey = KeyCode.N;
    public KeyCode PauseMenuKey = KeyCode.Escape;
    public KeyCode ChangeClassKey = KeyCode.M;
    [Header("Map Camera")]
    /// <summary>
    /// Rotate room camera?
    /// </summary>
    public bool RotateCamera = true;
    /// <summary>
    /// Rotation Camera Speed
    /// </summary>
    public float CameraRotationSpeed = 5;
    public static float m_alphafade = 3;
    [Header("LeftRoom")]
    [Range(0.0f,5)]
    public float DelayLeave = 1.5f;

    private GameObject ButtonsClassPlay = null;

    private bl_GameManager GM;  
    private bool CanSpawn = false;
    private bool AlredyAuto = false;
    private bl_UIReferences UIReferences;
    private bool m_showbuttons;
#if ULSP
    private bl_DataBase DataBase;
#endif
    private GameMode Mode;

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        if (!isConnected)
            return;

        base.Awake();
        GM = FindObjectOfType<bl_GameManager>();
        UIReferences = FindObjectOfType<bl_UIReferences>();
        #if ULSP
        DataBase = bl_DataBase.Instance;
        if(DataBase != null) { DataBase.RecordTime(); }
#endif
        Mode = GetGameMode;
        ButtonsClassPlay = UIReferences.ButtonsClassPlay;
        ShowWarningPing = false;
        showMenu = true;
        if (AutoTeamSelection)
        {
            if (!isOneTeamMode)
            {
                StartCoroutine(CanSpawnIE());
            }
            else
            {
                CanSpawn = true;
            }
        }
        GetPrefabs();
        bl_UIReferences.Instance.PlayerUI.PlayerUICanvas.enabled = false;
    }

    protected override void OnEnable()
    {
        bl_EventHandler.OnLocalPlayerSpawn += OnPlayerSpawn;
        bl_EventHandler.OnLocalPlayerDeath += OnPlayerLocalDeath;
#if MFPSM
        bl_TouchHelper.OnPause += OnPause;
#endif
        bl_PhotonCallbacks.LeftRoom += OnLeftRoom;
    }

    protected override void OnDisable()
    {
        bl_EventHandler.OnLocalPlayerSpawn -= OnPlayerSpawn;
        bl_EventHandler.OnLocalPlayerDeath -= OnPlayerLocalDeath;
#if MFPSM
        bl_TouchHelper.OnPause -= OnPause;
#endif
        bl_PhotonCallbacks.LeftRoom -= OnLeftRoom;
    }

    void OnPlayerSpawn()
    {
        bl_UIReferences.Instance.PlayerUI.PlayerUICanvas.enabled = true;
    }

    void OnPlayerLocalDeath()
    {
        bl_UIReferences.Instance.PlayerUI.PlayerUICanvas.enabled = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        PauseControll();
        ScoreboardControll();

        if (RotateCamera &&  !isPlaying && !SpectatorMode)
        {
            this.transform.Rotate(Vector3.up * Time.deltaTime * CameraRotationSpeed);
        }

        if (AutoTeamSelection && !AlredyAuto)
        {
            AutoTeam();
        }

        if (isPlaying && Input.GetKeyDown(ChangeClassKey) && ButtonsClassPlay != null && !bl_GameData.Instance.isChating)
        {
            m_showbuttons = !m_showbuttons;
            if (m_showbuttons)
            {
                if (!ButtonsClassPlay.activeSelf)
                {
                    ButtonsClassPlay.SetActive(true);
                    bl_UtilityHelper.LockCursor(false);
                }
            }
            else
            {
                if (ButtonsClassPlay.activeSelf)
                {
                    ButtonsClassPlay.SetActive(false);
                    bl_UtilityHelper.LockCursor(true);
                }
            }
        }

        if (Mode == GameMode.FFA)
        {
            FFAPlayerSort.Clear();
            FFAPlayerSort = GetPlayerList;
            if (FFAPlayerSort.Count > 0 && FFAPlayerSort != null)
            {
                FFAPlayerSort.Sort(GetSortPlayerByKills);
                PlayerStar = FFAPlayerSort[0].NickName;
            }
        }

        if (SpectatorMode && Input.GetKeyUp(KeyCode.Escape)) { bl_UtilityHelper.LockCursor(false); }
    }

    /// <summary>
    /// 
    /// </summary>
    void PauseControll()
    {
        bool pauseKey = Input.GetKeyDown(PauseMenuKey);
#if INPUT_MANAGER
        if (bl_Input.Instance.isGamePad)
        {
            pauseKey = bl_Input.isStartPad;
        }
#endif
        if (pauseKey && GM.isEnterinGamePlay && !isFinish && !SpectatorMode)
        {
            bool asb = UIReferences.isMenuActive;
            asb = !asb;
            UIReferences.ShowMenu(asb);
            bl_UtilityHelper.LockCursor(!asb);
            bl_UCrosshair.Instance.Show(!asb);
        }
    }

    public void OnPause()
    {
        if (GM.isEnterinGamePlay && !isFinish && !SpectatorMode)
        {
            bool asb = UIReferences.isMenuActive;
            asb = !asb;
            UIReferences.ShowMenu(asb);
            bl_UtilityHelper.LockCursor(!asb);
            bl_UCrosshair.Instance.Show(!asb);
        }
    }

    public bool isPaused { get { return UIReferences.isMenuActive; } }

    /// <summary>
    /// 
    /// </summary>
    void ScoreboardControll()
    {
        if (!UIReferences.isOnlyMenuActive && !isFinish)
        {
            if (Input.GetKeyDown(ScoreboardKey))
            {
                bool asb = UIReferences.isScoreboardActive;
                asb = !asb;
                UIReferences.ShowScoreboard(asb);
            }
            if (Input.GetKeyUp(ScoreboardKey))
            {
                bool asb = UIReferences.isScoreboardActive;
                asb = !asb;
                UIReferences.ShowScoreboard(asb);
            }
        }
    }

    public void OnSpectator(bool active)
    {
        SpectatorMode = active;
        bl_UtilityHelper.LockCursor(active);
        if (active)
        {
            this.GetComponentInChildren<Camera>().transform.rotation = Quaternion.identity;
        }
        GetComponentInChildren<bl_SpectatorCamera>().enabled = active;
    }

    /// <summary>
    /// Use for change player class for next Re spawn
    /// </summary>
    /// <param name="m_class"></param>
    public void ChangeClass()
    {
        if (isPlaying && GM.isEnterinGamePlay)
        {
            ButtonsClassPlay.SetActive(false);
            bl_UtilityHelper.LockCursor(true);
        }
        m_showbuttons = false;
    }

    /// <summary>
    /// 
    /// </summary>
    void AutoTeam()
    {
        string joinText = isOneTeamMode ? bl_GameTexts.JoinedInMatch : bl_GameTexts.JoinIn;
#if LOCALIZATION
         joinText = isOneTeamMode ? bl_Localization.Instance.GetText(17) : bl_Localization.Instance.GetText(23);
#endif
        if (CanSpawn && !isPlaying && !AlredyAuto)
        {
            AlredyAuto = true;
            if (!isOneTeamMode)
            {
                if (GetPlayerInDeltaCount > GetPlayerInReconCount)
                {
                    bl_UtilityHelper.LockCursor(true);
                    showMenu = false;
                    GM.SpawnPlayer(Team.Recon);
                    bl_EventHandler.KillEvent(PhotonNetwork.NickName, string.Empty, joinText + Team.Recon.GetTeamName(), Team.Recon.ToString(), 777, 30);
                    isPlaying = true;
                }
                else if (GetPlayerInDeltaCount < GetPlayerInReconCount)
                {
                    bl_UtilityHelper.LockCursor(true);
                    showMenu = false;
                    GM.SpawnPlayer(Team.Delta);
                    bl_EventHandler.KillEvent(PhotonNetwork.NickName, string.Empty, joinText + Team.Delta.GetTeamName(), Team.Delta.ToString(), 777, 30);
                    isPlaying = true;
                }
                else if (GetPlayerInDeltaCount == GetPlayerInReconCount)
                {
                    bl_UtilityHelper.LockCursor(true);
                    showMenu = false;
                    GM.SpawnPlayer(Team.Delta);
                    bl_EventHandler.KillEvent(PhotonNetwork.NickName, string.Empty, joinText + Team.Delta.GetTeamName(), Team.Delta.ToString(), 777, 30);
                    isPlaying = true;
                }
            }
            else
            {
                bl_UtilityHelper.LockCursor(true);
                showMenu = false;
                GM.SpawnPlayer(Team.All);
                bl_EventHandler.KillEvent(PhotonNetwork.NickName, string.Empty, joinText, Team.Delta.ToString(), 777, 30);
                isPlaying = true;
            }
            UIReferences.AutoTeam(false);
        }
        else
        {
            UIReferences.AutoTeam(true);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public void JoinTeam(int id)
    {

        Team t = Team.None;
        string tn = "Team";
        if (id == 0)
        {
            t = Team.All;
        }
        else if (id == 1)
        {
            t = Team.Delta;
            tn = bl_GameData.Instance.Team1Name;
        }
        else if (id == 2)
        {
            t = Team.Recon;
            tn = bl_GameData.Instance.Team2Name;
        }
        string joinText = isOneTeamMode ? bl_GameTexts.JoinedInMatch : bl_GameTexts.JoinIn;
#if LOCALIZATION
        joinText = isOneTeamMode ? bl_Localization.Instance.GetText(17) : bl_Localization.Instance.GetText(23);
#endif
        if (isOneTeamMode)
        {
            bl_EventHandler.KillEvent(PhotonNetwork.NickName, string.Empty, joinText, t.ToString(), 777, 30);
        }
        else
        {
            string jt = string.Format("{0} {1}", joinText, tn);
            bl_EventHandler.KillEvent(PhotonNetwork.NickName, string.Empty, jt, t.ToString(), 777, 30);
        }
        showMenu = false;
#if !PSELECTOR
        bl_UtilityHelper.LockCursor(true);
        isPlaying = true;
#endif
#if ELIM
        if (GetGameMode == GameMode.ELIM && GM.GameMatchState == MatchState.Playing)
        {
            bl_Elimination.Instance.SetSpectatorCamera(t);
            return;
        }
#endif
        GM.SpawnPlayer(t);
    }

    /// <summary>
    /// 
    /// </summary>
    public void LeftOfRoom()
    {
#if ULSP
        if (DataBase != null)
        {
            Player p = PhotonNetwork.LocalPlayer;
            DataBase.SaveData(p.GetPlayerScore(), p.GetKills(), p.GetDeaths());
            DataBase.StopAndSaveTime();
        }
#endif
        //Good place to save info before reset statistics
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            bl_UtilityHelper.LoadLevel(bl_GameData.Instance.MainMenuScene);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Suicide()
    {
        PhotonView view = PhotonView.Find(bl_GameManager.m_view);
        if (view != null)
        {

            bl_PlayerDamageManager pdm = view.GetComponent<bl_PlayerDamageManager>();
            pdm.Suicide();
            bl_UtilityHelper.LockCursor(true);
            showMenu = false;
            if (view.IsMine)
            {
                bl_GameManager.SuicideCount++;
                //Debug.Log("Suicide " + bl_GameManager.SuicideCount + " times");
                //if player is a joker o abuse of suicide, them kick of room
                if (bl_GameManager.SuicideCount >= 3)//Max number of suicides  = 3, you can change
                {
                    isPlaying = false;
                    bl_GameManager.isAlive = false;
                    bl_UtilityHelper.LockCursor(false);
                    LeftOfRoom();
                }
            }
        }
        else
        {
            Debug.LogError("This view " + bl_GameManager.m_view + " is not found");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void GetPrefabs()
    {
        PlayerClass = PlayerClass.GetSavePlayerClass();
#if CLASS_CUSTOMIZER
        PlayerClass = bl_ClassManager.Instance.m_Class;
#endif
        UIReferences.OnChangeClass(PlayerClass);
    }

    /// <summary>
    /// 
    /// </summary>
    public int GetStartPlayerScore
    {
        get
        {
            if(FFAPlayerSort.Count > 0)
            {
                return FFAPlayerSort[0].GetPlayerScore();
            }
            else
            {
                return 0;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public Player GetPlayerStar
    {
        get
        {
            if (FFAPlayerSort.Count <= 0)
            {
                return PhotonNetwork.LocalPlayer;
            }
            else
            {
                return FFAPlayerSort[0];
            }
        }
    }
    /// <summary>
    /// Get All Player in Room List
    /// </summary>
    public List<Player> GetPlayerList
    {
        get
        {
            List<Player> list = new List<Player>();
            foreach (Player players in PhotonNetwork.PlayerList)
            {
                list.Add(players);
            }
            return list;
        }
    }
    /// <summary>
    /// Get the total players in team Delta
    /// </summary>
    public int GetPlayerInDeltaCount
    {
        get
        {
            int count = 0;
            foreach (Player players in PhotonNetwork.PlayerList)
            {
                if ((string)players.CustomProperties[PropertiesKeys.TeamKey] == Team.Delta.ToString())
                {
                    count++;
                }
            }
            return count;
        }
    }
    /// <summary>
    /// Get the total players in team Recon
    /// </summary>
    public int GetPlayerInReconCount
    {
        get
        {
            int count = 0;
            foreach (Player players in PhotonNetwork.PlayerList)
            {
                if ((string)players.CustomProperties[PropertiesKeys.TeamKey] == Team.Recon.ToString())
                {
                    count++;
                }
            }
            return count;
        }
    }

    /// <summary>
    /// Sort Player by Kills,for more info watch this: http://answers.unity3d.com/questions/233917/custom-sorting-function-need-help.html
    /// </summary>
    /// <returns></returns>
    private static int GetSortPlayerByKills(Player player1, Player player2)
    {
        if (player1.CustomProperties[PropertiesKeys.KillsKey] != null && player2.CustomProperties[PropertiesKeys.KillsKey] != null)
        {
            return (int)player2.CustomProperties[PropertiesKeys.KillsKey] - (int)player1.CustomProperties[PropertiesKeys.KillsKey];
        }
        else
        {
            return 0;
        }
    }

    IEnumerator CanSpawnIE()
    {
        yield return new WaitForSeconds(3);
        CanSpawn = true;
    }

    private bool imv = false;
    public bool SetIMV
    {
        get
        {
            return imv;
        }set
        {
            imv = value;
            PlayerPrefs.SetInt(PropertiesKeys.InvertMouseVertical, (value) ? 1 : 0);
        }
    }

      private bool imh = false;
    public bool SetIMH
    {
        get
        {
            return imh;
        }
        set
        {
            imh = value;
            PlayerPrefs.SetInt(PropertiesKeys.InvertMouseHorizontal, (value) ? 1 : 0);
        }
    }

    public bool isMenuOpen
    {
        get
        {
            return UIReferences.State != bl_UIReferences.RoomMenuState.Hidde;
        }
    }

   public void OnLeftRoom()
   {
       Debug.Log("OnLeftRoom (local)");      
       this.GetComponent<bl_RoundTime>().enabled = false;
       StartCoroutine(UIReferences.FinalFade(true));
   }

    private static bl_RoomMenu _instance;
    public static bl_RoomMenu Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_RoomMenu>(); }
            return _instance;
        }
    }
}