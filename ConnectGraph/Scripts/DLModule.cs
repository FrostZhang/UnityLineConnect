using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ConnectGraph
{
    public class DLModule : DLComponent,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerEnterHandler
    {
        public string mname;
        public DLModulePort[] pots;

        //一种特殊的模块 由上下或左右两条线组成
        public DLLineSegment[] segs;

        public int LinesCount
        {
            get
            {
                int count = 0;
                foreach (var item in pots)
                {
                    count += item.linesOut.Count;
                    count += item.linesIn.Count;
                }
                return count;
            }
        }

        Vector2 tdelta;
        protected override void Awake()
        {
            base.Awake();
            pots = GetComponentsInChildren<DLModulePort>();
            for (int i = 0; i < pots.Length; i++)
            {
                pots[i].module = this;
                pots[i].index = i;
            }
        }

        private void Start()
        {
            if (string.IsNullOrEmpty(mname))
            {
                mname = name;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            panel.ClearEditPoints();
            foreach (var item in pots)
            {
                foreach (var dll in item.linesOut)
                {
                    dll.CalTempWeightForMove();
                }
                foreach (var dll in item.linesIn)
                {
                    dll.CalTempWeightForMove();
                }
            }
            tdelta = Vector2.zero;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            Move(eventData);
        }

        public void SetJsonID(int newid)
        {
            this.comid = newid.ToString();
            for (int i = 0; i < pots.Length; i++)
            {
                pots[i].SetJsonID(comid + "_" + i);
            }
        }

        //移动模块
        private void Move(PointerEventData eventData)
        {
            var delta = eventData.delta;
            tdelta += delta;

            var c = LinesCount;
            if (c == 0)
            {
                var uid = tdelta / panel.Scale;
                Vector2 pos = tr.anchoredPosition;
                pos += uid;
                pos.x = Mathf.RoundToInt(pos.x);
                pos.y = Mathf.RoundToInt(pos.y);
                tdelta -= (pos - tr.anchoredPosition) * panel.Scale;
                tr.anchoredPosition = pos;
            }
            else if (c == 1)
            {
                DLLine one = null;
                bool isout = false;
                foreach (var item in pots)
                {
                    if (item.linesOut.Count == 1)
                    {
                        one = item.linesOut.First();
                        isout = true;
                        break;
                    }
                    if (item.linesIn.Count == 1)
                    {
                        one = item.linesIn.First();
                        isout = false;
                        break;
                    }
                }
                if (isout)
                {
                    MoveOneLine(eventData, one);
                }
                else
                {
                    MoveOneLine2(eventData, one);
                }
            }
            else
            {
                MoveLines(eventData);
            }
        }

        private void MoveOneLine(PointerEventData eventData, DLLine one)
        {
            if (one.eport != null)
            {
                bool l2r = one.sport.portdir == DLPortDir.left && one.eport.portdir == DLPortDir.right && one.sport.WPos.x > one.eport.WPos.x;
                bool r2l = one.sport.portdir == DLPortDir.right && one.eport.portdir == DLPortDir.left && one.sport.WPos.x < one.eport.WPos.x;
                bool u2d = one.sport.portdir == DLPortDir.up && one.eport.portdir == DLPortDir.down && one.sport.WPos.y < one.eport.WPos.y;
                bool d2u = one.sport.portdir == DLPortDir.down && one.eport.portdir == DLPortDir.up && one.sport.WPos.y > one.eport.WPos.y;
                //水平吸附效应
                if (l2r || r2l)
                {
                    var y = one.sport.WPos.y + tdelta.y;
                    if (Mathf.Abs(y - one.eport.WPos.y) < 15)
                    {
                        var dy = one.eport.WPos.y - one.sport.WPos.y;
                        Vector2 p = tr.position;
                        p += new Vector2(tdelta.x, dy);
                        tr.position = p;
                        one.CollapaseFromStart(new Vector2(tdelta.x / panel.Scale, dy / panel.Scale));
                        tdelta.x = 0;
                    }
                    else
                    {
                        Vector2 p = tr.position;
                        p += tdelta;
                        tr.position = p;
                        one.CollapaseFromStart(tdelta / panel.Scale);
                        tdelta = Vector2.zero;
                    }
                }
                //竖直吸附效应
                else if (u2d || d2u)
                {
                    var x = one.sport.WPos.x + tdelta.x;
                    if (Mathf.Abs(x - one.eport.WPos.x) < 15)
                    {
                        var dx = one.eport.WPos.x - one.sport.WPos.x;
                        Vector2 p = tr.position;
                        p += new Vector2(dx, tdelta.y);
                        tr.position = p;
                        one.CollapaseFromStart(new Vector2(dx / panel.Scale, tdelta.y / panel.Scale));
                        tdelta.y = 0;
                    }
                    else
                    {
                        Vector2 p = tr.position;
                        p += tdelta;
                        tr.position = p;
                        one.CollapaseFromStart(tdelta / panel.Scale);
                        tdelta = Vector2.zero;
                    }
                }
                else
                {
                    MoveLines(eventData);
                }
            }
            else if (one.eseg)
            {
                //竖直吸附效应
                if (one.sport.PortDir.x != 0 && one.eseg.fx == LineDir.ver)
                {
                    var y = one.sport.WPos.y + tdelta.y;
                    if (Mathf.Abs(y - one.EndPos.y) < 15)
                    {
                        var dy = one.EndPos.y - one.sport.WPos.y;
                        Vector2 p = tr.position;
                        p += new Vector2(tdelta.x, dy);
                        tr.position = p;
                        one.CollapaseFromStart(new Vector2(tdelta.x / panel.Scale, dy / panel.Scale));
                        tdelta.x = 0;
                    }
                    else
                    {
                        Vector2 p = tr.position;
                        p += tdelta;
                        tr.position = p;
                        one.CollapaseFromStart(tdelta / panel.Scale);
                        tdelta = Vector2.zero;
                    }
                }
                //水平吸附效应
                else if (one.sport.PortDir.y != 0 && one.eseg.fx == LineDir.hor)
                {
                    var x = one.sport.WPos.x + tdelta.x;
                    if (Mathf.Abs(x - one.EndPos.x) < 15)
                    {
                        var dx = one.EndPos.x - one.sport.WPos.x;
                        Vector2 p = tr.position;
                        p += new Vector2(dx, tdelta.y);
                        tr.position = p;
                        one.CollapaseFromStart(new Vector2(dx / panel.Scale, tdelta.y / panel.Scale));
                        tdelta.y = 0;
                    }
                    else
                    {
                        Vector2 p = tr.position;
                        p += tdelta;
                        tr.position = p;
                        one.CollapaseFromStart(tdelta / panel.Scale);
                        tdelta = Vector2.zero;
                    }
                }
                else
                {
                    MoveLines(eventData);
                }
            }
            else
            {
                MoveLines(eventData);
            }
        }

        private void MoveOneLine2(PointerEventData eventData, DLLine one)
        {
            if (one.sport != null)
            {
                bool l2r = one.sport.portdir == DLPortDir.left && one.eport.portdir == DLPortDir.right && one.sport.WPos.x > one.eport.WPos.x;
                bool r2l = one.sport.portdir == DLPortDir.right && one.eport.portdir == DLPortDir.left && one.sport.WPos.x < one.eport.WPos.x;
                bool u2d = one.sport.portdir == DLPortDir.up && one.eport.portdir == DLPortDir.down && one.sport.WPos.y < one.eport.WPos.y;
                bool d2u = one.sport.portdir == DLPortDir.down && one.eport.portdir == DLPortDir.up && one.sport.WPos.y > one.eport.WPos.y;
                //水平吸附效应
                if (l2r || r2l)
                {
                    var y = one.eport.WPos.y + tdelta.y;
                    if (Mathf.Abs(y - one.sport.WPos.y) < 15)
                    {
                        var dy = one.sport.WPos.y - one.eport.WPos.y;
                        Vector2 p = tr.position;
                        p += new Vector2(tdelta.x, dy);
                        tr.position = p;
                        one.CollapaseFromEnd(new Vector2(tdelta.x / panel.Scale, dy / panel.Scale));
                        tdelta.x = 0;
                    }
                    else
                    {
                        Vector2 p = tr.position;
                        p += tdelta;
                        tr.position = p;
                        one.CollapaseFromEnd(tdelta / panel.Scale);
                        tdelta = Vector2.zero;
                    }
                }
                //竖直吸附效应
                else if (u2d || d2u)
                {
                    var x = one.eport.WPos.x + tdelta.x;
                    if (Mathf.Abs(x - one.sport.WPos.x) < 15)
                    {
                        var dx = one.sport.WPos.x - one.eport.WPos.x;
                        Vector2 p = tr.position;
                        p += new Vector2(dx, tdelta.y);
                        tr.position = p;
                        one.CollapaseFromEnd(new Vector2(dx / panel.Scale, tdelta.y / panel.Scale));
                        tdelta.y = 0;
                    }
                    else
                    {
                        Vector2 p = tr.position;
                        p += tdelta;
                        tr.position = p;
                        one.CollapaseFromEnd(tdelta / panel.Scale);
                        tdelta = Vector2.zero;
                    }
                }
                else
                {
                    MoveLines(eventData);
                }
            }
            else if (one.sseg)
            {
                //竖直吸附效应
                if (one.eport.PortDir.x != 0 && one.sseg.fx == LineDir.ver)
                {
                    var y = one.eport.WPos.y + tdelta.y;
                    if (Mathf.Abs(y - one.FirstPos.y) < 15)
                    {
                        var dy = one.FirstPos.y - one.eport.WPos.y;
                        Vector2 p = tr.position;
                        p += new Vector2(tdelta.x, dy);
                        tr.position = p;
                        one.CollapaseFromEnd(new Vector2(tdelta.x / panel.Scale, dy / panel.Scale));
                        tdelta.x = 0;
                    }
                    else
                    {
                        Vector2 p = tr.position;
                        p += tdelta;
                        tr.position = p;
                        one.CollapaseFromEnd(tdelta / panel.Scale);
                        tdelta = Vector2.zero;
                    }
                }
                //水平吸附效应
                else if (one.eport.PortDir.y != 0 && one.sseg.fx == LineDir.hor)
                {
                    var x = one.eport.WPos.x + tdelta.x;
                    if (Mathf.Abs(x - one.FirstPos.x) < 15)
                    {
                        var dx = one.FirstPos.x - one.eport.WPos.x;
                        Vector2 p = tr.position;
                        p += new Vector2(dx, tdelta.y);
                        tr.position = p;
                        one.CollapaseFromEnd(new Vector2(dx / panel.Scale, tdelta.y / panel.Scale));
                        tdelta.y = 0;
                    }
                    else
                    {
                        Vector2 p = tr.position;
                        p += tdelta;
                        tr.position = p;
                        one.CollapaseFromEnd(tdelta / panel.Scale);
                        tdelta = Vector2.zero;
                    }
                }
                else
                {
                    MoveLines(eventData);
                }
            }
            else
            {
                MoveLines(eventData);
            }
        }

        private void MoveLines(PointerEventData eventData)
        {
            tdelta = Vector2.zero;
            Vector2 pos = tr.position;
            pos += eventData.delta;
            tr.position = pos;
            Vector2 uidelta = eventData.delta / panel.Scale;
            foreach (var item in pots)
            {
                foreach (var dl in item.linesOut)
                {
                    dl.CollapaseFromStart(uidelta);
                }

                foreach (var dl in item.linesIn)
                {
                    dl.CollapaseFromEnd(uidelta);
                }
            }
        }

        private void ScaleLines(PointerEventData eventData)
        {
            Vector2 uidelta = eventData.delta / panel.Scale;
            foreach (var item in pots)
            {
                foreach (var dl in item.linesOut)
                {
                    var f = dl.Segments.First;
                    var nxt = f.Next;
                    var ext = nxt.Next;
                    var fp = f.Value.tr.position;
                    fp.x += eventData.delta.x;
                    fp.y += eventData.delta.y;
                    f.Value.tr.position = fp;
                    while (f != null)
                    {
                        var ptr = f.Value.tr;
                        if (f.Value.fx == LineDir.hor)
                        {
                            var sx = ptr.sizeDelta.x * ptr.localScale.x
                                - uidelta.x * f.Value.tempVecForMove.x
                                * f.Value.tempVecForMove.z;
                            DLLine.SetLineW(sx, ptr);
                        }
                        else
                        {
                            var sy = ptr.sizeDelta.y * ptr.localScale.y
                                - uidelta.y * f.Value.tempVecForMove.y
                                * f.Value.tempVecForMove.w;
                            DLLine.SetLineH(sy, ptr);
                        }
                        var n = f.Next;
                        if (n != null)
                        {
                            n.Value.tr.position = f.Value.EndPos;
                        }
                        f = n;
                    }
                }

                foreach (var dl in item.linesIn)
                {
                    var f = dl.Segments.First;
                    while (f != null)
                    {
                        var ptr = f.Value.tr;
                        if (f.Value.fx == LineDir.hor)
                        {
                            var sx = ptr.sizeDelta.x * ptr.localScale.x
                                + uidelta.x * f.Value.tempVecForMove.x
                                * f.Value.tempVecForMove.z;
                            DLLine.SetLineW(sx, ptr);
                        }
                        else
                        {
                            var sy = ptr.sizeDelta.y * ptr.localScale.y
                                + uidelta.y * f.Value.tempVecForMove.y
                                * f.Value.tempVecForMove.w;
                            DLLine.SetLineH(sy, ptr);
                        }
                        var n = f.Next;
                        if (n != null)
                        {
                            n.Value.tr.position = f.Value.EndPos;
                        }
                        f = n;
                    }
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {

        }

        public void OnPointerEnter(PointerEventData eventData)
        {

        }

        public override System.Xml.Linq.XElement ToJson()
        {
            System.Xml.Linq.XElement xe = new System.Xml.Linq.XElement("M");
            xe.SetAttributeValue("name", mname);
            xe.SetAttributeValue("id", comid);
            xe.SetAttributeValue("pos", tr.anchoredPosition.x.ToString() + "," + tr.anchoredPosition.y.ToString());
            foreach (var item in pots)
            {
                var x = item.ToJson();
                if (x != null)
                    xe.Add(x);
            }
            return xe;
        }

        public void ModuleStyleFromXML(System.Xml.Linq.XElement xe)
        {
            var li = pots.ToList();

            for (int i = li.Count - 1; i > 0; i--)
            {
                Destroy(li[i].gameObject);
                li.RemoveAt(i);
            }

            var s = xe.Attribute("size").Value.Split(',');
            tr.sizeDelta = new Vector2(float.Parse(s[0]), float.Parse(s[1]));

            foreach (var item in xe.Elements())
            {
                var p = Instantiate(pots[0], li[0].transform.parent);
                var d = item.Attribute("dir").Value;
                p.portdir = (DLPortDir)int.Parse(d);
                switch (p.portdir)
                {
                    case DLPortDir.up:
                        p.tr.anchorMin = p.tr.anchorMax = new Vector2(0.5f, 1);
                        break;
                    case DLPortDir.down:
                        p.tr.anchorMin = p.tr.anchorMax = new Vector2(0.5f, 0);
                        break;
                    case DLPortDir.left:
                        p.tr.anchorMin = p.tr.anchorMax = new Vector2(0, 0.5f);
                        break;
                    case DLPortDir.right:
                        p.tr.anchorMin = p.tr.anchorMax = new Vector2(1, 0.5f);
                        break;
                    default:
                        break;
                }
                p.tr.pivot = new Vector2(0.5f, 0.5f);
                var pa = item.Attribute("pos").Value.Split(',');
                p.tr.anchoredPosition = new Vector2(float.Parse(pa[0]), float.Parse(pa[1]));
                li.Add(p);
            }

            pots = li.ToArray();
        }
    }
}