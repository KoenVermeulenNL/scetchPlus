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
    void Teken(SchetsControl s, Point start, Point end, Color kleur, int pendikte);
}

public abstract class StartpuntTool : ISchetsTool
{
    protected Point startpunt;
    protected Brush kwast;
    protected Color color;
    protected int pengrootte;
    protected int newPengrootte = 0;

    public virtual void MuisVast(SchetsControl s, Point p)
    {   startpunt = p;
    }
    public virtual void MuisLos(SchetsControl s, Point p)
    {
        Debug.WriteLine(color.IsEmpty);
        kwast = new SolidBrush(s.PenKleur);
        if (!color.IsEmpty) {
            kwast = new SolidBrush(color);
            color = Color.Empty;
        }
        pengrootte = s.PenGrootte;
        if (newPengrootte != 0)
        {
            pengrootte = newPengrootte;
            newPengrootte = 0;
        }

    }
    public abstract void MuisDrag(SchetsControl s, Point p);
    public abstract void Letter(SchetsControl s, char c);

    public virtual void Teken(SchetsControl s, Point start, Point end, Color kleur, int pendikte) {  }
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
    public override void Teken(SchetsControl s, Point start, Point end, Color kleur, int pendikte) { }
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
        this.Bezig(s.CreateGraphics(), this.startpunt, p);
    }
    public override void MuisLos(SchetsControl s, Point p)
    {
        base.MuisLos(s, p);
        this.Compleet(s.MaakBitmapGraphics(), this.startpunt, p);
        s.Invalidate();
        s.schets.getekendeObjecten.Add(new GetekendObject(this, this.startpunt, p, ((SolidBrush)kwast).Color, pengrootte));
    }
    public override void Teken(SchetsControl s, Point start, Point end, Color kleur, int pendikte) {
        color = kleur;
        newPengrootte = pendikte;
        base.MuisLos(s, start);
        this.Compleet(s.MaakBitmapGraphics(), start, end);
        s.Invalidate();
    }
    public override void Letter(SchetsControl s, char c)
    {
    }
    public abstract void Bezig(Graphics g, Point p1, Point p2);
        
    public virtual void Compleet(Graphics g, Point p1, Point p2)
    {   this.Bezig(g, p1, p2);
    }
}

public class RechthoekTool : TweepuntTool
{
    public override string ToString() { return "kader"; }

    public override void Bezig(Graphics g, Point p1, Point p2)
    {   g.DrawRectangle(MaakPen(kwast,pengrootte), TweepuntTool.Punten2Rechthoek(p1, p2));
    }
}
    
public class VolRechthoekTool : RechthoekTool
{
    public override string ToString() { return "vlak"; }

    public override void Compleet(Graphics g, Point p1, Point p2)
    {   g.FillRectangle(kwast, TweepuntTool.Punten2Rechthoek(p1, p2));
    }
}

//CHANGED
public class CirkelTool : TweepuntTool
{
    public override string ToString() { return "rand"; }

    public override void Bezig(Graphics g, Point p1, Point p2)
    {
        g.DrawEllipse(MaakPen(kwast, pengrootte), TweepuntTool.Punten2Rechthoek(p1, p2));
    }
}

//CHANGED
public class VolCirkelTool : CirkelTool
{
    public override string ToString() { return "cirkel"; }

    public override void Compleet(Graphics g, Point p1, Point p2)
    {
        g.FillEllipse(kwast, TweepuntTool.Punten2Rechthoek(p1, p2));
    }
}

public class LijnTool : TweepuntTool
{
    public override string ToString() { return "lijn"; }

