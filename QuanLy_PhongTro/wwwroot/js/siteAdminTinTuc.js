document.addEventListener('DOMContentLoaded', function () {
    const newsListBody = document.getElementById('news-list-body');
    const searchNewsInput = document.getElementById('search-news-input');
    const searchNewsBtn = document.getElementById('search-news-btn');
    const resetNewsSearchBtn = document.getElementById('reset-news-search-btn');
    const btnAddNews = document.getElementById('btn-add-news');
    const newsModal = document.getElementById('news-modal');
    const newsForm = document.getElementById('news-form');
    const newsModalTitle = document.getElementById('news-modal-title');
    const newsIdInput = document.getElementById('news-id');
    const newsTitleInput = document.getElementById('news-title');
    const newsTypeSelect = document.getElementById('news-type');
    const newsUserSelect = document.getElementById('news-user');
    const newsImageInput = document.getElementById('news-image');
    const newsImagePreview = document.getElementById('news-image-preview');
    const newsDescInput = document.getElementById('news-desc');
    const newsBtnCancel = document.getElementById('news-btn-cancel');
    const newsPrevPage = document.getElementById('news-prev-page');
    const newsNextPage = document.getElementById('news-next-page');
    const totalNewsCount = document.getElementById('total-news-count');
    const newsItemsPerPageSpan = document.getElementById('news-items-per-page');
    const confirmModal = document.getElementById('confirm-modal');
    const confirmMessage = document.getElementById('confirm-message');
    const btnConfirmCancel = document.getElementById('btn-confirm-cancel');
    const btnConfirmDo = document.getElementById('btn-confirm-do');

    let currentPage = 1;
    let itemsPerPage = parseInt(newsItemsPerPageSpan.textContent);
    let newsData = [];
    let filteredNewsData = [];

    // URLs for API endpoints
    const getNewsUrl = `${window.location.origin}/Admin/TinTuc/GetAll`;
    const getNewsByIdUrl = `${window.location.origin}/Admin/TinTuc/GetById`;
    const addNewsUrl = `${window.location.origin}/Admin/TinTuc/Add`;
    const updateNewsUrl = `${window.location.origin}/Admin/TinTuc/Update`;
    const deleteNewsUrl = `${window.location.origin}/Admin/TinTuc/Delete`;
    const getLoaiTinTucsUrl = `${window.location.origin}/Admin/TinTuc/GetLoaiTinTucs`;
    const getUsersUrl = window.getUsersUrl;

    // Load news types and users for dropdowns
    async function loadDropdowns() {
        try {
            const [newsTypesResponse, usersResponse] = await Promise.all([
                fetch(getLoaiTinTucsUrl),
                fetch(getUsersUrl)
            ]);

            if (!newsTypesResponse.ok) {
                throw new Error(`Lỗi khi tải loại tin tức: ${newsTypesResponse.statusText}`);
            }
            if (!usersResponse.ok) {
                throw new Error(`Lỗi khi tải người dùng: ${usersResponse.statusText}`);
            }

            const newsTypes = await newsTypesResponse.json();
            const users = await usersResponse.json();

            console.log('News Types:', newsTypes);
            console.log('Users:', users);

            if (!Array.isArray(users.data)) {
                throw new Error('Dữ liệu người dùng trong users.data không phải là mảng: ' + JSON.stringify(users));
            }

            // Xóa và thêm option cho newsTypeSelect
            newsTypeSelect.innerHTML = '<option value="">Chọn loại tin tức</option>';
            newsTypes.forEach(type => {
                const option = document.createElement('option');
                option.value = type.maLoaiTinTuc;
                option.textContent = type.tenLoaiTinTuc;
                newsTypeSelect.appendChild(option);
            });

            // Xóa và thêm option cho newsUserSelect
            newsUserSelect.innerHTML = '<option value="">Chọn người dùng</option>';
            users.data.forEach(user => {
                const option = document.createElement('option');
                option.value = user.maNguoiDung;
                option.textContent = `${user.hoTen} (Email: ${user.email})`;
                newsUserSelect.appendChild(option);
            });
        } catch (error) {
            console.error('Error loading dropdowns:', error);
            alert('Lỗi khi tải dữ liệu dropdown: ' + error.message);
            newsTypeSelect.innerHTML = '<option value="">Chưa tải được loại tin tức</option>';
            newsUserSelect.innerHTML = '<option value="">Chưa tải được người dùng</option>';
        }
    }

    // Load news data
    async function loadNews(page = 1, searchQuery = '') {
        try {
            const response = await fetch(getNewsUrl);
            newsData = await response.json();
            filteredNewsData = searchQuery
                ? newsData.filter(news =>
                    news.tieuDe.toLowerCase().includes(searchQuery.toLowerCase()) ||
                    news.loaiTinTuc.toLowerCase().includes(searchQuery.toLowerCase())
                )
                : newsData;

            totalNewsCount.textContent = filteredNewsData.length;
            const start = (page - 1) * itemsPerPage;
            const end = start + itemsPerPage;
            const paginatedData = filteredNewsData.slice(start, end);

            renderNewsList(paginatedData);
            renderPagination(filteredNewsData.length, page);
        } catch (error) {
            console.error('Error loading news:', error);
            newsListBody.innerHTML = '<tr><td colspan="10" class="text-center">Không thể tải dữ liệu.</td></tr>';
        }
    }

    // Render news list
    function renderNewsList(data) {
        newsListBody.innerHTML = '';
        if (data.length === 0) {
            newsListBody.innerHTML = '<tr><td colspan="10" class="text-center">Không có tin tức nào để hiển thị.</td></tr>';
            return;
        }

        data.forEach((news, index) => {
            const row = document.createElement('tr');
            const words = news.moTa.split(' ');
            const shortDescription = words.length > 20 ? words.slice(0, 20).join(' ') + '...' : news.moTa;
            row.innerHTML = `
                <td>${(currentPage - 1) * itemsPerPage + index + 1}</td>
                <td>${news.maTinTuc}</td>
                <td>${news.tieuDe}</td>
                <td>${news.loaiTinTuc}</td>
                <td>${news.nguoiDung}</td>
                <td>${news.ngayDang}</td>
                <td>${news.luotXem}</td>
                <td>
                    ${news.duongDanHinhAnh ? `<img src="/asset/${news.duongDanHinhAnh}" alt="Hình ảnh" class="anh-dai-dien" style="width: 100px;" />` : 'Không có ảnh'}
                </td>
                <td>${shortDescription}</td>
                <td>
                    <button type="button" class="btn btn-warning btn-sm btn-update-news" data-id="${news.maTinTuc}">Sửa</button>
                    <button type="button" class="btn btn-danger btn-sm btn-delete-news" data-id="${news.maTinTuc}">Xóa</button>
                </td>
            `;
            newsListBody.appendChild(row);
        });
    }

    // Render pagination
    function renderPagination(totalItems, currentPage) {
        const totalPages = Math.ceil(totalItems / itemsPerPage);
        const pagination = document.querySelector('.news-section .pagination');
        pagination.innerHTML = `
            <li class="page-item ${currentPage === 1 ? 'disabled' : ''}" id="news-prev-page">
                <a class="page-link" href="#">Previous</a>
            </li>
        `;

        for (let i = 1; i <= totalPages; i++) {
            pagination.innerHTML += `
                <li class="page-item ${i === currentPage ? 'active' : ''}">
                    <a class="page-link" href="#" data-page="${i}">${i}</a>
                </li>
            `;
        }

        pagination.innerHTML += `
            <li class="page-item ${currentPage === totalPages ? 'disabled' : ''}" id="news-next-page">
                <a class="page-link" href="#">Next</a>
            </li>
        `;
    }

    // Open modal for adding/editing news
    function openNewsModal(mode = 'add', news = null) {
        newsModalTitle.textContent = mode === 'add' ? 'Thêm tin tức mới' : 'Sửa tin tức';
        newsForm.reset();

        // **FIXED**: Không set MaTinTuc khi thêm mới
        if (mode === 'edit' && news) {
            newsIdInput.value = news.maTinTuc;
        } else {
            newsIdInput.value = ''; // Để trống khi thêm mới
        }

        newsTitleInput.value = news ? news.tieuDe : '';
        newsTypeSelect.value = news ? news.maLoaiTinTuc : '';
        newsUserSelect.value = news ? news.maNguoiDung : '';
        newsDescInput.value = news ? news.moTa : '';
        newsImagePreview.innerHTML = news && news.duongDanHinhAnh ? `<img src="/asset/${news.duongDanHinhAnh}" alt="Preview" style="max-width: 200px;" />` : '';
        newsModal.style.display = 'block';
    }

    // Handle form submission
    newsForm.addEventListener('submit', async function (e) {
        e.preventDefault();
        const formData = new FormData();

        // Lấy giá trị từ các trường
        const tieuDe = newsTitleInput.value.trim();
        const moTa = newsDescInput.value.trim();
        const maLoaiTinTuc = newsTypeSelect.value;
        const maNguoiDung = newsUserSelect.value;
        const imageFile = newsImageInput.files[0];
        const isEdit = newsIdInput.value !== '';

        // Kiểm tra các trường bắt buộc
        if (!tieuDe || !moTa || !maLoaiTinTuc || !maNguoiDung) {
            alert('Vui lòng điền đầy đủ thông tin: Tiêu đề, Mô tả, Loại tin tức, và Người dùng!');
            return;
        }

        // **FIXED**: Chỉ thêm MaTinTuc khi edit
        if (isEdit) {
            formData.append('MaTinTuc', newsIdInput.value);
        }

        // Gán giá trị vào FormData
        formData.append('TieuDe', tieuDe);
        formData.append('MoTa', moTa);
        formData.append('MaLoaiTinTuc', maLoaiTinTuc);
        formData.append('MaNguoiDung', maNguoiDung);

        // **FIXED**: Xử lý hình ảnh - bắt buộc phải có ảnh khi thêm mới
        if (imageFile) {
            formData.append('imageFile', imageFile);
        } else if (!isEdit) {
            // Khi thêm mới mà không có ảnh, yêu cầu người dùng chọn ảnh
            alert('Vui lòng chọn hình ảnh cho tin tức!');
            return;
        }

        // **FIXED**: URL và method khác nhau cho Add và Update
        let url, method;
        if (isEdit) {
            url = `${updateNewsUrl}/${newsIdInput.value}`;
            method = 'POST';
        } else {
            url = addNewsUrl;
            method = 'POST';
        }

        try {
            const response = await fetch(url, {
                method: method,
                body: formData
            });

            if (response.ok) {
                newsModal.style.display = 'none';
                loadNews(currentPage, searchNewsInput.value);
                alert(isEdit ? 'Cập nhật tin tức thành công!' : 'Thêm tin tức thành công!');
            } else {
                const errorText = await response.text();
                console.error('Server response:', errorText);

                // Cố gắng parse JSON để hiển thị lỗi chi tiết
                try {
                    const errorObj = JSON.parse(errorText);
                    let errorMessage = 'Lỗi validation:\n';
                    for (const [field, errors] of Object.entries(errorObj)) {
                        errorMessage += `${field}: ${errors.join(', ')}\n`;
                    }
                    alert(errorMessage);
                } catch {
                    alert('Lỗi khi lưu tin tức: ' + errorText);
                }
            }
        } catch (error) {
            console.error('Error saving news:', error);
            alert('Lỗi khi lưu tin tức: ' + error.message);
        }
    });

    // Handle image preview
    newsImageInput.addEventListener('change', function () {
        newsImagePreview.innerHTML = '';
        const file = this.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = function (e) {
                const img = document.createElement('img');
                img.src = e.target.result;
                img.style.maxWidth = '200px';
                newsImagePreview.appendChild(img);
            };
            reader.readAsDataURL(file);
        }
    });

    // Event listeners
    btnAddNews.addEventListener('click', () => openNewsModal('add'));
    newsBtnCancel.addEventListener('click', () => newsModal.style.display = 'none');
    newsModal.querySelector('.close').addEventListener('click', () => newsModal.style.display = 'none');

    newsListBody.addEventListener('click', async function (e) {
        if (e.target.classList.contains('btn-update-news')) {
            const id = e.target.dataset.id;
            try {
                const response = await fetch(`${getNewsByIdUrl}?id=${id}`);
                const news = await response.json();
                openNewsModal('edit', news);
            } catch (error) {
                console.error('Error fetching news:', error);
                alert('Lỗi khi lấy thông tin tin tức.');
            }
        } else if (e.target.classList.contains('btn-delete-news')) {
            const id = e.target.dataset.id;
            confirmMessage.textContent = 'Bạn có chắc chắn muốn xóa tin tức này?';
            confirmModal.style.display = 'block';
            btnConfirmDo.onclick = async function () {
                try {
                    const response = await fetch(`${deleteNewsUrl}?id=${id}`, { method: 'POST' });
                    if (response.ok) {
                        confirmModal.style.display = 'none';
                        loadNews(currentPage, searchNewsInput.value);
                        alert('Xóa tin tức thành công!');
                    } else {
                        alert('Lỗi khi xóa tin tức.');
                    }
                } catch (error) {
                    console.error('Error deleting news:', error);
                    alert('Lỗi khi xóa tin tức.');
                }
            };
        }
    });

    btnConfirmCancel.addEventListener('click', () => confirmModal.style.display = 'none');

    searchNewsBtn.addEventListener('click', () => {
        currentPage = 1; // Reset về trang đầu khi search
        loadNews(1, searchNewsInput.value);
        resetNewsSearchBtn.style.display = 'inline-block';
    });

    resetNewsSearchBtn.addEventListener('click', () => {
        searchNewsInput.value = '';
        resetNewsSearchBtn.style.display = 'none';
        currentPage = 1; // Reset về trang đầu
        loadNews(1);
    });

    document.querySelector('.news-section .pagination').addEventListener('click', function (e) {
        e.preventDefault();
        if (e.target.classList.contains('page-link')) {
            const page = e.target.dataset.page || (e.target.parentElement.id === 'news-prev-page' ? currentPage - 1 : currentPage + 1);
            if (page) {
                currentPage = parseInt(page);
                loadNews(currentPage, searchNewsInput.value);
            }
        }
    });

    // Initialize
    loadDropdowns();
    loadNews();
});