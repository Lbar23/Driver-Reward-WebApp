[ApiController]
[Route("api/[controller]")]
public class AuditLogController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<ActionResult<AuditLogResult>> GetAuditLogs([FromQuery] AuditLogFilter filter)
    {
        var result = await _auditLogService.GetAuditLogsAsync(filter);
        return Ok(result);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<AuditLog>>> GetUserLogs(int userId)
    {
        var logs = await _auditLogService.GetUserAuditLogsAsync(userId);
        return Ok(logs);
    }
}