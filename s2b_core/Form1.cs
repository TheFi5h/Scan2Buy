using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OBID.TagHandler;

namespace Scan2Buy
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (buttonStart.Text.Equals("Starten"))
            {
                ReaderCommunicator rc = ReaderCommunicator.GetInstance();
                Logger.GetInstance().Log("F1: Received Instance of ReaderCommunicator");
                rc.Connect();
                Logger.GetInstance().Log("F1: Connected Reader Communicator");
                rc.ActivateScan();
                Logger.GetInstance().Log("F1: Activated Scanning");

                while (rc.Scanning == false)
                {
                } // wait for Scanner to start

                toolStripStatusLabel.Text = "Status: gestartet";

                buttonStart.Text = "Update Scanned Tags";
            }
            else
            {
                ReaderCommunicator rc = ReaderCommunicator.GetInstance();
                Logger.GetInstance().Log("F1: Received Instance of Reader Communicator");

                Dictionary<string, string> scannedTags = rc.GetScannedTags();
                Logger.GetInstance().Log("F1: Called Method GetScannedTags");

                if (scannedTags.Count > 0)
                {
                    Logger.GetInstance().Log($"F1: Received {scannedTags.Count} scanned Tags");

                    listBox.BeginUpdate();      // stop redrawing of the listbox while adding items

                    foreach (var item in scannedTags)
                    {
                        listBox.Items.Add($"Key: {item.Key} | Value: {item.Value}");
                    }
                    listBox.EndUpdate();        // start redrawing the listbox again
                }
            }


        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            ReaderCommunicator rc = ReaderCommunicator.GetInstance();

            rc.DeactivateScan();
            rc.Disconnect();

            buttonStart.Text = "Starten";
            toolStripStatusLabel.Text = "Status: gestoppt. Eventuell noch Tags in Communicator gespeichert!";
        }
    }
}
