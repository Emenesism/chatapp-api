import { useState, useEffect, useRef } from 'react';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { api, HUB_URL } from '../api';
import type { User, Message, AuthResponse } from '../types';
import { Send, LogOut, MessageSquare } from 'lucide-react';
import { format } from 'date-fns';

interface ChatProps {
  user: AuthResponse;
  onLogout: () => void;
}

export default function Chat({ user, onLogout }: ChatProps) {
  const [users, setUsers] = useState<User[]>([]);
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const [messages, setMessages] = useState<Message[]>([]);
  const [input, setInput] = useState('');
  const [connection, setConnection] = useState<HubConnection | null>(null);
  const [isConnecting, setIsConnecting] = useState(true);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const selectedUserRef = useRef<User | null>(null);

  useEffect(() => {
    selectedUserRef.current = selectedUser;
  }, [selectedUser]);

  useEffect(() => {
    fetchUsers();
    const conn = setupSignalR();
    return () => {
      conn.then(c => c.stop());
    };
  }, []);

  useEffect(() => {
    if (selectedUser) {
      fetchHistory(selectedUser.id);
    }
  }, [selectedUser]);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  const setupSignalR = async () => {
    const conn = new HubConnectionBuilder()
      .withUrl(`${HUB_URL}?access_token=${user.token}`)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    conn.on('pv_message', (payload: Message) => {
      // Use ref to avoid stale closure
      const currentSelected = selectedUserRef.current;
      if (
        (payload.senderId === user.id && payload.receiverId === currentSelected?.id) ||
        (payload.senderId === currentSelected?.id && payload.receiverId === user.id)
      ) {
        setMessages(prev => {
          // Avoid duplicates
          if (prev.some(m => m.id === payload.id)) return prev;
          return [...prev, payload];
        });
      }
    });

    try {
      await conn.start();
      setConnection(conn);
      setIsConnecting(false);
      return conn;
    } catch (err) {
      console.error('SignalR Connection Error: ', err);
      setIsConnecting(false);
      return conn;
    }
  };

  const fetchUsers = async () => {
    try {
      const { data } = await api.get('/users');
      setUsers(data.filter((u: User) => u.id !== user.id));
    } catch (err) {
      console.error('Failed to fetch users', err);
    }
  };

  const fetchHistory = async (otherUserId: string) => {
    try {
      const { data } = await api.get(`/messages/pv/${otherUserId}`);
      setMessages(data.reverse()); 
    } catch (err) {
      console.error('Failed to fetch history', err);
    }
  };

  const handleSendMessage = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!input.trim() || !selectedUser || !connection) return;

    const messageContent = input;
    setInput(''); 

    try {
      await connection.invoke('SendPv', selectedUser.id, messageContent);
    } catch (err) {
      console.error('Failed to send message', err);
      setInput(messageContent);
    }
  };

  return (
    <div className="app-container">
      {/* Sidebar */}
      <aside className="sidebar">
        <div className="sidebar-header">
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <h3 style={{ fontSize: '1.25rem' }}>Chats</h3>
            <button onClick={onLogout} style={{ background: 'none', border: 'none', color: '#94a3b8', cursor: 'pointer' }}>
              <LogOut size={20} />
            </button>
          </div>
          <div style={{ marginTop: '1rem', display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
            <div className="avatar" style={{ width: 32, height: 32, fontSize: '0.8rem' }}>
              {user.name[0]}
            </div>
            <div>
              <p style={{ fontSize: '0.875rem', fontWeight: 600 }}>{user.name}</p>
              <p style={{ fontSize: '0.75rem', color: isConnecting ? '#f59e0b' : '#10b981' }}>
                {isConnecting ? 'Connecting...' : 'Online'}
              </p>
            </div>
          </div>
        </div>

        <div className="user-list">
          {users.map(u => (
            <div 
              key={u.id} 
              className={`user-item ${selectedUser?.id === u.id ? 'active' : ''}`}
              onClick={() => setSelectedUser(u)}
            >
              <div className="avatar">{u.name[0]}</div>
              <div style={{ flex: 1 }}>
                <p style={{ fontWeight: 600 }}>{u.name}</p>
                <p style={{ fontSize: '0.75rem', color: '#94a3b8' }}>Click to chat</p>
              </div>
            </div>
          ))}
        </div>
      </aside>

      {/* Main Chat area */}
      <main className="chat-window">
        {selectedUser ? (
          <>
            <header className="chat-header">
              <div className="avatar">{selectedUser.name[0]}</div>
              <div>
                <h4 style={{ fontWeight: 600 }}>{selectedUser.name}</h4>
                <p style={{ fontSize: '0.75rem', color: '#94a3b8' }}>Personal Chat</p>
              </div>
            </header>

            <div className="messages-container">
              {messages.length === 0 && (
                <div style={{ flex: 1, display: 'flex', alignItems: 'center', justifySelf: 'center', color: '#94a3b8', opacity: 0.5 }}>
                  <p>No messages yet. Say hi!</p>
                </div>
              )}
              {messages.map(m => (
                <div key={m.id} className={`message-bubble ${m.senderId === user.id ? 'me' : 'other'}`}>
                  {m.content}
                  <div className="message-time">
                    {format(new Date(m.createdAt), 'HH:mm')}
                  </div>
                </div>
              ))}
              <div ref={messagesEndRef} />
            </div>

            <form className="chat-input-area" onSubmit={handleSendMessage}>
              <input 
                type="text" 
                placeholder="Type a message..." 
                value={input}
                onChange={(e) => setInput(e.target.value)}
              />
              <button type="submit" className="send-btn">
                <Send size={20} />
              </button>
            </form>
          </>
        ) : (
          <div style={{ flex: 1, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', color: '#94a3b8' }}>
            <MessageSquare size={64} style={{ marginBottom: '1rem', opacity: 0.2 }} />
            <p>Select a user to start chatting</p>
          </div>
        )}
      </main>
    </div>
  );
}
