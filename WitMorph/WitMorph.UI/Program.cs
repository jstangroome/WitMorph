using System;
using System.Windows.Forms;

namespace WitMorph.UI
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var mainForm = new HubForm();
            var presenter = new HubPresenter(mainForm);
            Application.Run(mainForm);
        }
    }
}
