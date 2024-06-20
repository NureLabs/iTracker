using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Win32;
using System.Net;
using System.Collections;
using System.Collections.Specialized;

namespace iTracker
{
    public partial class Form1 : Form
    {
        // WinAPI импорты
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();//Извлекает дескриптор окнa переднего плана

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processId);

        // Обработчик перечисления окон
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        // Список браузеров
        private readonly string[] browserNames =
        {
            "chrome",
            "firefox",
            "iexplore",
            "msedge",
            "safari",
            "opera",
            "tor",
            "brave",
            "vivaldi",
            "edgechromium",
            "ucbrowser",
            "maxthon",
            "palemoon",
            "seamonkey",
            "waterfox",
            "avant",
            "midori",
            "epiphany",
            "puffin"
        };
        //Количество символов
        private const int maxChars = 256;
       
        //DateTime при запуске формы (iTracker)
        public DateTime startTimeForm;
       
        //Oбъекты
        public ActiveWindowClass ActiveProcessObj;
        public List<ActiveWindowClass> ActiveProcessesList = new List<ActiveWindowClass>();//активное окно добавляется в список после закрытия
        public List<ActiveWindowClass> LoadDataList = new List<ActiveWindowClass>();
        public StringCollection unwritebleProcs = new StringCollection();
        public List<ProcessStatistic> StatisticOfProcessesList = new List<ProcessStatistic>();
        
        //Координаты мыши
        private bool isDragging = false;
        private int xOffset;
        private int yOffset;
        //
        private DateTime activeWindowActivationTime = DateTime.MinValue;
        private IntPtr previousWindowHandle = IntPtr.Zero;

        private bool realtimeDataShow = true;
        public Form1()
        {
            //Иницилизация

            InitializeComponent();
            //Для нахождения мыши в форме
            MouseDown += MainForm_MouseDown;
            MouseMove += MainForm_MouseMove;
            MouseUp += MainForm_MouseUp;
            // Сохраняем время запуска формы
            startTimeForm = DateTime.Now;

            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer
            {
                Interval = 100 //0.1с
            };
            timer.Tick += Timer_Tick;
            timer.Start();

            System.Windows.Forms.Timer timer3 = new System.Windows.Forms.Timer
            {
                Interval = 1000 //1с
            };
            timer3.Tick += Timer_TickDiagram;
            timer3.Start();
            SystemEvents.PowerModeChanged += PowerModeChanged;
            dateTimePicker1.Format = DateTimePickerFormat.Time;
        }

