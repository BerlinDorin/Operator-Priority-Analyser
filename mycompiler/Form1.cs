using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CCWin;
namespace mycompiler
{  

    public partial class Form1 : CCSkinMain
    {
      
        public Form1()
        {
            InitializeComponent();
            
            richTextBox3.Text = "Waiting for your input...\r\n";
        }
        struct ST
        {
            public char a;
            public char b;
        };
        //输入的规则数量
        private int countgrammar;
        //处理后的规则数量
        private int countc_grammar;
        //终结符数量
        private int countvt;
        //非终结符数量
        private int countvn;
        //优先关系矩阵
        private char[,] priority_array;
        //输入的规则
        string[] Grammar;
        //处理后的规则
        string[] c_Grammar;
        //判断vt是否已经加入过Firstvt和Lastvt矩阵
        int[,] Fflag;
        int[,] Lflag;
        //Firstvt和Lastvt矩阵
        char[,] Firstvt;
        char[,] Lastvt;
        //终结符集合
        char[] Vt;
        char[] Vn;
        private int countt_grammar;
        string[] t_Grammar;
        //数据初始化
        private void init()
        {
            Firstvt = new char[30, 30];
            Lastvt = new char[30, 30];
            Grammar = new string[30];
            c_Grammar = new string[30];
            t_Grammar = new string[30];
            Fflag = new int[30,30];
            Lflag = new int[30, 30];
            priority_array = new char[40, 40];
            Vt = new char[40];
            Vn = new char[40];
            for(int i = 0; i < 40; i++)
            {
                Vt[i] = Vn[i] = '\0';
            }
            countvn = 0;
            countvt = 0;
        }
        //判断文法合法性
        private bool isGrammar()
        {
            for(int i = 0; i < countgrammar; i++)
            {
                if (Grammar[i]==""||Grammar[i][0] < 'A' || Grammar[i][0] > 'Z')
                {
                   
                    MessageBox.Show("存在不合法的非终结符！");
                    return false;
                }
                for (int j = 3; j < Grammar[i].Length; j++)
                {   
                    if (Grammar[i][j] <= 'Z' && Grammar[i][j] >= 'A')
                    {
                        if (j+1< Grammar[i].Length&&Grammar[i][j+1] <= 'Z' && Grammar[i][j+1] >= 'A')
                        {
                            MessageBox.Show("存在两个相连的非终结符！");
                            return false;
                        }
                    }  
                }
            }
            return true;
        }
        //判断终结符并加入Vt
        private void Getvt()
        {
           
           
            for (int i = 0; i < countgrammar; i++)
            {
                for (int j = 3; j < Grammar[i].Length; j++)
                {
                    char tmp = Grammar[i][j];
                    if (tmp != '|' &&tmp!=' '&& (tmp < 'A' || tmp > 'Z'))
                    { 
                        if (isVt(tmp) == -1)
                        {
                            Vt[countvt] = tmp;
                            countvt++;
                        }
                       
                    }
                }
            }
            if (isVt('#') == -1) {
                Vt[countvt] = '#';
                countvt++;
            }
          
        }
        //判断非终结符并加入Vn
        private void Getvn()
        {
           
            int flag = -1;
            for (int i = 0; i < countgrammar; i++)
            {
                flag = isVn(Grammar[i][0]);
                if (flag == -1)
                {
                    Vn[countvn] = Grammar[i][0];
                    countvn++;
                }
            }
        }
        //判断一个字符是否为非终结符,并返回索引
        private int isVn(char x)
            {
            for(int i = 0; i < countvn; i++)
              {
                if (x == Vn[i]) return i;
              }
            return -1;
            }
        //判断一个字符是否为终结符,并返回索引
        private int isVt(char x)
        {
            for(int i = 0; i < countvt; i++)
            {
                if (x == Vt[i]) return i;
            }
            return -1;
        }

