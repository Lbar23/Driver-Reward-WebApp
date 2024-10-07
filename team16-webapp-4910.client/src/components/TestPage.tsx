import React, { useState } from 'react';
import axios from 'axios';

const TestPage: React.FC = () => {
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [registrationCode, setRegistrationCode] = useState('');
  const [message, setMessage] = useState('');
  const [token, setToken] = useState('');

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const response = await axios.post('/api/user/register', { 
        username, 
        email, 
        password, 
        registrationCode 
      });
      setMessage(`Registration successful. Role: ${response.data.role}`);
    } catch (error) {
      if (axios.isAxiosError(error)){ //check for axios issues
        setMessage(error.response?.data);
      }
      else if (error instanceof Error){ //should catch js
        setMessage(error.message);
      }
      else{ 
        setMessage(JSON.stringify(error));
      }
    }
  };

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const response = await axios.post('/api/user/login', { username, password });
      setToken(response.data.token);
      setMessage(`Login successful. Role: ${response.data.role}`);
    } catch (error) { //this should be a function later
      if (axios.isAxiosError(error)){ //check for axios issues
        setMessage(error.response?.data);
      }
      else if (error instanceof Error){ //should catch js
        setMessage(error.message);
      }
      else{ 
        setMessage(JSON.stringify(error));
      }
    }
  };

  return (
    <div>
      <h2>Test Registration and Login</h2>
      <form onSubmit={handleRegister}>
        <input
          type="text"
          placeholder="Username"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
        />
        <input
          type="email"
          placeholder="Email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
        />
        <input
          type="password"
          placeholder="Password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />
        <input
          type="text"
          placeholder="Registration Code"
          value={registrationCode}
          onChange={(e) => setRegistrationCode(e.target.value)}
        />
        <button type="submit">Register</button>
      </form>
      <form onSubmit={handleLogin}>
        <input
          type="text"
          placeholder="Username"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
        />
        <input
          type="password"
          placeholder="Password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />
        <button type="submit">Login</button>
      </form>
      {message && <p>{message}</p>}
      {token && <p>Token: {token}</p>}
    </div>
  );
};

export default TestPage;