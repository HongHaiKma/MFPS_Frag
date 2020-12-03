using UnityEngine;
using System.Collections;

public static class PropertiesKeys
{
    //Room
    public const string TimeRoomKey = "TimeRoom";
    public const string GameModeKey = "GameMode";
    public const string SceneNameKey = "LevelName";
    public const string CustomSceneName = "CustomSceneName";
    public const string RoomRoundKey = "GamePerRounds";
    public const string RoomMaxKills = "RoomMaxKills";
    public const string TeamSelectionKey = "AutoTeam";
    public const string RoomFriendlyFire = "RoomFrienlyFire";
    public const string Team1Score = "Team1Score";
    public const string Team2Score = "Team2Score";
    public const string MaxPing = "MaxPing";
    public const string RoomPassworld = "RoomPassworld";
    public const string WithBotsKey = "withbots";
    public const string NumberOfRounds = "GameRounds";
    public const string BombDefuseRounds = "BDRounds";

    //Player
    public const string TeamKey = "Team";
    public const string KillsKey = "Kills";
    public const string DeathsKey = "Deaths";
    public const string ScoreKey = "Score";
    public const string KickKey = "Kick";
    public const string UserRole = "UserRole";
    //Prefs
    public const string WeaponFov = "mfps.weapon.fov";
    public const string Quality = "mfps.settings.quality";
    public const string Aniso = "mfps.settings.aniso";
    public const string Volume = "mfps.settings.volume";
    public const string BackgroundVolume = "mfps.settings.backvolume";
    public const string Sensitivity = "mfps.settings.sensitivity";
    public const string SensitivityAim = "mfps.settings.sensitivity.aim";
    public const string SSAO = "mfps.settings.ssao";
    public const string FrameRate = "mfps.settings.framerate";
    public const string InvertMouseVertical = "mfps.settings.imv";
    public const string InvertMouseHorizontal = "mfps.settings.imh";
    public const string MuteVoiceChat = "mfps.settings.mutevoicechat";
    public const string PushToTalk = "mfps.settings.pushtotalk";
    public const string PreferredRegion = "mfps.lobby.region";
    public const string MotionBlur = "mfps.settings.motionb";
    public const string PlayerName = "mfps.game.username";
    //Event Code
    public const byte KickPlayerEvent = 101;

    //Unique User
    public static string UserCoins { get { return string.Format("{0}.mfps.coins", PlayerPrefs.GetString(PlayerName)); } }
    public static string GetUniqueKey(string key) { return string.Format("{0}.{1}.{2}", Application.companyName, Application.productName, key); }

    public static bool GetBoolPrefs(this string key, bool defaultValue)
    {
        return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1 ? true : false;
    }

    public static void SetBoolPrefs(this string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
    }
}