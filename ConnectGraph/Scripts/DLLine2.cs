using UnityEngine;

namespace ConnectGraph
{
    public partial class DLLine
    {
        public static void SetLineH(float sy, RectTransform n)
        {
            n.sizeDelta = new Vector2(2, Mathf.Abs(sy));
            n.localScale = new Vector3(1, Mathf.Sign(sy), 1);
        }

        public static void SetLineW(float sx, RectTransform n)
        {
            n.sizeDelta = new Vector2(Mathf.Abs(sx), 2);
            n.localScale = new Vector3(Mathf.Sign(sx), 1, 1);
        }

        public static void SegmentMoveX(DLLineSegment segment, float uidelta)
        {
            var p = segment.tr.anchoredPosition;
            p.x += uidelta;
            segment.tr.anchoredPosition = p;

            segment.OnPosMove(new Vector2(uidelta, 0));
        }

        public static void SegmentMoveY(DLLineSegment segment, float uidelta)
        {
            var p = segment.tr.anchoredPosition;
            p.y += uidelta;
            segment.tr.anchoredPosition = p;
            segment.OnPosMove(new Vector2(0, uidelta));
        }

        public static void SegmentMove(DLLineSegment segment, Vector2 uidelta)
        {
            var p = segment.tr.anchoredPosition;
            p += uidelta;
            segment.tr.anchoredPosition = p;
            segment.OnPosMove(uidelta);
        }

        public static void SegmentSetWPos(DLLineSegment segment, Vector2 worldpos)
        {
            var oldp = segment.tr.anchoredPosition;
            segment.tr.position = worldpos;
            var uidelta = segment.tr.anchoredPosition - oldp;
            segment.OnPosMove(uidelta);
        }

        public static void SegmentAddLength(DLLineSegment segment, float uidelta, bool lockpos = true)
        {
            if (uidelta == 0) return;
            var str = segment.tr;
            switch (segment.fx)
            {
                case LineDir.hor:
                    var sx = str.sizeDelta.x * str.localScale.x + uidelta;
                    if (!lockpos)
                    {
                        var p = str.anchoredPosition;
                        p.x -= uidelta;
                        str.anchoredPosition = p;
                    }
                    SetLineW(sx, str);
                    break;
                case LineDir.ver:
                    var sy = str.sizeDelta.y * str.localScale.y + uidelta;
                    if (!lockpos)
                    {
                        var p = str.anchoredPosition;
                        p.y -= uidelta;
                        str.anchoredPosition = p;
                    }
                    SetLineH(sy, str);
                    break;
                default:
                    break;
            }
            segment.OnSegSizeChanged(uidelta, lockpos);
        }

        public static void SegmentUD2Port(DLLineSegment segment, Vector2 wstartP, DLLine temp, DLModulePort p)
        {
            Vector2 endP = p.WPos;
            float vdir = Mathf.Sign(endP.y - wstartP.y);
            Vector2 z = Vector2.zero;
            endP = p.WPos;
            if (p.PortDir.y == -vdir) //up->down down->up 
            {
                //拉成一条直线
                if (Mathf.Abs(endP.x - wstartP.x) < 15 && endP.x > segment.StartPos.x && endP.x < segment.EndPos.x)
                {
                    temp.DrawLines(new Vector2(endP.x, wstartP.y), endP);
                }
                else
                {
                    //闪电
                    z = new Vector2(wstartP.x, wstartP.y + 50 * vdir);
                    temp.DrawLines(wstartP, z, new Vector2(endP.x, z.y), endP);
                }
            }
            else if (p.PortDir.y == vdir) //up->up down->down 
            {
                float y = vdir > 0 ? Mathf.Max(endP.y, wstartP.y) : Mathf.Min(endP.y, wstartP.y);
                z = new Vector2(wstartP.x, y + 50 * vdir);
                temp.DrawLines(wstartP, z, new Vector2(endP.x, z.y), endP);
            }
            else if (p.PortDir.x == vdir)//up->right down->left
            {
                if ((endP.x < wstartP.x && vdir > 0) ||
                    (endP.x > wstartP.x && vdir < 0))
                {
                    z = new Vector2(wstartP.x, endP.y);
                    temp.DrawLines(wstartP, z, endP);
                }
                else
                {
                    z = new Vector2(endP.x + 50 * vdir, wstartP.y + 50 * vdir);
                    temp.DrawLines(wstartP, new Vector2(wstartP.x, z.y),
                        z, new Vector2(z.x, endP.y), endP);
                }
            }
            else //up->left down->right
            {
                if ((endP.x > wstartP.x && vdir > 0) ||
                        (endP.x < wstartP.x && vdir < 0))
                {
                    z = new Vector2(wstartP.x, endP.y);
                    temp.DrawLines(wstartP, z, endP);
                }
                else
                {
                    z = new Vector2(endP.x + 50 * -vdir, wstartP.y + 50 * vdir);
                    temp.DrawLines(wstartP, new Vector2(wstartP.x, z.y),
                        z, new Vector2(z.x, endP.y), endP);
                }
            }
        }

