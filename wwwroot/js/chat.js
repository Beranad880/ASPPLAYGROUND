// Global SignalR Chat Client

// Generate a deterministic color from string
function stringToColor(str) {
    let hash = 0;
    for (let i = 0; i < str.length; i++) {
        hash = str.charCodeAt(i) + ((hash << 5) - hash);
    }
    const hue = Math.abs(hash % 360);
    return `hsl(${hue}, 65%, 45%)`;
}

// Play a subtle notification chime using Web Audio API
function playChime() {
    try {
        const AudioContext = window.AudioContext || window.webkitAudioContext;
        if (!AudioContext) return;
        const ctx = new AudioContext();
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();

        osc.type = 'sine';
        osc.frequency.setValueAtTime(587.33, ctx.currentTime); // D5
        osc.frequency.exponentialRampToValueAtTime(880, ctx.currentTime + 0.1); // A5

        gain.gain.setValueAtTime(0.1, ctx.currentTime);
        gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.25);

        osc.connect(gain);
        gain.connect(ctx.destination);

        osc.start();
        osc.stop(ctx.currentTime + 0.25);
    } catch (e) {
        // AudioContext may be blocked before first user interaction
    }
}

document.addEventListener("DOMContentLoaded", () => {
    const messagesContainer = document.getElementById("messagesList");
    const emptyChat = document.getElementById("emptyChat");
    const messageInput = document.getElementById("messageInput");
    const sendBtn = document.getElementById("sendButton");
    const nicknameInput = document.getElementById("userInput");
    const userAvatar = document.getElementById("userAvatar");
    const onlineCountBadge = document.getElementById("onlineCount");
    const statusDot = document.getElementById("statusDot");
    const statusText = document.getElementById("statusText");
    const quickEmojiButtons = document.querySelectorAll(".quick-emoji-btn");

    // Initialize username from localStorage or generate random guest name
    let currentUsername = localStorage.getItem("chat_username");
    if (!currentUsername) {
        const randomId = Math.floor(1000 + Math.random() * 9000);
        currentUsername = `Uživatel_${randomId}`;
        localStorage.setItem("chat_username", currentUsername);
    }
    nicknameInput.value = currentUsername;
    updateUserAvatar(currentUsername);

    nicknameInput.addEventListener("input", (e) => {
        const val = e.target.value.trim() || "Host";
        currentUsername = val;
        localStorage.setItem("chat_username", val);
        updateUserAvatar(val);
    });

    function updateUserAvatar(name) {
        const initial = name ? name.charAt(0).toUpperCase() : "?";
        userAvatar.textContent = initial;
        userAvatar.style.backgroundColor = stringToColor(name || "default");
    }

    // Initialize SignalR Connection
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .build();

    function setStatus(state, text) {
        statusDot.className = "status-dot " + state;
        statusText.textContent = text;
    }

    // Reconnecting handlers
    connection.onreconnecting((error) => {
        setStatus("reconnecting", "Obnovuji připojení...");
        sendBtn.disabled = true;
    });

    connection.onreconnected((connectionId) => {
        setStatus("connected", "Připojeno");
        sendBtn.disabled = false;
    });

    connection.onclose((error) => {
        setStatus("disconnected", "Odpojeno");
        sendBtn.disabled = true;
    });

    // Receive message
    connection.on("ReceiveMessage", (data) => {
        // Data format: { user, message, timestamp, isSystem }
        appendMessage(data);
        if (data.user !== currentUsername) {
            playChime();
        }
    });

    // Update user count
    connection.on("UpdateUserCount", (count) => {
        onlineCountBadge.textContent = count;
    });

    // Load initial message history
    connection.on("LoadHistory", (messages) => {
        if (messages && messages.length > 0) {
            if (emptyChat) emptyChat.style.display = "none";
            messagesContainer.innerHTML = "";
            messages.forEach(msg => {
                appendMessage(msg, false);
            });
            scrollToBottom();
        }
    });

    function appendMessage(data, shouldScroll = true) {
        if (emptyChat) {
            emptyChat.style.display = "none";
        }

        const isMe = (data.user === currentUsername);
        const msgDiv = document.createElement("div");
        msgDiv.className = `message-item ${isMe ? 'outgoing' : 'incoming'}`;

        if (data.isSystem) {
            msgDiv.className = "message-item system";
            msgDiv.innerHTML = `<div class="message-bubble">${escapeHtml(data.message)}</div>`;
        } else {
            const avatarColor = stringToColor(data.user);
            const userInitial = data.user.charAt(0).toUpperCase();

            msgDiv.innerHTML = `
                ${!isMe ? `<div class="user-avatar-badge" style="background-color: ${avatarColor};">${userInitial}</div>` : ''}
                <div class="message-content-wrapper">
                    ${!isMe ? `<div class="message-meta"><span class="message-sender">${escapeHtml(data.user)}</span></div>` : ''}
                    <div class="message-bubble">
                        <div class="message-text">${escapeHtml(data.message)}</div>
                        <div class="message-time">${data.timestamp || new Date().toLocaleTimeString()}</div>
                    </div>
                </div>
            `;
        }

        messagesContainer.appendChild(msgDiv);

        if (shouldScroll) {
            scrollToBottom();
        }
    }

    function scrollToBottom() {
        messagesContainer.scrollTop = messagesContainer.scrollHeight;
    }

    function escapeHtml(unsafe) {
        return unsafe
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    // Send Message
    async function sendMessage() {
        const message = messageInput.value.trim();
        const user = nicknameInput.value.trim() || currentUsername;

        if (!message) return;

        try {
            sendBtn.disabled = true;
            await connection.invoke("SendMessage", user, message);
            messageInput.value = "";
            messageInput.focus();
        } catch (err) {
            console.error("Chyba při odesílání zprávy:", err);
            alert("Zprávu se nepodařilo odeslat. Zkontrolujte připojení.");
        } finally {
            sendBtn.disabled = false;
        }
    }

    const chatForm = document.getElementById("chatForm");
    chatForm.addEventListener("submit", (e) => {
        e.preventDefault();
        sendMessage();
    });

    // Quick emoji bar
    quickEmojiButtons.forEach(btn => {
        btn.addEventListener("click", () => {
            messageInput.value += btn.textContent;
            messageInput.focus();
        });
    });

    // Start Connection
    async function start() {
        try {
            setStatus("connecting", "Připojování...");
            await connection.start();
            setStatus("connected", "Připojeno");
            sendBtn.disabled = false;
        } catch (err) {
            console.error("SignalR Connection Error:", err);
            setStatus("disconnected", "Chyba připojení");
            setTimeout(start, 5000);
        }
    }

    start();
});
