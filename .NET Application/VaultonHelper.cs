using System;
using System.Windows.Forms;
using VaultonSDK;

namespace VaultonIntegration
{
    /// <summary>
    /// Simple helper class for quick integration into Windows Forms applications
    /// </summary>
    public static class VaultonHelper
    {
        private static VaultonClient _client;
        private static bool _isLicensed = false;
        private static string _currentLicenseKey = null;

        /// <summary>
        /// Initialize the Vaulton helper with your API key
        /// </summary>
        /// <param name="apiKey">Your API key from the Vaulton dashboard</param>
        /// <param name="baseUrl">Optional custom API URL</param>
        public static void Initialize(string apiKey, string baseUrl = "https://vaulton.vercel.app")
        {
            _client = new VaultonClient(apiKey, baseUrl);
        }

        /// <summary>
        /// Check if the application is currently licensed
        /// </summary>
        public static bool IsLicensed => _isLicensed;

        /// <summary>
        /// Get the current license key
        /// </summary>
        public static string CurrentLicenseKey => _currentLicenseKey;

        /// <summary>
        /// Validate a license key and update the licensed status
        /// </summary>
        /// <param name="licenseKey">License key to validate</param>
        /// <returns>Validation result</returns>
        public static async System.Threading.Tasks.Task<LicenseValidationResult> ValidateLicenseAsync(string licenseKey)
        {
            if (_client == null)
            {
                throw new InvalidOperationException("VaultonHelper not initialized. Call Initialize() first.");
            }

            var result = await _client.ValidateLicenseAsync(licenseKey);
            
            if (result.IsValid)
            {
                _isLicensed = true;
                _currentLicenseKey = licenseKey;
            }
            else
            {
                _isLicensed = false;
                _currentLicenseKey = null;
            }

            return result;
        }

        /// <summary>
        /// Show a simple license dialog and validate the license
        /// Returns true if license is valid, false otherwise
        /// </summary>
        /// <param name="apiKey">Your Vaulton API key</param>
        /// <returns>True if licensed, false if cancelled or invalid</returns>
        public static async System.Threading.Tasks.Task<bool> ShowLicenseDialogAsync(string apiKey)
        {
            Initialize(apiKey);

            using (var form = new Form())
            {
                form.Text = "License Activation";
                form.Size = new System.Drawing.Size(450, 200);
                form.StartPosition = FormStartPosition.CenterScreen;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;

                var lblPrompt = new Label
                {
                    Text = "Enter your license key:",
                    Location = new System.Drawing.Point(20, 20),
                    AutoSize = true
                };

                var txtKey = new TextBox
                {
                    Location = new System.Drawing.Point(20, 45),
                    Size = new System.Drawing.Size(395, 25)
                };

                var lblStatus = new Label
                {
                    Text = "",
                    Location = new System.Drawing.Point(20, 80),
                    Size = new System.Drawing.Size(395, 25),
                    ForeColor = System.Drawing.Color.Gray
                };

                var btnActivate = new Button
                {
                    Text = "Activate",
                    Location = new System.Drawing.Point(260, 115),
                    Size = new System.Drawing.Size(75, 30)
                };

                var btnCancel = new Button
                {
                    Text = "Cancel",
                    Location = new System.Drawing.Point(340, 115),
                    Size = new System.Drawing.Size(75, 30),
                    DialogResult = DialogResult.Cancel
                };

                bool success = false;

                btnActivate.Click += async (s, e) =>
                {
                    string key = txtKey.Text.Trim();
                    if (string.IsNullOrEmpty(key))
                    {
                        lblStatus.ForeColor = System.Drawing.Color.Orange;
                        lblStatus.Text = "Please enter a license key";
                        return;
                    }

                    btnActivate.Enabled = false;
                    lblStatus.ForeColor = System.Drawing.Color.Gray;
                    lblStatus.Text = "Validating...";

                    var result = await ValidateLicenseAsync(key);

                    if (result.IsValid)
                    {
                        success = true;
                        form.DialogResult = DialogResult.OK;
                        form.Close();
                    }
                    else
                    {
                        lblStatus.ForeColor = System.Drawing.Color.Red;
                        lblStatus.Text = result.Message;
                        btnActivate.Enabled = true;
                    }
                };

                form.Controls.AddRange(new Control[] { lblPrompt, txtKey, lblStatus, btnActivate, btnCancel });
                form.AcceptButton = btnActivate;
                form.CancelButton = btnCancel;

                return form.ShowDialog() == DialogResult.OK && success;
            }
        }

        /// <summary>
        /// Require a valid license before proceeding
        /// Shows license dialog if not licensed
        /// Exits application if license is denied
        /// </summary>
        /// <param name="apiKey">Your Vaulton API key</param>
        public static async System.Threading.Tasks.Task RequireLicenseAsync(string apiKey)
        {
            bool licensed = await ShowLicenseDialogAsync(apiKey);
            
            if (!licensed)
            {
                MessageBox.Show("A valid license is required to use this application.", 
                    "License Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(0);
            }
        }
    }
}
