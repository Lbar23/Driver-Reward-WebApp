using Twilio.Rest.Api.V2010.Account;
using SendGrid;
using SendGrid.Helpers.Mail;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
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

    // Centralized template mapping
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

        // Init Twilio
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
            ?? []; 
    }

    // Send general SMS using Twilio
    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        try{
            var msg = await MessageResource.CreateAsync(
                body: message,
                from: new Twilio.Types.PhoneNumber(_fromPhoneNumber),
                to: new Twilio.Types.PhoneNumber(phoneNumber)
            );

        _logger.LogInformation($"SMS sent successfully to {phoneNumber}: SID {msg.Sid}");
        }
        catch (Exception ex){
            _logger.LogError($"Failed to send SMS to {phoneNumber}: {ex.Message}");
            throw;
        }
    }

    // Send general Email using SendGrid
    public async Task SendEmailAsync(string emailAddress, string subject, string message)
    {

        try
        {
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(_fromEmailAddress, "GitGudDriversApp"),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = $"<p>{message}</p>"
            };
            msg.AddTo(new EmailAddress(emailAddress));
            
            var response = await _sendGridClient.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode){
                _logger.LogInformation($"Email sent successfully to {emailAddress}");
            }
            else{
                _logger.LogError($"Failed to send email to {emailAddress}: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to send email to {emailAddress}: {ex.Message}");
            throw;
        }
    }

    // send email with a template id
    public async Task SendTemplateEmail(string emailAddress, string tempID, Dictionary<string, string> tempData)
    {
        if (!_templateIds.TryGetValue(tempID, out var templateId))
        {
            _logger.LogError($"Template ID not found for key: {tempID}");
            throw new ArgumentException($"Template ID not found for key: {tempID}");
        }

        try
        {
            var msg = new SendGridMessage();
            msg.SetFrom(new EmailAddress(_fromEmailAddress, "GitGudDriversApp"));
            msg.AddTo(new EmailAddress(emailAddress));
            msg.SetTemplateId(tempID);
            msg.SetTemplateData(tempData);
            var response = await _sendGridClient.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode){
                _logger.LogInformation($"Email sent successfully to {emailAddress}");
            }
            else{
                _logger.LogError($"Failed to send email to {emailAddress}: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to send email to {emailAddress}: {ex.Message}");
            throw;
        }
    }

    // for specific type examples
    public async Task NotifyPurchaseAsync(string emailAddress, string productName, int cartPrice, string username)
    {
        var templateData = new Dictionary<string, string>
        {
            { "product_name", productName },
            { "cart_price", cartPrice.ToString() },
            { "user_name", username}
        };

        await SendTemplateEmail(emailAddress, "Purchase", templateData);
    }

    public async Task NotifyPointsChangeAsync(string emailAddress, int newBalance, string pointsStatus, string username)
    {
        var statusMessage = pointsStatus switch
        {
            "gain" => "Congratulations, you've earned points! Kepp up the good work!",
            "loss" => "Unfortunately, you've lost points. Please reach out to your Sponsor if you feel there has been a mistake!",
            _ => "Your points balance has changed."
        };
        var templateData = new Dictionary<string, string>
        {

            { "new_balance", newBalance.ToString() },
            { "user_name", username}
            
        };

        await SendTemplateEmail(emailAddress, "PointsChange", templateData);
    }

    public async Task NotifySystemDropAsync(string emailAddress, string username)
    {
        var templateData = new Dictionary<string, string>
        {
            { "user_name", username}
        };

        await SendTemplateEmail(emailAddress, "SystemDrop", templateData);
    }

    public async Task NotifyAppStatusAsync(string emailAddress, string status, string username)
    {
        var statusMessage = status switch
        {
            "approved" => "Congratulations! Your application has been accepted. You can now start earning points!",
            "rejected" => "Unfortunately, your application was not accepted. Please contact support for further information.",
            "submitted" => "Your application has been submitted an is under review. You will be notified once a decision is made.",
            _ => "The status of your application has changed."
        };

        var templateData = new Dictionary<string, string>
        {
            { "status", status },
            { "status_message", statusMessage },
            { "user_name", username}
        };

        await SendTemplateEmail(emailAddress, "AppStatus", templateData);
    }

    public async Task NotifyOrderIssueAsync(string emailAddress, string orderId, string username)
    {
        var templateData = new Dictionary<string, string>
        {
            { "order_id", orderId },
            { "message", "An issue has occurred with your order. Please contact support for assistance." },
            { "user_name", username}
        };

        await SendTemplateEmail(emailAddress, "OrderIssue", templateData);
    }

    public async Task NotifyPointReportAsync(string emailAddress, int earnedPoints, int lostPoints, int spentPoints, string username)
    {
        
        var templateData = new Dictionary<string, string>
        {
            { "points_earned", earnedPoints.ToString() },
            { "points_lost", lostPoints.ToString() },
            { "points_spent", spentPoints.ToString() },
            { "user_name", username}
        };

        await SendTemplateEmail(emailAddress, "PointsReport", templateData);
    }




}
