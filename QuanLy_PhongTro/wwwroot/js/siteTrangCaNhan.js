window.redirectToPayment = function (maHoaDon) {
    console.log('Redirecting to payment for MaHoaDon:', maHoaDon);
    window.location.href = `/PayBill/PayBill?maHoaDon=${encodeURIComponent(maHoaDon)}`;
};
window.redirectToPaymentHD = function (maHopDong) {
    console.log('Redirecting to payment for MaHopDong:', maHopDong);
    window.location.href = `/PayContract/PayContract?maHopDong=${encodeURIComponent(maHopDong)}`;
};
document.addEventListener('DOMContentLoaded', function () {
    // Lấy tất cả các mục menu
    const menuItems = document.querySelectorAll('.sub-menu-item');
    const contentSections = document.querySelectorAll('.content-section');
    // --- Xử lý hóa đơn ---
    const searchHoadonInput = document.getElementById('search-hoadon-input');
    const searchHoadonBtn = document.getElementById('search-hoadon-btn');
    const resetHoadonSearchBtn = document.getElementById('reset-hoadon-search-btn');
    const fromMonthYearInput = document.getElementById('from-month-year');
    const toMonthYearInput = document.getElementById('to-month-year');
    const searchByMonthYearBtn = document.getElementById('search-by-month-year-btn');
    const resetMonthYearSearchBtn = document.getElementById('reset-month-year-search-btn');
    const filterHoadonStatus = document.getElementById('filter-hoadon-status');
    const hoadonListBody = document.getElementById('hoadon-list-body');
    const issueForm = document.getElementById('issue-report-form');
    if (issueForm) {
        const issueTypeInput = document.getElementById('issueType');
        const dataList = document.getElementById('issueTypeList');
        const mediaInput = document.getElementById('media');
        const mediaPreview = document.getElementById('media-preview');
        let selectedIssueTypeValue = '';
        let filesArray = []; // Lưu trữ danh sách file

        // Thêm validation cho file size (40MB limit)
        const MAX_FILE_SIZE = 40 * 1024 * 1024; // 40MB in bytes

        mediaInput.addEventListener('change', function () {
            const files = Array.from(this.files);
            
            // Kiểm tra kích thước file
            for (const file of files) {
                if (file.size > MAX_FILE_SIZE) {
                    const fileSizeMB = (file.size / (1024 * 1024)).toFixed(2);
                    showToast(`File '${file.name}' có kích thước ${fileSizeMB}MB vượt quá giới hạn 40MB. Vui lòng chọn file nhỏ hơn.`, 'error');
                    this.value = ''; // Clear input
                    return;
                }
            }
            
            files.forEach(file => {
                if (!filesArray.some(existingFile => existingFile.name === file.name && existingFile.size === file.size)) {
                    filesArray.push(file);
                    displayFile(file);
                }
            });

            // Cập nhật input file với danh sách file hiện tại
            updateFileInput();
        });

        // Hàm hiển thị file preview
        function displayFile(file) {
            const reader = new FileReader();
            reader.onload = function (e) {
                const previewItem = document.createElement('div');
                previewItem.classList.add('media-preview-item');

                if (file.type.startsWith('image/')) {
                    const img = document.createElement('img');
                    img.src = e.target.result;
                    previewItem.appendChild(img);
                } else if (file.type.startsWith('video/')) {
                    const video = document.createElement('video');
                    video.src = e.target.result;
                    video.controls = true;
                    previewItem.appendChild(video);
                }

                const removeBtn = document.createElement('button');
                removeBtn.classList.add('remove-btn');
                removeBtn.textContent = 'X';
                removeBtn.addEventListener('click', () => {
                    filesArray = filesArray.filter(f => f !== file);
                    previewItem.remove();
                    updateFileInput();
                });
                previewItem.appendChild(removeBtn);

                mediaPreview.appendChild(previewItem);
            };
            reader.readAsDataURL(file);
        }

        // Hàm cập nhật input file
        function updateFileInput() {
            const dataTransfer = new DataTransfer();
            filesArray.forEach(file => dataTransfer.items.add(file));
            mediaInput.files = dataTransfer.files;
        }

        // Handle LoaiSuCo label-to-value mapping
        issueTypeInput.addEventListener('input', function () {
            const selectedValue = this.value.trim();
            const options = dataList.options;

            let matchedOption = null;
            for (let option of options) {
                if (option.value === selectedValue || option.textContent === selectedValue) {
                    matchedOption = option;
                    break;
                }
            }

            if (matchedOption) {
                selectedIssueTypeValue = matchedOption.value;
            } else {
                selectedIssueTypeValue = selectedValue;
            }

            console.log('Giá trị nhập hiện tại:', selectedValue);
            console.log('Giá trị selectedIssueTypeValue:', selectedIssueTypeValue);
        });

        issueTypeInput.addEventListener('change', function () {
            const selectedValue = this.value.trim();
            const options = dataList.options;

            let matchedOption = null;
            for (let option of options) {
                if (option.value === selectedValue || option.textContent === selectedValue) {
                    matchedOption = option;
                    break;
                }
            }

            if (matchedOption) {
                selectedIssueTypeValue = matchedOption.value;
            } else {
                selectedIssueTypeValue = selectedValue;
            }

            console.log('Giá trị sau khi change:', selectedValue);
            console.log('Giá trị selectedIssueTypeValue sau khi change:', selectedIssueTypeValue);
        });

        issueForm.addEventListener('submit', function (e) {
            e.preventDefault();

            const MaPhong = document.getElementById('MaPhong').value;
            const description = document.getElementById('moTa').value.trim();
            const phone = document.getElementById('phone').value.trim();
            const issueType = issueTypeInput.value.trim();

            // Validate roomId
            if (!MaPhong || MaPhong <= 0) {
                showToast('error', 'Vui lòng chọn phòng hợp lệ.');
                return;
            }

            // Validate issueType
            if (!issueType) {
                showToast('error', 'Vui lòng chọn loại sự cố.');
                return;
            }

            // Validate description
            if (!description) {
                showToast('error', 'Vui lòng nhập mô tả chi tiết.');
                return;
            }

            // Validate phone
            if (phone && !/^\d{10}$/.test(phone)) {
                showToast('error', 'Số điện thoại phải có 10 chữ số.');
                return;
            }

            const formData = new FormData(issueForm);

            // Xử lý giá trị LoaiSuCo
            let finalIssueType = issueType;
            if (selectedIssueTypeValue === 'Khác' && issueType !== 'Khác') {
                finalIssueType = issueType;
            } else if (selectedIssueTypeValue !== issueType) {
                finalIssueType = issueType;
            } else {
                finalIssueType = selectedIssueTypeValue;
            }

            formData.set('LoaiSuCo', finalIssueType);

            fetch(issueForm.getAttribute('action'), {
                method: 'POST',
                body: formData
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        showToast(data.message, 'success');
                        issueForm.reset();
                        mediaPreview.innerHTML = ''; // Xóa preview sau khi gửi thành công
                        filesArray = []; // Reset danh sách file
                        selectedIssueTypeValue = ''; // Reset giá trị LoaiSuCo
                        const phoneInput = document.getElementById('phone');
                        phoneInput.setAttribute('readonly', 'readonly');
                        phoneInput.classList.remove('editable');
                    } else {
                        showToast(data.message, 'error');
                    }
                })
                .catch(error => showToast('error', 'Có lỗi xảy ra khi gửi báo cáo: ' + error.message));
        });
    }

    function showToast(message, type = 'success') {
        const toastContainer = document.getElementById('toast-container');
        if (!toastContainer) {
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
    let hasShownNotification = false;
    function handleNotification() {
        if (hasShownNotification) {
            return;
        }
        const { successMessage, errorMessage } = window.tempData;

        if (successMessage && successMessage !== 'null' && successMessage !== '') {
            showToast(successMessage, 'success');
            hasShownNotification = true;
        } else if (errorMessage && errorMessage !== 'null' && errorMessage !== '') {
            showToast(errorMessage, 'error');
            hasShownNotification = true;
        }
    }
    let currentTab = null;
    // Hàm để hiển thị tab
    function showTab(target) {

        if (currentTab === target) {
            return; // Bỏ qua nếu tab không đổi
        }

        currentTab = target;
        // Ẩn tất cả các tab
        contentSections.forEach(section => {
            section.classList.remove('active');
        });

        // Ẩn tất cả trạng thái active của menu
        menuItems.forEach(item => {
            item.classList.remove('active');
        });

        // Hiển thị tab tương ứng
        const targetSection = document.getElementById(target);
        if (targetSection) {
            targetSection.classList.add('active');
        }

        // Cập nhật trạng thái active cho menu
        const targetMenuItem = document.querySelector(`.sub-menu-item[data-target="${target}"]`);
        if (targetMenuItem) {
            targetMenuItem.classList.add('active');
        }

        // Tải dữ liệu tương ứng
        if (target === 'balance-info') {
            loadTransactions();
        } else if (target === 'room-book') {
            loadRooms();
        } else if (target === 'days-receipt') {
            handleNotification();
            loadHoaDons();
        } else if (target === 'deposit-history') {
            const ipMoneyInput = document.getElementById('ip_money');
            const soTienConThieu = sessionStorage.getItem('soTienConThieu');
            if (ipMoneyInput && soTienConThieu && soTienConThieu !== '0') {
                const soTien = parseFloat(soTienConThieu);
                ipMoneyInput.value = new Intl.NumberFormat('vi-VN').format(soTien);

                sessionStorage.removeItem('soTienConThieu');
            }
            handleNotification();
        } else if (target === 'profile') {
            handleNotification(); 
        }
    }
    

    // Kiểm tra tab mặc định từ window.ActiveTab
    const activeTab = window.ActiveTab || 'profile';
    showTab(activeTab);

    // Thêm sự kiện click cho từng mục menu
    menuItems.forEach(item => {
        item.addEventListener('click', function () {
            const target = this.getAttribute('data-target');
            showTab(target);
        });
    });


    // Hàm cho phép chỉnh sửa từng trường
    window.enableEdit = function (fieldId) {
        const input = document.getElementById(fieldId);
        input.removeAttribute('readonly');
        input.focus();
        input.style.backgroundColor = '#fff';
        input.style.cursor = 'text';
    };

    window.saveProfile = async function () {
        const hoTen = document.getElementById('display-name').value.trim();
        const email = document.getElementById('email').value.trim();
        const soDienThoai = document.getElementById('phone').value.trim();

        try {
            const response = await fetch('/TrangCaNhan/UpdateProfile', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    displayName: hoTen,
                    email: email,
                    phone: soDienThoai
                })
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText || 'Lỗi khi gửi yêu cầu');
            }

            const result = await response.json();                   
            showToast(result.message, result.success ? 'success' : 'error');

            if (result.success) {
                // Cập nhật tên trong layout
                if (result.displayName) {
                    const userNameElement = document.querySelector('.user-name');
                    if (userNameElement) {
                        userNameElement.textContent = result.displayName;
                    }
                    const accountNameElement = document.querySelector('.account-name');
                    if (accountNameElement) {
                        accountNameElement.textContent = result.displayName;
                    }
                }

                // Cập nhật avatar trong layout (nếu có thay đổi)
                if (result.avatarUrl) {
                    const userAvatarElement = document.querySelector('.user-avatar');
                    if (userAvatarElement) {
                        userAvatarElement.src = result.avatarUrl;
                    }
                }

                // Reset trạng thái các trường
                document.getElementById('email').setAttribute('readonly', true);
                document.getElementById('email').style.backgroundColor = '#f9f9f9';
                document.getElementById('email').style.cursor = 'not-allowed';

                document.getElementById('phone').setAttribute('readonly', true);
                document.getElementById('phone').style.backgroundColor = '#f9f9f9';
                document.getElementById('phone').style.cursor = 'not-allowed';
            }
        } catch (error) {
            alert(`Có lỗi xảy ra: ${error.message}`);
        }
    };
    // Xử lý tải lên avatar
    document.querySelector('.upload-btn').addEventListener('click', function () {
        const input = document.createElement('input');
        input.type = 'file';
        input.accept = 'image/*';

        input.onchange = async function (e) {
            const file = e.target.files[0];
            if (!file) return;

            const formData = new FormData();
            formData.append('avatarFile', file);

            try {
                const response = await fetch('/TrangCaNhan/CapNhatAvatar', {
                    method: 'POST',
                    body: formData
                });

                const result = await response.json();
                showToast(result.message, result.success ? 'success' : 'error');
                if (result.success) {
                    // Cập nhật avatar trong trang cá nhân
                    const avatarPreview = document.querySelector('.avatar-preview img');
                    if (avatarPreview) {
                        avatarPreview.src = result.avatarUrl;
                    }

                    // Cập nhật avatar trong layout
                    const userAvatarElement = document.querySelector('.user-avatar');
                    if (userAvatarElement) {
                        userAvatarElement.src = result.avatarUrl;
                    }

                    // Cập nhật tên trong layout (nếu có thay đổi)
                    if (result.displayName) {
                        const userNameElement = document.querySelector('.user-name');
                        if (userNameElement) {
                            userNameElement.textContent = result.displayName;
                        }
                    }
                }
            } catch (error) {
                alert('Có lỗi xảy ra khi tải lên avatar');
            }
        };

        input.click();
    });

    // Xử lý nút tìm kiếm giao dịch
    const searchButton = document.querySelector('.search-button');
    if (searchButton) {
        searchButton.addEventListener('click', searchTransactions);
    }

    const transactionSearchInput = document.getElementById('transaction-search');
    if (transactionSearchInput) {
        transactionSearchInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                searchTransactions();
            }
        });
        // Hiển thị/ẩn nút X dựa trên nội dung ô tìm kiếm
        transactionSearchInput.addEventListener('input', function () {
            const clearButton = this.parentElement.querySelector('.clear-transaction-search');
            if (this.value.trim()) {
                clearButton.style.display = 'inline-block';
            } else {
                clearButton.style.display = 'none';
            }
        });
    }
    // Xử lý nút xóa tìm kiếm giao dịch
    window.clearTransactionSearch = function () {
        const searchInput = document.getElementById('transaction-search');
        const clearButton = searchInput.parentElement.querySelector('.clear-transaction-search');
        searchInput.value = '';
        clearButton.style.display = 'none';
        loadTransactions(); // Tải lại toàn bộ danh sách giao dịch
    };


    // Xử lý nút tìm kiếm phòng
    const roomSearchButton = document.querySelector('.room-search-button');
    if (roomSearchButton) {
        roomSearchButton.addEventListener('click', searchRooms);
    }
    const roomSearchInput = document.querySelector('.room-search-input');
    if (roomSearchInput) {
        roomSearchInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                searchRooms();
            }
        });
        // Hiển thị/ẩn nút X dựa trên nội dung ô tìm kiếm
        roomSearchInput.addEventListener('input', function () {
            const clearButton = this.parentElement.querySelector('.clear-room-search');
            if (this.value.trim()) {
                clearButton.style.display = 'inline-block';
            } else {
                clearButton.style.display = 'none';
            }
        });
    }

    // Xử lý nút xóa tìm kiếm phòng
    window.clearRoomSearch = function () {
        const searchInput = document.querySelector('.room-search-input');
        const clearButton = searchInput.parentElement.querySelector('.clear-room-search');
        searchInput.value = '';
        clearButton.style.display = 'none';
        loadRooms(); // Tải lại toàn bộ danh sách phòng
    };

    //Hóa Đơn
    if (searchHoadonBtn) {
        searchHoadonBtn.addEventListener('click', function () {
            const searchTerm = searchHoadonInput.value.trim();
            const fromMonthYear = fromMonthYearInput.value;
            const toMonthYear = toMonthYearInput.value;
            let fromMonth = '', fromYear = '', toMonth = '', toYear = '';
            if (fromMonthYear && toMonthYear) {
                [fromYear, fromMonth] = fromMonthYear.split('-');
                [toYear, toMonth] = toMonthYear.split('-');
            }
            loadHoaDons(searchTerm, fromMonth, fromYear, toMonth, toYear, filterHoadonStatus.value);
            resetHoadonSearchBtn.style.display = searchTerm ? 'inline-block' : 'none';
        });
    }

    if (searchHoadonInput) {
        searchHoadonInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                const searchTerm = searchHoadonInput.value.trim();
                const fromMonthYear = fromMonthYearInput.value;
                const toMonthYear = toMonthYearInput.value;
                let fromMonth = '', fromYear = '', toMonth = '', toYear = '';
                if (fromMonthYear && toMonthYear) {
                    [fromYear, fromMonth] = fromMonthYear.split('-');
                    [toYear, toMonth] = toMonthYear.split('-');
                }
                loadHoaDons(searchTerm, fromMonth, fromYear, toMonth, toYear, filterHoadonStatus.value);
                resetHoadonSearchBtn.style.display = searchTerm ? 'inline-block' : 'none';
            }
        });
    }

    if (resetHoadonSearchBtn) {
        resetHoadonSearchBtn.addEventListener('click', function () {
            searchHoadonInput.value = '';
            const fromMonthYear = fromMonthYearInput.value;
            const toMonthYear = toMonthYearInput.value;
            let fromMonth = '', fromYear = '', toMonth = '', toYear = '';
            if (fromMonthYear && toMonthYear) {
                [fromYear, fromMonth] = fromMonthYear.split('-');
                [toYear, toMonth] = toMonthYear.split('-');
            }
            loadHoaDons('', fromMonth, fromYear, toMonth, toYear, filterHoadonStatus.value);
            resetHoadonSearchBtn.style.display = 'none';
        });
    }

    // Sự kiện tìm kiếm theo tháng/năm
    if (searchByMonthYearBtn) {
        searchByMonthYearBtn.addEventListener('click', function () {
            const fromMonthYear = fromMonthYearInput.value;
            const toMonthYear = toMonthYearInput.value;
            const searchTerm = searchHoadonInput.value.trim();

            if (fromMonthYear && toMonthYear) {
                const [fromYear, fromMonth] = fromMonthYear.split('-');
                const [toYear, toMonth] = toMonthYear.split('-');
                loadHoaDons(searchTerm, fromMonth, fromYear, toMonth, toYear, filterHoadonStatus.value);
                resetMonthYearSearchBtn.style.display = 'inline-block';
            } else {
                alert('Vui lòng chọn đầy đủ tháng/năm cho cả khoảng thời gian!');
            }
        });
    }

    if (resetMonthYearSearchBtn) {
        resetMonthYearSearchBtn.addEventListener('click', function () {
            fromMonthYearInput.value = '';
            toMonthYearInput.value = '';
            const searchTerm = searchHoadonInput.value.trim();
            loadHoaDons(searchTerm, '', '', '', '', filterHoadonStatus.value);
            resetMonthYearSearchBtn.style.display = 'none';
        });
    }

    // Sự kiện lọc theo trạng thái
    if (filterHoadonStatus) {
        filterHoadonStatus.addEventListener('change', function () {
            const trangThai = this.value;
            const searchTerm = searchHoadonInput.value.trim();
            const fromMonthYear = fromMonthYearInput.value;
            const toMonthYear = toMonthYearInput.value;
            let fromMonth = '', fromYear = '', toMonth = '', toYear = '';
            if (fromMonthYear && toMonthYear) {
                [fromYear, fromMonth] = fromMonthYear.split('-');
                [toYear, toMonth] = toMonthYear.split('-');
            }
            loadHoaDons(searchTerm, fromMonth, fromYear, toMonth, toYear, trangThai);
        });
    }

    //


    // Xử lý hiển thị/ẩn số dư
    const balanceAmount = document.getElementById('balance-amount');
    const toggleBalanceBtn = document.getElementById('toggle-balance-btn');
    let isBalanceHidden = true;

    // Hàm định dạng số dư
    function formatBalance(amount) {
        // Loại bỏ dấu phân cách hàng nghìn (dấu chấm ở định dạng Việt Nam)
        const cleanAmount = amount.replace(/\./g, '');
        // Chuyển thành số và định dạng lại
        if (!cleanAmount || isNaN(cleanAmount)) {
            return '0 VNĐ';
        }
        return Number(cleanAmount).toLocaleString('vi-VN') + ' VNĐ';
    }

    // Hiển thị số dư ngay khi tải trang
    if (balanceAmount && toggleBalanceBtn) {
        const initialAmount = balanceAmount.getAttribute('data-amount');

        // Hiển thị ban đầu (ẩn hoặc hiện tùy thuộc vào isBalanceHidden)
        balanceAmount.textContent = isBalanceHidden ? '****' : formatBalance(initialAmount);
        toggleBalanceBtn.innerHTML = isBalanceHidden ? '<i class="fas fa-eye-slash"></i>' : '<i class="fas fa-eye"></i>';

        toggleBalanceBtn.addEventListener('click', function () {
            isBalanceHidden = !isBalanceHidden;
            const amount = balanceAmount.getAttribute('data-amount');
            balanceAmount.textContent = isBalanceHidden ? '****' : formatBalance(amount);
            toggleBalanceBtn.innerHTML = isBalanceHidden ? '<i class="fas fa-eye-slash"></i>' : '<i class="fas fa-eye"></i>';
        });
    }
    

    // Định dạng số tiền khi nhập
    document.getElementById('ip_money').addEventListener('input', function (e) {
        let value = this.value.replace(/[^\d]/g, '');
        if (value.length > 0) {
            this.value = new Intl.NumberFormat('vi-VN').format(parseInt(value));
            setTimeout(() => {
                this.setSelectionRange(this.value.length, this.value.length);
            }, 0);
        } else {
            this.value = '';
        }
    });

    // Hàm chọn phương thức thanh toán
    function selectPayment(method) {
        const paymentOptions = document.querySelectorAll('.payment-option');
        paymentOptions.forEach(option => option.style.borderColor = '#ddd');
        event.currentTarget.style.borderColor = '#3498db';
        document.getElementById('paymentMethod').value = method; // Lưu phương thức thanh toán vào input ẩn
    }

    // Thêm sự kiện click cho các phương thức thanh toán
    document.querySelectorAll('.payment-option').forEach(option => {
        option.addEventListener('click', function () {
            const method = this.getAttribute('value');
            selectPayment(method);
        });
    });
    // Xử lý nút xác nhận nạp tiền
    const submitMoneyButton = document.getElementById('submitMoney');
    if (submitMoneyButton) {
        submitMoneyButton.addEventListener('click', function (e) {
            e.preventDefault(); // Ngăn form submit mặc định

            const amountInput = document.getElementById('ip_money');
            const amount = amountInput.value.replace(/[^0-9]/g, '');
            const paymentMethod = document.getElementById('paymentMethod').value;
            const amountHiddenInput = document.getElementById('amountInput');
            const form = document.getElementById('deposit-form');
            const returnToUrl = sessionStorage.getItem('returnToBillPayment');


            // Kiểm tra giá trị trước khi submit
            if (!amount || isNaN(amount) || amount <= 0) {
                console.log('Lỗi: Số tiền không hợp lệ', { amount });
                alert('Vui lòng nhập số tiền hợp lệ');
                return;
            }

            if (!paymentMethod) {
                console.log('Lỗi: Chưa chọn phương thức thanh toán', { paymentMethod });
                alert('Vui lòng chọn một phương thức thanh toán');
                return;
            }

            // Kiểm tra form
            if (!form) {
                console.error('Lỗi: Không tìm thấy form với id="deposit-form"');
                alert('Lỗi hệ thống: Không tìm thấy form nạp tiền.');
                return;
            }

            // Gán giá trị amount vào input ẩn
            if (amountHiddenInput) {
                amountHiddenInput.value = amount;
            } else {
                console.error('Lỗi: Không tìm thấy amountInput');
                return;
            }

            // Thêm returnToUrl vào form thông qua một input ẩn
            let returnToUrlInput = form.querySelector('input[name="returnToUrl"]');
            if (!returnToUrlInput) {
                returnToUrlInput = document.createElement('input');
                returnToUrlInput.type = 'hidden';
                returnToUrlInput.name = 'returnToUrl';
                form.appendChild(returnToUrlInput);
            }
            returnToUrlInput.value = returnToUrl || '';
            // Log toàn bộ dữ liệu của form trước khi submit
            const formData = new FormData(form);
            form.submit();
        });
    }
    //Hóa đơn

    async function loadHoaDons(searchTerm = '', fromMonth = '', fromYear = '', toMonth = '', toYear = '', trangThai = '') {
        try {
            let url = `/TrangCaNhan/GetHoaDons?searchTerm=${encodeURIComponent(searchTerm)}`;
            if (fromMonth && fromYear && toMonth && toYear) {
                url += `&fromMonth=${fromMonth}&fromYear=${fromYear}&toMonth=${toMonth}&toYear=${toYear}`;
            }
            if (trangThai) {
                url += `&trangThai=${encodeURIComponent(trangThai)}`;
            }

            const response = await fetch(url);
            console.log('Response status:', response.status); // Thêm log để kiểm tra status
            if (!response.ok) {
                throw new Error(`Lỗi HTTP: ${response.status} - ${response.statusText}`);
            }

            const result = await response.json();

            if (!result.success) {
                throw new Error(result.message || 'Lỗi từ API');
            }

            const hoadonListBody = document.getElementById('hoadon-list-body');
            if (!hoadonListBody) {
                throw new Error('Không tìm thấy phần tử hoadon-list-body');
            }

            hoadonListBody.innerHTML = '';
            const hoaDons = result.data;

            if (!Array.isArray(hoaDons) || hoaDons.length === 0) {
                hoadonListBody.innerHTML = '<tr><td colspan="13">Không có hóa đơn nào được tìm thấy</td></tr>';
                return;
            }

            hoaDons.forEach(hoaDon => {
                const showPayButton = hoaDon.trangThai === 'Chưa thanh toán';
                const actionButton = showPayButton
                    ? `<button class="pay-btn" onclick="redirectToPayment(${hoaDon.maHoaDon})"><i class="fas fa-money-bill"></i> Thanh toán</button>`
                    : '';

                const row = `
                <tr>
                    <td>${hoaDon.maHoaDon || 'N/A'}</td>
                    <td>${hoaDon.maHopDong || 'N/A'}</td>
                    <td>${hoaDon.thang || 'N/A'}</td>
                    <td>${hoaDon.nam || 'N/A'}</td>
                    <td>${(hoaDon.chiSoNuocMoi - hoaDon.chiSoNuocCu) || 0}</td>
                    <td>${(hoaDon.chiSoDienMoi - hoaDon.chiSoDienCu) || 0}</td>
                    <td>${formatCurrencyVN(hoaDon.tienDien || 0)}</td>
                    <td>${formatCurrencyVN(hoaDon.tienNuoc || 0)}</td>
                    <td>${formatCurrencyVN(hoaDon.tienRac || 0)}</td>
                    <td>${formatCurrencyVN(hoaDon.tongTien || 0)}</td>
                    <td>${hoaDon.ngayPhatHanh || 'N/A'}</td>
                    <td>${hoaDon.trangThai || 'N/A'}</td>
                    <td>${actionButton}</td>
                </tr>`;
                hoadonListBody.innerHTML += row;
            });
        } catch (error) {
            console.error('Error in loadHoaDons:', error);
            const hoadonListBody = document.getElementById('hoadon-list-body');
            if (hoadonListBody) {
                hoadonListBody.innerHTML = `<tr><td colspan="13">Có lỗi xảy ra khi tải danh sách hóa đơn: ${error.message}</td></tr>`;
            }
        }
    }
    //
    loadTransactions();
    loadRooms();
    loadHoaDons();

    
    
});



