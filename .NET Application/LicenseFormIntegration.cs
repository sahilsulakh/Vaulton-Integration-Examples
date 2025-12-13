using System;
using System.Windows.Forms;
using VaultonSDK;

namespace VaultonIntegration
{
    /// <summary>
    /// Example Windows Forms integration for Vaulton license authentication
    /// Copy this code to your Form that contains:
    /// - txtLicenseKey (TextBox)
    /// - btnAuthenticate (Button)
    /// - lblStatus (Label)
    /// </summary>
    public partial class LicenseForm : Form
    {
        // Initialize the Vaulton client with your API key
        private readonly VaultonClient _vaultonClient;

        public LicenseForm()
        {
            InitializeComponent();

            // Replace with your actual API key from Vaulton dashboard
            _vaultonClient = new VaultonClient("YOUR_API_KEY_HERE");
        }

        /// <summary>
        /// Wire this method to your btnAuthenticate.Click event
        /// </summary>
        private async void btnAuthenticate_Click(object sender, EventArgs e)
        {
            // Get the license key from the textbox
            string licenseKey = txtLicenseKey.Text.Trim();

            // Validate input
            if (string.IsNullOrEmpty(licenseKey))
            {
                lblStatus.ForeColor = System.Drawing.Color.Orange;
                lblStatus.Text = "Please enter a license key";
                return;
            }

            // Disable button during validation
            btnAuthenticate.Enabled = false;
            lblStatus.ForeColor = System.Drawing.Color.Gray;
            lblStatus.Text = "Validating license...";

            try
            {
                // Validate the license
                var result = await _vaultonClient.ValidateLicenseAsync(licenseKey);

                if (result.IsValid)
                {
                    // License is valid - proceed to main application
                    lblStatus.ForeColor = System.Drawing.Color.Green;
                    lblStatus.Text = "✓ License validated successfully!";

                    // Save the license key for future use
                    Properties.Settings.Default.LicenseKey = licenseKey;
                    Properties.Settings.Default.Save();

                    // Optional: Open main form
                    // var mainForm = new MainForm();
                    // mainForm.Show();
                    // this.Hide();
                }
                else
                {
                    // License validation failed
                    lblStatus.ForeColor = System.Drawing.Color.Red;
                    lblStatus.Text = result.Message;
                }
            }
            catch (Exception ex)
            {
                lblStatus.ForeColor = System.Drawing.Color.Red;
                lblStatus.Text = $"Error: {ex.Message}";
            }
            finally
            {
                btnAuthenticate.Enabled = true;
            }
        }

        /// <summary>
        /// Check for saved license on form load
        /// </summary>
        private async void LicenseForm_Load(object sender, EventArgs e)
        {
            // Check if we have a saved license key
            string savedLicenseKey = Properties.Settings.Default.LicenseKey;

            if (!string.IsNullOrEmpty(savedLicenseKey))
            {
                txtLicenseKey.Text = savedLicenseKey;
                lblStatus.Text = "Checking saved license...";

                // Auto-validate the saved license
                var result = await _vaultonClient.ValidateLicenseAsync(savedLicenseKey);

                if (result.IsValid)
                {
                    lblStatus.ForeColor = System.Drawing.Color.Green;
                    lblStatus.Text = "✓ License validated!";
                    
                    // Optional: Auto-proceed to main form
                    // var mainForm = new MainForm();
                    // mainForm.Show();
                    // this.Hide();
                }
                else
                {
                    lblStatus.ForeColor = System.Drawing.Color.Orange;
                    lblStatus.Text = "Saved license is invalid. Please enter a new one.";
                }
            }
        }
    }
}
