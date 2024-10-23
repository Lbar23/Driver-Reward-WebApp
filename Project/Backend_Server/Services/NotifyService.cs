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


    public NotifyService(IConfiguration configuration, IAmazonSecretsManager secretsManager, ILogger<NotifyService> logger)
    {
        _secretsManager = secretsManager;
        _configuration = configuration;

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
            SecretId = "team16/notifyapi/creds" // Replace with your actual secret ID
        });

        return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(secretValueResponse.SecretString)
            ?? new Dictionary<string, string>(); 
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
}
