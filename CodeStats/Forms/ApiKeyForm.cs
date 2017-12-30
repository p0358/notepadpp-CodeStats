using System;
using System.Windows.Forms;

namespace CodeStats.Forms
{
    public partial class ApiKeyForm : Form
    {
        private readonly ConfigFile _CodeStatsConfigFile;        

        public ApiKeyForm()
        {            
            InitializeComponent();

            _CodeStatsConfigFile = new ConfigFile();
        }

        private void ApiKeyForm_Load(object sender, EventArgs e)
        {
            try
            {
                txtAPIKey.Text = _CodeStatsConfigFile.ApiKey;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }            
        }
        
        private void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                string apiKey = txtAPIKey.Text.Trim();                         
                if (true)
                {
                    _CodeStatsConfigFile.ApiKey = apiKey;
                    _CodeStatsConfigFile.Save();
                    CodeStatsPackage.ApiKey = apiKey;
                }
                else
                {
                    MessageBox.Show("Please enter valid Api Key.");
                    DialogResult = DialogResult.None; // do not close dialog box
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://codestats.net");
        }
    }
}
