using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using ZedGraph;
using System.IO;


namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        //Define cmd
        const char STX = (char)2;
        const char ETX = (char)3;
        const char ACK = (char)6;
        const char KP = (char)17;
        const char KI = (char)18;
        const char KD = (char)19;
        const char SP = (char)21;
        const char RUN = (char)20;
        const char PAUSE = (char)24;
        const char TEST = (char)4;
        const char RESET = (char)5;
        double i = 0;
        string data;
        string b = string.Empty;
        bool Flag_check = true;
        double realtime, datas;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.DataSource = SerialPort.GetPortNames();
            GraphPane myPane = Graph.GraphPane;
            myPane.Title.Text = "Đồ thị vị trí theo thời gian";
            myPane.XAxis.Title.Text = "Thời gian (ms)";
            myPane.YAxis.Title.Text = "Vị trí";
            RollingPointPairList list = new RollingPointPairList(60000);
            LineItem curve = myPane.AddCurve("Value", list, Color.Red, SymbolType.None);
            myPane.XAxis.Scale.Min = 0;
            myPane.XAxis.Scale.Max = 30;
            myPane.XAxis.Scale.MinorStep = 1;
            myPane.XAxis.Scale.MajorStep = 5;
            myPane.YAxis.Scale.Min = -100;
            myPane.YAxis.Scale.Max = 100;
            myPane.AxisChange();


        }
        //Connect Port
        private void button1_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {

            }
            else
            {
                serialPort1.PortName = comboBox1.Text;
                serialPort1.Open();
                label5.Text = "Connected";
            }

        }

        private void fontDialog1_Apply(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
        //Disconnect Port
        private void button2_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen) {
                serialPort1.Close();
                label5.Text = "Disconnected";
            }
        }


        //Send KP
        private void button5_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    double x = Convert.ToDouble(textBoxkp.Text);
                    Warning(x, textBoxkp.Text);
                    sendvalue(KP, x);
                }
                catch {
                    status.Text = "Nothing to send";
                }
            }
        }

        private void Warning(double x, string a)
        {
            if ((x > 99999.999) || (a == null))
            {
                status.Text = "OverRange Number";
                return;
            }
            else status.Text = "Sent";
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void textBoxki_TextChanged(object sender, EventArgs e)
        {

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    double x = Convert.ToDouble(textBoxki.Text);
                    Warning(x, textBoxki.Text);
                    sendvalue(KI, x);
                }
                catch
                {
                    status.Text = "Nothing to send";
                }

            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    double x = Convert.ToDouble(textBoxkd.Text);
                    Warning(x, textBoxkd.Text);
                    sendvalue(KD, x);
                }
                catch
                {
                    status.Text = "Nothing to send";
                }
            }
        }

        private void sendvalue( char cmd, double x )
        {
            if (x >= 0)
            serialPort1.Write(STX + "" + cmd + x.ToString("+00000.000") + ETX);
            else serialPort1.Write(STX + "" + cmd + x.ToString("00000.000") + ETX);
            timer4.Start();

        }

        private void label7_Click(object sender, EventArgs e)
        {
           
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void Graph_Load(object sender, EventArgs e)
        {

        }
        //Turn on/off Timer to send data
        private void button8_Click(object sender, EventArgs e)
        {
            if (!timer1.Enabled)
            {
                timer1.Start();
                timer2.Start();
                button8.Text = "STOP";
               

            }
            else
            {
                timer1.Stop();
                timer2.Stop();
                button8.Text = "START";
            }
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }
        //Timer Interrupt
        private void timer1_Tick(object sender, EventArgs e)
        {
 
                if (serialPort1.IsOpen)
                {
                Draw();
                //Write data to text file
                StreamWriter write = new StreamWriter("TEST", true);
               
                if (Flag_check != true)
                {
                    sendvalue(TEST, i);
                    write.Write("Send:  " + i + "        ");
                    write.WriteLine("Receive:  Fail");
                }
                if ((Flag_check == true))
                {
                    Flag_check = false;
                    if (b==i.ToString()) write.WriteLine("Receive:  " + b);
                    if (i < 10000) i++;
                    else i = 0;
                    textBoxSend.Text = i.ToString();
                    sendvalue(TEST, i);
                    write.Write("Send:  " + i + "        ");
                    
                }
                write.Close();

            }
        }
        //Interrupt Receive via uart
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            data = serialPort1.ReadExisting();
            timer4.Stop();
            foreach (char c in data) //FInd if ack is existing
            {
               if ((c == ACK)&&(data[0] == STX)&&(data[data.Length-1] == ETX))  //[STX .... ACK ETX]
                {
                    b = null; 
                    Flag_check = true;
                    for (int j = 1;j < data.Length - 1;j++) {
                        if (Char.IsDigit(data[j])) //Get number from string
                            b += data[j];
                    }
                    if (data[1] == 45) b = "-"+b;
                   // textBoxReceive.Text = data;
                     Display(b);
                    datas = Convert.ToDouble(b);
                }
            }
        }
        private delegate void DlDisplay(string data);
        private void Display(string data)
        {
            if (textBoxReceive.InvokeRequired)
            {
                DlDisplay sd = new DlDisplay(Display);
                textBoxReceive.Invoke(sd, new object[] { data });
            }
            else textBoxReceive.Text = "STX"+data+"ACK"+"ETX";
        }
        private void textBoxReceive_TextChanged(object sender, EventArgs e)
        {

        }
        private
    void Draw()
        {

            if (Graph.GraphPane.CurveList.Count <= 0)
                return;

            LineItem curve = Graph.GraphPane.CurveList[0] as LineItem;

            if (curve == null)
                return;

            IPointListEdit list = curve.Points as IPointListEdit;

            if (list == null)
                return;

            list.Add(realtime, datas); // Thêm điểm trên đồ thị

            Scale xScale = Graph.GraphPane.XAxis.Scale;
            Scale yScale = Graph.GraphPane.YAxis.Scale;

            // Tự động Scale theo trục x
            if (realtime > xScale.Max - xScale.MajorStep)
            {
                xScale.Max = realtime + xScale.MajorStep;
                xScale.Min = xScale.Max - 30;
            }

            // Tự động Scale theo trục y
            if (datas > yScale.Max - yScale.MajorStep)
            {
                yScale.Max = datas + yScale.MajorStep;
            }
            else if (datas < yScale.Min + yScale.MajorStep)
            {
                yScale.Min = datas - yScale.MajorStep;
            }

            Graph.AxisChange();
            Graph.Invalidate();
           Graph.Refresh();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    double x = Convert.ToDouble(textBoxsp.Text);
                    Warning(x, textBoxsp.Text);
                    sendvalue(SP, x);
                }
                catch
                {
                    status.Text = "Nothing to send";
                }
            }
        }

        private void RUN_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                timer2.Start();
                timer3.Start();
                int x = 0;
                serialPort1.Write(STX + "" + RUN + x.ToString("0000000000") + ETX);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                timer2.Stop();
                timer3.Stop();
                int x = 0;
                serialPort1.Write(STX + "" + PAUSE + x.ToString("0000000000") + ETX);
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                int x = 0;
                serialPort1.Write(STX + "" + RESET + x.ToString("0000000000") + ETX);
                resetVal();
                ClearZedGraph();
            }
        }

        private void resetVal()
        {
            realtime = 0;
            i = 0;
            datas = 0;
            textBoxReceive.Text = "";
            textBoxSend.Text = "";

        }
        private
    void ClearZedGraph()
        {
            Graph.GraphPane.CurveList.Clear(); // Xóa đường
            Graph.GraphPane.GraphObjList.Clear(); // Xóa đối tượng

            Graph.AxisChange();
            Graph.Invalidate();

            GraphPane myPane = Graph.GraphPane;
            myPane.Title.Text = "Đồ thị vị trí theo thời gian";
            myPane.XAxis.Title.Text = "Thời gian (ms)";
            myPane.YAxis.Title.Text = "Vị trí";

            RollingPointPairList list = new RollingPointPairList(60000);
            LineItem curve = myPane.AddCurve("Value", list, Color.Red, SymbolType.None);

            myPane.XAxis.Scale.Min = 0;
            myPane.XAxis.Scale.Max = 30;
            myPane.XAxis.Scale.MinorStep = 1;
            myPane.XAxis.Scale.MajorStep = 5;
            myPane.YAxis.Scale.Min = -100;
            myPane.YAxis.Scale.Max = 100;

            Graph.AxisChange();
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            Draw();
        }

        private void colorDialog1_HelpRequest(object sender, EventArgs e)
        {

        }

        private void status_Click(object sender, EventArgs e)
        {

        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            status.Text = "Send Fail!";
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            realtime++;
        }
    }
}