        public static void SegmentLR2Port(DLLineSegment segment, Vector2 wstartP, DLLine temp, DLModulePort p)
        {
            Vector2 endP = p.WPos;
            float vdir = Mathf.Sign(endP.y - wstartP.y);
            Vector2 z = Vector2.zero;

            endP = p.WPos;
            if (p.PortDir.x == -vdir) //l->r r->l
            {
                if (Mathf.Abs(wstartP.y - endP.y) < 15 && endP.y > segment.StartPos.x && endP.y < segment.EndPos.y)
                {
                    temp.DrawLines(new Vector2(wstartP.x, endP.y), endP);
                }
                else
                {
                    z = new Vector2(wstartP.x + 50 * vdir, endP.y);
                    temp.DrawLines(wstartP, new Vector2(z.x, wstartP.y), z, endP);
                }
            }
            else if (p.PortDir.x == vdir)//l->l r->r
            {
                float x = vdir > 0 ? Mathf.Max(endP.x, wstartP.x) : Mathf.Min(endP.x, wstartP.x);
                z = new Vector2(x + 50 * vdir, wstartP.y);
                temp.DrawLines(wstartP, z, new Vector2(z.x, endP.y), endP);
            }
            else if (p.PortDir.y == vdir)//r->up l->down
            {
                if ((endP.y < wstartP.y && vdir > 0) ||
                        (endP.y > wstartP.y && vdir < 0))
                {
                    z = new Vector2(endP.x, wstartP.y);
                    temp.DrawLines(wstartP, z, endP);
                }
                else
                {
                    z = new Vector2(wstartP.x + 50 * vdir, endP.y + 50 * vdir);
                    temp.DrawLines(wstartP, new Vector2(z.x, wstartP.y),
                        z, new Vector2(endP.x, z.y), endP);
                }
            }
            else //r->down l->up
            {
                if ((endP.y > wstartP.y && vdir > 0) ||
                    (endP.y < wstartP.y && vdir < 0))
                {
                    z = new Vector2(endP.x, wstartP.y);
                    temp.DrawLines(wstartP, z, endP);
                }
                else
                {
                    z = new Vector2(wstartP.x + 50 * vdir, endP.y + 50 * -vdir);
                    temp.DrawLines(wstartP, new Vector2(z.x, wstartP.y),
                        z, new Vector2(endP.x, z.y), endP);
                }
            }
        }

