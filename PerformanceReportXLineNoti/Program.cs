using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Timers;
using static PerformanceReportXLineNoti.APISv;

namespace PerformanceReportXLineNoti
{
   class Program
    {
        static List<string> teams = new List<string>{"Arsenal", "Aston Villa", "Brentford", "Brighton", "Burnley", "Chelsea", "Crystal Palace",
            "Everton", "Leicester", "Leeds", "Liverpool", "Man City", "Man Utd", "Newcastle", "Norwich City", "Southampton", "Tottenham", "Watford", "West Ham", "Wolves" };

        static APISv _APISv = new APISv();
        static SystemConfig _SystemConfig = new SystemConfig();
        static DateTime beginko;
        static List<DateTime> MatchDay = new List<DateTime>();
        static DateTime curdate;
        static void Main(string[] args)
        {
            var testmsg = "";
            testmsg = "Apitoken => " + _APISv.Apitoken();
            _APISv.NotiLine(testmsg, _SystemConfig.LineNotiToken);
            _APISv.APISession();
           var test =   _APISv.GetArrayMonitor();
            testmsg += test.input_per_sec.ToString();
            _APISv.NotiLine(testmsg, _SystemConfig.LineNotiToken);
            //SetTimer();
            //LoadConfig();
            //LoadMatchDay();
        }
        private static void LoadMatchDay()
        {
       
            // curdate = DateTime.Now.AddHours(35).AddMinutes(30);
            curdate = DateTime.Now;
            var curmacth = DateTime.Now;
          
            var rs = _APISv.GetFixtures();

            var rs1 = rs.Where(a => Convert.ToDateTime(a.kickoff_time) >= Convert.ToDateTime(curdate.AddHours(-1))).FirstOrDefault();
            var rs2 = rs.Where(a => Convert.ToDateTime(a.kickoff_time) == Convert.ToDateTime(rs1.kickoff_time)).ToList();

            MatchDay = new List<DateTime>();
            foreach (var item in rs2)
            {
                var d = Convert.ToDateTime(item.kickoff_time);

                var h = teams[item.team_h - 1];
                var a = teams[item.team_a - 1];
                beginko = d.AddMinutes(-_SystemConfig.BeginKickOff);

                var msg = d + " : " + h + " vs " + a;
                msg += Environment.NewLine;
                if (true)
                {
                    msg += "rp1 : begin ko : " + beginko.ToString();
                    msg += Environment.NewLine;
                    MatchDay.Add(beginko);
                    msg += "rp2 : " + beginko.AddMinutes(_SystemConfig.EveryBeginKickOff*1).ToString();
                    msg += Environment.NewLine;
                    MatchDay.Add(beginko.AddMinutes(_SystemConfig.EveryBeginKickOff * 1));
                    msg += "rp3 : " + beginko.AddMinutes(_SystemConfig.EveryBeginKickOff * 2).ToString();
                    msg += Environment.NewLine;
                    MatchDay.Add(beginko.AddMinutes(_SystemConfig.EveryBeginKickOff * 2));
                    msg += "rp4" + " : " + d.ToString();
                    msg += Environment.NewLine;
                    MatchDay.Add(d);

                    for (int i = 1; i < 8; i++)
                    {
                        msg += "rp" + (i + 4) + " : " + d.AddMinutes(_SystemConfig.OnKickOff * i).ToString();
                        msg += Environment.NewLine;
                        MatchDay.Add(d.AddMinutes(15 * i));
                    }
                    curmacth = d;
                }
                if(!_SystemConfig.IsDevelop)
                {
                     msg = d + " : " + h + " vs " + a;
                }
                Console.WriteLine(msg);

                if (_SystemConfig.IsNotifyLine)
                {
                    _APISv.NotiLine(msg, _SystemConfig.LineNotiToken);
                }

            }
   
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

            curdate = DateTime.Now;
            DateTime MacthNow = new DateTime(curdate.Year, curdate.Month, curdate.Day, curdate.Hour, curdate.Minute, 0);

            if (MatchDay.Contains(MacthNow))
            {
                var bftime = MacthNow.AddHours(-2);
                TimeSpan t = bftime.ToUniversalTime() - new DateTime(1970, 1, 1);
                int secondsSinceEpoch = (int)t.TotalSeconds;
                
                TimeSpan t2 = MacthNow.ToUniversalTime() - new DateTime(1970, 1, 1);
                int endsecondsSinceEpoch = (int)t2.TotalSeconds;

                aTimer.Stop();
                aTimer = new System.Timers.Timer(60000);
                aTimer.Elapsed += OnTimedEvent;
                aTimer.AutoReset = true;
                aTimer.Enabled = true;
                aTimer.Start();
    
                var Arrays = _APISv.GetArrays();
                var SelectArrays = Arrays.items.Where(a => _SystemConfig.PureArray.Contains(a.name));
                Console.WriteLine("Report : " + curdate.ToString() );
                //var Valumes = _APISv.GetVolumes();
                //var SelectValume = Valumes.items;
                var Metrics = _APISv.GetMetricsHistory(secondsSinceEpoch, endsecondsSinceEpoch);           
                var Bandwidths = _APISv.GetMetricsBandwidth(secondsSinceEpoch, endsecondsSinceEpoch);
                var Total_loads = _APISv.GetMetricsTotalLoad(secondsSinceEpoch, endsecondsSinceEpoch);

                foreach (var item in _SystemConfig.PureArray)
                {
                    //var Metric = Metrics.items.Where(a => a.resources.Select(s=>s.name == item)).Select(a => a.).ToList();
                    double LatencyReads = 0;
                    double LatencyWrites = 0;
                    double IopsReads = 0;
                    double IopsWrites = 0;
                    double BandwidthReads = 0;
                    double BandwidthWrites = 0;
                    double TotalLoad = 0;

                    foreach (var Metric in Metrics.items)
                    {
                        if (Metric.resources.FirstOrDefault().name == item)
                        {
                            if (Metric.name == "array_read_latency_us" && Metric.data.Count > 0)
                                LatencyReads = Metric.data.LastOrDefault().LastOrDefault();
                            if (Metric.name == "array_write_latency_us" && Metric.data.Count > 0)
                                LatencyWrites = Metric.data.LastOrDefault().LastOrDefault();
                            if (Metric.name == "array_read_iops" && Metric.data.Count > 0)
                                IopsReads = Metric.data.LastOrDefault().LastOrDefault();
                            if (Metric.name == "array_write_iops" && Metric.data.Count>0)
                                IopsWrites = Metric.data.LastOrDefault().LastOrDefault();                           
                        }
                    }

                    foreach (var Bandwidth in Bandwidths.items)
                    {
                        if (Bandwidth.resources.FirstOrDefault().name == item)
                        {
                            if (Bandwidth.name == "array_read_bandwidth" && Bandwidth.data.Count > 0)
                                BandwidthReads = Bandwidth.data.LastOrDefault().LastOrDefault();
                            if (Bandwidth.name == "array_write_bandwidth" && Bandwidth.data.Count > 0)
                                BandwidthWrites = Bandwidth.data.LastOrDefault().LastOrDefault();
                        }
                        
                    }

                    foreach (var Total_load in Total_loads.items)
                    {
                        if (Total_load.resources.FirstOrDefault().name == item)
                        {
                            if (Total_load.name == "array_total_load" && Total_load.data.Count > 0)
                                TotalLoad = Total_load.data.LastOrDefault().LastOrDefault();
                        }

                    }

                    if (_SystemConfig.IsNotifyLine)
                    {
                        var status = Environment.NewLine;
                        status += "Report: " + MacthNow.ToString();
                        status += Environment.NewLine;
                        status += "Array Name = " + item;
                        status += Environment.NewLine;
                        status += "Latency (ms) : Reads : Writes " + (LatencyReads / 1000).ToString("0.00", CultureInfo.InvariantCulture) + " : " + (LatencyWrites / 1000).ToString("0.00", CultureInfo.InvariantCulture);
                        status += Environment.NewLine;
                        status += "IOPS (K) : Reads : Writes " + (IopsReads / 1000).ToString("0.00", CultureInfo.InvariantCulture) + " : " + (IopsWrites / 1000).ToString("0.00", CultureInfo.InvariantCulture);
                        status += Environment.NewLine;
                        status += "Bandwidth (MB/s) : Reads : Writes " + (BandwidthReads / 1000000).ToString("0.00", CultureInfo.InvariantCulture) + " : " + (BandwidthWrites / 1000000).ToString("0.00", CultureInfo.InvariantCulture);
                        status += Environment.NewLine;
                        status += "Load (%) : Value : " + (TotalLoad * 100).ToString("00", CultureInfo.InvariantCulture);
                        status += Environment.NewLine;
                        _APISv.NotiLine(status, _SystemConfig.LineNotiToken);
                    }
                }     
                
                foreach (var item in SelectArrays)
                {
                    Console.WriteLine(item.name);
                    Console.WriteLine(item.model);
                    Console.WriteLine(item.version);
                  
                }
              
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
            public bool IsDevelop { get; set; }
            

        }
    }
}
