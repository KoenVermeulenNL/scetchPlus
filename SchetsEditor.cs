using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

public class SchetsEditor : Form
{
    private MenuStrip menuStrip;
    private SchetsWin currentSchetswin;

    public SchetsEditor()
    {   
        this.ClientSize = new Size(800, 600);
        menuStrip = new MenuStrip();
        this.Controls.Add(menuStrip);
        this.maakFileMenu();
        this.maakHelpMenu();
        this.Text = "Schets editor";
        this.IsMdiContainer = true;
        this.MainMenuStrip = menuStrip;
    }
    private void maakFileMenu()
    {   
        ToolStripDropDownItem menu = new ToolStripMenuItem("File");
        menu.DropDownItems.Add("Nieuw", null, this.nieuw);
        menu.DropDownItems.Add("Openen...", null, this.open);
        menu.DropDownItems.Add("Exit", null, this.afsluiten);
        menuStrip.Items.Add(menu);
    }
    private void maakHelpMenu()
    {   
        ToolStripDropDownItem menu = new ToolStripMenuItem("Help");
        menu.DropDownItems.Add("Over \"Schets\"", null, this.about);
        menuStrip.Items.Add(menu);
    }
    private void about(object o, EventArgs ea)
    {   
        MessageBox.Show ( "Schets versie 2.0\n(c) UU Informatica 2022. Modified by Hugo & Koen."
                        , "Over \"Schets\""
                        , MessageBoxButtons.OK
                        , MessageBoxIcon.Information
                        );
    }

    private void nieuw(object sender, EventArgs e)
    {   
        SchetsWin s = new SchetsWin();
        s.MdiParent = this;
        currentSchetswin = s;
        s.Show();
    }
    private void afsluiten(object sender, EventArgs e)
    {   
        this.Close();
    }

    private void open(object sender, EventArgs e)
    {
        SchetsWin s = new SchetsWin();
        s.MdiParent = this;
        currentSchetswin = s;
        s.open(sender, e);
        s.Show();
    }
}