import { apiRequest } from '../core/api.js';
import { formatDateTime, getValue } from '../core/utils.js';
import { openModal, closeModal } from '../core/utils.js';
import { createTicketCard } from '../components/ticketCard.js';

let currentTicketId = null;
let selectedMessageFile = null;

const MAX_ATTACHMENT_BYTES = 10 * 1024 * 1024;
const ALLOWED_ATTACHMENT_EXTENSIONS = ['.jpg', '.jpeg', '.png', '.pdf', '.docx'];

export function getCurrentTicketId() { return currentTicketId; }
export function setCurrentTicketId(id) { currentTicketId = id; }

function escapeHtml(value) {
    return String(value ?? '')
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
}

function formatFileSize(bytes) {
    if (!bytes) return '0 B';
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / 1024 / 1024).toFixed(1)} MB`;
}

function getExtension(filename) {
    const index = filename.lastIndexOf('.');
    return index >= 0 ? filename.slice(index).toLowerCase() : '';
}

function getAttachments(message) {
    return getValue(message, 'attachments') || [];
}

function renderSelectedFilePreview() {
    const preview = document.getElementById('selectedFilePreview');
    if (!preview) return;

    if (!selectedMessageFile) {
        preview.classList.remove('active');
        preview.innerHTML = '';
        return;
    }

    preview.classList.add('active');
    preview.innerHTML = `
        <span>${escapeHtml(selectedMessageFile.name)} <span class="attachment-meta">${formatFileSize(selectedMessageFile.size)}</span></span>
        <button type="button" onclick="window.clearSelectedMessageFile()">Remove</button>
    `;
}

export function clearSelectedMessageFile() {
    selectedMessageFile = null;
    const input = document.getElementById('messageFileInput');
    if (input) input.value = '';
    renderSelectedFilePreview();
}

export function handleMessageFileSelected(file) {
    if (!file) {
        clearSelectedMessageFile();
        return;
    }

    const extension = getExtension(file.name);
    if (!ALLOWED_ATTACHMENT_EXTENSIONS.includes(extension)) {
        alert(`File type ${extension || 'unknown'} is not allowed.`);
        clearSelectedMessageFile();
        return;
    }

    if (file.size > MAX_ATTACHMENT_BYTES) {
        alert('File exceeds max size of 10 MB.');
        clearSelectedMessageFile();
        return;
    }

    selectedMessageFile = file;
    renderSelectedFilePreview();
}

export function renderTickets(containerId, tickets, isUnassigned = false, onOpenDetail) {
    const container = document.getElementById(containerId);
    if (!container) return;
    container.innerHTML = '';
    if (!tickets || tickets.length === 0) {
        container.innerHTML = '<p style="text-align: center; color: var(--text-muted); padding: 20px;">No tickets found.</p>';
        return;
    }
    tickets.forEach(ticket => container.appendChild(createTicketCard(ticket, isUnassigned, onOpenDetail)));
}

export async function loadUnassignedTickets(role, onLogout, showErrorFn, renderFn) {
    try {
        const response = await apiRequest('/ticket/not-assign', { method: 'GET' }, role, onLogout);
        if (!response) return;
        const tickets = await response.json();
        if (renderFn) renderFn(tickets, true);
    } catch (err) {
        if (showErrorFn) showErrorFn('Failed to load unassigned tickets');
    }
}

export async function loadMyTickets(role, onLogout, showErrorFn, renderFn) {
    try {
        const response = await apiRequest('/ticket/admin', { method: 'GET' }, role, onLogout);
        if (!response) return;
        const tickets = await response.json();
        if (renderFn) renderFn(tickets, false);
    } catch (err) {
        if (showErrorFn) showErrorFn('Failed to load assigned tickets');
    }
}

export async function loadUserTickets(role, onLogout, renderFn) {
    try {
        const response = await apiRequest('/ticket/user', { method: 'GET' }, role, onLogout);
        if (!response) return;
        const tickets = await response.json();
        if (renderFn) renderFn(tickets, false);
    } catch (err) {
        console.error('Failed to load tickets', err);
    }
}

export async function openTicketDetail(ticket, isUnassigned, role, userId, onLogout, hideUnassignedAdminChat = true) {
    currentTicketId = getValue(ticket, 'id');
    clearSelectedMessageFile();
    const titleEl = document.getElementById('modalTicketTitle');
    const metaEl = document.getElementById('modalTicketMeta');
    const statusTag = document.getElementById('modalTicketStatus');
    const assignSection = document.getElementById('adminAssignSection');
    const chatInputArea = document.getElementById('chatInputArea');
    const solveBtn = document.getElementById('solveTicketBtn');

    if (titleEl) titleEl.textContent = getValue(ticket, 'title') || 'Untitled';
    if (metaEl) metaEl.textContent = `Created on ${formatDateTime(getValue(ticket, 'createdAt'))}`;

    const tIsSolved = getValue(ticket, 'isSolved') !== undefined ? getValue(ticket, 'isSolved') : getValue(ticket, 'IsSolved');
    if (statusTag) {
        statusTag.textContent = tIsSolved ? 'Solved' : 'Open';
        statusTag.className = `status-tag ${tIsSolved ? 'status-solved' : 'status-open'}`;
    }

    const isAdmin = role === 'admin';
    if (assignSection) assignSection.style.display = isAdmin && isUnassigned ? 'block' : 'none';
    if (chatInputArea) chatInputArea.style.display = (hideUnassignedAdminChat && isUnassigned && isAdmin) || tIsSolved ? 'none' : 'flex';
    if (solveBtn) solveBtn.style.display = isAdmin && !isUnassigned && !tIsSolved ? 'block' : 'none';

    await loadMessages(role, onLogout);
    openModal('ticketDetailModal');
}

export async function loadMessages(role, onLogout, options = {}) {
    const chatMessages = document.getElementById('chatMessages');
    if (!chatMessages) return;
    if (options.showLoading !== false) {
        chatMessages.innerHTML = '<p style="text-align: center; color: var(--text-muted);">Loading messages...</p>';
    }
    try {
        const response = await apiRequest('/message/all', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ ticketId: currentTicketId })
        }, role, onLogout);
        if (!response) return;
        const messages = await response.json();
        renderMessages(messages, role);
    } catch (err) {
        chatMessages.innerHTML = 'Error loading messages.';
    }
}

export async function reloadOpenChatMessages(role, onLogout) {
    const modal = document.getElementById('ticketDetailModal');
    if (!currentTicketId || !modal || modal.style.display === 'none') return;

    await loadMessages(role, onLogout, { showLoading: false });
}

function renderMessages(messages, role) {
    const chatMessages = document.getElementById('chatMessages');
    if (!chatMessages) return;
    chatMessages.innerHTML = '';
    const userId = window.currentUserId || '';
    messages.forEach(msg => {
        const div = document.createElement('div');
        const messageId = getValue(msg, 'id');
        const senderId = getValue(msg, 'senderId');
        const isSentByMe = senderId === userId;
        const classes = ['message'];
        if (isSentByMe) classes.push('sent');
        else classes.push(role === 'user' ? 'admin' : 'received');
        div.className = classes.join(' ');

        const content = escapeHtml(getValue(msg, 'content') || '');
        const attachmentsHtml = renderMessageAttachments(getAttachments(msg), isSentByMe);
        div.innerHTML = `
            ${content ? `<div class="message-content">${content}</div>` : ''}
            ${attachmentsHtml}
            ${isSentByMe ? `<div class="delete-msg" onclick="window.deleteMessage('${messageId}')">x</div>` : ''}
        `;
        chatMessages.appendChild(div);
    });
    chatMessages.scrollTop = chatMessages.scrollHeight;
}

function renderMessageAttachments(attachments, isSentByMe) {
    if (!attachments || attachments.length === 0) return '';

    const items = attachments.map(attachment => {
        const id = getValue(attachment, 'id');
        const filename = getValue(attachment, 'filename') || 'attachment';
        const contentType = getValue(attachment, 'contentType') || 'file';
        const size = getValue(attachment, 'size') || 0;

        return `
            <div class="attachment-pill">
                <div>
                    <div>${escapeHtml(filename)}</div>
                    <div class="attachment-meta">${escapeHtml(contentType)} - ${formatFileSize(size)}</div>
                </div>
                <div style="display: flex; gap: 8px;">
                    <button type="button" onclick="window.downloadAttachment('${id}', decodeURIComponent('${encodeURIComponent(filename)}'))">Download</button>
                    ${isSentByMe ? `<button type="button" onclick="window.deleteAttachment('${id}')">Remove</button>` : ''}
                </div>
            </div>
        `;
    }).join('');

    return `<div class="message-attachments">${items}</div>`;
}

export async function sendMessage(role, onLogout, loadMessagesFn) {
    const input = document.getElementById('messageInput');
    if (!input) return;
    const content = input.value.trim();
    const file = selectedMessageFile;
    if (!content && !file) return;
    try {
        const response = await apiRequest('/message', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ content: content || `Attachment: ${file.name}`, ticketId: currentTicketId })
        }, role, onLogout);
        if (response && response.ok) {
            const message = await response.json();
            if (file) await uploadAttachment(getValue(message, 'id'), file, role, onLogout);
            input.value = '';
            clearSelectedMessageFile();
            if (loadMessagesFn) await loadMessagesFn(role, onLogout);
        }
    } catch (err) {
        alert(err.message || 'Failed to send message');
    }
}

async function uploadAttachment(messageId, file, role, onLogout) {
    const formData = new FormData();
    formData.append('file', file);

    const response = await apiRequest(`/message/${messageId}/attachment`, {
        method: 'POST',
        body: formData
    }, role, onLogout);

    if (!response) return null;
    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || 'Failed to upload attachment');
    }
    return await response.json();
}

export async function downloadAttachment(attachmentId, filename, role, onLogout) {
    try {
        const response = await apiRequest(`/message/attachment/${attachmentId}`, {
            method: 'GET'
        }, role, onLogout);

        if (!response) return;
        if (!response.ok) throw new Error('Failed to download attachment');

        const blob = await response.blob();
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = filename || `attachment-${attachmentId}`;
        document.body.appendChild(link);
        link.click();
        link.remove();
        URL.revokeObjectURL(url);
    } catch (err) {
        alert(err.message || 'Failed to download attachment');
    }
}

export async function deleteAttachment(attachmentId, role, onLogout, loadMessagesFn) {
    if (!confirm('Delete this attachment?')) return;
    try {
        const response = await apiRequest(`/message/attachment/${attachmentId}`, {
            method: 'DELETE'
        }, role, onLogout);
        if (response && response.ok) {
            if (loadMessagesFn) await loadMessagesFn(role, onLogout);
        }
    } catch (err) {
        console.error(err);
    }
}

export async function deleteMessage(messageId, role, onLogout, loadMessagesFn) {
    if (!confirm('Delete this message?')) return;
    try {
        const response = await apiRequest('/message', {
            method: 'DELETE',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ messageId })
        }, role, onLogout);
        if (response && response.ok) {
            if (loadMessagesFn) await loadMessagesFn(role, onLogout);
        }
    } catch (err) {
        console.error(err);
    }
}

export async function assignTicketToMe(role, onLogout, loadUnassignedFn, closeModalFn) {
    try {
        const response = await apiRequest('/ticket/assign', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ ticketId: currentTicketId })
        }, role, onLogout);
        if (response && response.ok) {
            if (closeModalFn) closeModalFn('ticketDetailModal');
            if (loadUnassignedFn) await loadUnassignedFn();
        }
    } catch (err) {
        console.error(err);
    }
}

export async function solveTicket(role, onLogout, loadMyTicketsFn, closeModalFn) {
    try {
        const response = await apiRequest('/ticket/solve', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ ticketId: currentTicketId })
        }, role, onLogout);
        if (response && response.ok) {
            if (closeModalFn) closeModalFn('ticketDetailModal');
            if (loadMyTicketsFn) await loadMyTicketsFn();
        }
    } catch (err) {
        console.error(err);
    }
}

export async function submitTicket(title, role, onLogout, onSuccess, closeModalFn) {
    if (!title.trim()) return;
    try {
        const response = await apiRequest('/ticket', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ title })
        }, role, onLogout);
        if (response && response.ok) {
            if (closeModalFn) closeModalFn('createTicketModal');
            if (onSuccess) onSuccess();
        } else {
            const data = await response.json();
            alert(data.message || 'Error creating ticket');
        }
    } catch (err) {
        alert('Request failed');
    }
}
