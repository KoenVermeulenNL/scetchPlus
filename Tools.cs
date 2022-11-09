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
    void Letter(SchetsControl s, char c, Color kleur, bool opened);
    void Teken(SchetsControl s, Point start, Point end, Color kleur, int pendikte);
    void veranderStartpunt(Point p);
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
    public abstract void Letter(SchetsControl s, char c, Color Kleur, bool opened);

    public virtual void Teken(SchetsControl s, Point start, Point end, Color kleur, int pendikte) {  }
    
    public abstract void veranderStartpunt(Point p);
}

public class TekstTool : StartpuntTool
{
    public override string ToString() { return "tekst"; }

    public override void MuisDrag(SchetsControl s, Point p) { }

    public override void Letter(SchetsControl s, char c, Color kleur, bool opened)
    {
        if (c >= 31)
        {
            Graphics gr = s.MaakBitmapGraphics();
            Font font = new Font("Tahoma", 40);
            string tekst = c.ToString();
            SizeF sz = gr.MeasureString(tekst, font, this.startpunt, StringFormat.GenericTypographic);
            gr.DrawString(tekst, font, new SolidBrush(kleur), this.startpunt, StringFormat.GenericTypographic);
            if (!opened) {
                s.schets.getekendeObjecten.Add(new GetekendObject(this, this.startpunt, new Point(this.startpunt.X + (int)sz.Width, this.startpunt.Y + (int)sz.Height), kleur, pengrootte, c.ToString()));
            }
            startpunt.X += (int)sz.Width;
            s.Invalidate();
        }
    }
    public override void Teken(SchetsControl s, Point start, Point end, Color kleur, int pendikte) { }

    public override void veranderStartpunt(Point p)
    {
        startpunt = p;
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
        this.Bezig(s.CreateGraphics(), this.startpunt, p);
    }
    public override void MuisLos(SchetsControl s, Point p)
    {
        base.MuisLos(s, p);
        this.Compleet(s.MaakBitmapGraphics(), this.startpunt, p);
        s.Invalidate();
        s.schets.getekendeObjecten.Add(new GetekendObject(this, this.startpunt, p, ((SolidBrush)kwast).Color, pengrootte, ""));
    }
    public override void Teken(SchetsControl s, Point start, Point end, Color kleur, int pendikte) {
        color = kleur;
        newPengrootte = pendikte;
        base.MuisLos(s, start);
        this.Compleet(s.MaakBitmapGraphics(), start, end);
        s.Invalidate();
    }
    public override void Letter(SchetsControl s, char c, Color kleur, bool opened)
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
    public override void veranderStartpunt(Point p)
    {
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
    public override void veranderStartpunt(Point p)
    {}
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
    public override void veranderStartpunt(Point p) {

    }

    public override void Bezig(Graphics g, Point p1, Point p2)
    {   g.DrawLine(MaakPen(this.kwast, pengrootte), p1, p2);
    }

    public double afstandCirkel(int x, int y, int bX, int eX, int bY, int eY) {
        double a = (eX - bX)/2;
        double b = (eY - bY)/2;
        double mX = bX + a;
        double mY = bY + b;
        double afstandCirkel = ((x-mX)*(x-mX))/(a*a) + ((y-mY)*(y-mY))/(b*b);
        return afstandCirkel;
    }

    public GetekendObject checkbounds(SchetsControl s, Point p, int threshold)
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
                double afstand = (Math.Abs((gobj.eindpunt.X - gobj.beginpunt.X) * (gobj.beginpunt.Y - y) - (gobj.beginpunt.X - x) * (gobj.eindpunt.Y - gobj.beginpunt.Y))) / (Math.Sqrt((gobj.eindpunt.X - gobj.beginpunt.X) * (gobj.eindpunt.X - gobj.beginpunt.X) + (gobj.eindpunt.Y - gobj.beginpunt.Y) * (gobj.eindpunt.Y - gobj.beginpunt.Y)));
                if (afstand <= gobj.lijndikte && afstand >= -1*gobj.lijndikte)
                {
                    eindObject = gobj;
                }
            }
            
            if ((x >= checkXbegin-threshold && x <= checkXeind+threshold) && (y >= checkYbegin-threshold && y <= checkYeind+threshold))
            {
                Debug.WriteLine("in");
                if ( threshold == 0 ) {
                    switch (gobj.soort.ToString()) {
                        case "vlak": case "tekst":
                            eindObject = gobj;
                            break;

                        case "kader":
                            bool randlinks = ((x >= checkXbegin - gobj.lijndikte && x <= checkXbegin + gobj.lijndikte) && (y >= checkYbegin && y <= checkYeind));
                            bool randrechts = ((x >= checkXeind - gobj.lijndikte && x <= checkXeind + gobj.lijndikte) && (y >= checkYbegin && y <= checkYeind));
                            bool randboven = ((x >= checkXbegin && x <= checkXeind) && (y >= checkYbegin - gobj.lijndikte && y <= checkYbegin + gobj.lijndikte));
                            bool randonder = ((x >= checkXbegin && x <= checkXeind) && (y >= checkYeind - gobj.lijndikte && y <= checkYeind + gobj.lijndikte));
                            if (randlinks || randrechts || randboven || randonder)
                            {
                                eindObject = gobj;
                            }
                            break;
                    
                        case "rand":
                            double afstandCirkelrand = afstandCirkel(x, y, (int)checkXbegin, (int)checkXeind, (int)checkYbegin, (int)checkYeind);
                            if (afstandCirkelrand <= 1+(double)((double)gobj.lijndikte/100) && afstandCirkelrand >= 1-(double)((double)gobj.lijndikte/100))
                            {
                                eindObject = gobj;
                            }
                            break;
                        case "cirkel":
                            // formule voor ellipse:
                            // (x-mx)^2/a^2 + (y-my)^2/b^2
                            double AfstandCirkel = afstandCirkel(x, y, (int)checkXbegin, (int)checkXeind, (int)checkYbegin, (int)checkYeind);
                            if (AfstandCirkel <= 1+(double)((double)gobj.lijndikte/100))
                            {
                                eindObject = gobj;
                            }
                            break;
                    }
                } else eindObject = gobj;
            }
        }
        return eindObject;
    }
}

public class PenTool : LijnTool
{
    public override string ToString() { return "pen"; }

    public override void MuisDrag(SchetsControl s, Point p)
    {   this.MuisLos(s, p);
        this.MuisVast(s, p);
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
        verwijderObject(s, checkbounds(s, p, 0));
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
        GetekendObject obj = checkbounds(s, p, 20);
        if (obj != null)
        {
            pInBoundX = p.X - obj.beginpunt.X;
            pInBoundY = p.Y - obj.beginpunt.Y;
        }
        
    }

    public override void MuisLos(SchetsControl s, Point p)
    {
        GetekendObject obj = checkbounds(s, p, 20);
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