// Hàm tìm kiếm giao dịch
function searchTransactions() {
    const searchTerm = document.getElementById('transaction-search').value;
    console.log('Tìm kiếm giao dịch:', searchTerm);
    loadTransactions(searchTerm);
}

// Hàm tìm kiếm phòng
function searchRooms() {
    const searchTerm = document.querySelector('.room-search-input').value;
    console.log('Tìm kiếm phòng:', searchTerm);
    loadRooms(searchTerm);
}

async function loadRooms(searchTerm = '') {
    try {
        const maNguoiDung = window.MaNguoiDung;
        console.log('MaNguoiDung:', maNguoiDung);

        if (!maNguoiDung || maNguoiDung === '' || isNaN(maNguoiDung)) {
            alert('Không thể xác định người dùng');
            return;
        }

        const response = await fetch(`/TrangCaNhan/GetRooms?maNguoiDung=${encodeURIComponent(maNguoiDung)}&searchTerm=${encodeURIComponent(searchTerm)}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            }
        });

        if (!response.ok) {
            throw new Error(`Lỗi HTTP: ${response.status} - ${response.statusText}`);
        }

        const result = await response.json();
        console.log('Result:', result);

        if (!result.success) {
            throw new Error(result.message || 'Lỗi từ API');
        }

        const rooms = result.data;
        const roomsBody = document.getElementById('rooms-body');
        roomsBody.innerHTML = '';

        if (!Array.isArray(rooms) || rooms.length === 0) {
            roomsBody.innerHTML = '<tr><td colspan="10">Không có phòng nào được tìm thấy</td></tr>';
            return;
        }

        rooms.forEach(room => {
            // Sử dụng camelCase cho tên thuộc tính từ API
            const maHopDong = room.maHopDong || null; // Đảm bảo khớp với API
            const maPhong = room.maPhong || 'N/A';
            const ngayBatDau = room.ngayBatDau ? new Date(room.ngayBatDau).toLocaleDateString('vi-VN') : 'N/A';
            const ngayKetThuc = room.ngayKetThuc ? new Date(room.ngayKetThuc).toLocaleDateString('vi-VN') : 'N/A';
            const tienCoc = room.tienCoc ? new Intl.NumberFormat('vi-VN').format(room.tienCoc) + ' VNĐ' : 'N/A';
            const trangThai = room.trangThai || 'N/A';
            const ngayKy = room.ngayKy ? new Date(room.ngayKy).toLocaleDateString('vi-VN') : 'N/A';
            const ghiChu = room.ghiChu || 'Không có ghi chú';
            const phuongThucThanhToan = room.phuongThucThanhToan || 'N/A';

            const showPayButton = phuongThucThanhToan === 'Chưa thanh toán';
            const actionButton = showPayButton && maHopDong !== null
                ? `<button class="pay-btn" onclick="redirectToPaymentHD(${maHopDong})"><i class="fas fa-money-bill"></i> Thanh toán</button>`
                : '';

            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${maHopDong !== null ? maHopDong : 'N/A'}</td>
                <td>${maPhong}</td>
                <td>${ngayBatDau}</td>
                <td>${ngayKetThuc}</td>
                <td>${tienCoc}</td>
                <td>${trangThai}</td>
                <td>${ngayKy}</td>
                <td>${ghiChu}</td>
                <td>${phuongThucThanhToan}</td>
                <td>${actionButton}</td> <!-- Thêm cột Hành động -->
            `;
            roomsBody.appendChild(row);
        });
    } catch (error) {
        console.error('Error:', error);
        const roomsBody = document.getElementById('rooms-body');
        roomsBody.innerHTML = '<tr><td colspan="10">Có lỗi xảy ra khi tải danh sách phòng: ' + error.message + '</td></tr>';
    }
}


