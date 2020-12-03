using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class bl_Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Settings")]
    public bool SmoothReturn = true;
    [SerializeField, Range(0.1f, 2f)]private float EdgeWidth = 0.5f;//the ratio of the circumference of the joystick

    public Color NormalColor = new Color(1, 1, 1, 1);
    public Color PressColor = new Color(1, 1, 1, 1);
    [SerializeField, Range(0.1f, 5)]private float Duration = 1;

    [Header("Reference")]
    [SerializeField]private RectTransform StickRect;//The middle joystick UI

    //Privates
    private int lastId = -2;
    private Image stickImage;
    private Image backImage;

    private Vector3 _inputVector;
    public Vector3 InputVector
    {
        get
        {
            return _inputVector;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        if (StickRect == null)
        {
            Debug.LogError("Please add the stick for joystick work!.");
            this.enabled = false;
            return;
        }

        //Get the default area of joystick
        if (GetComponent<Image>() != null)
        {
            backImage = GetComponent<Image>();
            stickImage = StickRect.GetComponent<Image>();
            backImage.CrossFadeColor(NormalColor, 0.1f, true, true);
            stickImage.CrossFadeColor(NormalColor, 0.1f, true, true);
        }
    }

    /// <summary>
    /// When click here event
    /// </summary>
    /// <param name="data"></param>
    public void OnPointerDown(PointerEventData data)
    {
        //Detect if is the default touchID
        if (lastId == -2)
        {
            //then get the current id of the current touch.
            //this for avoid that other touch can take effect in the drag position event.
            //we only need get the position of this touch
            lastId = data.pointerId;
            OnDrag(data);
            if (backImage != null)
            {
                backImage.CrossFadeColor(PressColor, Duration, true, true);
                stickImage.CrossFadeColor(PressColor, Duration, true, true);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    public void OnDrag(PointerEventData data)
    {
        //If this touch id is the first touch in the event
        if (data.pointerId == lastId)
        {
            Vector2 pos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(backImage.rectTransform, data.position, null, out pos))
            {

                pos.x = (pos.x / backImage.rectTransform.sizeDelta.x);
                pos.y = (pos.y / backImage.rectTransform.sizeDelta.y);

                _inputVector = new Vector3(pos.x * 2 - 1, 0, pos.y * 2 - 1);
                _inputVector = (_inputVector.magnitude > 1.0f) ? _inputVector.normalized : _inputVector;

                StickRect.anchoredPosition = new Vector3(_inputVector.x * (backImage.rectTransform.sizeDelta.x * EdgeWidth), _inputVector.z * (backImage.rectTransform.sizeDelta.y * EdgeWidth));
            }
        }
    }

    /// <summary>
    /// When touch is Up
    /// </summary>
    /// <param name="data"></param>
    public void OnPointerUp(PointerEventData data)
    {
        //leave the default id again
        if (data.pointerId == lastId)
        {
            //-2 due -1 is the first touch id
            lastId = -2;
            StickRect.anchoredPosition = Vector3.zero;
            _inputVector = Vector3.zero;
            if (backImage != null)
            {
                backImage.CrossFadeColor(NormalColor, Duration, true, true);
                stickImage.CrossFadeColor(NormalColor, Duration, true, true);
            }
        }
    }

    /// <summary>
    /// Get the touch by the store touchID 
    /// </summary>
    public int GetTouchID
    {
        get
        {
            //find in all touches
            for (int i = 0; i < Input.touches.Length; i++)
            {
                if (Input.touches[i].fingerId == lastId)
                {
                    return i;
                }
            }
            return -1;
        }
    }

    /// <summary>
    /// Value Horizontal of the Joystick
    /// Get this for get the horizontal value of joystick
    /// </summary>
    public float Horizontal
    {
        get
        {
            return _inputVector.x;
        }
    }

    /// <summary>
    /// Value Vertical of the Joystick
    /// Get this for get the vertical value of joystick
    /// </summary>
    public float Vertical
    {
        get
        {
            return _inputVector.z;
        }
    }
}