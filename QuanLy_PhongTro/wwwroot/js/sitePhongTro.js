// Hàm hiển thị toast thông báo
function showToast(message, isSuccess = true) {
    const toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        console.error('Toast container not found!');
        return;
    }

    const toast = document.createElement('div');
    toast.className = `toast ${isSuccess ? 'success' : 'error'}`;
    toast.innerHTML = `
        <span class="toast-icon fas fa-${isSuccess ? 'check-circle' : 'times-circle'}"></span>
        <span class="toast-text">${message}</span>
        <span class="toast-close fas fa-times"></span>
    `;
    toastContainer.appendChild(toast);
    console.log('Toast added to DOM:', toast); // Debug: Kiểm tra xem toast có được thêm không

    // Đảm bảo toast hiển thị ngay lập tức
    toast.style.display = 'flex'; // Đảm bảo toast hiển thị

    toast.querySelector('.toast-close').onclick = () => {
        toast.classList.add('fade-out');
        setTimeout(() => toast.remove(), 500);
    };

    setTimeout(() => {
        toast.classList.add('fade-out');
        setTimeout(() => toast.remove(), 500);
    }, 3000);
}

function toggleDropdown(dropdownId) {
    const dropdown = document.getElementById(dropdownId);
    if (!dropdown) return;

    // Đóng tất cả dropdown trước khi mở dropdown hiện tại
    document.querySelectorAll('.filter-dropdown').forEach(menu => {
        if (menu.id !== dropdownId) {
            menu.classList.remove('show');
        }
    });

    dropdown.classList.toggle('show');
}

document.addEventListener('click', function (event) {
    if (!event.target.closest('.filter-item')) {
        document.querySelectorAll('.filter-dropdown').forEach(menu => {
            menu.classList.remove('show');
        });
    }
});

// Ngăn dropdown đóng khi click bên trong
document.querySelectorAll('.filter-dropdown').forEach(menu => {
    menu.addEventListener('click', function (event) {
        event.stopPropagation();
    });
});

// Reset bộ lọc
function resetSelection(filterType) {
    const inputs = document.querySelectorAll(`input[name="${filterType}"]`);
    inputs.forEach(input => {
        input.checked = filterType === 'roomType' && input.value === 'all';
    });
    console.log(`Reset selection for ${filterType}`);
    fetchFilteredRooms(); // Cập nhật danh sách phòng và filter-title
}

// Áp dụng bộ lọc
function applySelection(filterType) {
    fetchFilteredRooms();
    toggleDropdown(`${filterType}Dropdown`);
    console.log(`Applied ${filterType} filter`);
}

// Áp dụng bộ lọc loại phòng
function applyRoomType() {
    fetchFilteredRooms();
    toggleDropdown('categoryDropdown');
    console.log('Applied room type filter');
}

// Lấy các bộ lọc hiện tại
function getFilters() {
    const roomTypes = Array.from(document.querySelectorAll('input[name="roomType"]:checked'))
        .map(input => input.value)
        .filter(value => value !== 'all');
    const price = document.querySelector('input[name="price"]:checked')?.value || '';
    const area = document.querySelector('input[name="area"]:checked')?.value || '';
    const amenities = Array.from(document.querySelectorAll('input[name="amenities"]:checked'))
        .map(input => input.value);
    const sort = document.getElementById('sort-options').value;

    // Collect current pagination state
    const page = {};
    document.querySelectorAll('.pagination').forEach(pagination => {
        const maLoaiPhong = parseInt(pagination.dataset.loaiPhong);
        if (!isNaN(maLoaiPhong)) {
            // Try to find active page
            const activePage = pagination.querySelector('.active .page-link');
            if (activePage) {
                const currentPage = parseInt(activePage.dataset.page) || 1;
                page[maLoaiPhong] = currentPage;
                console.log(`Current page for loaiPhong ${maLoaiPhong}: ${currentPage}`);
            } else {
                // Default to page 1 if no active page
                page[maLoaiPhong] = 1;
            }
        }
    });

    return {
        roomType: roomTypes.join(','),
        price,
        area,
        amenities: amenities.join(','),
        sort,
        page
    };
}


