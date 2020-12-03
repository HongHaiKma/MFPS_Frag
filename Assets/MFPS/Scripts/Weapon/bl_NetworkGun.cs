using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class bl_NetworkGun : MonoBehaviour
{

    [Header("Settings")]  
    public bl_Gun LocalGun;

    [Header("References")]
    public GameObject Bullet;
    public ParticleSystem MuzzleFlash;
    public GameObject DesactiveOnOffAmmo;
    public Transform LeftHandPosition;

    private AudioSource Source;
    private int WeaponID = -1;
    private BulletData m_BulletData = new BulletData();
    Vector3 bulletPosition = Vector3.zero;
    Quaternion bulletRotation = Quaternion.identity;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        Source = GetComponent<AudioSource>();
        Source.playOnAwake = false;
    }

    /// <summary>
    /// Update type each is enable 
    /// </summary>
    void OnEnable()
    {
        SetUpType();
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetUpType()
    {
        if (PlayerSync != null)
        {
            PlayerSync.SetNetworkWeapon(Info.Type);
        }
    }

    /// <summary>
    /// Fire Sync in network player
    /// </summary>
    public void Fire(Vector3 hitPoint)
    {
        if (LocalGun != null)
        {
            if (MuzzleFlash)
            {
                MuzzleFlash.Play();
                bulletPosition = MuzzleFlash.transform.position;
                bulletRotation = Quaternion.LookRotation(hitPoint - bulletPosition);
            }
            else
            {
                bulletPosition = transform.position;
                bulletRotation = Quaternion.LookRotation(hitPoint - bulletPosition);
            }
            //bullet info is set up in start function
            GameObject newBullet = bl_ObjectPooling.Instance.Instantiate(LocalGun.BulletName, bulletPosition, bulletRotation); // create a bullet
            // set the gun's info into an array to send to the bullet
            m_BulletData.Damage = 0;
            m_BulletData.ImpactForce = 0;
            m_BulletData.MaxSpread = 0;
            m_BulletData.Spread = 0;
            m_BulletData.Speed = LocalGun.bulletSpeed;
            m_BulletData.Position = transform.root.position;
            m_BulletData.isNetwork = true;

            newBullet.GetComponent<bl_Bullet>().SetUp(m_BulletData);
            Source.clip = LocalGun.FireSound;
            Source.spread = Random.Range(1.0f, 1.5f);
            Source.Play();
        }
    }

    /// <summary>
    /// if grenade 
    /// </summary>
    /// <param name="s"></param>
    public void GrenadeFire(float s,Vector3 position, Quaternion rotation, Vector3 angular)
    {
        if (LocalGun != null)
        {    
            //bullet info is set up in start function
            GameObject newBullet = Instantiate(Bullet, position, rotation) as GameObject; // create a bullet
            // set the gun's info into an array to send to the bullet
            BulletData t_info = new BulletData();    
            t_info.MaxSpread = LocalGun.maxSpread;
            t_info.Spread = s;
            t_info.Speed = LocalGun.bulletSpeed;
            t_info.Position = transform.root.position;
            t_info.isNetwork = true;

            if (newBullet.GetComponent<Rigidbody>() != null)//if grenade have a rigidbody,then apply velocity
            {
                newBullet.GetComponent<Rigidbody>().angularVelocity = angular;
            }
            newBullet.GetComponent<bl_Projectile>().SetUp(t_info);
            Source.clip = LocalGun.FireSound;
            Source.spread = Random.Range(1.0f, 1.5f);
            Source.Play();
        }
    }

    /// <summary>
    /// When is knife only reply sounds
    /// </summary>
    public void KnifeFire()
    {
        if (LocalGun != null)
        {
            Source.clip = LocalGun.FireSound;
            Source.spread = Random.Range(1.0f, 1.5f);
            Source.Play();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public void DesactiveGrenade(bool active,Material mat)
    {
        if(Info.Type != GunType.Grenade)
        {
            Debug.LogError("Gun type is not grenade, can't desactive it: " + Info.Type);
            return;
        }
        //when hide network gun / grenade we use method of change material to a invincible
        //due that if desactive the render cause animation  player broken.
        if (DesactiveOnOffAmmo != null)
        {
            DesactiveOnOffAmmo.SetActive(active);
        }
    }

    public int GetWeaponID
    {
        get
        {
            if(WeaponID == -1)
            {
                if (LocalGun != null)
                {
                    WeaponID = LocalGun.GunID;
                }
              //  Debug.Log("Gun type was not defined: " + gameObject.name);
            }
            return WeaponID;
        }
    }

    private bl_GunInfo Info
    {
        get
        {
            if (LocalGun != null)
            {
                return bl_GameData.Instance.GetWeapon(GetWeaponID);
            }
            else
            {
                Debug.LogError("This tpv weapon: " + gameObject.name + " has not been defined!");
                return bl_GameData.Instance.GetWeapon(0);
            }
        }
    }

    private bl_PlayerSync m_PS;
    private bl_PlayerSync PlayerSync
    {
        get
        {
            if(m_PS == null) { m_PS = transform.root.GetComponent<bl_PlayerSync>(); }
            return m_PS;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
      /*  if (Application.isPlaying)
            return;*/

        if (LeftHandPosition != null)
        {
            Gizmos.color = Color.green;
            //  Vector3 r = LeftHandPosition.eulerAngles;
            //  Quaternion rot = Quaternion.Euler(r /*+ new Vector3(0, -90, 0)*/);
            //  Vector3 pos = LeftHandPosition.position + new Vector3(0.03f,0.005f,0.09f);
            //  Gizmos.DrawMesh(RightHandMesh, pos, rot, Vector3.one * 33);
            Gizmos.DrawSphere(LeftHandPosition.position, 0.02f);
            Gizmos.DrawWireSphere(LeftHandPosition.position, 0.05f);
        }
    }
#endif
}