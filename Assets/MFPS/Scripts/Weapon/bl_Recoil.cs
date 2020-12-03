using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_Recoil : bl_MonoBehaviour
{
    [Range(1,10)] public float MaxRecoil = 5;

    private Transform m_Transform;
    private Vector3 RecoilRot;
    [SerializeField] private float Recoil = 0;
    private bl_GunManager GunManager;
    private float RecoilSpeed = 2;

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        GameObject g = new GameObject("Recoil");
        m_Transform = g.transform;
        m_Transform.parent = transform.parent;
        m_Transform.localPosition = transform.localPosition;
        m_Transform.localRotation = transform.localRotation;
        transform.parent = m_Transform;
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;

        RecoilRot = m_Transform.localEulerAngles;
        GunManager = transform.GetComponentInChildren<bl_GunManager>();
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        RecoilControl();
    }

    /// <summary>
    /// 
    /// </summary>
    void RecoilControl()
    {
        if (GunManager == null)
            return;

        if (GunManager.CurrentGun != null)
        {
            if (GunManager.CurrentGun.isFiring)
            {
                Quaternion q = Quaternion.Euler(new Vector3(-Recoil, 0, 0));
                m_Transform.localRotation = Quaternion.Slerp(m_Transform.localRotation, q, Time.deltaTime * RecoilSpeed);
            }
            else
            {
                Quaternion q = Quaternion.Euler(RecoilRot);
                m_Transform.localRotation = Quaternion.Slerp(m_Transform.localRotation, q, Time.deltaTime * RecoilSpeed);
                Recoil = m_Transform.localEulerAngles.x;
            }
        }
    }

    public void SetRecoil(float amount, float speed = 2)
    {
        Recoil += amount;
        Recoil = Mathf.Clamp(Recoil, 0, MaxRecoil);
        RecoilSpeed = speed;
    }

}