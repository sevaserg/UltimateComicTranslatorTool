using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UltimateComicTranslatorTool
{
    public partial class Form1 : Form
    {
        Pt[] pts;
        ToolStripMenuItem[] filesubitems;
        String currentFolder;
        String[] pages;
        int selectedPt = -1;
        public Form1()
        {
            InitializeComponent();
            pictureBox.MouseClick += new MouseEventHandler(editCircles);
            pts = new Pt[1];
            pts[0] = new Pt(10, 10, "", "Test", "");


            filesubitems = new ToolStripMenuItem[3];
            for (int i = 0; i < 3; i++)
                filesubitems[i] = new ToolStripMenuItem();
            filesubitems[0].Text = "Open";
            filesubitems[0].Click += new EventHandler(openEvt);
            filesubitems[1].Text = "Save";
            filesubitems[1].Click += new EventHandler(saveEvt);
            filesubitems[2].Text = "Exit";
            filesubitems[2].Click += new EventHandler(exitEvt);
            filesubitems[1].Enabled = false;
            fileMenuItem.DropDownItems.AddRange(filesubitems);
            LP.ColumnStyles[0].SizeType = SizeType.Percent;
            LP.ColumnStyles[0].Width = 70;
            LP.ColumnStyles[1].SizeType = SizeType.Percent;
            LP.ColumnStyles[1].Width = 30;
            pageChoice.TextChanged += new EventHandler(pageReselect);
            fwdBtn.Click += new EventHandler(fwdEvt);
            bckBtn.Click += new EventHandler(bckEvt);
            textBox.TextChanged += new EventHandler(dataChange);
            commentBox.TextChanged += new EventHandler(dataChange);

            pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
        }
        private void pageReselect(object sender, EventArgs e)
        {
            if (pageChoice.Items.Count > 0)
                redraw();
        }
        private void fwdEvt(object sender, EventArgs e)
        {
            int currentIndex = pageChoice.FindStringExact(pageChoice.Text);
            if (currentIndex < pageChoice.Items.Count - 1 && pageChoice.Items.Count > 0)
            {
                currentIndex++;
                pageChoice.SelectedIndex = currentIndex;
                redraw();
            }
        }
        private void bckEvt(object sender, EventArgs e)
        {
            int currentIndex = pageChoice.FindStringExact(pageChoice.Text);
            if (currentIndex > 0 && pageChoice.Items.Count > 0)
            {
                currentIndex--;
                pageChoice.SelectedIndex = currentIndex;
                redraw();
            }
        }
        private void openEvt(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog folderDialog = new FolderBrowserDialog();
                folderDialog.ShowDialog();
                currentFolder = folderDialog.SelectedPath;
                filesubitems[1].Enabled = true;
                pages = System.IO.Directory.GetFiles(currentFolder);
                textBox.Text = "";
                commentBox.Text = "";
                if (System.IO.File.Exists(currentFolder + "\\savefile.uctt"))
                {
                    String s = System.IO.File.ReadAllText(currentFolder + "\\savefile.uctt");
                    List<Pt> ptsList = new List<Pt>();
                    if (s.Split("<~~obj_razdel~~>").Length > 0)
                    {
                        try
                        {
                            foreach (String str in s.Split("<~~obj_razdel~~>"))
                            {
                                if (str != "")
                                {
                                    Pt newpt = new Pt(int.Parse(str.Split("<~~param_razdel~~>")[0]),
                                                      int.Parse(str.Split("<~~param_razdel~~>")[1]),
                                                      str.Split("<~~param_razdel~~>")[2],
                                                      str.Split("<~~param_razdel~~>")[3],
                                                      ""
                                                      );

                                    if (str.Split("<~~param_razdel~~>").Length > 4)
                                    {
                                        newpt.comment = str.Split("<~~param_razdel~~>")[4];
                                    }
                                    ptsList.Add(newpt);
                                }
                            }
                            pts = ptsList.ToArray();
                        }
                        catch
                        {
                            MessageBox.Show("Loading error!");
                        }
                    }
                }
                else
                {
                    System.IO.File.Create(currentFolder + "\\savefile.uctt");
                }
                pageChoice.Items.Clear();
                if (pages.Length > 0)
                {
                    foreach (string page in pages)
                        pageChoice.Items.Add(page.Split("\\")[page.Split("\\").Length - 1]);
                    pageChoice.Enabled = true;
                    pageChoice.SelectedIndex = 0;
                    redraw();
                }
                else
                {
                    pageChoice.Enabled = false;
                }
            }
            catch
            {

            }
        }
        private void saveEvt(object sender, EventArgs e)
        {
                save();
        }
        private void save()
        {
            String s = "";
            foreach (Pt pt in pts)
            {
                s += pt.X + "<~~param_razdel~~>" + pt.Y + "<~~param_razdel~~>" + pt.file + "<~~param_razdel~~>" + pt.contents + "<~~param_razdel~~>" + pt.comment + "<~~obj_razdel~~>";
            }
            try
            {
                if (System.IO.File.Exists(currentFolder + "\\savefile.uctt"))
                    System.IO.File.Delete(currentFolder + "\\savefile.uctt");
            }
            catch { }
            System.IO.File.WriteAllText(currentFolder+"\\savefile.uctt", s);
        }
        private void exitEvt(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void editCircles(object sender, MouseEventArgs e)
        {
            if (pageChoice.Text != "")
            {
                if (e.Button == MouseButtons.Right)
                {
                    List<Pt> ptsList = new List<Pt>();
                    bool removePt = false;
                    foreach (Pt pt in pts)
                    {
                        if ((pt.X - 5 > e.X || pt.X + 5 < e.X || pt.Y - 5 > e.Y || pt.Y + 5 < e.Y))
                            ptsList.Add(pt);
                        else
                            removePt = true;
                    }

                    Pt newpt = new Pt(e.X, e.Y, pageChoice.Text, "", "");
                    if (removePt == false)
                    {
                        ptsList.Add(newpt);
                        pts = ptsList.ToArray();

                        selectedPt = pts.Length - 1;
                    }
                    else
                    {
                        selectedPt = -1;
                        pts = ptsList.ToArray();
                    }
                    rewrite();

                    redraw();
                }
                if (e.Button == MouseButtons.Left)
                {
                    for (int i = 0; i < pts.Length; i++)
                    {
                        Pt pt = pts[i];
                        if (pt.file == pageChoice.Text && (!(pt.X - 5 > e.X || pt.X + 5 < e.X || pt.Y - 5 > e.Y || pt.Y + 5 < e.Y)))
                        {
                            selectedPt = i;
                            break;
                        }
                    }
                    rewrite();
                }
            }
        }
        private void rewrite()
        {
            if (selectedPt >= 0 && selectedPt < pts.Length)
            {
                String comment = pts[selectedPt].comment;
                textBox.Text = pts[selectedPt].contents;
                commentBox.Text = comment;
                // MessageBox.Show("Text: " + pts[selectedPt].contents + "\nComm: " + pts[selectedPt].comment);

            }
        }
        private void dataChange(object sender, EventArgs e)
        {
            if (selectedPt >= 0)
            {
                pts[selectedPt].comment = commentBox.Text;
            }
        }
        private void redraw()
        {
            if (pageChoice.Enabled)
            {
                Image DrawArea;
                DrawArea = new Bitmap(Image.FromFile(currentFolder + "\\" + pageChoice.Text));
                
                Brush mybrush = new SolidBrush(Color.Red);
                Graphics g = Graphics.FromImage(DrawArea);
                foreach (Pt pt in pts)
                {
                    if (pt.file == pageChoice.Text)
                    {
                        g.FillEllipse(mybrush, pt.X-5, pt.Y-5, 10, 10);
                    }
                }
                pictureBox.Size = DrawArea.Size;
                pictureBox.Image = DrawArea;
                g.Dispose();
            }
        }
        public class Pt
        {
            public int X;
            public int Y;
            public String file;
            public String contents;
            public String comment;
            public Pt(int X_, int Y_, String file_, String contents_, String comment_)
            {
                X = X_;
                Y = Y_;
                file = file_;
                contents = contents_;
                comment = comment_;
            }
            public Pt()
            {

            }
        }

        private void LP_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
