    document.addEventListener('DOMContentLoaded', function () {
        // Base URL for API calls
        const baseUrl = 'http://localhost:5181';

        // URLs for API endpoints
        window.getAllSuCosUrl = `${baseUrl}/Admin/SuCo/GetAll`;
        window.updateSuCoUrl = `${baseUrl}/Admin/SuCo/Update`;
        window.getMediaUrl = `${baseUrl}/Admin/SuCo/GetMedia`;

        // Elements
        const issuesListBody = document.getElementById('issues-list-body');
        const mediaModal = document.getElementById('media-modal');
        const mediaBtnCancel = document.getElementById('media-btn-cancel');
        const mediaContainer = document.getElementById('media-container');
        const updateIssueModal = document.getElementById('update-issue-modal');
        const updateIssueForm = document.getElementById('update-issue-form');
        const updateIssueIdInput = document.getElementById('update-issue-id');
        const updateIssueStatusSelect = document.getElementById('update-issue-status');
        const updateIssueBtnCancel = document.getElementById('update-issue-btn-cancel');
        const searchIssuesInput = document.getElementById('search-issues-input');
        const searchIssuesBtn = document.getElementById('search-issues-btn');
        const resetIssuesSearchBtn = document.getElementById('reset-issues-search-btn');
        const sortLatestBtn = document.getElementById('sort-latest');
        const sortEarliestBtn = document.getElementById('sort-earliest');
        const filterAllBtn = document.getElementById('filter-all');
        const filterUnresolvedBtn = document.getElementById('filter-unresolved');
        const filterResolvedBtn = document.getElementById('filter-resolved');

        let sortOrder = 'latest';
        let statusFilter = 'all';
        let searchQuery = '';

        // Function to load issues
        // Function to load issues
        function loadIssues() {
            const url = new URL(window.getAllSuCosUrl);
            url.searchParams.append('sortOrder', sortOrder);
            url.searchParams.append('statusFilter', statusFilter);
            if (searchQuery) {
                url.searchParams.append('searchQuery', searchQuery);
            }

            console.log('Request Data for loadIssues:', {
                url: url.toString(),
                sortOrder: sortOrder,
                statusFilter: statusFilter,
                searchQuery: searchQuery
            });

            fetch(url, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                }
            })
                .then(response => response.json())
                .then(data => {
                    console.log('Response Data from loadIssues:', data); // Log dữ liệu trả về từ API
                    if (data.success) {
                        renderIssues(data.data);
                    } else {
                        issuesListBody.innerHTML = '<tr><td colspan="9" class="text-center">Không có sự cố nào để hiển thị.</td></tr>';
                    }
                })
                .catch(error => {
                    console.error('Error loading issues:', error);
                    issuesListBody.innerHTML = '<tr><td colspan="9" class="text-center">Có lỗi xảy ra khi tải dữ liệu.</td></tr>';
                });
        }

        // Function to render issues in the table
        function renderIssues(suCos) {
            issuesListBody.innerHTML = '';
            if (suCos.length === 0) {
                issuesListBody.innerHTML = '<tr><td colspan="9" class="text-center">Không có sự cố nào để hiển thị.</td></tr>';
                return;
            }

            console.log('Rendering data:', suCos);
            suCos.forEach((item, index) => {
                const row = document.createElement('tr');
                row.innerHTML = `
                <td>${index + 1}</td>
                <td>${item.tenPhong ?? 'N/A'}</td> 
                <td>${item.hoTen ?? 'N/A'}</td>
                <td>${item.LoaiSuCo || item.loaiSuCo}</td>
                <td>${item.MoTa || item.moTa}</td>
                <td>${item.MucDo || item.mucDo}</td>
                <td>${item.TrangThai || item.trangThai}</td>    
                <td>${new Date(item.NgayTao || item.ngayTao).toLocaleString('vi-VN', {
                    day: '2-digit',
                    month: '2-digit',
                    year: 'numeric',
                    hour: '2-digit',
                    minute: '2-digit',
                    second: '2-digit'
                })}</td>
                <td>
                    <button type="button" class="btn btn-info btn-sm btn-view-media" data-id="${item.Id || item.id}">Xem Media</button>
                    <button type="button" class="btn btn-warning btn-sm btn-update-issue" data-id="${item.Id || item.id}" data-status="${item.TrangThai || item.trangThai}">Cập nhật</button>
                </td>
            `;
                issuesListBody.appendChild(row);
            });

            // Re-attach event listeners for view media and update buttons
            attachViewMediaEvents();
            attachUpdateIssueEvents();
        }

        // Function to attach view media events
        function attachViewMediaEvents() {
            const viewMediaButtons = document.querySelectorAll('.btn-view-media');
            viewMediaButtons.forEach(button => {
                button.addEventListener('click', function () {
                    const suCoId = this.getAttribute('data-id');
                    fetch(`${window.getMediaUrl}?id=${suCoId}`, {
                        method: 'GET',
                        headers: {
                            'Content-Type': 'application/json',
                            'X-Requested-With': 'XMLHttpRequest'
                        }
                    })
                        .then(response => response.json())
                        .then(data => {
                            if (data.success) {
                                displayMedia(data.mediaUrls);
                                mediaModal.style.display = 'block';
                            } else {
                                alert('Không tìm thấy media: ' + (data.message || 'Lỗi không xác định'));
                            }
                        })
                        .catch(error => {
                            console.error('Error fetching media:', error);
                            alert('Có lỗi xảy ra khi tải media.');
                        });
                });
            });
        }

        // Function to display media
        function displayMedia(mediaUrls) {
            mediaContainer.innerHTML = '';
            if (!mediaUrls || mediaUrls.length === 0) {
                mediaContainer.innerHTML = '<p>Không có media để hiển thị.</p>';
                return;
            }

            mediaUrls.forEach(url => {
                if (url.endsWith('.jpg') || url.endsWith('.jpeg') || url.endsWith('.png')) {
                    const img = document.createElement('img');
                    img.src = `/asset/uploads/${url}`;
                    mediaContainer.appendChild(img);
                } else if (url.endsWith('.mp4')) {
                    const video = document.createElement('video');
                    video.controls = true;
                    video.src = `/asset/uploads/${url}`;
                    mediaContainer.appendChild(video);
                }
            });
        }

        // Function to attach update issue events
        function attachUpdateIssueEvents() {
            const updateIssueButtons = document.querySelectorAll('.btn-update-issue');
            updateIssueButtons.forEach(button => {
                button.addEventListener('click', function () {
                    const suCoId = this.getAttribute('data-id');
                    const currentStatus = this.getAttribute('data-status');
                    updateIssueIdInput.value = suCoId;
                    updateIssueStatusSelect.value = currentStatus;
                    updateIssueModal.style.display = 'block';
                });
            });
        }

        // Search functionality
        searchIssuesBtn.addEventListener('click', function () {
            searchQuery = searchIssuesInput.value.trim();
            resetIssuesSearchBtn.style.display = searchQuery ? 'inline-block' : 'none';
            loadIssues();
        });

        resetIssuesSearchBtn.addEventListener('click', function () {
            searchIssuesInput.value = '';
            searchQuery = '';
            resetIssuesSearchBtn.style.display = 'none';
            loadIssues();
        });

        // Filter and sort functionality
        function updateFilterButtons(activeBtn) {
            [filterAllBtn, filterUnresolvedBtn, filterResolvedBtn].forEach(btn => btn.classList.remove('active'));
            activeBtn.classList.add('active');
        }

        function updateSortButtons(activeBtn) {
            [sortLatestBtn, sortEarliestBtn].forEach(btn => btn.classList.remove('active'));
            activeBtn.classList.add('active');
        }

        sortLatestBtn.addEventListener('click', function () {
            sortOrder = 'latest';
            updateSortButtons(sortLatestBtn);
            loadIssues();
        });

        sortEarliestBtn.addEventListener('click', function () {
            sortOrder = 'earliest';
            updateSortButtons(sortEarliestBtn);
            loadIssues();
        });

        filterAllBtn.addEventListener('click', function () {
            statusFilter = 'all';
            updateFilterButtons(filterAllBtn);
            loadIssues();
        });

        filterUnresolvedBtn.addEventListener('click', function () {
            statusFilter = 'Chưa xử lý';
            updateFilterButtons(filterUnresolvedBtn);
            loadIssues();
        });

        filterResolvedBtn.addEventListener('click', function () {
            statusFilter = 'Đã xử lý';
            updateFilterButtons(filterResolvedBtn);
            loadIssues();
        });

        // Media modal close events
        mediaBtnCancel.addEventListener('click', function () {
            mediaModal.style.display = 'none';
            mediaContainer.innerHTML = '';
        });

        window.addEventListener('click', function (event) {
            if (event.target === mediaModal) {
                mediaModal.style.display = 'none';
                mediaContainer.innerHTML = '';
            }
        });

        // Update issue modal close events
        updateIssueBtnCancel.addEventListener('click', function () {
            updateIssueModal.style.display = 'none';
        });

        window.addEventListener('click', function (event) {
            if (event.target === updateIssueModal) {
                updateIssueModal.style.display = 'none';
            }
        });

        // Handle update issue form submission
        updateIssueForm.addEventListener('submit', function (e) {
            e.preventDefault();
            const formData = new FormData(updateIssueForm);
            const data = {
                Id: parseInt(formData.get('Id')),
                TrangThai: formData.get('TrangThai')
            };

            fetch(window.updateSuCoUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },      
                body: JSON.stringify(data)
            })
                .then(response => response.json())
                .then(result => {
                    if (result.success) {
                        updateIssueModal.style.display = 'none';
                        loadIssues();
                    } else {
                        alert('Cập nhật thất bại: ' + (result.message || 'Lỗi không xác định'));
                    }
                })
                .catch(error => {
                    console.error('Error updating issue:', error);
                    alert('Có lỗi xảy ra khi cập nhật trạng thái.');
                });
        });

        // Initial load
        loadIssues();
    }); 