// Cập nhật tiêu đề bộ lọc (filter-title) dựa trên các giá trị đã chọn và số lượng phòng
function updateFilterTitle(totalRooms) {
    const filters = getFilters();
    const filterTitle = document.getElementById('filter-title');
    let titleParts = [];

    // Xử lý bộ lọc Loại phòng
    if (filters.roomType) {
        const roomTypes = filters.roomType.split(',').map(type => {
            if (type === '1') return 'Phòng trọ';
            if (type === '2') return 'Nguyên căn';
            return type;
        });
        titleParts.push(roomTypes.join(' và '));
    }

    // Xử lý bộ lọc Mức giá
    if (filters.price) {
        let priceText = '';
        switch (filters.price) {
            case 'under-2':
                priceText = 'Dưới 2 triệu';
                break;
            case '2-3':
                priceText = '2 - 3 triệu';
                break;
            case '3-5':
                priceText = '3 - 5 triệu';
                break;
            case '5-7':
                priceText = '5 - 7 triệu';
                break;
            case 'over':
                priceText = 'Trên 7 triệu';
                break;
        }
        if (priceText) titleParts.push(priceText);
    }

    // Xử lý bộ lọc Diện tích
    if (filters.area) {
        let areaText = '';
        switch (filters.area) {
            case 'under-20':
                areaText = 'Dưới 20m²';
                break;
            case '20-30':
                areaText = '20 - 30m²';
                break;
            case '30-50':
                areaText = '30 - 50m²';
                break;
            case 'over-50':
                areaText = 'Trên 50m²';
                break;
        }
        if (areaText) titleParts.push(areaText);
    }

    //thiet bi
    if (filters.amenities) {
        const amenityIds = filters.amenities.split(',');
        const amenityNames = [];

        // Lặp qua tất cả các checkbox amenities đã được chọn
        document.querySelectorAll('input[name="amenities"]:checked').forEach(input => {
            // Tìm thẻ label chứa checkbox này
            const label = input.closest('label.filter-option');
            if (label) {
                // Lấy text của label, loại bỏ khoảng trắng thừa
                const labelText = label.textContent.trim();
                amenityNames.push(labelText);
            }
        });

        if (amenityNames.length > 0) {
            titleParts.push(amenityNames.join(', '));
        }
    }

    // Cập nhật filter-title với số lượng phòng
    if (titleParts.length > 0) {
        filterTitle.textContent = `Tìm thấy ${totalRooms} kết quả: ${titleParts.join(', ')}`;
    } else {
        filterTitle.textContent = `Tìm thấy ${totalRooms} kết quả`;
    }
}

// Gửi yêu cầu AJAX để lấy danh sách phòng đã lọc
async function fetchFilteredRooms() {
    const filters = getFilters();
    console.log("Fetching rooms with filters:", filters);

    try {
        const response = await fetch('/phong-tro/filter', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify(filters)
        });

        if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);

        const result = await response.json();
        console.log('Server response:', result);

        if (result.success) {
            const totalRooms = result.totalCount || 0;
            updateRoomList(result.data || [], result.pagination);
            updateFilterTitle(totalRooms);

            // Only show toast for initial load or filter change, not pagination
            if (!filters.isPageChange) {
                showToast("Đã cập nhật danh sách phòng!", true);
            }
        } else {
            updateRoomList([], null);
            updateFilterTitle(0);
            showToast(result.message || "Hiện tại không tìm thấy phòng phù hợp.", false);
        }
    } catch (error) {
        console.error("Error fetching filtered rooms:", error);
        updateRoomList([], null);
        updateFilterTitle(0);
        showToast("Lỗi kết nối server: " + error.message, false);
    }
}

