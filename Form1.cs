using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;

namespace WinFormsApp1
{

    public partial class Form1 : Form
    {
        public Form1()
        {

            InitializeComponent();

            textBox_label_dict.Add(textBox1, label27);
            textBox_label_dict.Add(textBox11, label14);
            textBox_label_dict.Add(textBox2, label15);
            textBox_label_dict.Add(textBox3, label16);
            textBox_label_dict.Add(textBox4, label17);
            textBox_label_dict.Add(textBox5, label18);
            textBox_label_dict.Add(textBox6, label19);
            textBox_label_dict.Add(textBox7, label20);
            textBox_label_dict.Add(textBox9, label24);
            textBox_label_dict.Add(textBox10, label25);

            comboBox_label_dict.Add(comboBox1, label21);
            comboBox_label_dict.Add(comboBox2, label22);

            inputConditions.Add(textBox1, false);
            inputConditions.Add(textBox11, false);
            inputConditions.Add(textBox2, false);
            inputConditions.Add(textBox3, false);
            inputConditions.Add(textBox4, false);
            inputConditions.Add(textBox5, false);
            inputConditions.Add(textBox6, false);
            inputConditions.Add(textBox7, false);
            inputConditions.Add(textBox9, false);
            inputConditions.Add(textBox10, false);

            jobsNamesDict.Add("Офис", "Office");
            jobsNamesDict.Add("Продажа", "Sales");
            jobsNamesDict.Add("Менеджер", "Mgr");
            jobsNamesDict.Add("ProfExe", "ProfExe");
            jobsNamesDict.Add("Другое", "Other");

            reasonsNamesDict.Add("Консолидация долга", "DebtCon");
            reasonsNamesDict.Add("Обустройство дома", "HomeImp");

            comboBox1.SelectedItem = comboBox1.Items[5];
            comboBox2.SelectedItem = comboBox2.Items[0];
        }

        private Dictionary<string, string> jobsNamesDict = new Dictionary<string, string>();
        private Dictionary<string, string> reasonsNamesDict = new Dictionary<string, string>();

        private Dictionary<TextBox, Label> textBox_label_dict = new Dictionary<TextBox, Label>();
        private Dictionary<ComboBox, Label> comboBox_label_dict = new Dictionary<ComboBox, Label>();

        private Dictionary<TextBox, bool> inputConditions = new Dictionary<TextBox, bool>();

        // Проверка, можно ли строку конверировать в float, считая за разделитель как точку, так и запятую
        private bool isFloat(string input)
        {
            float float_value;
            bool is_float = float.TryParse(input.Replace('.', ','), out float_value);
            return is_float;
        }
        
        private float convertStringToFloat(string str_value)
        {
            str_value = str_value.Replace('.', ',');
            float float_value = float.Parse(str_value);
            return float_value;
        }

        private string convertFloatToString(float float_value)
        {
            string str_value = string.Format("{0}", float_value);
            str_value = str_value.Replace(',', '.');
            return str_value;
        }
        // Если строку можно предстваить в виде числа, то меняется разделитель на точку (для Python), в противном случае возвращает пустую строку (None в Python)
        private string[] getCorrectStringValue(string str_input)
        {
            if (isFloat(str_input))
            {
                float float_value = convertStringToFloat(str_input);
                if (float_value < 0)
                {
                    return new string[] { "1", "" };
                }

                return new string[] { "0", convertFloatToString(float_value) };
            }
            return new string[] { "1", "" };
        }
        // Ввод является корректным, если не является пустым (кроме LOAN) и может быть конвертировано в float
        private bool isInputCorrect(TextBox textBox, Label indicator)
        {
            if (textBox.Text == "" & textBox != textBox1)
                return true;

            string[] error_value = getCorrectStringValue(textBox.Text);
            string error = error_value[0];

            if (error == "1")
            {
                return false;
            }

            return true;
        }
        // Возвращает true только в том случае, когда все вводы корректны
        private bool areInputsCorrect(Dictionary<TextBox, Label> textBox_label)
        {
            foreach (var item in inputConditions)
            {
                if (item.Value == false)
                    return false;
            }
            return true;
        }
        // Изменяет состояние полей TextBox
        private void ChangeTextBoxCondition(TextBox textBox, bool condition)
        {
            Label label = textBox_label_dict[textBox];
            label_prediction.Text = "";

            if (condition == true)
            {
                label.ForeColor = Color.Green;
                label.Text = "ok";
                inputConditions[textBox] = true;
            }
            else
            {
                label.ForeColor = Color.Red;
                label.Text = "error";
                inputConditions[textBox] = false;
            }
        }
        // Изменяет состояние полей ComboBox (Они всегда корректны, но лучше это отображать, когда пользователь его изменяет)
        private void ChangeComboBoxCondition(ComboBox comboBox, bool condition)
        {
            Label label = comboBox_label_dict[comboBox];
            label_prediction.Text = "";

            if (condition == true)
            {
                label.ForeColor = Color.Green;
                label.Text = "ok";
            }
            else
            {
                label.ForeColor = Color.Red;
                label.Text = "error";
            }
        }
        // В зависимости от работы модели выводит результат на форму
        private void ChangePredictionLabel(int result)
        {
            if (InvokeRequired)
                Invoke((Action<int>)ChangePredictionLabel, result);
            else
            {
                if (result == 0)
                {
                    this.label_prediction.ForeColor = Color.Green;
                    this.label_prediction.Text = "Одобрено";
                }
                else if (result == 1)
                {
                    this.label_prediction.ForeColor = Color.Red;
                    this.label_prediction.Text = "Не одобрено";
                }
            }
        }

