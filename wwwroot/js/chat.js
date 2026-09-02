// SignalR Global Chat Client

// Generate deterministic vibrant HSL color from string
function stringToColor(str) {
    let hash = 0;
    for (let i = 0; i < str.length; i++) {
        hash = str.charCodeAt(i) + ((hash << 5) - hash);
    }
    const hue = Math.abs(hash % 360);
    return `hsl(${hue}, 70%, 50%)`;
}

// Generate cool random nicknames
function generateRandomName() {
    const adjectives = ["Cyber", "Neon", "Cosmic", "Pixel", "Solar", "Shadow", "Turbo", "Quantum", "Apex", "Hyper", "Vortex", "Frost"];
    const nouns = ["Ninja", "Rider", "Voyager", "Pilot", "Falcon", "Ghost", "Runner", "Spark", "Hunter", "Wizard", "Knight", "Echo"];
    const num = Math.floor(100 + Math.random() * 900);
    const adj = adjectives[Math.floor(Math.random() * adjectives.length)];
    const noun = nouns[Math.floor(Math.random() * nouns.length)];
    return `${adj}${noun}_${num}`;
}

// Audio notification manager
class SoundManager {
    constructor() {
        this.muted = localStorage.getItem("chat_sound_muted") === "true";
        this.ctx = null;
    }

    initAudio() {
        if (!this.ctx) {
            const AudioCtx = window.AudioContext || window.webkitAudioContext;
            if (AudioCtx) this.ctx = new AudioCtx();
        }
        if (this.ctx && this.ctx.state === "suspended") {
            this.ctx.resume();
        }
    }

    toggleMute() {
        this.muted = !this.muted;
        localStorage.setItem("chat_sound_muted", this.muted.toString());
        return this.muted;
    }

    playReceive() {
        if (this.muted) return;
        this.initAudio();
        if (!this.ctx) return;
        try {
            const now = this.ctx.currentTime;
            const osc = this.ctx.createOscillator();
            const gain = this.ctx.createGain();

            osc.type = "sine";
            osc.frequency.setValueAtTime(587.33, now); // D5
            osc.frequency.exponentialRampToValueAtTime(880, now + 0.1); // A5

            gain.gain.setValueAtTime(0.08, now);
            gain.gain.exponentialRampToValueAtTime(0.001, now + 0.22);

            osc.connect(gain);
            gain.connect(this.ctx.destination);

            osc.start(now);
            osc.stop(now + 0.22);
        } catch (e) {
            // Audio context policy
        }
    }

    playSend() {
        if (this.muted) return;
        this.initAudio();
        if (!this.ctx) return;
        try {
            const now = this.ctx.currentTime;
            const osc = this.ctx.createOscillator();
            const gain = this.ctx.createGain();

            osc.type = "sine";
            osc.frequency.setValueAtTime(440, now); // A4
            osc.frequency.exponentialRampToValueAtTime(659.25, now + 0.08); // E5

            gain.gain.setValueAtTime(0.05, now);
            gain.gain.exponentialRampToValueAtTime(0.001, now + 0.16);

            osc.connect(gain);
            gain.connect(this.ctx.destination);

            osc.start(now);
            osc.stop(now + 0.16);
        } catch (e) {}
    }
}

