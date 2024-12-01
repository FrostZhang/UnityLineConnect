using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ConnectGraph
{
    public class DLEditModule : MonoBehaviour
    {
        public string mname;
        public DLEditModulePort portprefab;
        public List<DLEditModulePort> pots;
        public RectTransform tr;

        private void Awake()
        {
            tr = transform as RectTransform;

        }

        private void Start()
        {
            portprefab = GetComponentInChildren<DLEditModulePort>();
            portprefab.gameObject.SetActive(false);
        }

        public void ModuleStyleFromXML(System.Xml.Linq.XElement xe)
        {
            var li = pots;

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
        }

        public void CreatPoint()
        {
            if (pots.Count > 0 && pots.Last().tr.anchoredPosition.x == 0)
            {
                //防止上一个没拖走又新生成一个
                return;
            }
            var p = Instantiate(portprefab, portprefab.tr.parent);
            p.pname = "port";
            p.gameObject.SetActive(true);
            pots.Add(p);
        }

        public System.Xml.Linq.XElement ModuleStyleToXML()
        {
            var xe = new System.Xml.Linq.XElement(mname);
            xe.SetAttributeValue("size", tr.sizeDelta.x + "," + tr.sizeDelta.y);

            foreach (var item in pots)
            {
                var pe = new System.Xml.Linq.XElement(item.pname);
                pe.SetAttributeValue("pos", item.tr.anchoredPosition.x + "," + item.tr.anchoredPosition.y);
                pe.SetAttributeValue("dir", (int)item.portdir);
                xe.Add(pe);
            }

            return xe;
        }
    }
}