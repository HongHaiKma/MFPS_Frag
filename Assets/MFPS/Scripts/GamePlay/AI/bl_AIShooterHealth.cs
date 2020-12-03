using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class bl_AIShooterHealth : bl_PhotonHelper
{

    [Range(10, 500)] public int Health = 100;

    [Header("References")]
    public Texture2D DeathIcon;

    private bl_AIShooterAgent Agent;
    private bl_AIMananger AIManager;
    private int LastActorEnemy = -1;
    private bl_AIAnimation AIAnim;
    private int m_RepetingDamage = 1;
    private DamageData RepetingDamageInfo;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        Agent = GetComponent<bl_AIShooterAgent>();
        AIManager = FindObjectOfType<bl_AIMananger>();
        AIAnim = GetComponentInChildren<bl_AIAnimation>();
    }
    /// <summary>
    /// 
    /// </summary>
    public void DoDamage(int damage, string wn, Vector3 direction, int vi, bool fromBot, Team team, bool ishead)
    {
        if (Agent.death)
            return;

        if (GetGameMode == GameMode.TDM)
        {
            if (team == Agent.AITeam) return;
        }

        photonView.RPC("RpcDoDamage", RpcTarget.All, damage, wn, direction, vi, fromBot, ishead);
    }

    [PunRPC]
    void RpcDoDamage(int damage, string wn, Vector3 direction, int viewID, bool fromBot, bool ishead)
    {
        if (Agent.death)
            return;

        Health -= damage;
        if (LastActorEnemy != viewID)
        {
            Agent.personal = false;
        }
        LastActorEnemy = viewID;

        if (PhotonNetwork.IsMasterClient)
        {
            Agent.OnGetHit(direction);
        }
        if (viewID == bl_GameManager.m_view)//if was me that make damage
        {
            bl_UCrosshair.Instance.OnHit();
        }

        if (Health > 0)
        {
            Transform t = bl_GameManager.Instance.FindActor(viewID);
            if (t != null)
            {
                if (Agent.Target == null)
                {
                    Agent.personal = true;
                    Agent.Target = t;
                }
                else
                {
                    if (t != Agent.Target)
                    {
                        float cd = bl_UtilityHelper.Distance(transform.position, Agent.Target.position);
                        float od = bl_UtilityHelper.Distance(transform.position, t.position);
                        if (od < cd && (cd - od) > 7)
                        {
                            Agent.personal = true;
                            Agent.Target = t;
                        }
                    }
                }
            }
            AIAnim.OnGetHit();
        }
        else
        {
            Agent.death = true;
            Agent.enabled = false;
            Agent.Agent.isStopped = true;
            GetComponent<bl_DrawName>().enabled = false;

            bl_AIShooterAgent killerBot = null;
            if (viewID == bl_GameManager.m_view && !fromBot)//if was me that kill AI
            {
                bl_EventHandler.KillEvent(base.LocalName, Agent.AIName, wn, PhotonNetwork.LocalPlayer.GetPlayerTeam().ToString(), 5, 20);
                //Add a new kill and update information
                PhotonNetwork.LocalPlayer.PostKill(1);//Send a new kill

                int score;
                //If heat shot will give you double experience
                if (ishead)
                {
                    bl_GameManager.Instance.Headshots++;
                    score = bl_GameData.Instance.ScoreReward.ScorePerKill + bl_GameData.Instance.ScoreReward.ScorePerHeadShot;
#if KILL_STREAK
                    bl_KillNotifierManager kn = bl_KillNotifierUtils.GetManager;
                    if(kn != null) { kn.NewKill(true); } else { Debug.LogWarning("Kill streak notifier is enabled but not integrate in this scene."); }              
#endif
                }
                else
                {
                    score = bl_GameData.Instance.ScoreReward.ScorePerKill;
#if KILL_STREAK
                    bl_KillNotifierManager kn = bl_KillNotifierUtils.GetManager;
                    if (kn != null) { kn.NewKill(); } else { Debug.LogWarning("Kill streak notifier is enabled but not integrate in this scene."); }
#endif
                }

                bl_KillFeed.LocalKillInfo localKillInfo = new bl_KillFeed.LocalKillInfo();
                localKillInfo.Killed = Agent.AIName;
                localKillInfo.HeadShot = ishead;
                localKillInfo.Weapon = wn;
                bl_EventHandler.OnKillEvent(localKillInfo);

                //Send to update score to player
                PhotonNetwork.LocalPlayer.PostScore(score);
            }
            else if (fromBot)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    PhotonView p = PhotonView.Find(viewID);
                    string killer = "Unknown";
                    if (p != null)
                    {
                        killer = p.gameObject.name;
                        //update bot stats
                        AIManager.SetBotKill(killer);
                    }                  
                    bl_EventHandler.KillEvent(killer, Agent.AIName, wn, Agent.AITeam.ToString(), 5, 20);

                    bl_AIShooterAgent bot = p.GetComponent<bl_AIShooterAgent>();
                    if (bot != null)
                    {
                        bot.KillTheTarget(transform);
                        killerBot = bot;
                    }
                    else
                    {
                        Debug.Log("Bot can't be found");
                    }
                }
            }
            AIManager.SetBotDeath(Agent.AIName);
            gameObject.name += " (die)";
            if (PhotonNetwork.IsMasterClient)
            {
                if (GetGameMode == GameMode.TDM)
                {
                    string t = (Agent.AITeam == Team.Recon) ? PropertiesKeys.Team1Score : PropertiesKeys.Team2Score;
                    int score = (int)PhotonNetwork.CurrentRoom.CustomProperties[t];
                    score++;
                    ExitGames.Client.Photon.Hashtable table = new ExitGames.Client.Photon.Hashtable();
                    table.Add(t, score);
                    PhotonNetwork.CurrentRoom.SetCustomProperties(table);
                    if(Agent.AITeam == PhotonNetwork.LocalPlayer.GetPlayerTeam())
                    {
                        GameObject di = bl_ObjectPooling.Instance.Instantiate("deathicon", transform.position, transform.rotation);
                        di.GetComponent<bl_ClampIcon>().SetTempIcon(DeathIcon, 5, 20);
                    }
                }
                AIManager.OnBotDeath(Agent, killerBot);
            }
            this.photonView.RPC("DestroyRpc", RpcTarget.AllBuffered, direction);
        }
    }

    public void DoRepetingDamage(int damage, int each, DamageData info = null)
    {
        m_RepetingDamage = damage;
        RepetingDamageInfo = info;
        InvokeRepeating("MakeDamageRepeting", 0, each);
    }

    /// <summary>
    /// 
    /// </summary>
    void MakeDamageRepeting()
    {
        DamageData info = new DamageData();
        info.Damage = m_RepetingDamage;
        if (RepetingDamageInfo != null)
        {
            info = RepetingDamageInfo;
            info.Damage = m_RepetingDamage;
        }
        else
        {
            info.Direction = Vector3.zero;
            info.Cause = DamageCause.Map;
        }
        DoDamage((int)info.Damage, "[Burn]", info.Direction, bl_GameManager.m_view, false, PhotonNetwork.LocalPlayer.GetPlayerTeam(), false);
    }

    public void CancelRepetingDamage()
    {
        CancelInvoke("MakeDamageRepeting");
    }

    [PunRPC]
    void RpcSync(int _health)
    {
        Health = _health;
    }
}