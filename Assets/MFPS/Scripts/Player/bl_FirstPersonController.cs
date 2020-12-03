using UnityEngine;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class bl_FirstPersonController : bl_MonoBehaviour
{
    [Header("Settings")]
    public PlayerState State;
    public float WalkSpeed = 4.5f;
    [SerializeField]
    private float m_RunSpeed = 8;
    [SerializeField]
    private float m_CrouchSpeed;
    [SerializeField]
    private float m_ClimbSpeed = 1f;
    [SerializeField]
    private float m_JumpSpeed;
    [SerializeField, Range(0, 2)] private float JumpMinRate = 0.82f;
    [SerializeField, Range(0, 2)] private float AirControlMultiplier = 0.8f;
    [SerializeField]
    private float m_StickToGroundForce;
    [SerializeField]
    private float m_GravityMultiplier;
    public bool RunFovEffect = true;
    public bool KeepToCrouch = true;
    [Header("Falling")]
    public bool FallDamage = true;
    [Range(0.1f, 5f), SerializeField]
    private float SafeFallDistance = 3;
    [Range(3, 25), SerializeField]
    private float DeathFallDistance = 15;
    [Header("Mouse Look")]
    [SerializeField]
    private MouseLook m_MouseLook;
    [SerializeField]
    private Transform HeatRoot;
    public Transform CameraRoot;
    [Header("HeadBob")]
    [SerializeField]
    private LerpControlledBob m_JumpBob = new LerpControlledBob();
    public float m_StepInterval;
    public float m_RunStepInterval;
    [Header("FootSteps")]
    [Range(0.01f, 1f)]
    public float FootStepVolume = 0.25f;
    [SerializeField] private bl_FootStepsLibrary FootStepLibrary;
    [SerializeField]
    private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
    [SerializeField]
    private AudioClip m_LandSound;           // the sound played when character touches back on ground.
    [Header("UI")]
     [SerializeField]private Sprite StandIcon;
    [SerializeField]private Sprite CrouchIcon;

    private bool m_Jump;
    private Vector2 m_Input;
    private Vector3 m_MoveDir = Vector3.zero;
    private CharacterController m_CharacterController;
    public CollisionFlags m_CollisionFlags { get; set; }
    private bool m_PreviouslyGrounded;
    private bool m_Jumping;
    private bool Crounching = false;
    private AudioSource m_AudioSource;
    [HideInInspector]
    public Vector3 Velocity;
    [HideInInspector]
    public float VelocityMagnitude;
    private bl_GunManager GunManager;
    private bool Finish = false;
    private Vector3 defaultCameraRPosition;
    private bool isClimbing = false;
    private bl_Ladder m_Ladder;
    private bool MoveToStarted = false;
#if MFPSM
    private bl_Joystick Joystick;
#endif
    private float PostGroundVerticalPos = 0;
    private bool isFalling = false;
    private int JumpDirection = 0;
    private float HigherPointOnJump;
    private bl_PlayerDamageManager DamageManager;
    private float lastJumpTime = 0;
    private int WeaponWeight = 1;
    private bool hasTouchGround = false;
    private bool JumpInmune = false;
    private Transform m_Transform;
    private RaycastHit SurfaceRay;
    private Vector3 desiredMove = Vector3.zero;
    private float VerticalInput, HorizontalInput;
    private bool lastCrouchState = false;
    private float fallingTime = 0;
    public float RunFov { get; set; }

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        if (!photonView.IsMine)
            return;

        base.Awake();
        m_Transform = transform;
        m_CharacterController = GetComponent<CharacterController>();
        GunManager = GetComponentInChildren<bl_GunManager>();
        DamageManager = GetComponent<bl_PlayerDamageManager>();
#if MFPSM
        Joystick = FindObjectOfType<bl_Joystick>();
#endif
        defaultCameraRPosition = CameraRoot.localPosition;
        m_Jumping = false;
        m_AudioSource = gameObject.AddComponent<AudioSource>();
        m_AudioSource.volume = FootStepVolume;
        m_MouseLook.Init(m_Transform, HeatRoot, GunManager);
        lastJumpTime = Time.time;
        RunFov = 0;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        bl_EventHandler.OnRoundEnd += OnRoundEnd;
        bl_EventHandler.OnChangeWeapon += OnChangeWeapon;
#if MFPSM
        bl_TouchHelper.OnCrouch += OnCrouchClicked;
        bl_TouchHelper.OnJump += OnJump;
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        bl_EventHandler.OnRoundEnd -= OnRoundEnd;
        bl_EventHandler.OnChangeWeapon -= OnChangeWeapon;
#if MFPSM
        bl_TouchHelper.OnCrouch -= OnCrouchClicked;
        bl_TouchHelper.OnJump -= OnJump;
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    void OnRoundEnd()
    {
        Finish = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        Velocity = m_CharacterController.velocity;
        VelocityMagnitude = Velocity.magnitude;
        RotateView();

        if (Finish)
            return;
        if (!bl_UtilityHelper.GetCursorState || bl_GameData.Instance.isChating)
            return;

        // the jump state needs to read here to make sure it is not missed
#if INPUT_MANAGER

        if (!m_Jump && State != PlayerState.Crouching && (Time.time - lastJumpTime) > JumpMinRate)
        {
            m_Jump = bl_Input.GetKeyDown("Jump");
        }
        if (State != PlayerState.Jumping)
        {
            if (KeepToCrouch)
            {
                Crounching = bl_Input.GetKey("Crouch");
                if (Crounching != lastCrouchState)
                {
                    if (Crounching)
                    {
                        State = PlayerState.Crouching;
                        bl_UIReferences.Instance.PlayerUI.PlayerStateIcon.sprite = CrouchIcon;
                    }
                    else
                    {
                        State = PlayerState.Idle;
                        bl_UIReferences.Instance.PlayerUI.PlayerStateIcon.sprite = StandIcon;
                    }
                    bl_UCrosshair.Instance.OnCrouch(Crounching);
                    lastCrouchState = Crounching;
                }
            }
            else
            {
                if (bl_Input.GetKeyDown("Crouch"))
                {
                    Crounching = !Crounching;
                    if (Crounching)
                    {
                        State = PlayerState.Crouching;
                        bl_UIReferences.Instance.PlayerUI.PlayerStateIcon.sprite = CrouchIcon;
                    }
                    else
                    {
                        State = PlayerState.Idle;
                        bl_UIReferences.Instance.PlayerUI.PlayerStateIcon.sprite = StandIcon;
                    }
                    bl_UCrosshair.Instance.OnCrouch(Crounching);
                }
            }
        }
#else
        if (!bl_UtilityHelper.isMobile)
        {
            if (!m_Jump && State != PlayerState.Crouching && (Time.time - lastJumpTime) > JumpMinRate)
            {
                m_Jump = Input.GetKeyDown(KeyCode.Space);
            }
            if (State != PlayerState.Jumping && State != PlayerState.Climbing)
            {
                if (KeepToCrouch)
                {
                    Crounching = Input.GetKey(KeyCode.C);
                    if(Crounching != lastCrouchState)
                    {
                        if (Crounching)
                        {
                            State = PlayerState.Crouching;
                            bl_UIReferences.Instance.PlayerUI.PlayerStateIcon.sprite = CrouchIcon;
                        }
                        else
                        {
                            State = PlayerState.Idle;
                            bl_UIReferences.Instance.PlayerUI.PlayerStateIcon.sprite = StandIcon;
                        }
                        bl_UCrosshair.Instance.OnCrouch(Crounching);
                        lastCrouchState = Crounching;
                    }
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.C))
                    {
                        Crounching = !Crounching;
                        if (Crounching)
                        {
                            State = PlayerState.Crouching;
                            bl_UIReferences.Instance.PlayerUI.PlayerStateIcon.sprite = CrouchIcon;
                        }
                        else
                        {
                            State = PlayerState.Idle;
                            bl_UIReferences.Instance.PlayerUI.PlayerStateIcon.sprite = StandIcon;
                        }
                        bl_UCrosshair.Instance.OnCrouch(Crounching);
                    }
                }
            }
        }
