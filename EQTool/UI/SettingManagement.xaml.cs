using EQTool.Models;
using EQToolShared.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EQTool.UI
{
    /// <summary>
    /// Interaction logic for SettingManagement.xaml
    /// </summary>
    public partial class SettingManagement : Window
    {
        private readonly EQToolSettings settings;

        public SettingManagement(EQToolSettings settings)
        {
            this.settings = settings;
            InitializeComponent();
            var persons = new List<Person>();
            foreach (var item in Enum.GetValues(typeof(Servers)).Cast<Servers>().Where(a => a != Servers.MaxServers && a != Servers.Quarm).ToList())
            {
                var players = settings.Players.Where(a => a.Server == item).ToList();
                var serv = new Person
                {
                    Children = new ObservableCollection<Person>(),
                    Class = string.Empty,
                    Name = item.ToString()
                };
                persons.Add(serv);
                serv.Children.Add(new Person
                {
                    Name = "Global",
                    Class = string.Empty,
                    Children = new ObservableCollection<Person>()
                }); 
                serv.Children.Add(new Person
                {
                    Name = "Zone(s)",
                    Class = string.Empty,
                    Children = new ObservableCollection<Person>()
                });
                foreach (var p in players)
                {
                    serv.Children.Add(new Person
                    {
                        Name = p.Name,
                        Class = p.PlayerClass.ToString(),
                        Children = new ObservableCollection<Person>()
                    });
                }
            }

            trvPersons.ItemsSource = persons;
        }

        private void btnSelectNext_Click(object sender, RoutedEventArgs e)
        {
            if (trvPersons.SelectedItem != null)
            {
                var list = (trvPersons.ItemsSource as List<Person>);
                int curIndex = list.IndexOf(trvPersons.SelectedItem as Person);
                if (curIndex >= 0)
                    curIndex++;
                if (curIndex >= list.Count)
                    curIndex = 0;
                if (curIndex >= 0)
                    list[curIndex].IsSelected = true;
            }
        }

        private void btnToggleExpansion_Click(object sender, RoutedEventArgs e)
        {
            if (trvPersons.SelectedItem != null)
                (trvPersons.SelectedItem as Person).IsExpanded = !(trvPersons.SelectedItem as Person).IsExpanded;
        }



    }

    public class Person : TreeViewItemBase
    {
        public Person()
        {
            this.Children = new ObservableCollection<Person>();
        }

        public string Name { get; set; }

        public string Class { get; set; }

        public ObservableCollection<Person> Children { get; set; }
    }

    public class TreeViewItemBase : INotifyPropertyChanged
    {
        private bool isSelected;
        public bool IsSelected
        {
            get { return this.isSelected; }
            set
            {
                if (value != this.isSelected)
                {
                    this.isSelected = value;
                    NotifyPropertyChanged("IsSelected");
                }
            }
        }

        private bool isExpanded;
        public bool IsExpanded
        {
            get { return this.isExpanded; }
            set
            {
                if (value != this.isExpanded)
                {
                    this.isExpanded = value;
                    NotifyPropertyChanged("IsExpanded");
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
