using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using static PerformanceReportXLineNoti.APISv;

namespace PerformanceReportXLineNoti
{
    class Program
    {
        static List<string> teams = new List<string>{"Arsenal", "Aston Villa", "Brentford", "Brighton", "Burnley", "Chelsea", "Crystal Palace",
            "Everton", "Leeds", "Leicester", "Liverpool", "Man City", "Man Utd", "Newcastle", "Norwich City", "Southampton", "Tottenham", "Watford", "West Ham", "Wolves" };

        static APISv _APISv = new APISv();
        static SystemConfig _SystemConfig = new SystemConfig();
        static DateTime beginko;
        static List<DateTime> MatchDay = new List<DateTime>();
        static DateTime curdate;
        static void Main(string[] args)
        {
            LoadMatchDay();
        }
        private static void LoadMatchDay()
        {
            SetTimer();
            LoadConfig();
            // curdate = DateTime.Now.AddHours(35).AddMinutes(30);
            curdate = DateTime.Now;
            var curmacth = DateTime.Now;
          
            var rs = _APISv.GetFixtures();

            var rs2 = rs.Where(a => Convert.ToDateTime(a.kickoff_time).Date >= curdate).Take(1).ToList();

            //var c = Convert.ToDateTime(rs2[0].kickoff_time);

            foreach (var item in rs2)
            {
                var d = Convert.ToDateTime(item.kickoff_time);

                var h = teams[item.team_h - 1];
                var a = teams[item.team_a - 1];
                beginko = d.AddMinutes(-15);

                var msg = d + " : " + h + " vs " + a;
                msg += Environment.NewLine;
                if (d > curmacth)
                {
                    msg += "rp1 : begin ko : " + beginko.ToString();
                    msg += Environment.NewLine;
                    MatchDay.Add(beginko);
                    msg += "rp2 : " + beginko.AddMinutes(5).ToString();
                    msg += Environment.NewLine;
                    MatchDay.Add(beginko.AddMinutes(5));
                    msg += "rp3 : " + beginko.AddMinutes(10).ToString();
                    msg += Environment.NewLine;
                    MatchDay.Add(beginko.AddMinutes(10));
                    msg += "rp4" + " : " + d.ToString();
                    msg += Environment.NewLine;
                    MatchDay.Add(d);

                    for (int i = 1; i < 8; i++)
                    {
                        msg += "rp" + (i + 4) + " : " + d.AddMinutes(15 * i).ToString();
                        msg += Environment.NewLine;
                        MatchDay.Add(d.AddMinutes(15 * i));
                    }
                    curmacth = d;
                }
                Console.WriteLine(msg);

                if (_SystemConfig.IsNotifyLine)
                {
                    _APISv.NotiLine(msg, _SystemConfig.LineNotiToken);
                }

            }
            var ss = _APISv.GetArrays();
            var x = ss.items.Where(a => _SystemConfig.PureArray.Contains(a.name));

            Console.ReadLine();
        }
        private static void LoadConfig()
        {
            using (StreamReader r = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "SystemConfig.json"))
            {
                string json = r.ReadToEnd();
                _SystemConfig = JsonConvert.DeserializeObject<SystemConfig>(json);
            }
        }

        private static System.Timers.Timer aTimer;

        private static void SetTimer()
        {
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(6000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            DateTime datenow = DateTime.Today;
            //DateTime date1 = new DateTime(datenow.Year, datenow.Month, datenow.Day, 6, 0, 0);
            //DateTime date2 = new DateTime(datenow.Year, datenow.Month, datenow.Day, 3, 45, 0);

            //curdate = DateTime.Now.AddHours(35).AddMinutes(30);
            DateTime MacthNow = new DateTime(curdate.Year, curdate.Month, curdate.Day, curdate.Hour, curdate.Minute, 0);
            //DateTime MacthNow = new DateTime(_TimeNow.Year, _TimeNow.Month, _TimeNow.Day, _TimeNow.Hour, _TimeNow.Minute, 0);
           
            if (MatchDay.Contains(MacthNow))           
            {
                aTimer.Stop();
                aTimer = new System.Timers.Timer(60000);
                aTimer.Elapsed += OnTimedEvent;
                aTimer.AutoReset = true;
                aTimer.Enabled = true;
                aTimer.Start();
                Console.WriteLine("Report : "+ curdate.ToString());
                var MaxTimeMatchDay = MatchDay.OrderByDescending(o => o).FirstOrDefault();
                if(MaxTimeMatchDay == MacthNow)
                {
                    Console.WriteLine("End Match");
                    LoadMatchDay();
                }
            }
        }
        public class SystemConfig
        {
            public int BeginKickOff { get; set; }
            public int EveryBeginKickOff { get; set; }
            public int OnKickOff { get; set; }
            public int AfterMatchStop { get; set; }
            public bool IsNotifyLine { get; set; }
            public bool IsNotifyLineWithImg { get; set; }
            public string LineNotiToken { get; set; }
            public List<string> PureArray { get; set; }
            
        }
    }
}