        public static void DrawPort2Segment(Vector2 start, Vector2 sdir,
            DLLineSegment target, Vector2 wendpos, DLLine line)
        {
            Vector2 z = Vector2.zero;
            if (sdir.x != 0)
            {
                if (target.fx == LineDir.ver)
                {
                    wendpos.x = target.tr.position.x;
                    if (Mathf.Abs(start.y - wendpos.y) < 15 && wendpos.y > target.StartPos.y && wendpos.y < target.EndPos.y)
                    {
                        line.DrawLines(start, new Vector2(wendpos.x, start.y));
                    }
                    else
                    {
                        z = new Vector2(start.x + 50 * sdir.x, wendpos.y);
                        line.DrawLines(start, new Vector2(z.x, start.y), z, wendpos);
                    }
                }
                else
                {
                    wendpos.y = target.tr.position.y;
                    if (sdir.x > 0)
                    {
                        if (wendpos.y < start.y)
                        {
                            DrawR2Port(line, start, wendpos, DLPortDir.up);
                        }
                        else
                        {
                            DrawR2Port(line, start, wendpos, DLPortDir.down);
                        }
                    }
                    else if (sdir.x < 0)
                    {
                        if (wendpos.y < start.y)
                        {
                            DrawL2Port(line, start, wendpos, DLPortDir.up);
                        }
                        else
                        {
                            DrawL2Port(line, start, wendpos, DLPortDir.down);
                        }
                    }
                }
            }
            else if (sdir.y != 0)
            {
                if (target.fx == LineDir.hor)
                {
                    wendpos.y = target.tr.position.y;
                    if (Mathf.Abs(start.x - wendpos.x) < 15 && start.x > target.StartPos.x && start.x < target.EndPos.x)
                    {
                        line.DrawLines(start, new Vector2(start.x, wendpos.y));
                    }
                    else
                    {
                        z = new Vector2(wendpos.x, start.y + 50 * sdir.y);
                        line.DrawLines(start, new Vector2(start.x, z.y), z, wendpos);
                    }
                }
                else
                {
                    wendpos.x = target.tr.position.x;
                    if (sdir.y > 0)
                    {
                        if (wendpos.x < start.x)
                        {
                            DrawU2Port(line, start, wendpos, DLPortDir.right);
                        }
                        else
                        {
                            DrawU2Port(line, start, wendpos, DLPortDir.left);
                        }
                    }
                    else if (sdir.y < 0)
                    {
                        if (wendpos.x < start.x)
                        {
                            DrawD2Port(line, start, wendpos, DLPortDir.right);
                        }
                        else
                        {
                            DrawD2Port(line, start, wendpos, DLPortDir.left);
                        }
                    }
                }
            }

        }

        public void DrawPortConnect(DLModulePort sport, DLModulePort eport)
        {
            switch (sport.portdir)
            {
                case DLPortDir.up:
                    DrawU2Port(this, sport.WPos, eport.WPos, eport.portdir);
                    break;
                case DLPortDir.down:
                    DrawD2Port(this, sport.WPos, eport.WPos, eport.portdir);
                    break;
                case DLPortDir.left:
                    DrawL2Port(this, sport.WPos, eport.WPos, eport.portdir);
                    break;
                case DLPortDir.right:
                    DrawR2Port(this, sport.WPos, eport.WPos, eport.portdir);
                    break;
            }
        }

        public static void DrawR2Port(DLLine line, Vector2 s, Vector2 e, DLPortDir enddir)
        {
            Vector2 z = Vector2.zero;
            if (enddir == DLPortDir.up)
            {
                if (e.x > s.x && e.y < s.y)
                {
                    z = new Vector2(e.x, s.y);
                    line.DrawLines(s, z, e);
                }
                else
                {
                    z = new Vector2(s.x + 50, e.y + 50);
                    line.DrawLines(s, new Vector2(z.x, s.y), z, new Vector2(e.x, z.y), e);
                }
            }
            if (enddir == DLPortDir.down)
            {
                if (e.x > s.x && e.y > s.y)
                {
                    z = new Vector2(e.x, s.y);
                    line.DrawLines(s, z, e);
                }
                else
                {
                    z = new Vector2(s.x + 50, e.y - 50);
                    line.DrawLines(s, new Vector2(z.x, s.y), z, new Vector2(e.x, z.y), e);
                }
            }
            if (enddir == DLPortDir.left)
            {
                if (e.x > s.x + 50)
                {
                    z = new Vector2(s.x + 50, e.y);
                    line.DrawLines(s, new Vector2(z.x, s.y), z, e);
                }
                else
                {
                    z = new Vector2(s.x + 50, s.y + (e.y - s.y) * 0.5f);
                    line.DrawLines(s, new Vector2(z.x, s.y), z,
                      new Vector2(e.x + 50, z.y), new Vector2(e.x + 50, e.y), e);
                }
            }
            if (enddir == DLPortDir.right)
            {
                var x = Mathf.Max(s.x, e.x);
                z = new Vector2(x + 50, s.y);
                line.DrawLines(s, z, new Vector2(z.x, e.y), e);
            }
        }

