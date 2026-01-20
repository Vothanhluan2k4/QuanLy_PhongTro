document.addEventListener('DOMContentLoaded', function () {
    // Khai báo các phần tử DOM
    const elements = {
        roomModal: document.getElementById('room-modal'),
        roomTypeModal: document.getElementById('room-type-modal'),
        equipmentModal: document.getElementById('equipment-modal'),
        confirmModal: document.getElementById('confirm-modal'),
        btnAddRoom: document.getElementById('btn-add-room'),
        btnAddRoomType: document.getElementById('btn-add-room-type'),
        btnAddEquipment: document.getElementById('btn-add-equipment'),
        btnCancel: document.getElementById('btn-cancel'),
        roomTypeBtnCancel: document.getElementById('room-type-btn-cancel'),
        equipmentBtnCancel: document.getElementById('equipment-btn-cancel'),
        btnConfirmCancel: document.getElementById('btn-confirm-cancel'),
        btnConfirmDo: document.getElementById('btn-confirm-do'),
        closeModal: document.querySelectorAll('.close'),
        imageInput: document.getElementById('room-images'),
        imagePreview: document.getElementById('image-preview'),
        roomForm: document.getElementById('room-form'),
        roomTypeForm: document.getElementById('room-type-form'),
        equipmentForm: document.getElementById('equipment-form'),
        confirmMessage: document.getElementById('confirm-message'),
        modalTitle: document.getElementById('modal-title'),
        roomTypeModalTitle: document.getElementById('room-type-modal-title'),
        equipmentModalTitle: document.getElementById('equipment-modal-title'),
        roomIdInput: document.getElementById('room-id'),
        roomTypeListBody: document.getElementById('room-type-list-body'),
        equipmentListBody: document.getElementById('equipment-list-body'),
        searchRoomInput: document.getElementById('search-room-input'),
        searchRoomBtn: document.getElementById('search-room-btn'),
        resetSearchBtn: document.getElementById('reset-search-btn'), 
        roomListBody: document.querySelector('.room-list-section .table tbody'),
        //User
        userModal: document.getElementById('user-modal'),
        userForm: document.getElementById('user-form'),
        btnAddUser: document.getElementById('btn-add-user'),
        userBtnCancel: document.getElementById('user-btn-cancel'),
        searchUserInput: document.getElementById('search-user-input'),
        searchUserBtn: document.getElementById('search-user-btn'),
        resetUserSearchBtn: document.getElementById('reset-user-search-btn'),
        userListBody: document.getElementById('user-list-body'),
        pagination: document.querySelector('.pagination'),
        roomPagination: document.querySelector('.room-list-section .pagination'),
        roomPrevPage: document.querySelector('.room-list-section #prev-page'),
        roomNextPage: document.querySelector('.room-list-section #next-page'),
        userPagination: document.querySelector('.user-section .pagination'),
        userPrevPage: document.querySelector('.user-section #prev-page'),
        userNextPage: document.querySelector('.user-section #next-page'),
        totalRoomCount: document.getElementById('total-room-count'), 
        totalUserCount: document.getElementById('total-user-count')
    };

    // Kiểm tra các phần tử quan trọng
    if (!elements.roomModal || !elements.roomTypeModal || !elements.equipmentModal || !elements.confirmModal || !elements.btnAddRoom || !elements.roomForm || !elements.roomTypeForm || !elements.equipmentForm) {
        console.error('Missing required DOM elements');
        return;
    }
    // Kiểm tra phần tử reset button
    if (!elements.searchRoomInput || !elements.searchRoomBtn || !elements.resetSearchBtn || !elements.roomListBody) {
        console.error('Missing required DOM elements for search or reset');
        return;
    }
    //User
    if (!elements.userModal || !elements.userForm || !elements.btnAddUser || !elements.searchUserInput || !elements.searchUserBtn || !elements.resetUserSearchBtn || !elements.userListBody) {
        console.error('Missing required DOM elements for user section');
        return;
    }

    // Hàm reset tìm kiếm
    function resetSearch() {
        elements.searchRoomInput.value = ''; // Xóa nội dung ô tìm kiếm
        elements.resetSearchBtn.style.display = 'none'; // Ẩn nút reset
        renderRoomList(originalRoomList); // Hiển thị lại danh sách ban đầu
    }

    // Gắn sự kiện cho nút reset
    elements.resetSearchBtn.addEventListener('click', (e) => {
        e.preventDefault();
        resetSearch();
    });

    // Gắn sự kiện tìm kiếm (đã có)
    elements.searchRoomBtn.addEventListener('click', (e) => {
        e.preventDefault();
        searchRooms(elements.searchRoomInput.value);
    });


    elements.searchRoomInput.addEventListener('input', (e) => {
        const searchTerm = e.target.value;
        searchRooms(searchTerm);
        elements.resetSearchBtn.style.display = searchTerm ? 'inline-block' : 'none'; // Hiển thị nút reset nếu có nội dung
    });

    // Kiểm tra các biến URL được truyền từ Razor
    if (!window.addRoomUrl || !window.updateRoomUrl ||
        !window.addRoomTypeUrl || !window.updateRoomTypeUrl || !window.getRoomTypesUrl ||
        !window.addEquipmentUrl || !window.updateEquipmentUrl || !window.getEquipmentsUrl) {
        console.error('Missing required URLs');
        alert('Không thể tải URL của server. Vui lòng kiểm tra cấu hình.');
        return;
    }


    // Mảng lưu trữ thông tin ảnh và các biến trạng thái
    let imagesList = [];
    let currentEditingRoomId = null;
    let currentEditingRoomTypeId = null;
    let currentEditingEquipmentId = null; 
    let editingRoomData = null;
    let originalRoomList = [];
    let originalUserList = [];
    let currentRoomPage = 1;
    let currentUserPage = 1;
    const roomItemsPerPage = 5; 
    const userItemsPerPage = 10;

    // Hàm lấy danh sách phòng từ bảng HTML và lưu vào originalRoomList
    function initializeRoomList() {
        originalRoomList = [];
        const rows = elements.roomListBody.querySelectorAll('tr');
        rows.forEach((row, index) => {
            const cells = row.cells;
            if (cells.length < 14) return;

            const maPhong = cells[1].textContent.trim();
            const tenPhong = cells[3].textContent.trim();
            const tenLoaiPhong = cells[2].textContent.trim();
            const diaChi = cells[4].textContent.trim();
            const thietBiList = [];
            const thietBiItems = cells[5].querySelectorAll('li');
            thietBiItems.forEach(item => thietBiList.push(item.textContent.trim()));
            const dienTich = cells[6].textContent.trim();
            const tienDien = cells[7].textContent.trim();
            const tienNuoc = cells[8].textContent.trim();
            const giaThue = cells[9].textContent.trim();
            const trangThai = cells[10].textContent.trim();
            const anhDaiDien = cells[11].querySelector('img') ? cells[11].querySelector('img').src : null;
            const moTa = cells[12].textContent.trim();

            const updateBtn = cells[13].querySelector('.btn-update-room');
            originalRoomList.push({
                index: index + 1,
                maPhong: updateBtn ? updateBtn.getAttribute('data-id') : maPhong,
                tenPhong,
                tenLoaiPhong,
                diaChi,
                thietBiList,
                dienTich,
                tienDien,
                tienNuoc,
                giaThue,
                trangThai,
                anhDaiDien,
                moTa
            });
        });
        renderRoomList(originalRoomList);
        if (elements.totalRoomCount) elements.totalRoomCount.textContent = `Tổng số phòng: ${originalRoomList.length}`;
    }
    // Hàm hiển thị danh sách phòng
    function renderRoomList(roomList) {
        elements.roomListBody.innerHTML = '';
        const totalItems = roomList.length;
        const totalPages = Math.ceil(totalItems / roomItemsPerPage);
        const startIndex = (currentRoomPage - 1) * roomItemsPerPage;
        const endIndex = Math.min(startIndex + roomItemsPerPage, totalItems);
        const paginatedList = roomList.slice(startIndex, endIndex);

        if (paginatedList.length === 0) {
            elements.roomListBody.innerHTML = '<tr><td colspan="13" class="text-center">Không tìm thấy phòng nào.</td></tr>';
        } else {
            paginatedList.forEach(room => {
                const thietBiHtml = room.thietBiList.length > 0
                    ? `<ul class="thiet-bi-list">${room.thietBiList.map(item => `<li>${item}</li>`).join('')}</ul>`
                    : '<ul class="thiet-bi-list"><li>Không có thiết bị</li></ul>';
                const anhDaiDienHtml = room.anhDaiDien
                    ? `<img src="${room.anhDaiDien}" alt="Hình đại diện" class="anh-dai-dien" />`
                    : '<span>Không có ảnh</span>';

                // Giới hạn mô tả ở 100 từ
                const words = room.moTa.split(' ');
                const limitedMoTa = words.length > 100 ? words.slice(0, 100).join(' ') + '...' : room.moTa;

                const row = `
                    <tr>
                        <td>${room.index}</td>
                        <td>${room.maPhong}</td>
                        <td>${room.tenLoaiPhong}</td>
                        <td>${room.tenPhong}</td>
                        <td>${room.diaChi}</td>
                        <td>${thietBiHtml}</td>
                        <td>${room.dienTich}</td>
                        <td>${room.tienDien}</td>
                        <td>${room.tienNuoc}</td>
                        <td>${room.giaThue}</td>
                        <td>${room.trangThai}</td>
                        <td>${anhDaiDienHtml}</td>
                        <td>${limitedMoTa}</td>
                        <td>
                            <button type="button" class="btn btn-warning btn-sm btn-update-room" data-id="${room.maPhong}">Sửa</button>
                            <button type="button" class="btn btn-danger btn-sm btn-delete-room" data-id="${room.maPhong}">Xóa</button>
                        </td>
                    </tr>`;
                elements.roomListBody.innerHTML += row;
            });
        }

        updateRoomPagination(totalPages);
        attachRoomActions();
    }
    function updateRoomPagination(totalPages) {
        elements.roomPagination.innerHTML = '';
        elements.roomPagination.appendChild(elements.roomPrevPage.cloneNode(true));

        for (let i = 1; i <= totalPages; i++) {
            const pageItem = document.createElement('li');
            pageItem.classList.add('page-item');
            if (i === currentRoomPage) pageItem.classList.add('active');
            const pageLink = document.createElement('a');
            pageLink.classList.add('page-link');
            pageLink.href = '#';
            pageLink.textContent = i;
            pageLink.addEventListener('click', (e) => {
                e.preventDefault();
                currentRoomPage = i;
                renderRoomList(originalRoomList);
            });
            pageItem.appendChild(pageLink);
            elements.roomPagination.appendChild(pageItem);
        }

        elements.roomPagination.appendChild(elements.roomNextPage.cloneNode(true));

        const prevLink = elements.roomPagination.querySelector('#prev-page .page-link');
        const nextLink = elements.roomPagination.querySelector('#next-page .page-link');

        elements.roomPagination.querySelector('#prev-page').classList.toggle('disabled', currentRoomPage === 1);
        elements.roomPagination.querySelector('#next-page').classList.toggle('disabled', currentRoomPage === totalPages);

        prevLink.addEventListener('click', (e) => {
            e.preventDefault();
            if (currentRoomPage > 1) {
                currentRoomPage--;
                renderRoomList(originalRoomList);
            }
        });

        nextLink.addEventListener('click', (e) => {
            e.preventDefault();
            if (currentRoomPage < totalPages) {
                currentRoomPage++;
                renderRoomList(originalRoomList);
            }
        });
    }

    

    // Hàm tìm kiếm phòng
    function searchRooms(searchTerm) {
        const term = searchTerm.toLowerCase().trim();
        if (!term) {
            renderRoomList(originalRoomList);
            return;
        }

        const filteredRooms = originalRoomList.filter(room =>
            room.tenPhong.toLowerCase().includes(term) ||
            room.tenLoaiPhong.toLowerCase().includes(term)
        );
        currentRoomPage = 1;
        renderRoomList(filteredRooms);
    }

    // Gắn sự kiện tìm kiếm
    elements.searchRoomInput.addEventListener('input', (e) => {
        searchRooms(e.target.value);
        elements.resetSearchBtn.style.display = e.target.value ? 'inline-block' : 'none';
    });

    elements.searchRoomBtn.addEventListener('click', (e) => {
        e.preventDefault();
        searchRooms(elements.searchRoomInput.value);
    });
    // Hàm gắn sự kiện cho các nút Sửa và Xóa
    function attachRoomActions() {
        // Xử lý nút Sửa phòng
        document.querySelectorAll('.btn-update-room').forEach(btn => {
            btn.addEventListener('click', async (e) => {
                e.preventDefault();
                currentEditingRoomId = btn.getAttribute('data-id');
                try {
                    const response = await fetch(`/Admin/PhongTro/GetById/${currentEditingRoomId}`);
                    if (!response.ok) {
                        const errorText = await response.text();
                        console.error(`Phản hồi lỗi từ server: ${errorText}`);
                        throw new Error(`Lỗi HTTP: ${response.status} - ${response.statusText}`);
                    }
                    const phong = await response.json();
                    if (!phong || !phong.maPhong) {
                        throw new Error('Dữ liệu phòng không hợp lệ');
                    }

                    editingRoomData = {
                        maPhong: phong.maPhong || '',
                        tenPhong: phong.tenPhong || '',
                        maLoaiPhong: phong.maLoaiPhong || '',
                        diaChi: phong.diaChi || '',
                        dienTich: phong.dienTich || '',
                        tienDien: phong.tienDien || '',
                        tienNuoc: phong.tienNuoc || '',
                        giaThue: phong.giaThue || '',
                        trangThai: phong.trangThai || '',
                        moTa: phong.moTa || '',
                        phongTroThietBis: phong.phongTroThietBis || [],
                        anhPhongTros: phong.anhPhongTros || []
                    };

                    // Điền dữ liệu vào form
                    document.getElementById('room-name').value = editingRoomData.tenPhong;
                    document.getElementById('room-type').value = editingRoomData.maLoaiPhong;
                    document.getElementById('room-area').value = editingRoomData.dienTich;
                    document.getElementById('room-electronic').value = editingRoomData.tienDien;
                    document.getElementById('room-water').value = editingRoomData.tienNuoc;
                    document.getElementById('room-price').value = editingRoomData.giaThue;
                    document.getElementById('room-status').value = editingRoomData.trangThai;
                    document.getElementById('room-desc').value = editingRoomData.moTa;

                    // Xử lý địa chỉ
                    const [houseNumber, districtText, provinceText] = editingRoomData.diaChi.split(', ').map(part => part.trim());
                    const provinceSelect = document.getElementById('province');
                    const districtSelect = document.getElementById('district');
                    const houseNumberInput = document.getElementById('house-number');

                    houseNumberInput.value = houseNumber || '';

                    await loadProvinces();
                    const provinceOption = Array.from(provinceSelect.options).find(opt => opt.text === provinceText);
                    if (provinceOption) {
                        provinceSelect.value = provinceOption.value;
                        await loadDistricts(provinceOption.value);
                        const districtOption = Array.from(districtSelect.options).find(opt => opt.text === districtText);
                        if (districtOption) {
                            districtSelect.value = districtOption.value;
                        }
                    }

                    // Xử lý thiết bị
                    const thietBiList = document.getElementById('thiet-bi-list');
                    if (thietBiList) {
                        thietBiList.querySelectorAll('input[type="checkbox"]').forEach(checkbox => {
                            const thietBiId = checkbox.value;
                            const isChecked = editingRoomData.phongTroThietBis.some(tb => tb.maThietBi == thietBiId);
                            checkbox.checked = isChecked;
                            const soLuongInput = document.querySelector(`input[name="SoLuong_${thietBiId}"]`);
                            if (isChecked && soLuongInput) {
                                const thietBi = editingRoomData.phongTroThietBis.find(tb => tb.maThietBi == thietBiId);
                                soLuongInput.value = thietBi ? thietBi.soLuong : 1;
                                soLuongInput.style.display = 'inline-block';
                            } else if (soLuongInput) {
                                soLuongInput.style.display = 'none';
                                soLuongInput.value = 1;
                            }
                        });
                    }

                    // Xử lý ảnh
                    imagesList = [];
                    if (editingRoomData.anhPhongTros && editingRoomData.anhPhongTros.length > 0) {
                        editingRoomData.anhPhongTros.forEach((anh) => {
                            imagesList.push({
                                src: `/asset/${anh.duongDan}`,
                                name: anh.duongDan,
                                isDaiDien: anh.laAnhDaiDien,
                                isExisting: true
                            });
                        });
                    }
                    updateImagePreview();

                    openModal(elements.roomModal, elements.modalTitle, 'Sửa phòng', elements.roomForm, `${window.updateRoomUrl}/${currentEditingRoomId}`);
                } catch (error) {
                    console.error('Lỗi khi lấy dữ liệu phòng:', error);
                    alert('Không thể tải dữ liệu phòng. Vui lòng thử lại. Chi tiết lỗi: ' + error.message);
                }
            });
        });

        // Xử lý nút Xóa phòng
        document.querySelectorAll('.btn-delete-room').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const id = btn.getAttribute('data-id');
                elements.confirmMessage.textContent = 'Bạn có chắc chắn muốn xóa phòng này?';
                elements.confirmModal.style.display = 'block';

                elements.btnConfirmDo.onclick = async () => {
                    try {
                        elements.btnConfirmDo.disabled = true;
                        elements.btnConfirmDo.innerText = 'Đang xóa...';

                        const response = await fetch(`/Admin/PhongTro/DeleteConfirmed`, {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json' },
                            body: JSON.stringify({ id })
                        });

                        const result = await response.json();

                        if (result.success) {
                            alert(result.message || 'Xóa phòng thành công!');
                            location.reload();
                        } else {
                            let errorMessage = result.message || 'Xóa phòng thất bại!';
                            if (result.detail && Array.isArray(result.detail)) {
                                errorMessage += ': ' + result.detail.join(', ');
                            }
                            alert(errorMessage);
                        }
                    } catch (error) {
                        console.error('Lỗi khi xóa phòng:', error);
                        alert('Đã xảy ra lỗi khi xóa phòng: ' + error.message);
                    } finally {
                        elements.btnConfirmDo.disabled = false;
                        elements.btnConfirmDo.innerText = 'Đồng ý';
                        closeAllModals();
                    }
                };
            });
        });
    }
        
    // Hàm hiển thị section tương ứng
    function showSection(sectionId) {
        document.querySelectorAll('.dashboard-section, .room-list-section, .room-type-section, .equipment-section, .user-section, .invoice-section, .settings-section,.news-section, .revenue-section, .issues-section').forEach(section => {
            section.classList.remove('section-active');
            section.style.display = 'none';
        });

        const targetSection = document.querySelector(`.${sectionId}`);
        if (targetSection) {
            targetSection.classList.add('section-active');
            targetSection.style.display = 'block';
            localStorage.setItem('activeSection', sectionId);
        } else {
            console.error(`Section with class ${sectionId} not found.`);
        }
    }

    // Xử lý toggle submenu và đánh dấu active
    document.querySelectorAll('.menu li a').forEach(link => {
        link.addEventListener('click', function (e) {
            const sectionId = this.getAttribute('data-section');
            const parentLi = this.parentElement;
            const submenu = parentLi.querySelector('.submenu');

            // Nếu mục này có submenu, toggle hiển thị submenu
            if (submenu) {
                e.preventDefault(); // Ngăn chặn hành vi mặc định của thẻ <a>
                const isSubmenuVisible = submenu.style.display === 'block';

                // Ẩn tất cả submenu khác
                document.querySelectorAll('.submenu').forEach(sub => {
                    sub.style.display = 'none';
                });

                // Toggle submenu hiện tại
                submenu.style.display = isSubmenuVisible ? 'none' : 'block';

                // Nếu submenu được mở, tự động kích hoạt mục con đầu tiên
                if (!isSubmenuVisible) {
                    const firstSubmenuLink = submenu.querySelector('.submenu-link');
                    if (firstSubmenuLink) {
                        const defaultSectionId = firstSubmenuLink.getAttribute('data-section');
                        showSection(defaultSectionId);

                        // Đánh dấu active cho mục con đầu tiên
                        submenu.querySelectorAll('.submenu-link').forEach(item => {
                            item.classList.remove('active');
                        });
                        firstSubmenuLink.classList.add('active');
                    }
                }
            } else {
                // Nếu không có submenu, xử lý như bình thường
                if (!this.getAttribute('asp-controller') && sectionId) {
                    e.preventDefault();
                }

                // Xóa active của tất cả các mục menu chính
                document.querySelectorAll('.menu li').forEach(item => {
                    item.classList.remove('active');
                });

                // Đánh dấu active cho mục được nhấp
                parentLi.classList.add('active');

                // Ẩn tất cả submenu khi nhấp vào mục không có submenu
                document.querySelectorAll('.submenu').forEach(sub => {
                    sub.style.display = 'none';
                });

                if (sectionId) {
                    showSection(sectionId);
                }
            }
        });
    });

    // Xử lý click vào các mục submenu
    document.querySelectorAll('.submenu-link').forEach(link => {
        link.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation(); // Ngăn sự kiện click lan lên thẻ cha

            const sectionId = this.getAttribute('data-section');

            // Hiển thị section tương ứng
            showSection(sectionId);

            // Xóa active của tất cả các mục submenu
            document.querySelectorAll('.submenu-link').forEach(item => {
                item.classList.remove('active');
            });

            // Đánh dấu active cho mục submenu được nhấp
            this.classList.add('active');

            // Đảm bảo mục cha (menu chính) được đánh dấu active
            const parentLi = this.closest('.menu li');
            document.querySelectorAll('.menu li').forEach(item => {
                item.classList.remove('active');
            });
            parentLi.classList.add('active');
        });
    });

    // Khôi phục trạng thái từ localStorage khi trang tải
    const lastActiveSection = localStorage.getItem('activeSection') || 'room-list-section';
    showSection(lastActiveSection);

    // Khôi phục trạng thái menu và submenu dựa trên section được lưu
    document.querySelectorAll('.menu li').forEach(item => {
        const link = item.querySelector('a');
        const sectionId = link.getAttribute('data-section');
        const submenu = item.querySelector('.submenu');

        if (sectionId === lastActiveSection) {
            item.classList.add('active');
            if (submenu) {
                submenu.style.display = 'block'; // Hiển thị submenu nếu mục cha được active
            }
        } else if (submenu && ['room-list-section', 'room-type-section', 'equipment-section'].includes(lastActiveSection)) {
            const submenuLink = item.querySelector(`.submenu-link[data-section="${lastActiveSection}"]`);
            if (submenuLink) {
                document.querySelectorAll('.submenu-link').forEach(subItem => {
                    subItem.classList.remove('active');
                });
                submenuLink.classList.add('active');
                item.classList.add('active');
                submenu.style.display = 'block'; // Hiển thị submenu
            }
        } else {
            if (submenu) {
                submenu.style.display = 'none'; // Ẩn submenu nếu không được active
            }
            item.classList.remove('active');
        }
    });

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
        currentEditingRoomTypeId = null;
        currentEditingEquipmentId = null;
        currentEditingRoomId = null;
        editingRoomData = null;

        elements.roomModal.style.display = 'none';
        elements.roomTypeModal.style.display = 'none';
        elements.equipmentModal.style.display = 'none';
        elements.confirmModal.style.display = 'none';
        elements.userModal.style.display = 'none'; // Thêm dòng này để đóng userModal
    }

    // Hàm cập nhật giao diện danh sách ảnh
    function updateImagePreview() {
        if (!elements.imagePreview) {
            console.error('Phần tử image-preview không tồn tại trong DOM');
            return;
        }
        elements.imagePreview.innerHTML = '';

        imagesList.forEach((image, index) => {
            const imgContainer = document.createElement('div');
            imgContainer.className = 'image-preview-item';

            const img = document.createElement('img');
            img.src = image.src;
            img.alt = 'Preview';
            imgContainer.appendChild(img);

            const deleteBtn = document.createElement('button');
            deleteBtn.className = 'btn-delete-image';
            deleteBtn.innerText = '×';
            deleteBtn.addEventListener('click', () => {
                imagesList.splice(index, 1);
                updateImagePreview();
            });
            imgContainer.appendChild(deleteBtn);

            const setMainBtn = document.createElement('button');
            setMainBtn.className = 'btn-set-main';
            setMainBtn.classList.toggle('active', image.isDaiDien);
            setMainBtn.innerText = image.isDaiDien ? 'Ảnh chính' : 'Đặt ảnh chính';
            setMainBtn.addEventListener('click', () => {
                imagesList.forEach(img => img.isDaiDien = false);
                image.isDaiDien = true;
                updateImagePreview();
            });
            imgContainer.appendChild(setMainBtn);

            elements.imagePreview.appendChild(imgContainer);
        });

        if (imagesList.length > 0 && !imagesList.some(img => img.isDaiDien)) {
            imagesList[0].isDaiDien = true;
            updateImagePreview();
        }
    }

    // Xử lý mở modal Thêm phòng
    elements.btnAddRoom.addEventListener('click', (e) => {
        e.preventDefault();
        currentEditingRoomId = null
        if (elements.imagePreview) elements.imagePreview.innerHTML = '';
        imagesList = [];
        openModal(elements.roomModal, elements.modalTitle, 'Thêm phòng mới', elements.roomForm, window.addRoomUrl, true);

        document.querySelectorAll('input[name="ThietBiIds"]').forEach(checkbox => {
            checkbox.checked = false;
            const thietBiId = checkbox.value;
            const soLuongInput = document.querySelector(`input[name="SoLuong_${thietBiId}"]`);
            if (soLuongInput) {
                soLuongInput.style.display = 'none';
                soLuongInput.value = 1;
            }
        });
    });

    // Xử lý mở modal Thêm loại phòng
    elements.btnAddRoomType.addEventListener('click', (e) => {
        e.preventDefault();
        currentEditingRoomTypeId = null;
        openModal(elements.roomTypeModal, elements.roomTypeModalTitle, 'Thêm loại phòng mới', elements.roomTypeForm, window.addRoomTypeUrl, true);
    });

    // Xử lý mở modal Thêm thiết bị
    elements.btnAddEquipment.addEventListener('click', (e) => {
        e.preventDefault();
        currentEditingEquipmentId = null;
        openModal(elements.equipmentModal, elements.equipmentModalTitle, 'Thêm thiết bị mới', elements.equipmentForm, window.addEquipmentUrl, true);
    });

    // Xử lý đóng modal
    elements.closeModal.forEach(btn => btn.addEventListener('click', closeAllModals));
    elements.btnCancel.addEventListener('click', closeAllModals);
    elements.roomTypeBtnCancel.addEventListener('click', closeAllModals);
    elements.equipmentBtnCancel.addEventListener('click', closeAllModals);
    elements.btnConfirmCancel.addEventListener('click', closeAllModals);

    // Xử lý nút Sửa phòng
    document.querySelectorAll('.btn-update-room').forEach(btn => {
        btn.addEventListener('click', async (e) => {
            e.preventDefault();
            currentEditingRoomId = btn.getAttribute('data-id');

            try {
                const response = await fetch(`/Admin/PhongTro/GetById/${currentEditingRoomId}`);
                if (!response.ok) {
                    throw new Error(`Lỗi HTTP: ${response.status} - ${response.statusText}`);
                }
                const phong = await response.json();

                if (!phong || !phong.maPhong) {
                    throw new Error('Dữ liệu phòng không hợp lệ');
                }

                // Lưu dữ liệu phòng vào biến let
                editingRoomData = {
                    maPhong: phong.maPhong || '',
                    tenPhong: phong.tenPhong || '',
                    maLoaiPhong: phong.maLoaiPhong || '',
                    diaChi: phong.diaChi || '',
                    dienTich: phong.dienTich || '',
                    tienDien: phong.tienDien || '',
                    tienNuoc: phong.tienNuoc || '',
                    giaThue: phong.giaThue || '',
                    trangThai: phong.trangThai || '',
                    moTa: phong.moTa || '',
                    phongTroThietBis: phong.phongTroThietBis || [],
                    anhPhongTros: phong.anhPhongTros || []
                };

                // Điền dữ liệu vào form
                document.getElementById('room-name').value = editingRoomData.tenPhong;
                document.getElementById('room-type').value = editingRoomData.maLoaiPhong;
                document.getElementById('room-area').value = editingRoomData.dienTich;
                document.getElementById('room-electronic').value = editingRoomData.tienDien;
                document.getElementById('room-water').value = editingRoomData.tienNuoc;
                document.getElementById('room-price').value = editingRoomData.giaThue;
                document.getElementById('room-status').value = editingRoomData.trangThai;
                document.getElementById('room-desc').value = editingRoomData.moTa;

                // Tách địa chỉ thành Tỉnh, Huyện, Số nhà
                const [houseNumber, districtText, provinceText] = editingRoomData.diaChi.split(', ').map(part => part.trim());
                const provinceSelect = document.getElementById('province');
                const districtSelect = document.getElementById('district');
                const houseNumberInput = document.getElementById('house-number');

                // Điền Số nhà
                houseNumberInput.value = houseNumber || '';

                // Tải danh sách tỉnh và chọn tỉnh tương ứng
                await loadProvinces();
                const provinceOption = Array.from(provinceSelect.options).find(opt => opt.text === provinceText);
                if (provinceOption) {
                    provinceSelect.value = provinceOption.value;
                    // Tải danh sách huyện và chọn huyện tương ứng
                    await loadDistricts(provinceOption.value);
                    const districtOption = Array.from(districtSelect.options).find(opt => opt.text === districtText);
                    if (districtOption) {
                        districtSelect.value = districtOption.value;
                    }
                }

                // Xử lý danh sách thiết bị
                const thietBiList = document.getElementById('thiet-bi-list');
                if (thietBiList) {
                    thietBiList.querySelectorAll('input[type="checkbox"]').forEach(checkbox => {
                        const thietBiId = checkbox.value;
                        const isChecked = editingRoomData.phongTroThietBis.some(tb => tb.maThietBi == thietBiId);
                        checkbox.checked = isChecked;
                        const soLuongInput = document.querySelector(`input[name="SoLuong_${thietBiId}"]`);
                        if (isChecked && soLuongInput) {
                            const thietBi = editingRoomData.phongTroThietBis.find(tb => tb.maThietBi == thietBiId);
                            soLuongInput.value = thietBi ? thietBi.soLuong : 1;
                            soLuongInput.style.display = 'inline-block';
                        } else if (soLuongInput) {
                            soLuongInput.style.display = 'none';
                            soLuongInput.value = 1;
                        }
                    });
                }

                // Xử lý danh sách ảnh
                imagesList = [];
                if (editingRoomData.anhPhongTros && editingRoomData.anhPhongTros.length > 0) {
                    editingRoomData.anhPhongTros.forEach((anh) => {
                        imagesList.push({
                            src: `/asset/${anh.duongDan}`,
                            name: anh.duongDan,
                            isDaiDien: anh.laAnhDaiDien,
                            isExisting: true
                        });
                    });
                }
                updateImagePreview();

                openModal(elements.roomModal, elements.modalTitle, 'Sửa phòng', elements.roomForm, `${window.updateRoomUrl}/${currentEditingRoomId}`);
            } catch (error) {
                console.error('Lỗi khi lấy dữ liệu phòng:', error);
                alert('Không thể tải dữ liệu phòng. Vui lòng thử lại.');
            }
        });
    });

    // Xử lý nút Sửa loại phòng
    document.addEventListener('click', async function (e) {
        if (e.target && e.target.classList.contains('btn-update-room-type')) {
            e.preventDefault();
            currentEditingRoomTypeId = e.target.getAttribute('data-id');
            try {
                const response = await fetch(`/Admin/LoaiPhong/GetById/${currentEditingRoomTypeId}`);
                if (!response.ok) {
                    throw new Error(`Lỗi HTTP: ${response.status} - ${response.statusText}`);
                }
                const contentType = response.headers.get('content-type');
                if (!contentType || !contentType.includes('application/json')) {
                    throw new Error('Phản hồi không phải là JSON');
                }
                const roomType = await response.json();
                document.getElementById('room-type-name').value = roomType.tenLoaiPhong || '';
                document.getElementById('room-type-desc').value = roomType.moTa || '';
                elements.roomTypeModalTitle.textContent = 'Sửa loại phòng';
                elements.roomTypeModal.style.display = 'block';
            } catch (error) {
                console.error('Lỗi khi lấy dữ liệu loại phòng:', error);
                alert('Không thể tải dữ liệu loại phòng. Vui lòng thử lại.');
            }
        }
        // Sửa thiết bị
        if (e.target && e.target.classList.contains('btn-update-equipment')) {
            e.preventDefault();
            currentEditingEquipmentId = e.target.getAttribute('data-id');
            try {
                const response = await fetch(`/Admin/ThietBi/GetById/${currentEditingEquipmentId}`);
                if (!response.ok) {
                    throw new Error(`Lỗi HTTP: ${response.status} - ${response.statusText}`);
                }
                const contentType = response.headers.get('content-type');
                if (!contentType || !contentType.includes('application/json')) {
                    throw new Error('Phản hồi không phải là JSON');
                }
                const equipment = await response.json();
                document.getElementById('equipment-name').value = equipment.tenThietBi || '';
                document.getElementById('equipment-unit').value = equipment.donViTinh || '';
                document.getElementById('equipment-desc').value = equipment.moTa || '';
                document.getElementById('equipment-situation').value = equipment.tinhTrang || ''; // Thêm dòng này
                elements.equipmentModalTitle.textContent = 'Sửa thiết bị';
                elements.equipmentModal.style.display = 'block';
            } catch (error) {
                console.error('Lỗi khi lấy dữ liệu thiết bị:', error);
                alert('Không thể tải dữ liệu thiết bị. Vui lòng thử lại.');
            }
        }
    });

    // Xử lý nút Xóa phòng
    document.querySelectorAll('.btn-delete-room').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            const id = btn.getAttribute('data-id');
            elements.confirmMessage.textContent = 'Bạn có chắc chắn muốn xóa phòng này?';
            elements.confirmModal.style.display = 'block';

            elements.btnConfirmDo.onclick = async () => {
                try {
                    // Hiển thị trạng thái loading
                    elements.btnConfirmDo.disabled = true;
                    elements.btnConfirmDo.innerText = 'Đang xóa...';

                    const response = await fetch(`/Admin/PhongTro/DeleteConfirmed`, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ id })
                    });

                    const result = await response.json();

                    if (result.success) {
                        alert(result.message || 'Xóa phòng thành công!');
                        location.reload();
                    } else {
                        let errorMessage = result.message || 'Xóa phòng thất bại!';
                        if (result.detail && Array.isArray(result.detail)) {
                            errorMessage += ': ' + result.detail.join(', ');
                        }
                        alert(errorMessage);
                    }
                } catch (error) {
                    console.error('Lỗi khi xóa phòng:', error);
                    alert('Đã xảy ra lỗi khi xóa phòng: ' + error.message);
                } finally {
                    // Khôi phục trạng thái nút
                    elements.btnConfirmDo.disabled = false;
                    elements.btnConfirmDo.innerText = 'Đồng ý';
                    closeAllModals();
                }
            };
        });
    });

    // Xử lý nút Xóa loại phòng
    document.querySelectorAll('.btn-delete-room-type').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            const id = btn.getAttribute('data-id');
            elements.confirmMessage.textContent = 'Bạn có chắc chắn muốn xóa loại phòng này?';
            elements.confirmModal.style.display = 'block';

            elements.btnConfirmDo.onclick = () => {
                const formData = new FormData();
                formData.append('id', id);
                formData.append('__RequestVerificationToken', document.querySelector('input[name="__RequestVerificationToken"]').value);

                fetch(`/Admin/LoaiPhong/DeleteConfirmed`, {
                    method: 'POST',
                    body: formData
                })
                    .then(response => {
                        if (response.ok) {
                            location.reload();
                        } else {
                            alert('Xóa loại phòng thất bại!');
                        }
                        closeAllModals();
                    })
                    .catch(error => console.error('Lỗi khi xóa loại phòng:', error));
            };
        });
    });

    // Xử lý nút Xóa thiết bị
    document.querySelectorAll('.btn-delete-equipment').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            const id = btn.getAttribute('data-id');
            elements.confirmMessage.textContent = 'Bạn có chắc chắn muốn xóa thiết bị này?';
            elements.confirmModal.style.display = 'block';

            elements.btnConfirmDo.onclick = () => {
                const formData = new FormData();
                formData.append('id', id);
                formData.append('__RequestVerificationToken', document.querySelector('input[name="__RequestVerificationToken"]').value);

                fetch(`/Admin/ThietBi/DeleteConfirmed`, {
                    method: 'POST',
                    body: formData
                })
                    .then(response => {
                        if (response.ok) {
                            location.reload();
                        } else {
                            alert('Xóa thiết bị thất bại!');
                        }
                        closeAllModals();
                    })
                    .catch(error => console.error('Lỗi khi xóa thiết bị:', error));
            };
        });
    });

    // Xử lý hiển thị số lượng thiết bị khi checkbox được chọn
    document.querySelectorAll('input[name="ThietBiIds"]').forEach(checkbox => {
        checkbox.addEventListener('change', () => {
            const thietBiId = checkbox.value;
            const soLuongInput = document.querySelector(`input[name="SoLuong_${thietBiId}"]`);
            if (soLuongInput) {
                soLuongInput.style.display = checkbox.checked ? 'inline-block' : 'none';
                if (!checkbox.checked) soLuongInput.value = 1;
            }
        });
    });

    // Xử lý preview ảnh
    // Xử lý preview và upload ảnh
    elements.imageInput.addEventListener('change', async () => {
        const files = Array.from(elements.imageInput.files);
        if (files.length === 0) return;

        // Tạo FormData để gửi file
        const formData = new FormData();
        files.forEach(file => formData.append('files', file));

        try {
            const response = await fetch('/Admin/PhongTro/UploadImages', {
                method: 'POST',
                body: formData
            });

            const result = await response.json();
            if (!result.success) {
                alert(result.message || 'Lỗi khi upload ảnh!');
                return;
            }

            // Thêm ảnh đã upload vào imagesList với tên mới
            result.fileNames.forEach((fileName, index) => {
                const file = files[index];
                const reader = new FileReader();
                reader.onload = (e) => {
                    imagesList.push({
                        src: e.target.result,
                        name: fileName, // Sử dụng tên mới từ server
                        isDaiDien: false,
                        isExisting: false
                    });
                    updateImagePreview();
                };
                reader.readAsDataURL(file);
            });
        } catch (error) {
            console.error('Lỗi khi upload ảnh:', error);
            alert('Đã xảy ra lỗi khi upload ảnh: ' + error.message);
        }
    });

    elements.roomForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        // Thu thập dữ liệu từ form
        const tenPhong = document.getElementById('room-name').value.trim();
        const maLoaiPhong = document.getElementById('room-type').value;
        const dienTich = parseFloat(document.getElementById('room-area').value);
        const giaThue = parseFloat(document.getElementById('room-price').value);
        const tienDien = parseFloat(document.getElementById('room-electronic').value);
        const tienNuoc = parseFloat(document.getElementById('room-water').value);
        const province = document.getElementById('province');
        const district = document.getElementById('district');
        const houseNumber = document.getElementById('house-number').value.trim();
        const trangThai = document.getElementById('room-status').value;
        const moTa = document.getElementById('room-desc').value.trim();

        // Thu thập danh sách thiết bị và số lượng
        const thietBiData = [];
        document.querySelectorAll('input[name="ThietBiIds"]:checked').forEach(checkbox => {
            const thietBiId = parseInt(checkbox.value);
            const soLuongInput = document.querySelector(`input[name="SoLuong_${thietBiId}"]`);
            const soLuong = parseInt(soLuongInput.value);
            const thietBiName = checkbox.nextElementSibling.textContent;
            thietBiData.push({ thietBiId, thietBiName, soLuong });
        });

        // Thu thập danh sách ảnh
        const anhData = imagesList.map(img => ({
            name: img.name,
            isDaiDien: img.isDaiDien,
            isExisting: img.isExisting
        }));

        // Tổng hợp địa chỉ
        const provinceText = province.options[province.selectedIndex]?.text || '';
        const districtText = district.options[district.selectedIndex]?.text || '';
        const diaChi = {
            province: provinceText,
            district: districtText,
            houseNumber
        };

        // Ghi log dữ liệu đầu vào
        console.log('Dữ liệu nhập khi thêm/sửa phòng:', {
            tenPhong,
            maLoaiPhong,
            diaChi,
            dienTich,
            giaThue,
            tienDien,
            tienNuoc,
            trangThai,
            moTa,
            thietBiData,
            anhData
        });

        // Kiểm tra dữ liệu
        if (!tenPhong) {
            alert('Vui lòng nhập tên phòng!');
            return;
        }

        if (!maLoaiPhong || maLoaiPhong <= 0) {
            alert('Vui lòng chọn loại phòng hợp lệ!');
            return;
        }

        if (dienTich <= 0 || isNaN(dienTich)) {
            alert('Diện tích phải lớn hơn 0!');
            return;
        }

        if (giaThue <= 0 || isNaN(giaThue)) {
            alert('Giá thuê phải lớn hơn 0!');
            return;
        }

        if (tienDien < 0 || isNaN(tienDien)) {
            alert('Tiền điện không được âm!');
            return;
        }

        if (tienNuoc < 0 || isNaN(tienNuoc)) {
            alert('Tiền nước không được âm!');
            return;
        }

        if (!province.value) {
            alert('Vui lòng chọn Tỉnh/Thành phố!');
            return;
        }

        if (!district.value) {
            alert('Vui lòng chọn Huyện/Quận!');
            return;
        }

        if (!houseNumber) {
            alert('Vui lòng nhập Số nhà!');
            return;
        }

        if (imagesList.length === 0) {
            alert('Vui lòng chọn ít nhất một ảnh cho phòng!');
            return;
        }

        const anhDaiDien = imagesList.find(img => img.isDaiDien);
        if (!anhDaiDien || !anhDaiDien.name) {
            alert('Vui lòng chọn một ảnh đại diện hợp lệ!');
            return;
        }

        // Tạo đối tượng JSON để gửi
        const data = {
            tenPhong,
            maLoaiPhong: parseInt(maLoaiPhong),
            diaChi,
            dienTich,
            giaThue,
            tienDien,
            tienNuoc,
            trangThai,
            moTa,
            thietBiData,
            anhData
        };
        console.log('Dữ liệu gửi lên server:', JSON.stringify(data, null, 2));
        const isEditing = elements.roomForm.action.includes(window.updateRoomUrl);
        const actionUrl = isEditing ? elements.roomForm.action : window.addRoomUrl;

        try {
            const submitBtn = elements.roomForm.querySelector('button[type="submit"]');
            submitBtn.disabled = true;
            submitBtn.innerText = 'Đang lưu...';

            const response = await fetch(actionUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(data)
            });

            const result = await response.json();

            if (result.success) {
                console.log('Thêm/Sửa phòng thành công, MaPhong:', result.maPhong);
                alert(result.message || 'Lưu phòng thành công!');
                location.reload();
            } else {
                let errorMessage = result.message || 'Lưu phòng thất bại!';
                if (result.detail && Array.isArray(result.detail)) {
                    errorMessage += ': ' + result.detail.join(', ');
                }
                alert(errorMessage);
            }
        } catch (error) {
            console.error('Lỗi khi lưu phòng:', error);
            alert('Đã xảy ra lỗi khi lưu phòng: ' + error.message);
        } finally {
            const submitBtn = elements.roomForm.querySelector('button[type="submit"]');
            submitBtn.disabled = false;
            submitBtn.innerText = 'Lưu';
        }
    });
    // Xử lý submit form loại phòng
    elements.roomTypeForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const formData = new FormData(elements.roomTypeForm);
        const actionUrl = currentEditingRoomTypeId ? `${window.updateRoomTypeUrl}/${currentEditingRoomTypeId}` : window.addRoomTypeUrl;

        if (currentEditingRoomTypeId) {
            formData.append('MaLoaiPhong', currentEditingRoomTypeId);
        }

        try {
            const response = await fetch(actionUrl, {
                method: 'POST',
                body: formData
            });

            if (response.ok) {
                location.reload();
            } else {
                const errorData = await response.json();
                let errorMessage = errorData.message || 'Không xác định';
                if (errorData.detail && Array.isArray(errorData.detail)) {
                    errorMessage += ': ' + errorData.detail.join(', ');
                }
                alert('Lưu loại phòng thất bại! Chi tiết: ' + errorMessage);
            }
        } catch (error) {
            console.error('Lỗi khi lưu loại phòng:', error);
            alert('Đã xảy ra lỗi khi lưu loại phòng: ' + error.message);
        }
    });

    // Xử lý submit form thiết bị
    elements.equipmentForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const formData = new FormData(elements.equipmentForm);
        const actionUrl = currentEditingEquipmentId ? `${window.updateEquipmentUrl}/${currentEditingEquipmentId}` : window.addEquipmentUrl;

        if (currentEditingEquipmentId) {
            formData.append('MaThietBi', currentEditingEquipmentId);
        }

        try {
            const response = await fetch(actionUrl, {
                method: 'POST',
                body: formData
            });

            if (response.ok) {
                location.reload();
            } else {
                const errorData = await response.json();
                let errorMessage = errorData.message || 'Không xác định';
                if (errorData.detail && Array.isArray(errorData.detail)) {
                    errorMessage += ': ' + errorData.detail.join(', ');
                }
                alert('Lưu thiết bị thất bại! Chi tiết: ' + errorMessage);
            }
        } catch (error) {
            console.error('Lỗi khi lưu thiết bị:', error);
            alert('Đã xảy ra lỗi khi lưu thiết bị: ' + error.message);
        }
    });

    // Tải danh sách loại phòng
    async function loadRoomTypes() {
        try {
            const response = await fetch(window.getRoomTypesUrl);
            const roomTypes = await response.json();
            elements.roomTypeListBody.innerHTML = '';
            roomTypes.forEach((type, index) => {
                const maLoaiPhong = parseInt(type.maLoaiPhong);
                if (isNaN(maLoaiPhong)) {
                    console.error(`Giá trị maLoaiPhong không hợp lệ: ${type.maLoaiPhong}`);
                    return;
                }
                const row = `
                <tr>
                    <td>${index + 1}</td>
                    <td>${maLoaiPhong || 'Không xác định'}</td>
                    <td>${type.tenLoaiPhong || 'Không xác định'}</td>
                    <td>${type.moTa || 'Không có mô tả'}</td>
                    <td>
                        <button class="btn btn-warning btn-sm btn-update-room-type" data-id="${maLoaiPhong}">Sửa</button>
                        <button class="btn btn-danger btn-sm btn-delete-room-type" data-id="${maLoaiPhong}">Xóa</button>
                    </td>
                </tr>`;
                elements.roomTypeListBody.innerHTML += row;
            });

            document.querySelectorAll('.btn-update-room-type').forEach(btn => {
                btn.addEventListener('click', async (e) => {
                    e.preventDefault();
                    const id = btn.getAttribute('data-id');
                    try {
                        const response = await fetch(`/Admin/LoaiPhong/GetById/${id}`);
                        if (!response.ok) {
                            throw new Error(`Lỗi HTTP: ${response.status} - ${response.statusText}`);
                        }
                        const contentType = response.headers.get('content-type');
                        if (!contentType || !contentType.includes('application/json')) {
                            throw new Error('Phản hồi không phải là JSON');
                        }
                        const roomType = await response.json();
                        currentEditingRoomTypeId = roomType.maLoaiPhong;
                        document.getElementById('room-type-name').value = roomType.tenLoaiPhong || '';
                        document.getElementById('room-type-desc').value = roomType.moTa || '';
                        openModal(elements.roomTypeModal, elements.roomTypeModalTitle, 'Sửa loại phòng', elements.roomTypeForm, `${window.updateRoomTypeUrl}/${id}`);
                    } catch (error) {
                        console.error('Lỗi khi lấy dữ liệu loại phòng:', error);
                        alert('Không thể tải dữ liệu loại phòng. Vui lòng thử lại.');
                    }
                });
            });

            document.querySelectorAll('.btn-delete-room-type').forEach(btn => {
                btn.addEventListener('click', (e) => {
                    e.preventDefault();
                    const id = btn.getAttribute('data-id');
                    elements.confirmMessage.textContent = 'Bạn có chắc chắn muốn xóa loại phòng này?';
                    elements.confirmModal.style.display = 'block';

                    elements.btnConfirmDo.onclick = () => {
                        const formData = new FormData();
                        formData.append('id', id);
                        formData.append('__RequestVerificationToken', document.querySelector('input[name="__RequestVerificationToken"]').value);

                        fetch(`/Admin/LoaiPhong/DeleteConfirmed`, {
                            method: 'POST',
                            body: formData
                        })
                            .then(response => {
                                if (response.ok) {
                                    location.reload();
                                } else {
                                    alert('Xóa loại phòng thất bại!');
                                }
                                closeAllModals();
                            })
                            .catch(error => console.error('Lỗi khi xóa loại phòng:', error));
                    };
                });
            });
        } catch (error) {
            console.error('Lỗi khi tải danh sách loại phòng:', error);
            elements.roomTypeListBody.innerHTML = '<tr><td colspan="4">Không thể tải danh sách loại phòng.</td></tr>';
        }
    }

    // Tải danh sách thiết bị
    async function loadEquipments() {
        try {
            const response = await fetch(window.getEquipmentsUrl);
            const equipments = await response.json();
            elements.equipmentListBody.innerHTML = '';
            equipments.forEach((equip, index) => {
                const maThietBi = parseInt(equip.maThietBi);
                if (isNaN(maThietBi)) {
                    console.error(`Giá trị maThietBi không hợp lệ: ${equip.maThietBi}`);
                    return;
                }
                const row = `
                    <tr>
                        <td>${index + 1}</td>
                        <td>${maThietBi || 'Không xác định'}</td>
                        <td>${equip.tenThietBi || 'Không xác định'}</td>
                        <td>${equip.moTa || 'Không có mô tả'}</td>
                        <td>${equip.tinhTrang || 'Không xác định'}</td>
                        <td>${equip.donViTinh || 'Không xác định'}</td>
                        <td>
                            <button class="btn btn-warning btn-sm btn-update-equipment" data-id="${maThietBi}">Sửa</button>
                            <button class="btn btn-danger btn-sm btn-delete-equipment" data-id="${maThietBi}">Xóa</button>
                        </td>
                    </tr>`;
                elements.equipmentListBody.innerHTML += row;
            });

            document.querySelectorAll('.btn-update-equipment').forEach(btn => {
                btn.addEventListener('click', async (e) => {
                    e.preventDefault();
                    const id = btn.getAttribute('data-id');
                    try {
                        const response = await fetch(`/Admin/ThietBi/GetById/${id}`);
                        if (!response.ok) {
                            throw new Error(`Lỗi HTTP: ${response.status} - ${response.statusText}`);
                        }
                        const contentType = response.headers.get('content-type');
                        if (!contentType || !contentType.includes('application/json')) {
                            throw new Error('Phản hồi không phải là JSON');
                        }
                        const equipment = await response.json();
                        currentEditingEquipmentId = equipment.maThietBi;
                        document.getElementById('equipment-name').value = equipment.tenThietBi || '';
                        document.getElementById('equipment-desc').value = equipment.moTa || '';
                        document.getElementById('equipment-unit').value = equipment.donViTinh || ''; // Thêm dòng này
                        openModal(elements.equipmentModal, elements.equipmentModalTitle, 'Sửa thiết bị', elements.equipmentForm, `${window.updateEquipmentUrl}/${id}`);
                    } catch (error) {
                        console.error('Lỗi khi lấy dữ liệu thiết bị:', error);
                        alert('Không thể tải dữ liệu thiết bị. Vui lòng thử lại.');
                    }
                });
            });

            document.querySelectorAll('.btn-delete-equipment').forEach(btn => {
                btn.addEventListener('click', (e) => {
                    e.preventDefault();
                    const id = btn.getAttribute('data-id');
                    elements.confirmMessage.textContent = 'Bạn có chắc chắn muốn xóa thiết bị này?';
                    elements.confirmModal.style.display = 'block';

                    elements.btnConfirmDo.onclick = () => {
                        const formData = new FormData();
                        formData.append('id', id);
                        formData.append('__RequestVerificationToken', document.querySelector('input[name="__RequestVerificationToken"]').value);

                        fetch(`/Admin/ThietBi/DeleteConfirmed`, {
                            method: 'POST',
                            body: formData
                        })
                            .then(response => {
                                if (response.ok) {
                                    location.reload();
                                } else {
                                    alert('Xóa thiết bị thất bại!');
                                }
                                closeAllModals();
                            })
                            .catch(error => console.error('Lỗi khi xóa thiết bị:', error));
                    };
                });
            });
        } catch (error) {
            console.error('Lỗi khi tải danh sách thiết bị:', error);
            elements.equipmentListBody.innerHTML = '<tr><td colspan="4">Không thể tải danh sách thiết bị.</td></tr>';
        }
    }
    //User

    async function loadUserList() {
        try {
            const response = await fetch('/Admin/NguoiDung/GetAll');
            const result = await response.json();

            if (result.success && result.data) {
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
            elements.userListBody.innerHTML = '<tr><td colspan="9" class="text-center">Không thể tải danh sách người dùng.</td></tr>';
        }
    }
    function initializeUserList() {
        loadUserList();
    }

    // Hàm hiển thị danh sách người dùng
    function renderUserList(userList) {
        elements.userListBody.innerHTML = '';
        const totalItems = userList.length;
        const totalPages = Math.ceil(totalItems / userItemsPerPage);
        const startIndex = (currentUserPage - 1) * userItemsPerPage;
        const endIndex = Math.min(startIndex + userItemsPerPage, totalItems);
        const paginatedList = userList.slice(startIndex, endIndex);

        if (paginatedList.length === 0) {
            elements.userListBody.innerHTML = '<tr><td colspan="9" class="text-center">Không tìm thấy người dùng nào.</td></tr>';
        } else {
            paginatedList.forEach(user => {
                const row = `
                <tr>
                    <td>${user.index}</td>
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

        updateUserPagination(totalPages);
        if (elements.totalUserCount) elements.totalUserCount.textContent = totalItems; // Cập nhật tổng số người dùng
        attachUserActions();
    }
    function updateUserPagination(totalPages) {
        elements.userPagination.innerHTML = '';
        elements.userPagination.appendChild(elements.userPrevPage.cloneNode(true));

        for (let i = 1; i <= totalPages; i++) {
            const pageItem = document.createElement('li');
            pageItem.classList.add('page-item');
            if (i === currentUserPage) pageItem.classList.add('active');
            const pageLink = document.createElement('a');
            pageLink.classList.add('page-link');
            pageLink.href = '#';
            pageLink.textContent = i;
            pageLink.addEventListener('click', (e) => {
                e.preventDefault();
                currentUserPage = i;
                renderUserList(originalUserList);
            });
            pageItem.appendChild(pageLink);
            elements.userPagination.appendChild(pageItem);
        }

        elements.userPagination.appendChild(elements.userNextPage.cloneNode(true));

        const prevLink = elements.userPagination.querySelector('#prev-page .page-link');
        const nextLink = elements.userPagination.querySelector('#next-page .page-link');

        elements.userPagination.querySelector('#prev-page').classList.toggle('disabled', currentUserPage === 1);
        elements.userPagination.querySelector('#next-page').classList.toggle('disabled', currentUserPage === totalPages);

        prevLink.addEventListener('click', (e) => {
            e.preventDefault();
            if (currentUserPage > 1) {
                currentUserPage--;
                renderUserList(originalUserList);
            }
        });

        nextLink.addEventListener('click', (e) => {
            e.preventDefault();
            if (currentUserPage < totalPages) {
                currentUserPage++;
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
        currentUserPage = 1;
        renderUserList(filteredUsers);
    }

    // Hàm reset tìm kiếm người dùng
    function resetUserSearch() {
        elements.searchUserInput.value = '';
        elements.resetUserSearchBtn.style.display = 'none';
        currentUserPage = 1;
        renderUserList(originalUserList);
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
                    document.getElementById('user-role').value = user.MaQuyen || '';
                    const isDeleted = user.IsDeleted !== undefined ? user.IsDeleted : false;
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

                    openModal(elements.userModal, document.getElementById('user-modal-title'), 'Sửa người dùng', elements.userForm, `${window.updateUserUrl}/${userId}`);

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

    // Xử lý mở modal Thêm người dùng
    elements.btnAddUser.addEventListener('click', (e) => {
        e.preventDefault();
        // Hiển thị input mật khẩu khi thêm
        const passwordGroup = document.querySelector('#user-password').parentElement;
        passwordGroup.style.display = 'block';
        openModal(elements.userModal, document.getElementById('user-modal-title'), 'Thêm người dùng mới', elements.userForm, window.addUserUrl, true);
    });

    // Xử lý đóng modal người dùng
    elements.userBtnCancel.addEventListener('click', closeAllModals);

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
        const isEditing = actionUrl.includes(window.updateUserUrl);

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
    attachUserActions();

    // Tải danh sách khi trang được tải
    loadRoomTypes();
    loadEquipments();
    loadUserList();
    initializeRoomList();

});
// Hàm lấy danh sách Tỉnh/Thành phố
async function loadProvinces() {
    try {
        const response = await fetch('https://provinces.open-api.vn/api/p/');
        const provinces = await response.json();
        const provinceSelect = document.getElementById('province');
        provinceSelect.innerHTML = '<option value="">Chọn Tỉnh/Thành phố</option>'; 
        provinces.forEach(province => {
            const option = document.createElement('option');
            option.value = province.code;
            option.textContent = province.name;
            provinceSelect.appendChild(option);
        });
    } catch (error) {
        console.error('Lỗi khi tải danh sách tỉnh:', error);
        alert('Không thể tải danh sách tỉnh. Vui lòng thử lại.');
    }
}

// Hàm lấy danh sách Huyện/Quận theo Tỉnh
async function loadDistricts(provinceCode) {
    try {
        const response = await fetch(`https://provinces.open-api.vn/api/p/${provinceCode}?depth=2`);
        const provinceData = await response.json();
        const districtSelect = document.getElementById('district');
        districtSelect.innerHTML = '<option value="">Chọn Huyện/Quận</option>'; // Reset dropdown
        provinceData.districts.forEach(district => {
            const option = document.createElement('option');
            option.value = district.code;
            option.textContent = district.name;
            districtSelect.appendChild(option);
        });
    } catch (error) {
        console.error('Lỗi khi tải danh sách huyện:', error);
        alert('Không thể tải danh sách huyện. Vui lòng thử lại.');
    }
}

// Gọi hàm loadProvinces khi trang tải
document.addEventListener('DOMContentLoaded', () => {
    loadProvinces();

    // Xử lý sự kiện khi chọn Tỉnh/Thành phố
    document.getElementById('province').addEventListener('change', (e) => {
        const provinceCode = e.target.value;
        if (provinceCode) {
            loadDistricts(provinceCode);
        } else {
            const districtSelect = document.getElementById('district');
            districtSelect.innerHTML = '<option value="">Chọn Huyện/Quận</option>';
        }
    });
});