// Cập nhật giao diện danh sách phòng
function updateRoomList(groupedRooms, pagination) {
    const container = document.querySelector('.container.show-all');
    container.innerHTML = '';

    if (!groupedRooms?.length) {
        container.innerHTML = '<p class="no-rooms-message">Không tìm thấy phòng phù hợp.</p>';
        return;
    }

    groupedRooms.forEach(group => {
        const section = document.createElement('div');
        section.className = 'building-section';
        section.innerHTML = `
            <div class="building-info"><h1>${group.key || 'Không xác định'}</h1></div>
            <div class="room-list"></div>
            <nav aria-label="Room pagination" class="pagination-container">
                <ul class="pagination justify-content-center" data-loai-phong="${group.maLoaiPhong || ''}"></ul>
            </nav>
        `;

        const roomList = section.querySelector('.room-list');
        if (group.rooms && group.rooms.length > 0) {
            group.rooms.forEach(room => {
                const roomCard = document.createElement('div');
                roomCard.className = 'room-card';
                roomCard.dataset.roomId = room.maPhong;
                roomCard.innerHTML = `
                    <div class="room-save-icon" data-room-id="${room.maPhong}">
                        <i class="${room.isSaved ? 'fas' : 'far'} fa-bookmark" id="save-icon-${room.maPhong}"></i>
                    </div>
                    <img class="room-image" src="${room.anhDaiDien ? `/asset/${room.anhDaiDien}` : '/asset/default-room.jpg'}" alt="${room.tenPhong}">
                    <div class="room-content">
                        <div class="room-header"><h2>${room.tenPhong} - ${room.diaChi}</h2></div>
                        <div class="room-price-area">
                            <span class="price">${room.giaThue.toLocaleString('vi-VN')} VNĐ</span>
                            <span class="area">${room.dienTich} m²</span>
                        </div>
                        <div class="room-features">
                            <span><i class="fas fa-${room.maLoaiPhong === 1 ? 'home' : 'building'}"></i> ${room.maLoaiPhong === 1 ? 'Phòng trọ' : 'Nguyên căn'}</span>
                            ${room.thietBis?.slice(0, 3).map(tb => `<span><i class="fas fa-check"></i> ${tb.tenThietBi}</span>`).join('') || ''}
                        </div>
                        <div class="room-divider"></div>
                        <div class="room-poster">
                            <span class="poster-name">Quản lý</span>
                            <span class="room-status ${room.trangThai === 'Còn trống' ? 'available' : 'rented'}">${room.trangThai}</span>
                        </div>
                    </div>
                `;
                roomList.appendChild(roomCard);
            });
        } else {
            roomList.innerHTML = '<p>Không có phòng nào trong nhóm này.</p>';
        }

        // Get pagination data from the group
        const paginationUl = section.querySelector('.pagination');
        const { currentPage = 1, totalPages = 1 } = group.pagination || {};

        console.log(`Building pagination for ${group.key}: currentPage=${currentPage}, totalPages=${totalPages}`);

        // Clear the pagination container
        paginationUl.innerHTML = '';

        // Don't show pagination if there's only one page
        if (totalPages <= 1) {
            section.querySelector('.pagination-container').style.display = 'none';
        } else {
            section.querySelector('.pagination-container').style.display = 'block';

            // Previous button
            const prevItem = document.createElement('li');
            prevItem.className = `page-item ${currentPage <= 1 ? 'disabled' : ''}`;
            prevItem.innerHTML = `<a class="page-link" href="#" data-page="${Math.max(1, currentPage - 1)}">Trước</a>`;
            paginationUl.appendChild(prevItem);

            // Page numbers
            // Show at most 5 page numbers (current, 2 before, 2 after) if there are many pages
            let startPage = Math.max(1, currentPage - 2);
            let endPage = Math.min(totalPages, startPage + 4);

            // Adjust startPage if endPage is limited
            if (endPage - startPage < 4) {
                startPage = Math.max(1, endPage - 4);
            }

            for (let i = startPage; i <= endPage; i++) {
                const pageItem = document.createElement('li');
                pageItem.className = `page-item ${i === currentPage ? 'active' : ''}`;
                pageItem.innerHTML = `<a class="page-link" href="#" data-page="${i}">${i}</a>`;
                paginationUl.appendChild(pageItem);
            }

            // Next button
            const nextItem = document.createElement('li');
            nextItem.className = `page-item ${currentPage >= totalPages ? 'disabled' : ''}`;
            nextItem.innerHTML = `<a class="page-link" href="#" data-page="${Math.min(totalPages, currentPage + 1)}">Sau</a>`;
            paginationUl.appendChild(nextItem);
        }

        container.appendChild(section);
    });

    // Make sure to bind events after updating the DOM
    rebindEvents();
}

