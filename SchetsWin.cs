using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

public class SchetsWin : Form
{   
    MenuStrip menuStrip;
    SchetsControl schetscontrol;
    ISchetsTool huidigeTool;
    Panel paneel;
    Button kleurKiezen;
    Bitmap penGrootteBitmap;
    Label penGrootteLabel;
    bool vast;

    private void veranderAfmeting(object o, EventArgs ea)
    {
        schetscontrol.Size = new Size ( this.ClientSize.Width  - 70
                                      , this.ClientSize.Height - 50);
        paneel.Location = new Point(64, this.ClientSize.Height - 30);
    }

    private void klikToolMenu(object obj, EventArgs ea)
    {
        this.huidigeTool = (ISchetsTool)((ToolStripMenuItem)obj).Tag;
    }

    private void klikToolButton(object obj, EventArgs ea)
    {
        this.huidigeTool = (ISchetsTool)((RadioButton)obj).Tag;
    }

    private void afsluiten(object obj, EventArgs ea)
    {
        this.Close();
    }

    public SchetsWin()
    {
        ISchetsTool[] deTools = { new PenTool()         
                                , new LijnTool()
                                , new RechthoekTool()
                                , new VolRechthoekTool()
                                , new CirkelTool()
                                , new VolCirkelTool()
                                , new TekstTool()
                                , new GumTool()
                                };
        String[] deKleuren = { "Black", "Red", "Green", "Blue", "Yellow", "Magenta", "Cyan" };

        this.ClientSize = new Size(700, 500);
        huidigeTool = deTools[0];

        schetscontrol = new SchetsControl();
        schetscontrol.Location = new Point(64, 10);
        schetscontrol.MouseDown += (object o, MouseEventArgs mea) =>
                                    {   vast=true;  
                                        huidigeTool.MuisVast(schetscontrol, mea.Location); 
                                    };
        schetscontrol.MouseMove += (object o, MouseEventArgs mea) =>
                                    {   if (vast)
                                        huidigeTool.MuisDrag(schetscontrol, mea.Location); 
                                    };
        schetscontrol.MouseUp   += (object o, MouseEventArgs mea) =>
                                    {   if (vast)
                                        huidigeTool.MuisLos (schetscontrol, mea.Location);
                                        vast = false;
                                    };
        schetscontrol.KeyPress +=  (object o, KeyPressEventArgs kpea) => 
                                    {   huidigeTool.Letter  (schetscontrol, kpea.KeyChar); 
                                    };
        this.Controls.Add(schetscontrol);

        menuStrip = new MenuStrip();
        menuStrip.Visible = false;
        this.Controls.Add(menuStrip);
        this.maakFileMenu();
        this.maakToolMenu(deTools);
        this.maakActieMenu(deKleuren);
        this.maakToolButtons(deTools);
        this.maakActieButtons(deKleuren);
        this.Resize += this.veranderAfmeting;
        this.veranderAfmeting(null, null);
    }

    private void maakFileMenu()
    {   
        ToolStripMenuItem menu = new ToolStripMenuItem("File");
        menu.MergeAction = MergeAction.MatchOnly;
        menu.DropDownItems.Add("Opslaan...", null, this.save);
        menu.DropDownItems.Add("Openen...", null, this.open);
        menu.DropDownItems.Add("Sluiten", null, this.afsluiten);
        menuStrip.Items.Add(menu);
    }

    private void maakToolMenu(ICollection<ISchetsTool> tools)
    {   
        ToolStripMenuItem menu = new ToolStripMenuItem("Tool");
        foreach (ISchetsTool tool in tools)
        {   ToolStripItem item = new ToolStripMenuItem();
            item.Tag = tool;
            item.Text = tool.ToString();
            try {
                item.Image = new Bitmap($"../../../Icons/{tool.ToString()}.png");
            } catch {
                item.Image = new Bitmap($"Icons/{tool.ToString()}.png");
            }
            item.Click += this.klikToolMenu;
            menu.DropDownItems.Add(item);
        }
        menuStrip.Items.Add(menu);
    }

    private void maakActieMenu(String[] kleuren)
    {   
        ToolStripMenuItem menu = new ToolStripMenuItem("Actie");
        menu.DropDownItems.Add("Clear", null, schetscontrol.Schoon );
        menu.DropDownItems.Add("Roteer", null, schetscontrol.Roteer );
        menu.DropDownItems.Add("Kies kleur", null, maakKleurMenu);
        menuStrip.Items.Add(menu);
    }

