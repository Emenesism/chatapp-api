import { apiRequest } from '../core/api.js';
import { getApiBaseUrl, getToken } from '../core/config.js';

let connection;
let notifications = [];
let initialized = false;
let activeRole = 'user';
let activeLogout;

function ensureStyles() {
    if (document.getElementById('notificationStyles')) return;

    const style = document.createElement('style');
    style.id = 'notificationStyles';
    style.textContent = `
        .notification-widget { position: relative; display: inline-flex; align-items: center; margin-right: 12px; }
        .notification-button { position: relative; width: 42px; height: 42px; border-radius: 50%; border: 1px solid rgba(255,255,255,.12); background: rgba(15,23,42,.65); color: #f8fafc; cursor: pointer; display: inline-flex; align-items: center; justify-content: center; transition: .2s ease; }
        .notification-button:hover { border-color: rgba(99,102,241,.65); transform: translateY(-1px); }
        .notification-count { position: absolute; top: -5px; right: -5px; min-width: 18px; height: 18px; padding: 0 5px; border-radius: 999px; background: #ef4444; color: white; font-size: 11px; font-weight: 800; display: none; align-items: center; justify-content: center; }
        .notification-panel { position: absolute; right: 0; top: 50px; width: min(360px, calc(100vw - 32px)); max-height: 390px; overflow-y: auto; z-index: 1000; display: none; background: rgba(15,23,42,.98); border: 1px solid rgba(255,255,255,.12); border-radius: 16px; box-shadow: 0 18px 50px rgba(0,0,0,.45); backdrop-filter: blur(16px); }
        .notification-panel.active { display: block; }
        .notification-header { display: flex; justify-content: space-between; gap: 12px; align-items: center; padding: 14px 16px; border-bottom: 1px solid rgba(255,255,255,.08); }
        .notification-header strong { font-size: .95rem; }
        .notification-clear { border: 0; background: transparent; color: #a5b4fc; cursor: pointer; font-weight: 700; font-size: .8rem; }
        .notification-list { display: grid; }
        .notification-item { padding: 13px 16px; border-bottom: 1px solid rgba(255,255,255,.06); }
        .notification-item:last-child { border-bottom: 0; }
        .notification-title { font-weight: 700; margin-bottom: 5px; }
        .notification-body { color: #cbd5e1; line-height: 1.35; font-size: .88rem; }
        .notification-date { color: #64748b; margin-top: 7px; font-size: .75rem; }
        .notification-empty { padding: 24px 16px; color: #94a3b8; text-align: center; }
        .notification-toast { position: fixed; right: 24px; bottom: 24px; width: min(360px, calc(100vw - 32px)); background: rgba(30,41,59,.98); border: 1px solid rgba(99,102,241,.45); border-radius: 16px; padding: 14px 16px; z-index: 1100; box-shadow: 0 18px 50px rgba(0,0,0,.4); animation: notificationSlide .25s ease; }
        @keyframes notificationSlide { from { opacity: 0; transform: translateY(12px); } to { opacity: 1; transform: translateY(0); } }
        @media (max-width: 640px) { .notification-widget { margin-right: 8px; } .notification-toast { right: 16px; bottom: 16px; } }
    `;
    document.head.appendChild(style);
}

function notificationDate(value) {
    if (!value) return '';
    return new Date(value).toLocaleString();
}

function normalizeNotification(notification) {
    return {
        id: notification.id || notification.Id || `${Date.now()}-${Math.random()}`,
        title: notification.title || notification.Title || 'Notification',
        body: notification.body || notification.Body || '',
        createdAt: notification.createdAt || notification.CreatedAt || new Date().toISOString()
    };
}