    public override void Bezig(Graphics g, Point p1, Point p2)
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
        GetekendObject eindObject = null;
        foreach (GetekendObject gobj in s.schets.getekendeObjecten)
        {
            int? checkXbegin = gobj.beginpunt.X < gobj.eindpunt.X ? gobj.beginpunt.X : gobj.eindpunt.X;
            int? checkXeind = gobj.beginpunt.X > gobj.eindpunt.X ? gobj.beginpunt.X : gobj.eindpunt.X;
            int? checkYbegin = gobj.beginpunt.Y < gobj.eindpunt.Y ? gobj.beginpunt.Y : gobj.eindpunt.Y;
            int? checkYeind = gobj.beginpunt.Y > gobj.eindpunt.Y ? gobj.beginpunt.Y : gobj.eindpunt.Y;

            // niks met bounding te maken
            if (gobj.soort.ToString() == "lijn"){
                int x0 = x;
                int y0 = y;
                int x1 = gobj.beginpunt.X;
                int y1 = gobj.beginpunt.Y;
                int x2 = gobj.eindpunt.X;
                int y2 = gobj.eindpunt.Y;
                double afstand = (Math.Abs((x2 - x1) * (y1 - y0) - (x1 - x0) * (y2 - y1))) / (Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1)));
                if (afstand <= 2 && afstand >= -2)
                {
                    eindObject = gobj;
                }
            }
            
            if ((x >= checkXbegin && x <= checkXeind) && (y >= checkYbegin && y <= checkYeind))
            {
                if (gobj.soort.ToString() == "vlak")
                {
                    eindObject = gobj;
                }
                if (gobj.soort.ToString() == "kader")
                {
                    bool randlinks = ((x >= checkXbegin - 5 && x <= checkXbegin + 5) && (y >= checkYbegin && y <= checkYeind));
                    bool randrechts = ((x >= checkXeind - 5 && x <= checkXeind + 5) && (y >= checkYbegin && y <= checkYeind));
                    bool randboven = ((x >= checkXbegin && x <= checkXeind) && (y >= checkYbegin - 5 && y <= checkYbegin + 5));
                    bool randonder = ((x >= checkXbegin && x <= checkXeind) && (y >= checkYeind - 5 && y <= checkYeind + 5));
                    if (randlinks || randrechts || randboven || randonder)
                    {
                        eindObject = gobj;
                    }
                }
                if (gobj.soort.ToString() == "cirkel")
                {
                    // deze nog goedmaken voor ellipse
                    int beginX = (int)checkXbegin;
                    int eindX = (int)checkXeind;
                    int beginY = (int)checkYbegin;
                    int eindY = (int)checkYeind;
                    double straal = (eindX - beginX) / 2;
                    double middenX = beginX + straal;
                    double middenY = beginY + straal;
                    double afstand = Math.Sqrt((x - middenX) * (x - middenX) + (y - middenY) * (y - middenY)) - straal;
                    if (afstand <=5)
                    {
                        eindObject = gobj;
                    }
                }
                if (gobj.soort.ToString() == "rand")
                {
                    int beginX = (int)checkXbegin;
                    int eindX = (int)checkXeind;
                    int beginY = (int)checkYbegin;
                    int eindY = (int)checkYeind;
                    double straal = (eindX - beginX) / 2;
                    double middenX = beginX + straal;
                    double middenY = beginY + straal;
                    double afstand = Math.Sqrt((x - middenX) * (x - middenX) + (y - middenY) * (y - middenY));
                    if ((afstand <= straal + 2 && afstand >= straal - 2))
                    {
                        eindObject = gobj;
                    }
                }
            }
        }
        return eindObject;
    }
}
    
public class GumTool : PenTool
{
    public override string ToString() { return "gum"; }

    public override void Bezig(Graphics g, Point p1, Point p2)
    {   g.DrawLine(MaakPen(Brushes.White, pengrootte+2), p1, p2);
    }
}

public class ObjectGumTool : PenTool
{
    //Weet niet of dit de aanpak is...
    public override string ToString() { return "delete"; }

    public override void MuisLos(SchetsControl s, Point p)
    { 
        verwijderObject(s, checkbounds(s, p));
    }

    private void verwijderObject(SchetsControl s, GetekendObject obj)
    {
        if (obj != null) {
            s.schets.getekendeObjecten.Remove(obj);
            s.DrawBitmapFromList();
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