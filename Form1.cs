using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace midiresolute
{
    #region struct
    class MideEvent
    {
        public int absoluteTime;//绝对时间
        public int key;
        public bool holdon;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="a">绝对时间</param>
        /// <param name="b">按键号</param>
        /// <param name="c">按下（抬起）</param>
        public MideEvent(int a, int b, bool c) {
            absoluteTime = a;
            key = b;
            holdon = c;
        }
    }
    class ChordEvent
    {
        public List<int> keys;//每个柱式和弦内的音符
        public string name;//和弦的名称
        public int startTime;//每个柱式和弦的开始时间
        public int endTime;//每个柱式和弦的结束时间
    }
    #endregion
    public partial class Form1 : Form
    {
        #region abbr
        string wavpath =  @"../../../../input/input.wav";//wav文件的位置
        List<MideEvent> midcsv;//readintoMemo()函数中完成创建
        List<ChordEvent> chrcsv;//和弦表

        System.Timers.Timer t;//定时器
        bool startorend;//true means start;
        int index;//现在执行到第几个和弦了
        #endregion
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 设置文本的委托
        /// </summary>
        /// <param name="text">需要传入的文本</param>
        delegate void SetTextCallback(string text);
        /// <summary>
        /// 可以在别的线程调用的函数
        /// </summary>
        /// <param name="text">需要显示的内容</param>
        private void SetText(string text)
        {
            if (this.InvokeRequired)
            {
                var hander = new SetTextCallback(SetText);
                this.Invoke(hander, text);
            }
            else
            {
                this.textBox1.Text = text;
            }

        }
        private void button1_Click(object sender, EventArgs e)
        {
            index = 0;
            MidiToCsv.run();//将Midi文件转化为csv格式方便处理
            //将csv文件读入内存并进行处理
            MidiToCsv.readintoMemo(out midcsv);
            MidiToCsv.chgIntoChord(midcsv,out chrcsv);
            MidiToCsv.nameEachChord(chrcsv);
            //开始实时更新和弦显示
            beginDisplay(chrcsv);
            //开始播放音乐
            WavReader wavrd = new WavReader(wavpath,this);
            wavrd.playsound();
        }
        private void beginDisplay(List<ChordEvent> list)
        {
            int startTime = list[0].startTime;
            t = new System.Timers.Timer(startTime);//实例化Timer类，设置间隔时间为10000毫秒；
            t.Elapsed += new System.Timers.ElapsedEventHandler(theout);//到达时间的时候执行事件；
            t.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；
            t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
            startorend = true;
        }
        private void theout(object source, System.Timers.ElapsedEventArgs e)
        {
            if(startorend)//开始阶段
            {
                //this.textBox1.Text = chrcsv[index].name;
                SetText(chrcsv[index].name);
                t.Interval = chrcsv[index].endTime - chrcsv[index].startTime;//设置下一次的时间间隔
                startorend = false;
            }
            else
            {
                SetText("");
                if(index==chrcsv.Count-1)
                {
                    t.Enabled = false;//所有的和弦显示完毕，就终止它吧。
                    return;
                }
                index++;
                int nextTime = chrcsv[index].startTime-chrcsv[index-1].endTime;
                t.Interval = nextTime;
                startorend = true;
            }
        }
    }

}
