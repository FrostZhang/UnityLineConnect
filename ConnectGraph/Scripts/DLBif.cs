using UnityEngine;

namespace ConnectGraph
{
    public class DLBif : ITrPoolItem
    {
        public Vector2 position
        {
            get { return Tr.position; }
            set { Tr.position = value; }
        }
        public bool isstart;

        public Transform Tr { get; set; }

        public void OnEnterPool()
        {

        }

        public void Start()
        {

        }

    }

}
