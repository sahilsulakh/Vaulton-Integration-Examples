using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Management;

namespace VaultonSDK
{
    /// <summary>
    /// Vaulton Authentication Client for .NET applications
    /// </summary>
    public class VaultonClient
    {
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Creates a new Vaulton client instance
        /// </summary>
        /// <param name="apiKey">Your Vaulton API key from the dashboard</param>
        /// <param name="baseUrl">Vaulton API base URL (default: https://vaulton.vercel.app)</param>
        public VaultonClient(string apiKey, string baseUrl = "https://vaulton.vercel.app")
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _baseUrl = baseUrl.TrimEnd('/');
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
        }

        /// <summary>
        /// Validates a license key
        /// </summary>
        /// <param name="licenseKey">The license key to validate</param>
        /// <returns>License validation result</returns>
        public async Task<LicenseValidationResult> ValidateLicenseAsync(string licenseKey)
        {
            try
            {
                var hwid = GetHardwareId();
                
                // Safety check: ensure HWID is never null or empty
                if (string.IsNullOrWhiteSpace(hwid))
                {
                    hwid = "FALLBACK-" + Environment.MachineName.GetHashCode().ToString("X");
                }

                var request = new
                {
                    licenseKey = licenseKey,
                    hwid = hwid,
                    machineName = Environment.MachineName
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/validate/validate", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<ValidationApiResponse>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Check the 'valid' field, not just 'success'
                if (result?.Valid == true)
                {
                    return new LicenseValidationResult
                    {
                        IsValid = true,
                        Message = "License validated successfully",
                        LicenseData = result.Data
                    };
                }
                else
                {
                    // Get error message from either 'error' or 'message' field
                    string errorMsg = result?.Error ?? result?.Message ?? "License validation failed";
                    return new LicenseValidationResult
                    {
                        IsValid = false,
                        Message = errorMsg
                    };
                }
            }
            catch (Exception ex)
            {
                return new LicenseValidationResult
                {
                    IsValid = false,
                    Message = $"Connection error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Authenticates a client user with username and password
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <returns>Authentication result</returns>
        public async Task<AuthenticationResult> AuthenticateUserAsync(string username, string password)
        {
            try
            {
                var hwid = GetHardwareId();
                if (string.IsNullOrWhiteSpace(hwid))
                {
                    hwid = "FALLBACK-" + Environment.MachineName.GetHashCode().ToString("X");
                }

                var request = new
                {
                    username = username,
                    password = password,
                    hwid = hwid
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/client-users/login", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<ApiResponse>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result?.Success == true)
                {
                    return new AuthenticationResult
                    {
                        IsAuthenticated = true,
                        Message = result.Message ?? "Authentication successful",
                        UserData = result.Data
                    };
                }
                else
                {
                    return new AuthenticationResult
                    {
                        IsAuthenticated = false,
                        Message = result?.Message ?? "Authentication failed"
                    };
                }
            }
            catch (Exception ex)
            {
                return new AuthenticationResult
                {
                    IsAuthenticated = false,
                    Message = $"Connection error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Gets the hardware ID of the current machine
        /// </summary>
        /// <returns>Hardware ID string</returns>
        public static string GetHardwareId()
        {
            try
            {
                // Try WMI first
                string cpuId = GetCpuId();
                string motherboardId = GetMotherboardId();
                
                // If WMI returns empty, use MachineName/UserName as fallback for components
                if (string.IsNullOrEmpty(cpuId)) cpuId = Environment.ProcessorCount.ToString();
                if (string.IsNullOrEmpty(motherboardId)) motherboardId = Environment.MachineName;

                string combined = $"{cpuId}-{motherboardId}";

                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                    return BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 32);
                }
            }
            catch
            {
                // Absolute fallback
                try 
                {
                    return "FB-" + Math.Abs(Environment.MachineName.GetHashCode()).ToString();
                } 
                catch 
                {
                    return "UNKNOWN-DEVICE";
                }
            }
        }

        private static string GetCpuId()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return obj["ProcessorId"]?.ToString() ?? "";
                    }
                }
            }
            catch { }
            return Environment.ProcessorCount.ToString();
        }

        private static string GetMotherboardId()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return obj["SerialNumber"]?.ToString() ?? "";
                    }
                }
            }
            catch { }
            return Environment.MachineName;
        }
    }

    /// <summary>
    /// Result of license validation
    /// </summary>
    public class LicenseValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public object LicenseData { get; set; }
    }

    /// <summary>
    /// Result of user authentication
    /// </summary>
    public class AuthenticationResult
    {
        public bool IsAuthenticated { get; set; }
        public string Message { get; set; }
        public object UserData { get; set; }
    }

    /// <summary>
    /// API Response structure
    /// </summary>
    internal class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }

    /// <summary>
    /// Validation API Response structure (includes valid and error fields)
    /// </summary>
    internal class ValidationApiResponse
    {
        public bool Success { get; set; }
        public bool Valid { get; set; }
        public string Message { get; set; }
        public string Error { get; set; }
        public object Data { get; set; }
    }
}
