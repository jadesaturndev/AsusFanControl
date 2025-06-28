using System.Windows;

namespace AsusFanControlGUI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Set the shutdown mode to close when the main window closes
            ShutdownMode = ShutdownMode.OnMainWindowClose;
        }
    }
}