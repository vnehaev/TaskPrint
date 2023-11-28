using System;
using System.Windows.Forms;
using TaskPrint.Models.Wildberries;

namespace TaskPrint
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        [STAThread]
        static void Main()
        {
            AppSettings appSettings = new AppSettings();

            Company Company1 = new Company();
            Company1.Name = "ООО Вектор";
            Company1.Id = "85385";
            Company1.ApiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhY2Nlc3NJRCI6ImM1YmYzMGY2LTBiMTUtNDQ5My1hNDhiLTkyYzNlYTA3ZmRkOCJ9.71YlmoQopYuGL4JbqfA39iywEenqxBoUXU1sOuiEg4M";

          
            appSettings.AddCompany(Company1);
            appSettings.SetSelectedCompany(Company1);
            

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(appSettings));
        }
    }
}
