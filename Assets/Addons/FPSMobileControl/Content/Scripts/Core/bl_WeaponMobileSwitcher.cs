using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class bl_WeaponMobileSwitcher : MonoBehaviour
{
    [SerializeField] private Image PreviewImage;

    private bl_GunManager GunManager;
    private bl_GameManager Manager;
    private bl_GameData Data;

    private void Awake()
    {
        Manager = FindObjectOfType<bl_GameManager>();
        Data = bl_GameData.Instance;
    }
    
    private void OnEnable()
    {
        bl_EventHandler.OnLocalPlayerSpawn += OnLocalSpawn;
    }

    private void OnDisable()
    {
        bl_EventHandler.OnLocalPlayerSpawn -= OnLocalSpawn;
    }

    void OnLocalSpawn()
    {
        GunManager = Manager.OurPlayer.GetComponentInChildren<bl_GunManager>();
        Invoke("TakeFirst", 1);
    }

    public void ChangeWeapon(bool forwa)
    {
        if (GunManager == null)
            return;

        int c = 0;
        if (forwa)
        {
           c = GunManager.SwitchNext();
        }
        else
        {
           c = GunManager.SwitchPrevious();
        }
        PreviewImage.sprite = Data.GetWeapon(GunManager.PlayerEquip[c].GunID).GunIcon;
    }

    void TakeFirst()
    {
        PreviewImage.sprite = Data.GetWeapon(GunManager.CurrentGun.GunID).GunIcon;
    }
}
