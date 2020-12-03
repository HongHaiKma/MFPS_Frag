using UnityEngine;
using Photon.Realtime;

public class DamageData
{
    public float Damage = 10;
    public string From = "";
    public DamageCause Cause;
    public Vector3 Direction = Vector3.zero;
    public bool isHeadShot = false;
    public int GunID = 0;
    public Player Actor;
}