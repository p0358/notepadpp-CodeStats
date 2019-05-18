using System;
using System.Windows.Forms;

namespace CodeStats.Forms
{
    public partial class SettingsForm : Form
    {
        private readonly ConfigFile _CodeStatsConfigFile;
        internal event EventHandler ConfigSaved;
        private ToolTip apiurlToolTip;

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
                if (String.IsNullOrWhiteSpace(_CodeStatsConfigFile.ApiUrl))
                {
                    txtAPIURL.Text = Constants.ApiMyPulsesEndpoint;
                }
                else
                {
                    txtAPIURL.Text = _CodeStatsConfigFile.ApiUrl;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error when loading form SettingsForm", ex);
                MessageBox.Show(ex.Message);
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                if (CodeStatsPackage._apikeyForm != null && CodeStatsPackage._apikeyForm.Visible)
                {
                    CodeStatsPackage._apikeyForm.Close();
                }
            }
            finally { }

            try
            {
                string apiKey = txtAPIKey.Text.Trim();       
                                     
                //if (true)
                //{
                    if (String.IsNullOrWhiteSpace(txtAPIURL.Text) || txtAPIURL.Text == Constants.ApiMyPulsesEndpoint)
                    {
                        //_CodeStatsConfigFile.ApiUrl = Constants.ApiMyPulsesEndpoint;
                        _CodeStatsConfigFile.ApiUrl = string.Empty;
                    }
                    else
                    {
                        // API URL was changed, previous one was default
                        if (_CodeStatsConfigFile.ApiUrl != txtAPIURL.Text && String.IsNullOrWhiteSpace(_CodeStatsConfigFile.ApiUrl))
                        {
                        // Show confirmation
                            if ((MessageBox.Show("API URL should only be changed if you intend to use another instance of Code::Stats service, for example beta or private, unpublic one.\nIt should normally be kept to default. If you put wrong URL here, no pulses will be registered and all your XP will be lost.\n\n" + txtAPIURL.Text + "\nDo you still want to save this API URL?", "Are you sure?",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                                MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes))
                            {
                                _CodeStatsConfigFile.ApiUrl = txtAPIURL.Text;
                            }
                            else
                            {
                                this.FocusTxtAPIURL();
                                this.ShowAPIURLTooltip();
                                this.DialogResult = DialogResult.None;
                                return;
                            }
                        }
                        else _CodeStatsConfigFile.ApiUrl = txtAPIURL.Text;
                    }

                    _CodeStatsConfigFile.ApiKey = apiKey;
                    _CodeStatsConfigFile.Proxy = txtProxy.Text.Trim();
                    _CodeStatsConfigFile.Debug = chkDebugMode.Checked;
                    _CodeStatsConfigFile.Stats = chkStats.Checked;

                    _CodeStatsConfigFile.Save();

                    CodeStatsPackage._hasAlreadyShownInvalidApiTokenMessage = false;

                    if (chkStats.Checked && !CodeStatsPackage._reportedStats)
                    {
                        CodeStatsPackage.ReportStats();
                    }

                    OnConfigSaved();
                /*}
                else // - kept in case we check API tokens in future
                {
                    MessageBox.Show(@"Please enter valid API token.");
                    DialogResult = DialogResult.None; // do not close dialog box
                }*/
            }
            catch (Exception ex)
            {
                Logger.Error("Error when saving data from SettingsForm", ex);
                MessageBox.Show(ex.Message);
            }
        }

        protected virtual void OnConfigSaved()
        {
            var handler = ConfigSaved;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public void FocusTxtAPIURL(bool tooltipShowAlways = false)
        {
            this.ActiveControl = txtAPIURL;
            this.txtAPIURL.Focus();
            this.txtAPIURL.SelectAll();
        }

        public void ShowAPIURLTooltip(bool tooltipShowAlways = false)
        {
            // https://www.c-sharpcorner.com/uploadfile/mahesh/tooltip-in-C-Sharp/
            // https://stackoverflow.com/questions/168550/display-a-tooltip-over-a-button-using-windows-forms
            /*ToolTip*/
            apiurlToolTip = new ToolTip();
            apiurlToolTip.ToolTipTitle = "Tip";
            //apiurlToolTip.IsBalloon = true;
            apiurlToolTip.ShowAlways = tooltipShowAlways;
            apiurlToolTip.Show("To restore the default API URL, simply remove the contents of this field and hit Save.", txtAPIURL);
            //apiurlToolTip.SetToolTip(txtAPIURL, "To restore the default API URL, simply remove the contents of this field and hit Save.");
        }

    }
}
