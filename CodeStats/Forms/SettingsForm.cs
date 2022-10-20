using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static CodeStats.Constants;

namespace CodeStats.Forms
{
    public partial class SettingsForm : Form
    {
        private readonly ConfigFile _CodeStatsConfigFile;
        internal event EventHandler OnConfigSaved;
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
                    txtAPIURL.Text = Constants.ApiEndpoint;
                }
                else
                {
                    txtAPIURL.Text = _CodeStatsConfigFile.ApiUrl;
                }
                LanguageDetectionUIRefresh();
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
                    if (String.IsNullOrWhiteSpace(txtAPIURL.Text) || txtAPIURL.Text == Constants.ApiEndpoint)
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
                            if ((MessageBox.Show("API URL should only be changed if you intend to use another instance of Code::Stats service, for example beta or private, unpublic one.\nIt should normally be kept to default. If you put wrong URL here, no pulses will be registered and all your recorded XP will be lost.\n\n" + txtAPIURL.Text + "\nDo you still want to save this API URL?", "Are you sure?",
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

                    LanguageDetectionUIRefresh();
                    _CodeStatsConfigFile.UseExtensionMapping = chkUseExtensionMapping.Checked;
                    _CodeStatsConfigFile.UseLexerLanguage = chkUseExtensionMapping.Checked;
                    _CodeStatsConfigFile.DetectionPriority = !radioDetectionPriority_extensionMapping.Checked ? Constants.DetectionType.LEXER_LANGUAGE : Constants.DetectionType.EXTENSION_MAPPING;
                    _CodeStatsConfigFile.UseCustomMapping = chkUseCustomMapping.Checked;

                    _CodeStatsConfigFile.Save();

                    CodeStatsPackage._hasAlreadyShownInvalidApiTokenMessage = false;
                    if (chkStats.Checked && !CodeStatsPackage._reportedStats)
                    {
                        CodeStatsPackage.ReportStats();
                    }

                    NotifyOnConfigSaved(); // it destroys the settings form in the handler
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

        protected virtual void NotifyOnConfigSaved()
        {
            var handler = OnConfigSaved;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public void FocusTxtAPIURL()
        {
            this.ActiveControl = txtAPIURL;
            this.txtAPIURL.Focus();
            this.txtAPIURL.SelectAll();
        }

        public void ShowAPIURLTooltip(bool tooltipShowAlways = false)
        {
            // https://www.c-sharpcorner.com/uploadfile/mahesh/tooltip-in-C-Sharp/
            // https://stackoverflow.com/questions/168550/display-a-tooltip-over-a-button-using-windows-forms
            apiurlToolTip = new ToolTip();
            apiurlToolTip.ToolTipTitle = "Tip";
            //apiurlToolTip.IsBalloon = true;
            apiurlToolTip.ShowAlways = tooltipShowAlways;
            apiurlToolTip.Show("To restore the default API URL, simply remove the contents of this field and hit Save.", txtAPIURL);
            //apiurlToolTip.SetToolTip(txtAPIURL, "To restore the default API URL, simply remove the contents of this field and hit Save.");
        }

        private List<string> LanguageDetectionUIRefresh()
        {
            string detectionOrder = "Detection order: ";
            List<string> things = new List<string>();

            if (chkUseCustomMapping.Checked)
            {
                things.Add("custom file extension mapping");
            }

            if (chkUseExtensionMapping.Checked && chkUseLexerLanguage.Checked)
            {
                radioDetectionPriority_extensionMapping.Enabled = true;
                radioDetectionPriority_lexerLanguage.Enabled = true;

                if ((!radioDetectionPriority_extensionMapping.Checked && !radioDetectionPriority_lexerLanguage.Checked)
                    || (radioDetectionPriority_extensionMapping.Checked && radioDetectionPriority_lexerLanguage.Checked))
                {
                    radioDetectionPriority_extensionMapping.Checked = true;
                    radioDetectionPriority_lexerLanguage.Checked = false;
                }

                if (radioDetectionPriority_extensionMapping.Checked)
                {
                    things.Add("file extension mapping");
                    things.Add("lexer language (custom language definitions are unsupported due to plugin interface limitations)");
                }
                else if (radioDetectionPriority_lexerLanguage.Checked)
                {
                    things.Add("lexer language (custom language definitions are unsupported due to plugin interface limitations)");
                    things.Add("file extension mapping");
                }
            }
            else
            {
                radioDetectionPriority_extensionMapping.Enabled = false;
                radioDetectionPriority_lexerLanguage.Enabled = false;

                if (chkUseExtensionMapping.Checked)
                {
                    things.Add("file extension mapping");
                }
                else if (chkUseLexerLanguage.Checked)
                {
                    things.Add("lexer language (custom language definitions are unsupported due to plugin interface limitations)");
                }
            }

            detectionOrder += string.Join(", ", things);
            labelDetectionOrder.Text = detectionOrder;

            return things;
        }

        private void LanguageDetectionUIRefresh(object sender, EventArgs e)
        {
            LanguageDetectionUIRefresh();
        }
    }
}
