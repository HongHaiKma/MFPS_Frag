using System;
using UnityEngine;

[Serializable]
public class MouseLook
{
    public float XSensitivity = 2f;
    public float YSensitivity = 2f;
    public bool clampVerticalRotation = true;
    public float MinimumX = -90F;
    public float MaximumX = 90F;
    public bool smooth;
    public float smoothTime = 5f;


    private Quaternion m_CharacterTargetRot;
    private Quaternion m_CameraTargetRot;
    private bl_GunManager GunManager;
    private bool InvertVertical;
    private bool InvertHorizontal;

    public void Init(Transform character, Transform camera, bl_GunManager gm)
    {
        m_CharacterTargetRot = character.localRotation;
        m_CameraTargetRot = camera.localRotation;
        GunManager = gm;
        XSensitivity = bl_RoomMenu.Instance.m_sensitive;
        YSensitivity = bl_RoomMenu.Instance.m_sensitive;
    }


    public void LookRotation(Transform character, Transform camera, Transform ladder = null)
    {
#if MFPSM
        if (bl_UtilityHelper.isMobile)
        {
            CalculateSensitivity();
            CameraRotation(character, camera);
            return;
        }
#endif

        if (bl_UtilityHelper.GetCursorState)
        {
            CalculateSensitivity();

            if (ladder == null)
            {
                float yRot = Input.GetAxis("Mouse X") * XSensitivity;
                yRot = (InvertHorizontal) ? (yRot * -1f) : yRot;
                m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
            }
            else
            {
                Vector3 direction = ladder.forward;
                direction.y = 0;
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                m_CharacterTargetRot = Quaternion.Slerp(m_CharacterTargetRot, lookRotation, Time.deltaTime * 5);
            }

            float xRot = Input.GetAxis("Mouse Y") * YSensitivity;
            xRot = (InvertVertical) ? (xRot * -1f) : xRot;
            m_CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

            if (clampVerticalRotation)
                m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

            if (smooth)
            {
                if (character != null) { character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot, smoothTime * Time.deltaTime); }
                if (camera != null) { camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot, smoothTime * Time.deltaTime); }
            }
            else
            {
                character.localRotation = m_CharacterTargetRot;
                camera.localRotation = m_CameraTargetRot;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void CalculateSensitivity()
    {
        if (GunManager != null && GunManager.GetCurrentWeapon() != null)
        {
            if (!GunManager.GetCurrentWeapon().isAimed)
            {
                XSensitivity = bl_RoomMenu.Instance.m_sensitive;
                YSensitivity = bl_RoomMenu.Instance.m_sensitive;
            }
            else
            {
                XSensitivity = bl_RoomMenu.Instance.SensitivityAim;
                YSensitivity = bl_RoomMenu.Instance.SensitivityAim;
            }
        }
        InvertHorizontal = bl_RoomMenu.Instance.SetIMH;
        InvertVertical = bl_RoomMenu.Instance.SetIMV;
    }

#if MFPSM
    void CameraRotation(Transform character, Transform camera)
    {
        Vector2 input = bl_TouchPad.Instance.GetInput(XSensitivity);
        input.x = !InvertHorizontal ? input.x : (input.x * -1f);
        input.y = !InvertVertical ? (input.y * -1f) : input.y;

        m_CharacterTargetRot *= Quaternion.Euler(0f, input.x, 0f);
        m_CameraTargetRot *= Quaternion.Euler(input.y, 0f, 0f);

        if (clampVerticalRotation)
            m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

        camera.localRotation = m_CameraTargetRot;
        character.localRotation = m_CharacterTargetRot;
    }
#endif

    Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

        angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }

}