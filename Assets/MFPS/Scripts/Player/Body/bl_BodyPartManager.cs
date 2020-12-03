//////////////////////////////////////////////////////////////////
// bl_BodyPartManager.cs
//
// This script helps us manage our remote player hitboxes
// mind just place it in the root of the remote player
// and executes the last two options of the "ContextMenu" component. 
//                       Lovatto Studio
//////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

public class bl_BodyPartManager : bl_PhotonHelper {

    [System.Serializable]
    public class Part
    {
        public string name;
        public Collider m_Collider;
        public float m_Multipler = 1.0f;
        public bool m_HeatShot = false;
    }
    /// <summary>
    /// Change the tag if you use other
    /// </summary>
    public const string HitBoxTag = "BodyPart";
    [Header("Hit Boxes")]
    public List<Part> HitBoxs = new List<Part>();
    public List<Rigidbody> mRigidBody = new List<Rigidbody>();
    public bool ApplyVelocityToRagdoll = true;
    [Header("References")]
    public bl_PlayerAnimations PlayerAnimation;
    public Animator m_Animator;
    public Transform RightHand;
    public Transform PelvisBone;

    public GameObject KillCameraCache { get; set; }
    private Vector3 Velocity = Vector3.zero;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        GetRigidBodys();
        if (mRigidBody.Count > 0)
        {
            SetKinematic();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetLocalRagdoll(Vector3 hitPos, Transform netRoot, Vector3 velocity)
    {
        gameObject.SetActive(true);
        Velocity = velocity;
        if (RightHand != null && netRoot != null)
        {
            Vector3 RootPos = netRoot.localPosition;
            netRoot.parent = RightHand;
            netRoot.localPosition = RootPos;
        }
        Ragdolled(hitPos, false);
#if ELIM
       if(bl_RoomSettings.Instance.m_GameMode == GameMode.ELIM)
        {
            bl_UIReferences.Instance.OnKillCam(false);
            Destroy(gameObject, 5);
            return;
        }
#endif
        StartCoroutine(RespawnCountdown());
    }

    IEnumerator RespawnCountdown()
    {
        float t = bl_GameData.Instance.PlayerRespawnTime / 3;
        yield return new WaitForSeconds(t * 2);
        if (!bl_RoomMenu.Instance.isFinish)
        {
            StartCoroutine(bl_UIReferences.Instance.FinalFade(true, false));
        }
        yield return new WaitForSeconds(t);
        bl_GameManager.Instance.SpawnPlayer(PhotonNetwork.LocalPlayer.GetPlayerTeam());
        if (KillCameraCache != null) { Destroy(KillCameraCache); }
        bl_UIReferences.Instance.OnKillCam(false);
        Destroy(gameObject);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="b">is Kinematic?</param>
    public void SetKinematic(bool b = true)
    {
        if (mRigidBody == null || mRigidBody.Count <= 0)
            return;

        foreach (Rigidbody r in mRigidBody)
        {
            r.isKinematic = b;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Ragdolled(Vector3 hitPos, bool autoDestroy = true)
    {
        this.transform.parent = null;
        m_Animator.enabled = false;
        foreach (Rigidbody r in mRigidBody)
        {
            r.isKinematic = false;
            r.useGravity = true;
            if(ApplyVelocityToRagdoll)
            r.velocity = autoDestroy ? PlayerAnimation.velocity : Velocity;
        }
        Destroy(PlayerAnimation);
        if(autoDestroy)
        Destroy(gameObject, 10);
    }
    /// <summary>
    /// 
    /// </summary>
    public void IgnorePlayerCollider()
    {
        Collider Player = FindPlayerRoot(bl_GameManager.m_view).GetComponent<Collider>();
        if (Player != null)
        {
            for (int i = 0; i < HitBoxs.Count; i++)
            {
                if (HitBoxs[i].m_Collider != null)
                {
                    Physics.IgnoreCollision(HitBoxs[i].m_Collider, Player);
                }
            }
        }
    }


    [ContextMenu("Get HitBoxes")]
    void GetAllCollider()
    {
        HitBoxs.Clear();
        Collider[] mCol = transform.GetComponentsInChildren<Collider>();
        if (mCol.Length > 0)
        {
            foreach (Collider c in mCol)
            {
                if (c.gameObject.tag != HitBoxTag)
                {
                    c.gameObject.tag = HitBoxTag;
                }
                Part p = new Part();
                p.m_Collider = c;
                p.name = c.name;
                HitBoxs.Add(p);
            }
        }
        else
        {
            Debug.LogError("This transform no have colliders in children's");
        }
    }

    [ContextMenu("Add Script")]
    public void AddScript()
    {
        if (HitBoxs.Count > 0)
        {
            foreach (Part p in HitBoxs)
            {
                //DestroyImmediate(p.m_Collider.gameObject.GetComponent<bl_BodyPart>()); //use for remove script
                if (p.m_Collider != null || p.m_Collider.gameObject.GetComponent<bl_BodyPart>() == null)
                {

                    p.m_Collider.gameObject.AddComponent<bl_BodyPart>();
                    bl_BodyPart bp = p.m_Collider.gameObject.GetComponent<bl_BodyPart>();
                    bp.TakeHeatShot = p.m_HeatShot;
                    bp.multiplier = p.m_Multipler;
                    bp.HealtScript = this.transform.root.GetComponent<bl_PlayerDamageManager>();
                }
            }
        }
        else
        {
            Debug.LogError("Hit box List is empty, get hit box before");
        }
    }

    [ContextMenu("Get RigidBodys")]
    void GetRigidBodys()
    {
        Rigidbody[] R = this.transform.GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in R)
        {
            if (!mRigidBody.Contains(rb))
            {
                mRigidBody.Add(rb);
            }
        }
    }

    [ContextMenu("Get Bones")]
    public void GetRequireBones()
    {
        if (m_Animator == null) return;
        RightHand = m_Animator.GetBoneTransform(HumanBodyBones.RightHand);
        PelvisBone = m_Animator.GetBoneTransform(HumanBodyBones.Hips);
    }
}