document.addEventListener("DOMContentLoaded", () => {
    const messagesContainer = document.getElementById("messagesList");
    const emptyChat = document.getElementById("emptyChat");
    const messageInput = document.getElementById("messageInput");
    const sendBtn = document.getElementById("sendButton");
    const nicknameInput = document.getElementById("userInput");
    const userAvatar = document.getElementById("userAvatar");
    const avatarText = document.getElementById("avatarText");
    const randomNameBtn = document.getElementById("randomNameBtn");
    const onlineCountBadge = document.getElementById("onlineCount");
    const statusDot = document.getElementById("statusDot");
    const statusText = document.getElementById("statusText");
    const soundToggle = document.getElementById("soundToggle");
    const soundIcon = document.getElementById("soundIcon");
    const shareBtn = document.getElementById("shareBtn");
    const scrollToBottomBtn = document.getElementById("scrollToBottomBtn");
    const toastNotification = document.getElementById("toastNotification");
    const toastMessage = document.getElementById("toastMessage");
    const reactionButtons = document.querySelectorAll(".reaction-btn");
    const suggestionChips = document.querySelectorAll(".suggestion-chip");
    const chatForm = document.getElementById("chatForm");

    const sound = new SoundManager();
    updateSoundIcon();

    // Show toast notification helper
    let toastTimeout = null;
    function showToast(msg) {
        if (toastMessage) toastMessage.textContent = msg;
        if (toastNotification) {
            toastNotification.classList.add("show");
            if (toastTimeout) clearTimeout(toastTimeout);
            toastTimeout = setTimeout(() => {
                toastNotification.classList.remove("show");
            }, 2500);
        }
    }

    // Sound toggle
    function updateSoundIcon() {
        if (soundIcon) {
            soundIcon.textContent = sound.muted ? "🔇" : "🔊";
        }
    }

    if (soundToggle) {
        soundToggle.addEventListener("click", () => {
            const isMuted = sound.toggleMute();
            updateSoundIcon();
            showToast(isMuted ? "Zvuk vypnut 🔇" : "Zvuk zapnut 🔊");
        });
    }

    // Share link button
    if (shareBtn) {
        shareBtn.addEventListener("click", async () => {
            const url = window.location.href;
            try {
                if (navigator.clipboard) {
                    await navigator.clipboard.writeText(url);
                    showToast("Odkaz zkopírován do schránky! 🔗");
                } else {
                    prompt("Zkopírujte tento odkaz:", url);
                }
            } catch (err) {
                prompt("Zkopírujte tento odkaz:", url);
            }
        });
    }

    // Nickname logic
    let currentUsername = localStorage.getItem("chat_username");
    if (!currentUsername) {
        currentUsername = generateRandomName();
        localStorage.setItem("chat_username", currentUsername);
    }
    nicknameInput.value = currentUsername;
    updateUserAvatarUI(currentUsername);

    function updateUserAvatarUI(name) {
        const cleanName = name.trim() || "Host";
        const initial = cleanName.charAt(0).toUpperCase();
        if (avatarText) avatarText.textContent = initial;
        if (userAvatar) {
            userAvatar.style.backgroundColor = stringToColor(cleanName);
        }
    }

    nicknameInput.addEventListener("input", (e) => {
        const val = e.target.value.trim() || "Host";
        currentUsername = val;
        localStorage.setItem("chat_username", val);
        updateUserAvatarUI(val);
    });

    if (randomNameBtn) {
        randomNameBtn.addEventListener("click", () => {
            const newName = generateRandomName();
            currentUsername = newName;
            nicknameInput.value = newName;
            localStorage.setItem("chat_username", newName);
            updateUserAvatarUI(newName);
            showToast(`Nová přezdívka: ${newName} 🎲`);
        });
    }

    // Initialize SignalR Connection
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .withAutomaticReconnect([0, 1500, 3000, 6000, 15000, 30000])
        .build();

    function setStatus(state, text) {
        if (statusDot) {
            statusDot.className = "status-dot " + state;
        }
        if (statusText) {
            statusText.textContent = text;
        }
    }

    connection.onreconnecting(() => {
        setStatus("reconnecting", "Obnovuji...");
        if (sendBtn) sendBtn.disabled = true;
    });

    connection.onreconnected(() => {
        setStatus("connected", "Připojeno");
        if (sendBtn) sendBtn.disabled = false;
        showToast("Spojení obnoveno! ⚡");
    });

    connection.onclose(() => {
        setStatus("disconnected", "Odpojeno");
        if (sendBtn) sendBtn.disabled = true;
    });

    // Receive message
    connection.on("ReceiveMessage", (data) => {
        appendMessage(data);
        if (data.user === currentUsername) {
            sound.playSend();
        } else {
            sound.playReceive();
        }
    });

    // Update user count
    connection.on("UpdateUserCount", (count) => {
        if (onlineCountBadge) {
            onlineCountBadge.textContent = count;
        }
    });

    // Load message history
    connection.on("LoadHistory", (messages) => {
        if (messages && messages.length > 0) {
            if (emptyChat) emptyChat.style.display = "none";
            messagesContainer.innerHTML = "";
            messages.forEach(msg => {
                appendMessage(msg, false);
            });
            scrollToBottom(false);
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
            msgDiv.innerHTML = `<div class="msg-bubble">${escapeHtml(data.message)}</div>`;
        } else {
            const avatarColor = stringToColor(data.user);
            const userInitial = data.user.charAt(0).toUpperCase();

            msgDiv.innerHTML = `
                ${!isMe ? `<div class="msg-avatar" style="background-color: ${avatarColor};">${userInitial}</div>` : ''}
                <div class="msg-wrapper">
                    ${!isMe ? `
                        <div class="msg-header">
                            <span class="msg-author" style="color: ${avatarColor};">${escapeHtml(data.user)}</span>
                        </div>
                    ` : ''}
                    <div class="msg-bubble" title="Klikněte pro zkopírování textu">
                        <div class="msg-text">${escapeHtml(data.message)}</div>
                        <div class="msg-meta">
                            <span class="msg-time">${data.timestamp || new Date().toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'})}</span>
                            ${isMe ? `<span class="msg-status-icon" title="Doručeno">✓✓</span>` : ''}
                        </div>
                    </div>
                </div>
            `;

            // Click message bubble to copy text
            const bubble = msgDiv.querySelector(".msg-bubble");
            if (bubble) {
                bubble.addEventListener("click", () => {
                    navigator.clipboard.writeText(data.message).then(() => {
                        showToast("Text zprávy zkopírován! 📋");
                    }).catch(() => {});
                });
            }
        }

        messagesContainer.appendChild(msgDiv);

        if (shouldScroll) {
            scrollToBottom(true);
        }
    }

    function scrollToBottom(smooth = true) {
        if (smooth) {
            messagesContainer.scrollTo({
                top: messagesContainer.scrollHeight,
                behavior: 'smooth'
            });
        } else {
            messagesContainer.scrollTop = messagesContainer.scrollHeight;
        }
    }

    // Scroll-to-bottom FAB detection
    messagesContainer.addEventListener("scroll", () => {
        const threshold = 150;
        const isScrolledUp = (messagesContainer.scrollHeight - messagesContainer.scrollTop - messagesContainer.clientHeight) > threshold;
        if (scrollToBottomBtn) {
            if (isScrolledUp) {
                scrollToBottomBtn.classList.add("visible");
            } else {
                scrollToBottomBtn.classList.remove("visible");
            }
        }
    });

    if (scrollToBottomBtn) {
        scrollToBottomBtn.addEventListener("click", () => {
            scrollToBottom(true);
        });
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
    async function sendMessage(textToSend) {
        const message = (textToSend !== undefined ? textToSend : messageInput.value).trim();
        const user = nicknameInput.value.trim() || currentUsername;

        if (!message) return;

        try {
            if (sendBtn) sendBtn.disabled = true;
            await connection.invoke("SendMessage", user, message);
            messageInput.value = "";
            messageInput.focus();
        } catch (err) {
            console.error("Chyba při odesílání zprávy:", err);
            showToast("Zprávu se nepodařilo odeslat. ❌");
        } finally {
            if (sendBtn) sendBtn.disabled = false;
        }
    }

    if (chatForm) {
        chatForm.addEventListener("submit", (e) => {
            e.preventDefault();
            sendMessage();
        });
    }

    // Quick emoji reactions
    reactionButtons.forEach(btn => {
        btn.addEventListener("click", () => {
            const emoji = btn.getAttribute("data-emoji") || btn.textContent;
            messageInput.value += emoji;
            messageInput.focus();
        });
    });

    // Quick suggestions in empty state
    suggestionChips.forEach(chip => {
        chip.addEventListener("click", () => {
            const msg = chip.getAttribute("data-msg") || chip.textContent;
            sendMessage(msg);
        });
    });

    // Start Connection
    async function start() {
        try {
            setStatus("connecting", "Připojování...");
            await connection.start();
            setStatus("connected", "Připojeno");
            if (sendBtn) sendBtn.disabled = false;
        } catch (err) {
            console.error("SignalR Connection Error:", err);
            setStatus("disconnected", "Chyba spojení");
            setTimeout(start, 4000);
        }
    }

    start();
});
