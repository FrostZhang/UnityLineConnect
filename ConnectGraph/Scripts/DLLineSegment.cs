using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ConnectGraph
{
    public enum LineDir
    {
        hor, ver
    }

    public class DLLineSegment : DLComponent, ITrPoolItem,
        IBeginDragHandler, IDragHandler, IEndDragHandler,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerClickHandler
    {
        public Transform Tr { get; set; }
        public LineDir fx;
        public DLLine master;

        /// <summary>  起点（世界坐标） </summary>
        public Vector2 StartPos => tr.position;

        /// <summary>  终点（世界坐标） </summary>
        public Vector2 EndPos => GetWPos(1);

        /// <summary>  中心点（世界坐标） </summary>
        public Vector2 MiddlePos => GetWPos(0.5f);

        public float UILength { get { if (fx == LineDir.hor) return tr.sizeDelta.x; return tr.sizeDelta.y; } }

        public int index; //from master
        public HashSet<DLLine> linesOut;
        public HashSet<DLLine> linesIn;
        [HideInInspector] public Vector4 tempVecForMove;

        Image im;
        Vector2 dragS;
        DLLine temp;
        protected override void Awake()
        {
            base.Awake();
            im = GetComponent<Image>();
            linesOut = new HashSet<DLLine>();
            linesIn = new HashSet<DLLine>();
        }

        /// <summary>  获取线上的某点（世界坐标） </summary>
        public Vector2 GetWPos(float normalize)
        {
            var p = tr.position;
            var rc = tr.rect;
            if (fx == LineDir.hor)
            {
                p.x += rc.width * normalize * panel.Scale * tr.localScale.x;
            }
            else
            {
                p.y += rc.height * normalize * panel.Scale * tr.localScale.y;
            }
            Vector2 pos = p;
            return pos;
        }

        public void OnEnterPool()
        {
            DisConnectAllPort();
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
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            var p = eventData.position;
            if (fx == LineDir.hor)
            {
                dragS = new Vector2(p.x, tr.position.y);
            }
            else
            {
                dragS = new Vector2(tr.position.x, p.y);
            }
            temp = panel.CreatLine();
            panel.ClearEditPoints();
        }

        public void DisConnectAllPort()
        {
            foreach (var item in linesOut)
            {
                item.DeleteBif();
                item.eseg = null;
            }
            foreach (var item in linesIn)
            {
                item.sseg = null;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            var e = eventData.position;
            if (fx == LineDir.hor)
            {
                DrawUD2Auto(e);
            }
            else
            {
                DrawLR2Auto(e);
            }
        }

        void DrawLR2Auto(Vector2 e)
        {
            float vdir = Mathf.Sign(e.y - dragS.y);
            Vector2 z = Vector2.zero;
            var p = panel.FindPort(e);
            if (p)
            {
                DLLine.SegmentLR2Port(this, dragS, temp, p);
            }
            else
            {
                z = new Vector2(dragS.x + 50 * vdir, dragS.y);
                temp.DrawLines(dragS, z, new Vector2(z.x, e.y), e);
            }
        }

        void DrawUD2Auto(Vector2 e)
        {
            float vdir = Mathf.Sign(e.y - dragS.y);
            Vector2 z = Vector2.zero;
            var p = panel.FindPort(e);
            if (p)
            {
                DLLine.SegmentUD2Port(this, dragS, temp, p);
            }
            else
            {
                z = new Vector2(dragS.x, dragS.y + 50 * vdir);
                temp.DrawLines(dragS, z, new Vector2(e.x, z.y), e);
            }
        }

        public bool Contains(Vector2 screenpos)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(tr, screenpos, null, im.raycastPadding);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            temp.sseg = this;
            temp.EndPosFindPointAndConnect();

            foreach (var item in linesOut)
            {
                if (temp.eport && item.eport == temp.eport)
                {
                    Debug.LogError($"{this}-{temp.eport.pname}重复连接！！！");
                    panel.poolLine.Reprefab(temp);
                    temp = null;
                    return;
                }
                if (temp.eseg && item.eseg == temp.eseg)
                {
                    Debug.LogError($"{this}-{temp.eseg}重复连接！！！");
                    panel.poolLine.Reprefab(temp);
                    temp = null;
                    return;
                }
            }

            linesOut.Add(temp);
            temp.DrawBif(true);
            temp = null;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.dragging)
            {
                return;
            }
            master?.OnPointerEnter(this, eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            master?.OnPointerExit(this, eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            master?.OnPointerClick(this, eventData);
        }

        public void OnPosMove(Vector2 uidelta)
        {
            ChildLinesMove(uidelta);
        }

        public void OnSegSizeChanged(float uidelta, bool lockpos)
        {
            if (lockpos) return;
        }

        private void ChildLinesMove(Vector2 uidelta)
        {
            if (fx == LineDir.hor)
            {
                uidelta.x = 0;
            }
            else
            {
                uidelta.y = 0;
            }
            if (linesOut.Count > 0)
            {
                DLLine[] tli = new DLLine[linesOut.Count];
                linesOut.CopyTo(tli);
                foreach (var item in tli)
                {
                    item.CollapaseFromStart(uidelta);
                }
            }

            if (linesIn.Count > 0)
            {
                DLLine[] tli = new DLLine[linesIn.Count];
                linesIn.CopyTo(tli);
                foreach (var item in tli)
                {
                    item.CollapaseFromEnd(uidelta);
                }
            }
        }

        public override System.Xml.Linq.XElement ToJson()
        {
            System.Xml.Linq.XElement xe = new System.Xml.Linq.XElement("S");
            xe.SetAttributeValue("pos", tr.anchoredPosition.x.ToString() + "," + tr.anchoredPosition.y.ToString());
            xe.SetAttributeValue("uilen", UILength);
            xe.SetAttributeValue("fx", (int)fx);

            if (linesOut.Count > 0)
            {
                System.Xml.Linq.XElement xo = new System.Xml.Linq.XElement("linesOut");
                foreach (var item in linesOut)
                {
                    xo.Value += item.comid + ",";
                }
                xo.Value = xo.Value.TrimEnd(',');
                xe.Add(xo);
            }

            if (linesIn.Count>0)
            {
                System.Xml.Linq.XElement xi = new System.Xml.Linq.XElement("linesIn");
                foreach (var item in linesIn)
                {
                    xi.Value += item.comid + ",";
                }
                xi.Value = xi.Value.TrimEnd(',');
                xe.Add(xi);
            }

            return xe;
        }
    }
}