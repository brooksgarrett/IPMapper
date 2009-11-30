using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace com.brooksgarrett.ipInfo
{
    public partial class MasterForm : Form
    {
        string fileName = @"C:\Documents and Settings\e10674\Desktop\ip.txt";
        ipInfoHelper helper = new ipInfoHelper();

        public MasterForm()
        {
            InitializeComponent();
        }

        private void MasterForm_Load(object sender, EventArgs e)
        {
            string line;
            List<string> ipList = new List<string>();
            StreamReader input = new StreamReader(File.OpenRead(fileName));
            while ((line = input.ReadLine()) != null)
            {
                ipList.Add(line);
            }
            helper.getInfo(ipList);
            dataGridView1.DataSource = helper.getipInformation();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            string ip = dataGridView1.Rows[e.RowIndex].Cells["IP"].Value.ToString();
            string lat = dataGridView1.Rows[e.RowIndex].Cells["Latitude"].Value.ToString();
            string lon = dataGridView1.Rows[e.RowIndex].Cells["Longitude"].Value.ToString();

            helper.generateKML();
        }
    }
}
