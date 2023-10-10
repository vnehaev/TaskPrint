using System.IO;

namespace TaskPrint
{
    class AppSettings
    {
        private string apiKeyFilePath = "api_key.txt";
        private string companyName = "ООО Вектор";
        private string companyId = "85385";


        public string GetApiKey()
        {
            if (File.Exists(apiKeyFilePath))
            {
                return File.ReadAllText(apiKeyFilePath);
            }
            else
            {
                return null;
            }
        }

        public void SetApiKey(string apiKey)
        {
            File.WriteAllText(apiKeyFilePath, apiKey);
        }

        public string GetCompanyName()
        {
            return companyName;
        }

        public string GetCompanyId()
        {
            return companyId;
        }
    }
}
