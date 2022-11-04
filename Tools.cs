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
        GetekendObject eindObject = null;
        foreach (GetekendObject gobj in s.schets.getekendeObjecten)
        {
            int? checkXbegin = gobj.beginpunt.X < gobj.eindpunt.X ? gobj.beginpunt.X : gobj.eindpunt.X;
            int? checkXeind = gobj.beginpunt.X > gobj.eindpunt.X ? gobj.beginpunt.X : gobj.eindpunt.X;
            int? checkYbegin = gobj.beginpunt.Y < gobj.eindpunt.Y ? gobj.beginpunt.Y : gobj.eindpunt.Y;
            int? checkYeind = gobj.beginpunt.Y > gobj.eindpunt.Y ? gobj.beginpunt.Y : gobj.eindpunt.Y;

            // niks met bounding te maken
            if (gobj.soort.ToString() == "lijn")
            {
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
                switch (gobj.soort.ToString()) {
                    case "vlak":
                        eindObject = gobj;
                        break;

                    case "kader":
                        bool randlinks = ((x >= checkXbegin - 5 && x <= checkXbegin + 5) && (y >= checkYbegin && y <= checkYeind));
                        bool randrechts = ((x >= checkXeind - 5 && x <= checkXeind + 5) && (y >= checkYbegin && y <= checkYeind));
                        bool randboven = ((x >= checkXbegin && x <= checkXeind) && (y >= checkYbegin - 5 && y <= checkYbegin + 5));
                        bool randonder = ((x >= checkXbegin && x <= checkXeind) && (y >= checkYeind - 5 && y <= checkYeind + 5));
                        if (randlinks || randrechts || randboven || randonder)
                        {
                            eindObject = gobj;
                        }
                        break;
                
                    case "rand":
                        int beginXrand = (int)checkXbegin;
                        int eindXrand = (int)checkXeind;
                        int beginYrand = (int)checkYbegin;
                        int eindYrand = (int)checkYeind;
                        double arand = (eindXrand - beginXrand)/2;
                        double brand = (eindYrand - beginYrand)/2;
                        double mXrand = beginXrand + arand;
                        double mYrand = beginYrand + brand;
                        double afstandCirkelrand = ((x-mXrand)*(x-mXrand))/(arand*arand) + ((y-mYrand)*(y-mYrand))/(brand*brand);
                        if (afstandCirkelrand <= 1.05 && afstandCirkelrand >= 0.95)
                        {
                            eindObject = gobj;
                        }
                        break;
                    case "cirkel":
                        // deze nog goedmaken voor ellipse
                        // formule voor ellipse:
                        // (x-mx)^2/a^2 + (y-my)^2/b^2
                        int beginX = (int)checkXbegin;
                        int eindX = (int)checkXeind;
                        int beginY = (int)checkYbegin;
                        int eindY = (int)checkYeind;
                        double a = (eindX - beginX)/2;
                        double b = (eindY - beginY)/2;
                        double mX = beginX + a;
                        double mY = beginY + b;
                        double afstandCirkel = ((x-mX)*(x-mX))/(a*a) + ((y-mY)*(y-mY))/(b*b);
                        if (afstandCirkel <= 1.05)
                        {
                            eindObject = gobj;
                        }
                        break;
                }
            }
        }
        return eindObject;
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
        verwijderObject(s, checkbounds(s, p));
    }

    private void verwijderObject(SchetsControl s, GetekendObject obj)
    {
        if (obj != null) 
        {
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