         //分离产生式
        private void SplitGrammar()
        {
            countc_grammar = 0;
            for (int i = 0; i < countgrammar; i++)
            {
                
                for(int j = 0; j < Grammar[i].Length; j++)
                {
                    if (Grammar[i][j] == '|')
                    {
                       // c_Grammar[countc_grammar] += '\0';
                        countc_grammar++;
                        c_Grammar[countc_grammar] += Grammar[i][0];
                        c_Grammar[countc_grammar] += Grammar[i][1];
                        c_Grammar[countc_grammar] += Grammar[i][2];
                    }
                    else
                    {
                        c_Grammar[countc_grammar] += Grammar[i][j];
                    }
                    //c_Grammar[countc_grammar] += '\0';
                   
                }
                countc_grammar++;
            }
        }
        //产生Firstvt
        private void getFirstvt()
        {   
            //非终结符和终结符位置
            int vnpos ;
            int vtpos ;
            Stack<ST> st=new Stack<ST>();
            ST temp, s;
            for(int i=0;i<30;i++)            //赋初值
                for(int j = 0; j < 30; j++)
                {
                    Fflag[i, j] = 0;
                }
           for(int i = 0; i < countc_grammar; i++)
            {
                vnpos = isVn(c_Grammar[i][0]);
                for(int j = 3; j<c_Grammar[i].Length; j++)
                {
                    vtpos = isVt(c_Grammar[i][j]);
                    if (vtpos != -1)
                    {
                        Fflag[vnpos,vtpos] = 1;
                        break;
                    }
                }
            }
           for(int i = 0; i < countvn; i++)
            {
                for(int j = 0; j < countvt; j++)
                {
                    if (Fflag[i, j] == 1)
                    {
                        s.a = Vn[i];
                        s.b = Vt[j];
                        st.Push(s);
                    }
                }
            }

            while (st.Count() != 0)
            {
                s = st.Pop();
                for(int i = 0; i < countc_grammar; i++)
                {
                    if (c_Grammar[i][3] == s.a)
                    {
                        vnpos = isVn(c_Grammar[i][0]);
                        vtpos = isVt(s.b);
                        if (vnpos != -1 && vtpos != -1)
                        {
                            if (Fflag[vnpos, vtpos] == 0)
                            {
                                Fflag[vnpos, vtpos] = 1;
                                temp.a = c_Grammar[i][0];
                                temp.b = s.b;
                                st.Push(temp);
                            }
                        }
                    }
                }

            }
            for(int i = 0; i < countvn; i++)
            {
                int count = 0;
                for (int j = 0; j < countvt; j++)
                {
                    if (Fflag[i, j] == 1)
                    {
                        Firstvt[i, count] = Vt[j];
                        count++;
                    }
                   
                }
                Firstvt[i, count] = '\0';
             
            }
           
        }

