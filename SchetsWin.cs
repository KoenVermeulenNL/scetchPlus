using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using static Schets;

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

    ISchetsTool[] tempTools = { new PenTool()
                                , new LijnTool()
                                , new RechthoekTool()
                                , new VolRechthoekTool()
                                , new CirkelTool()
                                , new VolCirkelTool()
                                , new TekstTool()
                                , new ObjectGumTool()
                                , new MoveTool()
                                , new GumTool()
                                };

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
        ISchetsTool[] deTools = tempTools;
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
                                    {   huidigeTool.Letter  (schetscontrol, kpea.KeyChar, kleurKiezen.BackColor, false); 
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
        ToolStripMenuItem saveMenu = new ToolStripMenuItem("File");
        saveMenu.DropDownItems.Add("Opslaan als afbeelding...", null, this.save);
        saveMenu.DropDownItems.Add("Opslaan als object...", null, this.saveObject);
        saveMenu.DropDownItems.Add("Openen...", null, this.open);
        saveMenu.DropDownItems.Add("Openen als object...", null, this.openObject);
        menu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { saveMenu });
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
        menu.DropDownItems.Add("Undo", null, schetscontrol.Undo);
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
        clear.Location = new Point(0, 0);
        clear.Click += schetscontrol.Schoon;

        Button rotate = new Button(); paneel.Controls.Add(rotate);
        rotate.Text = "Rotate";
        rotate.Location = new Point(80, 0);
        rotate.Click += schetscontrol.Roteer;

        Label penkleur = new Label(); paneel.Controls.Add(penkleur);
        penkleur.Text = "Penkleur:";
        penkleur.Location = new Point(320, 3);
        penkleur.AutoSize = true;

        kleurKiezen = new Button(); paneel.Controls.Add(kleurKiezen);
        kleurKiezen.BackColor = Color.Red;
        kleurKiezen.Location = new Point(380, 0);
        kleurKiezen.Click += maakKleurMenu;

        TrackBar penGrootteTrackBar = new TrackBar(); paneel.Controls.Add(penGrootteTrackBar);
        penGrootteTrackBar.Location = new Point(520, 0);
        penGrootteTrackBar.Value = 3;
        penGrootteTrackBar.Minimum = 1;
        penGrootteTrackBar.Maximum = 10;
        penGrootteTrackBar.TickFrequency = 1;

        Label penGrootteTekst = new Label(); paneel.Controls.Add(penGrootteTekst);
        penGrootteTekst.Text = "Pengrootte:";
        penGrootteTekst.Location = new Point(460, 0);

        penGrootteLabel = new Label(); paneel.Controls.Add(penGrootteLabel);
        penGrootteLabel.Location = new Point(520, 0);
        penGrootteLabel.Size = new Size(30, 30);
        penGrootteLabel.BackColor = Color.Transparent;
        penGrootteBitmap = new Bitmap(30, 30);
        penGrootteLabel.Image = penGrootteBitmap;

        Graphics tijdelijk = Graphics.FromImage(penGrootteBitmap);
        tijdelijk.FillEllipse(Brushes.Red, 16 - 3, 11 - 3, 6, 6);
        penGrootteTrackBar.ValueChanged += veranderPenGrootte;

        Button undo = new Button(); paneel.Controls.Add(undo);
        undo.Text = "Undo";
        undo.Location = new Point(160, 0);
        undo.Click += schetscontrol.Undo;

        Button redo = new Button(); paneel.Controls.Add(redo);
        redo.Text = "Redo";
        redo.Location = new Point(240, 0);
        redo.Click += schetscontrol.Redo;
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

    private void saveObject(object sender, EventArgs e)
    {
        SaveFileDialog dialog = new SaveFileDialog();
        dialog.Filter = "Text File | *.txt";
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            string res = ""; //; = seperator between objects
            foreach (GetekendObject obj in schetscontrol.schets.getekendeObjecten)
            {
                res += $"{obj.soort.ToString()}~{obj.beginpunt.X}~{obj.beginpunt.Y}~{obj.eindpunt.X}~{obj.eindpunt.Y}~{obj.kleur.A}-{obj.kleur.R}-{obj.kleur.G}-{obj.kleur.B}~{obj.lijndikte}~{obj.c};";
            }
            StreamWriter writer = new StreamWriter(dialog.OpenFile());

            writer.Write(res);
            writer.Dispose();
            writer.Close();
        }


    }

    public void openObject(object sender, EventArgs e)
    {
        OpenFileDialog dialog = new OpenFileDialog();
        dialog.Filter = "Text File | *.txt";
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            // try
            // {
                // Create a StreamReader  
                using (StreamReader reader = new StreamReader(dialog.FileName))
                {
                    string line;
                    schetscontrol.schets.getekendeObjecten.Clear();
                    // Read line by line  
                    while ((line = reader.ReadLine()) != null)
                    {
                        string fullLine = line;
                        string[] objectStrings = fullLine.Split(";");
                        foreach (string objectString in objectStrings) {
                            string[] objectProps = objectString.Split("~");
                            if (objectProps.Length > 1) { 

                                ISchetsTool tool = tempTools[0];

                                foreach (ISchetsTool t in tempTools) {
                                    if (t.ToString() == objectProps[0]) {
                                        tool = t;
                                    }
                                }

                            string[] rgbStrings = objectProps[5].Split("-");

                                GetekendObject gObj = new GetekendObject(   
                                        tool,
                                        new Point(Int32.Parse(objectProps[1]), Int32.Parse(objectProps[2])),
                                        new Point(Int32.Parse(objectProps[3]), Int32.Parse(objectProps[4])),
                                        Color.FromArgb(Int32.Parse(rgbStrings[0]), Int32.Parse(rgbStrings[1]), Int32.Parse(rgbStrings[2]), Int32.Parse(rgbStrings[3])), 
                                        Int32.Parse(objectProps[6]),
                                        objectProps[7]
                                    );
                                schetscontrol.schets.getekendeObjecten.Add(gObj);
                            }
                        }
                        schetscontrol.DrawBitmapFromList();
                    }
                }
            // }
            // catch (Exception exp)
            // {
            //     MessageBox.Show(exp.Message);
            // }
        }

    }
}