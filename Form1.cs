using EqTool.Services;
using System.Diagnostics;

namespace EqTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _ = ParseSpells.GetSpells();
            stopwatch.Stop();

            Debug.WriteLine($"took {stopwatch.ElapsedMilliseconds} ms to load");
        }
    }
}