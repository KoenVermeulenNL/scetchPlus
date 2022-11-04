using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Diagnostics;
using static Schets;
using System.Windows.Forms;

public interface ISchetsTool
{
    void MuisVast(SchetsControl s, Point p);
    void MuisDrag(SchetsControl s, Point p);
    void MuisLos(SchetsControl s, Point p);
    void Letter(SchetsControl s, char c);
    void Teken(SchetsControl s, Point start, Point end, Brush kleur);
}

public abstract class StartpuntTool : ISchetsTool
{
    protected Point startpunt;
    protected Brush kwast;
    protected int pengrootte;

    public virtual void MuisVast(SchetsControl s, Point p)
    {   startpunt = p;
    }
    public virtual void MuisLos(SchetsControl s, Point p)
    {   kwast = new SolidBrush(s.PenKleur);
        pengrootte = s.PenGrootte;
    }
    public abstract void MuisDrag(SchetsControl s, Point p);
    public abstract void Letter(SchetsControl s, char c);

    public virtual void Teken(SchetsControl s, Point start, Point end, Brush kleur) { kwast = kleur; }
}

public class TekstTool : StartpuntTool
{
    public override string ToString() { return "tekst"; }

    public override void MuisDrag(SchetsControl s, Point p) { }

    public override void Letter(SchetsControl s, char c)
    {
        if (c >= 32)
        {
            Graphics gr = s.MaakBitmapGraphics();
            Font font = new Font("Tahoma", 40);
            string tekst = c.ToString();
            SizeF sz = 
            gr.MeasureString(tekst, font, this.startpunt, StringFormat.GenericTypographic);
            gr.DrawString   (tekst, font, kwast, 
                                            this.startpunt, StringFormat.GenericTypographic);
            // gr.DrawRectangle(Pens.Black, startpunt.X, startpunt.Y, sz.Width, sz.Height);
            startpunt.X += (int)sz.Width;
            s.Invalidate();
        }
    }
    public override void Teken(SchetsControl s, Point start, Point end, Brush kleur) {
        kwast = kleur;
    }
}

public abstract class TweepuntTool : StartpuntTool
{
    public static Rectangle Punten2Rechthoek(Point p1, Point p2)
    {   return new Rectangle( new Point(Math.Min(p1.X,p2.X), Math.Min(p1.Y,p2.Y))
                            , new Size (Math.Abs(p1.X-p2.X), Math.Abs(p1.Y-p2.Y))
                            );
    }
    public static Pen MaakPen(Brush b, int dikte)
    {   Pen pen = new Pen(b, dikte);
        pen.StartCap = LineCap.Round;
        pen.EndCap = LineCap.Round;
        return pen;
    }
    public override void MuisVast(SchetsControl s, Point p)
    {   base.MuisVast(s, p);
        kwast = Brushes.Gray;
    }
    public override void MuisDrag(SchetsControl s, Point p)
    {   s.Refresh();
        this.Bezig(s.CreateGraphics(), this.startpunt, p, kwast);
    }
    public override void MuisLos(SchetsControl s, Point p)
    {
        base.MuisLos(s, p);
        this.Compleet(s.MaakBitmapGraphics(), this.startpunt, p, kwast);
        s.Invalidate();
        s.schets.getekendeObjecten.Add(new GetekendObject(this, this.startpunt, p, kwast));
        Debug.WriteLine(s.schets.getekendeObjecten.Count);
    }
    public override void Teken(SchetsControl s, Point start, Point end, Brush kleur) {
        
        base.MuisLos(s, start);
        this.Compleet(s.MaakBitmapGraphics(), start, end, kleur);
        s.Invalidate();
    }
    public override void Letter(SchetsControl s, char c)
    {
    }
    public abstract void Bezig(Graphics g, Point p1, Point p2, Brush kleur);
        
    public virtual void Compleet(Graphics g, Point p1, Point p2, Brush kleur)
    {   this.Bezig(g, p1, p2, kleur);
    }
}

public class RechthoekTool : TweepuntTool
{
    public override string ToString() { return "kader"; }

    public override void Bezig(Graphics g, Point p1, Point p2, Brush kleur)
    {   g.DrawRectangle(MaakPen(kwast,pengrootte), TweepuntTool.Punten2Rechthoek(p1, p2));
    }
}
    
public class VolRechthoekTool : RechthoekTool
{
    public override string ToString() { return "vlak"; }

    public override void Compleet(Graphics g, Point p1, Point p2, Brush kleur)
    {   g.FillRectangle(kwast, TweepuntTool.Punten2Rechthoek(p1, p2));
    }
}

//CHANGED
public class CirkelTool : TweepuntTool
{
    public override string ToString() { return "rand"; }

    public override void Bezig(Graphics g, Point p1, Point p2, Brush kleur)
    {
        g.DrawEllipse(MaakPen(kwast, pengrootte), TweepuntTool.Punten2Rechthoek(p1, p2));
    }
}

//CHANGED
public class VolCirkelTool : CirkelTool
{
    public override string ToString() { return "cirkel"; }

    public override void Compleet(Graphics g, Point p1, Point p2, Brush kleur)
    {
        g.FillEllipse(kwast, TweepuntTool.Punten2Rechthoek(p1, p2));
    }
}

public class LijnTool : TweepuntTool
{
    public override string ToString() { return "lijn"; }

    public override void Bezig(Graphics g, Point p1, Point p2, Brush kleur)
    {   g.DrawLine(MaakPen(this.kwast, pengrootte), p1, p2);
    }
}

public class PenTool : LijnTool
{
    public override string ToString() { return "pen"; }

    public override void MuisDrag(SchetsControl s, Point p)
    {   this.MuisLos(s, p);
        this.MuisVast(s, p);
    }

