using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using CloudStore.UI.ViewModels;
using CloudStore.UI.Views;
using System.Diagnostics;

namespace CloudStore.UI
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            //FIX ME
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var vm = new LoginAndRegistrationViewModel();

                var win = desktop.MainWindow = new LoginAndRegistrationWindow
                {
                    DataContext = vm,
                };

                vm.Closed += (s, e) =>
                {
                    Debug.WriteLine("start main Window");
                    var mwvm = new MainWindowViewModel(vm.User);
                    win.Hide();

                    win = desktop.MainWindow = new MainWindow
                    {
                        DataContext = mwvm
                    };
                    win.Show();
                };
               
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}