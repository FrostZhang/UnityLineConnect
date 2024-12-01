using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ConnectGraph
{
    public class EditPortPanel : MonoBehaviour
    {
        public InputField cname, pos, fx;
        public Button delete;
        DLEditModulePort port;
        void Start()
        {
            cname.onEndEdit.AddListener((x) =>
            {
                port.pname = x;
            });
            delete.onClick.AddListener(() =>
            {
                Destroy(port.gameObject);
                port = null;
            });
        }

        void Update()
        {
            if (port)
            {
                switch (port.portdir)
                {
                    case DLPortDir.up:
                        fx.text = "ÉÏ";
                        pos.text = port.tr.anchoredPosition.x.ToString();
                        break;
                    case DLPortDir.down:
                        fx.text = "ÏÂ";
                        pos.text = port.tr.anchoredPosition.x.ToString();
                        break;
                    case DLPortDir.left:
                        fx.text = "×ó";
                        pos.text = port.tr.anchoredPosition.y.ToString();
                        break;
                    case DLPortDir.right:
                        fx.text = "ÓÒ";
                        pos.text = port.tr.anchoredPosition.y.ToString();
                        break;
                    default:
                        fx.text = "Î´Öª";
                        break;
                }
            }
        }

        public void Bingding(DLEditModulePort port)
        {
            this.port = port;
            cname.text = port.pname;
        }
    }
}