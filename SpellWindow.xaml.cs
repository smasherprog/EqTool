using EQTool.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace EQTool
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class SpellWindow : Window
    {
        public ObservableCollection<Spell> SpellList = new ObservableCollection<Spell>();
        public SpellWindow()
        {
            InitializeComponent();
            spelllistview.ItemsSource = SpellList;
        }
    }
}
