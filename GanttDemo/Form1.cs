﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using nGantt.GanttChart;
using nGantt.PeriodSplitter;
namespace GanttDemo
{
    public struct zadania
    {
        public int start;
        public int end;
        public int nr;

    };
    public partial class Form1 : Form
    {
        private int GantLenght { get; set; }
        private ObservableCollection<ContextMenuItem> ganttTaskContextMenuItems = new ObservableCollection<ContextMenuItem>();
        private ObservableCollection<SelectionContextMenuItem> selectionContextMenuItems = new ObservableCollection<SelectionContextMenuItem>();
        List<GanttRow> RowList = new List<GanttRow>();
        List<GanttRowGroup> RowGroupList = new List<GanttRowGroup>();
        List<zadania> Kolejka = new List<zadania>();

        Thread watek_1;
        Thread watek_2;
        delegate void metoda_do_modyfikacji1(string tekst);
        delegate void metoda_do_modyfikacji2(string tekst);
        metoda_do_modyfikacji1 aktualna_metoda_do_modyfikacji1;
        metoda_do_modyfikacji2 aktualna_metoda_do_modyfikacji2;
        private Thread cpuThread;
        private double[] cpuArray = new double[60];
        private Rectangle tabArea;
        private RectangleF tabTextArea;
        

        public Form1()
        {
            InitializeComponent();
            Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GantLenght = 1;
            dateTimePicker.Value = DateTime.Now;
            DateTime minDate = dateTimePicker.Value.Date;
            DateTime maxDate = minDate.AddDays(GantLenght);

            // Set selection -mode
            ganttControl1.TaskSelectionMode = nGantt.GanttControl.SelectionMode.Single;
            // Enable GanttTasks to be selected
            ganttControl1.AllowUserSelection = true;

            // listen to the GanttRowAreaSelected event
            ganttControl1.GanttRowAreaSelected += new EventHandler<PeriodEventArgs>(ganttControl1_GanttRowAreaSelected);
            
            // define ganttTask context menu and action when each item is clicked
            ganttTaskContextMenuItems.Add(new ContextMenuItem(ViewClicked, "View..."));
            ganttTaskContextMenuItems.Add(new ContextMenuItem(EditClicked, "Edit..."));
            ganttTaskContextMenuItems.Add(new ContextMenuItem(DeleteClicked, "Delete..."));
            ganttControl1.GanttTaskContextMenuItems = ganttTaskContextMenuItems;

            // define selection context menu and action when each item is clicked
            selectionContextMenuItems.Add(new SelectionContextMenuItem(NewClicked, "New..."));
            ganttControl1.SelectionContextMenuItems = selectionContextMenuItems;

        }
        //zdarzenie klikniecia uruchamiajace watki
        private void Start()
        {
            watek_1 = new Thread(watek_1_setup);
            watek_2 = new Thread(watek_2_setup);
            aktualna_metoda_do_modyfikacji1 = new metoda_do_modyfikacji1(wpisywanie_do_textboxu1);
            aktualna_metoda_do_modyfikacji2 = new metoda_do_modyfikacji2(wpisywanie_do_textboxu2);
            watek_1.Start();
            watek_2.Start();
        }

        //metoda na ktora wskazuje a_m_d_m
        void wpisywanie_do_textboxu1(string tekst)
        {
            textBox1.Text += tekst;
        }
        void wpisywanie_do_textboxu2(string tekst)
        {
            textBox2.Text += tekst;
        }

        void watek_1_setup()
        {
            while (true)
            {
                Thread.Sleep(3000);
                this.Invoke(aktualna_metoda_do_modyfikacji1, "pierwszy wątek, ");
            }
        }

        void watek_2_setup()
        {
            while (true)
            {
                Thread.Sleep(3000);
                this.Invoke(aktualna_metoda_do_modyfikacji2, "drugi wątek, ");
            }
        }
        private void NewClicked(Period selectionPeriod)
        {
            MessageBox.Show("New clicked for task " + selectionPeriod.Start.ToString() + " -> " + selectionPeriod.End.ToString());
        }

        private void ViewClicked(GanttTask ganttTask)
        {
            MessageBox.Show("New clicked for task " + ganttTask.Name);
        }

        private void EditClicked(GanttTask ganttTask)
        {
            MessageBox.Show("Edit clicked for task " + ganttTask.Name);
        }

        private void DeleteClicked(GanttTask ganttTask)
        {
            MessageBox.Show("Delete clicked for task " + ganttTask.Name);
        }

