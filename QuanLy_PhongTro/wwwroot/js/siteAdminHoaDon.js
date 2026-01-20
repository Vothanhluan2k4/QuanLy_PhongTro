document.addEventListener('DOMContentLoaded', function () {
    // Lấy các phần tử DOM
    const hoadonListBody = document.getElementById('hoadon-list-body');
    const hoadonModal = document.getElementById('hoadon-modal');
    const hoadonForm = document.getElementById('hoadon-form');
    const hoadonModalTitle = document.getElementById('hoadon-modal-title');
    const hoadonIdInput = document.getElementById('hoadon-id');
    const hoadonHopDongSelect = document.getElementById('hoadon-hopdong');
    const hoadonThangInput = document.getElementById('hoadon-thang');
    const hoadonTienPhongInput = document.getElementById('hoadon-tienphong');
    const hoadonChiSoDienCuInput = document.getElementById('hoadon-chisoDienCu');
    const hoadonChiSoDienMoiInput = document.getElementById('hoadon-chisoDienMoi');
    const hoadonTienDienInput = document.getElementById('hoadon-tienDien');
    const hoadonChiSoNuocCuInput = document.getElementById('hoadon-chisoNuocCu');
    const hoadonChiSoNuocMoiInput = document.getElementById('hoadon-chisoNuocMoi');
    const hoadonTienNuocInput = document.getElementById('hoadon-tienNuoc');
    const hoadonTongTienInput = document.getElementById('hoadon-tongtien');
    const hoadonTienRacInput = document.getElementById('hoadon-tienrac');
    const hoadonTrangThaiSelect = document.getElementById('hoadon-trangthai');
    const closeModalBtn = hoadonModal?.querySelector('.close');
    const cancelBtn = document.getElementById('hoadon-btn-cancel');
    const searchInput = document.getElementById('search-hoadon-input');
    const searchBtn = document.getElementById('search-hoadon-btn');
    const resetSearchBtn = document.getElementById('reset-hoadon-search-btn');

    // Biến cho phân trang
    let currentPage = 1;
    const itemsPerPage = 10;
    let totalItems = 0;
    let allHoaDonData = [];
    let filteredData = [];

    // Kiểm tra DOM
    if (!hoadonModal || !hoadonForm || !hoadonListBody) {
        console.error('Một hoặc nhiều phần tử DOM không được tìm thấy!');
        return;
    }

    // Hiển thị danh sách hóa đơn khi trang được tải
    loadHoaDons();

    function formatCurrencyVN(amount) {
        return Number(amount).toLocaleString('vi-VN');
    }

    // Hàm lấy và hiển thị danh sách hóa đơn
    async function loadHoaDons(searchTerm = '') {
        try {
            const response = await fetch(`/Admin/HoaDon/GetAll?searchTerm=${searchTerm}`);
            const result = await response.json();
            console.log('LoadHoaDons response:', result);

            if (result.success) {
                allHoaDonData = result.data;
                totalItems = allHoaDonData.length;

                // Lọc dữ liệu nếu có từ khóa tìm kiếm
                if (searchTerm) {
                    filteredData = allHoaDonData.filter(hoaDon =>
                        hoaDon.maHoaDon.toString().includes(searchTerm) ||
                        hoaDon.maHopDong.toString().includes(searchTerm) ||
                        hoaDon.trangThai.toLowerCase().includes(searchTerm.toLowerCase())
                    );
                    totalItems = filteredData.length;
                } else {
                    filteredData = [...allHoaDonData];
                }

                // Hiển thị trang hiện tại
                displayHoaDons(currentPage);
                // Cập nhật phân trang
                updatePagination();
            } else {
                alert(result.message);
            }
        } catch (error) {
            alert('Lỗi khi tải danh sách hóa đơn: ' + error.message);
        }
    }

    // Hàm hiển thị danh sách hóa đơn theo trang
    function displayHoaDons(page) {
        const startIndex = (page - 1) * itemsPerPage;
        const endIndex = startIndex + itemsPerPage;
        const displayData = filteredData.slice(startIndex, endIndex);

        hoadonListBody.innerHTML = '';

        if (displayData.length === 0) {
            hoadonListBody.innerHTML = '<tr><td colspan="12" class="text-center">Không có dữ liệu hóa đơn</td></tr>';
            return;
        }

        displayData.forEach((hoaDon, index) => {
            const actualIndex = startIndex + index + 1;
            const row = `
                <tr>
                    <td>${actualIndex}</td>
                    <td>${hoaDon.maHoaDon}</td>
                    <td>${hoaDon.maHopDong}</td>
                    <td>${hoaDon.thang}</td>
                    <td>${hoaDon.nam}</td>
                    <td>${formatCurrencyVN(hoaDon.tienDien)}</td>
                    <td>${formatCurrencyVN(hoaDon.tienNuoc)}</td>
                    <td>${formatCurrencyVN(hoaDon.tienRac)}</td>
                    <td>${formatCurrencyVN(hoaDon.tongTien)}</td>
                    <td>${hoaDon.ngayPhatHanh}</td>
                    <td>${hoaDon.trangThai}</td>
                    <td>
                        <button class="btn btn-warning btn-sm" onclick="editHoaDon(${hoaDon.maHoaDon})">Sửa</button>
                        <button class="btn btn-danger btn-sm" onclick="deleteHoaDon(${hoaDon.maHoaDon})">Xóa</button>
                    </td>
                </tr>`;
            hoadonListBody.innerHTML += row;
        });
    }

    // Tạo và cập nhật phần phân trang
    function updatePagination() {
        const totalPages = Math.ceil(totalItems / itemsPerPage);
        let paginationContainer = document.getElementById('hoadon-pagination');

        // Nếu chưa có container phân trang, tạo mới
        if (!paginationContainer) {
            paginationContainer = document.createElement('div');
            paginationContainer.id = 'hoadon-pagination';
            paginationContainer.className = 'pagination-container';
            document.querySelector('.hoadon-list').appendChild(paginationContainer);
        }

        let paginationHTML = '<ul class="pagination justify-content-center">';

        // Nút Previous
        paginationHTML += `
            <li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
                <a class="page-link" href="#" data-page="${currentPage - 1}">Previous</a>
            </li>
        `;

        // Hiển thị tối đa 5 trang
        const maxPagesToShow = 5;
        let startPage = Math.max(1, currentPage - Math.floor(maxPagesToShow / 2));
        let endPage = Math.min(totalPages, startPage + maxPagesToShow - 1);

        // Điều chỉnh nếu không đủ trang ở cuối
        if (endPage - startPage + 1 < maxPagesToShow) {
            startPage = Math.max(1, endPage - maxPagesToShow + 1);
        }

        // Trang đầu và dấu "..."
        if (startPage > 1) {
            paginationHTML += `<li class="page-item"><a class="page-link" href="#" data-page="1">1</a></li>`;
            if (startPage > 2) {
                paginationHTML += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
            }
        }

        // Các trang giữa
        for (let i = startPage; i <= endPage; i++) {
            paginationHTML += `
                <li class="page-item ${i === currentPage ? 'active' : ''}">
                    <a class="page-link" href="#" data-page="${i}">${i}</a>
                </li>
            `;
        }

        // Trang cuối và dấu "..."
        if (endPage < totalPages) {
            if (endPage < totalPages - 1) {
                paginationHTML += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
            }
            paginationHTML += `<li class="page-item"><a class="page-link" href="#" data-page="${totalPages}">${totalPages}</a></li>`;
        }

        // Nút Next
        paginationHTML += `
            <li class="page-item ${currentPage === totalPages || totalPages === 0 ? 'disabled' : ''}">
                <a class="page-link" href="#" data-page="${currentPage + 1}">Next</a>
            </li>
        `;

        paginationHTML += '</ul>';

        // Thêm thông tin về tổng số mục
        paginationHTML += `
            <div class="pagination-info text-center mt-2">
                Hiển thị ${Math.min((currentPage - 1) * itemsPerPage + 1, totalItems)} - ${Math.min(currentPage * itemsPerPage, totalItems)} 
                trên tổng số ${totalItems} hóa đơn
            </div>
        `;

        paginationContainer.innerHTML = paginationHTML;

        // Thêm sự kiện cho các nút phân trang
        const pageLinks = paginationContainer.querySelectorAll('.page-link');
        pageLinks.forEach(link => {
            link.addEventListener('click', function (e) {
                e.preventDefault();
                const pageNumber = parseInt(this.getAttribute('data-page'));
                if (!isNaN(pageNumber) && pageNumber > 0 && pageNumber <= totalPages) {
                    currentPage = pageNumber;
                    displayHoaDons(currentPage);
                    updatePagination();
                }
            });
        });
    }

    // Sự kiện tìm kiếm
    searchBtn.addEventListener('click', function () {
        const searchTerm = searchInput.value.trim();
        currentPage = 1; // Reset về trang đầu tiên khi tìm kiếm

        if (searchTerm) {
            resetSearchBtn.style.display = 'inline-block';
        }

        loadHoaDons(searchTerm);
    });

    // Sự kiện reset tìm kiếm
    resetSearchBtn.addEventListener('click', function () {
        searchInput.value = '';
        resetSearchBtn.style.display = 'none';
        currentPage = 1;
        loadHoaDons();
    });

    // Sự kiện nhấn Enter để tìm kiếm
    searchInput.addEventListener('keypress', function (e) {
        if (e.key === 'Enter') {
            searchBtn.click();
        }
    });

    // Phần code còn lại giữ nguyên như trong paste.txt
    // Hàm lấy chỉ số điện nước dựa trên hợp đồng và tháng/năm
    async function loadChiSoDienNuoc() {
        const maHopDong = hoadonHopDongSelect.value;
        const thangNam = hoadonThangInput.value;
        console.log('loadChiSoDienNuoc - maHopDong:', maHopDong, 'thangNam:', thangNam);

        // Nếu không có mã hợp đồng hoặc tháng/năm, reset các input
        if (!maHopDong || !thangNam) {
            resetMeterInputs();
            return;
        }

        const [nam, thang] = thangNam.split('-');
        try {
            const response = await fetch(`/Admin/HoaDon/GetChiSoDienNuoc?maHopDong=${maHopDong}&thang=${thang}&nam=${nam}`);
            const result = await response.json();
            console.log('loadChiSoDienNuoc response:', result);

            if (result.success) {
                // Gán giá trị từ API vào các input
                hoadonChiSoDienCuInput.value = result.data.chiSoDienCu || 0;
                hoadonChiSoDienMoiInput.value = result.data.chiSoDienMoi || result.data.chiSoDienCu || 0;
                hoadonChiSoNuocCuInput.value = result.data.chiSoNuocCu || 0;
                hoadonChiSoNuocMoiInput.value = result.data.chiSoNuocMoi || result.data.chiSoNuocCu || 0;

                hoadonChiSoDienMoiInput.setCustomValidity('');
                hoadonChiSoNuocMoiInput.setCustomValidity('');
                // Tính lại tiền điện/nước dựa trên chỉ số
                calculateTienDienNuoc();
            } else {
                resetMeterInputs();
                alert(result.message || 'Không thể lấy chỉ số điện nước.');
            }
        } catch (error) {
            alert('Lỗi khi lấy chỉ số điện nước: ' + error.message);
            resetMeterInputs();
        }
    }

    // Tính tiền điện và nước
    function calculateTienDienNuoc() {
        const maHopDong = hoadonHopDongSelect.value;
        const thangNam = hoadonThangInput.value;
        if (!maHopDong || !thangNam) {
            hoadonTienDienInput.value = 0;
            hoadonTienNuocInput.value = 0;
            calculateTongTien();
            return;
        }

        const [nam, thang] = thangNam.split('-');
        fetch(`/Admin/HoaDon/GetChiSoDienNuoc?maHopDong=${maHopDong}&thang=${thang}&nam=${nam}`)
            .then(response => response.json())
            .then(result => {
                console.log('calculateTienDienNuoc response:', result);
                if (result.success) {
                    const chiSoDienMoi = parseFloat(hoadonChiSoDienMoiInput.value) || 0;
                    const chiSoDienCu = parseFloat(hoadonChiSoDienCuInput.value) || 0;
                    const tienDienUnit = result.data.tienDienUnit || 0;
                    const tienDien = (chiSoDienMoi - chiSoDienCu) * tienDienUnit;
                    hoadonTienDienInput.value = tienDien >= 0 ? tienDien : 0;

                    const chiSoNuocMoi = parseFloat(hoadonChiSoNuocMoiInput.value) || 0;
                    const chiSoNuocCu = parseFloat(hoadonChiSoNuocCuInput.value) || 0;
                    const tienNuocUnit = result.data.tienNuocUnit || 0;
                    const tienNuoc = (chiSoNuocMoi - chiSoNuocCu) * tienNuocUnit;
                    hoadonTienNuocInput.value = tienNuoc >= 0 ? tienNuoc : 0;

                    calculateTongTien();
                }
            })
            .catch(error => {
                alert('Lỗi khi tính tiền điện nước: ' + error.message);
                hoadonTienDienInput.value = 0;
                hoadonTienNuocInput.value = 0;
                calculateTongTien();
            });
    }

    // Reset các input điện/nước
    function resetMeterInputs() {
        hoadonChiSoDienCuInput.value = 0;
        hoadonChiSoDienMoiInput.value = 0;
        hoadonChiSoNuocCuInput.value = 0;
        hoadonChiSoNuocMoiInput.value = 0;
        hoadonTienDienInput.value = 0;
        hoadonTienNuocInput.value = 0;
        calculateTongTien();
    }

    // Tính tổng tiền
    function calculateTongTien() {
        const tienPhong = parseFloat(hoadonTienPhongInput.value) || 0;
        const tienDien = parseFloat(hoadonTienDienInput.value) || 0;
        const tienNuoc = parseFloat(hoadonTienNuocInput.value) || 0;
        const tienRac = parseFloat(hoadonTienRacInput.value) || 0;
        const tongTien = tienPhong + tienDien + tienNuoc + tienRac;
        hoadonTongTienInput.value = tongTien;
        hoadonTienPhongInput.value = tienPhong;
        console.log('Calculated TongTien:', tongTien);
    }

    hoadonHopDongSelect.addEventListener('change', function () {
        const selectedOption = hoadonHopDongSelect.options[hoadonHopDongSelect.selectedIndex];
        let giaThue = selectedOption.getAttribute('data-giathue');
        console.log('GiaThue raw:', giaThue);

        if (giaThue) {
            giaThue = parseFloat(giaThue);
        } else {
            giaThue = 0;
        }

        console.log('GiaThue parsed:', giaThue);
        hoadonTienPhongInput.value = giaThue;
        calculateTongTien();
        loadChiSoDienNuoc();
    });

    // Sự kiện khi thay đổi tháng/năm
    hoadonThangInput.addEventListener('change', function () {
        loadChiSoDienNuoc();
    });

    // Kiểm tra và hiển thị lỗi khi nhập chỉ số điện mới
    hoadonChiSoDienMoiInput.addEventListener('input', function () {
        const chiSoDienCu = parseFloat(hoadonChiSoDienCuInput.value) || 0;
        const chiSoDienMoi = parseFloat(hoadonChiSoDienMoiInput.value) || 0;

        if (chiSoDienMoi < chiSoDienCu) {
            hoadonChiSoDienMoiInput.setCustomValidity('Chỉ số điện mới không thể nhỏ hơn chỉ số cũ.');
        } else {
            hoadonChiSoDienMoiInput.setCustomValidity('');
        }

        calculateTienDienNuoc();
    });

    // Kiểm tra và hiển thị lỗi khi nhập chỉ số nước mới
    hoadonChiSoNuocMoiInput.addEventListener('input', function () {
        const chiSoNuocCu = parseFloat(hoadonChiSoNuocCuInput.value) || 0;
        const chiSoNuocMoi = parseFloat(hoadonChiSoNuocMoiInput.value) || 0;

        if (chiSoNuocMoi < chiSoNuocCu) {
            hoadonChiSoNuocMoiInput.setCustomValidity('Chỉ số nước mới không thể nhỏ hơn chỉ số cũ.');
        } else {
            hoadonChiSoNuocMoiInput.setCustomValidity('');
        }

        calculateTienDienNuoc();
    });

    // Sự kiện khi thay đổi tiền rác
    hoadonTienRacInput.addEventListener('input', function () {
        calculateTongTien();
    });

    // Mở modal để thêm hóa đơn
    window.openAddHoaDonModal = function () {
        console.log('Opening modal...');
        hoadonModalTitle.textContent = 'Thêm hóa đơn mới';
        hoadonIdInput.value = '';
        hoadonForm.reset();
        hoadonTienPhongInput.value = 0;
        hoadonTienDienInput.value = 0;
        hoadonTienNuocInput.value = 0;
        hoadonTienRacInput.value = 0;
        hoadonTongTienInput.value = 0;
        hoadonTrangThaiSelect.value = 'Chưa thanh toán';

        hoadonChiSoDienMoiInput.setCustomValidity('');
        hoadonChiSoNuocMoiInput.setCustomValidity('');
        hoadonModal.style.display = 'block';
        console.log('Modal display set to block');
        setTimeout(() => loadChiSoDienNuoc(), 0);
    };

    // Mở modal để sửa hóa đơn
    window.editHoaDon = async function (maHoaDon) {
        try {
            const response = await fetch(`/Admin/HoaDon/GetAll`);
            const result = await response.json();
            console.log('editHoaDon response:', result);

            if (result.success) {
                const hoaDon = result.data.find(hd => hd.maHoaDon === maHoaDon);
                if (hoaDon) {
                    hoadonModalTitle.textContent = 'Sửa hóa đơn';
                    hoadonIdInput.value = hoaDon.maHoaDon;
                    hoadonHopDongSelect.value = hoaDon.maHopDong;

                    // Kiểm tra giá trị Thang và Nam trước khi gán
                    const thang = hoaDon.thang !== undefined && hoaDon.thang !== null ? hoaDon.thang.toString().padStart(2, '0') : '01';
                    const nam = hoaDon.nam !== undefined && hoaDon.nam !== null ? hoaDon.nam : new Date().getFullYear();
                    hoadonThangInput.value = `${nam}-${thang}`;

                    hoadonTienDienInput.value = hoaDon.tienDien || 0;
                    hoadonTienNuocInput.value = hoaDon.tienNuoc || 0;
                    hoadonTienRacInput.value = hoaDon.tienRac || 0;
                    hoadonTongTienInput.value = hoaDon.tongTien || 0;
                    hoadonTrangThaiSelect.value = hoaDon.trangThai;

                    const selectedOption = hoadonHopDongSelect.options[hoadonHopDongSelect.selectedIndex];
                    let giaThue = selectedOption ? parseFloat(selectedOption.getAttribute('data-giathue') || 0) : 0;
                    hoadonTienPhongInput.value = giaThue;

                    await loadChiSoDienNuoc();
                    hoadonModal.style.display = 'block';
                } else {
                    alert('Không tìm thấy hóa đơn với mã: ' + maHoaDon);
                }
            } else {
                alert(result.message || 'Lỗi khi tải thông tin hóa đơn.');
            }
        } catch (error) {
            alert('Lỗi khi tải thông tin hóa đơn: ' + error.message);
        }
    };

    // Đóng modal
    closeModalBtn?.addEventListener('click', function () {
        hoadonModal.style.display = 'none';
    });

    cancelBtn?.addEventListener('click', function () {
        hoadonModal.style.display = 'none';
    });

    // Trong sự kiện submit form
    hoadonForm.addEventListener('submit', async function (e) {
        e.preventDefault();
        const formData = new FormData(hoadonForm);

        // Tách Thang và Nam
        const thangNam = hoadonThangInput.value;
        if (thangNam) {
            const [nam, thang] = thangNam.split('-');
            formData.set('Thang', parseInt(thang));
            formData.set('Nam', parseInt(nam));
        }


        // Thêm các giá trị chỉ số vào formData
        const chiSoDienCu = parseFloat(hoadonChiSoDienCuInput.value) || 0;
        const chiSoDienMoi = parseFloat(hoadonChiSoDienMoiInput.value) || 0;
        const chiSoNuocCu = parseFloat(hoadonChiSoNuocCuInput.value) || 0;
        const chiSoNuocMoi = parseFloat(hoadonChiSoNuocMoiInput.value) || 0;

        formData.set('CSDienCu', chiSoDienCu);
        formData.set('CSDienMoi', chiSoDienMoi);
        formData.set('CSNuocCu', chiSoNuocCu);
        formData.set('CSNuocMoi', chiSoNuocMoi);

        // Đảm bảo MaHoaDon được gửi từ input
        formData.set('MaHoaDon', hoadonIdInput.value || '0');
        // Xóa các giá trị không cần thiết để backend tự tính
        formData.delete('TienDien');
        formData.delete('TienNuoc');

        // Đảm bảo có TienRac
        if (!formData.has('TienRac')) {
            formData.append('TienRac', '0');
        }

        // Log dữ liệu trước khi gửi
        console.log('Form data before submit:', Object.fromEntries(formData));

        const url = hoadonIdInput.value ? '/Admin/HoaDon/Update' : '/Admin/HoaDon/Add';
        try {
            const response = await fetch(url, {
                method: 'POST',
                body: formData
            });
            const result = await response.json();

            // Log dữ liệu sau khi gửi
            console.log('Submit response:', result);

            if (result.success) {
                alert(result.message);
                hoadonModal.style.display = 'none';
                loadHoaDons(); // Tải lại danh sách sau khi lưu thành công
            } else {
                alert(result.message);
            }
        } catch (error) {
            alert('Lỗi khi lưu hóa đơn: ' + error.message);
        }
    });

    window.deleteHoaDon = async function (maHoaDon) {
        if (!confirm('Bạn có chắc chắn muốn xóa hóa đơn này? Hành động này không thể hoàn tác!')) {
            return;
        }

        try {
            const response = await fetch(`/Admin/HoaDon/Delete/${maHoaDon}`, {
                method: 'DELETE',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error(`Lỗi HTTP: ${response.status} - ${response.statusText}`);
            }

            const result = await response.json();
            console.log('Delete response:', result);

            if (result.success) {
                alert(result.message);
                loadHoaDons(); // Tải lại danh sách sau khi xóa thành công
            } else {
                alert(result.message);
            }
        } catch (error) {
            console.error('Lỗi khi xóa hóa đơn:', error);
            alert('Lỗi khi xóa hóa đơn: ' + error.message);
        }
    };
});