// Hàm xử lý sự kiện click phân trang
async function handlePageClick(e) {
    e.preventDefault();
    e.stopPropagation(); // Prevent event bubbling

    console.log('handlePageClick triggered, target:', e.target);

    // Get clicked page number
    const page = parseInt(e.target.dataset.page);

    // Find the closest pagination element and get loaiPhong value
    const paginationElement = e.target.closest('.pagination');
    const maLoaiPhong = parseInt(paginationElement.dataset.loaiPhong);

    console.log('Pagination click - Page:', page, 'maLoaiPhong:', maLoaiPhong);

    if (!isNaN(page) && !isNaN(maLoaiPhong)) {
        // Get current filters
        const filters = getFilters();

        // Initialize page object if needed
        filters.page = filters.page || {};

        // Update page for this specific maLoaiPhong
        filters.page[maLoaiPhong] = page;

        console.log('Updated filters with page info:', filters);

        try {
            // Send request to server with updated page information
            const response = await fetch('/phong-tro/filter', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify(filters)
            });

            if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);

            const result = await response.json();
            console.log('Server pagination response:', result);

            if (result.success) {
                const totalRooms = result.totalCount || 0;
                updateRoomList(result.data || [], result.pagination);
                updateFilterTitle(totalRooms);

                // Scroll to top of room section for better UX
                const roomSection = document.querySelector('.building-section');
                if (roomSection) {
                    roomSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
                }
            } else {
                showToast(result.message || "Không thể tải trang phòng", false);
            }
        } catch (error) {
            console.error("Pagination error:", error);
            showToast("Lỗi khi chuyển trang: " + error.message, false);
        }
    } else {
        console.error('Invalid page or maLoaiPhong:', { page, maLoaiPhong });
        showToast("Lỗi định dạng trang", false);
    }
}

// Đồng bộ trạng thái lưu
async function syncSavedStatus() {
    try {
        const response = await fetch('/phong-tro/danh-sach-luu-phong');
        if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);

        const result = await response.json();
        if (!result.success) throw new Error(result.message || 'Failed to fetch saved rooms');

        const savedRoomIds = result.data.map(r => r.MaPhong);
        document.querySelectorAll('.room-save-icon').forEach(icon => {
            const roomId = parseInt(icon.dataset.roomId);
            const iconElement = icon.querySelector('i');
            iconElement.classList.replace('fas', 'far');
            if (savedRoomIds.includes(roomId)) iconElement.classList.replace('far', 'fas');
        });
    } catch (error) {
        console.error("Lỗi đồng bộ trạng thái:", error);
    }
}

