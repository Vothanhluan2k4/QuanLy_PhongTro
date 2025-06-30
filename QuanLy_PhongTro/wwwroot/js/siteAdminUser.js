document.addEventListener('DOMContentLoaded', function () {
    // Khai báo các phần tử DOM liên quan đến User
    const elements = {
        userModal: document.getElementById('user-modal'),
        userForm: document.getElementById('user-form'),
        btnAddUser: document.getElementById('btn-add-user'),
        userBtnCancel: document.getElementById('user-btn-cancel'),
        searchUserInput: document.getElementById('search-user-input'),
        searchUserBtn: document.getElementById('search-user-btn'),
        resetUserSearchBtn: document.getElementById('reset-user-search-btn'),
        userListBody: document.getElementById('user-list-body'),
        pagination: document.querySelector('.pagination'),
        prevPage: document.getElementById('prev-page'),
        nextPage: document.getElementById('next-page'),
        confirmModal: document.getElementById('confirm-modal'),
        confirmMessage: document.getElementById('confirm-message'),
        btnConfirmDo: document.getElementById('btn-confirm-do'),
        btnConfirmCancel: document.getElementById('btn-confirm-cancel')
    };

    // Kiểm tra các phần tử quan trọng
    if (!elements.userModal || !elements.userForm || !elements.btnAddUser || !elements.searchUserInput || !elements.searchUserBtn || !elements.resetUserSearchBtn || !elements.userListBody) {
        console.error('Missing required DOM elements for user section');
        return;
    }

    // Biến toàn cục
    let originalUserList = [];
    let currentPage = 1;
    const itemsPerPage = 5;

    // Hàm tải danh sách người dùng từ API
    async function loadUserList() {
        try {
            const response = await fetch('/Admin/NguoiDung/GetAll');
            const result = await response.json();

            if (result.success && result.data) {
                console.log('Dữ liệu từ người dùng:', result.data);
                originalUserList = result.data.map((user, index) => ({
                    index: index + 1,
                    maNguoiDung: user.maNguoiDung,
                    hoTen: user.hoTen || 'Không có',
                    email: user.email || 'Không có',
                    soDienThoai: user.soDienThoai || 'Không có',
                    tenQuyen: user.tenQuyen || 'Chưa xác định',
                    ngayTao: user.ngayTao,
                    trangThai: user.isDeleted
                }));
                renderUserList(originalUserList);
            } else {
                alert('Không thể tải danh sách người dùng: ' + (result.message || 'Lỗi không xác định'));
                elements.userListBody.innerHTML = '<tr><td colspan="9" class="text-center">Không thể tải danh sách người dùng.</td></tr>';
            }
        } catch (error) {
            console.error('Lỗi khi tải danh sách người dùng:', error);
            alert('Đã xảy ra lỗi khi tải danh sách người dùng: ' + error.message);
            elements.userListBody.innerHTML = '<tr><td colspan="9" class="text-center">Không thể tải danh sách người dùng.</td></tr>';
        }
    }

    // Hàm khởi tạo danh sách người dùng
    function initializeUserList() {
        loadUserList();
    }

    // Hàm hiển thị danh sách người dùng
    function renderUserList(userList) {
        elements.userListBody.innerHTML = '';
        const totalItems = userList.length;
        const totalPages = Math.ceil(totalItems / itemsPerPage);

        // Tính chỉ số bắt đầu và kết thúc cho trang hiện tại
        const startIndex = (currentPage - 1) * itemsPerPage;
        const endIndex = Math.min(startIndex + itemsPerPage, totalItems);
        const paginatedList = userList.slice(startIndex, endIndex);

        if (paginatedList.length === 0) {
            elements.userListBody.innerHTML = '<tr><td colspan="9" class="text-center">Không tìm thấy người dùng nào.</td></tr>';
        } else {
            paginatedList.forEach(user => {
                const row = `
                    <tr>
                        <td>${startIndex + user.index}</td>
                        <td>${user.maNguoiDung}</td>
                        <td>${user.hoTen || 'Không có'}</td>
                        <td>${user.email || 'Không có'}</td>
                        <td>${user.soDienThoai || 'Không có'}</td>
                        <td>${user.tenQuyen || 'Chưa xác định'}</td>
                        <td>${user.ngayTao}</td>
                        <td>${user.trangThai ? 'Ngừng hoạt động' : 'Hoạt động'}</td>
                        <td>
                            <button type="button" class="btn btn-warning btn-sm btn-update-user" data-id="${user.maNguoiDung}">
                                <i class="fas fa-edit"></i> Sửa
                            </button>
                            <button type="button" class="btn btn-danger btn-sm btn-delete-user" data-id="${user.maNguoiDung}">
                                <i class="fas fa-trash-alt"></i> Xóa
                            </button>
                        </td>
                    </tr>`;
                elements.userListBody.innerHTML += row;
            });
        }

        // Cập nhật phân trang
        updatePagination(totalPages);
        attachUserActions();
    }

    // Hàm cập nhật phân trang
    function updatePagination(totalPages) {
        elements.pagination.innerHTML = '';
        elements.pagination.appendChild(elements.prevPage);

        for (let i = 1; i <= totalPages; i++) {
            const pageItem = document.createElement('li');
            pageItem.classList.add('page-item');
            if (i === currentPage) pageItem.classList.add('active');
            const pageLink = document.createElement('a');
            pageLink.classList.add('page-link');
            pageLink.href = '#';
            pageLink.textContent = i;
            pageLink.addEventListener('click', (e) => {
                e.preventDefault();
                currentPage = i;
                renderUserList(originalUserList);
            });
            pageItem.appendChild(pageLink);
            elements.pagination.appendChild(pageItem);
        }

        elements.pagination.appendChild(elements.nextPage);

        // Cập nhật trạng thái nút Previous và Next
        elements.prevPage.classList.toggle('disabled', currentPage === 1);
        elements.nextPage.classList.toggle('disabled', currentPage === totalPages);

        elements.prevPage.querySelector('.page-link').addEventListener('click', (e) => {
            e.preventDefault();
            if (currentPage > 1) {
                currentPage--;
                renderUserList(originalUserList);
            }
        });

        elements.nextPage.querySelector('.page-link').addEventListener('click', (e) => {
            e.preventDefault();
            if (currentPage < totalPages) {
                currentPage++;
                renderUserList(originalUserList);
            }
        });
    }

    // Hàm tìm kiếm người dùng
    function searchUsers(searchTerm) {
        const term = searchTerm.toLowerCase().trim();
        if (!term) {
            renderUserList(originalUserList);
            return;
        }

        const filteredUsers = originalUserList.filter(user =>
            user.hoTen.toLowerCase().includes(term) ||
            user.email.toLowerCase().includes(term)
        );
        currentPage = 1; // Reset về trang 1 khi tìm kiếm
        renderUserList(filteredUsers);
    }

    // Hàm reset tìm kiếm
    function resetUserSearch() {
        elements.searchUserInput.value = '';
        elements.resetUserSearchBtn.style.display = 'none';
        renderUserList(originalUserList);
    }

    // Hàm kiểm tra dữ liệu đầu vào
    function validateUserForm(userData, isAdding) {
        const errors = [];

        if (!userData.HoTen || userData.HoTen.trim().length === 0) {
            errors.push('Họ tên không được để trống.');
        }

        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!userData.Email || !emailRegex.test(userData.Email)) {
            errors.push('Email không hợp lệ.');
        }

        const phoneRegex = /^[0-9]{10,11}$/;
        if (!userData.SoDienThoai || !phoneRegex.test(userData.SoDienThoai)) {
            errors.push('Số điện thoại phải có 10-11 chữ số.');
        }

        if (!userData.MaQuyen || isNaN(userData.MaQuyen)) {
            errors.push('Vui lòng chọn quyền hợp lệ.');
        }

        if (isAdding && (!userData.MatKhau || userData.MatKhau.length < 6)) {
            errors.push('Mật khẩu phải có ít nhất 6 ký tự.');
        }

        return errors;
    }

    // Hàm gắn sự kiện cho các nút Sửa và Xóa người dùng
    function attachUserActions() {
        // Xử lý nút Sửa người dùng
        document.querySelectorAll('.btn-update-user').forEach(btn => {
            btn.addEventListener('click', async (e) => {
                e.preventDefault();
                const userId = btn.getAttribute('data-id');
                try {
                    const response = await fetch(`/Admin/NguoiDung/GetById/${userId}`);
                    if (!response.ok) {
                        throw new Error(`Lỗi HTTP: ${response.status} - ${response.statusText}`);
                    }
                    const user = await response.json();

                    // Kiểm tra nếu API trả về lỗi (success: false)
                    if (user.success === false) {
                        alert(user.message || 'Không thể tải dữ liệu người dùng.');
                        return;
                    }

                    // Ghi log để kiểm tra dữ liệu trả về
                    console.log('Dữ liệu người dùng từ API:', user);

                    // Điền dữ liệu vào form
                    document.getElementById('user-fullname').value = user.hoTen || '';
                    document.getElementById('user-email').value = user.email || '';
                    document.getElementById('user-phone').value = user.soDienThoai || '';
                    document.getElementById('user-role').value = user.maQuyen || '';
                    const isDeleted = user.isDeleted !== undefined ? user.isDeleted : false;
                    document.getElementById('user-status').value = isDeleted.toString();

                    console.log('Dữ liệu sau khi gán vào form:', {
                        fullName: document.getElementById('user-fullname').value,
                        email: document.getElementById('user-email').value,
                        phone: document.getElementById('user-phone').value,
                        role: document.getElementById('user-role').value,
                        status: document.getElementById('user-status').value
                    });

                    // Ẩn input mật khẩu khi sửa
                    const passwordGroup = document.querySelector('#user-password').parentElement;
                    const passwordInput = document.getElementById('user-password');
                    if (passwordGroup && passwordInput) {
                        passwordGroup.style.display = 'none';
                        passwordInput.removeAttribute('required'); // Loại bỏ required
                    }

                    openModal(elements.userModal, document.getElementById('user-modal-title'), 'Sửa người dùng', elements.userForm, `/Admin/NguoiDung/Update/${userId}`);

                    // Log dữ liệu sau khi mở modal
                    console.log('Dữ liệu sau khi mở modal:', {
                        fullName: document.getElementById('user-fullname').value,
                        email: document.getElementById('user-email').value,
                        phone: document.getElementById('user-phone').value,
                        role: document.getElementById('user-role').value,
                        status: document.getElementById('user-status').value
                    });

                } catch (error) {
                    console.error('Lỗi khi lấy dữ liệu người dùng:', error);
                    alert('Không thể tải dữ liệu người dùng: ' + error.message);
                }
            });
        });

        // Xử lý nút Xóa người dùng
        document.querySelectorAll('.btn-delete-user').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const userId = btn.getAttribute('data-id');
                elements.confirmMessage.textContent = 'Bạn có chắc chắn muốn xóa người dùng này?';
                elements.confirmModal.style.display = 'block';

                elements.btnConfirmDo.onclick = async () => {
                    try {
                        elements.btnConfirmDo.disabled = true;
                        elements.btnConfirmDo.innerText = 'Đang xóa...';

                        const response = await fetch(`/Admin/NguoiDung/DeleteConfirmed`, {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json' },
                            body: JSON.stringify(userId)
                        });

                        const result = await response.json();

                        if (result.success) {
                            alert(result.message || 'Xóa người dùng thành công!');
                            await loadUserList();
                        } else {
                            let errorMessage = result.message || 'Xóa người dùng thất bại!';
                            if (result.detail && Array.isArray(result.detail)) {
                                errorMessage += ': ' + result.detail.join(', ');
                            }
                            alert(errorMessage);
                        }
                    } catch (error) {
                        console.error('Lỗi khi xóa người dùng:', error);
                        alert('Đã xảy ra lỗi khi xóa người dùng: ' + error.message);
                    } finally {
                        elements.btnConfirmDo.disabled = false;
                        elements.btnConfirmDo.innerText = 'Đồng ý';
                        closeAllModals();
                    }
                };
            });
        });
    }

    // Hàm mở modal
    function openModal(modal, titleElement, title, form, actionUrl, resetForm = false) {
        if (!titleElement) {
            console.error('Title element not found');
            return;
        }

        titleElement.textContent = title;
        form.action = actionUrl;

        if (resetForm) {
            form.reset();
        }

        modal.style.display = 'block';
    }

    // Hàm đóng tất cả modal
    function closeAllModals() {
        elements.userModal.style.display = 'none';
        elements.confirmModal.style.display = 'none';
    }

    // Gắn sự kiện tìm kiếm
    elements.searchUserBtn.addEventListener('click', (e) => {
        e.preventDefault();
        searchUsers(elements.searchUserInput.value);
    });

    elements.searchUserInput.addEventListener('input', (e) => {
        const searchTerm = e.target.value;
        searchUsers(searchTerm);
        elements.resetUserSearchBtn.style.display = searchTerm ? 'inline-block' : 'none';
    });

    // Gắn sự kiện cho nút reset tìm kiếm
    elements.resetUserSearchBtn.addEventListener('click', (e) => {
        e.preventDefault();
        resetUserSearch();
    });

    // Xử lý mở modal Thêm người dùng
    elements.btnAddUser.addEventListener('click', (e) => {
        e.preventDefault();
        // Hiển thị input mật khẩu khi thêm
        const passwordGroup = document.querySelector('#user-password').parentElement;
        if (passwordGroup) {
            passwordGroup.style.display = 'block';
            const passwordInput = document.getElementById('user-password');
            if (passwordInput) {
                passwordInput.setAttribute('required', 'required');
            }
        }
        openModal(elements.userModal, document.getElementById('user-modal-title'), 'Thêm người dùng mới', elements.userForm, '/Admin/NguoiDung/Add', true);
    });

    // Xử lý đóng modal người dùng
    elements.userBtnCancel.addEventListener('click', closeAllModals);
    elements.btnConfirmCancel.addEventListener('click', closeAllModals);

    // Xử lý submit form người dùng
    elements.userForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const formData = new FormData(elements.userForm);
        const userData = {
            HoTen: formData.get('HoTen')?.trim(),
            Email: formData.get('Email')?.trim(),
            SoDienThoai: formData.get('SoDienThoai')?.trim(),
            MaQuyen: parseInt(formData.get('MaQuyen')),
            IsDeleted: formData.get('IsDeleted') === 'true'
        };
        console.log('Dữ liệu gửi lên server:', userData);

        // Chỉ thêm MatKhau nếu đang ở chế độ thêm (input mật khẩu hiển thị)
        const passwordGroup = document.querySelector('#user-password').parentElement;
        if (passwordGroup.style.display !== 'none') {
            userData.MatKhau = formData.get('MatKhau');
        }

        const actionUrl = elements.userForm.action;
        const isEditing = actionUrl.includes('/Admin/NguoiDung/Update');

        // Kiểm tra dữ liệu đầu vào
        const validationErrors = validateUserForm(userData, !isEditing);
        if (validationErrors.length > 0) {
            alert('Vui lòng kiểm tra lại dữ liệu:\n- ' + validationErrors.join('\n- '));
            return;
        }

        try {
            const submitBtn = elements.userForm.querySelector('button[type="submit"]');
            submitBtn.disabled = true;
            submitBtn.innerText = 'Đang lưu...';

            const response = await fetch(actionUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(userData)
            });

            const result = await response.json();

            if (result.success) {
                await loadUserList();
                closeAllModals();
            } else {
                let errorMessage = result.message || 'Lưu người dùng thất bại!';
                if (result.detail && Array.isArray(result.detail)) {
                    errorMessage += '\nChi tiết lỗi:\n- ' + result.detail.join('\n- ');
                }
                alert(errorMessage);
            }
        } catch (error) {
            console.error('Lỗi khi lưu người dùng:', error);
            alert('Đã xảy ra lỗi khi lưu người dùng: ' + error.message);
        } finally {
            const submitBtn = elements.userForm.querySelector('button[type="submit"]');
            submitBtn.disabled = false;
            submitBtn.innerText = 'Lưu';
        }
    });

    // Khởi tạo danh sách người dùng gốc
    initializeUserList();
});