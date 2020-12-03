using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(AudioSource))]
public class bl_Bullet : bl_MonoBehaviour
{
    #region Variables
    [HideInInspector]
    public Vector3 DirectionFrom = Vector3.zero;
    [HideInInspector]
    public bool isNetwork = false;
    private int hitCount = 0;         // hit counter for counting bullet impacts for bullet penetration
    private int OwnGunID = 777;  //Information for contain Gun id
    private float damage;             // damage bullet applies to a target
    private float impactForce;        // force applied to a rigid body object
    private float maxInaccuracy;      // maximum amount of inaccuracy
    private float variableInaccuracy; // used in machineguns to decrease accuracy if maintaining fire
    private float speed;              // bullet speed
                                      // private float lifetime = 1.5f;    // time till bullet is destroyed
    [HideInInspector]
    public string GunName = ""; //Weapon name
    private Vector3 velocity = Vector3.zero; // bullet velocity
    private Vector3 newPos = Vector3.zero;   // bullet's new position
    private Vector3 oldPos = Vector3.zero;   // bullet's previous location
    private bool hasHit = false;             // has the bullet hit something?
    private Vector3 direction;               // direction bullet is travelling
    public LayerMask HittableLayers;

    [SerializeField] private TrailRenderer Trail;
    public string AIFrom { get; set; }
    public int AIViewID { get; set; }
    private Team AITeam;
    private Vector3 dir = Vector3.zero;
    RaycastHit hit;

    #endregion

    #region Bullet Set Up

    public void SetUp(BulletData info) // information sent from gun to bullet to change bullet properties
    {
        ResetBullet();
        damage = info.Damage;              // bullet damage
        impactForce = info.ImpactForce;         // force applied to rigid bodies
        maxInaccuracy = info.MaxSpread;       // max inaccuracy of the bullet
        variableInaccuracy = info.Spread;  // current inaccuracy... mostly for machine guns that lose accuracy over time
        speed = info.Speed;               // bullet speed
        DirectionFrom = info.Position;
        GunName = info.WeaponName;
        OwnGunID = info.WeaponID;
        isNetwork = info.isNetwork;
        // lifetime = info.LifeTime;
        AIViewID = bl_GameManager.m_view;
        // direction bullet is traveling
        direction = transform.TransformDirection(Random.Range(-maxInaccuracy, maxInaccuracy) * variableInaccuracy, Random.Range(-maxInaccuracy, maxInaccuracy) * variableInaccuracy, 1);

        newPos = transform.position;   // bullet's new position
        oldPos = newPos;               // bullet's old position
        velocity = speed * transform.forward; // bullet's velocity determined by direction and bullet speed
        if (Trail != null) { if (!bl_GameData.Instance.BulletTracer) { Destroy(Trail); } }
        // schedule for destruction if bullet never hits anything
        Invoke("Disable", 5);
    }