        void ganttControl1_GanttRowAreaSelected(object sender, PeriodEventArgs e)
        {
            MessageBox.Show(e.SelectionStart.ToString("0:HH:mm d.MM.yyyy") + " -> " + e.SelectionEnd.ToString("0:HH:mm d.MM.yyyy"));
        }

        private System.Windows.Media.Brush DetermineBackground(TimeLineItem timeLineItem)
        {
            if (timeLineItem.End.Date.DayOfWeek == DayOfWeek.Saturday || timeLineItem.End.Date.DayOfWeek == DayOfWeek.Sunday)
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightBlue);
            else
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Transparent);
        }

        
        private void CreateData(DateTime minDate, DateTime maxDate)
        {
            // Set max and min dates
            ganttControl1.Initialize(minDate, maxDate);
            //header displaying
            // Create timelines and define how they should be presented
            //ganttControl1.CreateTimeLine(new PeriodYearSplitter(minDate, maxDate), FormatYear);
            //ganttControl1.CreateTimeLine(new PeriodMonthSplitter(minDate, maxDate), FormatMonth);
            /////var gridLineTimeLine = ganttControl1.CreateTimeLine(new PeriodDaySplitter(minDate, maxDate), FormatDayName);
            //ganttControl1.CreateTimeLine(new PeriodDaySplitter(minDate, maxDate), FormatDayName);
            ganttControl1.CreateTimeLine(new PeriodYearSplitter(minDate, maxDate), FormatYear);
            var gridLineTimeLine = ganttControl1.CreateTimeLine(new PeriodHourSplitter(minDate, maxDate), FormatHour);
            // Set the timeline to atatch gridlines to
            ganttControl1.SetGridLinesTimeline(gridLineTimeLine, DetermineBackground);

            // Create and data
            RowGroupList.Add(ganttControl1.CreateGanttRowGroup());
            RowList.Add(ganttControl1.CreateGanttRow(RowGroupList[0], ""));

            RowGroupList.Add(ganttControl1.CreateGanttRowGroup("HeaderdGanttRowGroup"));
            RowList.Add(ganttControl1.CreateGanttRow(RowGroupList[1], "GanttRow 1")); 
            ganttControl1.AddGanttTask(RowList[1], new GanttTask() { Start = new DateTime(2016,12,20,13,24,00), End = new DateTime(2016, 12, 20, 13, 44, 00), Name = "GanttRow 1:GanttTask 1", TaskProgressVisibility = System.Windows.Visibility.Hidden });
            ganttControl1.AddGanttTask(RowList[1], new GanttTask() { Start = DateTime.Now.AddHours(3), End = DateTime.Now.AddHours(5), Name = "GanttRow 1:GanttTask 2" });
            ganttControl1.AddGanttTask(RowList[1], new GanttTask() { Start = DateTime.Now.AddHours(4), End = DateTime.Now.AddHours(8), Name = "GanttRow 1:GanttTask 3" });

            
            RowGroupList.Add(new ExpandableGanttRowGroup());
            RowGroupList[2] = ganttControl1.CreateGanttRowGroup("ExpandableGanttRowGroup", true);

            RowList.Add(ganttControl1.CreateGanttRow(RowGroupList[2], "GanttRow 2"));
            RowList.Add(ganttControl1.CreateGanttRow(RowGroupList[2], "GanttRow 3"));

            ganttControl1.AddGanttTask(RowList[2], new GanttTask() { Start = DateTime.Now.AddHours(1), End = DateTime.Now.AddHours(4), Name = "GanttRow 2:GanttTask 1" });
            ganttControl1.AddGanttTask(RowList[2], new GanttTask() { Start = DateTime.Now.AddHours(2), End = DateTime.Now.AddHours(3), Name = "GanttRow 2:GanttTask 2" });
            ganttControl1.AddGanttTask(RowList[2], new GanttTask() { Start = DateTime.Now.AddHours(3), End = DateTime.Now.AddHours(4), Name = "GanttRow 2:GanttTask 3", PercentageCompleted = 0.375 });
            ganttControl1.AddGanttTask(RowList[3], new GanttTask() { Start = DateTime.Now.AddHours(4), End = DateTime.Now.AddHours(5), Name = "GanttRow 3:GanttTask 1", PercentageCompleted = 0.5 });

            RowGroupList.Add(ganttControl1.CreateGanttRowGroup());
            RowList.Add(ganttControl1.CreateGanttRow(RowGroupList[3], "GanttRow 4"));
            ganttControl1.AddGanttTask(RowList[4], new GanttTask() { Start = DateTime.Now.AddHours(7), End = DateTime.Now.AddHours(8), Name = "GanttRow 4:GanttTask 1", PercentageCompleted = 1 });
            ganttControl1.AddGanttTask(RowList[4], new GanttTask() { Start = DateTime.Now.AddHours(4), End = DateTime.Now.AddHours(7), Name = "GanttRow 4:GanttTask 2" });
            ReadFile();
            AddRow();
            foreach (var s in Kolejka)
            {
                AddBlock(s.start, s.end, 6, s.nr.ToString() + ". Task");
            }
        }
    
        private string FormatYear(Period period)
        {
            return (
                period.Start.DayOfWeek.ToString() + ", " +
                period.Start.Day.ToString() + "." +
                period.Start.Month.ToString() + "." +
                period.Start.Year.ToString());
        }

        private string FormatMonth(Period period)
        {
            return period.Start.Month.ToString();
        }

        private string FormatDay(Period period)
        {
            return period.Start.Day.ToString();
        }
        private string FormatHour(Period period)
        {
            return period.Start.Hour.ToString();
        }

        private string FormatMinute(Period period)
        {
            return period.Start.Minute.ToString();
        }
        private string FormatDayName(Period period)
        {
            return period.Start.DayOfWeek.ToString();
        }

        private void buttonPrevious_Click(object sender, EventArgs e)
        {
            dateTimePicker.Value = ganttControl1.GanttData.MinDate.AddDays(-GantLenght);
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            dateTimePicker.Value = ganttControl1.GanttData.MaxDate;
        }

        private void dateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            DateTime minDate = dateTimePicker.Value.Date;
            DateTime maxDate = minDate.AddDays(GantLenght);
            ////DateTime minDate = new DateTime(2016, 12, 03, 13, 24, 00);
            ////DateTime maxDate = new DateTime(2016, 12, 08, 13, 24, 00);
            ganttControl1.ClearGantt();
            RowList.Clear();
            RowGroupList.Clear();
            CreateData(minDate, maxDate);
        }
        

        private void AddRow_Click(object sender, EventArgs e)
        {
            RowList.Add(ganttControl1.CreateGanttRow(RowGroupList[2], "Added row"));
        }
        private void AddRow()
        {
            RowList.Add(ganttControl1.CreateGanttRow(RowGroupList[2], "New"));
        }

        private void AddBlock_Clik(object sender, EventArgs e)
        {
            ganttControl1.AddGanttTask(RowList[Int32.Parse(RowChosen.Text)-1], new GanttTask() { Start = dateTimePicker1.Value, End = dateTimePicker2.Value, Name = "GanttRow 2:GanttTask 1" });
        }
        private void AddBlock(int start, int end, int machinenr, string name)
        {
            // ganttControl1.AddGanttTask(RowList[machinenr - 1], new GanttTask() { Start = DateTime.Now.AddHours(start), End = DateTime.Now.AddHours(end), Name = "GanttRow 2:GanttTask 1" });
            // ganttControl1.AddGanttTask(RowList[machinenr - 1], new GanttTask() { Start = DateTime.Now.AddHours(start), End = DateTime.Now.AddHours(end), Name = name });
            ganttControl1.AddGanttTask(RowList[machinenr - 1], new GanttTask() { Start = new DateTime(2016, 12, 20, 00, 00, 00).AddHours(start), End = new DateTime(2016, 12, 20, 00, 00, 00).AddHours(end), Name = name });
        }

        void ReadFile()
        {
            string cmdOutput;

            // Schrage & Schrage z podziałem (tablice dynamiczne)
            Process proc = new Process();
            proc.StartInfo.FileName = "SPD.exe";
            proc.StartInfo.Arguments = "inJ.txt";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();

            cmdOutput = proc.StandardOutput.ReadToEnd();

            proc.WaitForExit();
            proc.Close();
            cmdOutput = cmdOutput.Replace("\r\n", " ");
            string[] s = cmdOutput.Split();
            int n = Int32.Parse(s[0]);
            int a, b,c;
            int j = 1;
            for (int i = 1; i <= n; i++)
            {
                 a = int.Parse(s[j]);
                 b = int.Parse(s[j+1]);
                 c = int.Parse(s[j+2]);
                zadania tmp=new zadania();
                tmp.nr = a;
                tmp.start = b;
                tmp.end = c;
                Kolejka.Add(tmp);
                j += 3;
            }
        }

        private void AppClosing(object sender, FormClosingEventArgs e)
        {
            if (watek_1.IsAlive) watek_1.Abort();
            if (watek_2.IsAlive) watek_2.Abort();
        }
    }
}
