using System.Collections.Generic;
using System.IO;
using TaskPrint.Models.Wildberries;

namespace TaskPrint
{
    public class AppSettings
    {
        public List<Company> companies = new List<Company>();
        public Company selectedCompany = new Company();


        public void AddCompany(Company company)
        {
            companies.Add(company);
        }

        public List<Company> GetAllCompanies()
        {
           return companies;
        }

        public void SetSelectedCompany(Company company)
        {
            selectedCompany = company;
        }
        
        public Company GetSelectedCompany()
        {
            return selectedCompany;
        }   
    }
}
