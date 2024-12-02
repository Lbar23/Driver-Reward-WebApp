using Twilio.Rest.Api.V2010.Account;
using SendGrid;
using SendGrid.Helpers.Mail;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Backend_Server.Models;
using Twilio;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Serilog;

namespace Backend_Server.Services;

public class NotifyService
{
    private readonly IAmazonSecretsManager _secretsManager;
    private readonly AppDBContext _context;
    private readonly SendGridClient _sendGridClient;
    private readonly UserManager<Users> _userManager;
    private readonly string _sendGridApiKey;
    private readonly string _twilioSID;
    private readonly string _twilioAuthToken;
    private readonly string _fromEmailAddress;
    private readonly string _fromPhoneNumber;

    public NotifyService(IAmazonSecretsManager secretsManager, 
                         AppDBContext context,
                         UserManager<Users> userManager)
    {
        _secretsManager = secretsManager;
        _context = context;
        _userManager = userManager;

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

        return JsonSerializer.Deserialize<Dictionary<string, string>>(secretValueResponse.SecretString)
            ?? throw new Exception("Failed to load secrets.");
    }

    private async Task NotifyUser(int userId, string templateName, Dictionary<string, string> templateData, string fallbackMessage)
    {
        
        var user = _userManager.FindByIdAsync(userId.ToString()).Result;
        if (user == null){
            Log.Error(userId.ToString(), $"No user found matching {userId}, notify attempt failed.");
            await LogNotifyAttempt(templateName, userId, false);
            throw new Exception("No user found for the Id, notify attempt failed");
        }
        switch (user.NotifyPref){
            case NotificationPref.Email when !string.IsNullOrEmpty(user.Email):
                await SendTemplateEmail(user, templateName, templateData);
                break;
            case NotificationPref.Phone when !string.IsNullOrEmpty(user.PhoneNumber):
                await SendSmsAsync(user, fallbackMessage);
                break;
            default:
                Log.Warning("Notification preference not supported or missing data for Email: {Email}, Phone: {Phone}", user.Email, user.PhoneNumber);
                await LogNotifyAttempt(templateName, userId, false);
                throw new InvalidOperationException("Notification preference not supported or required data missing.");
        }
    }

    public async Task SendSmsAsync(Users user, string message)
    {
        try
        {
            var msg = await MessageResource.CreateAsync(
                body: message,
                from: new Twilio.Types.PhoneNumber(_fromPhoneNumber),
                to: new Twilio.Types.PhoneNumber(user.PhoneNumber)
            );

            Log.Information($"SMS sent successfully to {user.PhoneNumber}: SID {msg.Sid}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to send SMS to {user.PhoneNumber}");
            

            throw;
        }
    }

    public async Task SendTemplateEmail(Users user, string templateName, Dictionary<string, string> templateData)
    {
        // Parse the template name string to enum
        if (!Enum.TryParse<NotifyCategory>(templateName, out var category))
        {
            throw new ArgumentException($"Invalid notification type: {templateName}");
        }

        // Retrieve the template configuration using the enum
        var notificationType = await _context.NotifyTypes
            .FirstOrDefaultAsync(nt => nt.Category == category) ?? 
                throw new ArgumentException($"Notification type '{templateName}' not found.");


        // Validate template data against expected fields
        var expectedFields = !string.IsNullOrEmpty(notificationType.TemplateFieldsJson) 
            ? JsonSerializer.Deserialize<List<string>>(notificationType.TemplateFieldsJson) 
            : [];
        
        var missingFields = expectedFields?.Except(templateData.Keys).ToList() ?? new List<string>();

        if (missingFields.Count > 0)
        {
            await LogNotifyAttempt(templateName, user.Id, false);
            throw new ArgumentException($"Missing required fields: {string.Join(", ", missingFields)}");
        }

        try
        {
            var msg = new SendGridMessage
            {
                From = new EmailAddress(_fromEmailAddress, "GitGudDriversApp"),
                TemplateId = notificationType.EmailTemplateID
            };

            msg.AddTo(new EmailAddress(user.Email));
            msg.SetTemplateData(templateData); // SendGrid handles variable replacement

            var response = await _sendGridClient.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                Log.Information($"Email sent successfully to {user.Email}");

            }
            else
            {
                Log.Error($"Failed to send email to {user.Email}: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to send email to {user.Email}: {ex}");
            throw;
        }
    }

