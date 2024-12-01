using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ConnectGraph
{
    public class DLEditPanel : MonoBehaviour
    {
        private RectTransform tr;
        public InputField cname;
        public Button addpoint, output;
        public DLEditModule module;
        public DLEditModulePort portprefab;
        public InputField kd, gd;
        public EditPortPanel epanel;
        public float Scale => transform.localScale.x;

        public float scaleMin;
        public float scaleMax;
        protected void Awake()
        {
            tr = transform as RectTransform;
        }

        private void Start()
        {
            gd.text = module.tr.sizeDelta.x.ToString();
            kd.text = module.tr.sizeDelta.y.ToString();

            gd.onEndEdit.AddListener((x) =>
            {
                if (string.IsNullOrEmpty(x))
                {
                    gd.text = module.tr.sizeDelta.y.ToString();
                    return;
                }
                var p = int.Parse(x);
                var s = module.tr.sizeDelta;
                s.y = p;
                module.tr.sizeDelta = s;
            });

            kd.onEndEdit.AddListener((x) =>
            {
                if (string.IsNullOrEmpty(x))
                {
                    kd.text = module.tr.sizeDelta.x.ToString();
                    return;
                }
                var p = int.Parse(x);
                var s = module.tr.sizeDelta;
                s.x = p;
                module.tr.sizeDelta = s;
            });

            addpoint.onClick.AddListener(() =>
            {
                module.CreatPoint();
            });

            output.onClick.AddListener(() =>
            {
                Debug.Log(module.ModuleStyleToXML());
            });

            if (string.IsNullOrEmpty(module.mname))
            {
                module.mname = "Ä£¿é" + Random.Range(100, 500);
            }
            cname.text = module.mname;
            cname.onEndEdit.AddListener((x) =>
            {
                module.mname = x;
            });
        }
    }
}