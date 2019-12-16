using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
//轨道的最终时间：MIdiToCsv.endtime
namespace midiresolute
{
    /// <summary>
    /// 用来播放wav文件的类
    /// </summary>
    class WavReader
    {
        #region
        string wavPath;//.wav音频文件路径
        Form1 view;
        # endregion
        public WavReader(string path,Form1 frm1)
        {
            wavPath = path;
            view = frm1;
        }
        public void playsound()
        {
            System.Media.SoundPlayer player = new System.Media.SoundPlayer(wavPath);
            player.Play();//简单播放一遍
        }
    }
    /// <summary>
    /// 负责将midi文件转化为csv格式的类
    /// </summary>
    class MidiToCsv
    {
        static int endtime;//最终轨道的结束时间
        const string addr= @"../../../../input/chag.bat";//wav文件的位置
        const string csvaddr = @"../../../../input/input.csv";//csv文件的位置
        /// <summary>
        /// 该函数会在input.mid下生成一个对应midi的csv文件，不需要读文件
        /// </summary>
        public static void run()
        {
            Process proc = null;
            try
            {
                proc = new Process();
                proc.StartInfo.FileName = addr;
                proc.StartInfo.Arguments = string.Format("10");//this is argument
                proc.StartInfo.CreateNoWindow = false;
                proc.Start();
                proc.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Occurred :{0},{1}", ex.Message, ex.StackTrace.ToString());
            }
        }
        /// <summary>
        /// 用来读入内存当中的函数，给出的结果是量化对齐后的
        /// </summary>
        public static void readintoMemo(out List<MideEvent> final)
        {
            string alltext;
            try
            {
                alltext = File.ReadAllText(csvaddr);
                var textlist = alltext.Split(new char[] {'\r','\n' }, StringSplitOptions.RemoveEmptyEntries);
                var medi = new List<string>(textlist);//每次重启之后final可以赋新的值
                absoluteTime(medi,out final);//开始建立简表
                quanti(final);//开始进行量化操作
            }
            catch(Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
                final = null;
            }
        }
        /// <summary>
        /// 该函数用来给和弦命名编号，重要
        /// </summary>
        /// <param name="down">未命名的和弦简表</param>
        public static void nameEachChord(List<ChordEvent> down)
        {
            foreach(ChordEvent o in down)
            {
                //先获取和弦的根音
                int bass = mod(o.keys.Min());
                int translate=0x0;//每个音的二进制位表示.
                int keymap=0x0;//每个和弦的二进制映射
                foreach(var k in o.keys)
                {
                    int keyword = mod(k-bass);
                    switch (keyword)
                    {
                        case 0://主音
                            translate = 0x800;
                            break;
                        case 1:
                            translate = 0x400;
                            break;
                        case 2://9音
                            translate = 0x200;
                            break;
                        case 3:
                            translate = 0x100;
                            break;
                        case 4://上中音
                            translate = 0x80;
                            break;
                        case 5://下属音
                            translate = 0x40;
                            break;
                        case 6:
                            translate = 0x20;
                            break;
                        case 7://属音，五音
                            translate = 0x10;
                            break;
                        case 8:
                            translate = 0x8;
                            break;
                        case 9://下中音
                            translate = 0x4;
                            break;
                        case 10:
                            translate = 0x2;
                            break;
                        case 11://七音
                            translate = 0x1;
                            break;
                        default:
                            translate = 0x800;
                            break;
                    }
                    keymap = keymap | translate;
                }
                string suffix = ChordFindList(keymap);//后缀名
                string prefix;//根音名
                switch(bass)
                {
                    case 0:
                        prefix = "C";
                        break;
                    case 1:
                        prefix = "#C";
                        break;
                    case 2:
                        prefix = "D";
                        break;
                    case 3:
                        prefix = "#D";
                        break;
                    case 4:
                        prefix = "E";
                        break;
                    case 5:
                        prefix = "F";
                        break;
                    case 6:
                        prefix = "#F";
                        break;
                    case 7:
                        prefix = "G";
                        break;
                    case 8:
                        prefix = "#G";
                        break;
                    case 9:
                        prefix = "A";
                        break;
                    case 10:
                        prefix = "#A";
                        break;
                    case 11:
                        prefix = "B";
                        break;
                    default:
                        prefix = "U";//U表示未知
                        break;
                }
                o.name = prefix+suffix;
            }
        }
        /// <summary>
        /// 更正化后的求余程序
        /// </summary>
        /// <param name="shou">被求数字</param>
        /// <returns></returns>
        private static int mod(int shou)
        {
            int preResult = shou % 12;
            if (preResult >= 0)
                return preResult;
            else
                return mod(preResult+12);
        }
        /// <summary>
        /// 将keymap找到对应的和弦名的函数
        /// </summary>
        /// <param name="keymap">16进制的keymap</param>
        /// <returns>命名后的和弦名</returns>
        private static string ChordFindList(int keymap)
        {
            string preName;
            //寻找7音
            switch (keymap& 0x983)
            {//先识别下面的7和弦，然后寻找高叠音
                case 0x982:
                case 0x882:
                    preName = "7";
                    keymap &= 0x77d;
                    break;
                case 0x881:
                case 0x883:
                    preName = "maj7";
                    keymap &= 0x77e;
                    break;
                case 0x902:
                    preName = "m7";
                    keymap &= 0x6fd;
                    break;
                default:
                    preName = "?7";
                    break;
            }
            //寻找9音
            switch(keymap& 0x700)
            {
                case 0x400:
                    preName += "-b9";
                    keymap &= 0xbff;
                    break;
                case 0x200:
                    preName += "-9";
                    keymap &= 0xdff;
                    break;
                case 0x100:
                    preName += "-#9";
                    keymap &= 0xeff;
                    break;
                case 0:
                default:
                    break;
            }
            //寻找11音
            switch (keymap & 0xe0)
            {
                case 0x80:
                    preName += "-b11";
                    keymap &= 0xf7f;
                    break;
                case 0x40:
                    preName += "-11";
                    keymap &= 0xfbf;
                    break;
                case 0x20:
                    preName += "-#11";
                    keymap &= 0xfdf;
                    break;
                case 0:
                    break;
                default:
                    preName += "-?11";
                    break;
            }
            //寻找13音
            switch (keymap & 0xe)
            {
                case 0x8:
                    preName += "-b13";
                    break;
                case 0x4:
                    preName += "-13";
                    break;
                case 0:
                    break;
                default:
                    preName += "-?13";
                    break;
            }
            return preName;
        }
        /// <summary>
        /// 将旋律简表转化为和弦简表的函数
        /// </summary>
        /// <param name="up">旋律简表</param>
        /// <param name="down">函数简表</param>
        public static void chgIntoChord(List<MideEvent> up,out List<ChordEvent> down)
        {
            down = new List<ChordEvent>();
            ChordEvent temp=new ChordEvent();
            temp.name = "begin";
            int currentTime = 0;//当前时间
            foreach(var o in up)
            {
                //如果发生在和弦的转变点
                if (o.absoluteTime > currentTime)
                {
                    //如果这是一个音头
                    if (o.holdon == true)
                    {
                        if (temp.name != "begin") //前面还有和弦
                        {
                            down.Add(temp);//上一个和弦已经编写完成，加入列表
                        }
                        temp = new ChordEvent();
                        temp.name = "unknown";//标记不为空和弦
                        temp.keys = new List<int>();
                        temp.keys.Add(o.key);
                        temp.startTime = o.absoluteTime;
                        currentTime = o.absoluteTime;
                    }
                    //如果这是一个音尾
                    else
                    {
                        temp.endTime = o.absoluteTime;
                        currentTime = o.absoluteTime;
                    }
                }
                else//这是发生在和弦内部
                {
                    if(o.holdon==true)//这是一个音头
                    {
                        temp.keys.Add(o.key);
                    }
                }
            }
            down.Add(temp);//将最后一个和弦添加进去
        }
        /// <summary>
        /// 对midi信息进行量化处理，把音头对齐。
        /// </summary>
        /// <param name="final">midi列表中间件</param>
        private static void quanti(List<MideEvent> postmid)
        {
            //首先按照绝对时间进行排序
            postmid.Sort(( a,  b) => { return a.absoluteTime  > b.absoluteTime ? 1 : -1; });
            int left=0, right=0;
            int size = postmid.Count;
            for(;right<size;right++)
            {
                if(postmid[right].absoluteTime-postmid[left].absoluteTime>200)
                {
                    int standard = postmid[left].absoluteTime;
                    for(int i=left+1;i!=right;i++ )
                    {
                        postmid[i].absoluteTime = standard;
                    }
                    left = right;
                }
            }

        }
        /// <summary>
        /// 将Midi列表中的时间处理为绝对时间，单位转化为毫秒
        /// </summary>
        /// <param name="final">midi列表中间件</param>
        private static void absoluteTime(List<string> mid,out List<MideEvent> postmid)
        {
            int unitTime;
            int unitTick;
            //查找unitTime
            string uTimeItem=mid.Find((string ostr) => ostr.Contains("Tempo"));
            string[] sp1 = uTimeItem.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            unitTime =Convert.ToInt32(sp1[3]);
            //查找unitTick
            var uTickItem = mid.Find((string ostr) => ostr.Contains("Header"));
            string[] sp2= uTickItem.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            unitTick = Convert.ToInt32(sp2[5]);
            double unitTimeMs = unitTime / 1000;
            double timePerTickMs = unitTimeMs / unitTick;
            //开始建立midi事件简表，对左侧的tick改成绝对时间
            postmid= new List<MideEvent>();
            foreach(string eachline in mid)
            {
                if(eachline.Contains("Note_on_c")||eachline.Contains("Note_off_c"))
                {
                    string[] spride = eachline.Split(',' );
                    int changedTime = Convert.ToInt32(timePerTickMs * Convert.ToDouble(spride[1]));
                    int keynumber = Convert.ToInt32(spride[4]);
                    bool hold;
                    if (eachline.Contains("Note_on_c"))
                    {
                        if (Convert.ToInt32(spride[5]) != 0)
                            hold = true;
                        else
                            hold = false;
                    }
                    else
                    {
                        hold = false;
                    }
                    MideEvent newEvent = new MideEvent(changedTime, keynumber, hold);
                    postmid.Add(newEvent);
                }
                //读入EndTrack标记之后的操作
                if(eachline.Contains("End_track"))
                {
                    string[] temp=eachline.Split(',');
                    int changedTime = Convert.ToInt32(timePerTickMs * Convert.ToInt32(temp[1]));
                    MidiToCsv.endtime = changedTime;
                }
            }
        }
    }




}
