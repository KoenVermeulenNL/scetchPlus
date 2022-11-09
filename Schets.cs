using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

public class Schets
{
    public Bitmap bitmap;
    public List<GetekendObject> getekendeObjecten = new List<GetekendObject>();
    public List<GetekendObject> savedGetekendeObjecten = new List<GetekendObject>();

    public Schets()
    {
        bitmap = new Bitmap(1, 1);
    }

    public class GetekendObject
    {
        public ISchetsTool soort;
        public Point beginpunt;
        public Point eindpunt;
        public Color kleur;
        public int lijndikte;
        public string c = "";

        public GetekendObject(ISchetsTool soort, Point beginpunt, Point eindpunt, Color kleur, int lijndikte, string c) {
            this.soort = soort;
            this.beginpunt = beginpunt;
            this.eindpunt = eindpunt;
            this.kleur = kleur;
            this.lijndikte = lijndikte;
            this.c = c;
        }
    }

    public Graphics BitmapGraphics
    {   
        get { return Graphics.FromImage(bitmap); }
    }
    public void VeranderAfmeting(Size sz)
    {
        if (sz.Width > bitmap.Size.Width || sz.Height > bitmap.Size.Height)
        {
            Bitmap nieuw = new Bitmap( Math.Max(sz.Width,  bitmap.Size.Width)
                                     , Math.Max(sz.Height, bitmap.Size.Height)
                                     );
            Graphics gr = Graphics.FromImage(nieuw);
            gr.FillRectangle(Brushes.White, 0, 0, sz.Width, sz.Height);
            gr.DrawImage(bitmap, 0, 0);
            bitmap = nieuw;
        }
    }
    public void Teken(Graphics gr)
    {
        gr.DrawImage(bitmap, 0, 0);
       
    }
    public void Schoon()
    {
        Graphics gr = Graphics.FromImage(bitmap);
        gr.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);
        
    }
    public void Roteer()
    {
        bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
    }
}