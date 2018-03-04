using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RestSharp;
using AngleSharp.Parser.Html;
using RestSharp.Authenticators;
using AngleSharp.Dom.Html;
using System.IO;

namespace KTU_AIS_Scraper
{
    //NOTE: Two solutions for the tray application
    // https://www.codeproject.com/Articles/290013/Formless-System-Tray-Application
    // https://www.codeproject.com/Articles/36468/WPF-NotifyIcon

    class Program
    {       
        private static RestClient client;
        private static HtmlParser parser;

        private static readonly string passwordFileName = "password.txt";

        static void Main(string[] args)
        {
            string username = null
                 , password = null;

            using(StreamReader reader = new StreamReader(passwordFileName))
            {
                username = reader.ReadLine();
                password = reader.ReadLine();
            }

            LoginData login = LoginManager.GetAuthCookies(username, password);

            client = new RestClient();
            parser = new HtmlParser();

            var modules = GetModules(login);

            string filename = @"..\..\Logs\" + 
                              DateTime.Now.ToString().Replace('/', ',').Replace(':', ';') + 
                              ".txt";

            var files = Directory.GetFiles(@"..\..\Logs\").ToList();
            files.Sort();
            string latestLog = files.Last();

            using (StreamWriter writer = new StreamWriter(filename))
            {
                foreach (Module module in modules)
                {
                    string line = String.Format("{0} : {1:X}",
                        module.ModuleName,
                        GetMarkHtmlByModule(module, login).GetHashCode());

                    Console.WriteLine(line);
                    writer.WriteLine(line);
                }
            }

            Console.WriteLine("\nNew marks : {0}",
                   File.ReadAllText(latestLog).GetHashCode()
                != File.ReadAllText(filename).GetHashCode());
        }

        static readonly string unauthorizedRequestButtonValue = "Prisijungimas";
        static readonly string unauthorizedRequestPostAction = "/ktuis/stp_prisijungimas";

        static bool IsResponseAuthError(IHtmlDocument document)
        {
            var button = document.QuerySelector("input");
            var form = document.QuerySelector("form");

            bool buttonValMatch = false
               , formValMatch = false;

            if (button != null)
            {
                buttonValMatch = button.GetAttribute("value").Equals(unauthorizedRequestButtonValue);
            }
            if (form != null)
            {
                formValMatch = form.GetAttribute("action").Equals(unauthorizedRequestPostAction);
            }

            return buttonValMatch && formValMatch;
        } 

        static List<Module> GetModules(LoginData login)
        {
            client.BaseUrl = new Uri("https://uais.cr.ktu.lt");
            client.CookieContainer = login.Cookies;

            var request = new RestRequest("ktuis/STUD_SS2.planas_busenos?" + login.StudyProgram, Method.GET);

            IRestResponse response = client.Execute(request);
            var document = parser.Parse(response.Content);

            if (IsResponseAuthError(document))
            {
                throw AuthenticationException("Request not authenticated!");
            }

            List<Module> modules = new List<Module>();

            try
            {
                foreach (var moduleElement in document.QuerySelectorAll("table.table-hover > tbody > tr"))
                {
                    modules.Add(new Module(moduleElement));
                }
            }
            catch
            {
                throw ScrapeException("Module scraping error!");
            }

            return modules;
        }

        static List<Mark> GetMarksByModules()
        {
            List<Mark> marks = new List<Mark>();

            return marks;
        }

        static string GetMarkHtmlByModule(Module module, LoginData login)
        {
            client.BaseUrl = new Uri("https://uais.cr.ktu.lt");
            client.CookieContainer = login.Cookies;

            var request = new RestRequest("ktuis/STUD_SS2.infivert", Method.POST);
            request.AddParameter("p1", module.p1);
            request.AddParameter("p2", module.p2);

            IRestResponse response = client.Execute(request);
            var document = parser.Parse(response.Content);

            var select = document.QuerySelector(".d_grd2[style=\"border-collapse:collapse; empty-cells:hide;\"]");

            //Now a little more specific:
            return select.Children[1].Children[2].OuterHtml;
        }

        private static Exception ScrapeException(string v)
        {
            throw new NotImplementedException();
        }

        private static Exception AuthenticationException(string v)
        {
            throw new NotImplementedException();
        }
    }
}