        public static void DrawL2Port(DLLine line, Vector2 s, Vector2 e, DLPortDir enddir)
        {
            Vector2 z = Vector2.zero;
            if (enddir == DLPortDir.up)
            {
                if (e.x < s.x && e.y < s.y)
                {
                    z = new Vector2(e.x, s.y);
                    line.DrawLines(s, z, e);
                }
                else
                {
                    z = new Vector2(s.x - 50, e.y + 50);
                    line.DrawLines(s, new Vector2(z.x, s.y), z, new Vector2(e.x, z.y), e);
                }
            }
            if (enddir == DLPortDir.down)
            {
                if (e.x < s.x && e.y > s.y)
                {
                    z = new Vector2(e.x, s.y);
                    line.DrawLines(s, z, e);
                }
                else
                {
                    z = new Vector2(s.x - 50, e.y - 50);
                    line.DrawLines(s, new Vector2(z.x, s.y), z, new Vector2(e.x, z.y), e);
                }
            }
            if (enddir == DLPortDir.right)
            {
                if (e.x < s.x - 50)
                {
                    z = new Vector2(s.x - 50, e.y);
                    line.DrawLines(s, new Vector2(z.x, s.y), z, e);
                }
                else
                {
                    z = new Vector2(s.x - 50, s.y + (e.y - s.y) * 0.5f);
                    line.DrawLines(s, new Vector2(z.x, s.y), z,
                      new Vector2(e.x + 50, z.y), new Vector2(e.x + 50, e.y), e);
                }
            }
            if (enddir == DLPortDir.left)
            {
                var x = Mathf.Min(s.x, e.x);
                z = new Vector2(x - 50, s.y);
                line.DrawLines(s, z, new Vector2(z.x, e.y), e);
            }
        }

        public static void DrawD2Port(DLLine line, Vector2 s, Vector2 e, DLPortDir enddir)
        {
            Vector2 z = Vector2.zero;
            if (enddir == DLPortDir.left)
            {
                if (e.x > s.x && e.y < s.y)
                {
                    z = new Vector2(s.x, e.y);
                    line.DrawLines(s, z, e);
                }
                else
                {
                    z = new Vector2(e.x - 50, s.y - 50);
                    line.DrawLines(s, new Vector2(s.x, z.y), z,
                            new Vector2(z.x, e.y), e);
                }
            }
            if (enddir == DLPortDir.right)
            {
                if (e.x < s.x && e.y < s.y)
                {
                    z = new Vector2(s.x, e.y);
                    line.DrawLines(s, z, e);
                }
                else
                {
                    z = new Vector2(e.x + 50, s.y - 50);
                    line.DrawLines(s, new Vector2(s.x, z.y), z,
                            new Vector2(z.x, e.y), e);
                }
            }
            if (enddir == DLPortDir.down)
            {
                var y = Mathf.Min(e.y, s.y);
                z = new Vector2(s.x, y - 50);
                line.DrawLines(s, z, new Vector2(e.x, z.y), e);
            }
            if (enddir == DLPortDir.up)
            {
                if (e.y < s.y - 50)
                {
                    z = new Vector2(e.x, s.y - 50);
                    line.DrawLines(s, new Vector2(s.x, z.y),
                        z, e);
                }
                else
                {
                    z = new Vector2(s.x + (e.x - s.x) * 0.5f, e.y + 50);
                    line.DrawLines(s, new Vector2(s.x, s.y - 50), new Vector2(z.x, s.y - 50),
                        z, new Vector2(e.x, z.y), e);
                }
            }
        }

