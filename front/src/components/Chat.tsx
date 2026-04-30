import { useState, useEffect, useRef } from 'react';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { api, HUB_URL } from '../api';
import type { User, Message, AuthResponse, Group, GroupMessage } from '../types';
import { Send, LogOut, MessageSquare, Plus, Hash, Users } from 'lucide-react';
import { format } from 'date-fns';

interface ChatProps {
  user: AuthResponse;
  onLogout: () => void;
}

export default function Chat({ user, onLogout }: ChatProps) {
  const [users, setUsers] = useState<User[]>([]);
  const [groups, setGroups] = useState<Group[]>([]);
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const [selectedGroup, setSelectedGroup] = useState<Group | null>(null);
  const [viewMode, setViewMode] = useState<'pv' | 'group'>('pv');
  const [messages, setMessages] = useState<(Message | GroupMessage)[]>([]);
  const [input, setInput] = useState('');
  const [connection, setConnection] = useState<HubConnection | null>(null);
  const [isConnecting, setIsConnecting] = useState(true);
  const [showCreateGroup, setShowCreateGroup] = useState(false);
  const [showJoinGroup, setShowJoinGroup] = useState(false);
  const [newGroupName, setNewGroupName] = useState('');
  const [newGroupUsername, setNewGroupUsername] = useState('');
  const [joinUsername, setJoinUsername] = useState('');

  const messagesEndRef = useRef<HTMLDivElement>(null);
  const selectedUserRef = useRef<User | null>(null);
  const selectedGroupRef = useRef<Group | null>(null);
  const viewModeRef = useRef<'pv' | 'group'>('pv');

  useEffect(() => {
    selectedUserRef.current = selectedUser;
    selectedGroupRef.current = selectedGroup;
    viewModeRef.current = viewMode;
  }, [selectedUser, selectedGroup, viewMode]);

  useEffect(() => {
    fetchUsers();
    fetchGroups();
    const conn = setupSignalR();
    return () => {
      conn.then(c => c.stop());
    };
  }, []);

  useEffect(() => {
    if (viewMode === 'pv' && selectedUser) {
      fetchHistory(selectedUser.id);
    } else if (viewMode === 'group' && selectedGroup) {
      fetchGroupHistory(selectedGroup.username);
    }
  }, [selectedUser, selectedGroup, viewMode]);

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

    conn.on('group_message', (payload: GroupMessage) => {
      const currentSelectedGroup = selectedGroupRef.current;
      const currentViewMode = viewModeRef.current;

      if (currentViewMode === 'group' && payload.groupUsername === currentSelectedGroup?.username) {
        setMessages(prev => {
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

  const fetchGroups = async () => {
    try {
      const { data } = await api.get('/groups/my');
      setGroups(data);
    } catch (err) {
      console.error('Failed to fetch groups', err);
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

  const fetchGroupHistory = async (groupUsername: string) => {
    try {
      const { data } = await api.get(`/groups/${groupUsername}`);
      setMessages(data.reverse());
    } catch (err) {
      console.error('Failed to fetch group history', err);
    }
  };

  const handleSendMessage = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!input.trim() || !connection) return;

    const messageContent = input;
    setInput(''); 

    try {
      if (viewMode === 'pv' && selectedUser) {
        await connection.invoke('SendPv', selectedUser.id, messageContent);
      } else if (viewMode === 'group' && selectedGroup) {
        await connection.invoke('SendGroup', selectedGroup.username, messageContent);
      }
    } catch (err) {
      console.error('Failed to send message', err);
      setInput(messageContent);
    }
  };

  const handleCreateGroup = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const { data } = await api.post('/groups', {
        name: newGroupName,
        username: newGroupUsername
      });
      setGroups(prev => [...prev, data]);
      setShowCreateGroup(false);
      setNewGroupName('');
      setNewGroupUsername('');
      
      // Join SignalR group
      if (connection) {
        await connection.invoke('JoinGroupChat', data.username);
      }
    } catch (err) {
      console.error('Failed to create group', err);
    }
  };

  const handleJoinGroup = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const { data } = await api.post('/groups/join', {
        username: joinUsername
      });
      if (!groups.some(g => g.id === data.groupId)) {
        setGroups(prev => [...prev, { id: data.groupId, name: data.groupName, username: data.groupUsername }]);
      }
      setShowJoinGroup(false);
      setJoinUsername('');

      // Join SignalR group
      if (connection) {
        await connection.invoke('JoinGroupChat', data.groupUsername);
      }
    } catch (err) {
      console.error('Failed to join group', err);
    }
  };

  return (
    <div className="app-container">
      {/* Sidebar */}
      <aside className="sidebar">
        <div className="sidebar-header">
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <h3 style={{ fontSize: '1.25rem' }}>ChatApp</h3>
            <button onClick={onLogout} style={{ background: 'none', border: 'none', color: '#94a3b8', cursor: 'pointer' }}>
              <LogOut size={20} />
            </button>
          </div>
          
          <div className="tabs">
            <button 
              className={`tab-btn ${viewMode === 'pv' ? 'active' : ''}`}
              onClick={() => setViewMode('pv')}
            >
              <Users size={16} /> People
            </button>
            <button 
              className={`tab-btn ${viewMode === 'group' ? 'active' : ''}`}
              onClick={() => setViewMode('group')}
            >
              <Hash size={16} /> Groups
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
          {viewMode === 'pv' ? (
            users.map(u => (
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
            ))
          ) : (
            <>
              <div style={{ padding: '0.5rem', display: 'flex', gap: '0.5rem' }}>
                <button 
                  onClick={() => setShowCreateGroup(true)}
                  style={{ flex: 1, padding: '0.4rem', fontSize: '0.75rem', borderRadius: '0.4rem', border: '1px solid #334155', background: 'transparent', color: 'white', cursor: 'pointer', display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '0.25rem' }}
                >
                  <Plus size={14} /> Create
                </button>
                <button 
                  onClick={() => setShowJoinGroup(true)}
                  style={{ flex: 1, padding: '0.4rem', fontSize: '0.75rem', borderRadius: '0.4rem', border: '1px solid #334155', background: 'transparent', color: 'white', cursor: 'pointer', display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '0.25rem' }}
                >
                  <Users size={14} /> Join
                </button>
              </div>
              {groups.map(g => (
                <div 
                  key={g.id} 
                  className={`user-item ${selectedGroup?.id === g.id ? 'active' : ''}`}
                  onClick={() => setSelectedGroup(g)}
                >
                  <div className="avatar" style={{ background: '#475569' }}>#</div>
                  <div style={{ flex: 1 }}>
                    <p style={{ fontWeight: 600 }}>{g.name}</p>
                    <p style={{ fontSize: '0.75rem', color: '#94a3b8' }}>@{g.username}</p>
                  </div>
                </div>
              ))}
            </>
          )}
        </div>
      </aside>

      {/* Main Chat area */}
      <main className="chat-window">
        {((viewMode === 'pv' && selectedUser) || (viewMode === 'group' && selectedGroup)) ? (
          <>
            <header className="chat-header">
              <div className="avatar">{viewMode === 'pv' ? selectedUser?.name[0] : '#'}</div>
              <div>
                <h4 style={{ fontWeight: 600 }}>{viewMode === 'pv' ? selectedUser?.name : selectedGroup?.name}</h4>
                <p style={{ fontSize: '0.75rem', color: '#94a3b8' }}>
                  {viewMode === 'pv' ? 'Personal Chat' : `@${selectedGroup?.username}`}
                </p>
              </div>
            </header>

            <div className="messages-container">
              {messages.length === 0 && (
                <div style={{ flex: 1, display: 'flex', alignItems: 'center', justifySelf: 'center', color: '#94a3b8', opacity: 0.5 }}>
                  <p>No messages yet. Say hi!</p>
                </div>
              )}
              {messages.map(m => {
                const isMe = m.senderId === user.id;
                // For group messages, we might want to show the sender name if we had it.
                // Since we don't have sender names in the message payload easily available right now, 
                // we'll just show the bubble.
                return (
                  <div key={m.id} className={`message-bubble ${isMe ? 'me' : 'other'}`}>
                    {m.content}
                    <div className="message-time">
                      {format(new Date(m.createdAt), 'HH:mm')}
                    </div>
                  </div>
                );
              })}
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
            <p>Select a {viewMode === 'pv' ? 'user' : 'group'} to start chatting</p>
          </div>
        )}
      </main>

      {/* Modals */}
      {showCreateGroup && (
        <div className="modal-overlay">
          <div className="modal">
            <h3>Create Group</h3>
            <form onSubmit={handleCreateGroup} className="modal-form">
              <input 
                className="modal-input"
                type="text" 
                placeholder="Group Name" 
                value={newGroupName} 
                onChange={e => setNewGroupName(e.target.value)}
                required
              />
              <input 
                className="modal-input"
                type="text" 
                placeholder="Username (unique)" 
                value={newGroupUsername} 
                onChange={e => setNewGroupUsername(e.target.value)}
                required
              />
              <div className="modal-actions">
                <button type="button" className="btn-secondary" onClick={() => setShowCreateGroup(false)}>Cancel</button>
                <button type="submit" className="btn-primary">Create</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {showJoinGroup && (
        <div className="modal-overlay">
          <div className="modal">
            <h3>Join Group</h3>
            <form onSubmit={handleJoinGroup} className="modal-form">
              <input 
                className="modal-input"
                type="text" 
                placeholder="Group Username" 
                value={joinUsername} 
                onChange={e => setJoinUsername(e.target.value)}
                required
              />
              <div className="modal-actions">
                <button type="button" className="btn-secondary" onClick={() => setShowJoinGroup(false)}>Cancel</button>
                <button type="submit" className="btn-primary">Join</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
