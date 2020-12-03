using UnityEngine;
using System;

public class bl_GunBob : bl_MonoBehaviour
{

    [Range(0.1f, 2)] public float WalkSpeedMultiplier = 1f;
    [Range(0.1f, 2)] public float RunSpeedMultiplier = 1f;
    [Range(0, 15)] public float EulerZAmount = 5;
    [Range(0, 15)] public float RunEulerZAmount = 5;
    [Range(0, 15)] public float EulerXAmount = 5;
    [Range(0, 15)] public float RunEulerXAmount = 5;

    public float idleBobbingSpeed = 0.1f;
    [Range(0, 0.2f)] public float WalkOscillationAmount = 0.04f;
    [Range(0, 0.2f)] public float RunOscillationAmount = 0.1f;

    public float WalkLerpSpeed = 2;
    public float RunLerpSpeed = 4;

    Vector3 midpoint;
    Vector3 localRotation;
    GameObject player;
    float timer = 0.0f;
    float lerp = 2;
    float bobbingSpeed;
    bl_FirstPersonController motor;
    float BobbingAmount;
    float tempWalkSpeed = 0;
    float tempRunSpeed = 0;
    float tempIdleSpeed = 0;
    float waveslice = 0.0f;
    float waveslice2 = 0.0f;
    public bool isAim { get; set; }
    float eulerZ = 0;
    float eulerX = 0;
    private bool rightFoot = false;
    public float Intensitity { get; set; }
    private Transform m_Transform;
    Vector3 currentPosition = Vector3.zero;
    Vector3 currentRotation = Vector3.zero;
    public bool useAnimation { get; set; }
    private Action<PlayerState> AnimateCallback = null;

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        player = transform.root.gameObject;
        motor = player.GetComponent<bl_FirstPersonController>();
        midpoint = transform.localPosition;
        localRotation = transform.localEulerAngles;
        Intensitity = 1;
        m_Transform = transform;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (motor == null) return;

        if (useAnimation)
        {
            if (AnimateCallback == null) return;
            AnimateCallback.Invoke(motor.State);
        }
        else
        {
            StateControl();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnFixedUpdate()
    {
        if (useAnimation) return;

        Movement();
    }

    /// <summary>
    /// 
    /// </summary>
    void StateControl()
    {
        if (motor.State == PlayerState.Jumping) return;
        if (motor.VelocityMagnitude > 0.1f && motor.State != PlayerState.Running)
        {
            bobbingSpeed = tempWalkSpeed;
            BobbingAmount = WalkOscillationAmount;
            lerp = WalkLerpSpeed;
            eulerZ = EulerZAmount;
            eulerX = EulerXAmount;
        }
        else if (motor.State == PlayerState.Running)
        {
            bobbingSpeed = tempRunSpeed;
            BobbingAmount = RunOscillationAmount;
            lerp = RunLerpSpeed;
            eulerZ = RunEulerZAmount;
            eulerX = RunEulerXAmount;
        }

        if (motor.State != PlayerState.Running && motor.VelocityMagnitude < 0.1f || !bl_UtilityHelper.GetCursorState)
        {
            bobbingSpeed = tempIdleSpeed;
            BobbingAmount = WalkOscillationAmount * 0.1f;
            lerp = WalkLerpSpeed;
            eulerZ = EulerZAmount;
            eulerX = EulerXAmount;
        }
    }

    void Movement()
    {
        float time = Time.smoothDeltaTime;

        tempWalkSpeed = 0;
        tempRunSpeed = 0;
        tempIdleSpeed = 0;

        if (tempIdleSpeed != idleBobbingSpeed)
        {
            tempWalkSpeed = motor.speed * 0.06f * WalkSpeedMultiplier;
            tempRunSpeed = motor.speed * 0.03f * RunSpeedMultiplier;
            tempIdleSpeed = idleBobbingSpeed;
        }

        waveslice = Mathf.Sin(timer * 2);
        waveslice2 = Mathf.Sin(timer);
        timer = timer + bobbingSpeed;
        if (timer > Mathf.PI * 2)
        {
            timer = timer - (Mathf.PI * 2);
        }

        if (waveslice != 0)
        {
            float TranslateChange = waveslice * BobbingAmount * Intensitity;
            float TranslateChange2 = waveslice2 * BobbingAmount * Intensitity;
            float rotChange = waveslice2 * eulerZ;
            float rotChange2 = waveslice * eulerX;

            if (motor.isGrounded)
            {
                if (motor.VelocityMagnitude > 0.1f && motor.State != PlayerState.Idle)
                {
                    currentPosition = new Vector3(midpoint.x + TranslateChange2, midpoint.y + TranslateChange, currentPosition.z);
                    currentRotation = new Vector3(localRotation.x + rotChange2, localRotation.y, localRotation.z + rotChange);
                    Vector3 bob = new Vector3(0, 0, (rotChange * 0.9f));
                    m_Transform.localRotation = Quaternion.Slerp(m_Transform.localRotation, Quaternion.Euler(currentRotation), time * lerp);
                    motor.CameraRoot.localRotation = Quaternion.Slerp(motor.CameraRoot.localRotation, Quaternion.Euler(bob), time * lerp);
                }
                else
                {
                    currentPosition = new Vector3(midpoint.x, midpoint.y + TranslateChange, currentPosition.z);
                    m_Transform.localRotation = Quaternion.Slerp(m_Transform.localRotation, Quaternion.Euler(Vector3.zero), time * 10);
                    motor.CameraRoot.localRotation = Quaternion.Slerp(motor.CameraRoot.localRotation, Quaternion.Euler(Vector3.zero), time * lerp);
                }
            }
        }
        else
        {
            //Player not move
            currentPosition = midpoint;
            m_Transform.localRotation = Quaternion.Slerp(m_Transform.localRotation, Quaternion.Euler(Vector3.zero), time * 12);
            motor.CameraRoot.localRotation = Quaternion.Slerp(motor.CameraRoot.localRotation, Quaternion.Euler(Vector3.zero), time * lerp);
        }
        m_Transform.localPosition = Vector3.Lerp(m_Transform.localPosition, currentPosition, time * lerp);

        if (motor.VelocityMagnitude > 0.1f)
        {
            if (waveslice2 >= 0.97f && !rightFoot)
            {
                motor.PlayFootStepAudio(true);
                rightFoot = true;
            }
            else if (waveslice2 <= (-0.97f) && rightFoot)
            {
                motor.PlayFootStepAudio(true);
                rightFoot = false;
            }
        }
    }

    public void AnimatedThis(Action<PlayerState> callback, bool useAnim)
    {
        AnimateCallback = callback;
        useAnimation = useAnim;
    }
}