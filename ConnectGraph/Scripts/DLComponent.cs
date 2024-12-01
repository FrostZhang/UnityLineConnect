using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ConnectGraph
{
    public class DLComponent : MonoBehaviour
    {
        [HideInInspector] public RectTransform tr;
        protected DLPanel panel;
        public string comid;

        protected virtual void Awake()
        {
            tr = transform as RectTransform;
            if (panel == null)
            {
                panel = GetComponentInParent<DLPanel>();
            }
        }


        public virtual System.Xml.Linq.XElement ToJson()
        {
            return null;
        }
    }
}