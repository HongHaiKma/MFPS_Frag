using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MFPS.Mobile
{
    public class bl_PersitentButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public bool Clamped = true;
        public float ClampArea = 100;

        private Vector3 defaultPosition, defaultRawPosition;
        private RectTransform m_Transform;
        private int touchID = 0;
        private Touch m_Touch;
        private bool init = false;

        void Start()
        {
            m_Transform = GetComponent<RectTransform>();
            defaultPosition = m_Transform.anchoredPosition;
            defaultRawPosition = m_Transform.position;
            init = true;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!init) return;
            touchID = eventData.pointerId;
#if !UNITY_EDITOR
          //  StartCoroutine(OnUpdate());
#endif
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!init) return;
#if !UNITY_EDITOR
           // StopAllCoroutines();
#endif
            m_Transform.anchoredPosition = defaultPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!init) return;
            if (Clamped)
            {
                if (bl_UtilityHelper.Distance(eventData.position, defaultRawPosition) > ClampArea)
                {
                    return;
                }
            }
            m_Transform.position = eventData.position;
        }

        IEnumerator OnUpdate()
        {
            while (true)
            {
                Follow();
                yield return null;
            }
        }

        void Follow()
        {
            m_Touch = Input.GetTouch(touchID);
            m_Transform.position = new Vector3(m_Touch.position.x , m_Touch.position.y, transform.position.z);
        }
    }
}