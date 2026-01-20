let phongTrosCache = null;

document.addEventListener('DOMContentLoaded', function () {
    // Hàm hiển thị toast thông báo
    function showToast(message, isSuccess = true) {
        const toast = document.createElement('div');
        toast.className = `toast ${isSuccess ? 'success' : 'error'}`;
        toast.innerHTML = `
            <span class="toast-icon fas fa-${isSuccess ? 'check-circle' : 'times-circle'}"></span>
            <span class="toast-text">${message}</span>
            <span class="toast-close fas fa-times"></span>
        `;
        document.body.appendChild(toast);

        toast.querySelector('.toast-close').onclick = () => {
            toast.classList.add('fade-out');
            setTimeout(() => toast.remove(), 500);
        };

        setTimeout(() => {
            toast.classList.add('fade-out');
            setTimeout(() => toast.remove(), 500);
        }, 3000);
    }

    // Hàm định dạng ngày cho thông báo
    function formatDateVN(date) {
        const day = String(date.getDate()).padStart(2, '0');
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const year = date.getFullYear();
        return `${day}/${month}/${year}`;
    }

    // Hàm định dạng ngày giờ từ API cho hiển thị
    function formatDateTimeForDisplay(dateTimeString) {
        if (!dateTimeString) return 'Không xác định';
        const date = new Date(dateTimeString);
        if (isNaN(date.getTime())) return 'Không xác định';
        return date.toLocaleString('vi-VN', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit'
        });
    }

    // Hàm định dạng ngày từ API cho hiển thị
    function formatDateForDisplay(dateString) {
        if (!dateString) return 'Không xác định';
        const date = new Date(dateString);
        if (isNaN(date.getTime())) return 'Không xác định';
        return date.toLocaleDateString('vi-VN', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        });
    }

    // Xử lý modal
    const modals = document.querySelectorAll('.modal');
    const closeModalButtons = document.querySelectorAll('.modal .close, .modal .btn-secondary');
    closeModalButtons.forEach(button => {
        button.addEventListener('click', function () {
            this.closest('.modal').style.display = 'none';
        });
    });

    window.addEventListener('click', function (event) {
        modals.forEach(modal => {
            if (event.target === modal) {
                modal.style.display = 'none';
            }
        });
    });

    // Xử lý modal thêm/sửa hợp đồng
    const hopDongModal = document.getElementById('hopdong-modal');
    const hopDongForm = document.getElementById('hopdong-form');
    const btnAddHopDong = document.getElementById('btn-add-hopdong');
    const hopDongModalTitle = document.getElementById('hopdong-modal-title');

    // Các trường liên quan
    const maPhongSelect = document.getElementById('hopdong-maphong');
    const ngayBatDauInput = document.getElementById('hopdong-ngaybatdau');
    const ngayKetThucInput = document.getElementById('hopdong-ngayketthuc');
    const tienCocInput = document.getElementById('hopdong-tiencoc');
    const userInputTypeRadioGroup = document.querySelector('.radio-group');
    const existingUserGroup = document.getElementById('existing-user-group');
    const newUserGroup = document.getElementById('new-user-group');
    const existingUserRadio = document.getElementById('existing-user');
    const newUserRadio = document.getElementById('new-user');
    const hopDongNguoiDungSelect = document.getElementById('hopdong-nguoidung');

    // Định dạng ngày hiện tại (YYYY-MM-DD) để đặt giá trị min
    const today = new Date();
    const todayFormatted = today.toISOString().split('T')[0];

    // Đặt giá trị min cho Ngày bắt đầu và Ngày kết thúc là ngày hiện tại
    ngayBatDauInput.setAttribute('min', todayFormatted);
    ngayKetThucInput.setAttribute('min', todayFormatted);

    // Hàm tính số tháng giữa hai ngày
    function calculateMonthsBetweenDates(startDate, endDate) {
        const start = new Date(startDate);
        const end = new Date(endDate);
        if (isNaN(start.getTime()) || isNaN(end.getTime())) {
            return 0; // Trả về 0 nếu ngày không hợp lệ
        }
        const yearsDiff = end.getFullYear() - start.getFullYear();
        const monthsDiff = end.getMonth() - start.getMonth();
        let totalMonths = yearsDiff * 12 + monthsDiff;
        if (end.getDate() < start.getDate()) {
            totalMonths = Math.max(0, totalMonths - 1); // Điều chỉnh nếu ngày kết thúc nhỏ hơn ngày bắt đầu trong tháng
        }
        return Math.max(1, totalMonths); // Đảm bảo ít nhất 1 tháng
    }

    // Hàm tính tiền cọc
    function calculateTienCoc() {
        console.log('Tính tiền cọc...');
        const maPhong = maPhongSelect.value;
        const ngayBatDau = ngayBatDauInput.value;
        const ngayKetThuc = ngayKetThucInput.value;

        console.log('Mã phòng:', maPhong);
        console.log('Ngày bắt đầu:', ngayBatDau);
        console.log('Ngày kết thúc:', ngayKetThuc);

        if (!maPhong || !ngayBatDau || !ngayKetThuc) {
            tienCocInput.value = 0;
            return;
        }

        const selectedOption = maPhongSelect.options[maPhongSelect.selectedIndex];
        if (!selectedOption) {
            showToast('Phòng không tồn tại trong danh sách.', false);
            tienCocInput.value = 0;
            return;
        }

        const giaThueRaw = selectedOption.getAttribute('data-giathue');
        console.log('Giá thuê raw:', giaThueRaw);

        if (!giaThueRaw) {
            showToast('Giá thuê không được cung cấp.', false);
            tienCocInput.value = 0;
            return;
        }

        const giaThue = parseFloat(giaThueRaw.replace(/\./g, '').replace(/,/g, '.'));
        console.log('Giá thuê parsed:', giaThue);

        if (isNaN(giaThue) || giaThue <= 0) {
            showToast(`Không thể tính tiền cọc: Giá thuê phòng không hợp lệ (${giaThueRaw})`, false);
            tienCocInput.value = 0;
            return;
        }

        const startDate = new Date(ngayBatDau);
        const endDate = new Date(ngayKetThuc);

        if (isNaN(startDate.getTime()) || isNaN(endDate.getTime())) {
            showToast('Ngày bắt đầu hoặc ngày kết thúc không hợp lệ.', false);
            tienCocInput.value = 0;
            return;
        }

        if (endDate <= startDate) {
            showToast(`Ngày kết thúc phải sau ngày bắt đầu`, false);
            tienCocInput.value = 0;
            return;
        }

        const months = calculateMonthsBetweenDates(ngayBatDau, ngayKetThuc);
        if (months <= 0) {
            showToast('Số tháng tính được không hợp lệ.', false);
            tienCocInput.value = 0;
            return;
        }

        const tienCoc = Math.round(months * giaThue * 0.5);
        console.log('Số tháng:', months);
        console.log('Tiền cọc tính được:', tienCoc);

        tienCocInput.value = tienCoc.toLocaleString('vi-VN');
    }

    // Gắn sự kiện change cho các trường để tính lại tiền cọc
    maPhongSelect.addEventListener('change', calculateTienCoc);
    ngayBatDauInput.addEventListener('change', function () {
        const today = new Date();
        const selectedDate = new Date(this.value);

        if (selectedDate < today) {
            showToast(`Không được chọn ngày trong quá khứ. Hôm nay là ${formatDateVN(today)}`, false);
            this.value = '';
            tienCocInput.value = 0;
            return;
        }

        const ngayBatDauValue = ngayBatDauInput.value;
        if (ngayBatDauValue) {
            ngayKetThucInput.setAttribute('min', ngayBatDauValue);
            if (ngayKetThucInput.value && ngayKetThucInput.value < ngayBatDauValue) {
                ngayKetThucInput.value = '';
                tienCocInput.value = 0;
            }
        }

        calculateTienCoc();
    });

    ngayKetThucInput.addEventListener('change', function () {
        const startDate = new Date(ngayBatDauInput.value);
        const endDate = new Date(this.value);

        if (ngayBatDauInput.value && endDate <= startDate) {
            showToast(`Ngày kết thúc (${formatDateVN(endDate)}) phải sau ngày bắt đầu (${formatDateVN(startDate)})`, false);
            this.value = '';
            tienCocInput.value = 0;
            return;
        }

        calculateTienCoc();
    });

    // Xử lý radio button chọn người dùng (chỉ cần khi thêm hợp đồng)
    existingUserRadio.addEventListener('change', function () {
        existingUserGroup.style.display = 'block';
        newUserGroup.style.display = 'none';
        hopDongNguoiDungSelect.setAttribute('required', 'required');
        document.getElementById('new-user-fullname').removeAttribute('required');
        document.getElementById('new-user-email').removeAttribute('required');
        document.getElementById('new-user-phone').removeAttribute('required');
    });

    newUserRadio.addEventListener('change', function () {
        existingUserGroup.style.display = 'none';
        newUserGroup.style.display = 'block';
        hopDongNguoiDungSelect.removeAttribute('required');
        document.getElementById('new-user-fullname').setAttribute('required', 'required');
        document.getElementById('new-user-email').setAttribute('required', 'required');
        document.getElementById('new-user-phone').setAttribute('required', 'required');
    });

    // Mở modal thêm hợp đồng
    btnAddHopDong.addEventListener('click', function () {
        hopDongModal.style.display = 'block';
        hopDongModalTitle.textContent = 'Thêm hợp đồng mới';
        hopDongForm.reset();
        document.getElementById('hopdong-id').value = '';
        existingUserRadio.checked = true;
        existingUserGroup.style.display = 'block';
        newUserGroup.style.display = 'none';
        userInputTypeRadioGroup.style.display = 'block';
        hopDongNguoiDungSelect.setAttribute('required', 'required');
        ngayBatDauInput.setAttribute('min', todayFormatted);
        ngayKetThucInput.setAttribute('min', todayFormatted);
        tienCocInput.value = 0;
    });

    // Hàm định dạng ngày từ API cho input date
    function formatDateForInput(dateString) {
        if (!dateString) return '';

        const date = new Date(dateString);
        if (isNaN(date.getTime())) {
            console.error('Ngày không hợp lệ:', dateString);
            return '';
        }

        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        const formattedDate = `${year}-${month}-${day}`;
        console.log(`Định dạng ngày (${dateString}):`, formattedDate);
        return formattedDate;
    }

    // Hàm định dạng datetime từ API cho input datetime-local
    function formatDateTimeForInput(dateTimeString) {
        if (!dateTimeString) return '';

        const date = new Date(dateTimeString);
        if (isNaN(date.getTime())) {
            console.error('Ngày không hợp lệ:', dateTimeString);
            return '';
        }

        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        const hours = String(date.getHours()).padStart(2, '0');
        const minutes = String(date.getMinutes()).padStart(2, '0');
        const formattedDateTime = `${year}-${month}-${day}T${hours}:${minutes}`;
        console.log(`Định dạng ngày giờ (${dateTimeString}):`, formattedDateTime);
        return formattedDateTime;
    }

    // Hàm tải danh sách tất cả phòng
    async function loadAllPhongTros() {
        if (phongTrosCache) return phongTrosCache;
        try {
            const response = await fetch(window.getAllPhongTrosUrl, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                }
            });

            const result = await response.json();
            if (result.success && result.data) {
                phongTrosCache = result.data;
                return phongTrosCache;
            } else {
                showToast(result.message || 'Không thể tải danh sách phòng.', false);
                return [];
            }
        } catch (error) {
            showToast('Lỗi khi tải danh sách phòng: ' + error.message, false);
            return [];
        }
    }

    // Load phòng trọ còn trống (dùng khi thêm hợp đồng và sửa hợp đồng)
    async function loadAllNullPhongTros(currentPhong = null) {
        try {
            const url = new URL(window.getAllNullPhongTrosUrl);
            if (currentPhong && currentPhong.maPhong != null) {
                url.searchParams.append('currentPhong', currentPhong.maPhong.toString());
            }

            console.log('URL gọi API:', url.toString());
            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                }
            });

            const result = await response.json();
            console.log('Phản hồi từ API GetAllNullRoom:', result);

            const maPhongSelect = document.getElementById('hopdong-maphong');
            maPhongSelect.innerHTML = '<option value="">Chọn phòng</option>';

            if (result.success && result.data) {
                let hasCurrentPhong = false;
                result.data.forEach(phong => {
                    const option = document.createElement('option');
                    option.value = phong.maPhong.toString();
                    option.setAttribute('data-giathue', phong.giaThue || '0');
                    const isCurrent = currentPhong && phong.maPhong.toString() === currentPhong.maPhong.toString();
                    if (isCurrent) hasCurrentPhong = true;
                    option.textContent = `${phong.tenPhong} (Mã: ${phong.maPhong})${isCurrent ? ' - Hiện tại' : ''}`;
                    maPhongSelect.appendChild(option);
                });

                if (currentPhong && !hasCurrentPhong && currentPhong.maPhong != null) {
                    const option = document.createElement('option');
                    option.value = currentPhong.maPhong.toString();
                    option.setAttribute('data-giathue', currentPhong.giaThue || '0');
                    option.textContent = `${currentPhong.tenPhong || 'Phòng không xác định'} (Mã: ${currentPhong.maPhong}) - Hiện tại`;
                    maPhongSelect.appendChild(option);
                }

                console.log('Danh sách phòng sau khi tải:', Array.from(maPhongSelect.options).map(opt => ({ value: opt.value, text: opt.text, giathue: opt.getAttribute('data-giathue') })));
            } else {
                showToast(result.message || 'Không thể tải danh sách phòng còn trống.', false);
                if (currentPhong && currentPhong.maPhong != null) {
                    const option = document.createElement('option');
                    option.value = currentPhong.maPhong.toString();
                    option.setAttribute('data-giathue', currentPhong.giaThue || '0');
                    option.textContent = `${currentPhong.tenPhong || 'Phòng không xác định'} (Mã: ${currentPhong.maPhong}) - Hiện tại`;
                    maPhongSelect.appendChild(option);
                }
            }
        } catch (error) {
            showToast('Lỗi khi tải danh sách phòng còn trống: ' + error.message, false);
            if (currentPhong && currentPhong.maPhong != null) {
                const maPhongSelect = document.getElementById('hopdong-maphong');
                const option = document.createElement('option');
                option.value = currentPhong.maPhong.toString();
                option.setAttribute('data-giathue', currentPhong.giaThue || '0');
                option.textContent = `${currentPhong.tenPhong || 'Phòng không xác định'} (Mã: ${currentPhong.maPhong}) - Hiện tại`;
                maPhongSelect.appendChild(option);
            }
        }
    }

    // Hàm tải danh sách hợp đồng từ API
    async function loadHopDongList(searchTerm = '', pageNumber = 1, pageSize = 10) {
        try {
            await loadAllPhongTros(); // Giả sử hàm này không ảnh hưởng

            const url = new URL(window.getHopDongsUrl);
            if (searchTerm) url.searchParams.append('search', searchTerm);
            url.searchParams.append('pageNumber', pageNumber);
            url.searchParams.append('pageSize', pageSize);

            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                }
            });

            const result = await response.json();
            console.log('Danh sách hợp đồng với phân trang:', result);

            if (result.success && result.data) {
                const hopDongListBody = document.getElementById('hopdong-list-body');
                hopDongListBody.innerHTML = '';

                if (result.data.length === 0) {
                    hopDongListBody.innerHTML = '<tr><td colspan="13" class="text-center">Không có hợp đồng nào để hiển thị.</td></tr>';
                    updatePagination(result.totalPages, result.pageNumber, result.pageSize, result.totalItems);
                    return;
                }

                result.data.forEach((hopDong, index) => {
                    const maHopDong = hopDong.maHopDong || 'Không xác định';
                    const maPhong = hopDong.maPhong || 'Không xác định';
                    const tenPhong = hopDong.tenPhong || 'Không xác định';
                    const hoTenNguoiDung = hopDong.hoTenNguoiDung || 'Không xác định';
                    const ngayBatDau = hopDong.ngayBatDau || 'Không xác định';
                    const ngayKetThuc = hopDong.ngayKetThuc || 'Không xác định';
                    const tienCoc = hopDong.tienCoc != null ? new Intl.NumberFormat('vi-VN').format(hopDong.tienCoc) + ' VNĐ' : '0 VNĐ';
                    const trangThai = hopDong.trangThai || 'Không xác định';
                    const phuongThucThanhToan = hopDong.phuongThucThanhToan || 'Không xác định';
                    const ngayKy = hopDong.ngayKy;
                    const ghiChu = hopDong.ghiChu || 'Không có ghi chú';

                    const row = document.createElement('tr');
                    row.innerHTML = `
                    <td>${(pageNumber - 1) * pageSize + index + 1}</td>
                    <td>${maHopDong}</td>
                    <td>${maPhong}</td>
                    <td>${tenPhong}</td>
                    <td>${hoTenNguoiDung}</td>
                    <td>${ngayBatDau}</td>
                    <td>${ngayKetThuc}</td>
                    <td>${tienCoc}</td>
                    <td>${trangThai}</td>
                    <td>${phuongThucThanhToan}</td>
                    <td>${ngayKy}</td>
                    <td>${ghiChu}</td>
                    <td>
                        <button type="button" class="btn btn-warning btn-sm btn-update-hopdong" data-id="${maHopDong}">
                            <i class="fas fa-edit"></i> Sửa
                        </button>
                        <button type="button" class="btn btn-danger btn-sm btn-delete-hopdong" data-id="${maHopDong}">
                            <i class="fas fa-trash-alt"></i> Xóa
                        </button>
                    </td>
                `;
                    hopDongListBody.appendChild(row);
                });

                document.querySelectorAll('.btn-update-hopdong').forEach(button => {
                    button.addEventListener('click', handleUpdateHopDong);
                });

                document.querySelectorAll('.btn-delete-hopdong').forEach(button => {
                    button.addEventListener('click', handleDeleteHopDong);
                });

                updatePagination(result.totalPages, result.pageNumber, result.pageSize, result.totalItems);
            } else {
                showToast(result.message || 'Không thể tải danh sách hợp đồng.', false);
                document.getElementById('hopdong-list-body').innerHTML = '<tr><td colspan="13" class="text-center">Không có hợp đồng nào để hiển thị.</td></tr>';
                updatePagination(0, pageNumber, pageSize, 0);
            }
        } catch (error) {
            console.error('Error:', error);
            showToast('Lỗi khi tải danh sách hợp đồng: ' + error.message, false);
            document.getElementById('hopdong-list-body').innerHTML = '<tr><td colspan="13" class="text-center">Có lỗi xảy ra khi tải danh sách hợp đồng.</td></tr>';
            updatePagination(0, pageNumber, pageSize, 0);
        }
    }

    function updatePagination(totalPages, currentPage, pageSize, totalItems) {
        const pagination = document.getElementById('pagination');
        pagination.innerHTML = '';

        const maxPagesToShow = 5; // Số trang tối đa hiển thị
        let startPage = Math.max(1, currentPage - Math.floor(maxPagesToShow / 2));
        let endPage = Math.min(totalPages, startPage + maxPagesToShow - 1);

        if (endPage - startPage + 1 < maxPagesToShow) {
            startPage = Math.max(1, endPage - maxPagesToShow + 1);
        }

        // Nút Previous
        if (currentPage > 1) {
            const prevLi = document.createElement('li');
            prevLi.className = 'page-item';
            prevLi.innerHTML = `<a class="page-link" href="#" data-page="${currentPage - 1}">Previous</a>`;
            pagination.appendChild(prevLi);
        }

        // Nút trang
        for (let i = startPage; i <= endPage; i++) {
            const li = document.createElement('li');
            li.className = 'page-item' + (i === currentPage ? ' active' : '');
            li.innerHTML = `<a class="page-link" href="#" data-page="${i}">${i}</a>`;
            pagination.appendChild(li);
        }

        // Nút Next
        if (currentPage < totalPages) {
            const nextLi = document.createElement('li');
            nextLi.className = 'page-item';
            nextLi.innerHTML = `<a class="page-link" href="#" data-page="${currentPage + 1}">Next</a>`;
            pagination.appendChild(nextLi);
        }

        // Thêm thông tin tổng số bản ghi
        const info = document.createElement('li');
        info.className = 'page-item disabled';
        info.innerHTML = `<a class="page-link" href="#">Tổng: ${totalItems} bản ghi (${pageSize}/trang)</a>`;
        pagination.appendChild(info);

        // Thêm sự kiện click cho các nút phân trang
        pagination.querySelectorAll('.page-link').forEach(link => {
            link.addEventListener('click', async function (e) {
                e.preventDefault();
                const page = parseInt(this.getAttribute('data-page'));
                const searchTerm = searchHopDongInput.value.trim();
                await loadHopDongList(searchTerm, page, pageSize);
            });
        });
    }

    // Xử lý sửa hợp đồng
    async function handleUpdateHopDong(event) {
        const hopDongId = event.target.closest('button').getAttribute('data-id');
        console.log('Mã hợp đồng để sửa:', hopDongId);
        try {
            const response = await fetch(`${window.getHopDongUrl}/${hopDongId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                }
            });

            const result = await response.json();
            console.log('Dữ liệu hợp đồng:', result);

            if (result.success && result.data) {
                const hopDong = result.data;
                console.log('Full hopDong object:', hopDong);

                // Kiểm tra và lấy giá thuê từ danh sách phòng trọ nếu không hợp lệ
                let giaThue = hopDong.giaThue;
                if (!giaThue || isNaN(giaThue) || giaThue <= 0) {
                    const phong = phongTrosCache.find(p => p.maPhong.toString() === hopDong.maPhong.toString());
                    giaThue = phong && phong.giaThue != null ? parseFloat(phong.giaThue) : 0;
                    if (giaThue <= 0) {
                        showToast('Không thể lấy giá thuê từ dữ liệu phòng. Vui lòng kiểm tra lại.', false);
                        return;
                    }
                }

                hopDongModal.style.display = 'block';
                hopDongModalTitle.textContent = 'Sửa hợp đồng';
                document.getElementById('hopdong-id').value = hopDong.maHopDong;

                const currentPhong = {
                    maPhong: hopDong.maPhong,
                    tenPhong: hopDong.tenPhong,
                    giaThue: giaThue,
                    trangThai: hopDong.trangThai || "null"
                };
                console.log('CurrentPhong:', currentPhong);

                // Tải lại danh sách phòng khi sửa hợp đồng
                await loadAllNullPhongTros(currentPhong);

                // Điền mã phòng
                const maPhongField = document.getElementById('hopdong-maphong');
                console.log('Các option của hopdong-maphong:', Array.from(maPhongField.options).map(opt => ({ value: opt.value, text: opt.text, giathue: opt.getAttribute('data-giathue') })));
                if (hopDong.maPhong != null) {
                    maPhongField.value = hopDong.maPhong.toString();
                    const selectedOption = Array.from(maPhongField.options).find(option => option.value === hopDong.maPhong.toString());
                    if (!selectedOption) {
                        showToast('Phòng đã được xóa hoặc không tồn tại. Vui lòng chọn phòng khác.', false);
                        maPhongField.value = '';
                    } else {
                        const selectedGiaThue = selectedOption.getAttribute('data-giathue');
                        if (!selectedGiaThue || isNaN(selectedGiaThue) || parseFloat(selectedGiaThue) <= 0) {
                            showToast('Giá thuê của phòng không hợp lệ. Vui lòng kiểm tra lại.', false);
                            maPhongField.value = '';
                        }
                    }
                } else {
                    showToast('Mã phòng không hợp lệ trong dữ liệu hợp đồng.', false);
                    maPhongField.value = '';
                }
                console.log('Điền mã phòng:', maPhongField.value);

                // Ẩn radio button và new-user-group khi sửa
                userInputTypeRadioGroup.style.display = 'none';
                newUserGroup.style.display = 'none';
                existingUserGroup.style.display = 'block';
                hopDongNguoiDungSelect.setAttribute('required', 'required');

                // Điền thông tin người dùng hiện tại
                const nguoiDungField = document.getElementById('hopdong-nguoidung');
                console.log('Các option của hopdong-nguoidung:', Array.from(nguoiDungField.options).map(opt => ({ value: opt.value, text: opt.text })));
                const nguoiDungOption = Array.from(hopDongNguoiDungSelect.options)
                    .find(option => option.value == hopDong.maNguoiDung);
                if (nguoiDungOption) {
                    nguoiDungField.value = hopDong.maNguoiDung;
                } else {
                    showToast('Người dùng không tồn tại trong danh sách. Vui lòng chọn người dùng khác.', false);
                    nguoiDungField.value = '';
                }
                console.log('Điền mã người dùng:', nguoiDungField.value);

                // Điền các trường khác trong try-catch
                try {
                    const ngayBatDauField = document.getElementById('hopdong-ngaybatdau');
                    const ngayKetThucField = document.getElementById('hopdong-ngayketthuc');
                    const tienCocField = document.getElementById('hopdong-tiencoc');
                    const phuongThucThanhToanField = document.getElementById('hopdong-phuongthuc-thanhtoan');
                    const trangThaiField = document.getElementById('hopdong-trangthai');
                    const ngayKyField = document.getElementById('hopdong-ngayky');
                    const ghiChuField = document.getElementById('hopdong-ghichu');

                    if (!ngayBatDauField || !ngayKetThucField || !tienCocField || !trangThaiField || !ngayKyField || !ghiChuField) {
                        throw new Error('Một hoặc nhiều phần tử không tồn tại trong DOM');
                    }

                    ngayBatDauField.value = formatDateForInput(hopDong.ngayBatDau);
                    ngayKetThucField.value = formatDateForInput(hopDong.ngayKetThuc);
                    trangThaiField.value = hopDong.trangThai;
                    phuongThucThanhToanField.value = hopDong.phuongThucThanhToan || 'Chưa thanh toán';
                    ngayKyField.value = formatDateTimeForInput(hopDong.ngayKy);
                    ghiChuField.value = hopDong.ghiChu || '';

                    console.log('Ngày bắt đầu:', ngayBatDauField.value);
                    console.log('Ngày kết thúc:', ngayKetThucField.value);
                    console.log('Trạng thái:', trangThaiField.value);
                    console.log('Ngày ký:', ngayKyField.value);
                    console.log('Ghi chú:', ghiChuField.value);

                    const ngayBatDauValue = ngayBatDauField.value;
                    ngayBatDauInput.setAttribute('min', todayFormatted);
                    ngayKetThucInput.setAttribute('min', ngayBatDauValue > todayFormatted ? ngayBatDauValue : todayFormatted);

                    // Tính lại tiền cọc sau khi điền dữ liệu
                    calculateTienCoc();
                } catch (error) {
                    console.error('Lỗi khi điền dữ liệu vào các trường:', error);
                    showToast('Lỗi khi điền dữ liệu vào modal: ' + error.message, false);
                }
            } else {
                showToast(result.message || 'Không thể tải thông tin hợp đồng.', false);
            }
        } catch (error) {
            showToast('Lỗi khi tải thông tin hợp đồng: ' + error.message, false);
        }
    }

    // Xử lý thêm/sửa hợp đồng
    hopDongForm.addEventListener('submit', async function (event) {
        event.preventDefault();
        const maPhong = document.getElementById('hopdong-maphong').value;
        const ngayBatDau = document.getElementById('hopdong-ngaybatdau').value;
        const ngayKetThuc = document.getElementById('hopdong-ngayketthuc').value;
        const ngayKy = document.getElementById('hopdong-ngayky').value;
        let tienCoc = document.getElementById('hopdong-tiencoc').value;
        const trangThai = document.getElementById('hopdong-trangthai').value;
        const phuongThucThanhToan = document.getElementById('hopdong-phuongthuc-thanhtoan').value;
        const ghiChu = document.getElementById('hopdong-ghichu').value;
        const hopDongId = document.getElementById('hopdong-id').value;

        if (!maPhong) {
            showToast('Vui lòng chọn phòng.', false);
            return;
        }
        if (!ngayBatDau) {
            showToast('Vui lòng chọn ngày bắt đầu.', false);
            return;
        }
        if (!ngayKetThuc) {
            showToast('Vui lòng chọn thời gian kết thúc.', false);
            return;
        }
        if (!ngayKy) {
            showToast('Vui lòng chọn ngày ký.', false);
            return;
        }
        if (!tienCoc || parseFloat(tienCoc.replace(/\./g, '')) <= 0) {
            showToast('Tiền cọc không hợp lệ.', false);
            return;
        }
        if (!trangThai) {
            showToast('Vui lòng chọn trạng thái.', false);
            return;
        }
        if (!phuongThucThanhToan) {
            showToast('Vui lòng chọn phương thức thanh toán.', false);
            return;
        }
        if (!ghiChu) {
            showToast('Vui lòng ghi nội dung ghi chú', false);
            return;
        }

        // Khởi tạo formData trước khi sử dụng
        const formData = new FormData(hopDongForm);

        if (!hopDongId) {
            const userInputType = document.querySelector('input[name="userInputType"]:checked')?.value;
            if (!userInputType) {
                showToast('Vui lòng chọn cách nhập người dùng.', false);
                return;
            }

            if (userInputType === 'existing') {
                const maNguoiDung = document.getElementById('hopdong-nguoidung').value;
                if (!maNguoiDung || maNguoiDung === '') {
                    showToast('Vui lòng chọn người dùng hiện có.', false);
                    return;
                }
                formData.set('MaNguoiDung', maNguoiDung);
            } else if (userInputType === 'new') {
                const hoTen = document.getElementById('new-user-fullname').value;
                const email = document.getElementById('new-user-email').value;
                const soDienThoai = document.getElementById('new-user-phone').value;
                if (!hoTen || !email || !soDienThoai) {
                    showToast('Vui lòng điền đầy đủ thông tin người dùng mới.', false);
                    return;
                }
                formData.set('NewUserHoTen', hoTen);
                formData.set('NewUserEmail', email);
                formData.set('NewUserSoDienThoai', soDienThoai);
            }
        } else {
            const maNguoiDung = document.getElementById('hopdong-nguoidung').value;
            if (!maNguoiDung) {
                showToast('Vui lòng chọn người dùng.', false);
                return;
            }
            formData.set('MaNguoiDung', maNguoiDung);
        }

        // Xử lý TienCoc: Xóa định dạng và gửi giá trị số
        const tienCocValue = parseFloat(tienCoc.replace(/\./g, '')); // Chuyển "10.000.000" thành 10000000
        formData.set('TienCoc', tienCocValue.toString()); // Ghi đè giá trị TienCoc

        formData.set('PhuongThucThanhToan', phuongThucThanhToan);

        const url = hopDongId ? window.updateHopDongUrl : window.addHopDongUrl;
        const method = hopDongId ? 'PUT' : 'POST';
        console.log('Sending request to URL:', url);
        console.log('Form data:');
        for (let [key, value] of formData.entries()) {
            console.log(`${key}: ${value}`);
        }

        try {
            const response = await fetch(url, {
                method: method,
                body: formData,
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                }
            });

            const result = await response.json();
            if (result.success) {
                showToast(result.message || 'Thao tác thành công!', true);
                alert(result.message)
                hopDongModal.style.display = 'none';
                await loadHopDongList();
            } else {
                alert(result.message)
                showToast(result.message || 'Thao tác thất bại.', false);
                console.log('Server errors:', result.detail);
            }
        } catch (error) {
            showToast('Lỗi kết nối server: ' + error.message, false);
            console.error('Error details:', error);
        }
    });
    // Xử lý xóa hợp đồng
    const confirmModal = document.getElementById('confirm-modal');
    const confirmMessage = document.getElementById('confirm-message');
    const btnConfirmDo = document.getElementById('btn-confirm-do');
    const btnConfirmCancel = document.getElementById('btn-confirm-cancel');
    let currentHopDongIdToDelete = null;

    function handleDeleteHopDong(event) {
        currentHopDongIdToDelete = event.target.closest('button').getAttribute('data-id');
        confirmMessage.textContent = 'Bạn có chắc chắn muốn xóa hợp đồng này?';
        confirmModal.style.display = 'block';
        console.log('ID hợp đồng để xóa:', currentHopDongIdToDelete);
    }

    btnConfirmDo.addEventListener('click', async function () {
        if (currentHopDongIdToDelete) {
            try {
                console.log(`Sending DELETE request to: ${window.deleteHopDongUrl}/${currentHopDongIdToDelete}`);
                const response = await fetch(`${window.deleteHopDongUrl}/${currentHopDongIdToDelete}`, {
                    method: 'DELETE',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                    }
                });

                const result = await response.json();
                console.log('Response from server:', result);
                if (result.success) {
                    showToast(result.message || 'Xóa hợp đồng thành công!', true);
                    await loadHopDongList();
                } else {
                    showToast(result.message || 'Xóa hợp đồng thất bại.', false);
                }
            } catch (error) {
                showToast('Lỗi khi xóa hợp đồng: ' + error.message, false);
                console.error('Error details:', error);
            }
        }
        confirmModal.style.display = 'none';
        currentHopDongIdToDelete = null;
    });

    btnConfirmCancel.addEventListener('click', function () {
        confirmModal.style.display = 'none';
        currentHopDongIdToDelete = null;
    });

    // Tìm kiếm hợp đồng
    const searchHopDongInput = document.getElementById('search-hopdong-input');
    const searchHopDongBtn = document.getElementById('search-hopdong-btn');
    const resetHopDongSearchBtn = document.getElementById('reset-hopdong-search-btn');

    searchHopDongBtn.addEventListener('click', async function () {
        const searchTerm = searchHopDongInput.value.trim();
        resetHopDongSearchBtn.style.display = searchTerm ? 'inline-block' : 'none';
        await loadHopDongList(searchTerm, 1, 10); // Reset về trang 1 khi tìm kiếm
    });

    searchHopDongInput.addEventListener('keypress', async function (event) {
        if (event.key === 'Enter') {
            const searchTerm = searchHopDongInput.value.trim();
            resetHopDongSearchBtn.style.display = searchTerm ? 'inline-block' : 'none';
            await loadHopDongList(searchTerm, 1, 10); // Reset về trang 1 khi tìm kiếm
        }
    });

    resetHopDongSearchBtn.addEventListener('click', async function () {
        searchHopDongInput.value = '';
        resetHopDongSearchBtn.style.display = 'none';
        await loadHopDongList('', 1, 10); // Reset về trang 1 khi xóa tìm kiếm
    });

    document.querySelectorAll('.btn-update-hopdong').forEach(button => {
        button.addEventListener('click', handleUpdateHopDong);
    });

    document.querySelectorAll('.btn-delete-hopdong').forEach(button => {
        button.addEventListener('click', handleDeleteHopDong);
    });

    loadHopDongList();
});