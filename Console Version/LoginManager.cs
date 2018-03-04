using System;
using System.Text;

using RestSharp;
using AngleSharp.Parser.Html;

namespace KTU_AIS_Scraper
{    
    public struct LoginData
    {
        public string StudentName;
        public string StudentId;
        public string StudyProgram;
        public string Username;
        public string Password;    

        public System.Net.CookieContainer Cookies;

        public bool Success;
    }

    public static class LoginManager
    {
        struct AutoLoginResponse
        {
            public string authState;
            public System.Net.CookieContainer cookies;
            public System.Net.HttpStatusCode status;
        }
        struct PostLoginResponse
        {
            public string stateId;
            public System.Net.CookieContainer cookies;
            public System.Net.HttpStatusCode status;
        }
        struct AgreeResponse
        {
            public string samlResponse;
            public string relayState;
            public System.Net.HttpStatusCode status;
        }
        struct AuthResponse
        {
            public System.Net.CookieContainer cookies;
            public System.Net.HttpStatusCode status;
        }

        private static RestClient client;
        private static HtmlParser parser;

        public static LoginData GetAuthCookies(string username, string password)
        {
            client = new RestClient()
            {
                CookieContainer = new System.Net.CookieContainer(),
                Encoding = Encoding.GetEncoding(1257)
            };
            parser = new HtmlParser();

            var autoLogin = GetAutoLogin();
            var postLogin = PostLogin(username, password, autoLogin);

            if (postLogin.stateId != null)
            {
                var agreeResponse = GetAgree(postLogin);
                var postContinue = PostContinue(agreeResponse);
                var login = GetInfo(postContinue, username, password);
                return login;
            }

            return new LoginData() { Success = false };
        }

        private static AutoLoginResponse GetAutoLogin()
        {
            client.BaseUrl = new Uri("https://uais.cr.ktu.lt");

            var request = new RestRequest("ktuis/studautologin", Method.GET);
            IRestResponse response = client.Execute(request);
            var cookies = client.CookieContainer;
            var status = response.StatusCode;

            var document = parser.Parse(response.Content);
            var select = document.QuerySelector("input[name=\"AuthState\"]").GetAttribute("value");

            return new AutoLoginResponse()
            {
                authState = select,
                cookies = cookies,
                status = status
            };
        }

        private static PostLoginResponse PostLogin(string username, string password, AutoLoginResponse autoLogin)
        {
            client.BaseUrl = new Uri("https://login.ktu.lt/");

            var request = new RestRequest("simplesaml/module.php/core/loginuserpass.php", Method.POST);
            request.AddParameter("username", username);
            request.AddParameter("password", password);
            request.AddParameter("AuthState", autoLogin.authState);

            IRestResponse response = client.Execute(request);
            var document = parser.Parse(response.Content);

            var select = document.QuerySelector("input[name=\"StateId\"]");

            if (select == null)
            {
                Console.WriteLine("Login failure"); // NOTE: Console outputs left
                return new PostLoginResponse() { stateId = null };
            }

            Console.WriteLine("Login success!");

            return new PostLoginResponse()
            {
                stateId = select.GetAttribute("value"),
                cookies = client.CookieContainer, // Same as autologin cookies, so useless
                status = response.StatusCode
            };
        }

        private static AgreeResponse GetAgree(PostLoginResponse postLogin)
        {
            client.BaseUrl = new Uri("https://login.ktu.lt/");

            string url = "simplesaml/module.php/consent/getconsent.php?" +
                          String.Format("StateId={0}&", postLogin.stateId) +
                         "yes=Yes%2C%20continue%0D%0A&" +
                         "saveconsent=1";

            var request = new RestRequest(url, Method.GET);

            IRestResponse response = client.Execute(request);
            var document = parser.Parse(response.Content);

            var samlResponse = document.QuerySelector("input[name=\"SAMLResponse\"]").GetAttribute("value");
            var relayState = document.QuerySelector("input[name=\"RelayState\"]").GetAttribute("value");

            return new AgreeResponse()
            {
                relayState = relayState,
                samlResponse = samlResponse,
                status = response.StatusCode
            };
        }

        private static AuthResponse PostContinue(AgreeResponse agreeResponse)
        {
            client.CookieContainer = new System.Net.CookieContainer();
            client.BaseUrl = new Uri("https://uais.cr.ktu.lt");

            var request = new RestRequest("shibboleth/SAML2/POST", Method.POST);
            request.AddParameter("SAMLResponse", agreeResponse.samlResponse);
            request.AddParameter("RelayState", agreeResponse.relayState);

            IRestResponse response = client.Execute(request);

            return new AuthResponse()
            {
                cookies = client.CookieContainer,
                status = response.StatusCode
            };
        }

        private static LoginData GetInfo(AuthResponse authResponse, string username, string password)
        {
            client.BaseUrl = new Uri("https://uais.cr.ktu.lt");

            var request = new RestRequest("ktuis/vs.ind_planas", Method.GET);

            IRestResponse response = client.Execute(request);
            var document = parser.Parse(response.Content);

            var nameItem = document.QuerySelector("#ais_lang_link_lt").Parent.TextContent.Split(' ');

            string studentID = nameItem[0];
            string studentName = nameItem[1] + " " + nameItem[2]; // NOTE: May be bad encoding

            string study = document.QuerySelector(".ind-lst.unstyled > li > a").GetAttribute("href").Split('?')[1];


            return new LoginData()
            {
                StudentId = studentID,
                StudentName = studentName,
                StudyProgram = study,
                Username = username,
                Password = password,
                Cookies = authResponse.cookies,
                Success = true
            };
        }

    }
}
