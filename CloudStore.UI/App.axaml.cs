using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CloudStore.UI.ViewModels;
using CloudStore.UI.Views;

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
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var vm = new LoginAndRegistrationViewModel();
                var mwvm = new MainWindowViewModel(null);
                var win = desktop.MainWindow = new LoginAndRegistrationWindow
                {
                    DataContext = vm,
                };
               
                vm.Closed += (s, e) =>
                {
                    mwvm.User = vm.User;
                    win.IsVisible = false;
                    
                    win = desktop.MainWindow = new MainWindow
                    {
                        DataContext = mwvm
                    };
                    win.IsVisible = true;
                };
                mwvm.Closed += (s, e) =>
                { 
                    if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
                    {
                        desktopApp.Shutdown();
                    }
                    else if (Current?.ApplicationLifetime is ISingleViewApplicationLifetime viewApp)
                    {
                        viewApp.MainView = null;
                    }
                    win.Close();
                };
               

            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}