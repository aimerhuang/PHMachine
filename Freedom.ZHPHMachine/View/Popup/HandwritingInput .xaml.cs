using Freedom.Common;
using Freedom.WinAPI;
using Microsoft.Ink;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Freedom.ZHPHMachine.View.Popup
{
    /// <summary>
    /// HandwritingInput.xaml 的交互逻辑
    /// </summary>
    public partial class HandwritingInput : Window
    {
        private InkCollector myInkCollector = null;
        RecognizerContext rct;
        string FullCACText = string.Empty;

        public HandwritingInput()
        {
            InitializeComponent();

            inkInput.DefaultDrawingAttributes.Width = 5;
            inkInput.DefaultDrawingAttributes.Height = 5;
            this.inkInput.DefaultDrawingAttributes.StylusTip = System.Windows.Ink.StylusTip.Ellipse;

            try
            {
                //获取识别器上下文
                Recognizers recos = new Recognizers();
                Recognizer chineseReco = recos.GetDefaultRecognizer();
                rct = chineseReco.CreateRecognizerContext();
                rct.RecognitionFlags = Microsoft.Ink.RecognitionModes.WordMode;
                //设置识别事件处理
                this.rct.RecognitionWithAlternates += new RecognizerContextRecognitionWithAlternatesEventHandler(rct_RecognitionWithAlternates);
            }
            catch (Exception ex)
            {
                Log.Instance.WriteError("手写板初始化失败" + ex.Message);
            }


        }

        public string StrContent
        {
            get
            {
                return txt.Text;
            }
            set
            {
                txt.Text = value;
                if (value != null)
                {
                    txt.SelectionStart = value.Length;
                }
            }
        }

        private string title;
        public new string Title
        {
            get { return title; }
            set
            {
                title = value;
                tb.Text = value;
            }
        }

        private int maxLength;
        public int MaxLength
        {
            get { return maxLength; }
            set
            {
                maxLength = value;
                txt.MaxLength = value;
            }
        }
        private void rct_RecognitionWithAlternates(object sender, RecognizerContextRecognitionWithAlternatesEventArgs e)
        {
            try
            {
                string ResultString = "";
                RecognitionAlternates alts = null;

                //识别正常，获取候选字
                if (e.RecognitionStatus == RecognitionStatus.NoError)
                {
                    alts = e.Result.GetAlternatesFromSelection();

                    foreach (RecognitionAlternate alt in alts)
                    {
                        ResultString = ResultString + alt + " ";
                    }
                }
                System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
                FullCACText = alts.Strokes.ToString();
                System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void inkInput_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            MemoryStream ms = new MemoryStream();
            inkInput.Strokes.Save(ms);
            InkCollector myInkCollector = new InkCollector();
            Ink ink = new Ink();
            ink.Load(ms.ToArray());
            rct.StopBackgroundRecognition();
            rct.Strokes = ink.Strokes;
            rct.BackgroundRecognizeWithAlternates(0);

        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            txtTip.Text = "";
            int index = txt.SelectionStart;
            string content = txt.Text;
            if (content.Length < maxLength)
            {
                txt.Text = content.Insert(index, FullCACText);
                txt.SelectionStart = index + FullCACText.Length;
            }
            FullCACText = string.Empty;
            inkInput.Strokes.Clear();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            txtTip.Text = "";
            inkInput.Strokes.Clear();

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            txt.Text = "";
            txtTip.Text = "";
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (title.Contains("姓名") && !string.IsNullOrWhiteSpace(txt.Text) && !Regex.IsMatch(txt.Text, @"^[\u4e00-\u9fbb]+$"))
            {

                txtTip.Text = "请输入正确中文姓名";
                return;
            }
            DialogResult = true;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            txtTip.Text = "";
            if (sender is Button)
            {
                int index = txt.SelectionStart;
                string str = (sender as Button).Content?.ToString();
                string content = txt.Text;
                if (content.Length < maxLength)
                {
                    txt.Text = content.Insert(index, str);
                    txt.SelectionStart = index + str.Length;
                }

            }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            txtTip.Text = "";
            Win32API.AddKeyBoardINput(0x08);
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
