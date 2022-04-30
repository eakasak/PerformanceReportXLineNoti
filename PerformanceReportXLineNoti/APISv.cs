using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using static PerformanceReportXLineNoti.Program;

namespace PerformanceReportXLineNoti
{
    public class APISv
    {
          public object NotiLine(string pMessage, string pAuthorization = "whVvZh1oelDBO1TpGlPw0rhyorbZZpehmFIH7EDO5I6")
        {
            //  client.Timeout = -1;

            var request = new RestRequest();
            request.RequestFormat = DataFormat.Json;
            request.Method = Method.Post;
            var client = new RestClient("https://notify-api.line.me/api/notify?message=" + pMessage);
            request.AddHeader("Authorization", "Bearer " + pAuthorization);
            request.AddHeader("Accept", "application/json");
            //request.AddFile("imageFile", "D://Image.png");
            var response = client.ExecuteAsync(request).Result;
            //Console.WriteLine(response.Content);
            return response.Content;
        }
        public object NotiLine(string pMessage, string imageFile, string pAuthorization = "whVvZh1oelDBO1TpGlPw0rhyorbZZpehmFIH7EDO5I6")
        {
            //  client.Timeout = -1;

            var request = new RestRequest();
            request.RequestFormat = DataFormat.Json;
            request.Method = Method.Post;
            var client = new RestClient("https://notify-api.line.me/api/notify?message=" + pMessage);
            request.AddHeader("Authorization", "Bearer " + pAuthorization);
            request.AddHeader("Accept", "application/json");
            request.AddFile("imageFile", imageFile);
            var response = client.ExecuteAsync(request).Result;
            //Console.WriteLine(response.Content);
            return response.Content;
        }
        public List<Fixture> GetFixtures()
        {
            var client = new RestClient("https://fantasy.premierleague.com/api/fixtures/");

            var request = new RestRequest();
            request.RequestFormat = DataFormat.Json;
            request.Method = Method.Get;
            var response = client.ExecuteAsync(request).Result;
            //Console.WriteLine(response.Content);
            var rs  = JsonConvert.DeserializeObject<List<Fixture>>(response.Content);
            return rs;

        }
        public string Gettoken()
        {
            var client = new RestClient("https://api.pure1.purestorage.com/oauth2/1.0/token");

            var request = new RestRequest();
            request.RequestFormat = DataFormat.Json;
            request.Method = Method.Post;
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddHeader("Authorization", "Basic ZWFrYXNhay5waWI6Ykd2aE1wMlhXa3RrVzY2aGtjalYjJCU=");
            request.AddParameter("grant_type", "urn:ietf:params:oauth:grant-type:token-exchange");
            request.AddParameter("subject_token", "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJwdXJlMTphcGlrZXk6ZVhjR3B1b0dpYzg4Q1lZYyIsImlhdCI6MTU0NjYyMDYxMzAyOCwiZXhwIjoxNTU2NjIwNjEzMDI4fQ.nDL6XHR8NqzuoHjSNLdcblkFUREZm2Z6Ki1pHl-zINwr7SAB-YqI6u0R5ETcXEed0z6cdu5UnvdhdAAJW5DSNJhTXT4q_3eCHn71Ruq6aHkIWuUz7ZdMYITktl4tyYpkBiCG6PClFWxARfJ5W0o10CTrhY4iKthdQH0UC4S_LQkzySARyFqu36S8tGIjKgv046tCHx2F4CAVoX38v5nEeYPYhyOz0rXMjKUOwPfNfhGqnCOjFGzCxGjzahZhrFk0B6aDEiOXUKLdxzcJiKefcxGaO3fdhJiSuD4RLYN9mWTVuRCS6OJyeaNkLwcDdOznjEA2_s6O1ELVSsMarwtqOA");
            request.AddParameter("subject_token_type", "urn:ietf:params:oauth:token-type:jwt");
            var response = client.ExecuteAsync(request).Result;
            //Console.WriteLine(response.Content);
            var rs = JsonConvert.DeserializeObject<AccessToken>(response.Content);
            return rs.access_token;

        }

        public PureArray GetArrays()
        {
            var client = new RestClient("https://api.pure1.purestorage.com/api/1.0/arrays");

            var request = new RestRequest();
            request.RequestFormat = DataFormat.Json;
            request.Method = Method.Get;
            request.AddHeader("Authorization", "Bearer " + Gettoken());
            var response = client.ExecuteAsync(request).Result;
            PureArray myDeserializedClass = JsonConvert.DeserializeObject<PureArray>(response.Content);
            var rs = new List<PureItem>();
            rs = myDeserializedClass.items;
            return myDeserializedClass;

        }

        public PureValume GetVolumes()
        {
            var client = new RestClient("https://api.pure1.purestorage.com/api/1.0/volumes");

            var request = new RestRequest();
            request.RequestFormat = DataFormat.Json;
            request.Method = Method.Get;
            request.AddHeader("Authorization", "Bearer " + Gettoken());
            var response = client.ExecuteAsync(request).Result;

            PureValume myDeserializedClass = JsonConvert.DeserializeObject<PureValume>(response.Content);
            var rs = new List<ValumeItem>();
            rs = myDeserializedClass.items;
            return myDeserializedClass;

        }
        public class AccessToken
        {
            public string access_token { get; set; }
            public string issued_token_type { get; set; }
            public string token_type { get; set; }
            public int expires_in { get; set; }
        }
        public class Fixture
        {
            public int code { get; set; }
            public object @event { get; set; }
            public bool finished { get; set; }
            public bool finished_provisional { get; set; }
            public int id { get; set; }
            public string kickoff_time { get; set; }
            public int minutes { get; set; }
            public bool provisional_start_time { get; set; }
            public object started { get; set; }
            public int team_a { get; set; }
            public object team_a_score { get; set; }
            public int team_h { get; set; }
            public object team_h_score { get; set; }
            public List<object> stats { get; set; }
            public int team_h_difficulty { get; set; }
            public int team_a_difficulty { get; set; }
            public int pulse_id { get; set; }
        }
        public class PureItem
        {
            public string id { get; set; }
            public string name { get; set; }
            public string model { get; set; }
            public string os { get; set; }
            public string version { get; set; }
            public object _as_of { get; set; }
        }

        public class PureArray
        {
            public int total_item_count { get; set; }
            public object continuation_token { get; set; }
            public List<PureItem> items { get; set; }
        }
        public class ValumeArray
        {
            public string id { get; set; }
            public string name { get; set; }
            public string resource_type { get; set; }
        }

        public class Pod
        {
            public string id { get; set; }
            public string name { get; set; }
            public string resource_type { get; set; }
        }

        public class ValumeItem
        {
            public string id { get; set; }
            public string name { get; set; }
            public List<ValumeArray> arrays { get; set; }
            public object created { get; set; }
            public bool destroyed { get; set; }
            public bool eradicated { get; set; }
            public Pod pod { get; set; }
            public object provisioned { get; set; }
            public string serial { get; set; }
            public object source { get; set; }
            public object _as_of { get; set; }
        }

        public class PureValume
        {
            public int total_item_count { get; set; }
            public object continuation_token { get; set; }
            public List<ValumeItem> items { get; set; }
        }

    }
}
