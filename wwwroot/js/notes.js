document.addEventListener('DOMContentLoaded', () => {
    const notesContainer = document.getElementById('notesContainer');
    const searchInput = document.getElementById('searchInput');
    const newNoteBtn = document.getElementById('newNoteBtn');
    const noteModal = document.getElementById('noteModal');
    const closeModalBtn = document.getElementById('closeModalBtn');
    const cancelNoteBtn = document.getElementById('cancelNoteBtn');
    const deleteNoteBtn = document.getElementById('deleteNoteBtn');
    const noteForm = document.getElementById('noteForm');
    
    const idInput = document.getElementById('noteId');
    const titleInput = document.getElementById('noteTitle');
    const contentInput = document.getElementById('noteContent');

    let searchTimeout = null;

    // Load initial data
    loadNotes();

    // SignalR Setup
    if (typeof signalR !== 'undefined') {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/notesHub")
            .withAutomaticReconnect()
            .build();

        connection.on("NoteCreated", (note) => {
            console.log("Live Note Created:", note);
            loadNotes();
        });

        connection.on("NoteUpdated", (note) => {
            console.log("Live Note Updated:", note);
            loadNotes();
        });

        connection.on("NoteDeleted", (id) => {
            console.log("Live Note Deleted:", id);
            loadNotes();
        });

        connection.start().catch(err => console.error("SignalR Notes error:", err));
    }

    // API Calls
    async function loadNotes(query = "") {
        try {
            const url = query ? `/api/notes/search?query=${encodeURIComponent(query)}` : `/api/notes`;
            const res = await fetch(url);
            if (!res.ok) throw new Error("Failed to load notes");
            const notes = await res.json();
            renderNotes(notes);
        } catch (e) {
            console.error(e);
        }
    }

    async function saveNote(note) {
        const isUpdate = !!note.id;
        const url = isUpdate ? `/api/notes/${note.id}` : `/api/notes`;
        const method = isUpdate ? 'PUT' : 'POST';

        const res = await fetch(url, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(note)
        });

        if (!res.ok) throw new Error("Failed to save note");
    }

    async function deleteNote(id) {
        const res = await fetch(`/api/notes/${id}`, { method: 'DELETE' });
        if (!res.ok) throw new Error("Failed to delete note");
    }

    // Rendering
    function renderNotes(notes) {
        notesContainer.innerHTML = "";
        if (notes.length === 0) {
            notesContainer.innerHTML = `<div class="brutalist-empty-box" style="grid-column: 1 / -1;">ŽÁDNÉ POZNÁMKY NENALEZENY</div>`;
            return;
        }

        notes.forEach(n => {
            const card = document.createElement('div');
            card.className = 'note-card';
            card.innerHTML = `
                <div class="note-title">${escapeHtml(n.title)}</div>
                <div class="note-content">${escapeHtml(n.content)}</div>
                <div class="note-meta">${new Date(n.updatedAt).toLocaleString()}</div>
            `;
            card.addEventListener('click', () => openModal(n));
            notesContainer.appendChild(card);
        });
    }

    // Event Listeners
    searchInput.addEventListener('input', (e) => {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
            loadNotes(e.target.value);
        }, 300);
    });

    newNoteBtn.addEventListener('click', () => openModal());

    const closeAll = () => { noteModal.style.display = 'none'; };
    closeModalBtn.addEventListener('click', closeAll);
    cancelNoteBtn.addEventListener('click', closeAll);

    noteForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        try {
            await saveNote({
                id: idInput.value || null,
                title: titleInput.value.trim(),
                content: contentInput.value.trim()
            });
            closeAll();
            loadNotes(searchInput.value);
        } catch (err) {
            alert(err.message);
        }
    });

    deleteNoteBtn.addEventListener('click', async () => {
        if (!idInput.value) return;
        if (confirm("Opravdu smazat poznámku?")) {
            try {
                await deleteNote(idInput.value);
                closeAll();
                loadNotes(searchInput.value);
            } catch (err) {
                alert(err.message);
            }
        }
    });

    function openModal(note = null) {
        if (note) {
            idInput.value = note.id;
            titleInput.value = note.title;
            contentInput.value = note.content;
            deleteNoteBtn.style.display = 'inline-block';
        } else {
            noteForm.reset();
            idInput.value = "";
            deleteNoteBtn.style.display = 'none';
        }
        noteModal.style.display = 'flex';
    }

    function escapeHtml(str) {
        if (!str) return "";
        return str.replace(/[&<>"']/g, function(m) {
            return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#039;' }[m];
        });
    }
});
