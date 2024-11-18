using Twilio.Rest.Api.V2010.Account;
using SendGrid;
using SendGrid.Helpers.Mail;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Backend_Server.Models;
using Twilio;

namespace Backend_Server.Services;

public class NotifyService
{
    private readonly IAmazonSecretsManager _secretsManager;
    private readonly IConfiguration _configuration;
    private readonly SendGridClient _sendGridClient;
    private readonly ILogger<NotifyService> _logger;
    private string _sendGridApiKey;
    private string _twilioSID;
    private string _twilioAuthToken;
    private string _fromEmailAddress;
    private string _fromPhoneNumber;

    private readonly Dictionary<string, string> _templateIds = new()
    {
        { "2FA", "d-16815c0473d948acb2715a5001907e8c" },
        { "Purchase", "d-69ac862108b441d6b289875f5365c4d3" },
        { "PointsChange", "d-0c016d5246e447d5873163bb0f9138b8" },
        { "SystemDrop", "d-8c3f3751f36a40a4820b5d14cd056386" },
        { "AppStatus", "d-b6f2e28c32e748ddafeab97761e74bb9" },
        { "OrderIssue", "d-f966dff0b760434f871016a1a9761600" },
        { "PointsReport", "d-972ccaa0eb9b427ea96436f8fd1af7c7" }
    };

    public NotifyService(IConfiguration configuration, IAmazonSecretsManager secretsManager, ILogger<NotifyService> logger)
    {
        _secretsManager = secretsManager;
        _configuration = configuration;
        _logger = logger;

        var secrets = LoadSecrets(secretsManager).Result;

        _sendGridApiKey = secrets["SendGridApiKey"];
        _twilioSID = secrets["TwilioSID"];
        _twilioAuthToken = secrets["TwilioAuthToken"];
        _fromEmailAddress = secrets["FromEmailAddress"];
        _fromPhoneNumber = secrets["FromPhoneNumber"];

        _sendGridClient = new SendGridClient(_sendGridApiKey);
        TwilioClient.Init(_twilioSID, _twilioAuthToken);
    }

    private async Task<Dictionary<string, string>> LoadSecrets(IAmazonSecretsManager secretsManager)
    {
        var secretValueResponse = await secretsManager.GetSecretValueAsync(new GetSecretValueRequest
        {
            SecretId = "team16/notifyapi/creds"
        });

        return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(secretValueResponse.SecretString)
            ?? throw new Exception("Failed to load secrets.");
    }

    private async Task NotifyUser(string email, string phoneNumber, NotificationPref preference, string templateId, Dictionary<string, string> templateData, string fallbackMessage)
    {
        switch (preference)
        {
            case NotificationPref.Email when !string.IsNullOrEmpty(email):
                await SendTemplateEmail(email, templateId, templateData);
                break;
            case NotificationPref.Phone when !string.IsNullOrEmpty(phoneNumber):
                await SendSmsAsync(phoneNumber, fallbackMessage);
                break;
            default:
                _logger.LogWarning("Notification preference not supported or missing data for Email: {Email}, Phone: {Phone}", email, phoneNumber);
                throw new InvalidOperationException("Notification preference not supported or required data missing.");
        }
    }

    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            var msg = await MessageResource.CreateAsync(
                body: message,
                from: new Twilio.Types.PhoneNumber(_fromPhoneNumber),
                to: new Twilio.Types.PhoneNumber(phoneNumber)
            );

            _logger.LogInformation("SMS sent successfully to {PhoneNumber}: SID {Sid}", phoneNumber, msg.Sid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", phoneNumber);
            throw;
        }
    }

    public async Task SendTemplateEmail(string emailAddress, string templateId, Dictionary<string, string> templateData)
    {
        if (!_templateIds.TryGetValue(templateId, out var sendGridTemplateId))
        {
            _logger.LogError("Template ID not found for key: {TemplateId}", templateId);
            throw new ArgumentException($"Template ID not found for key: {templateId}");
        }

        try
        {
            var msg = new SendGridMessage
            {
                From = new EmailAddress(_fromEmailAddress, "GitGudDriversApp"),
                TemplateId = sendGridTemplateId
            };

            msg.AddTo(new EmailAddress(emailAddress));
            msg.SetTemplateData(templateData);

            var response = await _sendGridClient.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent successfully to {EmailAddress}", emailAddress);
            }
            else
            {
                _logger.LogError("Failed to send email to {EmailAddress}: {StatusCode}", emailAddress, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {EmailAddress}", emailAddress);
            throw;
        }
    }

    // Refactored Notification Methods
    public async Task NotifyPurchaseAsync(string email, string phone, NotificationPref pref, string productName, int cartPrice, string username)
    {
        var templateData = new Dictionary<string, string>
        {
            { "product_name", productName },
            { "cart_price", cartPrice.ToString() },
            { "user_name", username }
        };

        var fallbackMessage = $"You purchased {productName} for {cartPrice} points.";
        await NotifyUser(email, phone, pref, "Purchase", templateData, fallbackMessage);
    }

    public async Task NotifyPointsChangeAsync(string email, string phone, NotificationPref pref, int newBalance, string pointsStatus, string username)
    {
        var statusMessage = pointsStatus switch
        {
            "gain" => "Congratulations, you've earned points! Keep up the good work!",
            "loss" => "Unfortunately, you've lost points. Contact your sponsor if there’s a mistake.",
            _ => "Your points balance has changed."
        };

        var templateData = new Dictionary<string, string>
        {
            { "new_balance", newBalance.ToString() },
            { "user_name", username },
            { "status_msg", statusMessage }
        };

        var fallbackMessage = $"Your points have been updated. New balance: {newBalance}.";
        await NotifyUser(email, phone, pref, "PointsChange", templateData, fallbackMessage);
    }

    public async Task NotifyAppStatusAsync(string email, string phone, NotificationPref pref, string status, string username)
    {
        var statusMessage = status switch
        {
            "approved" => "Your application has been accepted! Start earning points now!",
            "rejected" => "Your application was not accepted. Contact support for further details.",
            "submitted" => "Your application has been submitted and is under review.",
            _ => "The status of your application has changed."
        };

        var templateData = new Dictionary<string, string>
        {
            { "status", status },
            { "status_message", statusMessage },
            { "user_name", username }
        };

        var fallbackMessage = $"Your application status is now: {status}.";
        await NotifyUser(email, phone, pref, "AppStatus", templateData, fallbackMessage);
    }
}
