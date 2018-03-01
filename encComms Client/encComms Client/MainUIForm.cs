using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EncCommsPacketsDll;
using System.Threading;
namespace encComms_Client
{
    public partial class MainUIForm : Form
    {
        const string IP = "127.0.0.1";
        public MainUIForm()
        {
            InitializeComponent();
            //Tcp_Connection connection = new Tcp_Connection();
         //   connection.Connect(IP, 11000);
         //   LogIDPacket p = new LogIDPacket("Test ID");
          //  connection.sendPacket(p.toTransmittable());

            Engine c = new Engine(this);
            c.connectToServer(IP, 11000);
            
            c.broadCastIDPacket("TestConnection");

        }
    }
}
