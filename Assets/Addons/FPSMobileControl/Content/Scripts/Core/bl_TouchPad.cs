using UnityEngine;
using System.Linq;

public class bl_TouchPad : MonoBehaviour
{

    public float Vertical { get; private set; }
    public float Horizontal { get; private set; }

    private Touch m_Touch;
    private int m_TouchID;

    void Update()
    {
        CameraRotation();
    }

    /// <summary>
    /// 
    /// </summary>
    void CameraRotation()
    {
        m_TouchID = bl_TouchHelper.GetUsableTouch();
        if (m_TouchID != -1)
        {
            m_Touch = Input.touches.ToList<Touch>().Find(x => x.fingerId == m_TouchID);
            if (m_Touch.phase == TouchPhase.Moved)
            {
                Vector2 deltaPosition = m_Touch.deltaPosition;
                Vertical = deltaPosition.y;
                Horizontal = deltaPosition.x;
            }
            else
            {
                Vertical = 0;
                Horizontal = 0;
            }
        }
    }

    public Vector2 GetInput(float sensitivity)
    {
        float x = (Horizontal * sensitivity) * Time.deltaTime;
        float y = (Vertical * sensitivity) * Time.deltaTime;
        Vector2 v = new Vector2(x, y);
        return v;
    }

    private static bl_TouchPad m_Instance;
    public static bl_TouchPad Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = FindObjectOfType(typeof(bl_TouchPad)) as bl_TouchPad;
            }
            return m_Instance;
        }
    }
}