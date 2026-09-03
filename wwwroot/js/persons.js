// Person CRUD Client for ASPNET PLAYGROUND - Brutalist Edition

// Calculate age from DateOnly string (YYYY-MM-DD)
function calculateAge(dateStr) {
    if (!dateStr) return null;
    const birthDate = new Date(dateStr);
    const today = new Date();
    let age = today.getFullYear() - birthDate.getFullYear();
    const monthDiff = today.getMonth() - birthDate.getMonth();
    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
        age--;
    }
    return age;
}

// Format date to Czech format (D. M. YYYY)
function formatCzechDate(dateStr) {
    if (!dateStr) return "";
    const parts = dateStr.split("-");
    if (parts.length === 3) {
        return `${parseInt(parts[2], 10)}. ${parseInt(parts[1], 10)}. ${parts[0]}`;
    }
    return dateStr;
}

// Random Person Generator for quick testing
function generateRandomPersonData() {
    const firstNames = ["Jan", "Petr", "Tomáš", "Jakub", "Martin", "Lukáš", "Michal", "Ondřej", "David", "Filip", "Tereza", "Eliška", "Anna", "Adéla", "Karolína", "Natálie", "Kristýna", "Lucie", "Barbora", "Veronika"];
    const lastNames = ["Novák", "Svoboda", "Novotný", "Dvořák", "Černý", "Procházka", "Kučera", "Veselý", "Horák", "Němec", "Marek", "Pospíšil", "Pokorný", "Hájek", "Král", "Jelínek", "Růžička", "Beneš", "Fiala", "Sedláček"];
    const streets = ["Václavské náměstí", "Národní třída", "Karlova", "Husova", "Dvořákova", "Smetanova", "Palackého", "Masarykova", "Nádražní", "Pod Strání", "Lipová", "Lesní"];
    const cities = ["110 00 Praha 1", "602 00 Brno", "702 00 Ostrava", "301 00 Plzeň", "460 01 Liberec", "779 00 Olomouc", "370 01 České Budějovice", "500 02 Hradec Králové"];

    const isFemale = Math.random() > 0.5;
    const fnPool = isFemale ? firstNames.slice(10) : firstNames.slice(0, 10);
    const firstName = fnPool[Math.floor(Math.random() * fnPool.length)];
    let lastName = lastNames[Math.floor(Math.random() * lastNames.length)];
    if (isFemale && !lastName.endsWith("á") && !lastName.endsWith("í")) {
        lastName += "ová";
    }

    const birthYear = Math.floor(1960 + Math.random() * 45); // 1960 - 2005
    const birthMonth = Math.floor(1 + Math.random() * 12);
    const birthDay = Math.floor(1 + Math.random() * 28);
    const dateStr = `${birthYear}-${String(birthMonth).padStart(2, "0")}-${String(birthDay).padStart(2, "0")}`;

    const rcMonth = isFemale ? birthMonth + 50 : birthMonth;
    const rcPart1 = `${String(birthYear % 100).padStart(2, "0")}${String(rcMonth).padStart(2, "0")}${String(birthDay).padStart(2, "0")}`;
    const rcPart2 = String(Math.floor(1000 + Math.random() * 9000));
    const rodneCislo = `${rcPart1}/${rcPart2}`;

    const phonePrefixes = ["+420 602", "+420 603", "+420 721", "+420 732", "+420 774", "+420 777", "+420 776"];
    const phonePrefix = phonePrefixes[Math.floor(Math.random() * phonePrefixes.length)];
    const phoneRest = `${Math.floor(100 + Math.random() * 900)} ${Math.floor(100 + Math.random() * 900)}`;
    const telefon = `${phonePrefix} ${phoneRest}`;

    const cleanFn = firstName.toLowerCase().normalize("NFD").replace(/[\u0300-\u036f]/g, "");
    const cleanLn = lastName.toLowerCase().normalize("NFD").replace(/[\u0300-\u036f]/g, "");
    const email = `${cleanFn}.${cleanLn}@example.cz`;

    const street = streets[Math.floor(Math.random() * streets.length)];
    const streetNo = Math.floor(1 + Math.random() * 150);
    const city = cities[Math.floor(Math.random() * cities.length)];
    const trvalaAdresa = `${street} ${streetNo}, ${city}`;

    return {
        jmeno: `${firstName} ${lastName}`,
        datumNarozeni: dateStr,
        trvalaAdresa: trvalaAdresa,
        rodneCislo: rodneCislo,
        telefon: telefon,
        email: email
    };
}