        //产生Lastvt
        private void getLastvt()
        {
            //非终结符和终结符位置
            int vnpos;
            int vtpos;
            Stack<ST> st = new Stack<ST>();
            ST temp, s;
            for (int i = 0; i < 30; i++)            //赋初值
                for (int j = 0; j < 30; j++)
                {
                    Lflag[i, j] = 0;
                }
            for (int i = 0; i < countc_grammar; i++)
            {
                vnpos = isVn(c_Grammar[i][0]);
                for (int j = c_Grammar[i].Length-1; j>=3; j--)
                {
                    vtpos = isVt(c_Grammar[i][j]);
                    if (vtpos != -1)
                    {
                        Lflag[vnpos, vtpos] = 1;
                        break;
                    }
                }
            }
            for (int i = 0; i < countvn; i++)
            {
                for (int j = 0; j < countvt; j++)
                {
                    if (Lflag[i, j] == 1)
                    {
                        s.a = Vn[i];
                        s.b = Vt[j];
                        st.Push(s);
                    }
                }
            }

            while (st.Count() != 0)
            {
                s = st.Pop();
                for (int i = 0; i < countc_grammar; i++)
                {
                    if (c_Grammar[i][c_Grammar[i].Length-1] == s.a)
                    {
                        int len = c_Grammar[i].Length - 1;
                        vnpos = isVn(c_Grammar[i][0]);
                        vtpos = isVt(s.b);
                        if (vnpos != -1 && vtpos != -1)
                        {
                            if (Lflag[vnpos, vtpos] == 0)
                            {
                                Lflag[vnpos, vtpos] = 1;
                                temp.a = c_Grammar[i][0];
                                temp.b = s.b;
                                st.Push(temp);
                            }
                        }
                    }
                }

            }
            for (int i = 0; i < countvn; i++)
            {
                int count = 0;
                for (int j = 0; j < countvt; j++)
                {
                    if (Lflag[i, j] == 1)
                    {
                        Lastvt[i, count] = Vt[j];
                        count++;
                    }
                    
                }
                Lastvt[i, count] = '\0';

            }
        }
        //进行算符优先分析
        private bool getPriority_array()
        {
            for (int i = 0; i < 40; i++)
                for (int j = 0; j < 40; j++) priority_array[i, j] = ' ';

            for(int i = 0; i < countc_grammar; i++)
            {
                for(int j = 0; j < c_Grammar[i].Length; j++)
                {
                    int x1, x2, x3;
                    x1 = isVt(c_Grammar[i][j]);
                    if (x1 != -1)
                    {
                        //x_i和x_i+1 均为终结符
                        if (j + 1 < c_Grammar[i].Length)
                        {
                            x2 = isVt(c_Grammar[i][j + 1]);
                            if (x2 != -1)
                            {
                                if (priority_array[x1, x2] == ' ' || priority_array[x1, x2] == '=')
                                {
                                    priority_array[x1, x2] = '=';
                                }
                                else
                                {
                                    MessageBox.Show("优先关系出现冲突！", "ERROR");
                                    return false;
                                }
                            }

                        }
                        //x_i和x_i+2 为终结符, x_i+1 为非终结符
                        if (j + 2 < c_Grammar[i].Length)
                        {
                            x2 = isVn(c_Grammar[i][j + 1]);
                            x3 = isVt(c_Grammar[i][j + 2]);
                            if (x3 != -1 && x2 != -1)
                            {
                                if (priority_array[x1, x3] == ' ' || priority_array[x1, x3] == '=')
                                {
                                    priority_array[x1, x3] = '=';
                                }
                                else
                                {
                                    MessageBox.Show("aAa优先关系出现冲突！", "ERROR");
                                    return false;
                                }
                            }
                        }
                        //x_i 为终结符，x_i+1 为非终结符
                        if (j + 1 < c_Grammar[i].Length)
                        {
                            x2 = isVn(c_Grammar[i][j + 1]);
                            if (x2 != -1)
                            {
                                for (int k = 0; Firstvt[x2, k] != '\0'; k++)
                                {
                                    x3 = isVt(Firstvt[x2, k]);
                                    if (x3 != -1)
                                    {
                                        if (priority_array[x1, x3] == ' ' || priority_array[x1, x3] == '<')
                                        {
                                            priority_array[x1, x3] = '<';
                                        }
                                        else
                                        {
                                            MessageBox.Show("aA优先关系出现冲突！", "ERROR");
                                            return false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    //x_i 为非终结符，x_i+1 为终结符
                    else
                    {
                        if (j + 1 < c_Grammar[i].Length)
                        {
                            x2 = isVt(c_Grammar[i][j + 1]);
                            if (x2 != -1)
                            {
                                int tmp = isVn(c_Grammar[i][j]);
                                if (tmp != -1)
                                {
                                    for (int k = 0; Lastvt[tmp, k] != '\0'; k++)
                                    {
                                        x3 = isVt(Lastvt[tmp, k]);
                                        if (x3 != -1)
                                        {
                                            if (priority_array[x3, x2] == ' ' || priority_array[x3, x2] == '>')
                                            {
                                                priority_array[x3, x2] = '>';
                                            }
                                            else
                                            {
                                                MessageBox.Show("Aa优先关系出现冲突！", "ERROR");
                                                return false;
                                            }
                                        }
                               
                                    }
                                }
                            }
                        }
                    }
                    

                }                
            }
            
            //for(int i = 0; i < countvt - 1; i++)
            //{
            //    priority_array[i, countvt - 1] = '>';
            //    priority_array[countvt - 1, i] = '<';
            //}
            //priority_array[countvt - 1, countvt - 1] = ' ';
            return true;

        }


        private bool PriorityAnalysis(string str)
        {
            int i= 0,top=0; 
            //p指向栈顶,s指向分析串
            //int s = 0, p = 1;
            string stk = "#";
            int len = str.Length;
            int countstep = 1;
            //char a, b;
            Convert();
            if (str[len-1] != '#')
            {
                MessageBox.Show("该句型缺少结束符！");
                return false;
            }

            while (i < len)
            {
                if (top >= stk.Length)
                {
                    MessageBox.Show(top.ToString());
                }
                char l = stk[top];
                char r = str[i];
                if (l=='E')
                {
                    l = stk[top - 1];
                }
                if (isVt(l) == -1 || isVt(r) == -1) return false;
                int x = isVt(l);
                int y = isVt(r);

                if (priority_array[x, y] == '<' || priority_array[x, y] == '=')
                {
                    richTextBox3.Text += OutFormat(countstep.ToString(), stk, priority_array[x, y].ToString(), r.ToString(), str.Substring(i + 1, str.Length - i - 1), "移进");
                    countstep++;
                    stk += str[i];
                    top++;
                    i++;
                }
                else if(priority_array[x, y] == '>')
                {
                    richTextBox3.Text += OutFormat(countstep.ToString(), stk, priority_array[x, y].ToString(), r.ToString(), str.Substring(i + 1, str.Length - i - 1), "规约");
                    countstep++;

                    int ii, jj, kk;
                    int flag = 0;
                    for (ii = top; ii >= 0; ii--)
                    {
                        for (jj = 0; jj <countt_grammar; jj++)
                        {
                            int kkn = 0;

                            for (kk = t_Grammar[jj].Length - 1; kk >= 0; kk--)
                            {
                               
                                if (stk[ii] == t_Grammar[jj][kk])
                                {
                                    ii--;
                                    kkn++;
                                }
                                else { break; }
                            }
                            if (t_Grammar[jj].Length == kkn)
                            {
                                top = ii + 1;
                                stk = stk.Substring(0, top);
                                
                                stk += 'E';
                                
                                flag = 1;
                                break;
                            }
                            else
                            {
                                ii = ii + kkn;
                            }
                        }
                        if (flag!=1)
                        {
                            richTextBox3.Text += "规约失败\r\n";
                            return false;
                        }
                        else
                        {
                            if (top == 1 && str[i] == '#')
                            {
                                richTextBox3.Text += OutFormat(countstep.ToString(), stk,'='.ToString() , '#'.ToString(), "", "成功");
                                return true;
                            }
                            break;
                        }
                    }
                }
            }
            return true;
        }
        //对单词表的输出结果进行格式化
        private string OutFormat(string a, string b, string c, string d,string e,string f)
        {
            //return a.PadRight(25, ' ') + b.PadRight(25, ' ') + c.PadRight(25, ' ') + d.PadRight(25, ' ') + e.PadRight(25, ' ') + f.PadRight(25, ' ')+"\r\n";
            return string.Format("{0,-25} {1,-25} {2,-30} {3,-32} {4,-35} {5,-30}", a, b, c, d,e ,f) + "\r\n";
            
        }
        private void Convert()
        {
            countt_grammar = 0;
            for(int i = 0; i < countc_grammar; i++)
            {
                string tmp = "";
                if (c_Grammar[i].Length == 4 && isVn(c_Grammar[i][3]) != -1)
                {
                    continue;
                }

                for (int j = 3; j < c_Grammar[i].Length; j++)
                {
                    if (isVn(c_Grammar[i][j]) != -1)
                    {
                        tmp += 'E';
                        continue;
                    }
                    tmp += c_Grammar[i][j];
                }
                t_Grammar[countt_grammar] = tmp;
                countt_grammar++;
            }
        }
        //--------------------界面部分-----------------------------------------------------
        private void skinButton1_Click(object sender, EventArgs e)
        {
            //init();
            richTextBox1.Text = "";
           
            richTextBox3.Text = "Waiting....";

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "../../";
            openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;   //默认打开txt文件
            openFileDialog1.RestoreDirectory = true;

            var fileContent = string.Empty;
            var filePath = string.Empty;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Get the path of specified file
                filePath = openFileDialog.FileName;

                //Read the contents of the file into a stream
                var fileStream = openFileDialog.OpenFile();

                try
                {
                    StreamReader reader = new StreamReader(fileStream);
                    string line;    

                    while ((line = reader.ReadLine()) != null)
                    {
                        richTextBox1.Text += line;
                        richTextBox1.Text += "\r\n";
                    }
                    reader.Close();
                }
                catch (IOException)
                {
                    MessageBox.Show("请选择一个有效的文件！");
                }
            }
        }

        private void skinButton2_Click(object sender, EventArgs e)
        {
           
        }

        private void skinButton3_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = ".txt"; // Default file extension
            saveFileDialog.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            // Show save file dialog box
            var filePath = string.Empty;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Get the path of specified file
                filePath = saveFileDialog.FileName;
                try
                {
                    StreamWriter writer = new StreamWriter(filePath);

                    writer.Write(richTextBox2.Text);
                    writer.Close();

                }
                catch (IOException)
                {
                    MessageBox.Show("保存失败！");
                }
            }
        }

        
       
        //输出为结果
        private void ShowResult()
        {
            richTextBox2.Text += "##########################VN集########################\r\n";
            for (int i = 0; i < countvn; i++)
            {
                richTextBox2.Text += Vn[i].ToString() + " ";
            }
            richTextBox2.Text += "\r\n";
            richTextBox2.Text += "##########################VT集########################\r\n";
            for (int i = 0; i < countvt; i++)
            {
                richTextBox2.Text += Vt[i].ToString() + " ";
            }
            richTextBox2.Text += "\r\n";
            richTextBox2.Text += "##########################产生式########################\r\n";
            for(int i = 0; i < countc_grammar; i++)
            {
                richTextBox2.Text += c_Grammar[i] + "\r\n";
            }
            richTextBox2.Text += "##########################FIRST集########################\r\n";
            for(int i = 0; i < countvn; i++)
            {
                richTextBox2.Text += Vn[i].ToString() + " : ";
                for (int j = 0; j < countvt; j++)
                {
                    if (Fflag[i, j] == 1) richTextBox2.Text += Vt[j].ToString() + " ";
                }
                richTextBox2.Text += "\r\n";
            }
            richTextBox2.Text += "##########################LAST集########################\r\n";
            for (int i = 0; i < countvn; i++)
            {
                richTextBox2.Text += Vn[i].ToString() + " : ";
                for (int j = 0; j < countvt; j++)
                {
                    if (Lflag[i, j] == 1) richTextBox2.Text += Vt[j].ToString() + " ";
                }
                richTextBox2.Text += "\r\n";
            }
            richTextBox2.Text += "#############################################算符优先关系表############################################\r\n";
            richTextBox2.Text += "|\t\t";
            for (int i = 0; i < countvt; i++)
            {
                richTextBox2.Text += "|\t" + Vt[i] + "\t";
            }
            richTextBox2.Text += "|\r\n";
            for (int i = 0; i < countvt; i++)
            {
                richTextBox2.Text += "|\t"+Vt[i].ToString()+"\t";
                for (int j = 0; j < countvt; j++)
                {
                    richTextBox2.Text += "|\t" + priority_array[i, j].ToString() + "\t";
                }
                richTextBox2.Text += "|\r\n";
            }
            richTextBox2.Text += "##########################(改变)产生式########################\r\n";
            for (int i = 0; i < countt_grammar; i++)
            {
                richTextBox2.Text += t_Grammar[i] + "\r\n";
            }

        }
        private void dealerror(int lineID)
        {


        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void skinButton4_Click(object sender, EventArgs e)
        {
            richTextBox2.Text = "";
            init();
            if (richTextBox1.Text == "")
            {
                MessageBox.Show("输入的文法为空！", "ERROR");

            }
            if (rtfRichTextBox1.Text == "")
            {
                MessageBox.Show("输入的句型为空！", "ERROR");

            }
            richTextBox3.Text = "";


            countgrammar = 0;
            foreach (string nextLine in richTextBox1.Lines)
            {
                Grammar[countgrammar] = nextLine;
                countgrammar++;
            }

            if (isGrammar())
            {
                Getvn();
                Getvt();
                SplitGrammar();
                getFirstvt();
                getLastvt();

                
                if (getPriority_array() == false)
                {
                    return;
                }
                
                richTextBox3.Text += "步骤\t\t 栈\t\t优先关系\t\t当前符号\t\t剩余符号\t\t    动作\r\n";
                foreach (string nextLine in rtfRichTextBox1.Lines)
                {
                    PriorityAnalysis(nextLine);
                }
                ShowResult();

            }
            foreach (string nextLine in rtfRichTextBox1.Lines)
            {
                //PriorityAnalysis(nextLine);
            }
            //ShowResult();

            richTextBox3.Text += "mission accomplished!!\r\n";

        }
    }
}
