using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.AI;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(NavMeshAgent))]
public class bl_AIShooterAgent : bl_MonoBehaviour, IPunObservable
{

    public Transform Target;
    [Space(5)]
    [Header("AI Settings")]
    public AIAgentState AgentState = AIAgentState.Idle;
    public Team AITeam = Team.None;
    public bool GetRandomTargetOnStart = true;
    public float FollowRange = 10.0f;       //when the AI starts to chase the player
    public float LookRange = 25.0f;   //when the AI starts to look at the player
    public float PatrolRadius = 20f; //Radius for get the random point
    public float LosseRange = 50f;
    public float RotationLerp = 6.0f;
    public LayerMask ObstaclesLayer;
    public bool DebugStates = false;

    [Space(5)]
    [Header("AutoTargets")]
    public float UpdatePlayerEach = 5f;
    public List<Transform> PlayersInRoom = new List<Transform>();//All Players in room

    [Header("References")]
    public Transform AimTarget;
    [SerializeField] private AudioSource FootStepSource;
    [SerializeField] private AudioClip[] FootSteps;

    //Privates
    private Vector3 correctPlayerPos = Vector3.zero; // We lerp towards this
    private Quaternion correctPlayerRot = Quaternion.identity; // We lerp towards this
    public NavMeshAgent Agent { get; set; }
    public bool death { get; set; }
    public bool personal { get; set; }

    private Animator Anim;
    public bool playerInFront { get; set; }
    private Vector3 finalPosition;
    private float lastPathTime = 0;
    private float defaultSpeed;
    private bl_AIAnimation AIAnim;
    private float stepTime;

    [HideInInspector] public Vector3 vel;
    private bl_AICovertPointManager CoverManager;
    private bl_AIMananger AIManager;
    private bl_AICoverPoint CoverPoint = null;
    private bool ForceCoverFire = false;
    public bool ObstacleBetweenTarget { get; set; }
    private float CoverTime = 0;
    private bool lookToDirection = false;
    private Vector3 LastHitDirection;
    private int SwitchCoverTimes = 0;
    private float lookTime = 0;
    private bool strafing = false;
    private bool randomOnStartTake = false;
    private float strafingTime = 0;
    private Vector2 strafingPosition = Vector3.zero;
    private bool AllOrNothing = false;
    private bl_RoundTime TimeManager;
    private bl_AIShooterWeapon AIWeapon;
    private bl_AIShooterHealth AIHealth;
    private bl_DrawName DrawName;
    private GameMode m_GameMode;
    private float time = 0;
    private float delta = 0;
    private Transform m_Transform;
    private float nextEnemysCheck = 0;
    private bl_AIMananger.BotsStats BotStat = null;

