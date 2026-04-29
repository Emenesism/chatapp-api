import { useState, useEffect } from 'react';
import Login from './components/Login';
import Chat from './components/Chat';
import type { AuthResponse } from './types';

function App() {
  const [auth, setAuth] = useState<AuthResponse | null>(null);

  useEffect(() => {
    const savedAuth = localStorage.getItem('auth');
    if (savedAuth) {
      const parsed = JSON.parse(savedAuth);
      setAuth(parsed);
      localStorage.setItem('token', parsed.token);
    }
  }, []);

  const handleLogin = (data: AuthResponse) => {
    setAuth(data);
    localStorage.setItem('auth', JSON.stringify(data));
    localStorage.setItem('token', data.token);
  };

  const handleLogout = () => {
    setAuth(null);
    localStorage.removeItem('auth');
    localStorage.removeItem('token');
  };

  if (!auth) {
    return <Login onLogin={handleLogin} />;
  }

  return <Chat user={auth} onLogout={handleLogout} />;
}

export default App;