    private void maakToolButtons(ICollection<ISchetsTool> tools)
    {
        int t = 0;
        foreach (ISchetsTool tool in tools)
        {
            RadioButton b = new RadioButton();
            b.Appearance = Appearance.Button;
            b.Size = new Size(45, 62);
            b.Location = new Point(10, 10 + t * 62);
            b.Tag = tool;
            b.Text = tool.ToString();
            try {
                b.Image = new Bitmap($"../../../Icons/{tool.ToString()}.png");
            } catch {
                b.Image = new Bitmap($"Icons/{tool.ToString()}.png");
            }
            b.TextAlign = ContentAlignment.TopCenter;
            b.ImageAlign = ContentAlignment.BottomCenter;
            b.Click += this.klikToolButton;
            this.Controls.Add(b);
            if (t == 0) b.Select();
            t++;
        }
    }

    private void maakActieButtons(String[] kleuren)
    {   
        paneel = new Panel(); this.Controls.Add(paneel);
        paneel.Size = new Size(600, 24);
            
        Button clear = new Button(); paneel.Controls.Add(clear);
        clear.Text = "Clear";  
        clear.Location = new Point(  0, 0); 
        clear.Click += schetscontrol.Schoon;        
            
        Button rotate = new Button(); paneel.Controls.Add(rotate);
        rotate.Text = "Rotate"; 
        rotate.Location = new Point( 80, 0); 
        rotate.Click += schetscontrol.Roteer; 
           
        Label penkleur = new Label(); paneel.Controls.Add(penkleur);
        penkleur.Text = "Penkleur:"; 
        penkleur.Location = new Point(180, 3); 
        penkleur.AutoSize = true;               

        kleurKiezen = new Button(); paneel.Controls.Add(kleurKiezen);
        kleurKiezen.BackColor = Color.Red;
        kleurKiezen.Location = new Point(240, 0);
        kleurKiezen.Click += maakKleurMenu;

        TrackBar penGrootteTrackBar = new TrackBar(); paneel.Controls.Add(penGrootteTrackBar);
        penGrootteTrackBar.Location = new Point(420, 0);
        penGrootteTrackBar.Value = 3;
        penGrootteTrackBar.Minimum = 1;
        penGrootteTrackBar.Maximum = 10;
        penGrootteTrackBar.TickFrequency = 1;

        Label penGrootteTekst = new Label(); paneel.Controls.Add(penGrootteTekst);
        penGrootteTekst.Text = "Pengrootte:";
        penGrootteTekst.Location = new Point(350, 0);

        penGrootteLabel = new Label(); paneel.Controls.Add(penGrootteLabel);
        penGrootteLabel.Location = new Point(520, 0);
        penGrootteLabel.Size = new Size(30, 30);
        penGrootteLabel.BackColor = Color.Transparent;
        penGrootteBitmap = new Bitmap(30, 30);
        penGrootteLabel.Image = penGrootteBitmap;

        Graphics tijdelijk = Graphics.FromImage(penGrootteBitmap);
        tijdelijk.FillEllipse(Brushes.Red, 16-3, 11-3, 6, 6);
        penGrootteTrackBar.ValueChanged += veranderPenGrootte;
    }

    private void maakKleurMenu(object sender, EventArgs e) {
        ColorDialog colorPicker = new ColorDialog(); 
        if (colorPicker.ShowDialog() == DialogResult.OK)
        {
            kleurKiezen.BackColor = colorPicker.Color;
            schetscontrol.VeranderKleur(kleurKiezen);
        }
    }

    private void veranderPenGrootte(object sender, EventArgs e) {
        Debug.WriteLine(((TrackBar)sender).Value);
        int trackbarValue = ((TrackBar)sender).Value * 2;
        Graphics g = Graphics.FromImage(penGrootteBitmap);
        Brush kleur = new SolidBrush(kleurKiezen.BackColor);
        g.FillRectangle(new SolidBrush(Color.FromArgb(240,240,240)), 0, 0, 30, 30);
        g.FillEllipse(kleur, 16-(trackbarValue/2), 11-(trackbarValue/2), trackbarValue, trackbarValue);
        penGrootteLabel.Invalidate();

        schetscontrol.VeranderPenGrootte(trackbarValue);
    }


    //CHANGED
    private void save(object sender, EventArgs e)
    {
        SaveFileDialog dialog = new SaveFileDialog();
        dialog.Filter = "*png (*.png)|*.png|jpeg (*.jpeg)|*.jpeg";
        dialog.AddExtension = true;
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            schetscontrol.schets.bitmap.Save(dialog.FileName);
            MessageBox.Show($"{dialog.FileName} saved!");
        }
        
    }

    public void open(object sender, EventArgs e)
    {
        OpenFileDialog dialog = new OpenFileDialog();
        dialog.Filter = "*png (*.png)|*.png|jpeg (*.jpeg)|*.jpeg";
        dialog.AddExtension = true;
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            schetscontrol.schets.bitmap = new Bitmap(dialog.FileName);
            schetscontrol.Invalidate();
        }

    }
}