        private void PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Resume:
                    ActiveProcessObj.SetActiveProcess_EndTime(DateTime.Now);
                    ActiveProcessesList.Add(ActiveProcessObj);
                    break;
                case PowerModes.Suspend:
                    ActiveProcessObj = new ActiveWindowClass();
                    ActiveProcessObj.SetActiveProgram_Name("SLEEPMODE");
                    ActiveProcessObj.SetActiveProcess_Name("WINDOWS_SLEEP_MODE");
                    ActiveProcessObj.SetActiveProcess_StartTime(DateTime.Now);
                    ActiveProcessObj.SetActiveProcess_Path("--service_proc_name--");
                    break;
            }
        }
        private string GetActiveWindowTitle()//Функция для нахождения названия окна
        {
            IntPtr handle = GetForegroundWindow();
            StringBuilder windowTitle = new StringBuilder(maxChars);

            if (GetWindowText(handle, windowTitle, maxChars) > 0)
            {
                return windowTitle.ToString();
            }

            return string.Empty;
        }
        private string GetActiveWindowProgramName()//Функция для нахождения имени исполняемого файла
        {
            IntPtr currentActiveWindowHandle = GetForegroundWindow();
            Process activeProcess = GetProcessFromWindowHandle(currentActiveWindowHandle);
            return activeProcess.ProcessName;
        }
        private DateTime GetActiveWindowStartProcess()//Функция для нахождения старта нового активного окна
        {
            IntPtr handle = GetForegroundWindow();

            if (handle != previousWindowHandle) // Проверка, что окно отличается от предыдущего активного окна
            {
                activeWindowActivationTime = DateTime.Now; // Зафиксировать время активации окна
            }

            previousWindowHandle = handle; // Обновить предыдущее активное окно

            return activeWindowActivationTime;
        }
        private void GetActiveWindowEndTime()//Функция для нахождения конца активного окна
        {
            if (ActiveProcessesList.Count >= 1)
            {
                IntPtr handle = GetForegroundWindow();
                if (ActiveProcessesList.Last().GetActiveProcess_Handle() != handle)
                {
                    ActiveProcessesList.Last().SetActiveProcess_EndTime(GetActiveWindowStartProcess());
                }
            }
        }
        private void GetActiveExecutionTime()//Функция для нахождения суммы (или же сколько окно было активным)
        {
            if (ActiveProcessesList.Count >= 1)
            {
                IntPtr handle = GetForegroundWindow();
                GetWindowThreadProcessId(handle, out int processId);
                if (ActiveProcessesList.Last().GetActiveProcess_Handle() != handle)
                {
                    ActiveProcessesList.Last().SetActiveProcess_ExcecutionTime(GetActiveWindowStartProcess() - ActiveProcessesList.Last().GetActiveProcess_StartTime());
                }
            }
        }
        private string GetActiveWindowFilePath()
        {
            IntPtr handle = GetForegroundWindow();
            GetWindowThreadProcessId(handle, out int processId);
            Process process = Process.GetProcessById((int)processId);
            try
            {
                string filePath = process.MainModule.FileName;
                return filePath;
            }
            catch (Win32Exception)
            {
                // Обработка исключения, если файл программного модуля недоступен
                return string.Empty; // Или другое значение по умолчанию, которое имеет смысл для вашего приложения
            }
        }
        private static Process GetProcessFromWindowHandle(IntPtr hWnd) // Получает объект процесса на основе дескриптора окна
        {
            GetWindowThreadProcessId(hWnd, out int processId);
            try
            {
                return Process.GetProcessById(processId);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private void Show_Current_Activity()
        {
            for (int i = 0; i < ActiveProcessesList.Count; i++)
            {
                if (dataGridView1.Rows.Count < ActiveProcessesList.Count) dataGridView1.Rows.Add();
                dataGridView1.Rows[i].Cells[0].Value = ActiveProcessesList[i].GetActiveProgram_Name();
                dataGridView1.Rows[i].Cells[1].Value = ActiveProcessesList[i].GetActiveProcess_Name();
                dataGridView1.Rows[i].Cells[2].Value = ActiveProcessesList[i].GetActiveProcess_StartTime();
                if (i < ActiveProcessesList.Count - 1)
                {
                    dataGridView1.Rows[i].Cells[3].Value = ActiveProcessesList[i].GetActiveProcess_EndTime();
                }
                dataGridView1.Rows[i].Cells[4].Value = ActiveProcessesList[i].GetActiveProcess_ExcecutionTime().ToString(@"hh\:mm\:ss");
                dataGridView1.Rows[i].Cells[5].Value = ActiveProcessesList[i].GetActiveBrowser_Tab();
                dataGridView1.Rows[i].Cells[6].Value = ActiveProcessesList[i].GetActiveProcess_Path();
                if (unwritebleProcs.Contains(ActiveProcessesList[i].GetActiveProgram_Name()))
                {
                    dataGridView1.Rows[i].Visible = false;
                }
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            bool newProcAdded = false;
            if (GetActiveWindowProgramName() != "Idle")
            {
                ActiveProcessObj = new ActiveWindowClass();
                ActiveProcessObj.SetActiveProcess_Handle(GetForegroundWindow());
                ActiveProcessObj.SetActiveProcess_Name(GetActiveWindowTitle());
                ActiveProcessObj.SetActiveProcess_StartTime(GetActiveWindowStartProcess());
                ActiveProcessObj.SetActiveProgram_Name(GetActiveWindowProgramName());
                GetActiveExecutionTime();
                GetActiveWindowEndTime();
                if (browserNames.Contains(ActiveProcessObj.GetActiveProgram_Name()))
                {
                    ActiveProcessObj.SetActiveBrowser_Tab(GetActiveWindowTitle());
                }
                ActiveProcessObj.SetActiveProcess_Path(GetActiveWindowFilePath());
            }
            if ((ActiveProcessesList.Count == 0 || ActiveProcessesList.Last().GetActiveProcess_Handle() != GetForegroundWindow()) && GetActiveWindowProgramName() != "Idle")
            {
                if (ActiveProcessObj.ProgramName != "Idle")
                {
                    ActiveProcessesList.Add(ActiveProcessObj);
                    newProcAdded = true;
                }
            }
            if (realtimeDataShow == true)
            {
                Show_Current_Activity();
                label6.Text = (DateTime.Now - startTimeForm).ToString().Split('.')[0]; //счет секунд активного окна в режиме реального времени

                if (dataGridView1.Rows.Count > 0)
                {
                    dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[4].Value = FormatTime(DateTime.Now - ActiveProcessesList[ActiveProcessesList.Count - 1].GetActiveProcess_StartTime());
                }
                if (newProcAdded)
                    // Прокрутка DataGridView до последней строки
                    dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.RowCount - 1;
            }
        }
        private void Timer_TickDiagram(object sender, EventArgs e)//Функция для таблицы статистики
        {
            FillSecondListForStats();
            dataGridView3.Rows.Clear();

            if (StatisticOfProcessesList.Count > 0)
            {
                for (int i = 0; i < StatisticOfProcessesList.Count; i++)
                {
                    dataGridView3.Rows.Add(StatisticOfProcessesList[i].GetActiveProgram_Name(), StatisticOfProcessesList[i].GetActiveProcess_ExcecutionTime().ToString(@"hh\:mm\:ss"), StatisticOfProcessesList[i].GetActiveProcess_PersentOfUsage() + "%");
                }
            }

            if (dataGridView3.Rows.Count > 0) //вывод в форму информации про самый долгий процесс
            {
                label4.Text = "Name Programm: " + dataGridView3.Rows[0].Cells[0].Value.ToString() +
                              "\nTime: " + dataGridView3.Rows[0].Cells[1].Value.ToString() +
                              "\nPercent: " + dataGridView3.Rows[0].Cells[2].Value.ToString();
            }
        }

        //Для перетаскивания окна формочки без рамки с помощью мыши
        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            isDragging = true;
            xOffset = e.X;
            yOffset = e.Y;
        }
        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Location = new Point(
                    Location.X + (e.X - xOffset),
                    Location.Y + (e.Y - yOffset)
                );
            }
        }
        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void FillSecondListForStats() //метод ведения статистики всех процессов
        {
            StatisticOfProcessesList.Clear();

            var statlist = ActiveProcessesList;
            if (realtimeDataShow == false) statlist = LoadDataList;

            for (int i = 0; i < statlist.Count - 1; i++)
            {
                ProcessStatistic procStats = new ProcessStatistic();
                procStats.SetActiveProgram_Name(statlist[i].GetActiveProgram_Name());
                procStats.SetActiveProcess_ExcecutionTime(statlist[i].GetActiveProcess_ExcecutionTime());
                procStats.SetActiveProcess_PersentOfUsage(0);
                StatisticOfProcessesList.Add(procStats);
            }
            for (int i = 0; i < StatisticOfProcessesList.Count; i++)//служебный цикл для удаления дублей списка
            {
                for (int j = 0; j < StatisticOfProcessesList.Count; j++)
                {
                    if (StatisticOfProcessesList[i].GetActiveProgram_Name() == StatisticOfProcessesList[j].GetActiveProgram_Name() && i != j)
                    {
                        StatisticOfProcessesList[i].SetActiveProcess_ExcecutionTime(StatisticOfProcessesList[i].GetActiveProcess_ExcecutionTime() + StatisticOfProcessesList[j].GetActiveProcess_ExcecutionTime());
                        StatisticOfProcessesList.RemoveAt(j);
                        j--;
                    }
                }
            }

            TimeSpan wholeTime = new TimeSpan();
            for (int i = 0; i < StatisticOfProcessesList.Count; i++)
            {
                wholeTime += StatisticOfProcessesList[i].GetActiveProcess_ExcecutionTime();
            }
            for (int i = 0; i < StatisticOfProcessesList.Count; i++)
            {
                StatisticOfProcessesList[i].SetActiveProcess_PersentOfUsage(Math.Round(StatisticOfProcessesList[i].GetActiveProcess_ExcecutionTime().TotalMilliseconds / wholeTime.TotalMilliseconds * 100, 2));
            }
            StatisticOfProcessesList.Sort((x, y) => y.GetActiveProcess_PersentOfUsage().CompareTo(x.GetActiveProcess_PersentOfUsage()));
        }

        private string FormatTime(TimeSpan timerspan) //Функция для записи в формате hh\mm\ss
        {
            if (timerspan == TimeSpan.Zero)
            {
                return string.Empty;
            }
            return timerspan.ToString(@"hh\:mm\:ss");
        }

        private void buttonMinimiz_Click(object sender, EventArgs e)
        {
            // Свернуть программу
            WindowState = FormWindowState.Minimized;

        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            // Закрытие текущего окна
            buttonSaveFile_Click(sender, e);
            Close();
        }

        private void buttonSaveFile_Click(object sender, EventArgs e)
        {
            ActiveProcessesList[ActiveProcessesList.Count - 1].SetActiveProcess_EndTime(DateTime.Now);
            string jsonString = JsonConvert.SerializeObject(ActiveProcessesList);
            string cryptokey = "cryptokey1234567";
            byte[] encrypted;

            using (Aes aesAlg = Aes.Create())//сохранение данных в зашифрованном виде
            {
                aesAlg.Key = Encoding.ASCII.GetBytes(cryptokey);
                aesAlg.IV = SHA512CryptoServiceProvider.Create().ComputeHash(aesAlg.Key).Take(aesAlg.BlockSize / 8).ToArray();
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(jsonString);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
                File.WriteAllBytes("./statistic/" + DateTime.Now.ToString().Replace(':', '.').Replace(' ', '_') + ".aesiv", aesAlg.IV);
                File.WriteAllBytes("./statistic/" + DateTime.Now.ToString().Replace(':', '.').Replace(' ', '_') + ".aesdata", encrypted);
            }
        }

        private void load_Data_Click(object sender, EventArgs e)
        {
            realtimeDataShow = false;
            LoadDataList.Clear();
            dataGridView1.Rows.Clear();

            string cryptokey = "cryptokey1234567";

            using (Aes aesAlg = Aes.Create()) {//расшифровка данных и импортирование
                aesAlg.Key = Encoding.ASCII.GetBytes(cryptokey);
                string[] aesivFiles = Directory.GetFiles("./statistic/", dateTimePicker1.Value.ToString().Split(' ')[0] + "*.aesiv");
                for (int i = 0; i < aesivFiles.Length; i++) {
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV = File.ReadAllBytes("./" + aesivFiles[i]));
                    char[] trim = { 'i', 'v' };
                    using (MemoryStream msDecrypt = new MemoryStream(File.ReadAllBytes("./" + aesivFiles[i].TrimEnd(trim) + "data"))) {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)) {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt)) {
                                LoadDataList.AddRange(JsonConvert.DeserializeObject<List<ActiveWindowClass>>(srDecrypt.ReadToEnd()));
                            }
                        }
                    }
                }
                for (int i = 0; i < LoadDataList.Count; i++) {
                    dataGridView1.Rows.Add();
                    dataGridView1.Rows[i].Cells[0].Value = LoadDataList[i].GetActiveProgram_Name();
                    dataGridView1.Rows[i].Cells[1].Value = LoadDataList[i].GetActiveProcess_Name();
                    dataGridView1.Rows[i].Cells[2].Value = LoadDataList[i].GetActiveProcess_StartTime();
                    if (i < LoadDataList.Count - 1) {
                        dataGridView1.Rows[i].Cells[3].Value = LoadDataList[i].GetActiveProcess_EndTime();
                    }
                    dataGridView1.Rows[i].Cells[4].Value = LoadDataList[i].GetActiveProcess_ExcecutionTime().ToString(@"hh\:mm\:ss");
                    dataGridView1.Rows[i].Cells[5].Value = LoadDataList[i].GetActiveBrowser_Tab();
                    dataGridView1.Rows[i].Cells[6].Value = LoadDataList[i].GetActiveProcess_Path(); 
                    if (unwritebleProcs.Contains(LoadDataList[i].GetActiveProgram_Name())) {
                        dataGridView1.Rows[i].Visible = false;
                    }
                }
                for (int i = 0; i < LoadDataList.Count; i++) {
                    if (LoadDataList[i].GetActiveProcess_StartTime() < dateTimePicker1.Value && LoadDataList[i].GetActiveProcess_EndTime() > dateTimePicker1.Value) {
                        MessageBox.Show(LoadDataList[i].GetActiveProgram_Name() + " " + LoadDataList[i].GetActiveProcess_StartTime() + " - " + LoadDataList[i].GetActiveProcess_EndTime());
                        break;
                    }
                }
                label6.Text = (LoadDataList[LoadDataList.Count - 1].GetActiveProcess_StartTime() - LoadDataList[0].GetActiveProcess_StartTime()).ToString().Split('.')[0];
            }
        }

        private void button_currentActivity_Click(object sender, EventArgs e)
        {
            if (realtimeDataShow)
                return;
            realtimeDataShow = true;
            dataGridView1.Rows.Clear();
            Show_Current_Activity();
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            string serchingProgram = textBox1.Text;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                var programName = row.Cells["ProgramName"].Value.ToString();
                if (!programName.Contains(serchingProgram))
                {
                    row.Visible = false;
                }
                else
                {
                    row.Visible = true;
                }
            }
            textBox1.Clear();
        }

        private void buttonViewAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                row.Visible = true;
            }
            textBox1.Clear();
            unwritebleProcs.Clear();
        }

        private void button_banProcess_Click(object sender, EventArgs e)
        {
            string banProgram = textBox1.Text;
            unwritebleProcs.Add(banProgram);
            textBox1.Clear();
        }
    }
}