    public void AISetUp(string AIname, int viewID, Team aiTeam)
    {
        AIFrom = AIname;
        AIViewID = viewID;
        AITeam = aiTeam;
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    void ResetBullet()
    {
        hasHit = false;
        AIFrom = "";
        AIViewID = 0;
        AITeam = Team.All;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (hasHit)
            return; // if bullet has already hit its max hits... exit

        Travel();
    }

    /// <summary>
    /// 
    /// </summary>
    void Travel()
    {

        // assume we move all the way
        newPos += (velocity + direction) * Time.deltaTime;
        // Check if we hit anything on the way
        dir = newPos - oldPos;
        float dist = dir.magnitude;

        if (dist > 0)
        {
            dir /= dist;
            if (Physics.Raycast(oldPos, dir, out hit, dist, HittableLayers, QueryTriggerInteraction.Ignore))
            {
                newPos = hit.point;
                OnHit(hit);
                hasHit = true;
                Disable();
            }
        }

        oldPos = transform.position;  // set old position to current position
        transform.position = newPos;  // set current position to the new position
    }

    /// <summary>
    /// 
    /// </summary>
    private void Disable()
    {
        gameObject.SetActive(false);
    }

    #region Bullet On Hits

    void OnHit(RaycastHit hit)
    {
        GameObject go = null;
        Ray mRay = new Ray(transform.position, transform.forward);
        if (!isNetwork)
        {
            if (hit.rigidbody != null && !hit.rigidbody.isKinematic) // if we hit a rigi body... apply a force
            {
                float mAdjust = 1.0f / (Time.timeScale * (0.02f / Time.fixedDeltaTime));
                hit.rigidbody.AddForceAtPosition(((mRay.direction * impactForce) / Time.timeScale) / mAdjust, hit.point);
            }
        }
        switch (hit.transform.tag) // decide what the bullet collided with and what to do with it
        {
            case "Projectile":
                // do nothing if 2 bullets collide
                break;
            case "BodyPart"://Send Damage for other players
                if (hit.transform.GetComponent<bl_BodyPart>() != null && !isNetwork)
                {
                    hit.transform.GetComponent<bl_BodyPart>().GetDamage(damage, PhotonNetwork.NickName, DamageCause.Player, DirectionFrom, OwnGunID);
                }
                if (bl_GameData.Instance.ShowBlood)
                {
                    go = bl_ObjectPooling.Instance.Instantiate("blood", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
                    go.transform.parent = hit.transform;
                }
                Disable();
                break;
            case "AI"://Send Damage for other players
                bool bot = !string.IsNullOrEmpty(AIFrom);
                Team t = (bot) ? AITeam : PhotonNetwork.LocalPlayer.GetPlayerTeam();
                if (!string.IsNullOrEmpty(AIFrom) && AIFrom == hit.transform.root.name) { return; }
                if (hit.transform.GetComponent<bl_AIShooterHealth>() != null && !isNetwork)
                {
                    hit.transform.GetComponent<bl_AIShooterHealth>().DoDamage((int)damage, GunName, DirectionFrom, AIViewID, bot, t, false);
                }
                else if (hit.transform.GetComponent<bl_AIHitBox>() != null && !isNetwork)
                {
                    hit.transform.GetComponent<bl_AIHitBox>().DoDamage((int)damage, GunName, DirectionFrom, AIViewID, bot, t);
                }
                go = bl_ObjectPooling.Instance.Instantiate("blood", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
                go.transform.parent = hit.transform;
                Disable();
                break;
            case "Player":
                bl_PlayerDamageManager pdm = hit.transform.GetComponent<bl_PlayerDamageManager>();
                if (pdm != null && !isNetwork && !string.IsNullOrEmpty(AIFrom))
                {
                    if (!isOneTeamMode)
                    {
                        if(pdm.GetComponent<bl_PlayerSettings>().m_Team == AITeam)//if hit a team mate player
                        {
                            Disable();
                            return;
                        }
                    }
                    DamageData info = new DamageData();
                    info.Actor = null;
                    info.Damage = damage;
                    info.Direction = DirectionFrom;
                    info.From = AIFrom;
                    info.Cause = DamageCause.Bot;
                    info.GunID = OwnGunID;
                    pdm.GetDamage(info);
                    Disable();
                }
                break;
            case "Wood":
                hitCount++; // add another hit to counter
                go = bl_ObjectPooling.Instance.Instantiate("decalw", hit.point, Quaternion.LookRotation(hit.normal));
                go.transform.parent = hit.transform;
                break;
            case "Concrete":
                hitCount += 2; // add 2 hits to counter... concrete is hard
                go = bl_ObjectPooling.Instance.Instantiate("decalc", hit.point, Quaternion.LookRotation(hit.normal));
                go.transform.parent = hit.transform;
                break;
            case "Metal":
                hitCount += 3; // metal slows bullets alot
                go = bl_ObjectPooling.Instance.Instantiate("decalm", hit.point, Quaternion.LookRotation(hit.normal));
                go.transform.parent = hit.transform;
                break;
            case "Dirt":
                hasHit = true; // ground kills bullet
                go = bl_ObjectPooling.Instance.Instantiate("decals", hit.point, Quaternion.LookRotation(hit.normal));
                go.transform.parent = hit.transform;
                break;
            case "Water":
                hasHit = true; // water kills bullet
                go = bl_ObjectPooling.Instance.Instantiate("decalwt", hit.point, Quaternion.LookRotation(hit.normal));
                go.transform.parent = hit.transform;
                break;
            default:
                hitCount++; // add a hit
                go = bl_ObjectPooling.Instance.Instantiate("decal", hit.point, Quaternion.LookRotation(hit.normal));
                go.transform.parent = hit.transform;
                break;
        }
        Disable();
    }
    #endregion
}