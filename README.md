# Vaulton .NET Integration

Complete integration package for using Vaulton license authentication in your .NET Windows Forms applications.

> **Note:** Default base URL is `http://localhost:3001` for local development.
> Change this when deploying to production.

## Quick Start

### 1. Add the SDK files to your project

Copy these files to your project:
- `VaultonClient.cs` - Core SDK client
- `VaultonHelper.cs` - Static helper for quick integration 
- `LicenseFormIntegration.cs` - Example form integration

### 2. Add Required References

Ensure your project references:
- `System.Management` (for hardware ID generation)
- `System.Net.Http` (for API calls)
- `System.Text.Json` (for JSON serialization)

### 3. Get Your API Key

1. Login to your Vaulton dashboard
2. Go to Applications
3. Copy your application's API Key

## Integration Options

### Option A: Quick One-Liner (Recommended for most apps)

Add this to your `Program.cs` or at app startup:

```csharp
using VaultonIntegration;

static async Task Main()
{
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    
    // Require valid license - shows dialog and exits if invalid
    await VaultonHelper.RequireLicenseAsync("YOUR_API_KEY_HERE");
    
    // If we get here, license is valid
    Application.Run(new MainForm());
}
```

### Option B: Custom License Form

If you have an existing form with `txtLicenseKey`, `btnAuthenticate`, and `lblStatus`:

```csharp
using VaultonSDK;

public partial class MyLicenseForm : Form
{
    private readonly VaultonClient _vaulton;
    
    public MyLicenseForm()
    {
        InitializeComponent();
        _vaulton = new VaultonClient("YOUR_API_KEY_HERE");
    }
    
    private async void btnAuthenticate_Click(object sender, EventArgs e)
    {
        lblStatus.Text = "Validating...";
        btnAuthenticate.Enabled = false;
        
        var result = await _vaulton.ValidateLicenseAsync(txtLicenseKey.Text);
        
        if (result.IsValid)
        {
            lblStatus.ForeColor = Color.Green;
            lblStatus.Text = "✓ " + result.Message;
            // Proceed to main app
        }
        else
        {
            lblStatus.ForeColor = Color.Red;
            lblStatus.Text = result.Message;
        }
        
        btnAuthenticate.Enabled = true;
    }
}
```

### Option C: User Authentication (Username/Password)

```csharp
using VaultonSDK;

private async void btnLogin_Click(object sender, EventArgs e)
{
    var vaulton = new VaultonClient("YOUR_API_KEY_HERE");
    
    var result = await vaulton.AuthenticateUserAsync(
        txtUsername.Text,
        txtPassword.Text
    );
    
    if (result.IsAuthenticated)
    {
        // User logged in successfully
        MessageBox.Show("Welcome!");
    }
    else
    {
        MessageBox.Show(result.Message);
    }
}
```

## Hardware ID (HWID) Lock

The SDK automatically captures and sends the user's hardware ID for HWID locking. This prevents license sharing between devices.

To get the hardware ID manually:
```csharp
string hwid = VaultonClient.GetHardwareId();
```

## Response Messages

The `Message` property in results contains status messages configured in your Vaulton dashboard:
- Invalid license key
- License expired
- License revoked/suspended
- Hardware ID mismatch
- Max activations reached
- etc.

## Saving License Keys

To persist the license key between sessions:

```csharp
// Save
Properties.Settings.Default.LicenseKey = licenseKey;
Properties.Settings.Default.Save();

// Load
string savedKey = Properties.Settings.Default.LicenseKey;
```

First, add a setting in Project Properties > Settings:
- Name: `LicenseKey`
- Type: `string`
- Scope: `User`

## Error Handling

Always wrap API calls in try-catch:

```csharp
try
{
    var result = await _vaulton.ValidateLicenseAsync(key);
    // Handle result
}
catch (Exception ex)
{
    MessageBox.Show($"Network error: {ex.Message}");
}
```

## API Reference

### VaultonClient

| Method | Description |
|--------|-------------|
| `ValidateLicenseAsync(string licenseKey)` | Validates a license key |
| `AuthenticateUserAsync(string username, string password)` | Authenticates with username/password |
| `GetHardwareId()` | Gets the current machine's hardware ID |

### VaultonHelper

| Method | Description |
|--------|-------------|
| `Initialize(string apiKey)` | Initialize the helper |
| `ValidateLicenseAsync(string licenseKey)` | Validate and track license status |
| `ShowLicenseDialogAsync(string apiKey)` | Show built-in license dialog |
| `RequireLicenseAsync(string apiKey)` | Require license or exit app |
| `IsLicensed` | Check if currently licensed |

## Support

For issues or questions:
- Dashboard: https://vaulton.app
- Documentation: https://docs.vaulton.app