async function toggleSave(event, roomId) {
    event.preventDefault();
    event.stopPropagation();

    const saveIcon = event.currentTarget.querySelector('i');
    const isCurrentlySaved = saveIcon.classList.contains('fas');
    saveIcon.classList.replace('fa-bookmark', 'fa-spinner');
    saveIcon.classList.add('fa-spin');

    try {
        const response = await fetch(`/phong-tro/luu-phong/${roomId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            }
        });

        if (response.status === 401) {
            window.location.href = `/DangNhap?returnUrl=${encodeURIComponent(window.location.pathname)}`;
            return;
        }

        if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);

        const data = await response.json();
        saveIcon.classList.remove('fa-spinner', 'fa-spin');
        saveIcon.classList.add('fa-bookmark', data.isSaved ? 'fas' : 'far');


        if (typeof data.count !== 'undefined') {
            updateSavedRoomsCounter(data.count);
        }

        // Show notification
        if (typeof showToast === 'function') {
            showToast(data.message, data.success);
        }
    } catch (error) {
        saveIcon.classList.remove('fa-spinner', 'fa-spin');
        saveIcon.classList.add('fa-bookmark', isCurrentlySaved ? 'fas' : 'far');
        showToast("Lỗi kết nối server: " + error.message, false);
    }
}

async function loadSavedRoomsCount() {
    try {
        const response = await fetch('/luu-phong/count');
        const data = await response.json();

        if (data.success) {
            updateSavedRoomsCounter(data.count);
        }
    } catch (error) {
        console.error('Lỗi khi tải số lượng phòng đã lưu:', error);
    }
}

// Update counter ở tất cả vị trí
function updateSavedRoomsCounter(count) {
    savedRoomsCount = count;

    // Update floating button counter
    const floatingCounter = document.getElementById('savedCounter');
    if (floatingCounter) {
        floatingCounter.textContent = count > 99 ? '99+' : count;
        floatingCounter.classList.add('counter-animation');

        setTimeout(() => {
            floatingCounter.classList.remove('counter-animation');
        }, 400);
    }

    // Update navigation counter
    const navCounter = document.getElementById('savedCounterNav');
    if (navCounter) {
        navCounter.textContent = count > 99 ? '99+' : count;
        navCounter.style.display = count > 0 ? 'inline' : 'none';
    }

    // Update floating button style
    const floatingBtn = document.getElementById('savedRoomsFloatingBtn');
    if (floatingBtn) {
        if (count === 0) {
            floatingBtn.classList.add('empty');
        } else {
            floatingBtn.classList.remove('empty');
        }
    }
}

// Xem chi tiết phòng
function viewRoomDetail(roomId) {
    window.location.href = `/phong-tro/chi-tiet/${roomId}`;
}

// Gắn lại sự kiện sau khi tải AJAX
function rebindEvents() {
    // Handle room card clicks
    document.querySelectorAll('.room-card').forEach(card => {
        card.onclick = e => {
            if (!e.target.closest('.room-save-icon')) {
                viewRoomDetail(card.dataset.roomId);
            }
        };
    });

    // Handle bookmark/save clicks
    document.querySelectorAll('.room-save-icon').forEach(icon => {
        icon.onclick = e => toggleSave(e, icon.dataset.roomId);
    });

    // Ensure all pagination links have proper event handlers
    document.querySelectorAll('.pagination .page-link').forEach(link => {
        // Remove any existing handlers to prevent duplicates
        link.removeEventListener('click', handlePageClick);
        // Add new handler
        link.addEventListener('click', handlePageClick);
        console.log('Pagination event attached to:', link.textContent, 'Page:', link.dataset.page);
    });
}
// Gắn sự kiện khi tải trang
document.addEventListener('DOMContentLoaded', () => {
    const urlParams = new URLSearchParams(window.location.search);
    const successMessage = urlParams.get('success');
    const errorMessage = urlParams.get('error');

    if (successMessage) {
        try {
            const decodedMessage = decodeURIComponent(successMessage);
            showToast(decodedMessage, true);
        } catch (e) {
            console.error('Error decoding success message:', e);
            showToast('Lỗi khi hiển thị thông báo thành công', false);
        }
    } else if (errorMessage) {
        try {
            const decodedMessage = decodeURIComponent(errorMessage);
            showToast(decodedMessage, false);
        } catch (e) {
            console.error('Error decoding error message:', e);
            showToast('Lỗi khi hiển thị thông báo lỗi', false);
        }
    }
    const searchBtn = document.querySelector('.search-btn');
    if (searchBtn) {
        searchBtn.addEventListener('click', fetchFilteredRooms);
    }

    const sortOptions = document.getElementById('sort-options');
    if (sortOptions) {
        sortOptions.addEventListener('change', fetchFilteredRooms);
    }

    // Đóng dropdown khi click bên ngoài
    document.addEventListener('click', e => {
        if (!e.target.closest('.dropdown')) {
            document.querySelectorAll('.dropdown-menu').forEach(menu => menu.classList.remove('show'));
            document.querySelectorAll('.dropdown').forEach(dropdown => dropdown.classList.remove('active'));
        }
    });

    // Ngăn sự kiện click trong dropdown lan ra ngoài
    document.querySelectorAll('.dropdown-menu').forEach(menu => {
        menu.addEventListener('click', e => e.stopPropagation());
    });

    syncSavedStatus();
    rebindEvents();
    loadSavedRoomsCount();

    // Tải danh sách phòng ban đầu
    fetchFilteredRooms();
});