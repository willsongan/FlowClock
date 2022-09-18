using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows.Forms;

namespace FlowClock
{
    public partial class mainForm : Form
    {
        #region DraggableWindow

        public const int WM_NCLBUTTONDOWN = 0xA1;

        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        #endregion

        //timer
        System.Timers.Timer timer;
        Action timerBehavior;
        int milisecond, second, minute;
        public bool isPlaying;
        public bool isResting;

        //timeLabelFont
        public float minTimeLabelFont = 21.75f;
        public float maxTimeLabelFont = 38.25f;

        public mainForm()
        {
            InitializeComponent();
            contentTableLayout.MouseWheel += ContentTableLayout_MouseWheel;
        }

        private void ContentTableLayout_MouseWheel(object sender, MouseEventArgs e)
        {
            if(e.Delta > 0)
            {
                Size += new Size(6,5);
                var fontSizeAfter = timeLabel.Font.Size + 0.8f;
                if (fontSizeAfter > maxTimeLabelFont) fontSizeAfter = maxTimeLabelFont;
                timeLabel.Font = new Font(timeLabel.Font.FontFamily,fontSizeAfter);
            }
            else
            {
                Size -= new Size(5,6);
                var fontSizeBefore = timeLabel.Font.Size - 1;
                if (fontSizeBefore < minTimeLabelFont) fontSizeBefore = minTimeLabelFont;
                timeLabel.Font = new Font(timeLabel.Font.FontFamily, fontSizeBefore);
            }
        }

        private void mainForm_Load(object sender, EventArgs e)
        {
            //mainForm
            TopMost = true;
            Size = MinimumSize;

            //closeButton
            closeButton.TabStop = false;
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);

            //timer
            timer = new System.Timers.Timer();
            timer.Interval = 10;
            timerBehavior = CountUpTimer;
            timer.Elapsed += OnTimeEvent;
            button2.Enabled = false;

            //button
            button1.MouseUp += Button1_MouseUp;
            button2.MouseUp += Rest;
            button3.MouseUp += Button3_MouseUp;
        }

        private void Button3_MouseUp(object sender, MouseEventArgs e)
        {
            timer.Stop();
            SetTimerZero();
            Print();
            isPlaying = false;
            button1.Text = ">";
            button2.Enabled = false;
        }

        private void Button1_MouseUp(object sender, MouseEventArgs e)
        {
            //initial start, isPlaying jadi pause/stop
            if (!isPlaying)
            {
                timerBehavior = CountUpTimer;
                timer.Start();
                isPlaying = true;
                button1.Text = "||";
            }
            else
            {
                timer.Stop();
                isPlaying = false;
                button1.Text = ">";
            }
        }

        private void Rest(object sender, MouseEventArgs e)
        {
            isPlaying = false;
            isResting = true;

            minute = 5;
            second = 0;
            milisecond = 0;

            timerBehavior = CountDownTimer;
            timer.Start();

            button2.MouseUp -= Rest;
            button2.MouseUp += IncreaseRestTime;

            button3.Enabled = false;
        }

        private void OnTimeEvent(object sender, ElapsedEventArgs e)
        {
            Invoke(timerBehavior);
            Invoke(new Action(SetRestButtonState));
        }

        private void SetRestButtonState()
        {
            if (isPlaying)
            {
                bool passFlowTime = minute > 14;
                button2.Enabled = passFlowTime;
            }
            
            if (isResting)
            {
                button2.Text = "m+=1";
            }
        }

        private void IncreaseRestTime(object sender, MouseEventArgs e)
        {
            minute += 1;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            //is this needed? just to be safe first
            Dispose();

            Close();
        }

        private void tableLayoutPanel2_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }

        private void CountUpTimer()
        {
            Print();
            CountUp();

            void CountUp()
            {
                milisecond += 1;
                if (milisecond > 99)
                {
                    second += 1;
                    milisecond = 0;
                }

                if (second == 60)
                {
                    minute += 1;
                    second = 0;
                }
            }
        }

        bool isMinSize = false;
        private void contentTableLayout_DoubleClick(object sender, EventArgs e)
        {
            if(isMinSize)
            {
                Size = MaximumSize;
                timeLabel.Font = new Font(timeLabel.Font.FontFamily, maxTimeLabelFont);
                isMinSize = false;
            }
            else
            {
                Size = MinimumSize;
                timeLabel.Font = new Font(timeLabel.Font.FontFamily, minTimeLabelFont);
                isMinSize = true;
            }
            
        }

        private void Print(bool showMiliSec = true)
        {
            if(showMiliSec)
            {
                var m = minute.ToString().PadLeft(2, '0');
                var s = second.ToString().PadLeft(2, '0');
                var ms = milisecond.ToString().PadLeft(2, '0');
                timeLabel.Text = string.Format("{0}:{1}:{2}", m, s, ms);
            }
            else
            {
                var m = minute.ToString().PadLeft(2, '0');
                var s = second.ToString().PadLeft(2, '0');
                timeLabel.Text = string.Format("{0}:{1}", m, s);
            }
            
        }

        private void CountDownTimer()
        {
            button1.Enabled = false;
            CountDown();
            Print(showMiliSec:false);

            void CountDown()
            {
                milisecond -= 1;

                if (milisecond > 0) return;
                milisecond += 99;
                second -= 1;
                if (second > 0) return;
                second += 60;
                minute -= 1;
                if (minute >= 0) return;
                RestFinished();
            }
        }

        private void SetTimerZero()
        {
            minute = 0;
            second = 0;
            milisecond = 0;
        }

        private void RestFinished()
        {
            timer.Stop();
            SetTimerZero();
            isResting = false;
            button2.Text = "Zzz";
            button2.MouseUp -= IncreaseRestTime;
            button2.MouseUp += Rest;
            button2.Enabled = false;
            button1.Text = ">";
            button1.Enabled = true;
            button3.Enabled = true;
        }
    }
}
