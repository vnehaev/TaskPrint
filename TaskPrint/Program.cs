using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

            string filePath = "license.txt";
            string fileContent = File.ReadAllText(filePath);
            byte[] base64DecodedBytes = Convert.FromBase64String(fileContent);
            string decryptedText = Encoding.UTF8.GetString(base64DecodedBytes);
            string[] companyStrings = decryptedText.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            List<Company> companies = new List<Company>();
            foreach (string companyString in companyStrings)
            {
                string[] lines = companyString.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                int id = 0;
                string name = "";
                string api = "";

                foreach (string line in lines)
                {
                    string[] parts = line.Split(new char[] { '-' }, 2);
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();

                        if (key.Equals("ID", StringComparison.OrdinalIgnoreCase))
                        {
                            int.TryParse(value, out id);
                        }
                        else if (key.Equals("Name", StringComparison.OrdinalIgnoreCase))
                        {
                            name = value;
                        }
                        else if (key.Equals("API", StringComparison.OrdinalIgnoreCase))
                        {
                            api = value;
                        }
                    }
                }

                Company company = new Company
                {
                    Name = name,
                    Id = id.ToString(),
                    ApiKey = api
                };

                companies.Add(company);

            }

            AppSettings appSettings = new AppSettings();
           
          
            appSettings.companies = companies;
            appSettings.SetSelectedCompany(companies[0]);
            

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(appSettings));
        }
    }
}