    //debug helper
    public List<int> DebugPath = new List<int>();
    public int SetDebug
    {
        get
        {
            return 0;
        }
        set
        {
            if (DebugStates)
            {
                if (DebugPath[DebugPath.Count - 1] != value)
                {
                    DebugPath.Add(value);
                    if (DebugPath.Count >= 25)
                    {
                        DebugPath.RemoveAt(0);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        m_Transform = transform;
        bl_PhotonCallbacks.PlayerEnteredRoom += OnPhotonPlayerConnected;
        bl_AIMananger.OnBotStatUpdate += OnBotStatUpdate;
        Agent = this.GetComponent<NavMeshAgent>();
        AIAnim = GetComponentInChildren<bl_AIAnimation>();
        AIHealth = GetComponent<bl_AIShooterHealth>();
        AIWeapon = GetComponent<bl_AIShooterWeapon>();
        defaultSpeed = Agent.speed;
        Anim = GetComponentInChildren<Animator>();
        ObstacleBetweenTarget = false;
        CoverManager = FindObjectOfType<bl_AICovertPointManager>();
        AIManager = CoverManager.GetComponent<bl_AIMananger>();
        TimeManager = FindObjectOfType<bl_RoundTime>();
        DrawName = GetComponent<bl_DrawName>();
        m_GameMode = GetGameMode;
    }

    public void Init()
    {
        InvokeRepeating("UpdateList", 0, UpdatePlayerEach);
        DrawName.SetName(AIName);
        if (!isOneTeamMode && bl_GameManager.Instance.OurPlayer != null && !death)
        {
            DrawName.enabled = bl_GameManager.Instance.LocalPlayerTeam == AITeam;
        }
        else
        {
            DrawName.enabled = false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnMasterStatsReceived(List<bl_AIMananger.BotsStats> stats)
    {
        ApplyMasterInfo(stats);
    }

    /// <summary>
    /// 
    /// </summary>
    void ApplyMasterInfo(List<bl_AIMananger.BotsStats> stats)
    {
        int viewID = photonView.ViewID;
        bl_AIMananger.BotsStats bs = stats.Find(x => x.ViewID == viewID);
        if (bs != null)
        {
            AIName = bs.Name;
            AITeam = bs.Team;
            gameObject.name = AIName;
            BotStat = new bl_AIMananger.BotsStats();
            BotStat.Name = AIName;
            BotStat.Score = bs.Score;
            BotStat.Kills = bs.Kills;
            BotStat.Deaths = bs.Deaths;
            BotStat.ViewID = bs.ViewID;
            bl_EventHandler.OnRemoteActorChange(transform, true, false);
        }
    }

    void OnBotStatUpdate(bl_AIMananger.BotsStats stat)
    {
        if (stat.ViewID != photonView.ViewID) return;

        BotStat = stat;
        AIName = stat.Name;
        AITeam = BotStat.Team;
        gameObject.name = AIName;
        bl_EventHandler.OnRemoteActorChange(transform, true, false);
    }
    /// <summary>
    /// 
    /// </summary>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(m_Transform.localPosition);
            stream.SendNext(m_Transform.rotation);
            stream.SendNext(Agent.velocity);
        }
        else
        {
            correctPlayerPos = (Vector3)stream.ReceiveNext();
            correctPlayerRot = (Quaternion)stream.ReceiveNext();
            vel = (Vector3)stream.ReceiveNext();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void CheckTargets()
    {
        if (Target != null && Target.root.name.Contains("(die)"))
        {
            Target = null;
            photonView.RPC("ResetTarget", RpcTarget.Others);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnGetHit(Vector3 pos)
    {
        LastHitDirection = pos;
        //if the AI is not covering, will look for a cover point
        if (AgentState != AIAgentState.Covering)
        {
            //if the AI is following and attacking the target he will not look for cover point
            if (AgentState == AIAgentState.Following && TargetDistance <= LookRange)
            {
                lookToDirection = true;
                return;
            }
            Cover(false);
        }
        else
        {
            //if already in a cover and still get shoots from far away will force the AI to fire.
            if (!playerInFront)
            {
                lookToDirection = true;
                Cover(true);
            }
            else
            {
                ForceCoverFire = true;
                lookToDirection = false;
            }
            //if the AI is cover but still get hit, he will search other cover point 
            if (AIHealth.Health <= 50 && Agent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                Cover(true);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        time = Time.time;
        delta = Time.deltaTime;
        if (death) return;

        if (!PhotonNetwork.IsMasterClient)//if not master client, then get position from server
        {
            m_Transform.localPosition = Vector3.Lerp(m_Transform.localPosition, correctPlayerPos, delta * 7);
            m_Transform.rotation = Quaternion.Lerp(m_Transform.rotation, correctPlayerRot, delta * 7);
        }
        else
        {
            vel = Agent.velocity;
            if (TimeManager.isFinish)
            {
                Agent.isStopped = true;
                return;
            }
        }
        if (Target != null)
        {
            if (AgentState != AIAgentState.Covering)
            {
                TargetControll();
            }
            else
            {
                OnCovering();
            }
        }
    }

    /// <summary>
    /// this is called one time each second instead of each frame
    /// </summary>
    public override void OnSlowUpdate()
    {
        if (death) return;
        if (TimeManager.isFinish)
        {
            return;
        }

        if (Target == null)
        {
            //Get the player nearest player
            SearchPlayers();
            //if target null yet, the patrol         
            RandomPatrol(m_GameMode == GameMode.TDM);
        }
        else
        {
            CheckEnemysDistances();
            CalculateAngle();
        }
        FootStep();
    }

    /// <summary>
    /// 
    /// </summary>
    bool Cover(bool overridePoint, bool toTarget = false)
    {
        Transform t = (!toTarget) ? transform : Target;
        if (overridePoint)
        {
            if (Agent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                //look for another cover point 
                CoverPoint = CoverManager.GetCloseCover(t, CoverPoint);
            }
        }
        else
        {
            //look for a near cover point
            CoverPoint = CoverManager.GetCloseCover(t);
        }
        if (CoverPoint != null)
        {
            Agent.stoppingDistance = 0.1f;
            Speed = playerInFront ? defaultSpeed : 6;
            Agent.SetDestination(CoverPoint.transform.position);
            AgentState = AIAgentState.Covering;
            CoverTime = time;
            AIWeapon.Fire(bl_AIShooterWeapon.FireReason.OnMove);
            return true;
        }
        else
        {
            //if there are not a near cover point
            if (Target != null)
            {
                //follow target
                Agent.SetDestination(Target.position);
                Speed = playerInFront ? defaultSpeed : 7;
                personal = true;
                AgentState = AIAgentState.Searching;
            }
            else
            {
                CoverPoint = CoverManager.GetCloseCoverForced(transform);
                Agent.SetDestination(CoverPoint.transform.position);
                Speed = defaultSpeed;
            }
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnCovering()
    {
        if (Target != null)
        {
            float Distance = TargetDistance;
            if (Distance <= LookRange && playerInFront)//if in look range and in front, start follow him and shot
            {
                if (!strafing)
                {
                    AgentState = AIAgentState.Following;
                    Agent.SetDestination(Target.position);
                    SetDebug = 1;
                }
                else
                {
                    AgentState = AIAgentState.Covering;
                    Agent.SetDestination(strafingPosition);
                    SetDebug = 19;
                }
                AIWeapon.Fire();

            }
            else if (Distance > LosseRange && (time - CoverTime) >= 7)// if out of line of sight, start searching him
            {
                AgentState = AIAgentState.Searching;
                SetCrouch(false);
                AIWeapon.Fire(bl_AIShooterWeapon.FireReason.OnMove);
                SetDebug = 2;
            }
            else if (ForceCoverFire && !ObstacleBetweenTarget)//if in cover and still get damage, start shoot at him
            {
                AIWeapon.Fire(bl_AIShooterWeapon.FireReason.Forced);
                if (CanCover(10)) { SwichCover(); }
                SetDebug = 3;
            }
            else if (CanCover(10) && Distance >= 7)
            {
                SwichCover();
                AIWeapon.Fire(bl_AIShooterWeapon.FireReason.OnMove);
                SetDebug = 4;
            }
            else
            {
                if (playerInFront)
                {
                    AIWeapon.Fire();
                    SetDebug = 5;
                }
                else
                {
                    AIWeapon.Fire(bl_AIShooterWeapon.FireReason.Forced);
                    Look();
                    SetCrouch(false);
                    SetDebug = 6;
                }
            }
        }
        if (Agent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            if (CoverPoint != null && CoverPoint.Crouch) { SetCrouch(true); }
        }
        if (lookToDirection)
        {
            LookToHitDirection();
        }
        else
        {
            Quaternion rotation = Quaternion.LookRotation(Target.position - m_Transform.localPosition);
            m_Transform.rotation = Quaternion.Slerp(m_Transform.rotation, rotation, delta * RotationLerp);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void SwichCover()
    {
        if (Agent.pathStatus != NavMeshPathStatus.PathComplete)
            return;

        if (SwitchCoverTimes <= 3)
        {
            Cover(true, true);
            SwitchCoverTimes++;
        }
        else
        {
            AgentState = AIAgentState.Following;
            Agent.SetDestination(TargetPosition);
            SwitchCoverTimes = 0;
            AllOrNothing = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void TargetControll()
    {
        float Distance = bl_UtilityHelper.Distance(Target.position, m_Transform.localPosition);
        if (Distance >= LosseRange)
        {
            if (AgentState == AIAgentState.Following || personal || AgentState == AIAgentState.Searching)
            {
                RandomPatrol(true);
                SetDebug = 7;
            }
            else
            {
                photonView.RPC("ResetTarget", RpcTarget.All);
                RandomPatrol(false);
                AgentState = AIAgentState.Patroling;
                SetDebug = 8;
            }
            Speed = defaultSpeed;
            if (!AIWeapon.isFiring)
            {
                Anim.SetInteger("UpperState", 4);
            }
        }
        else if (Distance > FollowRange && Distance < LookRange)//look range
        {
            OnTargetInSight(false);
        }
        else if (Distance <= FollowRange)
        {
            Follow();
        }
        else if (Distance < LosseRange)
        {
            OnTargetInSight(true);
        }
        else
        {
            Debug.Log("Unknown state: " + Distance);
            SetDebug = 10;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnTargetInSight(bool overrideCover)
    {
        if (AgentState == AIAgentState.Following || AIHealth.Health <= 50)
        {
            if (!Cover(overrideCover) || CanCover(17) || AllOrNothing)
            {
                Follow();
                AgentState = AIAgentState.Following;
                SetDebug = 11;
            }
            else
            {
                if (Target != null)
                {
                    AIWeapon.Fire(bl_AIShooterWeapon.FireReason.OnMove);
                }
                if (!strafing)
                {
                    strafingTime += delta;
                    if (strafingTime >= 5)
                    {
                        strafing = true;
                        Invoke("ResetStrafing", 4);
                    }
                    SetCrouch(true);
                    SetDebug = 12;
                }
                else
                {
                    if (strafingTime > 0)
                    {
                        strafingPosition = m_Transform.localPosition + m_Transform.TransformDirection(m_Transform.localPosition + (Vector3.right * Random.Range(-3, 3)));
                        strafingTime = 0;
                    }
                    Agent.destination = strafingPosition;
                    SetCrouch(false);
                    SetDebug = 18;
                }
            }
        }
        else if (AgentState == AIAgentState.Covering)
        {
            if (CanCover(5) && TargetDistance >= 7)
            {
                Cover(true);
                SetDebug = 13;
            }
            else
            {
                SetDebug = 14;
            }
        }
        else
        {
            Look();
            SetDebug = 15;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void SearchPlayers()
    {
        GameMode gm = GetGameMode;
        for (int i = 0; i < PlayersInRoom.Count; i++)
        {
            Transform enemy = PlayersInRoom[i];
            if (enemy != null)
            {
                float Distance = bl_UtilityHelper.Distance(enemy.position, m_Transform.localPosition);//if a player in range, get this
                if (gm == GameMode.FFA)
                {
                    if (Distance < LookRange && !enemy.root.name.Contains("(die)"))//if in range
                    {
                        GetTarget(PlayersInRoom[i]);//get this player
                    }
                }
                else
                {
                    bl_AIShooterAgent aisa = enemy.root.GetComponent<bl_AIShooterAgent>();
                    if (aisa != null)
                    {
                        if (Distance < LookRange && !enemy.root.name.Contains("(die)") && enemy.root.GetComponent<bl_AIShooterAgent>().AITeam != AITeam)//if in range
                        {
                            GetTarget(PlayersInRoom[i]);//get this player
                        }
                    }
                }
            }
        }

        if (PhotonNetwork.IsMasterClient && !randomOnStartTake && PlayersInRoom.Count > 0)
        {
            if (GetRandomTargetOnStart)
            {
                Target = PlayersInRoom[Random.Range(0, PlayersInRoom.Count)];
                randomOnStartTake = true;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void CalculateAngle()
    {
        if (Target == null || !PhotonNetwork.IsMasterClient)
        {
            ObstacleBetweenTarget = false;
            return;
        }

        Vector3 relative = m_Transform.InverseTransformPoint(Target.position);
        if ((relative.x < 2f && relative.x > -2f) || (relative.x > -2f && relative.x < 2f))
        {
            //target is in front
            playerInFront = true;
        }
        else
        {
            playerInFront = false;
        }
        if (Physics.Linecast(AIWeapon.FirePoint.position, TargetPosition, out obsRay, ObstaclesLayer, QueryTriggerInteraction.Ignore))
        {
            ObstacleBetweenTarget = obsRay.transform.root.CompareTag(bl_PlayerSettings.LocalTag) == false;
        }
        else { ObstacleBetweenTarget = false; }
        Debug.DrawLine(AIWeapon.FirePoint.position, TargetPosition, Color.red);
    }
    RaycastHit obsRay;

    /// <summary>
    /// If player not in range then the AI patrol in map
    /// </summary>
    void RandomPatrol(bool precision)
    {
        if (death)
            return;

        float pre = PatrolRadius;
        if (precision)
        {
            if (TargetDistance < LookRange)
            {
                if (Target == null)
                {
                    Target = GetNearestPlayer;
                }
                AgentState = AIAgentState.Looking;
                SetDebug = 23;
            }
            else
            {
                AgentState = AIAgentState.Searching;
                SetDebug = 16;
            }
            pre = 8;
        }
        else
        {
            AgentState = AIAgentState.Patroling;
            ForceCoverFire = false;
            SetDebug = 22;
        }
        lookToDirection = false;
        AIWeapon.isFiring = false;
        if (!Agent.hasPath || TargetDistance <= 5.2f || (time - lastPathTime) > 7)
        {
            bool toAnCover = (Random.Range(0, 20) >= 18);//probability of get a cover point as random destination
            Vector3 randomDirection = TargetPosition + (Random.insideUnitSphere * pre);
            if (toAnCover) { randomDirection = CoverManager.GetCoverOnRadius(transform, 20).transform.position; }
            if (Target == null && m_GameMode == GameMode.FFA)
            {
                randomDirection += m_Transform.localPosition;
            }
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, pre, 1);
            finalPosition = hit.position;
            lastPathTime = time + Random.Range(0, 5);
            Speed = (Random.Range(0, 5) == 3) ? 4 : 6;
            SetCrouch(false);
        }
        else
        {
            SetDebug = 17;
        }
        Agent.SetDestination(finalPosition);
    }

    /// <summary>
    /// 
    /// </summary>
    void SetCrouch(bool crouch)
    {
        Anim.SetBool("Crouch", crouch);
        Speed = crouch ? 3.5f : defaultSpeed;
    }

    /// <summary>
    /// 
    /// </summary>
    public void KillTheTarget(Transform t)
    {
        Target = null;
        photonView.RPC("ResetTarget", RpcTarget.All);
    }

    /// <summary>
    /// Force AI to look the target
    /// </summary>
    void Look()
    {
        if (AgentState != AIAgentState.Covering)
        {
            if (lookTime >= 5)
            {
                AgentState = AIAgentState.Following;
                lookTime = 0;
                return;
            }
            lookTime += delta;
            AgentState = AIAgentState.Looking;
        }
        Quaternion rotation = Quaternion.LookRotation(TargetPosition - m_Transform.localPosition);
        m_Transform.rotation = Quaternion.Slerp(m_Transform.rotation, rotation, delta * RotationLerp);
        AIWeapon.Fire();
        SetCrouch(playerInFront);
        Speed = playerInFront ? defaultSpeed : 5.5f;
        lookToDirection = false;
    }

    /// <summary>
    /// 
    /// </summary>
    void LookToHitDirection()
    {
        if (LastHitDirection == Vector3.zero)
            return;
        Vector3 rhs = Target.position - LastHitDirection;
        if (rhs == Vector3.zero) { rhs = m_Transform.forward * 10; }

        Quaternion rotation = Quaternion.LookRotation(rhs);
        m_Transform.rotation = Quaternion.Slerp(m_Transform.rotation, rotation, delta * RotationLerp);
        SetCrouch(playerInFront);
        Speed = playerInFront ? defaultSpeed : 8;
    }

    /// <summary>
    /// 
    /// </summary>
    void Follow()
    {
        Agent.stoppingDistance = 3;
        lookToDirection = false;
        SetCrouch(false);
        Speed = defaultSpeed;
        Agent.destination = TargetPosition;
        if (Agent.remainingDistance <= 3)
        {
            if (Cover(false, true))
            {
                AgentState = AIAgentState.Covering;
            }
            else { Look(); }
            AIWeapon.Fire(bl_AIShooterWeapon.FireReason.Forced);
            SetDebug = 20;
        }
        else
        {
            AgentState = AIAgentState.Following;
            AIWeapon.Fire();
        }
    }

    /// <summary>
    /// This is called when the bot have a Target
    /// this check if other enemy is nearest and change of target if it's require
    /// </summary>
    void CheckEnemysDistances()
    {
        if (PlayersInRoom.Count <= 0) return;
        if (time < nextEnemysCheck) return;

        float targetDistance = bl_UtilityHelper.Distance(m_Transform.localPosition, TargetPosition);
        for (int i = 0; i < PlayersInRoom.Count; i++)
        {
            //if the enemy transform is not null or the same target that have currently have.
            if (PlayersInRoom[i] == null || PlayersInRoom[i] == Target) continue;
            //calculate the distance from this other enemy
            float otherDistance = bl_UtilityHelper.Distance(m_Transform.localPosition, PlayersInRoom[i].position);
            if (otherDistance > LosseRange) continue;//if this enemy is too far away...
            //and check if it's nearest than the current target (5 meters close at least)
            if (otherDistance < targetDistance && (targetDistance - otherDistance) > 5)
            {
                //calculate the angle between this bot and the other enemy to check if it's in a "View Angle"
                Vector3 targetDir = PlayersInRoom[i].position - m_Transform.localPosition;
                float Angle = Vector3.Angle(targetDir, m_Transform.forward);
                if (Angle > -55 && Angle < 55)
                {
                    //so then get it as new dangerous target
                    Target = PlayersInRoom[i];
                    //prevent to change target in at least the next 3 seconds
                    nextEnemysCheck = time + 3;
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void GetTarget(Transform t)
    {
        if (t == null)
            return;

        Target = t;
        PhotonView view = GetPhotonView(Target.root.gameObject);
        if (view != null)
        {
            photonView.RPC("SyncTargetAI", RpcTarget.Others, view.ViewID);
        }
        else
        {
            Debug.Log("This Target " + Target.name + "no have photonview");
        }
    }


    [PunRPC]
    void SyncTargetAI(int view)
    {
        Transform t = FindPlayerRoot(view).transform;
        if (t != null)
        {
            Target = t;
        }
    }

    [PunRPC]
    void ResetTarget()
    {
        Target = null;
    }

    /// <summary>
    /// 
    /// </summary>
    void UpdateList()
    {
        PlayersInRoom = AllPlayers;
        AimTarget.name = AIName;
    }

    /// <summary>
    /// 
    /// </summary>
    public void FootStep()
    {
        float vel = Agent.velocity.magnitude;
        if (vel < 1)
            return;

        float lenght = 0.6f;
        if (vel > 5)
        {
            lenght = 0.45f;
        }

        if ((time - stepTime) > lenght)
        {
            stepTime = time;
            FootStepSource.clip = FootSteps[Random.Range(0, FootSteps.Length)];
            FootStepSource.Play();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected List<Transform> AllPlayers
    {
        get
        {
            List<Transform> list = new List<Transform>();
            Player[] players = PhotonNetwork.PlayerList;
            for (int i = 0; i < players.Length; i++)
            {
                Player p = players[i];
                if (!isOneTeamMode)
                {
                    Team pt = p.GetPlayerTeam();
                    if (pt != AITeam && pt != Team.None)
                    {
                        bl_GameManager.SceneActors g = bl_GameManager.Instance.FindActor(p.NickName);
                        if (g != null)
                        {
                            list.Add(g.Actor);
                        }
                    }
                }
                else
                {
                    bl_GameManager.SceneActors g = bl_GameManager.Instance.FindActor(p.NickName);
                    if (g != null)
                    {
                        list.Add(g.Actor);
                    }
                }
            }
            list.AddRange(AIManager.GetOtherBots(AimTarget, AITeam));
            return list;
        }
    }

    private float Speed
    {
        get
        {
            return Agent.speed;
        }
        set
        {
            bool cr = Anim.GetBool("Crouch");
            if (cr)
            {
                Agent.speed = 2;
            }
            else
            {
                Agent.speed = value;
            }
        }
    }

    void ResetStrafing() { strafingTime = 0; strafing = false; }

    [PunRPC]
    public void DestroyRpc(Vector3 position, PhotonMessageInfo info)
    {
        StartCoroutine(DestroyNetwork(position, info));
    }

    IEnumerator DestroyNetwork(Vector3 position, PhotonMessageInfo info)
    {
        if ((PhotonNetwork.Time - info.SentServerTime) > 2.2f)
        {
            Destroy(gameObject);
            yield break;
        }
        AIAnim.Ragdolled(position);
        yield return new WaitForSeconds(5);
        if (!PhotonNetwork.IsMasterClient)
        {
            Destroy(this.gameObject);
            yield return 0; // if you allow 1 frame to pass, the object's OnDestroy() method gets called and cleans up references.
        }
    }

    void OnLocalSpawn()
    {
        if (!isOneTeamMode && bl_GameManager.Instance.LocalPlayerTeam == AITeam)
        {
            DrawName.enabled = true;
        }
    }

    public void OnPhotonPlayerConnected(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient && newPlayer.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            photonView.RPC("RpcSync", newPlayer, AIHealth.Health);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!DebugStates) return;
        if (m_Transform == null) { m_Transform = transform; }
        Gizmos.color = Color.yellow;
        bl_UtilityHelper.DrawWireArc(m_Transform.position, LosseRange, 360, 12, Quaternion.identity);
        Gizmos.color = Color.white;
        bl_UtilityHelper.DrawWireArc(m_Transform.position, PatrolRadius, 360, 12, Quaternion.identity);
        Gizmos.color = Color.yellow;
        bl_UtilityHelper.DrawWireArc(m_Transform.position, LookRange, 360, 12, Quaternion.identity);
        Gizmos.color = Color.white;
        bl_UtilityHelper.DrawWireArc(m_Transform.position, FollowRange, 360, 12, Quaternion.identity);
    }
#endif
    /// <summary>
    /// 
    /// </summary>
    protected override void OnDestroy()
    {
        base.OnDestroy();
        bl_EventHandler.OnRemoteActorChange(transform, false, false);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        bl_EventHandler.OnLocalPlayerSpawn += OnLocalSpawn;
        bl_AIMananger.OnMaterStatsReceived += OnMasterStatsReceived;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        bl_EventHandler.OnLocalPlayerSpawn -= OnLocalSpawn;
        bl_PhotonCallbacks.PlayerEnteredRoom -= OnPhotonPlayerConnected;
        bl_AIMananger.OnMaterStatsReceived -= OnMasterStatsReceived;
        bl_AIMananger.OnBotStatUpdate -= OnBotStatUpdate;
    }

    public Vector3 TargetPosition
    {
        get
        {
            if (Target != null) { return Target.position; }
            if (!isOneTeamMode && PlayersInRoom.Count > 0)
            {
                Transform t = GetNearestPlayer;
                if (t != null)
                {
                    return t.position;
                }
                else { return m_Transform.position + (m_Transform.forward * 3); }
            }
            return Vector3.zero;
        }
    }

    public Transform GetNearestPlayer
    {
        get
        {
            if (PlayersInRoom.Count > 0)
            {
                Transform t = null;
                float d = 1000;
                for (int i = 0; i < PlayersInRoom.Count; i++)
                {
                    if (PlayersInRoom[i] == null) continue;
                    float dis = bl_UtilityHelper.Distance(m_Transform.position, PlayersInRoom[i].position);
                    if (dis < d)
                    {
                        d = dis;
                        t = PlayersInRoom[i];
                    }
                }
                return t;
            }
            else { return null; }
        }
    }
    private string _ainame;
    public string AIName
    {
        get
        {
            return _ainame;
        }
        set
        {
            _ainame = value;
            gameObject.name = value;
        }
    }

    public float TargetDistance { get { return bl_UtilityHelper.Distance(m_Transform.position, TargetPosition); } }
    private bool CanCover(float inTimePassed) { return ((time - CoverTime) >= inTimePassed); }

}