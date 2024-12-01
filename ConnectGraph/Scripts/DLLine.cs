using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ConnectGraph
{
    public partial class DLLine : DLComponent, ITrPoolItem
    {
        static Vector2[] templinepos;

        public DLModulePort sport; //起点绑定的port
        public DLModulePort eport; //终点绑定的port
        public DLLineSegment sseg; //起点绑定的线段
        public DLLineSegment eseg; //终点绑定的线段
        public RectTransform jiantou; //末端箭头

        /// <summary>  起始点 </summary>
        public Vector2 FirstPos
        {
            get
            {
                if (link != null)
                {
                    return link.First.Value.tr.position;
                }
                return Vector2.zero;
            }
        }

        /// <summary>  终点 </summary>
        public Vector2 EndPos
        {
            get
            {
                if (link != null)
                {
                    return link.Last.Value.EndPos;
                }
                return Vector2.zero;
            }
        }

        public LinkedList<DLLineSegment> Segments { get => link; }
        public Transform Tr { get; set; }

        private LinkedList<DLLineSegment> link;

        public DLLineSegment cprefab;
        TrPool<DLLineSegment> uiSegpool;
        private DLBif bif;

        protected override void Awake()
        {
            base.Awake();
            uiSegpool = new TrPool<DLLineSegment>(cprefab.transform);
            link = new LinkedList<DLLineSegment>();
            templinepos = new Vector2[99];
        }

        public void Start()
        {

        }

        public void OnEnterPool()
        {
            DisConnectAllPort();
            ClearSegments();
        }

        //和所有外部连接断开关系
        public void DisConnectAllPort()
        {
            if (sport)
            {
                sport.linesOut.Remove(this);
            }
            if (eport)
            {
                eport.linesIn.Remove(this);
            }
            if (sseg)
            {
                sseg.linesOut.Remove(this);
            }
            if (eseg)
            {
                eseg.linesIn.Remove(this);
            }
            foreach (var item in link)
            {
                item.DisConnectAllPort();
            }
        }

        //删除所有线段 但是不改变连接关系 一般用于重绘图形
        public void ClearSegments()
        {
            uiSegpool.ReprefabAll();
            link.Clear();
        }

        //获取所有点位
        public int GetLinePos(Vector2[] li)
        {
            if (link.Count > 0)
            {
                int i = 0;
                foreach (var item in link)
                {
                    li[i] = item.tr.position;
                    i++;
                }
                li[i] = link.Last.Value.tr.position;
                return i + 1;
            }
            return 0;
        }

        /// <summary>  获取线条的点 采用公共数组 无GC </summary> 
        public Vector2[] GetLinePosNonAlloc(out int count)
        {
            count = 0;
            if (link.Count > 0)
            {
                foreach (var item in link)
                {
                    templinepos[count] = item.tr.position;
                    count++;
                }
                templinepos[count] = link.Last.Value.tr.position;
                count++;
            }
            return templinepos;
        }

        //添加线段
        public LinkedListNode<DLLineSegment> AddLineC(Vector2 start, Vector2 end,
            LinkedListNode<DLLineSegment> after = null,
            LinkedListNode<DLLineSegment> before = null)
        {
            var (tr, a) = uiSegpool.Getprefab(transform);
            tr.SetAsLastSibling();
            tr.position = start;
            if (end.x != start.x)
            {
                a.tr.pivot = new Vector2(0, 0.5f);
                var x = end.x - start.x;
                a.tr.sizeDelta = new Vector2(Mathf.Abs(x) / panel.Scale, 2);
                a.fx = LineDir.hor;
                if (x < 0)
                {
                    tr.localScale = new Vector3(-1, 1, 1);
                }
                else
                {
                    tr.localScale = Vector3.one;
                }
            }
            else
            {
                a.tr.pivot = new Vector2(0.5f, 0);
                var y = end.y - start.y;
                a.tr.sizeDelta = new Vector2(2, Mathf.Abs(y) / panel.Scale);
                a.fx = LineDir.ver;
                if (y < 0)
                {
                    tr.localScale = new Vector3(1, -1, 1);
                }
                else
                {
                    tr.localScale = Vector3.one;
                }
            }
            if (after != null)
            {
                return link.AddAfter(after, a);
            }
            else if (before != null)
            {
                return link.AddBefore(before, a);
            }
            else
            {
                return link.AddLast(a);
            }
        }

        public void SetColor(Color yellow)
        {
            foreach (var item in uiSegpool.LivingDic)
            {
                item.Value.SetColor(yellow);
            }
        }

        //可编辑点即将拖拽
        public void UIEditPointBeginDrag(DLLineEditPoint dLLinep, PointerEventData eventData)
        {
            if (dLLinep.linec != null)
            {
                if (dLLinep.fx == DLPointPos.middle) //0头1中2尾
                {

                }
            }
        }

        //可编辑点结束拖拽
        public void UIEditPointEndDrag(DLLineEditPoint dLLinep, PointerEventData eventData)
        {
            if (dLLinep.linec != null)
            {
                if (dLLinep.fx == DLPointPos.middle) //0头1中2尾
                {
                    Debug.Log(link.Count + name);
                    var templine = panel.GetTempLine();
                    //单线变折角
                    if (templine.gameObject.activeSelf)
                    {
                        ZheJiao(dLLinep);
                    }
                    else
                    {
                        //合线检测
                        MergeLine(dLLinep.linec);
                    }
                }
                if (dLLinep.fx == 0)
                {
                    if (dLLinep.linec.Previous == null)
                    {
                        StartPosFindPointAndConnect();
                        if (sport && eport)
                        {
                            panel.ClearEditPoints();
                            LayoutJiantou();
                        }
                        else
                        {
                            DrawEditPoint();
                        }
                    }
                }
                if (dLLinep.fx == DLPointPos.weiba)
                {
                    if (dLLinep.linec.Next == null)
                    {
                        EndPosFindPointAndConnect();
                        if (sport && eport)
                        {
                            panel.ClearEditPoints();
                            LayoutJiantou();
                        }
                        else
                        {
                            DrawEditPoint();
                        }
                    }
                }
            }
        }

        //起点找相近的端子连接并且调整线的显示
        public void StartPosFindPointAndConnect()
        {
            var p1 = FirstPos;
            if (sseg)
            {
                if (sseg.tr.position.x == p1.x && sseg.fx == LineDir.hor) { return; }
                else if (sseg.tr.position.y == p1.y && sseg.fx == LineDir.ver) { return; }
                else
                {
                    sseg.linesOut.Remove(this);
                    Debug.Log($"与{sseg}断开连接");
                    sseg = null;
                    DeleteBif();
                }
            }
            if (sport)
            {
                if (sport.WPos == p1) { return; }
                else
                {
                    Debug.Log($"与{sport}断开连接");
                    sport.linesOut.Remove(this);
                    sport = null;
                }
            }
            var dl = panel.FindConnect(p1, this);
            if (dl is DLModulePort port)
            {
                if (port == eport)
                {
                    return;
                }
                sport = port;
                port.linesOut.Add(this);
                AdjustLineFromStart();
            }
            else if (dl is DLLineSegment seg)
            {
                sseg = seg;
                seg.linesOut.Add(this);
                AdjustLineFromStart();
                DrawBif(true);
            }
        }

        //末端点找相近的端子连接并且调整线的显示
        public void EndPosFindPointAndConnect()
        {
            var p1 = EndPos;
            if (eport != null)
            {
                if (eport.WPos == p1) { return; }
                else
                {
                    Debug.Log($"与{eport}断开连接");
                    eport.linesOut.Remove(this);
                    eport = null;
                }
            }
            if (eseg)
            {
                if (eseg.tr.position.y == p1.y && eseg.fx == LineDir.hor) { return; }
                else if (eseg.tr.position.x == p1.x && eseg.fx == LineDir.ver) { return; }
                else
                {
                    eseg.linesOut.Remove(this);
                    Debug.Log($"与{sseg}断开连接");
                    eseg = null;
                    DeleteBif();
                }
            }

            var dl = panel.FindConnect(p1, this);
            if (dl is DLModulePort port)
            {
                if (port == sport)
                {
                    return;
                }
                eport = port;
                port.linesIn.Add(this);
                AdjustLineFromEnd();
            }
            else if (dl is DLLineSegment seg)
            {
                eseg = seg;
                seg.linesOut.Add(this);
                AdjustLineFromEnd();
                DrawBif(false);
            }
            LayoutJiantou();
        }

        //当尾部连接端子后  优化线条的显示
        private void AdjustLineFromEnd()
        {
            if (eport != null)
            {
                Adjustline2Eport();
            }
            else if (eseg != null)
            {
                if (eseg.fx == LineDir.hor)
                {
                    if (sport)
                    {
                        DrawPort2Segment(sport.WPos, sport.PortDir, eseg, EndPos, this);
                    }
                    else
                    {
                        float delta = sseg.tr.position.y - EndPos.y;
                        SegmentAddLength(link.Last.Value, delta, true);
                    }
                }
                else
                {
                    if (sport)
                    {
                        DrawPort2Segment(sport.WPos, sport.PortDir, eseg, EndPos, this);
                    }
                    else
                    {
                        float delta = sseg.tr.position.x - EndPos.x;
                        SegmentAddLength(link.Last.Value, delta, true);
                    }
                }
            }
        }

        //当首部连接端子后  优化线条的显示 未完成！！！！
        private void AdjustLineFromStart()
        {
            if (sport != null)
            {
                AdjustSport2Line();
            }
            else if (sseg != null)
            {
                if (sseg.fx == LineDir.hor)
                {
                    if (eport)
                    {
                        SegmentUD2Port(sseg, new Vector2(eport.WPos.x, sseg.tr.position.y), this, eport);
                    }
                    else
                    {
                        float delta = sseg.tr.position.y - link.First.Value.StartPos.y;
                        SegmentAddLength(link.First.Value, delta, false);
                    }
                }
                else
                {
                    if (eport)
                    {
                        SegmentLR2Port(sseg, new Vector2(sseg.tr.position.x, eport.WPos.y), this, eport);
                    }
                    else
                    {
                        float delta = sseg.tr.position.x - link.First.Value.StartPos.x;
                        SegmentAddLength(link.First.Value, delta, false);
                    }
                }
            }
        }

        //接线 -> 端子 调整线条的长度和位置
        private void Adjustline2Eport()
        {
            if (link.Count <= 1) return;
            if (EndPos == eport.WPos)
            {
                Debug.Log("线段 -> 端子：不需要调整");
                return;
            }
            Vector3[] wc = new Vector3[4];
            var pdir = eport.PortDir;
            if (pdir.x != 0) //l or r
            {
                //接入线条的方向与受体方向相反 可直线连接
                if (link.Last.Value.fx == LineDir.hor)
                {
                    if (link.Last.Value.tr.localScale.x == -pdir.x)
                    {//-1左 1右
                        var e = eport.tr.position;
                        var ltr = link.Last.Value.tr;
                        if (sport && link.Count == 3)
                        {
                            sport.module.tr.GetWorldCorners(wc);

                            bool flag = false;
                            if (pdir.x < 0 &&
                                (ltr.position.x < wc[1].x &&
                                ltr.position.y > wc[0].y &&
                                ltr.position.y < wc[1].y
                                ))
                            {
                                flag = true;
                                //线段从start模块穿过需要绕开
                            }
                            else if (pdir.x > 0 &&
                                (ltr.position.x > wc[2].x &&
                                ltr.position.y > wc[0].y &&
                                ltr.position.y < wc[1].y
                                ))
                            {
                                flag = true;
                                //线段从start模块穿过需要绕开
                            }

                            if (flag)
                            {
                                var e12 = new Vector2(e.x + 50 * pdir.x, e.y);
                                var e13 = new Vector2(e.x + 50 * pdir.x, wc[2].y + 50);
                                var e14 = new Vector2(sport.tr.position.x + 50 * pdir.x, wc[2].y + 50);
                                var e15 = new Vector2(sport.tr.position.x + 50 * pdir.x, sport.tr.position.y);
                                uiSegpool.Reprefab(link.Last.Value);
                                link.RemoveLast();
                                uiSegpool.Reprefab(link.Last.Value);
                                link.RemoveLast();
                                AddLineC(e15, e14);
                                AddLineC(e14, e13);
                                AddLineC(e13, e12);
                                AddLineC(e12, e);
                                return;
                            }
                        }
                        var e1 = new Vector2(ltr.position.x, e.y);
                        var ptr = link.Last.Previous.Value.tr;
                        var e2 = ptr.position;
                        uiSegpool.Reprefab(link.Last.Value);
                        link.RemoveLast();
                        uiSegpool.Reprefab(link.Last.Value);
                        link.RemoveLast();
                        AddLineC(e2, e1);
                        AddLineC(e1, e);
                    }
                    else
                    {
                        var e = eport.tr.position;
                        var e1 = new Vector2(e.x + 50 * pdir.x, e.y);
                        eport.module.tr.GetWorldCorners(wc);
                        var ptr = link.Last.Previous.Value.tr;
                        var e2 = new Vector2(e.x + 50 * pdir.x, wc[1].y + 50);
                        var e3 = new Vector2(ptr.position.x, wc[1].y + 50);
                        var e4 = ptr.transform.position;
                        uiSegpool.Reprefab(link.Last.Value);
                        link.RemoveLast();
                        uiSegpool.Reprefab(link.Last.Value);
                        link.RemoveLast();
                        AddLineC(e4, e3);
                        AddLineC(e3, e2);
                        AddLineC(e2, e1);
                        AddLineC(e1, e);
                    }
                }

                //末端竖线转横线
                if (link.Last.Value.fx == LineDir.ver)
                {
                    var e = eport.tr.position;
                    var e1 = new Vector2(e.x + 50 * pdir.x, e.y);
                    eport.module.tr.GetWorldCorners(wc);
                    var ptr = link.Last.Previous.Value.tr;
                    var e2 = new Vector2(e.x + 50 * pdir.x, ptr.position.y);
                    var e3 = ptr.transform.position;
                    uiSegpool.Reprefab(link.Last.Value);
                    link.RemoveLast();
                    uiSegpool.Reprefab(link.Last.Value);
                    link.RemoveLast();
                    AddLineC(e3, e2);
                    AddLineC(e2, e1);
                    AddLineC(e1, e);
                }
            }

            if (pdir.y != 0)
            {
                if (link.Last.Value.fx == LineDir.ver)
                {
                    //接入线条的方向与受体方向相反 可直线连接
                    if (link.Last.Value.tr.localScale.y == -pdir.y)
                    {//-1下 1上
                        var e = eport.tr.position;
                        var ltr = link.Last.Value.tr;
                        if (sport && link.Count == 3)
                        {
                            sport.module.tr.GetWorldCorners(wc);

                            bool flag = false;
                            if (pdir.y < 0 &&
                                (ltr.position.y < wc[0].y &&
                                ltr.position.x > wc[1].x &&
                                ltr.position.x < wc[2].x
                                ))
                            {
                                flag = true;
                                //线段从start模块穿过需要绕开
                            }
                            else if (pdir.y > 0 &&
                                (ltr.position.y > wc[1].y &&
                                ltr.position.x > wc[1].x &&
                                ltr.position.x < wc[2].x
                                ))
                            {
                                flag = true;
                                //线段从start模块穿过需要绕开
                            }
                            if (flag)
                            {
                                var e12 = new Vector2(e.x, e.y + 50 * pdir.y);
                                var e13 = new Vector2(wc[2].x + 50, e.y + 50 * pdir.y);
                                var e14 = new Vector2(wc[2].x + 50, sport.tr.position.y + 50 * pdir.y);
                                var e15 = new Vector2(sport.tr.position.x, sport.tr.position.y + 50 * pdir.y);
                                uiSegpool.Reprefab(link.Last.Value);
                                link.RemoveLast();
                                uiSegpool.Reprefab(link.Last.Value);
                                link.RemoveLast();
                                AddLineC(e15, e14);
                                AddLineC(e14, e13);
                                AddLineC(e13, e12);
                                AddLineC(e12, e);
                                return;
                            }
                        }
                        var e1 = new Vector2(e.x, ltr.position.y);
                        var ptr = link.Last.Previous.Value.tr;
                        var e2 = ptr.position;
                        uiSegpool.Reprefab(link.Last.Value);
                        link.RemoveLast();
                        uiSegpool.Reprefab(link.Last.Value);
                        link.RemoveLast();
                        AddLineC(e2, e1);
                        AddLineC(e1, e);
                    }
                    else
                    {
                        var e = eport.tr.position;
                        var e1 = new Vector2(e.x, e.y + 50 * pdir.y);
                        eport.module.tr.GetWorldCorners(wc);
                        var ptr = link.Last.Previous.Value.tr;
                        var e2 = new Vector2(wc[1].x - 50, e.y + 50 * pdir.y);
                        var e3 = new Vector2(wc[1].x - 50, ptr.position.y);
                        var e4 = ptr.transform.position;
                        uiSegpool.Reprefab(link.Last.Value);
                        link.RemoveLast();
                        uiSegpool.Reprefab(link.Last.Value);
                        link.RemoveLast();
                        AddLineC(e4, e3);
                        AddLineC(e3, e2);
                        AddLineC(e2, e1);
                        AddLineC(e1, e);
                    }
                }
                //末端横线转竖线
                if (link.Last.Value.fx == LineDir.hor)
                {
                    var e = eport.tr.position;
                    var e1 = new Vector2(e.x, e.y + 50 * pdir.y);
                    eport.module.tr.GetWorldCorners(wc);
                    var ptr = link.Last.Previous.Value.tr;
                    var e2 = new Vector2(ptr.position.x, e.y + 50 * pdir.y);
                    var e3 = ptr.transform.position;
                    uiSegpool.Reprefab(link.Last.Value);
                    link.RemoveLast();
                    uiSegpool.Reprefab(link.Last.Value);
                    link.RemoveLast();
                    AddLineC(e3, e2);
                    AddLineC(e2, e1);
                    AddLineC(e1, e);
                }
            }
        }

        //端子 -> 接线 调整线条的位置
        private void AdjustSport2Line()
        {
            if (FirstPos == sport.WPos)
            {
                Debug.Log("端子 -> 线段：不需要调整");
                return;
            }
            Vector3[] wc = new Vector3[4];
            var pdir = sport.PortDir;
            if (sport.portdir == DLPortDir.left || sport.portdir == DLPortDir.right)
            {
                //接入线条的方向与受体方向相反 可直线连接
                if (link.First.Value.fx == LineDir.hor)
                {
                    if (link.First.Value.tr.localScale.x == pdir.x)
                    {//-1左 1右
                        var e = sport.tr.position;
                        var ntr = link.First.Next.Value.tr;
                        var e1 = new Vector2(ntr.position.x, e.y);
                        var e2 = link.First.Next.Value.EndPos;
                        uiSegpool.Reprefab(link.First.Value);
                        link.RemoveFirst();
                        uiSegpool.Reprefab(link.First.Value);
                        link.RemoveFirst();
                        AddLineC(e, e1, null, link.First);
                        link.First.Value.tr.name = "0";
                        var b = AddLineC(e1, e2, link.First);
                        b.Value.tr.name = "1";
                    }
                    else
                    {
                        //------------------  以下未完成
                        var e = sport.tr.position;
                        var e1 = new Vector2(e.x + 50 * pdir.x, e.y);
                        sport.module.tr.GetWorldCorners(wc);
                        var ptr = link.Last.Previous.Value.tr;
                        var e2 = new Vector2(e.x + 50 * pdir.x, wc[1].y + 50);
                        var e3 = new Vector2(ptr.position.x, wc[1].y + 50);
                        var e4 = ptr.transform.position;
                        uiSegpool.Reprefab(link.Last.Value);
                        link.RemoveLast();
                        uiSegpool.Reprefab(link.Last.Value);
                        link.RemoveLast();
                        AddLineC(e4, e3);
                        AddLineC(e3, e2);
                        AddLineC(e2, e1);
                        AddLineC(e1, e);
                    }
                }

                //末端竖线转横线
                if (link.First.Value.fx == LineDir.ver)
                {
                    var e = eport.tr.position;
                    var e1 = new Vector2(e.x + 50 * pdir.x, e.y);
                    eport.module.tr.GetWorldCorners(wc);
                    var ptr = link.Last.Previous.Value.tr;
                    var e2 = new Vector2(e.x + 50 * pdir.x, ptr.position.y);
                    var e3 = ptr.transform.position;
                    uiSegpool.Reprefab(link.Last.Value);
                    link.RemoveLast();
                    uiSegpool.Reprefab(link.Last.Value);
                    link.RemoveLast();
                    AddLineC(e3, e2);
                    AddLineC(e2, e1);
                    AddLineC(e1, e);
                }
            }

            if (sport.portdir == DLPortDir.up || sport.portdir == DLPortDir.down)
            {
                if (link.First.Value.fx == LineDir.ver)
                {
                    //接入线条的方向与受体方向相反 可直线连接
                    if (link.First.Value.tr.localScale.y == -pdir.y)
                    {//-1下 1上
                        var e = sport.tr.position;
                        var ltr = link.Last.Value.tr;
                        var e1 = new Vector2(e.x, ltr.position.y);
                        var ptr = link.Last.Previous.Value.tr;
                        var e2 = ptr.position;
                        uiSegpool.Reprefab(link.Last.Value);
                        link.RemoveLast();
                        uiSegpool.Reprefab(link.Last.Value);
                        link.RemoveLast();
                        AddLineC(e2, e1);
                        AddLineC(e1, e);
                    }
                    else
                    {
                        var e = eport.tr.position;
                        var e1 = new Vector2(e.x, e.y + 50 * pdir.y);
                        eport.module.tr.GetWorldCorners(wc);
                        var ptr = link.Last.Previous.Value.tr;
                        var e2 = new Vector2(wc[1].x - 50, e.y + 50 * pdir.y);
                        var e3 = new Vector2(wc[1].x - 50, ptr.position.y);
                        var e4 = ptr.transform.position;
                        uiSegpool.Reprefab(link.Last.Value);
                        link.RemoveLast();
                        uiSegpool.Reprefab(link.Last.Value);
                        link.RemoveLast();
                        AddLineC(e4, e3);
                        AddLineC(e3, e2);
                        AddLineC(e2, e1);
                        AddLineC(e1, e);
                    }
                }
                //末端横线转竖线
                if (link.Last.Value.fx == LineDir.hor)
                {
                    var e = eport.tr.position;
                    var e1 = new Vector2(e.x, e.y + 50 * pdir.y);
                    eport.module.tr.GetWorldCorners(wc);
                    var ptr = link.Last.Previous.Value.tr;
                    var e2 = new Vector2(ptr.position.x, e.y + 50 * pdir.y);
                    var e3 = ptr.transform.position;
                    uiSegpool.Reprefab(link.Last.Value);
                    link.RemoveLast();
                    uiSegpool.Reprefab(link.Last.Value);
                    link.RemoveLast();
                    AddLineC(e3, e2);
                    AddLineC(e2, e1);
                    AddLineC(e1, e);
                }
            }
        }

        //可编辑点拖拽事件
        public void UIEditPointDrag(DLLineEditPoint dLLinep, PointerEventData eventData)
        {
            if (dLLinep.linec == null)
            {
                return;
            }

            if (dLLinep.fx == DLPointPos.middle) //0头1中2尾
            {
                //尾部线段增加
                if (dLLinep.linec.Next == null)
                {
                    ShowHelpLineWB(dLLinep);
                }
                //头部线段增加
                else if (dLLinep.linec.Previous == null)
                {
                    ShowHelpLineHead(dLLinep);
                }
                else
                {
                    MoveLine(dLLinep.linec, eventData);
                    LayoutEditPoint();
                }
            }
            if (dLLinep.fx == 0) // 0头1中2尾
            {
                MoveHead(dLLinep, eventData);
                LayoutEditPoint();
            }
            if (dLLinep.fx == DLPointPos.weiba) // 0头1中2尾
            {
                MoveWeiBa(dLLinep, eventData);
                LayoutEditPoint();
            }
        }

        //合并线段：在拖拽后检测线段与其他线段是不是过近
        private void MergeLine(LinkedListNode<DLLineSegment> dLLinep)
        {
            var tre = dLLinep;
            var pre = dLLinep.Previous;
            var sre = pre?.Previous;
            var nxt = dLLinep.Next;
            var ext = nxt?.Next;

            if (sre != null && pre != null && nxt != null)
            {
                if (tre.Value.fx == LineDir.hor &&
                 pre.Value.tr.sizeDelta.y < 15)
                {
                    SegmentAddLength(sre.Value, tre.Value.tr.sizeDelta.x * tre.Value.tr.localScale.x);
                    SegmentAddLength(nxt.Value, pre.Value.tr.sizeDelta.y * pre.Value.tr.localScale.y, false);

                    uiSegpool.Reprefab(pre.Value);
                    link.Remove(pre);
                    uiSegpool.Reprefab(tre.Value);
                    link.Remove(tre);
                    DrawEditPoint();
                }
                if (tre.Value.fx == LineDir.ver &&
                    pre.Value.tr.sizeDelta.x < 15)
                {
                    SegmentAddLength(sre.Value, tre.Value.tr.sizeDelta.y * tre.Value.tr.localScale.y);
                    SegmentAddLength(nxt.Value, pre.Value.tr.sizeDelta.x * pre.Value.tr.localScale.x, false);

                    uiSegpool.Reprefab(tre.Value);
                    link.Remove(tre);
                    uiSegpool.Reprefab(pre.Value);
                    link.Remove(pre);
                    DrawEditPoint();
                }
            }

            else if (pre != null && nxt != null && ext != null)
            {
                if (tre.Value.fx == LineDir.hor &&
                 nxt.Value.tr.sizeDelta.y < 15)
                {
                    SegmentAddLength(pre.Value, nxt.Value.tr.sizeDelta.y * nxt.Value.tr.localScale.y);
                    SegmentAddLength(ext.Value, tre.Value.tr.sizeDelta.x * tre.Value.tr.localScale.x, false);

                    uiSegpool.Reprefab(tre.Value);
                    link.Remove(tre);
                    uiSegpool.Reprefab(nxt.Value);
                    link.Remove(nxt);
                    DrawEditPoint();
                }
                if (tre.Value.fx == LineDir.ver &&
                    nxt.Value.tr.sizeDelta.x < 15)
                {
                    SegmentAddLength(pre.Value, nxt.Value.tr.sizeDelta.x * nxt.Value.tr.localScale.x);
                    SegmentAddLength(ext.Value, tre.Value.tr.sizeDelta.y * tre.Value.tr.localScale.y, false);

                    uiSegpool.Reprefab(tre.Value);
                    link.Remove(tre);
                    uiSegpool.Reprefab(nxt.Value);
                    link.Remove(nxt);
                    DrawEditPoint();
                }
            }
        }

        //第一根线段拖拽时显示辅助线 新折角
        private void ShowHelpLineHead(DLLineEditPoint dLLinep)
        {
            if (dLLinep.linec.Value.fx == LineDir.hor)
            {
                var tr = dLLinep.linec.Next.Value.tr;
                var line = panel.GetTempLine();
                line.gameObject.SetActive(true);
                line.ClearSegments();
                Vector2 zhuanze = new Vector2(tr.position.x, Input.mousePosition.y);
                var p = dLLinep.linec.Value.GetWPos(0.25f);
                line.AddLineC(p, new Vector2(p.x, zhuanze.y));
                line.AddLineC(new Vector2(p.x, zhuanze.y), zhuanze);
                line.AddLineC(zhuanze, tr.position);
                line.SetColor(Color.blue);
            }
            else
            {
                var tr = dLLinep.linec.Next.Value.tr;
                var line = panel.GetTempLine();
                line.gameObject.SetActive(true);
                line.ClearSegments();
                Vector2 zhuanze = new Vector2(Input.mousePosition.x, tr.position.y);
                var p = dLLinep.linec.Value.GetWPos(0.25f);
                line.AddLineC(p, new Vector2(zhuanze.x, p.y));
                line.AddLineC(new Vector2(zhuanze.x, p.y), zhuanze);
                line.AddLineC(zhuanze, tr.position);
                line.SetColor(Color.blue);
            }
        }

        //最后一根线段拖拽时显示辅助线 新折角
        private void ShowHelpLineWB(DLLineEditPoint dLLinep)
        {
            if (dLLinep.linec.Value.fx == LineDir.hor)
            {
                var tr = dLLinep.linec.Previous.Value.tr;
                var line = panel.GetTempLine();
                line.gameObject.SetActive(true);
                line.ClearSegments();
                Vector2 zhuanze = new Vector2(tr.position.x, Input.mousePosition.y);
                var p = dLLinep.linec.Value.GetWPos(0.75f);
                line.AddLineC(tr.position, zhuanze);
                line.AddLineC(zhuanze, new Vector2(p.x, zhuanze.y));
                line.AddLineC(new Vector2(p.x, zhuanze.y), p);
                line.SetColor(Color.blue);
            }
            else
            {
                var tr = dLLinep.linec.Previous.Value.tr;
                var line = panel.GetTempLine();
                line.gameObject.SetActive(true);
                line.ClearSegments();
                Vector2 zhuanze = new Vector2(Input.mousePosition.x, tr.position.y);
                var p = dLLinep.linec.Value.GetWPos(0.75f);
                line.AddLineC(tr.position, zhuanze);
                line.AddLineC(zhuanze, new Vector2(zhuanze.x, p.y));
                line.AddLineC(new Vector2(zhuanze.x, p.y), p);
                line.SetColor(Color.blue);
            }
        }

        //根据辅助线改折角
        private void ZheJiao(DLLineEditPoint dLLinep)
        {
            var templine = panel.GetTempLine();
            foreach (var item in templine.link)
            {
                if (item.fx == LineDir.hor && item.tr.sizeDelta.x < 30)
                {
                    templine.gameObject.SetActive(false);
                    return;
                }
                if (item.fx == LineDir.ver && item.tr.sizeDelta.y < 30)
                {
                    templine.gameObject.SetActive(false);
                    return;
                }
            }
            if (dLLinep.linec.Next == null)
            {
                //末端裁掉最后两根线，再补4根，变成折线
                var e = link.Last.Value.EndPos;
                uiSegpool.Reprefab(link.Last.Value);
                link.RemoveLast();
                uiSegpool.Reprefab(link.Last.Value);
                link.RemoveLast();
                Debug.Log(templine.link.Count);
                foreach (var item in templine.link)
                {
                    AddLineC(item.tr.position, item.EndPos);
                }
                AddLineC(templine.link.Last.Value.EndPos, e);
            }
            else if (dLLinep.linec.Previous == null)
            {
                var oldf1 = link.First;
                var oldf2 = oldf1.Next;
                //首端裁掉最后两根线，再补4根，变成折线
                var f1 = templine.link.First;
                var f2 = f1.Next;
                var a1 = AddLineC(oldf1.Value.tr.position, f1.Value.tr.position, null, oldf1);
                var a2 = AddLineC(f1.Value.tr.position, f2.Value.tr.position, a1);
                var a3 = AddLineC(f2.Value.tr.position, f2.Value.EndPos, a2);
                AddLineC(f2.Value.EndPos, oldf2.Value.EndPos, a3);

                uiSegpool.Reprefab(oldf1.Value);
                link.Remove(oldf1);
                uiSegpool.Reprefab(oldf2.Value);
                link.Remove(oldf2);
            }
            templine.gameObject.SetActive(false);
            DrawEditPoint();
        }

        //移动最后一个点
        private void MoveWeiBa(DLLineEditPoint dLLinep, PointerEventData eventData)
        {
            var uidelta = eventData.delta / panel.Scale;
            if (dLLinep.linec.Value.fx == LineDir.hor)
            {
                var pre = dLLinep.linec.Previous;
                var tre = dLLinep.linec;
                if (pre != null)
                {
                    SegmentAddLength(pre.Value, uidelta.y);
                }
                SegmentMoveY(tre.Value, uidelta.y);
                SegmentAddLength(tre.Value, uidelta.x, true);

            }
            else
            {
                var pre = dLLinep.linec.Previous;
                var tre = dLLinep.linec;
                if (pre != null)
                {
                    SegmentAddLength(pre.Value, uidelta.x);
                }
                SegmentMoveX(tre.Value, uidelta.x);
                SegmentAddLength(tre.Value, uidelta.y, true);
            }
        }

        //移动任意线段的首部点
        private void MoveHead(DLLineEditPoint dLLinep, PointerEventData eventData)
        {
            var uidelta = eventData.delta / panel.Scale;
            if (dLLinep.linec.Value.fx == LineDir.ver)
            {
                var pre_hor = dLLinep.linec.Previous; //横
                var sre_ver = pre_hor?.Previous; //竖
                var tre_ver = dLLinep.linec; //竖
                var nxt_hor = dLLinep.linec.Next; //横
                bool locky = false;
                bool lockx = false;

                //末端连接固定x轴
                if (nxt_hor == null && eport != null)
                {
                    lockx = true;
                }

                if (pre_hor != null)
                {
                    var ptr = pre_hor.Value.tr;
                    if (!lockx)
                    {
                        SegmentAddLength(pre_hor.Value, uidelta.x);
                    }

                    //起始点被固定 不能移动起始点横线的Y轴
                    if (sre_ver == null && sport != null)
                    {
                        locky = true;
                    }
                    else
                    {
                        SegmentMoveY(pre_hor.Value, uidelta.y);
                    }
                    if (sre_ver != null)
                    {
                        SegmentAddLength(sre_ver.Value, uidelta.y);
                    }
                }

                if (pre_hor != null)
                {
                    SegmentSetWPos(tre_ver.Value, pre_hor.Value.EndPos);
                }
                else
                {
                    SegmentMove(tre_ver.Value, uidelta);
                }

                if (!locky)
                {
                    SegmentAddLength(tre_ver.Value, -uidelta.y);
                }

                if (nxt_hor != null)
                {
                    SegmentAddLength(nxt_hor.Value, -uidelta.x, false);
                }
            }
            else if (dLLinep.linec.Value.fx == LineDir.hor)
            {
                var pre_ver = dLLinep.linec.Previous;
                var sre_hor = pre_ver?.Previous;
                var tre_hor = dLLinep.linec;
                var nex_ver = dLLinep.linec.Next;
                var lockx = false;
                var locky = false;
                //末端连接固定y轴
                if (nex_ver == null && eport != null)
                {
                    locky = true;
                }
                if (pre_ver != null)
                {
                    if (!locky)
                    {
                        SegmentAddLength(pre_ver.Value, uidelta.y);
                    }

                    //起始点被固定 不能移动起始点竖线的X轴
                    if (sre_hor == null && sport != null)
                    {
                        lockx = true;
                    }
                    else
                    {
                        SegmentMoveX(pre_ver.Value, uidelta.x);
                    }

                    if (sre_hor != null)
                    {
                        SegmentAddLength(sre_hor.Value, uidelta.x);
                    }
                }

                if (pre_ver != null)
                {
                    SegmentSetWPos(tre_hor.Value, pre_ver.Value.EndPos);
                }
                else
                {
                    SegmentMove(tre_hor.Value, uidelta);
                }
                if (!lockx)
                {
                    SegmentAddLength(tre_hor.Value, -uidelta.x);
                }

                if (nex_ver != null)
                {
                    SegmentAddLength(nex_ver.Value, -uidelta.y, false);
                }
            }
        }

        //移动任意线段的中部点
        private void MoveLine(LinkedListNode<DLLineSegment> dLLinec, PointerEventData eventData)
        {
            var uidelta = eventData.delta / panel.Scale;
            if (dLLinec.Value.fx == LineDir.ver)
            {
                var pre = dLLinec.Previous;
                if (pre != null)
                {
                    SegmentAddLength(pre.Value, uidelta.x);
                    SegmentSetWPos(dLLinec.Value, pre.Value.EndPos);
                }
                else
                {
                    SegmentMoveX(dLLinec.Value, uidelta.x);
                }

                var nxt = dLLinec.Next;
                if (nxt != null)
                {
                    SegmentAddLength(nxt.Value, -uidelta.x, false);
                }
            }
            else
            {
                var pre = dLLinec.Previous;
                if (pre != null)
                {
                    SegmentAddLength(pre.Value, uidelta.y);
                    SegmentSetWPos(dLLinec.Value, pre.Value.EndPos);
                }
                else
                {
                    SegmentMoveY(dLLinec.Value, uidelta.y);
                }
                var nxt = dLLinec.Next;
                if (nxt != null)
                {
                    SegmentAddLength(nxt.Value, -uidelta.y, false);
                }
            }
        }

        //某一节线段事件
        public void OnPointerEnter(DLLineSegment dLLinec, PointerEventData eventData)
        {
            SetColor(Color.yellow);
        }

        public void OnPointerExit(DLLineSegment dLLinec, PointerEventData eventData)
        {
            SetColor(Color.white);
        }

        //某一节线段被点击后进入编辑模式
        public void OnPointerClick(DLLineSegment dLLinec, PointerEventData eventData)
        {
            DrawEditPoint();
        }

        //从起点移动连线
        public void CollapaseFromStart(Vector2 uidelta)
        {
            var fst = link.First;
            var nxt = fst?.Next;
            var ext = nxt?.Next;
            if (fst.Value.fx == LineDir.hor)
            {
                if (nxt == null)
                {
                    if (eport && sport)
                    {
                        DrawPortConnect(sport, eport);
                    }
                    else
                    {
                        SegmentMove(fst.Value, uidelta);
                    }
                    return;
                }
                if (fst.Value.tr.sizeDelta.x > 150)
                {
                    SegmentMoveY(fst.Value, uidelta.y);
                    SegmentAddLength(fst.Value, -uidelta.x, false);
                    SegmentAddLength(nxt.Value, -uidelta.y, false);
                }
                else
                {
                    SegmentMove(fst.Value, uidelta);
                    if (ext != null)
                    {
                        SegmentMove(nxt.Value, uidelta);
                        SegmentAddLength(nxt.Value, -uidelta.y);
                        SegmentAddLength(ext.Value, -uidelta.x, false);
                    }
                    else
                    {
                        if (eport != null || eseg != null)
                        {
                            //锁轴
                            Debug.Log("锁轴");
                            SegmentAddLength(nxt.Value, -uidelta.y, false);
                            SegmentAddLength(fst.Value, -uidelta.x);
                        }
                        else
                        {
                            SegmentMove(nxt.Value, uidelta);
                        }
                    }
                }
            }
            else if (fst.Value.fx == LineDir.ver)
            {
                if (nxt == null)
                {
                    if (eport && sport)
                    {
                        DrawPortConnect(sport, eport);
                    }
                    else
                    {
                        SegmentMove(fst.Value, uidelta);
                    }
                    return;
                }
                if (fst.Value.tr.sizeDelta.y > 150)
                {
                    SegmentMoveX(fst.Value, uidelta.x);
                    SegmentAddLength(fst.Value, -uidelta.y, false);
                    SegmentAddLength(nxt.Value, -uidelta.x, false);
                }
                else
                {
                    SegmentMove(fst.Value, uidelta);
                    if (ext != null)
                    {
                        SegmentMove(nxt.Value, uidelta);
                        SegmentAddLength(nxt.Value, -uidelta.x, true);
                        SegmentAddLength(ext.Value, -uidelta.y, false);
                    }
                    else
                    {
                        if (eport != null)
                        {
                            //锁轴
                            Debug.Log("锁轴");
                            SegmentAddLength(nxt.Value, -uidelta.x, false);
                            SegmentAddLength(fst.Value, -uidelta.y);
                        }
                        else
                        {
                            SegmentMove(fst.Value, uidelta);
                        }
                    }
                }
            }

            LayoutJiantou();
            LayoutBif();
        }

        //从末端移动连线
        public void CollapaseFromEnd(Vector2 uidelta)
        {
            var fst = link.Last;
            var pre = fst?.Previous;
            var sre = pre?.Previous;
            if (fst.Value.fx == LineDir.hor)
            {
                if (pre == null) //单根线
                {
                    if (eport && sport)
                    {
                        DrawPortConnect(sport, eport);
                    }
                    else
                    {
                        SegmentMove(fst.Value, uidelta);
                    }
                    return;
                }
                else
                {
                    if (fst.Value.UILength > 100)
                    {
                        SegmentMoveY(fst.Value, uidelta.y);
                        SegmentAddLength(fst.Value, uidelta.x, true);
                        SegmentAddLength(pre.Value, uidelta.y, true);
                    }
                    else
                    {
                        if (sre == null)
                        {
                            SegmentMoveY(fst.Value, uidelta.y);
                            SegmentAddLength(fst.Value, uidelta.x, true);
                            SegmentAddLength(pre.Value, uidelta.y, true);
                        }
                        else
                        {
                            SegmentMove(fst.Value, uidelta);
                            SegmentMoveX(pre.Value, uidelta.x);
                            SegmentAddLength(pre.Value, uidelta.y, true);
                            SegmentAddLength(sre.Value, uidelta.x, true);
                        }
                    }
                }
            }
            else
            {
                if (pre == null) //单根线
                {
                    if (eport && sport)
                    {
                        DrawPortConnect(sport, eport);
                    }
                    else
                    {
                        SegmentMove(fst.Value, uidelta);
                    }
                    return;
                }
                else
                {
                    if (fst.Value.UILength > 100)
                    {
                        SegmentMoveX(fst.Value, uidelta.x);
                        SegmentAddLength(fst.Value, uidelta.y, true);
                        SegmentAddLength(pre.Value, uidelta.x, true);
                    }
                    else
                    {
                        if (sre == null)
                        {
                            SegmentMoveY(fst.Value, uidelta.y);
                            SegmentAddLength(fst.Value, uidelta.x, true);
                            SegmentAddLength(pre.Value, uidelta.y, true);
                        }
                        else
                        {
                            SegmentMove(fst.Value, uidelta);
                            SegmentMoveY(pre.Value, uidelta.y);
                            SegmentAddLength(pre.Value, uidelta.x, true);
                            SegmentAddLength(sre.Value, uidelta.y, true);
                        }
                    }
                }
            }

            LayoutJiantou();
            LayoutBif();
        }

        /// <summary> 新建线段（保留连接关系）</summary>
        public void DrawLines(params Vector2[] allwpos)
        {
            ClearSegments();
            if (allwpos.Length <= 1) return;
            for (int i = 0; i < allwpos.Length - 1; i++)
            {
                AddLineC(allwpos[i], allwpos[i + 1]);
            }
            if (bif != null)
                bif.position = allwpos[0];
            LayoutJiantou();
            LayoutBif();
        }

        //重新布局编辑点（不清除）
        private void LayoutEditPoint()
        {
            foreach (var item in panel.poolp.LivingDic.Values)
            {
                if (item.fx == 0)
                {
                    item.tr.position = item.linec.Value.tr.position;
                }
                else if (item.fx == DLPointPos.middle)
                {
                    item.tr.position = item.linec.Value.MiddlePos;
                }
                else if (item.fx == DLPointPos.weiba)
                {
                    item.tr.position = item.linec.Value.EndPos;
                }
            }
            LayoutJiantou();
            LayoutBif();
        }

        //重新布局分叉点（不清除）
        private void LayoutBif()
        {
            if (bif != null)
            {
                bif.position = bif.isstart ? FirstPos : EndPos;
            }
        }

        //布局终点箭头
        public void LayoutJiantou()
        {
            jiantou.position = link.Last.Value.EndPos;
            var e = jiantou.localEulerAngles;
            switch (link.Last.Value.fx)
            {
                case LineDir.hor:
                    if (link.Last.Value.tr.localScale.x > 0)
                    {
                        e.z = 90;
                        jiantou.localEulerAngles = e;
                    }
                    else
                    {
                        e.z = 270;
                        jiantou.localEulerAngles = e;
                    }
                    break;
                case LineDir.ver:
                    if (link.Last.Value.tr.localScale.y > 0)
                    {
                        e.z = 180;
                        jiantou.localEulerAngles = e;
                    }
                    else
                    {
                        e.z = 0;
                        jiantou.localEulerAngles = e;
                    }
                    break;
                default:
                    break;
            }
        }

        //进入编辑模式
        public void DrawEditPoint()
        {
            panel.EditLine(this);
            //panel.ClearLinep();
            //var f = link.First;
            //while (f != null)
            //{
            //    var (tr, point) = panel.poolp.Getprefab(transform);
            //    tr.position = f.Value.tr.position;
            //    point.linec = f;
            //    point.fx = 0;
            //    point.master = this;

            //    (tr, point) = panel.poolp.Getprefab(transform);
            //    tr.position = f.Value.MiddlePos;
            //    point.linec = f;
            //    point.fx = DLPointPos.middle;
            //    point.master = this;

            //    if (f.Next == null)
            //    {
            //        (tr, point) = panel.poolp.Getprefab(transform);
            //        tr.position = f.Value.EndPos;
            //        point.linec = f;
            //        point.fx = DLPointPos.weiba;
            //        point.master = this;
            //    }
            //    f = f.Next;
            //}
            LayoutJiantou();
        }

        //绘制分叉点
        public void DrawBif(bool isstart)
        {
            if (bif != null)
            {
                bif.position = isstart ? FirstPos : EndPos;
                return;
            }
            var b = panel.CreatBif();
            b.Tr.SetParent(tr);
            b.Tr.SetSiblingIndex(0);
            b.position = isstart ? FirstPos : EndPos;
            bif = b;
            bif.isstart = isstart;
        }

        //清除分叉点
        public void DeleteBif()
        {
            if (bif != null)
            {
                panel.poolbif.Reprefab(bif);
                bif = null;
            }
        }

        //计算各线段的权重 实现放大 缩小
        public void CalTempWeightForMove()
        {
            Vector2 dir = EndPos - FirstPos;
            var bdx = Mathf.Sign(dir.x);
            var bdy = Mathf.Sign(dir.y);
            dir.x = Mathf.Abs(dir.x);
            dir.y = Mathf.Abs(dir.y);

            foreach (var item in link)
            {
                var w = item.tr.sizeDelta.x * panel.Scale / dir.x;
                var h = item.tr.sizeDelta.y * panel.Scale / dir.y;
                item.tempVecForMove.x = w;
                item.tempVecForMove.y = h;
                item.tempVecForMove.z = item.tr.localScale.x * bdx;
                item.tempVecForMove.w = item.tr.localScale.y * bdy;
            }
        }

        public override System.Xml.Linq.XElement ToJson()
        {
            System.Xml.Linq.XElement xe = new System.Xml.Linq.XElement("L");
            xe.SetAttributeValue("id", comid);
            if (sport)
            {
                xe.SetAttributeValue("sp", sport.module.comid + "," + sport.index);
            }
            if (eport)
            {
                xe.SetAttributeValue("ep", eport.module.comid + "," + eport.index);
            }
            if (sseg)
            {
                xe.SetAttributeValue("sg", sseg.master.comid + "," + sseg.index);
            }
            int i = 0;
            foreach (var item in link)
            {
                item.index = i;
                var x = item.ToJson();
                xe.Add(x);
                i++;
            }
            return xe;
        }
    }
}