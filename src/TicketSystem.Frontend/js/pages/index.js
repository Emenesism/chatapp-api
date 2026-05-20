import { getApiBaseUrl, setApiBaseUrl, getToken, setToken, setUserId, setRole, setUserData, getUserData, clearSession, getRole, getUserId } from '../core/config.js';
import { apiRequest, logout, logoutAll } from '../core/api.js';
import { openModal, closeModal, showError, showLoading, formatDate, formatDateTime, getInitial, getValue } from '../core/utils.js';
import { renderUsers, fetchAllUsers, loadUsers, applyFilters, clearFilters, goToPage, changeLimit, getCurrentPage, setCurrentPage, getCurrentLimit, setCurrentLimit } from '../modules/users.js';
import { renderTickets, loadUnassignedTickets, loadMyTickets, loadUserTickets, openTicketDetail, loadMessages, reloadOpenChatMessages, sendMessage, deleteMessage, assignTicketToMe, solveTicket, submitTicket, getCurrentTicketId, setCurrentTicketId, handleMessageFileSelected, clearSelectedMessageFile, downloadAttachment, deleteAttachment as deleteFileAttachment } from '../modules/tickets.js';
import { fetchAllAdmins, submitCreateAdmin, openCreateAdminModal, syncSuperAdminCheckbox } from '../modules/admins.js';
import { fetchAllSessions } from '../modules/sessions.js';
import { startNotifications, stopNotifications } from '../modules/notifications.js';
import { setAdminTab, showSuperAdminNavigation } from '../components/layout.js';

// ── Config ──
let API_BASE_URL = getApiBaseUrl();
let currentRole = getRole();
let currentToken = getToken();
let currentUserId = getUserId();

document.getElementById('apiUrlInput').value = API_BASE_URL;

window.API_BASE_URL = API_BASE_URL;
window.currentRole = currentRole;
window.currentUserId = currentUserId;

// ── Helpers ──
window.openModal = openModal;
window.closeModal = closeModal;

window.toggleSettings = () => document.getElementById('settingsPanel').classList.toggle('active');
window.updateApiUrl = (val) => { API_BASE_URL = val; setApiBaseUrl(val); };

function switchTab(role) {
    currentRole = role;
    window.currentRole = role;
    document.getElementById('userTab').classList.toggle('active', role === 'user');
    document.getElementById('adminTab').classList.toggle('active', role === 'admin');
    document.getElementById('headerSubtitle').textContent = role === 'user' ? 'Welcome back! User login.' : 'Administrator access.';
    document.getElementById('nameGroup').style.display = role === 'user' ? 'block' : 'none';
    document.getElementById('name').required = role === 'user';
    document.getElementById('errorMessage').style.display = 'none';
}
window.switchTab = switchTab;

function setLoading(isLoading) {
    document.getElementById('submitBtn').disabled = isLoading;
    document.getElementById('btnText').style.display = isLoading ? 'none' : 'inline';
    document.getElementById('btnSpinner').style.display = isLoading ? 'block' : 'none';
}

// ── Show screens ──
function showProfile(data, role) {
    document.getElementById('profileName').textContent = data.name || data.Name;
    document.getElementById('profileAvatar').textContent = getInitial(data.name || data.Name || 'U');
    document.getElementById('infoUsername').textContent = `@${data.username || data.Username}`;
    document.getElementById('infoRoleBadge').textContent = role;
    document.getElementById('infoRoleBadge').className = `badge badge-${role}`;
    document.getElementById('infoJoined').textContent = new Date(data.createdAt || data.CreatedAt).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' });
    document.getElementById('loginScreen').style.display = 'none';
    document.getElementById('profileScreen').style.display = 'block';
    document.getElementById('profileScreen').classList.add('animate-fade-in');
    startNotifications(role, onLogout);
    loadUserTickets(role, onLogout, (tickets) => renderTickets('userTicketList', tickets, false, (t, u) => window.openTicketDetail(t, u)));
}

function showAdminDashboard(data) {
    document.getElementById('adminNavName').textContent = data.name || data.Name;
    document.getElementById('adminNavAvatar').textContent = getInitial(data.name || data.Name || 'A');
    const isSuper = data.isSuperAdmin || data.IsSuperAdmin;
    showSuperAdminNavigation(isSuper);
    document.getElementById('loginWrapper').style.display = 'none';
    document.getElementById('adminLayout').style.display = 'flex';
    document.getElementById('adminLayout').classList.add('animate-fade-in');
    startNotifications('admin', onLogout);
    fetchAllUsers(
        (l) => showLoading('loadingState', l),
        (err) => showError('adminErrorAlert', 'adminErrorText', err),
        'admin',
        () => { clearSession(); window.location.reload(); },
        (data) => renderUsers(data, 'usersTableBody', 'emptyState', 'paginationContainer')
    );
}

function onLogout() {
    stopNotifications();
    document.getElementById('loginWrapper').style.display = 'flex';
    document.getElementById('loginScreen').style.display = 'block';
    document.getElementById('profileScreen').style.display = 'none';
    document.getElementById('adminLayout').style.display = 'none';
    document.getElementById('authForm').reset();
    switchTab('user');
}