        public static void ThreadMessage()
        {
            MessageBox.Show("Подождите, пожалуйста, модель думает...");
        }
        // Создает 2 потока - вывод сообщения о работе модели и поток для вычислений. В результате работы обращается к методу ChangePredictionLabel
        private void getModelPrediction(string filename)
        {

            Thread t_message = new Thread(new ThreadStart(ThreadMessage));
            t_message.Start();

            int result = -1;
            ThreadStart ths = new ThreadStart(() =>
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = Environment.CurrentDirectory + "\\scoring\\dist\\forest_model.exe";
                    process.StartInfo.Arguments = "";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardInput = true;
                    process.Start();
                    process.StandardInput.WriteLine(Environment.CurrentDirectory + "\\" + filename);
                    string str_result = process.StandardOutput.ReadToEnd();
                    int.TryParse(str_result, out result);
                    process.WaitForExit();

                    ChangePredictionLabel(result);
                }
            });
            Thread th = new Thread(ths);
            th.Start();

            t_message.Join();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button1.Click += new EventHandler(button1_Click);

            textBox1.TextChanged += new System.EventHandler(textBox_Changed);
            textBox2.TextChanged += new System.EventHandler(textBox_Changed);
            textBox3.TextChanged += new System.EventHandler(textBox_Changed);
            textBox4.TextChanged += new System.EventHandler(textBox_Changed);
            textBox5.TextChanged += new System.EventHandler(textBox_Changed);
            textBox6.TextChanged += new System.EventHandler(textBox_Changed);
            textBox7.TextChanged += new System.EventHandler(textBox_Changed);
            textBox9.TextChanged += new System.EventHandler(textBox_Changed);
            textBox10.TextChanged += new System.EventHandler(textBox_Changed);
            textBox11.TextChanged += new System.EventHandler(textBox_Changed);

            comboBox1.TextChanged += new System.EventHandler(comboBox_Changed);
            comboBox2.TextChanged += new System.EventHandler(comboBox_Changed);

        }
        // Обработчик события изменения TextBox
        private void textBox_Changed(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Label label = textBox_label_dict[textBox];

            ChangeTextBoxCondition(textBox, isInputCorrect(textBox, label));
        }
        // Обработчик события изменения ComboBox
        private void comboBox_Changed(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            Label label = comboBox_label_dict[comboBox];

            ChangeComboBoxCondition(comboBox, true);
        }
        // Обработчик события нажатия на кнопку
        private void button1_Click(object sender, System.EventArgs e)
        {

            label_prediction.Text = "";

            foreach (var item in textBox_label_dict)
            {
                ChangeTextBoxCondition(item.Key, isInputCorrect(item.Key, item.Value));
            }
            foreach (var item in comboBox_label_dict)
            {
                ChangeComboBoxCondition(item.Key, true);
            }

            if (!areInputsCorrect(textBox_label_dict))
                return;

            string LOAN = getCorrectStringValue(textBox1.Text)[1];
            string MORTDUE = getCorrectStringValue(textBox11.Text)[1];
            string VALUE = getCorrectStringValue(textBox2.Text)[1];
            string YOJ = getCorrectStringValue(textBox3.Text)[1];
            string DEROG = getCorrectStringValue(textBox4.Text)[1];
            string DELINQ = getCorrectStringValue(textBox5.Text)[1];
            string CLAGE = getCorrectStringValue(textBox6.Text)[1];
            string NINQ = getCorrectStringValue(textBox7.Text)[1];
            string DEBTINC = getCorrectStringValue(textBox9.Text)[1];
            string CLNO = getCorrectStringValue(textBox10.Text)[1];

            string JOB = jobsNamesDict[comboBox1.Text];
            string REASON = reasonsNamesDict[comboBox2.Text];

            //string DEBTINC = "";

            //if (LOAN != "" & INCOME != "")
            //    DEBTINC = convertFloatToString(convertStringToFloat(LOAN) / convertStringToFloat(INCOME));

            string csv_filename = "input.csv";

            using (var sw = new StreamWriter(Environment.CurrentDirectory + "\\" + csv_filename, false, Encoding.UTF8))
            {
                sw.WriteLine("LOAN,MORTDUE,VALUE,REASON,JOB,YOJ,DEROG,DELINQ,CLAGE,NINQ,CLNO,DEBTINC");
                sw.WriteLine(LOAN + "," + MORTDUE + "," + VALUE + "," + REASON + "," + JOB + "," + YOJ + "," + DEROG + "," + DELINQ + "," + CLAGE + "," + NINQ + "," + CLNO + "," + DEBTINC);
            }


            getModelPrediction(csv_filename);

            return;
        }
    }
}
