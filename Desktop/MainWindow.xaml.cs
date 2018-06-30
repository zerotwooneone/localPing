using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using Desktop.Target;

namespace Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewmodel mainWindowViewmodel)
        {
            DataContext = mainWindowViewmodel;
            InitializeComponent();
            mainWindowViewmodel.ResortObservable.Subscribe(t =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    CollectionViewSource.GetDefaultView(TargetListView.ItemsSource).Refresh();
                }, DispatcherPriority.ApplicationIdle);
                
            });
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(TargetListView.ItemsSource).Filter = TargetFilter;
        }

        private bool TargetFilter(object obj)
        {
            var target = (TargetDatamodel) obj;
            return target.ShowUntil.HasValue || target.Equals(TargetListView.SelectedItem);
        }
    }
}
