import axios from 'axios';

const API_URL = 'http://localhost:5184'; // Backend port from launchSettings.json

export const api = axios.create({
  baseURL: API_URL,
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export const HUB_URL = `${API_URL}/hubs/chat`;
