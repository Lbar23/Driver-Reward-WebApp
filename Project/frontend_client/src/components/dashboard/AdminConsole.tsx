import React from 'react';
import ReactTerminal from 'react-terminal-ui';
import { Paper, Box, Typography } from '@mui/material';
import TerminalIcon from '@mui/icons-material/Terminal';
import { useAuth } from '../../service/authContext';

const AdminConsole = () => {
  const { user } = useAuth();

  const handleInput = (input: string) => {
    const [cmd, ...args] = input.split(' ');
    
    switch (cmd) {
      case 'help':
        return `Available commands:
  help - Show this help message
  status - Show system status
  users - List recent user activities
  clear - Clear console
  audit [date] - Show audit logs for date (YYYY-MM-DD)
  metrics - Show system metrics
  version - Show system version`;
      
      case 'status':
        return `System Status: Operational
Last backup: 2024-03-10
Active users: 42
Server load: Normal`;

      case 'users':
        return `Recent User Activities:
admin1@gitgud.com - Login - 2 mins ago
driver2@email.com - Updated profile - 5 mins ago
sponsor1@company.com - Approved application - 10 mins ago`;

      case 'metrics':
        return `System Metrics:
CPU Usage: 45%
Memory: 2.1GB/4GB
Active Sessions: 12
Response Time: 230ms`;

      case 'audit':
        const date = args[0];
        if (!date || !/^\d{4}-\d{2}-\d{2}$/.test(date)) {
          return 'Error: Please provide a date in YYYY-MM-DD format';
        }
        return `Fetching audit logs for ${date}...
[${date} 09:15:23] User login - admin1@gitgud.com
[${date} 09:20:45] Configuration change - System settings updated
[${date} 10:30:12] New user registration - driver3@email.com`;

      case 'version':
        return 'GitGud Drivers v1.0.0';

      case 'clear':
        return 'clear';

      default:
        return `Unknown command: ${cmd}. Type 'help' for available commands.`;
    }
  };

  // Initial terminal lines
  const terminalLineData = [{
    type: 'text',
    value: `Welcome ${user?.userName || 'Admin'} to the GitGud Drivers Admin Console\nType 'help' for available commands`
  }];

  return (
    <Box sx={{ p: 2 }}>
      <Paper 
        elevation={3}
        sx={{
          bgcolor: '#1a1a1a',
          borderRadius: 1,
          overflow: 'hidden',
          height: '500px'
        }}
      >
        <Box 
          sx={{ 
            display: 'flex', 
            alignItems: 'center', 
            gap: 1,
            bgcolor: '#2d2d2d',
            p: 1,
            borderBottom: '1px solid #333'
          }}
        >
          <TerminalIcon sx={{ color: '#fff' }} />
          <Typography variant="h6" sx={{ color: '#fff' }}>
            Admin Console
          </Typography>
        </Box>
        <ReactTerminal
          name="Admin Console"
          prompt="$"
          lineData={terminalLineData}
          onInput={handleInput}
        />
      </Paper>
    </Box>
  );
};

export default AdminConsole;