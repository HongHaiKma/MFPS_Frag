using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable; //Replace default Hashtables with Photon hashtables
using Photon.Pun;
using Photon.Realtime;

public class bl_RoomSettings : bl_MonoBehaviour
{

    //Private
    [HideInInspector] public int Team_1_Score = 0;
    [HideInInspector] public int Team_2_Score = 0;
    private bl_RoomMenu RoomMenu;
    private bl_RoundTime TimeManager;
    [HideInInspector] public GameMode m_GameMode = GameMode.FFA;
    private bl_UIReferences UIReferences;
    private int MaxKills = 0;
    private bl_GameData GameData;
    private bl_AIMananger AIMananger;
#if BDGM
    private bl_BombDefuse BombDefuse;
#endif
#if CP
        private bl_CoverPointRoom CoverPoint;
#endif

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
            return;

        RoomMenu = base.GetComponent<bl_RoomMenu>();
        TimeManager = base.GetComponent<bl_RoundTime>();
        AIMananger = FindObjectOfType<bl_AIMananger>();
        UIReferences = bl_UIReferences.Instance;
        GameData = bl_GameData.Instance;
#if BDGM
        BombDefuse = FindObjectOfType<bl_BombDefuse>();
#endif
#if CP
         CoverPoint = FindObjectOfType<bl_CoverPointRoom>();
#endif
        ResetRoom();
        GetRoomInfo();
    }

   
    /// <summary>
    /// 
    /// </summary>
    public void ResetRoom()
    {
        Hashtable table = new Hashtable();
        //Initialize new properties where the information will stay Room
        if (PhotonNetwork.IsMasterClient)
        {
            table.Add(PropertiesKeys.Team1Score, 0);
            table.Add(PropertiesKeys.Team2Score, 0);
            PhotonNetwork.CurrentRoom.SetCustomProperties(table);
        }
        table.Clear();
        //Initialize new properties where the information will stay Players
        table.Add(PropertiesKeys.TeamKey, Team.None.ToString());
        table.Add(PropertiesKeys.KillsKey, 0);
        table.Add(PropertiesKeys.DeathsKey, 0);
        table.Add(PropertiesKeys.ScoreKey, 0);
        table.Add(PropertiesKeys.UserRole, bl_GameData.Instance.RolePrefix);
        PhotonNetwork.LocalPlayer.SetCustomProperties(table);

#if ULSP && LM
        bl_DataBase db = FindObjectOfType<bl_DataBase>();
        int scoreLevel = 0;
        if (db != null)
        {
            scoreLevel = db.LocalUser.Score;
        }
        Hashtable PlayerTotalScore = new Hashtable();
        PlayerTotalScore.Add("TotalScore", scoreLevel);
        PhotonNetwork.LocalPlayer.SetCustomProperties(PlayerTotalScore);
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    void GetRoomInfo()
    {
        m_GameMode = GetGameMode;
        if (m_GameMode == GameMode.FFA)
        {
            m_GameMode = GameMode.FFA;
            bl_UIReferences.Instance.PlayerUI.FFAScoreboard.SetActive(true);
            bl_UIReferences.Instance.PlayerUI.TwoTeamsScoreboard.SetActive(false);
            bl_GameManager.Instance.SetGameState(MatchState.Starting);
        }
        else if (m_GameMode == GameMode.TDM)
        {
            bl_UIReferences.Instance.PlayerUI.TwoTeamsScoreboard.SetActive(true);
            bl_UIReferences.Instance.PlayerUI.FFAScoreboard.SetActive(false);
            bl_GameManager.Instance.SetGameState(MatchState.Starting);
        }
        else if (m_GameMode == GameMode.CTF)
        {
            bl_UIReferences.Instance.PlayerUI.TwoTeamsScoreboard.SetActive(true);
            bl_UIReferences.Instance.PlayerUI.FFAScoreboard.SetActive(false);
            bl_GameManager.Instance.WaitForPlayers(2);
        }
#if BDGM
        else if (m_GameMode == GameMode.SND)
        {
            bl_UIReferences.Instance.PlayerUI.TwoTeamsScoreboard.SetActive(true);
            bl_UIReferences.Instance.PlayerUI.FFAScoreboard.SetActive(false);
        }
#endif
#if CP
        else if (m_GameMode == GameMode.CP)
        {
            bl_UIReferences.Instance.PlayerUI.TwoTeamsScoreboard.SetActive(false);
            bl_UIReferences.Instance.PlayerUI.FFAScoreboard.SetActive(false);
            CoverPoint.CPObjects.SetActive(true);
            bl_UIReferences.Instance.PlayerUI.MaxKillsUI.SetActive(false);
        }
#endif
#if GR
        else if (m_GameMode == GameMode.GR)
        {
            bl_UIReferences.Instance.PlayerUI.TwoTeamsScoreboard.SetActive(false);
            bl_UIReferences.Instance.PlayerUI.FFAScoreboard.SetActive(true);
            bl_UIReferences.Instance.PlayerUI.MaxKillsUI.SetActive(false);
        }
#endif
#if LMS
        else if (m_GameMode == GameMode.LSM)
        {
            bl_UIReferences.Instance.PlayerUI.TwoTeamsScoreboard.SetActive(false);
            bl_UIReferences.Instance.PlayerUI.FFAScoreboard.SetActive(false);
            bl_UIReferences.Instance.PlayerUI.MaxKillsUI.SetActive(false);
        }
#endif
        else
        {
            bl_UIReferences.Instance.PlayerUI.TwoTeamsScoreboard.SetActive(false);
            bl_UIReferences.Instance.PlayerUI.FFAScoreboard.SetActive(false);
            bl_UIReferences.Instance.PlayerUI.MaxKillsUI.SetActive(false);
        }

#if CP
            CoverPoint.CPObjects.SetActive(m_GameMode == GameMode.CP);
#endif
#if BDGM
            if (BombDefuse != null)
            {
                BombDefuse.BombRoot.SetActive(m_GameMode == GameMode.SND);
            }
#endif

        TimeManager.m_RoundStyle = (RoundStyle)PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.RoomRoundKey];
        RoomMenu.AutoTeamSelection = (bool)PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.TeamSelectionKey];
        MaxKills = (int)PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.RoomMaxKills];

        QualitySettings.SetQualityLevel(PlayerPrefs.GetInt(PropertiesKeys.Quality, 3));
        AudioListener.volume = PlayerPrefs.GetFloat(PropertiesKeys.Volume, 1);
        int i = PlayerPrefs.GetInt(PropertiesKeys.Aniso, 2);
        if (i == 0)
        {
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
        }
        else if (i == 1)
        {
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
        }
        else
        {
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.Awake();
        bl_EventHandler.OnRoundEnd += this.OnRoundEnd;
        bl_PhotonCallbacks.RoomPropertiesUpdate += OnPhotonCustomRoomPropertiesChanged;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_EventHandler.OnRoundEnd -= this.OnRoundEnd;
        bl_PhotonCallbacks.RoomPropertiesUpdate -= OnPhotonCustomRoomPropertiesChanged;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (m_GameMode == GameMode.FFA)
        {
            string PlayerStartFormat = string.Format(bl_GameTexts.PlayerStart, RoomMenu.PlayerStar);
            UIReferences.PlayerUI.FFAScoreText.text = PlayerStartFormat;
        }
    }
   
    /// <summary>
    /// 
    /// </summary>
    /// <param name="propertiesThatChanged"></param>
    public void OnPhotonCustomRoomPropertiesChanged(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(PropertiesKeys.Team1Score))
        {
            Team_1_Score = PhotonNetwork.CurrentRoom.GetRoomScore(Team.Delta);
        }
        else if (propertiesThatChanged.ContainsKey(PropertiesKeys.Team2Score))
        {
            Team_2_Score = PhotonNetwork.CurrentRoom.GetRoomScore(Team.Recon);
        }
        if (UIReferences != null)
        {
            UIReferences.PlayerUI.Team1ScoreText.text = Team_1_Score.ToString();
            UIReferences.PlayerUI.Team2ScoreText.text = Team_2_Score.ToString();
        }

        CheckScore();
    }

    /// <summary>
    /// 
    /// </summary>
    public void CheckScore()
    {
        if (MaxKills <= 0) return;

        if (!isOneTeamMode)
        {
            if (Team_1_Score >= MaxKills)
            {
                TimeManager.FinishRound();
            }
            if (Team_2_Score >= MaxKills)
            {
                TimeManager.FinishRound();
            }
        }
        else
        {
            if(RoomMenu.GetPlayerStar.GetKills() >= MaxKills)
            {
                TimeManager.FinishRound();
                return;
            }
            if(AIMananger != null && AIMananger.BotsActive && AIMananger.BotsStatistics.Count > 0)
            {
                if(AIMananger.GetBotWithMoreKills().Kills >= MaxKills)
                {
                    TimeManager.FinishRound();
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnRoundEnd()
    {
        StartCoroutine(DisableUI());        
    }

    /// <summary>
    /// 
    /// </summary>
    public string GetWinnerName
    {
        get
        {
            if (!isOneTeamMode)
            {
                if (Team_1_Score > Team_2_Score)
                {
                    return GameData.Team1Name;
                }
                else if (Team_1_Score < Team_2_Score)
                {
                    return GameData.Team2Name;
                }
                else
                {
                    return bl_GameTexts.NoOneWonName;
                }
            }
            else
            {
                if (GetGameMode == GameMode.FFA)
                {
                    string winner = RoomMenu.PlayerStar;
                    if(AIMananger != null && AIMananger.GetBotWithMoreKills().Kills >= MaxKills)
                    {
                        winner = AIMananger.GetBotWithMoreKills().Name;
                    }
                    return winner;
                }
#if GR
                else if(GetGameMode == GameMode.GR)
                {
                   return FindObjectOfType<bl_GunRace>().GetWinnerPlayer.NickName;
                }
#endif
                else
                {
                    return RoomMenu.PlayerStar;
                }
            }
        }
    }

#if BDGM
    public void ResetOnDeath()
    {
        if (m_GameMode == GameMode.SND)
        {
            bl_BombDefuse[] bp = FindObjectsOfType<bl_BombDefuse>();
            if (bp.Length > 0)
            {
                for (int i = 0; i < bp.Length; i++)
                {
                    bp[i].PlayerReset();
                }
            }
            Debug.Log("Reset PlantBombs");
        }
    }
#endif

    IEnumerator DisableUI()
    {
        yield return new WaitForSeconds(10);
    }

    private static bl_RoomSettings _instance;
    public static bl_RoomSettings Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_RoomSettings>(); }
            return _instance;
        }
    }
}