#endif
        if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
        {
            StartCoroutine(m_JumpBob.DoBobCycle());
            PlayLandingSound();
            isFalling = false;
            if (FallDamage && hasTouchGround)
            {
                CalculateFall();
            }
            JumpDirection = 0;
            m_MoveDir.y = 0f;
            m_Jumping = false;
            State = PlayerState.Idle;

            bl_EventHandler.OnSmallImpactEvent();
        }
        else if (m_PreviouslyGrounded && !m_CharacterController.isGrounded)
        {
            if (!isFalling)
            {
                PostGroundVerticalPos = m_Transform.position.y;
                isFalling = true;
                fallingTime = Time.time;
            }
        }

        if (isFalling)
        {
            FallingCalculation();
        }

        if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
        {
            m_MoveDir.y = 0f;
        }

        Crouch();
        m_PreviouslyGrounded = m_CharacterController.isGrounded;
    }

    /// <summary>
    /// 
    /// </summary>
    void CalculateFall()
    {
        if (JumpInmune) { JumpInmune = false; return; }
        if ((Time.time - fallingTime) <= 0.4f) return;

        float ver = HigherPointOnJump - m_Transform.position.y;
        if (JumpDirection == -1)
        {
            float normalized = m_Transform.position.y + Mathf.Abs(PostGroundVerticalPos);
            ver = Mathf.Abs(normalized);
        }
        if (ver > SafeFallDistance)
        {
            float damage = (ver / DeathFallDistance) * 100;
            DamageManager.GetFallDamage(damage);
        }
      //  Debug.Log(string.Format("distance: {0}", ver));
    }

    /// <summary>
    /// 
    /// </summary>
    void FallingCalculation()
    {
        if (m_Transform.position.y == PostGroundVerticalPos) return;

        if (JumpDirection == 0)
        {
            JumpDirection = (m_Transform.position.y > PostGroundVerticalPos) ? 1 : -1;
        }
        else if (JumpDirection == 1)
        {
            if (m_Transform.position.y < PostGroundVerticalPos)
            {
                HigherPointOnJump = PostGroundVerticalPos;
            }
            else
                PostGroundVerticalPos = m_Transform.position.y;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    private void PlayLandingSound()
    {
        m_AudioSource.clip = m_LandSound;
        m_AudioSource.Play();
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnFixedUpdate()
    {
        if (Finish)
            return;
        if (m_CharacterController == null || !m_CharacterController.enabled || MoveToStarted)
            return;

        if (bl_UtilityHelper.GetCursorState && !bl_GameData.Instance.isChating)
        {
            float s = 0;
            GetInput(out s);
            speed = s;
        }
        else
        {
            m_Input = Vector2.zero;
        }
        if (isClimbing && m_Ladder != null)
        {
            OnClimbing();
        }
        else
        {
            // always move along the camera forward as it is the direction that it being aimed at
            desiredMove = (m_Transform.forward * m_Input.y) + (m_Transform.right * m_Input.x);

            // get a normal for the surface that is being touched to move along it
            Physics.SphereCast(m_Transform.position, m_CharacterController.radius * 0.5f, Vector3.down, out SurfaceRay, m_CharacterController.height * 0.5f, Physics.AllLayers, QueryTriggerInteraction.Ignore);

            desiredMove = Vector3.ProjectOnPlane(desiredMove, SurfaceRay.normal);
            m_MoveDir.x = desiredMove.x * speed;
            m_MoveDir.z = desiredMove.z * speed;

            if (m_CharacterController.isGrounded)
            {
                m_MoveDir.y = -m_StickToGroundForce;
                hasTouchGround = true;
                if (m_Jump || hasPlatformJump)
                {
                    m_MoveDir.y = (hasPlatformJump) ? PlatformJumpForce : m_JumpSpeed;
                    PlayJumpSound();
                    m_Jump = false;
                    m_Jumping = true;
                    hasPlatformJump = false;
                    State = PlayerState.Jumping;
                    lastJumpTime = Time.time;
                }
            }
            else
            {
                float airControlMult = AirControlMultiplier;
                float gravity = m_GravityMultiplier;
#if LMS
                if(GetGameMode == GameMode.LSM && !hasTouchGround)
                {
                    airControlMult = 3f;
                    gravity = gravity / 2.5f;
                }
#endif
                m_MoveDir += Physics.gravity * gravity * Time.fixedDeltaTime;
                m_MoveDir.x = (desiredMove.x * speed) * airControlMult;
                m_MoveDir.z = (desiredMove.z * speed) * airControlMult;
            }
            m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void PlayJumpSound()
    {
        m_AudioSource.clip = m_JumpSound;
        m_AudioSource.Play();
    }

    /// <summary>
    /// 
    /// </summary>
    RaycastHit footRay;
    public void PlayFootStepAudio(bool b)
    {
        if (!m_CharacterController.isGrounded && !isClimbing)
        {
            return;
        }
        if (!isClimbing)
        {
            string _tag = "none";
            int n = 0;
            if (Physics.Raycast(m_Transform.position, -Vector3.up, out footRay, 5))
            {
                _tag = footRay.transform.tag;
            }
            switch (_tag)
            {
                case "Water":
                    n = Random.Range(1,FootStepLibrary.WatertepSounds.Length);
                    m_AudioSource.clip = FootStepLibrary.WatertepSounds[n];
                    m_AudioSource.PlayOneShot(m_AudioSource.clip);
                    // move picked sound to index 0 so it's not picked next time
                    FootStepLibrary.WatertepSounds[n] = FootStepLibrary.WatertepSounds[0];
                    FootStepLibrary.WatertepSounds[0] = m_AudioSource.clip;
                    break;
                case "Metal":
                    n = Random.Range(1, FootStepLibrary.MetalStepSounds.Length);
                    m_AudioSource.clip = FootStepLibrary.MetalStepSounds[n];
                    m_AudioSource.PlayOneShot(m_AudioSource.clip);
                    // move picked sound to index 0 so it's not picked next time
                    FootStepLibrary.MetalStepSounds[n] = FootStepLibrary.MetalStepSounds[0];
                    FootStepLibrary.MetalStepSounds[0] = m_AudioSource.clip;
                    break;
                default:
                    n = Random.Range(1, FootStepLibrary.m_FootstepSounds.Length);
                    m_AudioSource.clip = FootStepLibrary.m_FootstepSounds[n];
                    m_AudioSource.PlayOneShot(m_AudioSource.clip);
                    // move picked sound to index 0 so it's not picked next time
                    FootStepLibrary.m_FootstepSounds[n] = FootStepLibrary.m_FootstepSounds[0];
                    FootStepLibrary.m_FootstepSounds[0] = m_AudioSource.clip;
                    break;
            }
        }
        else
        {
            int n = Random.Range(1, FootStepLibrary.MetalStepSounds.Length);
            m_AudioSource.clip = FootStepLibrary.MetalStepSounds[n];
            m_AudioSource.PlayOneShot(m_AudioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            FootStepLibrary.MetalStepSounds[n] = FootStepLibrary.MetalStepSounds[0];
            FootStepLibrary.MetalStepSounds[0] = m_AudioSource.clip;
        }
    }

    /// <summary>
    /// When player is in Crouch
    /// </summary>
    void Crouch()
    {
        if (Crounching)
        {
            if (m_CharacterController.height != 1.4f)
            {
                m_CharacterController.height = 1.4f;
            }
            m_CharacterController.center = new Vector3(0, -0.3f, 0);
            Vector3 ch = CameraRoot.transform.localPosition;
            if (CameraRoot.transform.localPosition.y != 0.2f)
            {
                ch.y = Mathf.Lerp(ch.y, 0.2f, Time.deltaTime * 8);
                CameraRoot.transform.localPosition = ch;
            }
        }
        else
        {
            if (m_CharacterController.height != 2f)
            {
                m_CharacterController.height = 2f;
            }
            m_CharacterController.center = Vector3.zero;
            Vector3 ch = CameraRoot.transform.localPosition;
            if (ch.y != defaultCameraRPosition.y)
            {
                ch.y = Mathf.Lerp(ch.y, defaultCameraRPosition.y, Time.deltaTime * 8);
                CameraRoot.transform.localPosition = ch;
            }
        }
    }

    private void GetInput(out float speed)
    {
        // Read input
#if INPUT_MANAGER
        HorizontalInput = bl_Input.HorizontalAxis;
        VerticalInput = bl_Input.VerticalAxis;
#else
        HorizontalInput = Input.GetAxis("Horizontal");
        VerticalInput = Input.GetAxis("Vertical");

#endif
#if MFPSM
        if (bl_UtilityHelper.isMobile)
        {
            HorizontalInput = Joystick.Horizontal;
            VerticalInput = Joystick.Vertical;
            VerticalInput = VerticalInput * 1.25f;
        }
#endif

        m_Input = new Vector2(HorizontalInput, VerticalInput);


#if !INPUT_MANAGER
        if (State != PlayerState.Climbing)
        {
            if (m_Input.magnitude > 0)
            {
                if (!bl_UtilityHelper.isMobile)
                {
                    // On standalone builds, walk/run speed is modified by a key press.
                    // keep track of whether or not the character is walking or running
                    if (Input.GetKey(KeyCode.LeftShift) && State != PlayerState.Crouching && VerticalInput > 0)
                    {
                        State = PlayerState.Running;
                    }
                    else if (Input.GetKeyUp(KeyCode.LeftShift) && State != PlayerState.Crouching && VerticalInput > 0)
                    {
                        State = PlayerState.Walking;
                    }
                    else if (State != PlayerState.Crouching && VerticalInput > 0)
                    {
                        State = PlayerState.Walking;
                    }
                    else if (State != PlayerState.Jumping && State != PlayerState.Crouching)
                    {
                        State = PlayerState.Idle;
                    }

                }
                else
                {
                    if (VerticalInput > 1 && VerticalInput > 0.05f && State != PlayerState.Crouching)
                    {
                        State = PlayerState.Running;
                    }
                    else if (VerticalInput <= 1 && VerticalInput != 0 && State != PlayerState.Crouching)
                    {
                        State = PlayerState.Walking;
                    }
                    else if (State != PlayerState.Crouching && VerticalInput != 0)
                    {
                        State = PlayerState.Walking;
                    }
                    else if (State != PlayerState.Jumping && State != PlayerState.Crouching)
                    {
                        State = PlayerState.Idle;
                    }
                }
            }
            else if (m_CharacterController.isGrounded)
            {
                if (State != PlayerState.Jumping && State != PlayerState.Crouching)
                {
                    State = PlayerState.Idle;
                }
            }
        }
#else

        if (State != PlayerState.Climbing)
        {
            if (m_Input.magnitude > 0)
            {
                if (!bl_UtilityHelper.isMobile)
                {
                    // On standalone builds, walk/run speed is modified by a key press.
                    // keep track of whether or not the character is walking or running
                    if (bl_Input.GetKey("Run") && State != PlayerState.Crouching && VerticalInput > 0)
                    {
                        State = PlayerState.Running;
                    }
                    else if (bl_Input.GetKeyUp("Run") && State != PlayerState.Crouching && VerticalInput > 0)
                    {
                        State = PlayerState.Walking;
                    }
                    else if (State != PlayerState.Crouching && VerticalInput > 0)
                    {
                        State = PlayerState.Walking;
                    }
                    else if (State != PlayerState.Jumping && State != PlayerState.Crouching)
                    {
                        State = PlayerState.Idle;
                    }

                }
                else
                {
                    if (VerticalInput > 1 && VerticalInput > 0.05f && State != PlayerState.Crouching)
                    {
                        State = PlayerState.Running;
                    }
                    else if (VerticalInput <= 1 && VerticalInput != 0 && State != PlayerState.Crouching)
                    {
                        State = PlayerState.Walking;
                    }
                    else if (State != PlayerState.Crouching && VerticalInput != 0)
                    {
                        State = PlayerState.Walking;
                    }
                    else if (State != PlayerState.Jumping && State != PlayerState.Crouching)
                    {
                        State = PlayerState.Idle;
                    }
                }
            }
            else if (m_CharacterController.isGrounded)
            {
                if (State != PlayerState.Jumping && State != PlayerState.Crouching)
                {
                    State = PlayerState.Idle;
                }
            }
        }       
#endif

        if (Crounching)
        {
            speed = m_CrouchSpeed;
        }
        else
        {
            // set the desired speed to be walking or running
            speed = (State == PlayerState.Running) ? m_RunSpeed : WalkSpeed;
        }
        // normalize input if it exceeds 1 in combined length:
        if (m_Input.sqrMagnitude > 1)
        {
            m_Input.Normalize();
        }
        if (RunFovEffect)
        {
            float rf = State == PlayerState.Running ? 8 : 0;
            RunFov = Mathf.Lerp(RunFov, rf, Time.deltaTime * 6);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnClimbing()
    {
        if (m_Ladder.HasPending)
        {
            if (!MoveToStarted)
            {
                StartCoroutine(MoveTo(m_Ladder.GetCurrent, false));
            }
        }
        else
        {

            desiredMove = m_Ladder.transform.rotation * Vector3.forward * m_Input.y;
            m_MoveDir.y = desiredMove.y * m_ClimbSpeed;
            m_MoveDir.x = desiredMove.x * m_ClimbSpeed;
            m_MoveDir.z = desiredMove.z * m_ClimbSpeed;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ToggleClimbing();
                m_Ladder.JumpOut();
                m_MoveDir.y = m_JumpSpeed;
                m_MoveDir.z = 30;
                lastJumpTime = Time.time;
            }
            m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private bool hasPlatformJump = false;
    private float PlatformJumpForce = 0;
    public void PlatformJump(float force)
    {
        hasPlatformJump = true;
        PlatformJumpForce = force;
        JumpInmune = true;
    }

#if MFPSM
    /// <summary>
    /// 
    /// </summary>
    void OnCrouchClicked()
    {
        Crounching = !Crounching;
        if (Crounching) { State = PlayerState.Crouching; } else { State = PlayerState.Idle; }
       bl_UIReferences.Instance.PlayerUI.PlayerStateIcon.sprite = (Crounching) ? CrouchIcon : StandIcon;
        bl_UCrosshair.Instance.OnCrouch(Crounching);
    }

    void OnJump()
    {
        if (!m_Jump && State != PlayerState.Crouching)
        {
            m_Jump = true;
        }
    }
#endif
    /// <summary>
    /// 
    /// </summary>
    private void RotateView()
    {
        if (!isClimbing)
        {
            m_MouseLook.LookRotation(m_Transform, HeatRoot);
        }
        else
        {
            m_MouseLook.LookRotation(m_Transform, HeatRoot, m_Ladder.InsertionPoint);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent == null)
            return;

        bl_Ladder l = other.transform.parent.GetComponent<bl_Ladder>();
        if (l != null)
        {
            if (!l.CanUse)
                return;

            m_Ladder = l;
            if (other.transform.name == bl_Ladder.BottomColName)
            {
                m_Ladder.InsertionPoint = other.transform;
                if (!isClimbing)
                {
                    m_Ladder.ToBottom();
                    ToggleClimbing();
                }
                else
                {
                    ToggleClimbing();
                    m_Ladder.HasPending = false;
                }
            }
            else if (other.transform.name == bl_Ladder.TopColName)
            {
                m_Ladder.InsertionPoint = other.transform;
                if (isClimbing)
                {
                    m_Ladder.SetToTop();
                    if (!MoveToStarted)
                    {
                        StartCoroutine(MoveTo(m_Ladder.GetCurrent, true));
                    }
                }
                else
                {
                    m_Ladder.ToMiddle();
                }
                ToggleClimbing();
            }
        }
    }


    void OnChangeWeapon(int id)
    {
        WeaponWeight = bl_GameData.Instance.GetWeapon(id).Weight;
    }

    private void ToggleClimbing()
    {
        isClimbing = !isClimbing;
        State = (isClimbing) ? PlayerState.Climbing : PlayerState.Idle;
        bl_UIReferences.Instance.JumpLadder.SetActive(isClimbing);
    }

    IEnumerator MoveTo(Vector3 pos, bool down)
    {
        MoveToStarted = true;
        bool small = false;
        float t = 0;
        while (t < 0.7f)
        {
            t += Time.deltaTime / 1.5f;
            m_Transform.position = Vector3.Lerp(m_Transform.position, pos, t);
            if (t >= 0.6f && !small && down)
            {
                bl_EventHandler.OnSmallImpact();
                small = true;
            }
            yield return new WaitForFixedUpdate();
        }
        if (m_Ladder != null)
        {
            m_Ladder.HasPending = false;
        }
        MoveToStarted = false;
    }

    /// <summary>
    /// Enable this if you want player controller apply force on contact to rigidbodys
    /// is commented by default for performance matters.
    /// </summary>
  /*  private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        //dont move the rigidbody if the character is on top of it
        if (m_CollisionFlags == CollisionFlags.Below)
        {
            return;
        }

        if (body == null || body.isKinematic)
        {
            return;
        }
        body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
    }*/

    internal float _speed = 0;
    public float speed
    {
        get
        {
            return _speed;
        }
        set
        {
            _speed = value - WeaponWeight;
            _speed = Mathf.Clamp(_speed, 2, 10);
        }
    }

    public bool isGrounded { get { return m_CharacterController.isGrounded; } }
}