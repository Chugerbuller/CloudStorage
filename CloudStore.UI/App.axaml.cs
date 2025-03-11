using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CloudStore.BL.Models;
using CloudStore.UI.Configs;
using CloudStore.UI.ViewModels;
using CloudStore.UI.Views;
using System.IO;
using System.Text.Json;

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
                var appCfg = JsonSerializer.Deserialize<ApplicationConfig>(File.ReadAllText("Configs\\ApplicationConfig.json"));

                if (appCfg.RememberUser)
                {
                    var user = JsonSerializer.Deserialize<User>(File.ReadAllText("Configs\\UserConfig.json"));
                    var vm = new MainWindowViewModel(user);
                    var win = desktop.MainWindow = new MainWindow
                    {
                        DataContext = vm,
                    };
                    vm.Closed += (s, e) =>
                    {
                        win.Close();
                    };
                }
                else
                {
                    var vm = new LoginAndRegistrationViewModel();

                    var win = desktop.MainWindow = new LoginAndRegistrationWindow
                    {
                        DataContext = vm,
                    };

                    vm.Closed += (s, e) =>
                    {
                        var mwvm = new MainWindowViewModel(vm.User);
                        win.Hide();

                        win = desktop.MainWindow = new MainWindow
                        {
                            DataContext = mwvm
                        };
                        win.Show();
                        mwvm.Closed += (s, e) => win.Close();
                    };
                }
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}