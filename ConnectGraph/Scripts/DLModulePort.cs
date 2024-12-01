using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace ConnectGraph
{
    public enum DLPortDir
    {
        up, down, left, right
    }

    public class DLModulePort : DLComponent,
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
        public DLPortDir portdir;
        public DLModule module;
        public int index; //init from module

        public HashSet<DLLine> linesOut;
        public HashSet<DLLine> linesIn;
        public int ndis = 50; //引出线默认长度
        DLLine temp;

        public Vector2 WPos => tr.position;

        protected override void Awake()
        {
            base.Awake();
            linesOut = new HashSet<DLLine>();
            linesIn = new HashSet<DLLine>();
            if (string.IsNullOrEmpty(pname))
            {
                pname = name;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            panel.ClearEditPoints();
            panel.portIsDrag = true;
            temp = panel.CreatLine(pname + "-");
        }

        public void OnDrag(PointerEventData eventData)
        {
            DrawLine();
        }

        public void SetJsonID(string newid)
        {
            comid = newid;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            panel.portIsDrag = false;
            temp.sport = this;
            temp.EndPosFindPointAndConnect();

            foreach (var item in linesOut)
            {
                if (temp.eport && item.eport == temp.eport)
                {
                    Debug.LogError($"{pname}-{temp.eport.pname}重复连接！！！");
                    panel.poolLine.Reprefab(temp);
                    temp = null;
                    return;
                }
                if (temp.eseg && item.eseg == temp.eseg)
                {
                    Debug.LogError($"{pname}-{temp.eseg}重复连接！！！");
                    panel.poolLine.Reprefab(temp);
                    temp = null;
                    return;
                }
            }

            linesOut.Add(temp);
            temp = null;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {

        }

        public void DrawLine()
        {
            var s = WPos;
            var e = Input.mousePosition;
            temp.DrawPort2Auto(PortDir, s, e, this);
        }

        public override System.Xml.Linq.XElement ToJson()
        {
            if (linesOut.Count > 0 || linesIn.Count > 0)
            {

            }
            else
            {
                return null;
            }
            System.Xml.Linq.XElement xe = new System.Xml.Linq.XElement("P");
            xe.SetAttributeValue("index", index);
            xe.SetAttributeValue("n", pname);
            if (linesOut.Count > 0)
            {
                var s = string.Empty;
                foreach (var item in linesOut)
                    s += item.comid + ",";
                s = s.TrimEnd(',');
                xe.SetAttributeValue("Out", s);
            }

            if (linesIn.Count > 0)
            {
                var s = string.Empty;
                foreach (var item in linesIn)
                    s += item.comid + ",";
                s = s.TrimEnd(',');
                xe.SetAttributeValue("In", s);
            }

            return xe;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log(1);
        }
    }
}