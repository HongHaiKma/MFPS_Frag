using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class bl_PlayerScoreboardUI : MonoBehaviour
{
    [SerializeField]private Text NameText;
    [SerializeField]private Text KillsText;
    [SerializeField]private Text DeathsText;
    [SerializeField]private Text ScoreText;
    [SerializeField] private GameObject KickButton;
    [SerializeField] private Image LevelIcon;

    private Player cachePlayer = null;
    private bl_UIReferences UIReference;
    private bool isInitializated = false;
    private Image BackgroundImage;
    private Team InitTeam = Team.None;
    private bl_AIMananger.BotsStats Bot;

    public void Init(Player player, bl_UIReferences uir, bl_AIMananger.BotsStats bot = null)
    {
        Bot = bot;
        UIReference = uir;
        BackgroundImage = GetComponent<Image>();

        if (Bot != null)
        {
            InitBot();
            return;
        }

        cachePlayer = player;
        gameObject.name = player.NickName + player.ActorNumber;
        if(player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            Color c = BackgroundImage.color;
            c.a = 0.35f;
            BackgroundImage.color = c;
        }
        InitTeam = player.GetPlayerTeam();
        NameText.text = player.NickNameAndRole();
        KillsText.text = player.CustomProperties[PropertiesKeys.KillsKey].ToString();
        DeathsText.text = player.CustomProperties[PropertiesKeys.DeathsKey].ToString();
        ScoreText.text = player.CustomProperties[PropertiesKeys.ScoreKey].ToString();
        KickButton.SetActive(PhotonNetwork.IsMasterClient && player.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber && bl_GameData.Instance.MasterCanKickPlayers);
#if LM
         LevelIcon.gameObject.SetActive(true);
         LevelIcon.sprite = bl_LevelManager.Instance.GetPlayerLevelInfo(cachePlayer).Icon;
#else
        LevelIcon.gameObject.SetActive(false);
#endif
    }

    public void Refresh()
    {
        if(Bot != null) { InitBot(); return; }

        if(cachePlayer == null || cachePlayer.GetPlayerTeam() != InitTeam)
        {
            UIReference.RemoveUIPlayer(this);
            Destroy(gameObject);
        }

        NameText.text = cachePlayer.NickNameAndRole();
        KillsText.text = cachePlayer.CustomProperties[PropertiesKeys.KillsKey].ToString();
        DeathsText.text = cachePlayer.CustomProperties[PropertiesKeys.DeathsKey].ToString();
        ScoreText.text = cachePlayer.CustomProperties[PropertiesKeys.ScoreKey].ToString();
#if LM
         LevelIcon.sprite = bl_LevelManager.Instance.GetPlayerLevelInfo(cachePlayer).Icon;
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public void InitBot()
    {
        gameObject.name = Bot.Name;
        NameText.text = Bot.Name;
        KillsText.text = Bot.Kills.ToString();
        DeathsText.text = Bot.Deaths.ToString();
        ScoreText.text = Bot.Score.ToString();
        InitTeam = Bot.Team;
        KickButton.SetActive(false);
        LevelIcon.gameObject.SetActive(false);
    }

    public void Kick()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            bl_PhotonGame.Instance.KickPlayer(cachePlayer);
        }
    }

    public void OnClick()
    {
        if (cachePlayer == null)
            return;
        if (cachePlayer.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber && Bot == null)
        {
            bl_UIReferences.Instance.OpenScoreboardPopUp(true, cachePlayer);
        }
    }

    void OnEnable()
    {
        if (cachePlayer == null && isInitializated)
        {
            Destroy(gameObject);
            isInitializated = true;
        }
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    public int GetScore()
    {
        if(Bot == null) { return cachePlayer.GetPlayerScore(); }
        else { return Bot.Score; }
    }

    public Team GetTeam()
    {
        return InitTeam;
    }
}