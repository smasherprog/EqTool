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
            var spells = ParseSpells.GetSpells();
            stopwatch.Stop();
            SpellListView.Columns.Clear();
            _ = SpellListView.Columns.Add("id");
            _ = SpellListView.Columns.Add("name");
            _ = SpellListView.Columns.Add("cast_on_you");

            _ = SpellListView.Columns.Add("cast_on_you");
            _ = SpellListView.Columns.Add("cast_on_other");
            _ = SpellListView.Columns.Add("buffduration");
            _ = SpellListView.Columns.Add("buffdurationformula");

            var itemstoadd = new List<ListViewItem>();
            foreach (var item in spells.SpellList)
            {
                var i = new ListViewItem();
                _ = i.SubItems.Add(item.id.ToString());
                _ = i.SubItems.Add(item.name);
                _ = i.SubItems.Add(item.cast_on_you);
                _ = i.SubItems.Add(item.cast_on_other);
                _ = i.SubItems.Add(item.buffduration.ToString());
                _ = i.SubItems.Add(item.buffdurationformula.ToString());
                itemstoadd.Add(i);
            }
            SpellListView.Items.AddRange(itemstoadd.ToArray());
            Debug.WriteLine($"took {stopwatch.ElapsedMilliseconds} ms to load {spells.SpellList.Count}");
        }
    }
}