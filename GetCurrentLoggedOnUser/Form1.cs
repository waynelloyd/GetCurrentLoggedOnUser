using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Management;

namespace GetCurrentLoggedOnUser
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void pingCheck()
        {
            string strComputer = comboBox1.Text;

            Ping objPing = new Ping();
            try
            {
                PingReply objPingReply = objPing.Send(this.comboBox1.Text);
                if (objPingReply.Status == IPStatus.Success)
                {
                    getResults();
                    if (!Properties.Settings.Default.ComboItems.Contains(strComputer))
                    {
                        Properties.Settings.Default.ComboItems.Add(strComputer);
                        this.comboBox1.AutoCompleteCustomSource.Add(strComputer);
                        Properties.Settings.Default.Save();
                    }
                }
                else
                {
                    //// Initializes the variables to pass to the MessageBox.Show method.
                    string message = ("Host " + strComputer + " is unreachable");
                    string caption = string.Empty;
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    DialogResult result;
                    //// Displays the MessageBox.
                    result = MessageBox.Show(this, message, caption, buttons, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                }
            }
            catch (System.Net.NetworkInformation.PingException snnp)
            {
                //// Initializes the variables to pass to the MessageBox.Show method.
                string caption = string.Empty;
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result;
                //// Displays the MessageBox.
                result = MessageBox.Show(snnp.InnerException.Message, caption, buttons, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
            }

        }

        private void getResults()
        {
            string strComputer = comboBox1.Text;
            string LoggedOnUser = "";
            DateTime dtBootTime = new DateTime();
            string OSVersion = "";
            string LongDate = "";
            string LongTime = "";
            try
            {
                ConnectionOptions options = new ConnectionOptions();
                ManagementScope theScope = new ManagementScope("\\\\" + strComputer + "\\root\\cimv2");
                theScope.Options.Impersonation = ImpersonationLevel.Impersonate;
                theScope.Options.EnablePrivileges = true;
                theScope.Connect();
                ObjectQuery ExplorerProcessQuery = new ObjectQuery("SELECT * FROM Win32_Process WHERE Caption = 'explorer.exe'");
                ObjectQuery OperatingSystemQuery = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
                ManagementObjectSearcher ExplorerProcessSearcher = new ManagementObjectSearcher(theScope, ExplorerProcessQuery);
                ManagementObjectSearcher OperatingSystemSearcher = new ManagementObjectSearcher(theScope, OperatingSystemQuery);
                ManagementObjectCollection ExplorerProcessCollection = ExplorerProcessSearcher.Get();
                ManagementObjectCollection OperatingSystemCollection = OperatingSystemSearcher.Get();
                foreach (ManagementObject theCurObject in ExplorerProcessCollection)
                {
                    string[] argList = new string[] { string.Empty, string.Empty };
                    int returnVal = Convert.ToInt32(theCurObject.InvokeMethod("GetOwner", argList));
                    //string[] UsrDom = new String[2];             
                    //theCurObject.InvokeMethod("GetOwner", (object[])UsrDom);
                    //if (UsrDom.Length != 0)
                    if (returnVal == 0)
                    {
                        // return DOMAIN\user
                        LoggedOnUser = (argList[1] + "\\" + argList[0]);
                        //LoggedOnUser = (UsrDom[1] + "\\" + UsrDom[0]);

                    }
                    if (argList.Length == 0)
                    {
                        LoggedOnUser = "";
                    }
                }
                foreach (ManagementObject theCurObject in OperatingSystemCollection)
                {
                    OSVersion = theCurObject.Properties["caption"].Value.ToString();
                    dtBootTime = ManagementDateTimeConverter.ToDateTime(
                    theCurObject.Properties["LastBootUpTime"].Value.ToString());
                    // display the start time and date
                    LongDate = dtBootTime.ToLongDateString();
                    LongTime = dtBootTime.ToLongTimeString();
                }
                if (LoggedOnUser.Length == 0)
                {
                    //// Initializes the variables to pass to the MessageBox.Show method.
                    string caption = string.Empty;
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    DialogResult result;
                    //// Displays the MessageBox.
                    result = MessageBox.Show("No User Detected! " + Environment.NewLine + Environment.NewLine + "OS: " + OSVersion + Environment.NewLine + Environment.NewLine + "System Boot Time: " + LongDate + " " + LongTime, caption, buttons, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    //// Initializes the variables to pass to the MessageBox.Show method.
                    string caption = string.Empty;
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    DialogResult result;
                    //// Displays the MessageBox.
                    result = MessageBox.Show("Logged On User: " + LoggedOnUser + Environment.NewLine + Environment.NewLine + "OS: " + OSVersion + Environment.NewLine + Environment.NewLine + "System Boot Time: " + LongDate + " " + LongTime, caption, buttons, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
            }
            catch (ManagementException me)
            {
                //// Initializes the variables to pass to the MessageBox.Show method.
                string caption = string.Empty;
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result;
                //// Displays the MessageBox.
                result = MessageBox.Show(me.Message, caption, buttons, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
            }
            catch (System.UnauthorizedAccessException)
            {
                //// Initializes the variables to pass to the MessageBox.Show method.
                string caption = "Access Denied";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result;
                //// Displays the MessageBox.
                result = MessageBox.Show("Access denied. The connection to " + strComputer + " could not be established. Check to see that you have sufficient permissions to access Windows Management Instrumentation, and that Windows Management Instrumentation is installed on the computer.", caption, buttons, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                //MessageBox.Show(uae.Message, caption, buttons, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
            }
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AboutBox1 MyAboutBox = new AboutBox1();
            MyAboutBox.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.comboBox1.Text.Length == 0)
                {
                    //// Initializes the variables to pass to the MessageBox.Show method.
                    string message = "Please specify a computer name or IP address";
                    string caption = "No Computer Name or IP Specified";
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    DialogResult result;
                    //// Displays the MessageBox.
                    result = MessageBox.Show(this, message, caption, buttons, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    this.pingCheck();
                }
            }       
            catch (System.Exception ex)
            {
                //// Initializes the variables to pass to the MessageBox.Show method.
                string caption = string.Empty;
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result;
                //// Displays the MessageBox.
                result = MessageBox.Show(ex.Message, caption, buttons, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.ComboItems == null)
            {
                Properties.Settings.Default.ComboItems = new System.Collections.Specialized.StringCollection();
            }
            //// Adds each stored string from user.config to combobox autocompletecustomsource stringcollection
            foreach (string s in Properties.Settings.Default.ComboItems)
            {
                this.comboBox1.AutoCompleteCustomSource.Add(s);
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Properties.Settings.Default.ComboItems.Clear();
            this.comboBox1.AutoCompleteCustomSource.Clear();
            Properties.Settings.Default.Save();
            //// Initializes the variables to pass to the MessageBox.Show method.
            string message = "The autocomplete history has been cleared successfully!";
            string caption = string.Empty;
            MessageBoxButtons buttons = MessageBoxButtons.OK;
            DialogResult result;
            //// Displays the MessageBox.
            result = MessageBox.Show(this, message, caption, buttons, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }


    }

}

