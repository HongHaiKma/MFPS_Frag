using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class bl_GameData : ScriptableObject
{

    [Header("Game Settings")]
    public bool UseLobbyChat = true;
    public bool UseVoiceChat = true;
    public bool BulletTracer = false;
    public bool DropGunOnDeath = true;
    public bool SelfGrenadeDamage = true;
    public bool CanFireWhileRunning = true;
    public bool HealthRegeneration = true;
    public bool ShowTeamMateHealthBar = true;
    public bool CanChangeTeam = false;
    public bool KillCamFollowEnemys = true;
    public bool ShowBlood = true;
    public bool DetectAFK = false;
    public bool MasterCanKickPlayers = true;
    public bool ArriveKitsCauseDamage = true;
    public bool CalculateNetworkFootSteps = false;
    public bool Lock60FPS = true;
    public bool ShowNetworkStats = false;
    public bool RememberPlayerName = true;
    public bool ShowWeaponLoadout = true;
#if MFPSM
    public bool AutoWeaponFire = false;
#endif
#if LM
    public bool LockWeaponsByLevel = true;
#endif
    public AmmunitionType AmmoType = AmmunitionType.Bullets;

    [Header("Rewards")]
    public ScoreRewards ScoreReward;
    public VirtualCoin VirtualCoins;

    [Header("Settings")]
    public string GameVersion = "1.0";
    public float AFKTimeLimit = 60;
    [Range(1, 10)] public float PlayerRespawnTime = 5.0f;
    public int MaxChangeTeamTimes = 3;
    public string MainMenuScene = "MainMenu";
    public string OnDisconnectScene = "MainMenu";

    [Header("Levels Manager")]
    /*[Reorderable]*/
    public List<SceneInfo> AllScenes = new List<SceneInfo>();

    [Header("Weapons")]
   /* [Reorderable]*/
    public List<bl_GunInfo> AllWeapons = new List<bl_GunInfo>();

    [Header("Default Settings")]
    public DefaultSettingsData DefaultSettings;

    [Header("Game Modes Available")]
    public List<GameModesEnabled> m_GameModesAvailables = new List<GameModesEnabled>();

    [Header("Teams")]
    public string Team1Name = "Delta";
    public Color Team1Color = Color.blue;
    [Space(5)]
    public string Team2Name = "Recon";
    public Color Team2Color = Color.green;

    [Header("Players")]
    public GameObject Player1;
    public GameObject Player2;

    [Header("Bots")]
    public GameObject BotTeam1;
    public GameObject BotTeam2;

    [Header("Game Team")]
    public List<GameTeamInfo> GameTeam = new List<GameTeamInfo>();

    [HideInInspector] public GameTeamInfo CurrentTeamUser = null;
    [HideInInspector] public bool isChating = false;

    [HideInInspector] public string _MFPSLicense = string.Empty;
    [HideInInspector] public int _MFPSFromStore = 2;

    
    public bl_GunInfo GetWeapon(int ID)
    {
        if (ID < 0 || ID > AllWeapons.Count - 1)
            return AllWeapons[0];
        
        return AllWeapons[ID];
    }

    public string[] AllWeaponStringList()
    {
        return AllWeapons.Select(x => x.Name).ToList().ToArray();
    }
    
    public int GetWeaponID(string gunName)
    {
        int id = -1;
        if(AllWeapons.Exists(x => x.Name == gunName))
        {
            id = AllWeapons.FindIndex(x => x.Name == gunName);
        }
        return id;
    }

    /// <summary>
    /// 
    /// </summary>
    public int CheckPlayerName(string pName)
    {
        for (int i = 0; i < GameTeam.Count; i++)
        {
            if (pName == GameTeam[i].UserName)
            {
                return 1;
            }
        }
        if (pName.Contains('[') || pName.Contains('{'))
        {
            return 2;
        }
        CurrentTeamUser = null;
        return 0;
    }

    /// <summary>
    /// 
    /// </summary>
    public bool CheckPasswordUse(string PName, string Pass)
    {
        for (int i = 0; i < GameTeam.Count; i++)
        {
            if (PName == GameTeam[i].UserName)
            {
               if(Pass == GameTeam[i].Password)
                {
                    CurrentTeamUser = GameTeam[i];
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
#if CLANS
    private string _role = string.Empty;
#endif
    public string RolePrefix
    {
        get
        {
#if !CLANS
            if (CurrentTeamUser != null)
            {
                return string.Format(" <color=#{1}>[{0}]</color>", CurrentTeamUser.m_Role.ToString(), ColorUtility.ToHtmlStringRGBA(CurrentTeamUser.m_Color));
            }
            else
            {
                return string.Empty;
            }
#else
            if(bl_DataBase.Instance == null || !bl_DataBase.Instance.isLogged || !bl_DataBase.Instance.LocalUser.Clan.haveClan)
            {
                return string.Empty;
            }
            else
            {
                if (string.IsNullOrEmpty(_role))
                {
                    _role = string.Format(" [{0}]", bl_DataBase.Instance.LocalUser.Clan.Name);
                }
                return _role;
            }
#endif
        }

    }

    void OnDisable()
    {
        isDataCached = false;
    }

    [System.Serializable]
    public class GameTeamInfo
    {
        public string UserName;
        public Role m_Role = Role.Moderator;
        public string Password;
        public Color m_Color;

        public enum Role
        {
            Admin = 0,
            Moderator = 1,
        }
    }

    /// <summary>
    /// cache the GameData from Resources asynchronous to avoid freeze the main thread
    /// </summary>
    /// <returns></returns>
    public static IEnumerator AsyncLoadData()
    {
        if (m_Data == null)
        {
            isCaching = true;
            ResourceRequest rr = Resources.LoadAsync("GameData", typeof(bl_GameData));
            while (!rr.isDone) { yield return null; }
            m_Data = rr.asset as bl_GameData;
            isCaching = false;
        }
        isDataCached = true;
    }

    public static bool isDataCached = false;
    private static bool isCaching = false;
    private static bl_GameData m_Data;
    public static bl_GameData Instance
    {
        get
        {
            if (m_Data == null && !isCaching)
            {
                m_Data = Resources.Load("GameData", typeof(bl_GameData)) as bl_GameData;
            }
            return m_Data;
        }
    }

    [System.Serializable]
    public class ScoreRewards
    {
        public int ScorePerKill = 50;
        public int ScorePerHeadShot = 25;
        public int ScoreForWinMatch = 100;
        [Tooltip("Per minute played")]
        public int ScorePerTimePlayed = 3;
    }

    [System.Serializable]
    public class VirtualCoin
    {
        public int InitialCoins = 1000;
        [Tooltip("how much score/xp worth one coin")]
        public int CoinScoreValue = 1000;//how much score/xp worth one coin

        public int UserCoins { get; set; }

        public void LoadCoins(string userName)
        {
            UserCoins = PlayerPrefs.GetInt(string.Format("{0}.{1}", userName, PropertiesKeys.UserCoins), InitialCoins);
        }

        public void SetCoins(int coins, string userName)
        {
            LoadCoins(userName);
            int total = UserCoins + coins;
            PlayerPrefs.SetInt(string.Format("{0}.{1}", userName, PropertiesKeys.UserCoins), total);
            UserCoins = total;
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        for (int i = 0; i < AllScenes.Count; i++)
        {
            if (AllScenes[i].m_Scene == null) continue;
            AllScenes[i].RealSceneName = AllScenes[i].m_Scene.name;
        }
    }
#endif

    [Serializable]
    public class SceneInfo
    {
        public string ShowName;
        [SerializeField]
        public Object m_Scene;
        [HideInInspector] public string RealSceneName;
        public Sprite Preview;
    }

    [Serializable]
    public class DefaultSettingsData
    {
        [Range(1, 20)] public float DefaultSensitivity = 5.0f;
        [Range(1, 20)] public float DefaultSensitivityAim = 2;
        public int DefaultQualityLevel = 3;
        public int DefaultAnisoTropic = 2;
        [Range(0, 1)] public float DefaultVolume = 1;
        [Range(40, 100)] public int DefaultWeaponFoV = 60;
        public bool DefaultShowFrameRate = false;
        public bool DefaultMotionBlur = true;
    }

    [Serializable]
    public class GameModesEnabled
    {
        public string ModeName;
        public GameMode m_GameMode;
        public bool isEnabled = true;
    }
}