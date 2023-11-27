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
            Company Vector = new Company();
            Vector.Name = "ООО Вектор";
            Vector.Id = "85385";
            Vector.ApiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhY2Nlc3NJRCI6ImM1YmYzMGY2LTBiMTUtNDQ5My1hNDhiLTkyYzNlYTA3ZmRkOCJ9.71YlmoQopYuGL4JbqfA39iywEenqxBoUXU1sOuiEg4M";

            Company Vector1 = new Company();
            Vector1.Name = "ООО Вектор-1";
            Vector1.Id = "85385";
            Vector1.ApiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhY2Nlc3NJRCI6ImM1YmYzMGY2LTBiMTUtNDQ5My1hNDhiLTkyYzNlYTA3ZmRkOCJ9.71YlmoQopYuGL4JbqfA39iywEenqxBoUXU1sOuiEg4M";

            appSettings.AddCompany(Vector);
            appSettings.AddCompany(Vector1);
            appSettings.SetSelectedCompany(Vector);
            

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(appSettings));
        }
    }
}
