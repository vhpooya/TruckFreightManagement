// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Global variables
let map;
let markers = {};
let polylines = {};
let updateInterval;

// Initialize the application
$(document).ready(function () {
    initializeMap();
    setupEventListeners();
    startPeriodicUpdates();
});

// Initialize the map
function initializeMap() {
    map = L.map('map').setView([35.6892, 51.3890], 13);
    L.tileLayer('https://api.neshan.org/v1/static/tile/{z}/{x}/{y}.png?key=YOUR_API_KEY', {
        attribution: '© <a href="https://www.neshan.org">Neshan</a> contributors'
    }).addTo(map);
}

// Set up event listeners
function setupEventListeners() {
    // Sidebar toggle
    $('#sidebarCollapse').on('click', function () {
        $('#sidebar').toggleClass('active');
    });

    // Notification dropdown
    $('.dropdown-toggle').dropdown();

    // Date range picker for revenue chart
    $('#dateRange').daterangepicker({
        startDate: moment().subtract(30, 'days'),
        endDate: moment(),
        ranges: {
            'Today': [moment(), moment()],
            'Yesterday': [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
            'Last 7 Days': [moment().subtract(6, 'days'), moment()],
            'Last 30 Days': [moment().subtract(29, 'days'), moment()],
            'This Month': [moment().startOf('month'), moment().endOf('month')],
            'Last Month': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')]
        }
    }, function (start, end) {
        updateRevenueChart(start, end);
    });
}

// Start periodic updates
function startPeriodicUpdates() {
    updateInterval = setInterval(function () {
        updateActiveCargos();
        updateActiveDrivers();
    }, 30000); // Update every 30 seconds
}

// Update active cargos
function updateActiveCargos() {
    showLoading();
    $.get('/Dashboard/GetActiveCargos', function (data) {
        updateCargosTable(data);
        updateCargoMarkers(data);
        hideLoading();
    }).fail(function (error) {
        console.error('Error updating active cargos:', error);
        hideLoading();
    });
}

// Update active drivers
function updateActiveDrivers() {
    showLoading();
    $.get('/Dashboard/GetActiveDrivers', function (data) {
        updateDriversTable(data);
        updateDriverMarkers(data);
        hideLoading();
    }).fail(function (error) {
        console.error('Error updating active drivers:', error);
        hideLoading();
    });
}

// Update cargo markers on the map
function updateCargoMarkers(cargos) {
    cargos.forEach(function (cargo) {
        if (markers[cargo.id]) {
            markers[cargo.id].setLatLng([cargo.currentLatitude, cargo.currentLongitude]);
        } else {
            markers[cargo.id] = L.marker([cargo.currentLatitude, cargo.currentLongitude])
                .bindPopup(createCargoPopup(cargo))
                .addTo(map);
        }
    });
}

// Update driver markers on the map
function updateDriverMarkers(drivers) {
    drivers.forEach(function (driver) {
        if (markers[driver.id]) {
            markers[driver.id].setLatLng([driver.currentLatitude, driver.currentLongitude]);
        } else {
            markers[driver.id] = L.marker([driver.currentLatitude, driver.currentLongitude])
                .bindPopup(createDriverPopup(driver))
                .addTo(map);
        }
    });
}

// Create cargo popup content
function createCargoPopup(cargo) {
    return `
        <div class="popup-content">
            <h6>${cargo.title}</h6>
            <p><strong>Driver:</strong> ${cargo.driverName}</p>
            <p><strong>Status:</strong> ${cargo.status}</p>
            <p><strong>Location:</strong> ${cargo.currentLocation}</p>
            <p><strong>Distance:</strong> ${cargo.distanceTraveled.toFixed(2)} km</p>
            <p><strong>Remaining:</strong> ${cargo.remainingDistance.toFixed(2)} km</p>
            <p><strong>ETA:</strong> ${formatTimeSpan(cargo.estimatedTimeRemaining)}</p>
        </div>
    `;
}

// Create driver popup content
function createDriverPopup(driver) {
    return `
        <div class="popup-content">
            <h6>${driver.fullName}</h6>
            <p><strong>Vehicle:</strong> ${driver.vehicleNumber}</p>
            <p><strong>Status:</strong> ${driver.status}</p>
            <p><strong>Location:</strong> ${driver.currentLocation}</p>
            <p><strong>Speed:</strong> ${driver.currentSpeed} km/h</p>
            <p><strong>Rating:</strong> ${driver.rating.toFixed(1)}</p>
            <p><strong>Deliveries:</strong> ${driver.successfulDeliveries}/${driver.totalDeliveries}</p>
        </div>
    `;
}

// Update cargo table
function updateCargosTable(cargos) {
    var tableBody = $('#activeCargosTable');
    tableBody.empty();
    
    cargos.forEach(function (cargo) {
        tableBody.append(`
            <tr>
                <td>${cargo.title}</td>
                <td>${cargo.driverName}</td>
                <td><span class="badge bg-${getStatusColor(cargo.status)}">${cargo.status}</span></td>
                <td>
                    <button class="btn btn-sm btn-primary" onclick="trackCargo('${cargo.id}')">
                        <i class="fas fa-map-marker-alt"></i>
                    </button>
                </td>
            </tr>
        `);
    });
}

// Update driver table
function updateDriversTable(drivers) {
    var tableBody = $('#activeDriversTable');
    tableBody.empty();
    
    drivers.forEach(function (driver) {
        tableBody.append(`
            <tr>
                <td>${driver.fullName}</td>
                <td><span class="badge bg-${getStatusColor(driver.status)}">${driver.status}</span></td>
                <td>
                    <button class="btn btn-sm btn-primary" onclick="trackDriver('${driver.id}')">
                        <i class="fas fa-map-marker-alt"></i>
                    </button>
                </td>
            </tr>
        `);
    });
}

// Track specific cargo
function trackCargo(cargoId) {
    $.get('/Dashboard/GetCargoLocation', { cargoId: cargoId }, function (data) {
        map.setView([data.latitude, data.longitude], 13);
        if (markers[cargoId]) {
            markers[cargoId].openPopup();
        }
    });
}

// Track specific driver
function trackDriver(driverId) {
    $.get('/Dashboard/GetDriverLocation', { driverId: driverId }, function (data) {
        map.setView([data.latitude, data.longitude], 13);
        if (markers[driverId]) {
            markers[driverId].openPopup();
        }
    });
}

// Update revenue chart
function updateRevenueChart(startDate, endDate) {
    $.get('/Dashboard/GetRevenueData', {
        startDate: startDate.toISOString(),
        endDate: endDate.toISOString()
    }, function (data) {
        revenueChart.data.labels = data.map(d => new Date(d.date).toLocaleDateString());
        revenueChart.data.datasets[0].data = data.map(d => d.revenue);
        revenueChart.update();
    });
}

// Helper function to get status color
function getStatusColor(status) {
    switch (status.toLowerCase()) {
        case 'active': return 'success';
        case 'pending': return 'warning';
        case 'completed': return 'info';
        case 'cancelled': return 'danger';
        default: return 'secondary';
    }
}

// Helper function to format TimeSpan
function formatTimeSpan(timeSpan) {
    const hours = Math.floor(timeSpan.totalHours);
    const minutes = timeSpan.minutes;
    return `${hours}h ${minutes}m`;
}

// Show loading spinner
function showLoading() {
    if (!$('.spinner-overlay').length) {
        $('body').append(`
            <div class="spinner-overlay">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
            </div>
        `);
    }
}

// Hide loading spinner
function hideLoading() {
    $('.spinner-overlay').remove();
}

// Clean up on page unload
$(window).on('unload', function () {
    if (updateInterval) {
        clearInterval(updateInterval);
    }
});