    public GetekendObject checkbounds(SchetsControl s, Point p)
    {
        int x = p.X;
        int y = p.Y;
        foreach (GetekendObject gobj in s.schets.getekendeObjecten)
        {
            int? checkXbegin = gobj.beginpunt.X < gobj.eindpunt.X ? gobj.beginpunt.X : gobj.eindpunt.X;
            int? checkXeind = gobj.beginpunt.X > gobj.eindpunt.X ? gobj.beginpunt.X : gobj.eindpunt.X;
            int? checkYbegin = gobj.beginpunt.Y < gobj.eindpunt.Y ? gobj.beginpunt.Y : gobj.eindpunt.Y;
            int? checkYeind = gobj.beginpunt.Y > gobj.eindpunt.Y ? gobj.beginpunt.Y : gobj.eindpunt.Y;

            // Debug.WriteLine($"muis: {p} beginpunt: {gobj.beginpunt}");
            if ((x >= checkXbegin && x <= checkXeind) && (y >= checkYbegin && y <= checkYeind))
            {
                if (gobj.soort.ToString() == "vlak")
                {
                    return gobj;
                }
                if (gobj.soort.ToString() == "kader")
                {
                    bool randlinks = ((x >= gobj.beginpunt.X - 5 && x <= gobj.beginpunt.X + 5) && (y >= gobj.beginpunt.Y && y <= gobj.eindpunt.Y));
                    bool randrechts = ((x >= gobj.eindpunt.X - 5 && x <= gobj.eindpunt.X + 5) && (y >= gobj.beginpunt.Y && y <= gobj.eindpunt.Y));
                    bool randboven = ((x >= gobj.beginpunt.X && x <= gobj.eindpunt.X) && (y >= gobj.beginpunt.Y - 5 && y <= gobj.beginpunt.Y + 5));
                    bool randonder = ((x >= gobj.beginpunt.X && x <= gobj.eindpunt.X) && (y >= gobj.eindpunt.Y - 5 && y <= gobj.eindpunt.Y + 5));
                    if (randlinks || randrechts || randboven || randonder)
                    {
                        return gobj;
                    }
                    return null;
                }
                if (gobj.soort.ToString() == "cirkel")
                {
                    int beginX = gobj.beginpunt.X;
                    int eindX = gobj.eindpunt.X;
                    int beginY = gobj.beginpunt.Y;
                    int eindY = gobj.eindpunt.Y;
                    double straal = (eindX - beginX) / 2;
                    double middenX = beginX + straal;
                    double middenY = beginY + straal;
                    double afstand = Math.Sqrt((x - middenX) * (x - middenX) + (y - middenY) * (y - middenY));
                    if ((afstand <= straal))
                    {
                        return gobj;
                    }
                    return null;
                }
                if (gobj.soort.ToString() == "rand")
                {
                    int beginX = gobj.beginpunt.X;
                    int eindX = gobj.eindpunt.X;
                    int beginY = gobj.beginpunt.Y;
                    int eindY = gobj.eindpunt.Y;
                    double straal = (eindX - beginX) / 2;
                    double middenX = beginX + straal;
                    double middenY = beginY + straal;
                    double afstand = Math.Sqrt((x - middenX) * (x - middenX) + (y - middenY) * (y - middenY));
                    if ((afstand <= straal + 5 && afstand >= straal - 5))
                    {
                        return gobj;
                    }
                    return null;
                }
                if (gobj.soort.ToString() == "lijn")
                {
                    int x0 = x;
                    int y0 = y;
                    int x1 = gobj.beginpunt.X;
                    int y1 = gobj.beginpunt.Y;
                    int x2 = gobj.eindpunt.X;
                    int y2 = gobj.eindpunt.Y;
                    double afstand = (Math.Abs((x2 - x1) * (y1 - y0) - (x1 - x0) * (y2 - y1))) / (Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1)));
                    if (afstand <= 5 && afstand >= -5)
                    {
                        return gobj;
                    }
                    return null;
                }
            }
        }
        return null;
    }
}
    
public class GumTool : PenTool
{
    public override string ToString() { return "gum"; }

    public override void Bezig(Graphics g, Point p1, Point p2, Brush kleur)
    {   g.DrawLine(MaakPen(Brushes.White, pengrootte+2), p1, p2);
    }
}

public class ObjectGumTool : PenTool
{
    //Weet niet of dit de aanpak is...
    public override string ToString() { return "delete"; }

    public override void MuisLos(SchetsControl s, Point p)
    { 
        verwijderObject(checkbounds(s, p));
    }

    private void verwijderObject(GetekendObject obj) {
        if (obj != null)
        {
            Debug.WriteLine(obj.soort.ToString());
        }
    }
}

public class MoveTool : PenTool
{
    //Weet niet of dit de aanpak is...
    public override string ToString() { return "move"; }


    int pInBoundX = 0;
    int pInBoundY = 0;

    public override void MuisVast(SchetsControl s, Point p)
    {
        GetekendObject obj = checkbounds(s, p);
        if (obj != null)
        {
            pInBoundX = p.X - obj.beginpunt.X;
            pInBoundY = p.Y - obj.beginpunt.Y;
        }
        
    }

    public override void MuisLos(SchetsControl s, Point p)
    {
        GetekendObject obj = checkbounds(s, p);
        if (obj != null)
        {
            int width = obj.eindpunt.X - obj.beginpunt.X;
            int height = obj.eindpunt.Y - obj.beginpunt.Y;

            

            obj.beginpunt.X = p.X - pInBoundX;
            obj.eindpunt.X = obj.beginpunt.X + width;
            obj.beginpunt.Y = p.Y - pInBoundY;
            obj.eindpunt.Y = obj.beginpunt.Y + height;
            s.DrawBitmapFromList();
        }
    }
}