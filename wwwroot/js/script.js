// Form Validation
// function validateForm(formId) {
//     const form = document.getElementById(formId);
//     if (form.checkValidity() === false) {
//         event.preventDefault();
//         event.stopPropagation();
//     }
//     form.classList.add('was-validated');
// }

// Add to Favorites
function addFavorite(propertyId) {
    $.ajax({
        url: '/Customer/AddFavorite',
        type: 'POST',
        data: { propertyId: propertyId },
        success: function (response) {
            if (response.success) {
                showAlert('success', response.message);
                setTimeout(() => location.reload(), 1000);
            } else {
                showAlert('danger', response.message);
            }
        },
        error: function () {
            showAlert('danger', 'Error adding to favorites');
        }
    });
}

// Remove from Favorites
function removeFavorite(propertyId) {
    if (confirm('Are you sure you want to remove this property from favorites?')) {
        $.ajax({
            url: '/Customer/RemoveFavorite',
            type: 'POST',
            data: { propertyId: propertyId },
            success: function (response) {
                if (response.success) {
                    showAlert('success', response.message);
                    setTimeout(() => location.reload(), 1000);
                } else {
                    showAlert('danger', response.message);
                }
            },
            error: function () {
                showAlert('danger', 'Error removing favorite');
            }
        });
    }
}

// Submit Enquiry
function submitEnquiry(propertyId) {
    const message = document.getElementById('enquiryMessage').value;
    
    if (!message || message.length < 5 || message.length > 2000) {
        showAlert('danger', 'Message must be between 5 and 2000 characters');
        return;
    }

    $.ajax({
        url: '/Customer/SubmitEnquiry',
        type: 'POST',
        data: { propertyId: propertyId, message: message },
        success: function (response) {
            if (response.success) {
                showAlert('success', response.message);
                document.getElementById('enquiryMessage').value = '';
                setTimeout(() => location.reload(), 1000);
            } else {
                showAlert('danger', response.message);
            }
        },
        error: function () {
            showAlert('danger', 'Error submitting enquiry');
        }
    });
}

// Schedule Site Visit
function scheduleSiteVisit(propertyId) {
    const visitDate = document.getElementById('visitDate').value;
    const visitTime = document.getElementById('visitTime').value;

    if (!visitDate || !visitTime) {
        showAlert('danger', 'Please select both date and time');
        return;
    }

    $.ajax({
        url: '/Customer/ScheduleSiteVisit',
        type: 'POST',
        data: { propertyId: propertyId, visitDate: visitDate, visitTime: visitTime },
        success: function (response) {
            if (response.success) {
                showAlert('success', response.message);
                document.getElementById('visitDate').value = '';
                document.getElementById('visitTime').value = '';
                setTimeout(() => location.reload(), 1000);
            } else {
                showAlert('danger', response.message);
            }
        },
        error: function () {
            showAlert('danger', 'Error scheduling visit');
        }
    });
}

// Respond to Enquiry
function respondToEnquiry(enquiryId) {
    const response = document.getElementById('responseMessage_' + enquiryId).value;

    if (!response || response.length < 5 || response.length > 2000) {
        showAlert('danger', 'Response must be between 5 and 2000 characters');
        return;
    }

    $.ajax({
        url: '/PropertyOwner/RespondToEnquiry',
        type: 'POST',
        data: { enquiryId: enquiryId, response: response },
        success: function (response) {
            if (response.success) {
                showAlert('success', response.message);
                setTimeout(() => location.reload(), 1000);
            } else {
                showAlert('danger', response.message);
            }
        },
        error: function () {
            showAlert('danger', 'Error sending response');
        }
    });
}

// Update Visit Status
function updateVisitStatus(visitId, newStatus) {
    $.ajax({
        url: '/Broker/UpdateVisitStatus',
        type: 'POST',
        data: { visitId: visitId, status: newStatus },
        success: function (response) {
            if (response.success) {
                showAlert('success', response.message);
                setTimeout(() => location.reload(), 1000);
            } else {
                showAlert('danger', response.message);
            }
        },
        error: function () {
            showAlert('danger', 'Error updating visit status');
        }
    });
}

// Toggle User Status (Admin)
function toggleUserStatus(userId) {
    if (confirm('Are you sure you want to change this user\'s status?')) {
        $.ajax({
            url: '/Admin/ToggleUserStatus',
            type: 'POST',
            data: { userId: userId },
            success: function (response) {
                if (response.success) {
                    showAlert('success', response.message);
                    setTimeout(() => location.reload(), 1000);
                } else {
                    showAlert('danger', response.message);
                }
            },
            error: function () {
                showAlert('danger', 'Error updating user status');
            }
        });
    }
}

// Approve Property (Admin)
function approveProperty(propertyId) {
    if (confirm('Are you sure you want to approve this property?')) {
        $.ajax({
            url: '/Admin/ApprovePropertyConfirm',
            type: 'POST',
            data: { propertyId: propertyId },
            success: function (response) {
                if (response.success) {
                    showAlert('success', response.message);
                    setTimeout(() => location.reload(), 1000);
                } else {
                    showAlert('danger', response.message);
                }
            },
            error: function () {
                showAlert('danger', 'Error approving property');
            }
        });
    }
}

// Reject Property (Admin)
function rejectProperty(propertyId) {
    const reason = prompt('Please enter rejection reason:');
    
    if (reason !== null && reason.trim() !== '') {
        $.ajax({
            url: '/Admin/RejectProperty',
            type: 'POST',
            data: { propertyId: propertyId, reason: reason },
            success: function (response) {
                if (response.success) {
                    showAlert('success', response.message);
                    setTimeout(() => location.reload(), 1000);
                } else {
                    showAlert('danger', response.message);
                }
            },
            error: function () {
                showAlert('danger', 'Error rejecting property');
            }
        });
    }
}

// Show Alert Message
function showAlert(type, message) {
    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            <i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-circle'}"></i> ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    
    const alertContainer = document.createElement('div');
    alertContainer.innerHTML = alertHtml;
    alertContainer.className = 'position-fixed top-0 start-50 translate-middle-x mt-3';
    alertContainer.style.zIndex = '9999';
    document.body.appendChild(alertContainer);

    setTimeout(() => alertContainer.remove(), 5000);
}

// Logout Confirmation
function confirmLogout() {
    return confirm('Are you sure you want to logout?');
}

// Initialize tooltips
document.addEventListener('DOMContentLoaded', function () {
    // Bootstrap tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});

// Format currency
function formatCurrency(value) {
    return new Intl.NumberFormat('en-IN', {
        style: 'currency',
        currency: 'INR'
    }).format(value);
}

// Delete Confirmation
function confirmDelete(message = 'Are you sure you want to delete this item?') {
    return confirm(message);
}