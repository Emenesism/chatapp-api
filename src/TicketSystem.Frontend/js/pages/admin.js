import { getApiBaseUrl, setApiBaseUrl, getToken, setToken, setUserId, setRole, setUserData, getUserData, clearSession, getUserId } from '../core/config.js';
import { apiRequest, logout, logoutAll } from '../core/api.js';
import { openModal, closeModal, showError, showLoading, formatDate, formatDateTime, getInitial, getValue } from '../core/utils.js';
import { renderUsers, fetchAllUsers, loadUsers, applyFilters, clearFilters, goToPage, changeLimit, getCurrentPage, setCurrentPage, getCurrentLimit, setCurrentLimit } from '../modules/users.js';
import { renderTickets, loadUnassignedTickets, loadMyTickets, loadUserTickets, openTicketDetail, loadMessages, sendMessage, deleteMessage, assignTicketToMe, solveTicket, submitTicket, handleMessageFileSelected, clearSelectedMessageFile, downloadAttachment, deleteAttachment as deleteFileAttachment } from '../modules/tickets.js';
import { fetchAllAdmins, submitCreateAdmin, openCreateAdminModal, syncSuperAdminCheckbox } from '../modules/admins.js';
import { fetchAllSessions } from '../modules/sessions.js';
import { startNotifications, stopNotifications } from '../modules/notifications.js';
import { setAdminTab, setPageHeader, showSuperAdminNavigation } from '../components/layout.js';

// ── Config ──
let API_BASE_URL = getApiBaseUrl();
let AUTH_TOKEN = getToken();
let currentUserId = getUserId();
window.currentUserId = currentUserId;

document.getElementById('apiUrlInput').value = API_BASE_URL;
document.getElementById('jwtTokenInput').value = AUTH_TOKEN;

window.toggleSettings = () => document.getElementById('settingsPanel').classList.toggle('active');
window.updateApiUrl = (val) => { API_BASE_URL = val; setApiBaseUrl(val); };
window.updateToken = (val) => { AUTH_TOKEN = val; setToken(val); };

function onLogout() {
    stopNotifications();
    localStorage.clear();
    window.location.href = 'index.html';
}

window.logout = onLogout;
window.logoutAll = () => logoutAll('admin', onLogout);
window.openModal = openModal;
window.closeModal = closeModal;

// ── Tab switching ──
window.switchAdminTab = (tab) => {
    const views = { users: 'usersView', tickets: 'ticketsView', admins: 'adminsView', dashboard: 'dashboardView' };
    const navs = { users: 'navUsers', tickets: 'navTickets', admins: 'navAdmins', dashboard: 'navDashboard' };
    const titles = {
        users: ['Users Directory', 'Manage users and system settings'],
        tickets: ['Support Tickets', 'Manage and resolve customer requests'],
        admins: ['System Administrators', 'Manage system access and roles'],
        dashboard: ['System Dashboard', 'Monitor active system sessions']
    };

    setAdminTab(tab, views, navs);
    setPageHeader('viewTitle', 'viewSubtitle', titles[tab][0], titles[tab][1]);

    const load = (l) => showLoading('loadingState', l);
    const err = (msg) => showError('errorAlert', 'errorText', msg);
    const render = (data) => renderUsers(data, 'usersTableBody', 'emptyState', 'paginationContainer');
    const renderTicketsFn = (tickets, u) => renderTickets('adminTicketList', tickets, u, (t, uu) => window.openTicketDetail(t, uu));

    if (tab === 'tickets') loadUnassignedTickets('admin', onLogout, err, renderTicketsFn);
    else if (tab === 'users') fetchAllUsers(load, err, 'admin', onLogout, render);
    else if (tab === 'admins') fetchAllAdmins('admin', onLogout, err);
    else if (tab === 'dashboard') fetchAllSessions('admin', onLogout, err);
};

// ── Wire up user functions ──
const showLoad = (l) => showLoading('loadingState', l);
const showErr = (msg) => showError('errorAlert', 'errorText', msg);
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
window.openTicketDetail = (t, u) => openTicketDetail(t, u, 'admin', currentUserId, onLogout, false);
window.sendMessage = () => sendMessage('admin', onLogout, (r, o) => loadMessages(r, o));
window.deleteMessage = (id) => deleteMessage(id, 'admin', onLogout, (r, o) => loadMessages(r, o));
window.handleMessageFileSelected = handleMessageFileSelected;
window.clearSelectedMessageFile = clearSelectedMessageFile;
window.downloadAttachment = (id, filename) => downloadAttachment(id, filename, 'admin', onLogout);
window.deleteAttachment = (id) => deleteFileAttachment(id, 'admin', onLogout, (r, o) => loadMessages(r, o));
window.assignTicketToMe = () => assignTicketToMe('admin', onLogout, loadUnassigned, closeModal);
window.solveTicket = () => solveTicket('admin', onLogout, loadMy, closeModal);

// ── Wire up admin functions ──
window.openCreateAdminModal = () => openCreateAdminModal(openModal);
window.submitCreateAdmin = () => submitCreateAdmin('admin', onLogout, () => fetchAllAdmins('admin', onLogout, showErr), closeModal);
window.syncSuperAdminCheckbox = syncSuperAdminCheckbox;

// ── Init ──
document.addEventListener('DOMContentLoaded', () => {
    const savedData = getUserData();
    if (savedData && (savedData.isSuperAdmin || savedData.IsSuperAdmin)) {
        showSuperAdminNavigation(true);
    }
    startNotifications('admin', onLogout);
    fetchAllUsers(showLoad, showErr, 'admin', onLogout, renderFn);
});
