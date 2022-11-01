using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Diagnostics;
using static Schets;

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

    public override void Bezig(Graphics g, Point p1, Point p2)
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

    public override void Bezig(Graphics g, Point p1, Point p2, Brush kleur)
    {
        
    }
}