    public async Task LogNotifyAttempt(string? templateName, int userId, bool success)
    {
        var notificationType = await _context.NotifyTypes
            .FirstOrDefaultAsync(nt => nt.Category.ToString() == templateName);
        
        var notifyattempt = new NotificationHistory
        {
            UserID = userId,
            Success = success,
            NotifyDate = DateTime.UtcNow,
            NotifyTypeID = notificationType?.TypeID ?? 0  // Use the TypeID instead of the whole object
        };
        
        _context.NotificationHistory.Add(notifyattempt);
        await _context.SaveChangesAsync();
    }


    // Refactored Notification Methods

    public async Task NotifyAuthAsync(int userId, string code, string username)
    {
        // Populate template data
        var templateData = new Dictionary<string, string>
        {
            { "auth_code", code },
            { "user_name", username }
        };

        var fallbackMessage = $"Your requested code is {code}";
        await NotifyUser(userId, "Auth", templateData, fallbackMessage);
    }

    public async Task NotifyPurchaseAsync(int userId, Dictionary<string, string> cartItems, int cartPrice, string username)
    {
        // Create a formatted string of cart items with price details
        // expected dictionary is <Product Name, Points Cost>
        var cartItemsSummary = string.Join(", ", cartItems.Select(item => $"{item.Key} ({item.Value} Points)"));

        // Populate template data
        var templateData = new Dictionary<string, string>
        {
            { "cart_items", cartItemsSummary },
            { "cart_price", cartPrice.ToString() },
            { "user_name", username }
        };

        var fallbackMessage = $"You purchased {cartItems.Count} items for {cartPrice} points.";
        await NotifyUser(userId, "Purchase", templateData, fallbackMessage);
    }

    public async Task NotifyPointsChangeAsync(int userId, int newBalance, string pointsStatus, string username)
    {
        var statusMessage = pointsStatus switch
        {
            "gain" => "Congratulations, you've earned points! Keep up the good work!",
            "loss" => "Unfortunately, you've lost points. Contact your sponsor if there's a mistake.",
            _ => "Your points balance has changed."
        };

        var templateData = new Dictionary<string, string>
        {
            { "new_balance", newBalance.ToString() },
            { "user_name", username },
            { "status_msg", statusMessage }
        };

        var fallbackMessage = $"Your points have been updated. New balance: {newBalance}.";
        await NotifyUser(userId, "PointsChange", templateData, fallbackMessage);
    }

    public async Task NotifyAppStatusAsync(int userId, string status, string username)
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
        await NotifyUser(userId, "AppStatus", templateData, fallbackMessage);
    }

    public async Task NotifySystemChangeAsync(int userId, string type, string username, string? reason = null, string? sponsorName = null)
    {
        // Determine change type to create message
        var baseMsg = type switch
        {
            "SponsorDrop" => $"You have been removed from the incentive program by {sponsorName ?? "a sponsor"}.",
            "SystemDrop" => "Your account has been removed from our system.",
            "RoleChange" => "Your account role has been updated.",
            _ => "The status of your account has been updated."
        };

        // Append reason if provided
        var msg = string.IsNullOrEmpty(reason) ? baseMsg : $"{baseMsg} Reason: {reason}";


        var templateData = new Dictionary<string, string>
        {
            { "message", msg },
            { "user_name", username }        
        };

        var fallbackMessage = $"{msg}";
        await NotifyUser(userId, "SystemChange", templateData, fallbackMessage);
    }

    public async Task NotifyOrderIssueAsync(int userId, string orderId, string issueDescription, string username)
    {
        var templateData = new Dictionary<string, string>
        {
            { "order_id", orderId },
            { "issue_details", issueDescription },
            { "user_name", username }
        };

        var fallbackMessage = $"We're sorry, an issue occured with your order {orderId}: {issueDescription}";
        await NotifyUser(userId, "OrderIssue", templateData, fallbackMessage);
    }

    public async Task NotifyPointsReportAsync(int userId, int currentPoints, int pointsEarned, int pointsSpent, string username, string? reportPeriod = null)
    {
        // Define the report period or default to "week"
        reportPeriod ??= "week";

        // Create the main message
        var msg = $"Hello {username}, here is your points summary for this {reportPeriod}:\n" +
                $"- Current Balance: {currentPoints} points\n" +
                $"- Points Earned: {pointsEarned} points\n" +
                $"- Points Spent: {pointsSpent} points";

        // Prepare the template data for a detailed notification
        var templateData = new Dictionary<string, string>
        {
            { "user_name", username },
            { "report_period", reportPeriod },
            { "summary_msg", msg}
        };

        var fallbackMessage = msg;

        // Notify the user
        await NotifyUser(userId, "PointsReport", templateData, fallbackMessage);
    }



}
