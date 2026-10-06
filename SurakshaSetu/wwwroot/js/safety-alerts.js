"use strict";


/* =========================================================
   SAFETY ALERT DATA
   ========================================================= */

const safetyAlerts = [

    {
        id: "ALT-1001",

        title: "Security Drill Scheduled",

        message:
            "A campus security drill is scheduled. Follow instructions from security staff and avoid restricted drill areas.",

        priority: "High",

        category: "Security",

        time: "Today · 08:15 AM",

        location: "Main Campus",

        publishedBy: "Campus Security",

        action:
            "Follow instructions from campus security staff and avoid restricted drill areas during the exercise."
    },


    {
        id: "ALT-1002",

        title: "Maintenance Near Hostel Road",

        message:
            "Temporary maintenance activity is taking place near Hostel Road. Please use the alternate pathway.",

        priority: "Medium",

        category: "Infrastructure",

        time: "Today · 10:30 AM",

        location: "Hostel Road",

        publishedBy: "Campus Facilities",

        action:
            "Use the alternate pathway and avoid the maintenance area until the work is completed."
    },


    {
        id: "ALT-1003",

        title: "South Gate Temporary Closure",

        message:
            "South Gate is temporarily closed. Use the Main Gate until further notice.",

        priority: "Medium",

        category: "Access",

        time: "Today · 09:20 AM",

        location: "South Gate",

        publishedBy: "Campus Security",

        action:
            "Use the Main Gate and follow security staff instructions."
    },


    {
        id: "ALT-1004",

        title: "North Gate Lighting Inspection",

        message:
            "Lighting inspection is being carried out near North Gate. Students should use well-lit pathways.",

        priority: "Low",

        category: "Infrastructure",

        time: "Yesterday · 06:45 PM",

        location: "North Gate",

        publishedBy: "Campus Facilities",

        action:
            "Prefer well-lit pathways while the inspection is in progress."
    },


    {
        id: "ALT-1005",

        title: "Evening Transport Pickup Change",

        message:
            "The evening student transport pickup point has temporarily moved to the Central Activity Area.",

        priority: "Medium",

        category: "Transport",

        time: "Yesterday · 05:10 PM",

        location: "Central Activity Area",

        publishedBy: "Transport Security",

        action:
            "Use the temporary pickup location for evening campus transport."
    },


    {
        id: "ALT-1006",

        title: "Hostel Access Reminder",

        message:
            "Students are reminded to carry valid campus identification when entering residential blocks.",

        priority: "Low",

        category: "Hostel",

        time: "28 Sep 2026 · 07:00 PM",

        location: "Residential Blocks",

        publishedBy: "Hostel Administration",

        action:
            "Carry your valid student identification when entering hostel/residential areas."
    },


    {
        id: "ALT-1007",

        title: "Rain and Pathway Safety Advisory",

        message:
            "During heavy rain, avoid waterlogged pathways and use marked routes around academic blocks.",

        priority: "Low",

        category: "Weather",

        time: "27 Sep 2026 · 04:30 PM",

        location: "Academic Blocks",

        publishedBy: "Campus Safety",

        action:
            "Avoid waterlogged areas and use marked pedestrian routes."
    },


    {
        id: "ALT-1008",

        title: "Emergency Response Contact Reminder",

        message:
            "Keep the campus safety contact information accessible and use Emergency SOS for urgent campus assistance.",

        priority: "High",

        category: "Emergency",

        time: "26 Sep 2026 · 11:00 AM",

        location: "Campus Wide",

        publishedBy: "Campus Safety",

        action:
            "For immediate danger, move to a safer location if possible and use the available emergency response channels."
    }

];


/* =========================================================
   DOM READY
   ========================================================= */

document.addEventListener(
    "DOMContentLoaded",
    function () {

        initializeSafetyAlerts();

    }
);


/* =========================================================
   INITIALIZATION
   ========================================================= */

