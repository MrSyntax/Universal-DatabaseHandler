using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DBHandler;
using KledingWinkelLib.Objects;
using KledingWinkelLib;

namespace DBHandlerTester
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public static DataBaseHandler dbhandler;
        private void Form1_Load(object sender, EventArgs e)
        {
            dbhandler = new DataBaseHandler(DatabaseType.MSSQL, @"Server=localhost\SYNSERNET;Database=KledingWinkel;User ID=KledingWinkelAppl; Password=kledingwinkel");
            
        }

        private void ExecuteTest()
        {
            DataTable dt = null;
            dbhandler.RegisterType(typeof(Customer), "Customer");
            if (dbhandler.SqlSelectAll(typeof(Customer), out dt))
            {
                dataGridView1.DataSource = dt;
                dataGridView1.Refresh();
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            ExecuteTest();
        }
    }
}
