import React, { useState, useEffect, useCallback } from 'react';
import Terminal, { ColorMode, TerminalOutput } from 'react-terminal-ui';
import { Paper, Box, Typography, CircularProgress } from '@mui/material';
import TerminalIcon from '@mui/icons-material/Terminal';
import { useAuth } from '../../service/authContext';
import { HubConnectionBuilder, HubConnection, LogLevel, HttpTransportType } from '@microsoft/signalr';

const AdminConsole = () => {
  const { user } = useAuth();
  const [connection, setConnection] = useState<HubConnection | null>(null);
  const [isExecuting, setIsExecuting] = useState(false);
  const [terminalLineData, setTerminalLineData] = useState<React.ReactNode[]>([]);
  const [commandHistory, setCommandHistory] = useState<string[]>([]);
  const [historyIndex, setHistoryIndex] = useState(-1);
  const [connectionAttempts, setConnectionAttempts] = useState(0);
  const MAX_RETRIES = 3;

  useEffect(() => {
    // Initial welcome message with ASCII art
    const welcomeMessage = [
      <TerminalOutput key="logo">
        <pre style={{ color: '#00ff00' }}>
        {`
  ________  .___  ___________   ________   ____ ___  ________    
 /  _____/  |   | \\__    ___/  /  _____/  |    |   \\ \\______ \\   
/   \\  ___  |   |   |    |    /   \\  ___  |    |   /  |    |  \\  
\\    \\_\\  \\ |   |   |    |    \\    \\_\\  \\ |    |  /   |    \`   \\ 
 \\______  / |___|   |____|     \\______  / |______/   /_______  / 
        \\/                            \\/                     \\/  
`}
        </pre>
      </TerminalOutput>,
      <TerminalOutput key="welcome">
        {`Welcome ${user?.userName || 'Admin'} to the GitGud Drivers Admin Console\n`}
        Type 'help' for available commands
      </TerminalOutput>,
      <TerminalOutput key="status">
        <span style={{ color: '#888' }}>Initializing terminal connection...</span>
      </TerminalOutput>
    ];
    setTerminalLineData(welcomeMessage);
  }, [user]);

  const addLine = useCallback((type: string, content: string, color?: string) => {
    const timestamp = new Date().toLocaleTimeString();
    let formattedContent: React.ReactNode;

    if (color) {
      formattedContent = <span style={{ color }}>{content}</span>;
    } else {
      formattedContent = content;
    }

    setTerminalLineData(prev => [
      ...prev,
      <TerminalOutput key={`${type}-${Date.now()}`}>
        <span style={{ color: '#666' }}>[{timestamp}]</span> {formattedContent}
      </TerminalOutput>
    ]);
  }, []);

  useEffect(() => {
    const startConnection = async () => {
      if (connectionAttempts >= MAX_RETRIES) {
        addLine('error', 'Maximum connection attempts reached. Please refresh the page.', 'red');
        return;
      }

      const newConnection = new HubConnectionBuilder()
        .withUrl('http://localhost:5041/terminalHub', { //Change this to also handle production server...
          skipNegotiation: false,
          transport: HttpTransportType.WebSockets | HttpTransportType.LongPolling,
          withCredentials: true
        })
        .configureLogging(LogLevel.Debug)
        .withAutomaticReconnect([0, 2000, 5000, 10000, 20000])
        .build();

      try {
        // Set up handlers before starting connection
        newConnection.onreconnecting((error) => {
          console.log('Reconnecting...', error);
          addLine('system', 'Connection lost. Attempting to reconnect...', 'yellow');
        });

        newConnection.onreconnected((connectionId) => {
          console.log('Reconnected!', connectionId);
          addLine('system', 'Connection restored!', 'green');
        });

        newConnection.onclose((error) => {
          console.log('Connection closed.', error);
          addLine('system', 'Connection closed. Please refresh the page.', 'red');
        });

        // Set up message handlers
        newConnection.on('ReceiveOutput', (message: string) => {
          setIsExecuting(false);
          addLine('output', message);
        });

        newConnection.on('ReceiveError', (error: string) => {
          setIsExecuting(false);
          addLine('error', error, 'red');
        });

        newConnection.on('SystemAlert', (message: string) => {
          addLine('alert', `🚨 ALERT: ${message}`, 'orange');
        });

        newConnection.on('SystemUpdate', (message: string) => {
          addLine('update', `📢 UPDATE: ${message}`, 'cyan');
        });

        // Start the connection
        await newConnection.start();
        console.log('SignalR Connected successfully.');
        addLine('system', 'Terminal connected successfully!', 'green');
        setConnection(newConnection);
        setConnectionAttempts(0); // Reset attempts on successful connection
      } catch (err) {
        console.error('SignalR Connection Error: ', err);
        const errorMessage = err instanceof Error ? err.message : 'Unknown error';
        addLine('error', `Connection error: ${errorMessage}`, 'red');
        
        const nextAttempt = connectionAttempts + 1;
        if (nextAttempt >= MAX_RETRIES) {
          addLine('error', 'Maximum connection attempts reached. Please refresh the page.', 'red');
          return;
        }
        
        setConnectionAttempts(nextAttempt);
        setTimeout(startConnection, 3000);
      }
    };

    if (connectionAttempts < MAX_RETRIES) {
      startConnection();
    }

    return () => {
      if (connection) {
        connection.stop()
          .catch(err => console.error('Error stopping connection:', err));
      }
    };
  }, [connectionAttempts, addLine]);

  const handleInput = async (input: string) => {
    if (!connection) {
      addLine('error', 'Terminal is not connected', 'red');
      return;
    }

    const trimmedInput = input.trim();
    if (!trimmedInput) return;

    // Add to command history
    setCommandHistory(prev => [...prev, trimmedInput]);
    setHistoryIndex(-1);

    // Show the command
    addLine('input', `$ ${trimmedInput}`, '#00ff00');

    // Handle clear locally
    if (trimmedInput.toLowerCase() === 'clear') {
      setTerminalLineData([]);
      return;
    }

    try {
      setIsExecuting(true);
      await connection.invoke('ExecuteCommand', trimmedInput);
    } catch (error) {
      console.error('Error sending command:', error);
      setIsExecuting(false);
      addLine('error', 'Failed to execute command. Please try again.', 'red');
    }
  };

  const handleKeyDown = useCallback((event: KeyboardEvent) => {
    if (event.key === 'ArrowUp') {
      event.preventDefault();
      setHistoryIndex(prev => {
        const newIndex = prev + 1;
        if (newIndex < commandHistory.length) {
          const command = commandHistory[commandHistory.length - 1 - newIndex];
          // You'll need to implement a way to set the terminal input value
          return newIndex;
        }
        return prev;
      });
    } else if (event.key === 'ArrowDown') {
      event.preventDefault();
      setHistoryIndex(prev => {
        const newIndex = prev - 1;
        if (newIndex >= -1) {
          const command = newIndex === -1 
            ? '' 
            : commandHistory[commandHistory.length - 1 - newIndex];
          // You'll need to implement a way to set the terminal input value
          return newIndex;
        }
        return prev;
      });
    }
  }, [commandHistory]);

  // Add keyboard event listeners
  useEffect(() => {
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [handleKeyDown]);

  console.log(localStorage.getItem('token'))

  return (
    <Box sx={{ p: 2 }}>
      <Paper 
        elevation={3}
        sx={{
          bgcolor: '#1a1a1a',
          borderRadius: 1,
          overflow: 'hidden',
          height: '500px',
          position: 'relative'
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
          <Typography variant="h6" sx={{ color: '#fff', flex: 1 }}>
            Admin Console 
          </Typography>
          {isExecuting && (
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <CircularProgress size={20} sx={{ color: '#00ff00' }} />
              <Typography variant="caption" sx={{ color: '#00ff00' }}>
                Executing...
              </Typography>
            </Box>
          )}
          {!connection?.state && (
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <Typography variant="caption" sx={{ color: '#ff4444' }}>
                Disconnected {connectionAttempts > 0 ? `(Attempt ${connectionAttempts}/${MAX_RETRIES})` : ''}
              </Typography>
              {connectionAttempts > 0 && <CircularProgress size={16} sx={{ color: '#ff4444' }} />}
            </Box>
          )}
        </Box>
        <div 
  style={{ 
    height: 'calc(100% - 48px)', 
    background: '#1a1a1a',
    overflowY: 'auto',  // Add scroll
    display: 'flex',
    flexDirection: 'column-reverse'  // Keep input at bottom
  }}
  ref={(el) => {
    if (el) {
      el.scrollTop = el.scrollHeight;
    }
  }}
>
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