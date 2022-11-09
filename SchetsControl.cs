using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Diagnostics;
using static Schets;

public class SchetsControl : UserControl
{   
    public Schets schets;
    private Color penkleur = Color.Red;
    private int pengrootte = 3;

    public Color PenKleur
    { get { return penkleur; }
    }
    public int PenGrootte
    { get { return pengrootte; }
    }
    public Schets Schets
    { get { return schets;   }
    }
    public SchetsControl()
    {   this.BorderStyle = BorderStyle.Fixed3D;
        this.schets = new Schets();
        this.Paint += this.teken;
        this.Resize += this.veranderAfmeting;
        this.veranderAfmeting(null, null);
    }
    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }
    private void teken(object o, PaintEventArgs pea)
    {   schets.Teken(pea.Graphics);
        
    }
    private void veranderAfmeting(object o, EventArgs ea)
    {   schets.VeranderAfmeting(this.ClientSize);
        this.Invalidate();
    }
    public Graphics MaakBitmapGraphics()
    {   Graphics g = schets.BitmapGraphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        return g;
    }
    public void Schoon(object o, EventArgs ea)
    {   schets.Schoon();
        schets.getekendeObjecten.Clear();
        this.Invalidate();
    }
    public void Roteer(object o, EventArgs ea)
    {   /*schets.VeranderAfmeting(new Size(this.ClientSize.Height, this.ClientSize.Width));
        schets.Roteer();
        this.Invalidate();*/
        DrawBitmapFromList();
    }
    public void VeranderKleur(Button kleurKiezen)
    {   Color kleurNaam = kleurKiezen.BackColor;
        penkleur = kleurNaam;
    }

    public void VeranderPenGrootte(int value) {
        pengrootte = value;
    }

        //CHANGED
    public void DrawBitmapFromList() {
        schets.Schoon();
        foreach (GetekendObject gObject in schets.getekendeObjecten)
        {
            if (gObject.soort.ToString() == "tekst")
            {
                gObject.soort.veranderStartpunt(gObject.beginpunt);
                gObject.soort.Letter(this, gObject.c.ToCharArray()[0], gObject.kleur, true);
            }
            else if (gObject.soort.ToString() == "pen") {
                gObject.soort.Teken(this, gObject.beginpunt, gObject.eindpunt, gObject.kleur, gObject.lijndikte);
            
            foreach (GetekendObject pTSegment in gObject.penToolSegments) {
                    gObject.soort.Teken(this, pTSegment.beginpunt, pTSegment.eindpunt, pTSegment.kleur, pTSegment.lijndikte);
                }            
            } else { gObject.soort.Teken(this, gObject.beginpunt, gObject.eindpunt, gObject.kleur, gObject.lijndikte); }
        }
        this.Invalidate();
    }

    public void Undo(object o, EventArgs ea)
    {
        if (schets.getekendeObjecten.Count > 0)
        {
            schets.savedGetekendeObjecten.Add(schets.getekendeObjecten[schets.getekendeObjecten.Count - 1]);
            schets.getekendeObjecten.RemoveAt(schets.getekendeObjecten.Count - 1);
            DrawBitmapFromList();
        }
    }
    public void Redo(object o, EventArgs ea)
    {
        if (schets.savedGetekendeObjecten.Count > 0)
        {
            schets.getekendeObjecten.Add(schets.savedGetekendeObjecten[schets.savedGetekendeObjecten.Count - 1]);
            schets.savedGetekendeObjecten.RemoveAt(schets.savedGetekendeObjecten.Count - 1);
            DrawBitmapFromList();
        }
    }
}