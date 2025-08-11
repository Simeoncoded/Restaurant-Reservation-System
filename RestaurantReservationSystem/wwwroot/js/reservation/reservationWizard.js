(function () {
    const dateInput = document.querySelector("input[name='Date']");
    const tableSelect = document.getElementById("TableID");
    const timeHidden = document.getElementById("Time");
    const timesContainer = document.getElementById("availableTimes");

    if (!timesContainer) return; // step 2 only

    // init flatpickr if available
    if (window.flatpickr && dateInput) {
        flatpickr(dateInput, { dateFormat: "Y-m-d", minDate: "today", onChange: handleChange });
    }

    dateInput?.addEventListener("change", handleChange);
    tableSelect?.addEventListener("change", handleChange);

    // If user already picked Date + Table (returning to step 2), populate immediately
    document.addEventListener("DOMContentLoaded", () => {
        if (dateInput?.value && tableSelect?.value) handleChange();
    });

    function handleChange() {
        const dateStr = dateInput?.value;
        const tableId = tableSelect?.value;
        timeHidden.value = "";
        timesContainer.innerHTML = "";
        if (!dateStr || !tableId) return;

        fetch(`/Reservation/AvailableTimes?date=${encodeURIComponent(dateStr)}&tableId=${encodeURIComponent(tableId)}`)
            .then(r => r.ok ? r.json() : Promise.reject())
            .then(renderTimes)
            .catch(() => timesContainer.innerHTML = "<p class='text-danger m-0'>Could not load available times.</p>");
    }

    function renderTimes(times) {
        if (!Array.isArray(times) || times.length === 0) {
            timesContainer.innerHTML = "<p class='text-danger m-0'>No available times for this date and table.</p>";
            return;
        }
        const frag = document.createDocumentFragment();
        times.forEach(label => {
            const btn = document.createElement("button");
            btn.type = "button";
            btn.className = "btn btn-outline-primary m-1 time-button";
            btn.textContent = label;                 // e.g., "10:00 AM"
            btn.dataset.value = toTimeSpan(label);   // "HH:mm:ss"
            btn.addEventListener("click", () => select(btn));
            frag.appendChild(btn);
        });
        timesContainer.appendChild(frag);
    }

    function select(btn) {
        document.querySelectorAll(".time-button").forEach(b => {
            b.classList.remove("btn-primary");
            b.classList.add("btn-outline-primary");
            b.setAttribute("aria-pressed", "false");
        });
        btn.classList.remove("btn-outline-primary");
        btn.classList.add("btn-primary");
        btn.setAttribute("aria-pressed", "true");
        timeHidden.value = btn.dataset.value; // post as TimeSpan "HH:mm:ss"
    }

    function toTimeSpan(label) {
        const m = /^(\d{1,2}):(\d{2})\s*(AM|PM)$/i.exec(label);
        if (!m) return "";
        let h = parseInt(m[1], 10), min = parseInt(m[2], 10);
        const mer = m[3].toUpperCase();
        if (mer === "PM" && h !== 12) h += 12;
        if (mer === "AM" && h === 12) h = 0;
        return `${String(h).padStart(2, "0")}:${String(min).padStart(2, "0")}:00`;
    }
})();
