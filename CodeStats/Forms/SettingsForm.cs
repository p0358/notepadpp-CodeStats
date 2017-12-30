using System;
using System.Windows.Forms;

namespace CodeStats.Forms
{
    public partial class SettingsForm : Form
    {
        private readonly ConfigFile _CodeStatsConfigFile;
        internal event EventHandler ConfigSaved;

        public SettingsForm()
        {
            InitializeComponent();

            _CodeStatsConfigFile = new ConfigFile();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            try
            {
                _CodeStatsConfigFile.Read();
                txtAPIKey.Text = _CodeStatsConfigFile.ApiKey;
                txtProxy.Text = _CodeStatsConfigFile.Proxy;
                chkDebugMode.Checked = _CodeStatsConfigFile.Debug;
                chkStats.Checked = _CodeStatsConfigFile.Stats;
            }
            catch (Exception ex)
            {
                Logger.Error("Error when loading form SettingsForm:", ex);
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
                    _CodeStatsConfigFile.Proxy = txtProxy.Text.Trim();
                    _CodeStatsConfigFile.Debug = chkDebugMode.Checked;
                    _CodeStatsConfigFile.Stats = chkStats.Checked;
                    _CodeStatsConfigFile.Save();

                    if (chkStats.Checked && !CodeStatsPackage._reportedStats)
                    {
                        CodeStatsPackage.ReportStats();
                    }

                    OnConfigSaved();
                }
                else
                {
                    MessageBox.Show(@"Please enter valid Api Key.");
                    DialogResult = DialogResult.None; // do not close dialog box
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error when saving data from SettingsForm:", ex);
                MessageBox.Show(ex.Message);
            }
        }

        protected virtual void OnConfigSaved()
        {
            var handler = ConfigSaved;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