// Hàm tải lịch sử giao dịch
async function loadTransactions(searchTerm = '') {
    try {
        const maNguoiDung = window.MaNguoiDung;
        console.log('MaNguoiDung:', maNguoiDung);

        if (!maNguoiDung || maNguoiDung === '' || isNaN(maNguoiDung)) {
            alert('Không thể xác định người dùng');
            return;
        }

        const response = await fetch(`/TrangCaNhan/GetTransactions?maNguoiDung=${encodeURIComponent(maNguoiDung)}&searchTerm=${encodeURIComponent(searchTerm)}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            }
        });

        if (!response.ok) {
            throw new Error(`Lỗi HTTP: ${response.status} - ${response.statusText}`);
        }

        const result = await response.json();
        console.log('Transactions Result:', result);

        if (!result.success) {
            throw new Error(result.message || 'Lỗi từ API');
        }

        const transactions = result.data;
        const transactionsBody = document.getElementById('transactions-body');
        transactionsBody.innerHTML = '';

        if (!Array.isArray(transactions) || transactions.length === 0) {
            transactionsBody.innerHTML = '<tr><td colspan="5">Không có giao dịch nào được tìm thấy</td></tr>';
            return;
        }

        transactions.forEach(transaction => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${transaction.transactionId || 'N/A'}</td>
                <td>${transaction.orderDescription || 'Không có mô tả'}</td>
                <td>${transaction.paymentMethod || 'N/A'}</td>
                <td>${transaction.amount || 'N/A'}</td>
                <td>${transaction.dateCreated || 'N/A'}</td>
            `;
            transactionsBody.appendChild(row);
        });
    } catch (error) {
        console.error('Error:', error);
        const transactionsBody = document.getElementById('transactions-body');
        transactionsBody.innerHTML = `<tr><td colspan="5">Có lỗi xảy ra khi tải lịch sử giao dịch: ${error.message}</td></tr>`;
    }
}

function formatCurrencyVN(amount) {
    return Number(amount).toLocaleString('vi-VN', { style: 'currency', currency: 'VND' }).replace('₫', 'VNĐ');
}
