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
    private string _sendGridApiKey;
    private string _twilioSID;
    private string _twilioAuthToken;
    private string _fromEmailAddress;
    private string _fromPhoneNumber;


    public NotifyService(IConfiguration configuration, IAmazonSecretsManager secretsManager)
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

        return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(secretValueResponse.SecretString);
    }

    // Send SMS using Twilio
    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        var msg = await MessageResource.CreateAsync(
            body: message,
            from: new Twilio.Types.PhoneNumber(_fromPhoneNumber),
            to: new Twilio.Types.PhoneNumber(phoneNumber)
        );
    }

    // Send Email using SendGrid
    public async Task SendEmailAsync(string emailAddress, string subject, string message)
    {
        var msg = new SendGridMessage()
        {
            From = new EmailAddress(_fromEmailAddress, "GitGudDriversApp"),
            Subject = subject,
            PlainTextContent = message,
            HtmlContent = $"<p>{message}</p>"
        };
        msg.AddTo(new EmailAddress(emailAddress));
        await _sendGridClient.SendEmailAsync(msg);
    }
}