        public void DrawPort2Auto(Vector2 startdir, Vector2 startP, Vector2 endP, DLComponent ignore)
        {
            var c = panel.FindConnect(endP, ignore);
            if (c is DLModulePort p)
            {
                if (startdir.x == 1)
                {
                    DrawR2Port(this, startP, p.WPos, p.portdir);
                }
                if (startdir.x == -1)
                {
                    DrawL2Port(this, startP, p.WPos, p.portdir);
                }
                if (startdir.y == -1)
                {
                    DrawD2Port(this, startP, p.WPos, p.portdir);
                }
                if (startdir.y == 1)
                {
                    DrawU2Port(this, startP, p.WPos, p.portdir);
                }
            }
            else if (c is DLLineSegment seg)
            {
                DrawPort2Segment(startP, startdir, seg, endP, this);
            }
            else
            {
                Vector2 z = Vector2.zero;
                if (startdir.x == 1)
                {
                    z = new Vector2(startP.x + 50, startP.y);
                    DrawLines(startP, z, new Vector2(z.x, endP.y), endP);
                }
                if (startdir.x == -1)
                {
                    z = new Vector2(startP.x - 50, startP.y);
                    DrawLines(startP, z, new Vector2(z.x, endP.y), endP);
                }
                if (startdir.y == -1)
                {
                    z = new Vector2(startP.x, startP.y - 50);
                    DrawLines(startP, z, new Vector2(endP.x, z.y), endP);
                }
                if (startdir.y == 1)
                {
                    z = new Vector2(startP.x, startP.y + 50);
                    DrawLines(startP, z, new Vector2(endP.x, z.y), endP);
                }
            }
        }

        public static void DrawU2Port(DLLine line, Vector3 s, Vector2 e, DLPortDir enddir)
        {
            Vector2 z = Vector2.zero;
            if (enddir == DLPortDir.left)
            {
                if (e.y > s.y + 50 && e.x > s.x)
                {
                    z = new Vector2(s.x, e.y);
                    line.DrawLines(s, z, e);
                }
                else
                {
                    z = new Vector2(e.x - 50, s.y + 50);
                    line.DrawLines(s, new Vector2(s.x, z.y),
                        z, new Vector2(z.x, e.y), e);
                }
            }
            if (enddir == DLPortDir.right)
            {
                if (e.y > s.y + 50 && e.x < s.x)
                {
                    z = new Vector2(s.x, e.y);
                    line.DrawLines(s, z, e);
                }
                else
                {
                    z = new Vector2(e.x + 50, s.y + 50);
                    line.DrawLines(s, new Vector2(s.x, z.y),
                        z, new Vector2(z.x, e.y), e);
                }
            }
            if (enddir == DLPortDir.up)
            {
                var y = Mathf.Max(e.y, s.y);
                z = new Vector2(s.x, y + 50);
                line.DrawLines(s, z, new Vector2(e.x, z.y), e);
            }
            if (enddir == DLPortDir.down)
            {
                if (e.y < s.y + 50)
                {
                    z = new Vector2(s.x + (e.x - s.x) * 0.5f, e.y - 50);
                    line.DrawLines(s, new Vector2(s.x, s.y + 50), new Vector2(z.x, s.y + 50),
                        z, new Vector2(e.x, z.y), e);
                }
                else
                {
                    z = new Vector2(e.x, s.y + 50);
                    line.DrawLines(s, new Vector2(s.x, z.y),
                        z, e);
                }
            }
        }
    }
}