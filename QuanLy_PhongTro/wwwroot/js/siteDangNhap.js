document.addEventListener('DOMContentLoaded', function () {
    // Xử lý hiển thị/ẩn mật khẩu
    const togglePassword = document.getElementById('togglePassword');
    const passwordInput = document.getElementById('password');

    togglePassword.addEventListener('click', function () {
        const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
        passwordInput.setAttribute('type', type);

        // Thay đổi icon
        this.querySelector('i').classList.toggle('fa-eye');
        this.querySelector('i').classList.toggle('fa-eye-slash');
    });

    const loginForm = document.getElementById('loginForm');
    const loginButton = document.getElementById('loginButton');
    const emailInput = document.getElementById('username');
    const emailError = document.getElementById('emailError');
    const passwordError = document.getElementById('passwordError');
    const captchaError = document.getElementById('captchaError');
    const googleLogin = document.getElementById('googleLogin');

    function showToast(message, type = 'success') {
        const toastContainer = document.getElementById('toast-container');
        if (!toastContainer) {
            console.error('Toast container not found!');
            return;
        }

        const toast = document.createElement('div');
        toast.classList.add('toast');
        toast.classList.add(type === 'success' ? 'toast-success' : 'toast-error');
        toast.innerHTML = `
        <span class="toast-icon fas fa-${type === 'success' ? 'check-circle' : 'times-circle'}"></span>
        <span class="toast-message">${message}</span>
        <span class="toast-close">${document.fonts.check('16px "Font Awesome 6 Free"') ? '<i class="fas fa-times"></i>' : 'x'}</span>
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



    loginForm.addEventListener('submit', async function (event) {
        event.preventDefault();
        emailError.textContent = '';
        passwordError.textContent = '';
        emailInput.classList.remove('input-error');
        passwordInput.classList.remove('input-error');

        let isValid = true;

        // Validate email
        if (!emailInput.value.trim()) {
            emailError.textContent = 'Vui lòng nhập email';
            emailInput.classList.add('input-error');
            isValid = false;
        } else if (!isValidEmail(emailInput.value.trim())) {
            emailError.textContent = 'Email không hợp lệ';
            emailInput.classList.add('input-error');
            isValid = false;
        }

        // Validate mật khẩu
        if (!passwordInput.value.trim()) {
            passwordError.textContent = 'Vui lòng nhập mật khẩu';
            passwordInput.classList.add('input-error');
            isValid = false;
        }
        const captchaResponse = grecaptcha.getResponse();
        console.log("CAPTCHA Token before submit:", captchaResponse);
        if (!captchaResponse) {
            captchaError.textContent = 'Vui lòng xác nhận bạn không phải là robot.';
            isValid = false;
        }

        if (!isValid) return;

        loginButton.disabled = true;
        loginButton.textContent = 'Đang xử lý...';

        const urlParams = new URLSearchParams(window.location.search);
        const returnUrl = urlParams.get('returnUrl') ||
            '@Url.Action("ChiTietPhong", "PhongTro", new { id = Model.MaPhong })';

        try {
            const response = await fetch('/DangNhap/XuLyDangNhap', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    Email: emailInput.value.trim(),
                    Password: passwordInput.value.trim(),
                    ReturnUrl: returnUrl, // Gửi returnUrl chính xác
                    Captcha: captchaResponse
                })
            });

            const result = await response.json();
            if (result.success) {
                showToast(result.message, 'success');
                setTimeout(() => {
                    window.location.href = result.redirectUrl;
                }, 1000);
            } else {
                showToast(result.message, 'error');
                if (result.message.includes('Email')) {
                    emailError.textContent = result.message;
                    emailInput.classList.add('input-error');
                } else if (result.message.includes('mật khẩu')) {
                    passwordError.textContent = result.message;
                    passwordInput.classList.add('input-error');
                } else if (result.message.includes('captcha')) {
                    captchaError.textContent = 'Xác nhận CAPTCHA thất bại. Vui lòng thử lại.';
                    grecaptcha.reset();
                    console.log("CAPTCHA reset triggered. New token after reset:", grecaptcha.getResponse());
                } else {
                    passwordError.textContent = result.message;
                    passwordInput.classList.add('input-error');
                }
            }
        } catch (error) {
            showToast('Lỗi kết nối đến máy chủ', 'error');
            console.error('Fetch error:', error);
        } finally {
            loginButton.disabled = false;
            loginButton.textContent = 'Đăng nhập';
        }
    });

    // Hàm kiểm tra email hợp lệ
    function isValidEmail(email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    }
    
});