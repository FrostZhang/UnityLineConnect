using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ConnectGraph
{
    public class DLPanelScale : MonoBehaviour, IScrollHandler, IDragHandler
    {
        private RectTransform tr;

        public float Scale => transform.localScale.x;

        public float scaleMin;
        public float scaleMax;
        protected void Awake()
        {
            tr = transform as RectTransform;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Middle)
            {
                var p = tr.localPosition;
                p.x += eventData.delta.x;
                p.y += eventData.delta.y;
                tr.localPosition = p;
            }
        }

        public void OnScroll(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(tr, eventData.position, null, out Vector2 local);
            var sc = tr.localScale;
            var f = eventData.scrollDelta.y * 0.025f;
            bool issmall = false;
            if (sc.x + f < scaleMin)
            {
                f = scaleMin - sc.x;
                issmall = true;
            }
            if (sc.x + f > scaleMax)
            {
                f = scaleMax - sc.x;
            }
            sc.x += f;
            sc.y += f;
            tr.localScale = sc;
            local.x *= f;
            local.y *= f;
            var p = tr.localPosition;
            p.x -= local.x;
            p.y -= local.y;
            tr.localPosition = p;
            if (issmall) tr.localPosition = Vector3.zero;
        }
    }
}