function initializeSafetyAlerts() {

    updateAlertStatistics();

    renderAlerts("all");

    initializeAlertFilters();

}


/* =========================================================
   UPDATE STATISTICS
   ========================================================= */

function updateAlertStatistics() {

    const total =
        safetyAlerts.length;

    const high =
        safetyAlerts.filter(
            alert => alert.priority === "High"
        ).length;

    const medium =
        safetyAlerts.filter(
            alert => alert.priority === "Medium"
        ).length;


    const totalElement =
        document.getElementById("totalAlerts");

    const highElement =
        document.getElementById("highAlerts");

    const mediumElement =
        document.getElementById("mediumAlerts");


    if (totalElement) {

        totalElement.textContent =
            total;

    }


    if (highElement) {

        highElement.textContent =
            high;

    }


    if (mediumElement) {

        mediumElement.textContent =
            medium;

    }

}


/* =========================================================
   FILTER BUTTONS
   ========================================================= */

function initializeAlertFilters() {

    const filters =
        document.querySelectorAll(".sa-filter");


    filters.forEach(
        filter => {

            filter.addEventListener(
                "click",
                function () {

                    const priority =
                        this.dataset.priority;


                    filters.forEach(
                        button => {

                            button.classList.remove(
                                "active"
                            );

                        }
                    );


                    this.classList.add(
                        "active"
                    );


                    renderAlerts(
                        priority
                    );

                }
            );

        }
    );

}


/* =========================================================
   RENDER ALERTS
   ========================================================= */

function renderAlerts(priority) {

    const container =
        document.getElementById(
            "alertContainer"
        );


    if (!container) {

        return;

    }


    let filteredAlerts;


    if (
        !priority ||
        priority === "all"
    ) {

        filteredAlerts =
            safetyAlerts;

    }

    else {

        filteredAlerts =
            safetyAlerts.filter(
                alert =>
                    alert.priority === priority
            );

    }


    if (
        filteredAlerts.length === 0
    ) {

        container.innerHTML = `

            <div class="sa-empty-state">

                <div>
                    🔔
                </div>

                <h3>
                    No alerts found
                </h3>

                <p>
                    There are currently no alerts
                    in this category.
                </p>

            </div>

        `;

        return;

    }


    container.innerHTML =
        filteredAlerts
            .map(
                alert =>
                    createAlertCard(alert)
            )
            .join("");

}


/* =========================================================
   CREATE ALERT CARD
   ========================================================= */

function createAlertCard(alert) {

    const priorityClass =
        alert.priority.toLowerCase();


    const icon =
        getAlertIcon(
            alert.category
        );


    return `

        <article
            class="sa-alert-card ${priorityClass}">

            <div class="sa-alert-icon">

                ${icon}

            </div>


            <div class="sa-alert-content">

                <div class="sa-alert-top">

                    <span
                        class="sa-priority-badge ${priorityClass}">

                        ${getPriorityText(
                            alert.priority
                        )}

                    </span>


                    <span
                        class="sa-alert-category">

                        ${escapeHtml(
                            alert.category
                        )}

                    </span>

                </div>


                <h3>

                    ${escapeHtml(
                        alert.title
                    )}

                </h3>


                <p>

                    ${escapeHtml(
                        alert.message
                    )}

                </p>


                <div class="sa-alert-meta">

                    <span>
                        🕐
                        ${escapeHtml(
                            alert.time
                        )}
                    </span>

                    <span>
                        📍
                        ${escapeHtml(
                            alert.location
                        )}
                    </span>

                </div>

            </div>


            <div class="sa-alert-action">

                <button
                    type="button"
                    onclick="openAlertDetails('${alert.id}')">

                    View Details

                </button>

            </div>

        </article>

    `;

}


/* =========================================================
   PRIORITY TEXT
   ========================================================= */

