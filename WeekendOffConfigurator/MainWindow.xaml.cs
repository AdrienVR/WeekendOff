using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace WeekendOffConfigurator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            var workingDays = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
            _workingHours = new List<(int, int)> { (0, 24), (0, 24), (0, 24), (0, 24), (0, 24) };
            try
            {
                var lines = File.ReadAllLines("WorkingDaysConfig.txt");
                if (lines.Length > 0)
                {
                    workingDays.Clear();
                    workingDays.AddRange(lines.Select(s => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), s.Split(':')[0])));
                    _workingHours.Clear();
                    _workingHours.AddRange(lines.Select(s =>
                    {
                        var xToY = s.Split(':')[1];
                        var xy = xToY.Split(new[] { "to" }, StringSplitOptions.RemoveEmptyEntries);
                        return (int.Parse(xy[0]), int.Parse(xy[1]));
                    }));
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch { }

            CB_Monday.IsOn = workingDays.IndexOf(DayOfWeek.Monday) != -1;
            CB_Tuesday.IsOn = workingDays.IndexOf(DayOfWeek.Tuesday) != -1;
            CB_Wednesday.IsOn = workingDays.IndexOf(DayOfWeek.Wednesday) != -1;
            CB_Thursday.IsOn = workingDays.IndexOf(DayOfWeek.Thursday) != -1;
            CB_Friday.IsOn = workingDays.IndexOf(DayOfWeek.Friday) != -1;
            CB_Saturday.IsOn = workingDays.IndexOf(DayOfWeek.Saturday) != -1;
            CB_Sunday.IsOn = workingDays.IndexOf(DayOfWeek.Sunday) != -1;

            TB_Process.Text = "Teams";
            try
            {
                TB_Process.Text = File.ReadAllText("ProcessToKillList.txt");
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch { }
        }
        
        private readonly List<(int, int)> _workingHours;

        private void SaveButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            List<DayOfWeek> workingDays = new List<DayOfWeek>(7);
            if (CB_Monday.IsOn)
                workingDays.Add(DayOfWeek.Monday);
            if (CB_Tuesday.IsOn)
                workingDays.Add(DayOfWeek.Tuesday);
            if (CB_Wednesday.IsOn)
                workingDays.Add(DayOfWeek.Wednesday);
            if (CB_Thursday.IsOn)
                workingDays.Add(DayOfWeek.Thursday);
            if (CB_Friday.IsOn)
                workingDays.Add(DayOfWeek.Friday);
            if (CB_Saturday.IsOn)
                workingDays.Add(DayOfWeek.Saturday);
            if (CB_Sunday.IsOn)
                workingDays.Add(DayOfWeek.Sunday);

            var allLines = new List<string>();
            for (int i = 0; i < workingDays.Count; i++)
            {
                allLines.Add($"{workingDays[i]}: {_workingHours[i].Item1} to {_workingHours[i].Item2}");
            }

            File.WriteAllLines("WorkingDaysConfig.txt", allLines);

            File.WriteAllText("ProcessToKillList.txt", TB_Process.Text);
        }
    }
}
