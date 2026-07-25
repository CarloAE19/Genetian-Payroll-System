using System.Windows;
using GB_Payroll_System.Data;

namespace GB_Payroll_System
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DatabaseInitializer.Initialize();
        }
    }
}
