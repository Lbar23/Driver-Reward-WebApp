import React, { useState } from 'react';
import Terminal, { ColorMode, TerminalOutput } from 'react-terminal-ui';
import { Paper, Box, Typography } from '@mui/material';
import TerminalIcon from '@mui/icons-material/Terminal';
import { useAuth } from '../../service/authContext';

const AdminConsole = () => {
  const { user } = useAuth();
  const [terminalLineData, setTerminalLineData] = useState<React.ReactNode[]>([
    <TerminalOutput key="welcome">
      Welcome {user?.userName || 'Admin'} to the GitGud Drivers Admin Console
      {'\n'}
      Type 'help' for available commands
    </TerminalOutput>
  ]);

  const handleInput = (input: string) => {
    const [cmd, ...args] = input.split(' ');
    let outputText = '';
    
    switch (cmd) {
      case 'help':
        outputText = 'Available commands:\n' +
          '  help - Show this help message\n' +
          '  status - Show system status\n' +
          '  users - List recent user activities\n' +
          '  clear - Clear console\n' +
          '  audit [date] - Show audit logs for date (YYYY-MM-DD)\n' +
          '  metrics - Show system metrics\n' +
          '  version - Show system version';
        break;
      
      case 'status':
        outputText = 'System Status: Operational\n' +
          'Last backup: 2024-03-10\n' +
          'Active users: 42\n' +
          'Server load: Normal';
        break;

      case 'users':
        outputText = 'Recent User Activities:\n' +
          'admin1@gitgud.com - Login - 2 mins ago\n' +
          'driver2@email.com - Updated profile - 5 mins ago\n' +
          'sponsor1@company.com - Approved application - 10 mins ago';
        break;

      case 'metrics':
        outputText = 'System Metrics:\n' +
          'CPU Usage: 45%\n' +
          'Memory: 2.1GB/4GB\n' +
          'Active Sessions: 12\n' +
          'Response Time: 230ms';
        break;

      case 'audit':
        const date = args[0];
        if (!date || !/^\d{4}-\d{2}-\d{2}$/.test(date)) {
          outputText = 'Error: Please provide a date in YYYY-MM-DD format';
        } else {
          outputText = `Fetching audit logs for ${date}...\n` +
            `[${date} 09:15:23] User login - admin1@gitgud.com\n` +
            `[${date} 09:20:45] Configuration change - System settings updated\n` +
            `[${date} 10:30:12] New user registration - driver3@email.com`;
        }
        break;

      case 'version':
        outputText = 'GitGud Drivers v1.0.0';
        break;

      case 'clear':
        setTerminalLineData([]);
        return;

      default:
        outputText = `Unknown command: ${cmd}. Type 'help' for available commands.`;
    }

    setTerminalLineData(prev => [
      ...prev,
      <TerminalOutput key={`input-${Date.now()}`}>
        {`$ ${input}`}
      </TerminalOutput>,
      <TerminalOutput key={`output-${Date.now()}`}>
        {outputText}
      </TerminalOutput>
    ]);
  };

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
        <div style={{ height: 'calc(100% - 48px)', background: '#1a1a1a' }}>
          <Terminal
            name="Admin Console"
            colorMode={ColorMode.Dark}
            prompt="$"
            onInput={handleInput}
          >
            {terminalLineData}
          </Terminal>
        </div>
      </Paper>
    </Box>
  );
};

export default AdminConsole;