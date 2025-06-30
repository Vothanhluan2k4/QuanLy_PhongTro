document.addEventListener('DOMContentLoaded', function () {
    const forgotPasswordForm = document.getElementById('forgotPasswordForm');
    const submitButton = document.getElementById('submitButton');
    const emailInput = document.getElementById('email');
    const emailError = document.getElementById('emailError');
    const successMessage = document.getElementById('successMessage');
    const errorMessage = document.getElementById('errorMessage');

    // Kiểm tra xem các phần tử có tồn tại không
    if (!forgotPasswordForm || !emailInput || !submitButton) {
        console.error("Các phần tử form không được tìm thấy");
        return;
    }

    // Hàm kiểm tra email hợp lệ
    function isValidEmail(email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    }

    // Xử lý sự kiện submit form
    forgotPasswordForm.addEventListener('submit', async function (event) {
        event.preventDefault();

        // Reset các thông báo lỗi
        emailError.textContent = '';
        successMessage.style.display = 'none';
        errorMessage.style.display = 'none';
        emailInput.classList.remove('input-error');

        // Validate email
        const emailValue = emailInput.value.trim();
        let isValid = true;

        if (!emailValue) {
            emailError.textContent = 'Vui lòng nhập email';
            emailInput.classList.add('input-error');
            isValid = false;
        } else if (!isValidEmail(emailValue)) {
            emailError.textContent = 'Email không hợp lệ';
            emailInput.classList.add('input-error');
            isValid = false;
        }

        if (!isValid) return;

        // Disable nút submit để tránh gửi nhiều lần
        submitButton.disabled = true;
        submitButton.textContent = 'Đang xử lý...';

        try {
            // Gửi request đến server
            const response = await fetch('/QuenMatKhau/GuiOTP', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                },
                body: `email=${encodeURIComponent(emailValue)}`
            });

            const result = await response.json();

            if (result.success) {
                // Hiển thị thông báo thành công
                successMessage.textContent = result.message;
                successMessage.style.display = 'block';

                // Chuyển hướng sau 2 giây
                setTimeout(() => {
                    window.location.href = '/QuenMatKhau/XacNhanOTP';
                }, 2000);
            } else {
                // Hiển thị thông báo lỗi
                errorMessage.textContent = result.message;
                errorMessage.style.display = 'block';
            }
        } catch (error) {
            // Xử lý lỗi kết nối
            console.error('Lỗi khi gửi yêu cầu:', error);
            errorMessage.textContent = 'Lỗi kết nối đến máy chủ';
            errorMessage.style.display = 'block';
        } finally {
            // Kích hoạt lại nút submit
            submitButton.disabled = false;
            submitButton.textContent = 'Gửi mã OTP';
        }
    });

    // Tự động ẩn thông báo sau 5 giây
    const autoHideMessages = () => {
        const messages = document.querySelectorAll('.success-message, .error-message');
        messages.forEach(msg => {
            if (msg.style.display === 'block') {
                setTimeout(() => {
                    msg.style.display = 'none';
                }, 5000);
            }
        });
    };

    // Gọi hàm tự động ẩn thông báo
    autoHideMessages();
});