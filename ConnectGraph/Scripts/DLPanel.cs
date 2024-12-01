using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ConnectGraph
{
    public class DLPanel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, 
        IPointerEnterHandler,IPointerClickHandler,  IScrollHandler
    {
        //线
        [SerializeField] public DLLine linepreafab;
        //编辑点
        [SerializeField] DLLineEditPoint editpointprefab;
        //分叉点
        [SerializeField] RectTransform bifprefab;
        //模块
        [SerializeField] DLModule modprefab;

        public List<DLModule> allModules;
        public TrPool<DLLineEditPoint> poolp;
        public TrPool<DLLine> poolLine;
        public TrPool<DLBif> poolbif;

        public float Scale => transform.localScale.x;
        public float scaleMin;
        public float scaleMax;

        public bool portIsDrag;
        DLLine templine;
        protected RectTransform tr;

        DLComponent focus;

        int newid => tempid++;
        int tempid;
        protected virtual void Awake()
        {
            poolp = new TrPool<DLLineEditPoint>(editpointprefab.transform);
            poolLine = new TrPool<DLLine>(linepreafab.transform);
            poolbif = new TrPool<DLBif>(bifprefab);
            templine = CreatLine("templine"); //全局临时线条
            tempid = 0;
            allModules = new List<DLModule>(GetComponentsInChildren<DLModule>());
            for (int i = 0; i < allModules.Count; i++)
            {
                allModules[i].SetJsonID(newid);
            }
            GetTempLine().gameObject.SetActive(false);
            tr = transform as RectTransform;
        }

        protected virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                if (focus)
                {
                    if (focus is DLLine line)
                    {
                        poolLine.Reprefab(line);
                        ClearEditPoints();
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                Debug.Log(Panel2Json());
            }
        }

        public void ClearEditPoints()
        {
            poolp.ReprefabAll();
            poolp.Clear();
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

        public void OnBeginDrag(PointerEventData eventData)
        {

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

        public void OnEndDrag(PointerEventData eventData)
        {

        }

        public void OnPointerEnter(PointerEventData eventData)
        {

        }

        public DLBif CreatBif()
        {
            var (tr, bif) = poolbif.Getprefab();
            return bif;
        }

        public DLLine CreatLine(string rname = null)
        {
            var (tr, d) = poolLine.Getprefab(linepreafab.transform.parent);
            d.comid = newid.ToString();
            if (rname != null)
            {
                d.name = rname;
            }
            else
            {
                d.name = d.comid;
            }
            return d;
        }

        public DLLine GetTempLine()
        {
            return templine;
        }

        public DLComponent FindConnect(Vector2 wpos, DLComponent ignore)
        {
            foreach (var md in allModules)
            {
                foreach (var port in md.pots)
                {
                    if (port == ignore)
                    {
                        continue;
                    }
                    var res = RectTransformUtility.RectangleContainsScreenPoint(port.tr, wpos);
                    if (res)
                    {
                        return port;
                    }
                }
                foreach (var seg in md.segs)
                {
                    if (seg == ignore)
                    {
                        continue;
                    }
                    if (seg.Contains(wpos))
                    {
                        return seg;
                    }
                }
            }

            foreach (var item in poolLine.LivingDic.Values)
            {
                if (item == templine || item == ignore)
                {
                    continue;
                }
                foreach (var seg in item.Segments)
                {
                    if (seg.Contains(wpos))
                    {
                        Debug.Log(seg);
                        return seg;
                    }
                }
            }
            return null;
        }

        public DLModulePort FindPort(Vector2 wpos)
        {
            foreach (var md in allModules)
            {
                foreach (var port in md.pots)
                {
                    var res = RectTransformUtility.RectangleContainsScreenPoint(port.tr, wpos);
                    if (res)
                    {
                        return port;
                    }
                }
            }
            return null;
        }

        public DLLineSegment FindSegment(Vector2 wpos, DLLine ignore)
        {
            foreach (var item in poolLine.LivingDic.Values)
            {
                if (item == templine || item == ignore)
                {
                    continue;
                }
                foreach (var seg in item.Segments)
                {
                    if (seg.Contains(wpos))
                    {
                        Debug.Log(seg);
                        return seg;
                    }
                }
            }
            return null;
        }

        public void EditLine(DLLine line)
        {
            ClearEditPoints();
            focus = line;
            var f = line.Segments.First;
            while (f != null)
            {
                var (tr, point) = poolp.Getprefab(transform);
                tr.position = f.Value.tr.position;
                point.linec = f;
                point.fx = 0;
                point.master = line;

                (tr, point) = poolp.Getprefab(transform);
                tr.position = f.Value.MiddlePos;
                point.linec = f;
                point.fx = DLPointPos.middle;
                point.master = line;

                if (f.Next == null)
                {
                    (tr, point) = poolp.Getprefab(transform);
                    tr.position = f.Value.EndPos;
                    point.linec = f;
                    point.fx = DLPointPos.weiba;
                    point.master = line;
                }
                f = f.Next;
            }
        }

        public System.Xml.Linq.XDocument Panel2Json()
        {
            System.Xml.Linq.XElement xe = new System.Xml.Linq.XElement("S");
            xe.SetAttributeValue("maxid", tempid);
            System.Xml.Linq.XElement xm = new System.Xml.Linq.XElement("Modules");
            foreach (var item in allModules)
            {
                var m = item.ToJson();
                xm.Add(m);
            }
            xe.Add(xm);

            System.Xml.Linq.XElement xl = new System.Xml.Linq.XElement("Lines");
            var lines = poolLine.LivingDic.Values;
            foreach (var item in lines)
            {
                if (item.comid == "0")
                    continue;
                var x = item.ToJson();
                xl.Add(x);
            }
            xe.Add(xl);

            return new System.Xml.Linq.XDocument(xe);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ClearEditPoints();
        }
    }

}
