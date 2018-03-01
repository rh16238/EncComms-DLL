using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EncComms_Server
{
    interface ILoggable
    {
         void AddToLog(String s);
    }
    public partial class ServerUI : Form, ILoggable
    {
        public ServerUI()
        {
            InitializeComponent();
            Server s = new Server(this, this);
            

            
        }

        public void AddToLog(string s)
        {
            if (listBoxLog.InvokeRequired)
            {
                listBoxLog.Invoke((MethodInvoker)delegate { listBoxLog.Items.Add(s); });
            }
            else { listBoxLog.Items.Add(s); }
        }

        public void updateClientList(object[] names)
        {

            if (listBoxConnected.InvokeRequired)
            {
                listBoxConnected.Invoke((MethodInvoker)delegate { listBoxConnected.Items.Clear(); listBoxConnected.Items.AddRange(names); });
            }
            else { listBoxConnected.Items.Clear(); listBoxConnected.Items.AddRange(names); }
        }
    }
}