document.addEventListener("DOMContentLoaded", () => {
    const tableBody = document.getElementById("personsTableBody");
    const emptyState = document.getElementById("emptyPersonsState");
    const loadingState = document.getElementById("loadingState");
    const searchInput = document.getElementById("personSearchInput");
    const clearSearchBtn = document.getElementById("clearSearchBtn");
    const totalCountEl = document.getElementById("totalPersonsCount");
    const openCreateModalBtn = document.getElementById("openCreateModalBtn");
    const generateRandomBtn = document.getElementById("generateRandomPersonBtn");
    const refreshBtn = document.getElementById("refreshListBtn");
    const emptyCreateBtn = document.getElementById("emptyCreateBtn");
    const emptyGenerateBtn = document.getElementById("emptyGenerateBtn");

    // Detail Modal Elements
    const personDetailModal = document.getElementById("personDetailModal");
    const closeDetailModalBtn = document.getElementById("closeDetailModalBtn");
    const closeDetailBtn = document.getElementById("closeDetailBtn");
    const detailEditBtn = document.getElementById("detailEditBtn");
    const detailDeleteBtn = document.getElementById("detailDeleteBtn");
    const detailAvatarInitial = document.getElementById("detailAvatarInitial");
    const detailJmeno = document.getElementById("detailJmeno");
    const detailIdText = document.getElementById("detailIdText");
    const copyDetailIdBtn = document.getElementById("copyDetailIdBtn");
    const detailDatumNarozeni = document.getElementById("detailDatumNarozeni");
    const detailVek = document.getElementById("detailVek");
    const detailRodneCislo = document.getElementById("detailRodneCislo");
    const detailEmailLink = document.getElementById("detailEmailLink");
    const detailTelefonLink = document.getElementById("detailTelefonLink");
    const detailTrvalaAdresa = document.getElementById("detailTrvalaAdresa");

    // Create / Edit Modal Elements
    const personModal = document.getElementById("personModal");
    const modalTitle = document.getElementById("modalTitle");
    const modalIcon = document.getElementById("modalIcon");
    const closeModalBtn = document.getElementById("closeModalBtn");
    const cancelModalBtn = document.getElementById("cancelModalBtn");
    const personForm = document.getElementById("personForm");
    const personIdInput = document.getElementById("personId");
    const saveBtnText = document.getElementById("saveBtnText");
    const generalFormError = document.getElementById("generalFormError");

    // Delete Modal Elements
    const deleteModal = document.getElementById("deleteConfirmModal");
    const deleteConfirmMsg = document.getElementById("deleteConfirmMessage");
    const cancelDeleteBtn = document.getElementById("cancelDeleteBtn");
    const confirmDeleteBtn = document.getElementById("confirmDeleteBtn");

    // Toast
    const toastNotification = document.getElementById("toastNotification");
    const toastMessage = document.getElementById("toastMessage");

    let personsList = [];
    let deleteTargetId = null;
    let currentSelectedPerson = null;
    let toastTimeout = null;

    function showToast(msg) {
        if (!toastNotification || !toastMessage) return;
        toastMessage.textContent = msg;
        toastNotification.classList.add("show");
        if (toastTimeout) clearTimeout(toastTimeout);
        toastTimeout = setTimeout(() => {
            toastNotification.classList.remove("show");
        }, 2800);
    }

    // Fetch all persons from API
    async function fetchPersons() {
        if (loadingState) loadingState.style.display = "flex";
        if (tableBody) tableBody.style.opacity = "0.5";

        try {
            const res = await fetch("/api/persons?page=1&pageSize=200");
            if (!res.ok) throw new Error(`HTTP error! status: ${res.status}`);
            const data = await res.json();
            personsList = data.items || [];
            renderTable();
        } catch (err) {
            console.error("Chyba při načítání osob:", err);
            showToast("[ NEPOVAŘILO SE NAČÍST DATA ❌ ]");
            if (emptyState) {
                emptyState.style.display = "flex";
            }
        } finally {
            if (loadingState) loadingState.style.display = "none";
            if (tableBody) tableBody.style.opacity = "1";
        }
    }

    // Render table rows
    function renderTable() {
        if (!tableBody) return;
        tableBody.innerHTML = "";

        const query = searchInput ? searchInput.value.trim().toLowerCase() : "";
        const filtered = personsList.filter(p => {
            if (!query) return true;
            return (
                (p.jmeno && p.jmeno.toLowerCase().includes(query)) ||
                (p.id && p.id.toLowerCase().includes(query)) ||
                (p.email && p.email.toLowerCase().includes(query)) ||
                (p.telefon && p.telefon.toLowerCase().includes(query)) ||
                (p.rodneCislo && p.rodneCislo.toLowerCase().includes(query)) ||
                (p.trvalaAdresa && p.trvalaAdresa.toLowerCase().includes(query))
            );
        });

        if (totalCountEl) {
            totalCountEl.textContent = personsList.length;
        }

        if (filtered.length === 0) {
            if (emptyState) emptyState.style.display = "flex";
            return;
        }

        if (emptyState) emptyState.style.display = "none";

        filtered.forEach(person => {
            const tr = document.createElement("tr");
            tr.className = "person-row";

            const initials = person.jmeno ? person.jmeno.charAt(0).toUpperCase() : "?";
            const age = calculateAge(person.datumNarozeni);
            const ageLabel = age !== null ? `(${age} LET)` : "";
            const formattedDate = formatCzechDate(person.datumNarozeni);

            tr.innerHTML = `
                <td class="col-person">
                    <div class="person-cell person-clickable" title="Klikněte pro zobrazení podrobností">
                        <div class="table-avatar">
                            <span>${initials}</span>
                        </div>
                        <div class="person-cell-info">
                            <span class="person-name">${escapeHtml(person.jmeno)}</span>
                            <span class="person-id-sub">[ID: ${person.id ? person.id.substring(0, 8) : ""}...]</span>
                        </div>
                    </div>
                </td>
                <td class="col-birth">
                    <div class="birth-cell">
                        <span class="birth-date">${formattedDate}</span>
                        <span class="birth-age">${ageLabel}</span>
                    </div>
                </td>
                <td class="col-address">
                    <div class="address-cell" title="${escapeHtml(person.trvalaAdresa)}">
                        <span>📍 ${escapeHtml(person.trvalaAdresa)}</span>
                    </div>
                </td>
                <td class="col-rc">
                    <span class="rc-badge">[ ${escapeHtml(person.rodneCislo)} ]</span>
                </td>
                <td class="col-contact">
                    <div class="contact-cell">
                        <a href="mailto:${encodeURIComponent(person.email)}" class="contact-link email-link" title="Odeslat email">
                            ✉️ ${escapeHtml(person.email)}
                        </a>
                        <a href="tel:${encodeURIComponent(person.telefon)}" class="contact-link phone-link" title="Zavolat">
                            📞 ${escapeHtml(person.telefon)}
                        </a>
                    </div>
                </td>
                <td class="col-actions text-right">
                    <div class="row-actions">
                        <button type="button" class="btn-row-action btn-detail" data-id="${person.id}" title="Zobrazit celé podrobnosti osoby">
                            [ 👁️ DETAIL ]
                        </button>
                        <button type="button" class="btn-row-action btn-edit" data-id="${person.id}" title="Upravit záznam">
                            [ ✏️ UPRAVIT ]
                        </button>
                        <button type="button" class="btn-row-action btn-delete" data-id="${person.id}" title="Smazat záznam">
                            [ 🗑️ SMAZAT ]
                        </button>
                    </div>
                </td>
            `;

            // Attach detail listener on name cell & detail button
            const detailBtn = tr.querySelector(".btn-detail");
            if (detailBtn) {
                detailBtn.addEventListener("click", () => openDetailModal(person));
            }
            const personCell = tr.querySelector(".person-clickable");
            if (personCell) {
                personCell.addEventListener("click", () => openDetailModal(person));
            }

            // Attach edit listener
            const editBtn = tr.querySelector(".btn-edit");
            if (editBtn) {
                editBtn.addEventListener("click", () => openEditModal(person));
            }

            // Attach delete listener
            const delBtn = tr.querySelector(".btn-delete");
            if (delBtn) {
                delBtn.addEventListener("click", () => openDeleteModal(person));
            }

            tableBody.appendChild(tr);
        });
    }

    // Detail Modal logic
    function openDetailModal(person) {
        currentSelectedPerson = person;
        const initials = person.jmeno ? person.jmeno.charAt(0).toUpperCase() : "?";
        const age = calculateAge(person.datumNarozeni);
        const ageLabel = age !== null ? `${age} let` : "-";
        const formattedDate = formatCzechDate(person.datumNarozeni);

        if (detailAvatarInitial) detailAvatarInitial.textContent = initials;
        if (detailJmeno) detailJmeno.textContent = person.jmeno || "-";
        if (detailIdText) detailIdText.textContent = person.id || "-";
        if (detailDatumNarozeni) detailDatumNarozeni.textContent = formattedDate || "-";
        if (detailVek) detailVek.textContent = ageLabel;
        if (detailRodneCislo) detailRodneCislo.textContent = person.rodneCislo || "-";
        
        if (detailEmailLink) {
            detailEmailLink.textContent = person.email || "-";
            detailEmailLink.href = person.email ? `mailto:${encodeURIComponent(person.email)}` : "#";
        }
        if (detailTelefonLink) {
            detailTelefonLink.textContent = person.telefon || "-";
            detailTelefonLink.href = person.telefon ? `tel:${encodeURIComponent(person.telefon)}` : "#";
        }
        if (detailTrvalaAdresa) detailTrvalaAdresa.textContent = person.trvalaAdresa || "-";

        if (personDetailModal) personDetailModal.style.display = "flex";
    }

    function closeDetailModal() {
        if (personDetailModal) personDetailModal.style.display = "none";
    }

    if (closeDetailModalBtn) closeDetailModalBtn.addEventListener("click", closeDetailModal);
    if (closeDetailBtn) closeDetailBtn.addEventListener("click", closeDetailModal);

    if (copyDetailIdBtn) {
        copyDetailIdBtn.addEventListener("click", () => {
            if (currentSelectedPerson && currentSelectedPerson.id) {
                navigator.clipboard.writeText(currentSelectedPerson.id).then(() => {
                    showToast("[ ID ZKOPÍROVÁNO DO SCHRÁNKY 📋 ]");
                }).catch(() => {
                    prompt("ID osoby:", currentSelectedPerson.id);
                });
            }
        });
    }

    if (detailEditBtn) {
        detailEditBtn.addEventListener("click", () => {
            if (currentSelectedPerson) {
                const p = currentSelectedPerson;
                closeDetailModal();
                openEditModal(p);
            }
        });
    }

    if (detailDeleteBtn) {
        detailDeleteBtn.addEventListener("click", () => {
            if (currentSelectedPerson) {
                const p = currentSelectedPerson;
                closeDetailModal();
                openDeleteModal(p);
            }
        });
    }

    // Modal Create / Edit logic
    function openCreateModal() {
        clearForm();
        if (modalTitle) modalTitle.textContent = "NOVÁ OSOBA";
        if (modalIcon) modalIcon.textContent = "➕";
        if (saveBtnText) saveBtnText.textContent = "VYTVOŘIT OSOBU";
        if (personIdInput) personIdInput.value = "";
        if (personModal) personModal.style.display = "flex";
        const firstInput = document.getElementById("formJmeno");
        if (firstInput) firstInput.focus();
    }

    function openEditModal(person) {
        clearForm();
        if (modalTitle) modalTitle.textContent = "UPRAVIT OSOBU";
        if (modalIcon) modalIcon.textContent = "✏️";
        if (saveBtnText) saveBtnText.textContent = "ULOŽIT ZMĚNY";
        if (personIdInput) personIdInput.value = person.id;

        document.getElementById("formJmeno").value = person.jmeno || "";
        document.getElementById("formDatumNarozeni").value = person.datumNarozeni || "";
        document.getElementById("formTrvalaAdresa").value = person.trvalaAdresa || "";
        document.getElementById("formRodneCislo").value = person.rodneCislo || "";
        document.getElementById("formTelefon").value = person.telefon || "";
        document.getElementById("formEmail").value = person.email || "";

        if (personModal) personModal.style.display = "flex";
    }

    function closeModal() {
        if (personModal) personModal.style.display = "none";
        clearForm();
    }

    function clearForm() {
        if (personForm) personForm.reset();
        document.querySelectorAll(".field-error").forEach(el => el.textContent = "");
        if (generalFormError) {
            generalFormError.style.display = "none";
            generalFormError.textContent = "";
        }
    }

    // Form submit handler
    if (personForm) {
        personForm.addEventListener("submit", async (e) => {
            e.preventDefault();

            // Clear previous errors
            document.querySelectorAll(".field-error").forEach(el => el.textContent = "");
            if (generalFormError) generalFormError.style.display = "none";

            const id = personIdInput.value;
            const isEditing = Boolean(id);

            const payload = {
                jmeno: document.getElementById("formJmeno").value.trim(),
                datumNarozeni: document.getElementById("formDatumNarozeni").value,
                trvalaAdresa: document.getElementById("formTrvalaAdresa").value.trim(),
                rodneCislo: document.getElementById("formRodneCislo").value.trim(),
                telefon: document.getElementById("formTelefon").value.trim(),
                email: document.getElementById("formEmail").value.trim()
            };

            // Basic client validations
            let hasClientError = false;
            if (!payload.jmeno) {
                document.getElementById("errorJmeno").textContent = "Jméno je povinné pole.";
                hasClientError = true;
            }
            if (!payload.datumNarozeni) {
                document.getElementById("errorDatumNarozeni").textContent = "Zadejte platné datum narození.";
                hasClientError = true;
            }
            if (!payload.trvalaAdresa) {
                document.getElementById("errorTrvalaAdresa").textContent = "Trvalá adresa je povinné pole.";
                hasClientError = true;
            }
            if (!payload.rodneCislo) {
                document.getElementById("errorRodneCislo").textContent = "Rodné číslo je povinné pole.";
                hasClientError = true;
            }
            if (!payload.telefon) {
                document.getElementById("errorTelefon").textContent = "Telefonní číslo je povinné.";
                hasClientError = true;
            }
            if (!payload.email || !payload.email.includes("@")) {
                document.getElementById("errorEmail").textContent = "Zadejte platnou emailovou adresu.";
                hasClientError = true;
            }

            if (hasClientError) return;

            const submitBtn = document.getElementById("savePersonBtn");
            if (submitBtn) submitBtn.disabled = true;

            try {
                const url = isEditing ? `/api/persons/${id}` : "/api/persons";
                const method = isEditing ? "PUT" : "POST";

                const res = await fetch(url, {
                    method: method,
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(payload)
                });

                if (!res.ok) {
                    const errData = await res.json().catch(() => null);
                    if (errData && errData.errors) {
                        for (const [key, msgs] of Object.entries(errData.errors)) {
                            const errEl = document.getElementById(`error${key}`);
                            if (errEl) errEl.textContent = Array.isArray(msgs) ? msgs.join(", ") : msgs;
                        }
                    } else if (errData && errData.message) {
                        if (generalFormError) {
                            generalFormError.textContent = errData.message;
                            generalFormError.style.display = "block";
                        }
                    } else {
                        throw new Error(`Chyba serveru: ${res.status}`);
                    }
                    return;
                }

                closeModal();
                showToast(isEditing ? "[ OSOBA BYLA ÚSPĚŠNĚ UPRAVENA ✏️ ]" : "[ NOVÁ OSOBA VYTVOŘENA 🎉 ]");
                await fetchPersons();
            } catch (err) {
                console.error("Chyba při ukládání osoby:", err);
                if (generalFormError) {
                    generalFormError.textContent = "Při ukládání došlo k neočekávané chybě. Zkontrolujte spojení s databází.";
                    generalFormError.style.display = "block";
                }
            } finally {
                if (submitBtn) submitBtn.disabled = false;
            }
        });
    }

    // Delete Modal
    function openDeleteModal(person) {
        deleteTargetId = person.id;
        if (deleteConfirmMsg) {
            deleteConfirmMsg.textContent = `OPRAVDU SI PŘEJETE SMAZAT ZÁZNAM PRO OSOBU "${person.jmeno}"? TATO AKCE JE NEVRATNÁ.`;
        }
        if (deleteModal) deleteModal.style.display = "flex";
    }

    function closeDeleteModal() {
        if (deleteModal) deleteModal.style.display = "none";
        deleteTargetId = null;
    }

    if (confirmDeleteBtn) {
        confirmDeleteBtn.addEventListener("click", async () => {
            if (!deleteTargetId) return;
            confirmDeleteBtn.disabled = true;

            try {
                const res = await fetch(`/api/persons/${deleteTargetId}`, { method: "DELETE" });
                if (!res.ok && res.status !== 204) throw new Error("Chyba při mazání");

                closeDeleteModal();
                showToast("[ OSOBA BYLA ÚSPĚŠNĚ SMAZÁNA 🗑️ ]");
                await fetchPersons();
            } catch (err) {
                console.error("Chyba při mazání:", err);
                showToast("[ SMAZÁNÍ SE NEZDAŘILO ❌ ]");
            } finally {
                confirmDeleteBtn.disabled = false;
            }
        });
    }

    // Quick random generator
    async function createRandomPerson() {
        const data = generateRandomPersonData();
        try {
            const res = await fetch("/api/persons", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(data)
            });
            if (!res.ok) throw new Error("Generování selhalo");
            showToast(`[ VYGENEROVÁNA OSOBA: ${data.jmeno} 🎲 ]`);
            await fetchPersons();
        } catch (err) {
            console.error("Chyba při generování:", err);
            showToast("[ GENEROVÁNÍ SELHALO ❌ ]");
        }
    }

    // Event listeners
    if (openCreateModalBtn) openCreateModalBtn.addEventListener("click", openCreateModal);
    if (emptyCreateBtn) emptyCreateBtn.addEventListener("click", openCreateModal);
    if (closeModalBtn) closeModalBtn.addEventListener("click", closeModal);
    if (cancelModalBtn) cancelModalBtn.addEventListener("click", closeModal);

    if (cancelDeleteBtn) cancelDeleteBtn.addEventListener("click", closeDeleteModal);

    if (generateRandomBtn) generateRandomBtn.addEventListener("click", createRandomPerson);
    if (emptyGenerateBtn) emptyGenerateBtn.addEventListener("click", createRandomPerson);

    if (refreshBtn) {
        refreshBtn.addEventListener("click", async () => {
            const icon = document.getElementById("refreshIcon");
            if (icon) icon.classList.add("rotating");
            await fetchPersons();
            if (icon) icon.classList.remove("rotating");
            showToast("[ DATA OBNOVENA 🔄 ]");
        });
    }

    // Search filter input
    if (searchInput) {
        searchInput.addEventListener("input", () => {
            if (clearSearchBtn) {
                clearSearchBtn.style.display = searchInput.value ? "block" : "none";
            }
            renderTable();
        });
    }

    if (clearSearchBtn) {
        clearSearchBtn.addEventListener("click", () => {
            searchInput.value = "";
            clearSearchBtn.style.display = "none";
            renderTable();
            searchInput.focus();
        });
    }

    // Close modals on outside click or ESC
    window.addEventListener("click", (e) => {
        if (e.target === personDetailModal) closeDetailModal();
        if (e.target === personModal) closeModal();
        if (e.target === deleteModal) closeDeleteModal();
    });

    window.addEventListener("keydown", (e) => {
        if (e.key === "Escape") {
            closeDetailModal();
            closeModal();
            closeDeleteModal();
        }
    });

    // Helper escapeHtml
    function escapeHtml(str) {
        if (!str) return "";
        return String(str)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    // Initial load
    fetchPersons();
});
