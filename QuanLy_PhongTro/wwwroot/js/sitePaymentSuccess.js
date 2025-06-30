// Gọi hàm hiển thị thông báo khi trang được tải
document.addEventListener('DOMContentLoaded', function () {
    handleNotification();
});

// Hàm showToast
function showToast(message, type = 'success') {
    console.log('Showing toast:', message, type); // Debug
    const toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        console.error('Toast container not found!');
        return;
    }
    const toast = document.createElement('div');
    toast.classList.add('toast');
    toast.classList.add(type === 'success' ? 'toast-success' : 'toast-error');
    toast.innerHTML = `
                <span class="toast-icon">${type === 'success' ? '✔' : '✖'}</span>
                <span class="toast-message">${message}</span>
                <span class="toast-close">×</span>
            `;
    toastContainer.appendChild(toast);

    setTimeout(() => {
        toast.classList.add('hide');
        setTimeout(() => toast.remove(), 500);
    }, 3000);

    toast.querySelector('.toast-close').addEventListener('click', () => {
        toast.classList.add('hide');
        setTimeout(() => toast.remove(), 500);
    });
}

// Hàm hiển thị thông báo
function handleNotification() {
    const { successMessage, errorMessage } = window.tempData;

    if (successMessage && successMessage !== 'null' && successMessage !== '""') {
        showToast(successMessage, 'success');
        fetch('/PayBill/ClearTempData', { method: 'POST' })
            .then(response => console.log('TempData cleared:', response))
            .catch(err => console.error('Error clearing TempData:', err));
    } else if (errorMessage && errorMessage !== 'null' && errorMessage !== '""') {
        showToast(errorMessage, 'error');
        fetch('/PayBill/ClearTempData', { method: 'POST' })
            .then(response => console.log('TempData cleared:', response))
            .catch(err => console.error('Error clearing TempData:', err));
    }
}

