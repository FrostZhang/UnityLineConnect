using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ConnectGraph
{
    public class DLEditModulePort : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerClickHandler
    {
        public Vector2 PortDir
        {
            get
            {
                switch (portdir)
                {
                    case DLPortDir.up:
                        return Vector2.up;
                    case DLPortDir.down:
                        return Vector2.down;
                    case DLPortDir.left:
                        return Vector2.left;
                    case DLPortDir.right:
                        return Vector2.right;
                    default:
                        break;
                }
                return Vector2.zero;
            }
        }
        public string pname;
        public RectTransform module;
        public DLPortDir portdir;
        DLLine temp;
        DLEditPanel panel;
        public Vector2 WPos => tr.position;

        Vector3[] tempRect;
        public RectTransform tr;
        protected void Awake()
        {
            panel = GetComponentInParent<DLEditPanel>();
            tempRect = new Vector3[4];
            tr = transform as RectTransform;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            module.GetWorldCorners(tempRect);
            panel.epanel.Bingding(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            var mp = eventData.position;
            var bj = 10 * panel.Scale;
            if (mp.x < tempRect[0].x && mp.y > tempRect[0].y + bj && mp.y < tempRect[1].y - bj)
            {
                portdir = DLPortDir.left;
                tr.anchorMin = tr.anchorMax = new Vector2(0, 0.5f);
                tr.position = eventData.position;
                var ap = tr.anchoredPosition;
                ap.x = 10;
                ap.y = Mathf.FloorToInt(ap.y / 5) * 5;
                tr.anchoredPosition = ap;
            }
            else if (mp.x > tempRect[3].x && mp.y > tempRect[0].y + bj && mp.y < tempRect[1].y - bj)
            {
                portdir = DLPortDir.right;
                tr.anchorMin = tr.anchorMax = new Vector2(1, 0.5f);

                tr.position = eventData.position;
                var ap = tr.anchoredPosition;
                ap.x = -10;
                ap.y = Mathf.FloorToInt(ap.y / 5) * 5;
                tr.anchoredPosition = ap;
            }
            else if (mp.y > tempRect[1].y && mp.x > tempRect[0].x + bj && mp.x < tempRect[3].x - bj)
            {
                portdir = DLPortDir.up;
                tr.anchorMin = tr.anchorMax = new Vector2(0.5f, 1);

                tr.position = eventData.position;
                var ap = tr.anchoredPosition;
                ap.y = -10;
                ap.x = Mathf.FloorToInt(ap.x / 5) * 5;
                tr.anchoredPosition = ap;
            }
            else if (mp.y < tempRect[0].y && mp.x > tempRect[0].x + bj && mp.x < tempRect[3].x - bj)
            {
                portdir = DLPortDir.down;
                tr.anchorMin = tr.anchorMax = new Vector2(0.5f, 0);

                tr.position = eventData.position;
                var ap = tr.anchoredPosition;
                ap.y = 10;
                ap.x = Mathf.FloorToInt(ap.x / 5) * 5;
                tr.anchoredPosition = ap;
            }
            else
            {
                switch (portdir)
                {
                    case DLPortDir.up:
                        if (mp.x > tempRect[0].x + bj && mp.x < tempRect[3].x - bj)
                        {
                            tr.anchorMin = tr.anchorMax = new Vector2(0.5f, 1);
                            tr.position = eventData.position;
                            var ap = tr.anchoredPosition;
                            ap.y = -10;
                            ap.x = Mathf.FloorToInt(ap.x / 5) * 5;
                            tr.anchoredPosition = ap;
                        }
                        break;
                    case DLPortDir.down:
                        if (mp.x > tempRect[0].x + bj && mp.x < tempRect[3].x - bj)
                        {
                            tr.anchorMin = tr.anchorMax = new Vector2(0.5f, 0);
                            tr.position = eventData.position;
                            var ap = tr.anchoredPosition;
                            ap.y = 10;
                            ap.x = Mathf.FloorToInt(ap.x / 5) * 5;
                            tr.anchoredPosition = ap;
                        }
                        break;
                    case DLPortDir.left:
                        if (mp.y > tempRect[0].y + bj && mp.y < tempRect[1].y - bj)
                        {
                            tr.anchorMin = tr.anchorMax = new Vector2(0, 0.5f);
                            tr.position = eventData.position;
                            var ap = tr.anchoredPosition;
                            ap.x = 10;
                            ap.y = Mathf.FloorToInt(ap.y / 5) * 5;
                            tr.anchoredPosition = ap;
                        }
                        break;
                    case DLPortDir.right:
                        if (mp.y > tempRect[0].y + bj && mp.y < tempRect[1].y - bj)
                        {
                            tr.anchorMin = tr.anchorMax = new Vector2(1, 0.5f);
                            tr.position = eventData.position;
                            var ap = tr.anchoredPosition;
                            ap.x = -10;
                            ap.y = Mathf.FloorToInt(ap.y / 5) * 5;
                            tr.anchoredPosition = ap;
                        }
                        break;
                    default:
                        break;
                }
            }

        }

        public void OnEndDrag(PointerEventData eventData)
        {

        }

        public void OnPointerEnter(PointerEventData eventData)
        {

        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.dragging)
            {
                return;
            }
            panel.epanel.Bingding(this);
        }
    }
}