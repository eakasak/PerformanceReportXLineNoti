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
            SetTimer();
            LoadConfig();
            LoadMatchDay();
        }
        private static void LoadMatchDay()
        {

            var _curdate = new DateTime(2022, 05, 22, 15, 00, 00);

            ////curdate = DateTime.Now;
            var curmacth = DateTime.Now;

            var rs = _APISv.GetFixtures();

            var rs1 = rs.Where(a => Convert.ToDateTime(a.kickoff_time) >= Convert.ToDateTime(_curdate.AddHours(-1))).FirstOrDefault();
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
                    msg += "rp2 : " + beginko.AddMinutes(_SystemConfig.EveryBeginKickOff * 1).ToString();
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
                if (!_SystemConfig.IsDevelop)
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
            aTimer = new System.Timers.Timer(1000);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            DateTime datenow = DateTime.Today;
            DateTime datetimenow = DateTime.Now;

            curdate = new DateTime(2022, 05, 22, datetimenow.Hour, datetimenow.Minute, datetimenow.Second);
            DateTime MacthNow = new DateTime(curdate.Year, curdate.Month, curdate.Day, curdate.Hour, curdate.Minute, 0);

            if (MatchDay.Contains(MacthNow))
            {
                //var bftime = MacthNow.AddHours(-2);
                //TimeSpan t = bftime.ToUniversalTime() - new DateTime(1970, 1, 1);
                //int secondsSinceEpoch = (int)t.TotalSeconds;

                //TimeSpan t2 = MacthNow.ToUniversalTime() - new DateTime(1970, 1, 1);
                //int endsecondsSinceEpoch = (int)t2.TotalSeconds;

                aTimer.Stop();
                aTimer = new System.Timers.Timer(60000);
                aTimer.Elapsed += OnTimedEvent;
                aTimer.AutoReset = true;
                aTimer.Enabled = true;
                aTimer.Start();

                var Arrays = _APISv.GetArrays();
                //  var SelectArrays = Arrays.items.Where(a => _SystemConfig.PureArrayConfig..Contains(a.name));
                Console.WriteLine("Report : " + curdate.ToString());
                //var Valumes = _APISv.GetVolumes();
                //var SelectValume = Valumes.items;
                //var Metrics = _APISv.GetMetricsHistory(secondsSinceEpoch, endsecondsSinceEpoch);           
                //var Bandwidths = _APISv.GetMetricsBandwidth(secondsSinceEpoch, endsecondsSinceEpoch);
                //var Total_loads = _APISv.GetMetricsTotalLoad(secondsSinceEpoch, endsecondsSinceEpoch);

                foreach (var item in _SystemConfig.PureArrayConfig)
                {
                    if (item.IsMonitor)
                    {
                        var ArrayMonitor = _APISv.GetArrayMonitor(item);


                        double LatencyReads = 0;
                        double LatencyWrites = 0;
                        double IopsReads = 0;
                        double IopsWrites = 0;
                        double BandwidthReads = 0;
                        double BandwidthWrites = 0;
                        double TotalLoad = 0;


                        LatencyReads = ArrayMonitor.reads_per_sec;
                        LatencyWrites = ArrayMonitor.writes_per_sec;
                        IopsReads = ArrayMonitor.usec_per_read_op;
                        IopsWrites = ArrayMonitor.usec_per_write_op;
                        BandwidthReads = ArrayMonitor.input_per_sec;
                        BandwidthWrites = ArrayMonitor.output_per_sec;
                        TotalLoad = 0;

                        //foreach (var Metric in Metrics.items)
                        //{
                        //if (Metric.resources.FirstOrDefault().name == item)
                        //{
                        //    if (Metric.name == "array_read_latency_us" && Metric.data.Count > 0)
                        //        LatencyReads = Metric.data.LastOrDefault().LastOrDefault();
                        //    if (Metric.name == "array_write_latency_us" && Metric.data.Count > 0)
                        //        LatencyWrites = Metric.data.LastOrDefault().LastOrDefault();
                        //    if (Metric.name == "array_read_iops" && Metric.data.Count > 0)
                        //        IopsReads = Metric.data.LastOrDefault().LastOrDefault();
                        //    if (Metric.name == "array_write_iops" && Metric.data.Count>0)
                        //        IopsWrites = Metric.data.LastOrDefault().LastOrDefault();                           
                        //}
                        //}

                        //foreach (var Bandwidth in Bandwidths.items)
                        //{
                        //if (Bandwidth.resources.FirstOrDefault().name == item)
                        //{
                        //    if (Bandwidth.name == "array_read_bandwidth" && Bandwidth.data.Count > 0)
                        //        BandwidthReads = Bandwidth.data.LastOrDefault().LastOrDefault();
                        //    if (Bandwidth.name == "array_write_bandwidth" && Bandwidth.data.Count > 0)
                        //        BandwidthWrites = Bandwidth.data.LastOrDefault().LastOrDefault();
                        //}

                        //}

                        //foreach (var Total_load in Total_loads.items)
                        //{
                        //    if (Total_load.resources.FirstOrDefault().name == item)
                        //    {
                        //        if (Total_load.name == "array_total_load" && Total_load.data.Count > 0)
                        //            TotalLoad = Total_load.data.LastOrDefault().LastOrDefault();
                        //    }

                        //}

                        if (_SystemConfig.IsNotifyLine)
                        {
                            var status = Environment.NewLine;
                            status += "Report: " + MacthNow.ToString();
                            status += Environment.NewLine;
                            status += "Name : " + item.ArrayName;
                            status += Environment.NewLine;
                            status += "Latency (ms) : R/W : " + (LatencyReads / 1000).ToString("0.00", CultureInfo.InvariantCulture) + "/" + (LatencyWrites / 1000).ToString("0.00", CultureInfo.InvariantCulture);
                            status += Environment.NewLine;
                            status += "IOPS (K) : R/W : " + (IopsReads / 1000).ToString("0.00", CultureInfo.InvariantCulture) + "/" + (IopsWrites / 1000).ToString("0.00", CultureInfo.InvariantCulture);
                            status += Environment.NewLine;
                            status += "Bandwidth (MB/s) : R/W : " + (BandwidthReads / 1000000).ToString("0.00", CultureInfo.InvariantCulture) + "/" + (BandwidthWrites / 1000000).ToString("0.00", CultureInfo.InvariantCulture);
                            status += Environment.NewLine;
                            status += "Load (%) : Value : TODO " + TotalLoad;
                            status += Environment.NewLine;
                            Console.WriteLine(status);
                            _APISv.NotiLine(status, _SystemConfig.LineNotiToken);
                        }
                    }
                }
                //foreach (var item in SelectArrays)
                //{
                //    Console.WriteLine(item.name);
                //    Console.WriteLine(item.model);
                //    Console.WriteLine(item.version);

                //}

                var MaxTimeMatchDay = MatchDay.OrderByDescending(o => o).FirstOrDefault();
                if (MaxTimeMatchDay == MacthNow)
                {
                    Console.WriteLine("End Match");
                    LoadMatchDay();
                }
            }
        }
      
    }
}