function getPriorityText(priority) {

    if (priority === "High") {

        return "🚨 High";

    }


    if (priority === "Medium") {

        return "⚠ Medium";

    }


    return "ℹ Information";

}


/* =========================================================
   ALERT ICON
   ========================================================= */

function getAlertIcon(category) {

    switch (category) {

        case "Security":
            return "🛡";

        case "Infrastructure":
            return "🛠";

        case "Transport":
            return "🚌";

        case "Emergency":
            return "🚨";

        case "Hostel":
            return "🏠";

        case "Weather":
            return "🌧";

        case "Access":
            return "🚧";

        default:
            return "🔔";

    }

}


/* =========================================================
   OPEN ALERT DETAILS
   ========================================================= */

function openAlertDetails(alertId) {

    const alert =
        safetyAlerts.find(
            item =>
                item.id === alertId
        );


    if (!alert) {

        return;

    }


    document.getElementById(
        "modalTitle"
    ).textContent =
        alert.title;


    document.getElementById(
        "modalId"
    ).textContent =
        alert.id;


    document.getElementById(
        "modalCategory"
    ).textContent =
        alert.category;


    document.getElementById(
        "modalLocation"
    ).textContent =
        alert.location;


    document.getElementById(
        "modalTime"
    ).textContent =
        alert.time;


    document.getElementById(
        "modalPublisher"
    ).textContent =
        alert.publishedBy;


    document.getElementById(
        "modalMessage"
    ).textContent =
        alert.message;


    document.getElementById(
        "modalAction"
    ).textContent =
        alert.action;


    const priorityElement =
        document.getElementById(
            "modalPriority"
        );


    priorityElement.textContent =
        getPriorityText(
            alert.priority
        );


    priorityElement.className =
        "sa-priority-badge " +
        alert.priority.toLowerCase();


    document.getElementById(
        "alertModal"
    ).classList.add(
        "show"
    );

}


/* =========================================================
   CLOSE MODAL
   ========================================================= */

function closeAlertModal() {

    document.getElementById(
        "alertModal"
    ).classList.remove(
        "show"
    );

}


/* =========================================================
   ESCAPE KEY
   ========================================================= */

document.addEventListener(
    "keydown",
    function (event) {

        if (
            event.key === "Escape"
        ) {

            closeAlertModal();

        }

    }
);


/* =========================================================
   CLICK OUTSIDE MODAL
   ========================================================= */

document.addEventListener(
    "click",
    function (event) {

        const modal =
            document.getElementById(
                "alertModal"
            );


        if (
            event.target === modal
        ) {

            closeAlertModal();

        }

    }
);


/* =========================================================
   REPORT RELATED CONCERN
   ========================================================= */

function reportRelatedConcern() {
    closeAlertModal();
    window.location.href = "/IncidentReport/Create";
}


/* =========================================================
   EMERGENCY SOS
   ========================================================= */

function activateEmergencySOS() {
    closeAlertModal();
    if (typeof bootstrap !== 'undefined') {
        const modalEl = document.getElementById('sosModal');
        if (modalEl) {
            const modal = new bootstrap.Modal(modalEl);
            modal.show();
            return;
        }
    }
    alert("Emergency SOS module is not available on this page.");
}


/* =========================================================
   SAFETY MAP
   ========================================================= */

function openSafetyMap() {
    window.location.href = "/Safety/Map";
}


/* =========================================================
   EMERGENCY CONTACTS
   ========================================================= */

function openEmergencyContacts() {
    window.location.href = "/EmergencyContact";
}


/* =========================================================
   HTML ESCAPING
   ========================================================= */

function escapeHtml(value) {

    if (!value) {

        return "";

    }


    return String(value)
        .replace(
            /&/g,
            "&amp;"
        )
        .replace(
            /</g,
            "&lt;"
        )
        .replace(
            />/g,
            "&gt;"
        )
        .replace(
            /"/g,
            "&quot;"
        )
        .replace(
            /'/g,
            "&#039;"
        );

}
