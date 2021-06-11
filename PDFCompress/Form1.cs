using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PDFCompress
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            btnCompress.MouseEnter += OnMouseEnter2;
            btnCompress.MouseLeave += OnMouseLeave2;
        }

        private void OnMouseEnter(object sender, EventArgs e)
        {
            ChangeBtnColor1(btnAdd);
        }
        private void OnMouseLeave(object sender, EventArgs e)
        {
            ChangeBtnColor2(btnAdd);
        }
        private void OnMouseEnter2(object sender, EventArgs e)
        {
            ChangeBtnColor1(btnCompress);
        }
        private void OnMouseLeave2(object sender, EventArgs e)
        {
            ChangeBtnColor2(btnCompress);
        }
        private void ChangeBtnColor1(Button btn)
        {
            btn.BackColor = Color.Yellow; 
            btn.ForeColor = Color.MidnightBlue;
        }
        private void ChangeBtnColor2(Button btn)
        {
            btn.BackColor = Color.Transparent;
            btn.ForeColor = Color.Yellow;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog oFD = new OpenFileDialog())
            {
                oFD.Multiselect = true;
                oFD.Title = "Выберите PDF файл";
                oFD.Filter = "PDF фалы|*.pdf";

                if (oFD.ShowDialog() == DialogResult.OK)
                {
                    foreach (var f in oFD.FileNames)
                    {
                        listBoxPDFs.Items.Add(f);
                    }
                }
            }
        }

        /// <summary> Method that uses ghost script to compress PDF files </summary>
        /// <param name="InputFile">our initial PDF file</param>
        /// <param name="OutPutFile">result of compression</param>
        /// <param name="CompressValue">how far we go..</param>
        private bool CompressPDF(string InputFile, string OutPutFile, string CompressValue)
        {
            try
            {
                Process proc = new Process();
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.CreateNoWindow = true;
                psi.ErrorDialog = false;
                psi.UseShellExecute = false;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.FileName = string.Concat(Path.GetDirectoryName(Application.ExecutablePath), "\\ghost.exe"); //where's the ghost exe dwells

                string args = "-sDEVICE=pdfwrite -dCompatibilityLevel=1.4" + " -dPDFSETTINGS=/" + CompressValue + " -dNOPAUSE  -dQUIET -dBATCH" + " -sOutputFile=\"" + OutPutFile + "\" " + "\"" + InputFile + "\""; //ghostscript settings

                psi.Arguments = args;

                //start the execution
                proc.StartInfo = psi;

                proc.Start();
                proc.WaitForExit();

                return true;
            }
            catch
            {
                return false;
            }
        }

        string outputDir = string.Empty; //to save compressed PDFs into

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            foreach (string pdffile in listBoxPDFs.Items) //compres everything that listed
            {
                string outp = outputDir + "\\" + Path.GetFileNameWithoutExtension(pdffile) + $" (Сжатый).pdf";

                CompressPDF(pdffile, outp, "screen");

                if (progressBar1.InvokeRequired)
                {
                    progressBar1.Invoke(new Action(() => progressBar1.Increment(1)));
                }
                else
                {
                    progressBar1.Increment(1);
                }
            }
        }

        private void btnCompress_Click(object sender, EventArgs e)
        {
            if (listBoxPDFs.Items.Count == 0)
            {
                MessageBox.Show("Добавьте файлы формата PDF для сжатия!");
            }
            else
            {
                FolderBrowserDialog fdialog = new FolderBrowserDialog();
                if (fdialog.ShowDialog() == DialogResult.OK)
                {
                    outputDir = fdialog.SelectedPath;
                }

                progressBar1.Maximum = listBoxPDFs.Items.Count;

                btnCompress.Enabled = false;
                pictureBox1.Visible = true;

                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            listBoxPDFs.Items.Clear();
            btnCompress.Enabled = true;
            pictureBox1.Visible = false;
            MessageBox.Show("Готово!", "Сжатие PDF файла", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            Application.Exit();
        }
    }
}