function renderNotifications() {
    const count = document.getElementById('notificationCount');
    const list = document.getElementById('notificationList');
    if (!count || !list) return;

    count.textContent = notifications.length;
    count.style.display = notifications.length ? 'inline-flex' : 'none';

    if (!notifications.length) {
        list.innerHTML = '<div class="notification-empty">No unread notifications</div>';
        return;
    }

    list.innerHTML = notifications.map(notification => `
        <div class="notification-item">
            <div class="notification-title">${escapeHtml(notification.title)}</div>
            <div class="notification-body">${escapeHtml(notification.body)}</div>
            <div class="notification-date">${escapeHtml(notificationDate(notification.createdAt))}</div>
        </div>
    `).join('');
}

function escapeHtml(value) {
    return String(value)
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#039;');
}

function showToast(notification) {
    const toast = document.createElement('div');
    toast.className = 'notification-toast';
    toast.innerHTML = `
        <div class="notification-title">${escapeHtml(notification.title)}</div>
        <div class="notification-body">${escapeHtml(notification.body)}</div>
    `;
    document.body.appendChild(toast);
    setTimeout(() => toast.remove(), 5000);
}

function addNotification(notification, showPopup = false) {
    const normalized = normalizeNotification(notification);
    notifications = [normalized, ...notifications.filter(item => item.id !== normalized.id)];
    renderNotifications();
    if (showPopup) showToast(normalized);
}

function ensureWidget() {
    if (document.getElementById('notificationWidget')) return;

    ensureStyles();

    const widget = document.createElement('div');
    widget.id = 'notificationWidget';
    widget.className = 'notification-widget';
    widget.innerHTML = `
        <button class="notification-button" type="button" aria-label="Notifications" id="notificationButton">
            <span>&#128276;</span>
            <span class="notification-count" id="notificationCount">0</span>
        </button>
        <div class="notification-panel" id="notificationPanel">
            <div class="notification-header">
                <strong>Notifications</strong>
                <button class="notification-clear" type="button" id="notificationClear">Mark all read</button>
            </div>
            <div class="notification-list" id="notificationList"></div>
        </div>
    `;

    const target = document.querySelector('.user-profile-header') || document.querySelector('.user-profile') || document.body;
    target.prepend(widget);

    document.getElementById('notificationButton').addEventListener('click', (event) => {
        event.stopPropagation();
        document.getElementById('notificationPanel').classList.toggle('active');
    });

    document.getElementById('notificationClear').addEventListener('click', async () => {
        await markNotificationsAsRead(activeRole, activeLogout);
    });

    document.addEventListener('click', (event) => {
        if (!widget.contains(event.target)) {
            document.getElementById('notificationPanel').classList.remove('active');
        }
    });

    renderNotifications();
}

export async function loadNotifications(role, onLogout) {
    const response = await apiRequest('/notification', { method: 'GET' }, role, onLogout);
    if (!response || !response.ok) return;

    const data = await response.json();
    notifications = data.map(normalizeNotification);
    renderNotifications();
}

export async function markNotificationsAsRead(role = 'user', onLogout) {
    await apiRequest('/notification/read-all', { method: 'POST' }, role, onLogout);
    notifications = [];
    renderNotifications();
}

export async function startNotifications(role, onLogout) {
    activeRole = role;
    activeLogout = onLogout;

    if (initialized) return;
    initialized = true;

    ensureWidget();
    await loadNotifications(role, onLogout);

    try {
        const signalR = await import('https://cdn.jsdelivr.net/npm/@microsoft/signalr@8.0.7/+esm');
        connection = new signalR.HubConnectionBuilder()
            .withUrl(`${getApiBaseUrl()}/hub/notifications`, {
                accessTokenFactory: () => getToken()
            })
            .withAutomaticReconnect()
            .build();

        connection.on('ReceiveNotification', notification => addNotification(notification, true));
        await connection.start();
    } catch (error) {
        console.warn('SignalR notifications unavailable. DB fallback is still active.', error);
    }
}

export async function stopNotifications() {
    initialized = false;
    notifications = [];
    renderNotifications();

    if (connection) {
        await connection.stop();
        connection = null;
    }
}