// ── Auth form ──
document.getElementById('authForm').onsubmit = async (e) => {
    e.preventDefault();
    const name = document.getElementById('name').value;
    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;
    setLoading(true);
    document.getElementById('errorMessage').style.display = 'none';
    const endpoint = currentRole === 'user' ? '/auth/user/login' : '/auth/admin/login';
    const payload = currentRole === 'user' ? { name, username, password } : { username, password };
    try {
        const response = await apiRequest(endpoint, { method: 'POST', credentials: 'include', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) }, currentRole, onLogout);
        if (!response) return;
        const data = await response.json();
        if (response.ok) {
            setToken(data.token || data.Token || '');
            setUserId(data.id || data.Id || '');
            setRole(currentRole);
            setUserData(data);
            currentToken = getToken();
            currentUserId = getUserId();
            window.currentUserId = currentUserId;
            if (currentRole === 'admin') showAdminDashboard(data);
            else showProfile(data, currentRole);
        } else {
            throw new Error(data.message || 'Authentication failed');
        }
    } catch (err) {
        document.getElementById('errorMessage').textContent = err.message;
        document.getElementById('errorMessage').style.display = 'block';
    } finally {
        setLoading(false);
    }
};

window.logout = () => logout(currentRole, onLogout);
window.logoutAll = () => logoutAll(currentRole, onLogout);

// ── Init ──
function initApp() {
    const token = getToken();
    const savedData = getUserData();
    if (token && savedData) {
        if (currentRole === 'admin') showAdminDashboard(savedData);
        else showProfile(savedData, currentRole);
    } else {
        switchTab(currentRole);
    }
}

// ── Admin tab switching ──
window.switchAdminTab = (tab) => {
    setAdminTab(tab, {
        users: 'usersView',
        tickets: 'ticketsView',
        admins: 'adminsView',
        dashboard: 'dashboardView',
    }, {
        users: 'navUsers',
        tickets: 'navTickets',
        admins: 'navAdmins',
        dashboard: 'navDashboard',
    });
    const load = (l) => showLoading('loadingState', l);
    const err = (msg) => showError('adminErrorAlert', 'adminErrorText', msg);
    const render = (data) => renderUsers(data, 'usersTableBody', 'emptyState', 'paginationContainer');

    if (tab === 'tickets') loadUnassignedTickets('admin', onLogout, err, (tickets, u) => renderTickets('adminTicketList', tickets, u, (t, uu) => window.openTicketDetail(t, uu)));
    else if (tab === 'users') fetchAllUsers(load, err, 'admin', onLogout, render);
    else if (tab === 'admins') fetchAllAdmins('admin', onLogout, err);
    else if (tab === 'dashboard') fetchAllSessions('admin', onLogout, err);
};

// ── Wire up user functions to window ──
const showLoad = (l) => showLoading('loadingState', l);
const showErr = (msg) => showError('adminErrorAlert', 'adminErrorText', msg);
const renderFn = (data) => renderUsers(data, 'usersTableBody', 'emptyState', 'paginationContainer');

window.renderUsersCallback = renderFn;
window.fetchAllUsers = () => fetchAllUsers(showLoad, showErr, 'admin', onLogout, renderFn);
window.applyFilters = () => applyFilters(showLoad, showErr, 'admin', onLogout, renderFn);
window.clearFilters = () => clearFilters(() => fetchAllUsers(showLoad, showErr, 'admin', onLogout, renderFn));
window.goToPage = (page) => { setCurrentPage(page); loadUsers(showLoad, showErr, 'admin', onLogout, renderFn); };
window.changeLimit = (limit) => { setCurrentLimit(parseInt(limit)); setCurrentPage(1); loadUsers(showLoad, showErr, 'admin', onLogout, renderFn); };

// ── Wire up ticket functions ──
const loadUnassigned = () => loadUnassignedTickets('admin', onLogout, showErr, (tickets, u) => renderTickets('adminTicketList', tickets, u, (t, uu) => window.openTicketDetail(t, uu)));
const loadMy = () => loadMyTickets('admin', onLogout, showErr, (tickets, u) => renderTickets('adminTicketList', tickets, u, (t, uu) => window.openTicketDetail(t, uu)));

window.loadUnassignedTickets = loadUnassigned;
window.loadMyTickets = loadMy;
window.openTicketDetail = (t, u) => openTicketDetail(t, u, currentRole, currentUserId, onLogout, true);
window.sendMessage = () => sendMessage(currentRole, onLogout, (r, o) => loadMessages(r, o));
window.deleteMessage = (id) => deleteMessage(id, currentRole, onLogout, (r, o) => loadMessages(r, o));
window.handleMessageFileSelected = handleMessageFileSelected;
window.clearSelectedMessageFile = clearSelectedMessageFile;
window.downloadAttachment = (id, filename) => downloadAttachment(id, filename, currentRole, onLogout);
window.deleteAttachment = (id) => deleteFileAttachment(id, currentRole, onLogout, (r, o) => loadMessages(r, o));
window.assignTicketToMe = () => assignTicketToMe('admin', onLogout, loadUnassigned, closeModal);
window.solveTicket = () => solveTicket('admin', onLogout, loadMy, closeModal);

window.addEventListener('ticket-system:notification-received', () => {
    reloadOpenChatMessages(currentRole, onLogout);
});

// ── Wire up admin/ticket creation ──
window.openCreateTicketModal = () => { document.getElementById('newTicketTitle').value = ''; openModal('createTicketModal'); };
window.submitTicket = () => submitTicket(
    document.getElementById('newTicketTitle').value.trim(),
    currentRole, onLogout,
    () => loadUserTickets(currentRole, onLogout, (tickets) => renderTickets('userTicketList', tickets, false, (t, u) => window.openTicketDetail(t, u))),
    closeModal
);
window.openCreateAdminModal = () => openCreateAdminModal(openModal);
window.submitCreateAdmin = () => submitCreateAdmin('admin', onLogout, () => fetchAllAdmins('admin', onLogout, showErr), closeModal);
window.syncSuperAdminCheckbox = syncSuperAdminCheckbox;

initApp();
