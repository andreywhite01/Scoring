using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Drawing.Drawing2D;


namespace Scoring
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            
            InitializeComponent();

            input_condition_dict.Add(textBox1,    pictureBox1);
            input_condition_dict.Add(textBox11,   pictureBox2);
            input_condition_dict.Add(textBox2,    pictureBox3);
            input_condition_dict.Add(comboBox1,  pictureBox4);
            input_condition_dict.Add(comboBox2,  pictureBox5);
            input_condition_dict.Add(textBox3,    pictureBox6);
            input_condition_dict.Add(textBox4,    pictureBox7);
            input_condition_dict.Add(textBox5,    pictureBox8);
            input_condition_dict.Add(textBox6,    pictureBox9);
            input_condition_dict.Add(textBox7,    pictureBox10);
            input_condition_dict.Add(textBox9,    pictureBox11);
            input_condition_dict.Add(textBox10,   pictureBox12);

            comboBox1.SelectedItem = comboBox1.Items[5];
            comboBox2.SelectedItem = comboBox2.Items[0];

            textBoxesOrder = new List<Object>
            {
                textBox1,
                textBox11,
                textBox2,
                comboBox2,
                comboBox1,
                textBox3,
                textBox4,
                textBox5,
                textBox6,
                textBox7,
                textBox10,
                textBox9
            };
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            button1.Click += new EventHandler(Button_StartPrediction);
            button2.Click += new EventHandler(Button_Refresh);
            toolStripMenuItemRefreshWindow.Click += new EventHandler(Button_Refresh);
            toolStripMenuItemCloseWindow.Click += new EventHandler(Button_CloseForm);
            toolStripMenuItemAboutProgramm.Click += new EventHandler(Button_GetInformation);

            textBox1.TextChanged += new System.EventHandler(Input_Changed);
            textBox2.TextChanged += new System.EventHandler(Input_Changed);
            textBox3.TextChanged += new System.EventHandler(Input_Changed);
            textBox4.TextChanged += new System.EventHandler(Input_Changed);
            textBox5.TextChanged += new System.EventHandler(Input_Changed);
            textBox6.TextChanged += new System.EventHandler(Input_Changed);
            textBox7.TextChanged += new System.EventHandler(Input_Changed);
            textBox9.TextChanged += new System.EventHandler(Input_Changed);
            textBox10.TextChanged += new System.EventHandler(Input_Changed);
            textBox11.TextChanged += new System.EventHandler(Input_Changed);
            comboBox1.TextChanged += new System.EventHandler(Input_Changed);
            comboBox2.TextChanged += new System.EventHandler(Input_Changed);

            textBox1.KeyDown += new KeyEventHandler(TextBox_KeyDown);
            textBox2.KeyDown += new KeyEventHandler(TextBox_KeyDown);
            textBox3.KeyDown += new KeyEventHandler(TextBox_KeyDown);
            textBox4.KeyDown += new KeyEventHandler(TextBox_KeyDown);
            textBox5.KeyDown += new KeyEventHandler(TextBox_KeyDown);
            textBox6.KeyDown += new KeyEventHandler(TextBox_KeyDown);
            textBox7.KeyDown += new KeyEventHandler(TextBox_KeyDown);
            textBox9.KeyDown += new KeyEventHandler(TextBox_KeyDown);
            textBox10.KeyDown += new KeyEventHandler(TextBox_KeyDown);
            textBox11.KeyDown += new KeyEventHandler(TextBox_KeyDown);
            comboBox1.KeyDown += new KeyEventHandler(TextBox_KeyDown);
            comboBox2.KeyDown += new KeyEventHandler(TextBox_KeyDown);
        }

        private readonly Dictionary<object, PictureBox> input_condition_dict = new Dictionary<object, PictureBox>();

        private readonly List<object> textBoxesOrder;

        // Проверка, можно ли строку конверировать в float, считая за разделитель как точку, так и запятую
        private bool IsFloat(string input)
        {
            bool is_float = float.TryParse(input.Replace('.', ','), out _);
            return is_float;
        }

        private float ConvertStringToFloat(string str_value)
        {
            str_value = str_value.Replace('.', ',');
            float float_value = float.Parse(str_value);
            return float_value;
        }

        private string ConvertFloatToString(float float_value)
        {
            string str_value = string.Format("{0}", float_value);
            str_value = str_value.Replace(',', '.');
            return str_value;
        }
        // Если строку можно предстваить в виде числа, то меняется разделитель на точку (для Python), в противном случае возвращает пустую строку (None в Python)
        private string[] GetCorrectStringValue(string str_input)
        {
            if (IsFloat(str_input))
            {
                float float_value = ConvertStringToFloat(str_input);
                if (float_value < 0)
                {
                    return new string[] { "1", "" };
                }

                return new string[] { "0", ConvertFloatToString(float_value) };
            }
            return new string[] { "1", "" };
        }
        // Ввод является корректным, если не является пустым (кроме LOAN) и может быть конвертировано в float
        private bool IsInputCorrect(object input)
        {
            if (input.GetType() == (new TextBox()).GetType())
            {
                TextBox textBox = (TextBox)input;

                if (textBox.Text == "" & textBox != textBox1)
                    return true;

                string[] error_value = GetCorrectStringValue(textBox.Text);
                string error = error_value[0];

                if (error == "1")
                {
                    return false;
                }

                return true;
            }
            else if (input.GetType() == (new ComboBox()).GetType())
            {
                ComboBox comboBox = (ComboBox)input;

                if (comboBox.Text == "")
                    return false;
                return true;
            }
            return false;
        }
        // Возвращает true только в том случае, когда все вводы корректны
        private bool AreInputsCorrect(Dictionary<object, PictureBox> input_condition)
        {
            foreach (var item in input_condition)
            {
                if (!IsInputCorrect(item.Key))
                    return false;
            }
            return true;
        }
        // Изменяет состояние полей ввода
        private void ChangeInputCondition(object input, bool condition)
        {
            this.labelPrediction.Font = new Font("Sitka Small", 10, FontStyle.Italic);
            this.labelPrediction.ForeColor = Color.Gray;
            this.labelPrediction.Text = "Заполните и отправьте анкету, чтобы получить результат";
            this.picture_result.Image = null;

            PictureBox pictureBox = input_condition_dict[input];

            if (condition == true)
            {
                pictureBox.Image = Properties.Resources.yes;
            }
            else
            {
                pictureBox.Image = Properties.Resources.no;
            }
        }
        // В зависимости от работы модели выводит результат на форму
        private void ChangePredictionLabel(float result, string model_name)
        {
            if (InvokeRequired)
                Invoke((Action<float, string>)ChangePredictionLabel, result, model_name);
            else
            {
                if (result == -1)
                {
                    this.picture_result.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
                    this.picture_result.Image = Properties.Resources.loading;
                    return;
                }

                string str_score = "";

                if (model_name == "RFR_model")
                {
                    double threshold = 0.33669679687801124;
                    int score;
                    if (result <= threshold)
                    {

                        score = 100 - (int)Math.Round(result * 50 / threshold);
                        result = 0;
                    }
                    else
                    {
                        score = (int)Math.Round((1 - result) * 50 / (1 - threshold));
                        result = 1;
                    }
                    str_score = String.Format("{0}", score);
                }

                this.labelScore.Text = str_score;
                if (result == 0)
                {
                    this.picture_result.Image = Properties.Resources.score_bg;
                    Image arrow = Properties.Resources.arrow;
                    this.pictureBoxArrow.Image = RotateImage(arrow, 50);

                    this.picture_result.Refresh();

                    this.labelPrediction.Font = new Font("Sitka Small", 14, FontStyle.Bold);
                    this.labelPrediction.ForeColor = Color.Green;
                    this.labelPrediction.Text = "Кредит одобрен.";
                    this.labelPrediction.Refresh();

                    this.labelScore.ForeColor = Color.Green;
                    return;
                }
                if (result == 1)
                {
                    this.picture_result.Image = Properties.Resources.score_bg;
                    Image arrow = Properties.Resources.arrow;
                    this.pictureBoxArrow.Image = RotateImage(arrow, 50);

                    this.picture_result.Refresh();

                    this.labelPrediction.Font = new Font("Sitka Small", 14, FontStyle.Bold);
                    this.labelPrediction.ForeColor = Color.Red;
                    this.labelPrediction.Text = "Кредит не одобрен.";
                    this.labelPrediction.Refresh();

                    this.labelScore.ForeColor = Color.Red;
                    return;
                }
                this.labelPrediction.ForeColor = Color.Blue;
                this.labelPrediction.Text = "Произошла непредвиденная ошибка";
                return;
            }
        }
        public static Image RotateImage(Image img, float rotationAngle)
        {
            Bitmap bmp = new Bitmap(img.Width, img.Height);
            Graphics gfx = Graphics.FromImage(bmp);
            gfx.TranslateTransform((float)bmp.Width / 2, (float)bmp.Height / 2);
            gfx.RotateTransform(rotationAngle);
            gfx.TranslateTransform(-(float)bmp.Width / 2, -(float)bmp.Height / 2);
            gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
            gfx.DrawImage(img, new Point(0, 0));
            gfx.Dispose();
            return bmp;
        }
        private void ThreadMessage()
        {
            MessageBox.Show("Результат скоро отобразится...");
        }
        // Создает 2 потока - вывод сообщения о работе модели и поток для вычислений. В результате работы обращается к методу ChangePredictionLabel
        private void GetCatBoostPrediction(string input_name, string model_name)
        {

            Thread t_message = new Thread(new ThreadStart(ThreadMessage));
            t_message.Start();

            float result = -1;
            ThreadStart thread_model_prediction_start = new ThreadStart(() =>
            {
                DeactivateElements();

                Process process = new Process();
                ChangePredictionLabel(result, model_name);

                process.StartInfo.FileName = Environment.CurrentDirectory + "\\scoring\\dist\\" + model_name + ".exe";
                process.StartInfo.Arguments = "";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardInput = true;
                process.Start();

                process.StandardInput.WriteLine(Environment.CurrentDirectory + "\\" + input_name);
                string str_result = process.StandardOutput.ReadToEnd();
                result = ConvertStringToFloat(str_result);
                process.WaitForExit();

                ChangePredictionLabel(result, model_name);
                ActivateElements();
            });


            Thread thread_model_prediction = new Thread(thread_model_prediction_start);
            thread_model_prediction.Start();
        }

        // Обработчик события нажатия на стрелки вниз и вверх внутри textBox
        void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            int index = textBoxesOrder.IndexOf(sender);



            switch (e.KeyCode)
            {
                case Keys.Up:
                    {
                        if (sender.GetType() == (new ComboBox()).GetType())
                            if (((ComboBox)sender).DroppedDown == true)
                                break;

                        if (index > 0)
                        {
                            if (textBoxesOrder[index - 1].GetType() == (new TextBox()).GetType())
                            {
                                TextBox prevTextBox = (TextBox)textBoxesOrder[index - 1];
                                prevTextBox.SelectionStart = 0;
                                prevTextBox.SelectionLength = prevTextBox.Text.Length;
                                prevTextBox.Focus();
                            }
                            else
                            {
                                ComboBox prevComboBox = (ComboBox)textBoxesOrder[index - 1];
                                prevComboBox.Focus();
                                prevComboBox.DroppedDown = true;
                            }
                        }
                        break;
                    }
                case Keys.Down:
                    {
                        if (sender.GetType() == (new ComboBox()).GetType())
                            if (((ComboBox)sender).DroppedDown == true)
                                break;

                        if (index < textBoxesOrder.Count() - 1)
                        {
                            if (textBoxesOrder[index + 1].GetType() == (new TextBox()).GetType())
                            {
                                TextBox nextTextBox = (TextBox)textBoxesOrder[index + 1];
                                nextTextBox.SelectionStart = 0;
                                nextTextBox.SelectionLength = nextTextBox.Text.Length;
                                nextTextBox.Focus();
                            }
                            else
                            {
                                ComboBox nextComboBox = (ComboBox)textBoxesOrder[index + 1];
                                nextComboBox.Focus();
                                nextComboBox.DroppedDown = true;
                            }
                        }
                        break;
                    }
                case Keys.Enter:
                    {
                        if (sender.GetType() == (new ComboBox()).GetType())
                        {
                            ComboBox currentComboBox = (ComboBox)sender;
                            currentComboBox.DroppedDown = false;
                        }
                        if (index < textBoxesOrder.Count() - 1)
                        {
                            if (textBoxesOrder[index + 1].GetType() == (new TextBox()).GetType())
                            {
                                TextBox nextTextBox = (TextBox)textBoxesOrder[index + 1];
                                nextTextBox.SelectionStart = 0;
                                nextTextBox.SelectionLength = nextTextBox.Text.Length;
                                nextTextBox.Focus();
                            }
                            else
                            {
                                ComboBox nextComboBox = (ComboBox)textBoxesOrder[index + 1];
                                nextComboBox.Focus();
                                nextComboBox.DroppedDown = true;
                            }
                        }
                    }
                    break;
            }
        }
        // Обработчик события изменения TextBox
        private void Input_Changed(object sender, EventArgs e)
        {
            ChangeInputCondition(sender, IsInputCorrect(sender));
        }
        // Обработчики события нажатия на кнопку
        private void Button_StartPrediction(object sender, System.EventArgs e)
        {
            foreach (var item in input_condition_dict)
            {
                ChangeInputCondition(item.Key, IsInputCorrect(item.Key));
            }

            if (!AreInputsCorrect(input_condition_dict))
                return;

            string LOAN = GetCorrectStringValue(textBox1.Text)[1];
            string MORTDUE = GetCorrectStringValue(textBox11.Text)[1];
            string VALUE = GetCorrectStringValue(textBox2.Text)[1];
            string DEROG = GetCorrectStringValue(textBox4.Text)[1];
            string DELINQ = GetCorrectStringValue(textBox5.Text)[1];
            string CLAGE = GetCorrectStringValue(textBox6.Text)[1];
            string NINQ = GetCorrectStringValue(textBox7.Text)[1];
            string INCOME = GetCorrectStringValue(textBox9.Text)[1];

            string DEBTINC = "";

            float totalDEBT = ConvertStringToFloat(LOAN);
            if (MORTDUE != "")
                totalDEBT += ConvertStringToFloat(MORTDUE);

            if (LOAN != "" & INCOME != "")
            {
                if (ConvertStringToFloat(INCOME) != 0)
                    DEBTINC = ConvertFloatToString(totalDEBT / ConvertStringToFloat(INCOME) * 100);
            }

            string csv_filename = "input.csv";

            using (var sw = new StreamWriter(Environment.CurrentDirectory + "\\" + csv_filename, false, Encoding.UTF8))
            {
                sw.WriteLine("LOAN,MORTDUE,VALUE,DEROG,DELINQ,CLAGE,NINQ,DEBTINC");
                sw.WriteLine(LOAN + "," + MORTDUE + "," + VALUE + "," + DEROG + "," + DELINQ + "," + CLAGE + "," + NINQ + "," + DEBTINC);
            }
            //if ((Button)sender == button1)
            GetCatBoostPrediction(csv_filename, "RFR_model");

            return;
        }
        private void Button_Refresh(object sender, System.EventArgs e)
        {
            textBox1.Focus();
            foreach (var item in input_condition_dict)
            {
                if (item.Key.GetType() == (new TextBox()).GetType())
                    ((TextBox)item.Key).Text = "";

                item.Value.Image = null;
            }
            this.labelPrediction.Font = new Font("Sitka Small", 10, FontStyle.Italic);
            this.labelPrediction.ForeColor = Color.Gray;
            this.labelPrediction.Text = "Заполните и отправьте анкету, чтобы получить результат";

            this.labelScore.Text = "";

            this.picture_result.Image = null;
        }
        private void Button_CloseForm(object sender, EventArgs e)
        {
            Close();
        }
        private void Button_GetInformation(object sender, EventArgs e)
        {
            AboutBox1 aboutProgramm = new AboutBox1();
            aboutProgramm.ShowDialog();
        }
        // Активация/деакивация элементов на форме во время работы модели
        private void DeactivateElements()
        {
            if (InvokeRequired)
                Invoke((Action)DeactivateElements);
            else
            {
                button1.Enabled = false;
                button2.Enabled = false;

                foreach (var item in input_condition_dict)
                {
                    if (item.Key.GetType() == (new TextBox()).GetType())
                        ((TextBox)item.Key).ReadOnly = true;
                    else
                        ((ComboBox)item.Key).Enabled = false;
                }
            }
        }
        private void ActivateElements()
        {
            if (InvokeRequired)
                Invoke((Action)ActivateElements);
            else
            {
                button1.Enabled = true;
                button2.Enabled = true;

                foreach (var item in input_condition_dict)
                {
                    if (item.Key.GetType() == (new TextBox()).GetType())
                        ((TextBox)item.Key).ReadOnly = false;
                    else
                        ((ComboBox)item.Key).Enabled = true;
                }
            }
        }
    }
}
