using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class bl_AutoWeaponFire : bl_PhotonHelper
{
    public float WaitToFireTime = 1;
    public float ViewRange = 50;
    [Range(1, 7)] public float SniperRangeMultiplier = 3;

    [Header("References")]
    [SerializeField] private GameObject BarUI;
    [SerializeField] private Image BarImage;

    private Camera PlayerCamera;
    private RaycastHit Ray;
    private bool HitSome = false;
    private float waitTime = 0;
    private bl_GameManager GameManager;
    private bl_GunInfo ActualGun;
    private float LastOverTime = 0;
    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        GameManager = FindObjectOfType<bl_GameManager>();
#if MFPSM
        if (!bl_GameData.Instance.AutoWeaponFire)
        {
            this.enabled = false;
        }
#endif
        BarUI.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        bl_EventHandler.OnLocalPlayerSpawn += OnLocalSpawn;
        bl_EventHandler.OnLocalPlayerDeath += OnLocalDeath;
        bl_EventHandler.OnChangeWeapon += OnChangeWeapon;
        if(PlayerCamera == null && GameManager.OurPlayer != null)
        {
            PlayerCamera = GameManager.OurPlayer.GetComponent<bl_PlayerSettings>().PlayerCamera;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        bl_EventHandler.OnLocalPlayerSpawn -= OnLocalSpawn;
        bl_EventHandler.OnLocalPlayerDeath -= OnLocalDeath;
        bl_EventHandler.OnChangeWeapon -= OnChangeWeapon;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalSpawn()
    {
        PlayerCamera = GameManager.OurPlayer.GetComponent<bl_PlayerSettings>().PlayerCamera;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalDeath()
    {
        ResetParamt();
        BarUI.SetActive(false);
    }

    void OnChangeWeapon(int GunID)
    {
        ActualGun = bl_GameData.Instance.GetWeapon(GunID);
    }

    public bool Fire()
    {
        if (PlayerCamera == null)
            return false;

        float range = ViewRange;
        if(ActualGun != null)
        {
            if(ActualGun.Type == GunType.Knife) { range = 3; }
            else if( ActualGun.Type == GunType.Grenade) { return false; }
            else if( ActualGun.Type == GunType.Sniper) { range = ViewRange * SniperRangeMultiplier; }
        }

        Ray r = new Ray(PlayerCamera.transform.position, PlayerCamera.transform.forward);
        Debug.DrawRay(PlayerCamera.transform.position, PlayerCamera.transform.forward * range, Color.yellow);
        if (Physics.Raycast(r, out Ray, range))
        {
            if (Ray.transform.tag == "BodyPart" || Ray.transform.tag == "AI")
            {
                if (GetGameMode != GameMode.FFA)
                {
                    bl_PlayerSettings ps = Ray.transform.root.GetComponent<bl_PlayerSettings>();
                    if (ps != null) { if (ps.m_Team == PhotonNetwork.LocalPlayer.GetPlayerTeam()) { return false; } }
                }

                return OnOverEnemy();
            }
            else
            {
                ResetParamt();
            }
        }
        else
        {
            ResetParamt();
        }
        return false;
    }

    bool OnOverEnemy()
    {
        HitSome = true;
        if (waitTime >= WaitToFireTime)
        {
            BarUI.SetActive(false);
            LastOverTime = Time.time;
            return true;
        }
        else
        {
            waitTime += Time.deltaTime;
            waitTime = Mathf.Clamp(waitTime, 0, WaitToFireTime);
            BarUI.SetActive(true);
            BarImage.fillAmount = waitTime / WaitToFireTime;
            return false;
        }
    }

    void ResetParamt()
    {
        if (HitSome && (Time.time - LastOverTime) > 0.22f)
        {
            HitSome = false;
            BarUI.SetActive(false);
            BarImage.fillAmount = 0;
            waitTime = 0;
        }
    }
}