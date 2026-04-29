export interface User {
  id: string;
  name: string;
  phoneNumber: string;
}

export interface AuthResponse {
  id: string;
  name: string;
  token: string;
}

export interface Message {
  id: string;
  senderId: string;
  receiverId: string;
  content: string;
  createdAt: string;
}
