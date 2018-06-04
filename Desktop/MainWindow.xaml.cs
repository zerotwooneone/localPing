using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Data;

namespace Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CollectionViewSource _targetViewSource;

        public MainWindow(MainWindowViewmodel mainWindowViewmodel)
        {
            DataContext = mainWindowViewmodel;
            InitializeComponent();
            _targetViewSource = (CollectionViewSource)Resources["TargetView"];
            mainWindowViewmodel.ResortObservable.Subscribe(t =>
            {
                Dispatcher.Invoke(() =>
                {
                    _targetViewSource.View.Refresh();
                });
                
            });

        }
    }
}
