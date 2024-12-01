using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ConnectGraph
{
    public enum DLPointPos
    {
        head, middle, weiba
    }

    public class DLLineEditPoint : MonoBehaviour, ITrPoolItem,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public DLLine master;
        public LinkedListNode<DLLineSegment> linec;

        public DLPointPos fx = 0;// 0Í·1ÖÐ2Î²

        public RectTransform tr;
        public Transform Tr { get; set; }
        Image im;
        DLPanel panel;

        private void Awake()
        {
            tr = transform as RectTransform;
            im = GetComponent<Image>();
            panel = GetComponentInParent<DLPanel>();
        }

        public void OnEnterPool()
        {

        }

        public void Start()
        {

        }

        public void SetColor(Color color)
        {
            im.color = color;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            SetColor(Color.blue);
            master.UIEditPointBeginDrag(this, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            master.UIEditPointDrag(this, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            SetColor(Color.white);
            master.UIEditPointEndDrag(this, eventData);
        }
    }
}