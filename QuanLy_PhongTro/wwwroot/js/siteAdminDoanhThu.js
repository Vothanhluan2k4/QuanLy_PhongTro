document.addEventListener('DOMContentLoaded', () => {
    let columnChart, pieChart;
    let isLoading = false; // Biến cờ để kiểm soát việc gọi loadCharts

    async function fetchRevenueData(startDate, endDate, method) {
        const validMethods = ['revenue-booking', 'revenue-receipt'];
        if (!validMethods.includes(method)) {
            console.warn(`Phương thức không hợp lệ: ${method}. Mặc định về revenue-booking.`);
            method = 'revenue-booking';
        }

        const url = `/Admin/DoanhThu?startDate=${startDate}&endDate=${endDate}&method=${method}`;
        console.log(`Đang lấy dữ liệu từ: ${url}`);

        try {
            const response = await fetch(url, { credentials: 'include' });
            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Lỗi HTTP! Trạng thái: ${response.status}, Phản hồi: ${errorText}`);
            }
            const data = await response.json();
            console.log('Dữ liệu nhận được từ server:', data);
            return data;
        } catch (error) {
            console.error('Lỗi khi lấy dữ liệu:', error);
            return { revenue: {}, chartData: [], roomTypeData: [] };
        }
    }

    function updateRevenueDisplay(data) {
        const revenue = data.revenue || {};
        document.getElementById('total-customers').value = revenue.totalCustomers || 0;
        document.getElementById('total-rooms').value = revenue.totalRooms || 0;
        document.getElementById('total-contracts').value = revenue.totalContracts || 0;
    }

    function updateColumnChart(chartData, method) {
        const ctx = document.getElementById('column-data').getContext('2d');
        if (columnChart) columnChart.destroy();

        // Cố định kích thước canvas
        ctx.canvas.width = 600;
        ctx.canvas.height = 400;

        const labels = chartData.map(item => item.label);
        const values = chartData.map(item => item.value);

        columnChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: method === 'revenue-booking' ? 'Tổng tiền cọc hợp đồng (VND)' : 'Tổng tiền hóa đơn (VND)',
                    data: values,
                    backgroundColor: 'rgba(52, 152, 219, 0.7)',
                    borderColor: 'rgba(52, 152, 219, 1)',
                    borderWidth: 2,
                    borderRadius: 8,
                    borderSkipped: false
                }]
            },
            options: {
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: { color: '#e9ecef' },
                        ticks: { font: { size: 12, family: 'Poppins' } },
                        title: { display: true, text: 'Số tiền (VND)', font: { size: 14, family: 'Poppins' } }
                    },
                    x: {
                        grid: { display: false },
                        ticks: { font: { size: 12, family: 'Poppins' } },
                        title: { display: true, text: 'Tháng/Năm', font: { size: 14, family: 'Poppins' } }
                    }
                },
                plugins: {
                    legend: { position: 'top', labels: { font: { size: 14, family: 'Poppins' } } },
                    tooltip: {
                        backgroundColor: '#343a40',
                        titleFont: { size: 14, family: 'Poppins' },
                        bodyFont: { size: 12, family: 'Poppins' },
                        callbacks: {
                            label: function (context) {
                                return `${context.dataset.label}: ${context.raw.toLocaleString()} VND`;
                            }
                        }
                    }
                },
                responsive: false, // Tắt responsive để cố định kích thước
                maintainAspectRatio: false,
                animation: {
                    duration: 0 // Tắt hoạt hình để tránh hiệu ứng di chuyển
                }
            }
        });
    }

    function updatePieChart(roomTypeData) {
        const ctx = document.getElementById('pieChart').getContext('2d');
        if (!ctx) {
            console.error('Không tìm thấy canvas với id "pieChart".');
            return;
        }
        if (pieChart) pieChart.destroy();

        // Cố định kích thước canvas
        ctx.canvas.width = 400;
        ctx.canvas.height = 400;

        const filteredData = roomTypeData && roomTypeData.length > 0
            ? roomTypeData.filter(item => item.value > 0)
            : [];
        const labels = filteredData.length > 0 ? filteredData.map(item => item.label) : ['Không có dữ liệu'];
        const values = filteredData.length > 0 ? filteredData.map(item => item.value) : [1];
        const backgroundColors = filteredData.length > 0
            ? ['#ff6f61', '#6ab04c', '#3498db', '#f1c40f', '#e84393', '#2ecc71', '#9b59b6']
            : ['#d3d3d3'];

        console.log('Dữ liệu biểu đồ hình tròn:', { labels, values });

        pieChart = new Chart(ctx, {
            type: 'pie',
            data: {
                labels: labels,
                datasets: [{
                    data: values,
                    backgroundColor: backgroundColors,
                    borderWidth: 2,
                    borderColor: '#fff'
                }]
            },
            options: {
                plugins: {
                    legend: { position: 'bottom', labels: { font: { size: 14, family: 'Poppins' } } },
                    tooltip: {
                        backgroundColor: '#343a40',
                        titleFont: { size: 14, family: 'Poppins' },
                        bodyFont: { size: 12, family: 'Poppins' },
                        callbacks: {
                            label: function (context) {
                                const label = context.label || '';
                                const value = context.raw || 0;
                                return `${label}: ${value} hợp đồng`;
                            }
                        }
                    }
                },
                responsive: false, // Tắt responsive để cố định kích thước
                maintainAspectRatio: false,
                animation: {
                    duration: 0 // Tắt hoạt hình để tránh hiệu ứng di chuyển
                }
            }
        });
    }

    // Hàm debounce để giảm tần suất gọi hàm
    function debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    const startDateInput = document.getElementById('start-date');
    const endDateInput = document.getElementById('end-date');
    const methodSelect = document.getElementById('revenue-method');
    const currentDate = new Date().toISOString().split('T')[0];

    if (!startDateInput || !endDateInput || !methodSelect) {
        console.error('Không tìm thấy một hoặc nhiều phần tử đầu vào: start-date, end-date, revenue-method.');
        return;
    }

    startDateInput.max = currentDate;
    endDateInput.max = currentDate;

    const validMethods = ['revenue-booking', 'revenue-receipt'];
    if (!validMethods.includes(methodSelect.value)) {
        console.log('Giá trị ban đầu của methodSelect không hợp lệ:', methodSelect.value);
        methodSelect.value = 'revenue-booking';
    }

    async function loadCharts(startDate, endDate, method) {
        if (isLoading) return; // Ngăn gọi lặp lại nếu đang tải
        isLoading = true;

        try {
            const data = await fetchRevenueData(startDate, endDate, method);
            updateRevenueDisplay(data);
            updateColumnChart(data.chartData, method);
            updatePieChart(data.roomTypeData);
        } finally {
            isLoading = false;
        }
    }

    startDateInput.addEventListener('change', debounce(() => {
        const startDate = startDateInput.value;
        const endDate = endDateInput.value || currentDate;
        const method = methodSelect.value;
        if (startDate && endDate && startDate > endDate) {
            alert('Ngày bắt đầu không được lớn hơn ngày kết thúc!');
            startDateInput.value = '';
            return;
        }
        loadCharts(startDate, endDate, method);
    }, 300));

    endDateInput.addEventListener('change', debounce(() => {
        const startDate = startDateInput.value || currentDate;
        const endDate = endDateInput.value;
        const method = methodSelect.value;
        if (startDate && endDate && startDate > endDate) {
            alert('Ngày kết thúc không được nhỏ hơn ngày bắt đầu!');
            endDateInput.value = '';
            return;
        }
        loadCharts(startDate, endDate, method);
    }, 300));

    methodSelect.addEventListener('change', debounce(() => {
        const startDate = startDateInput.value || currentDate;
        const endDate = endDateInput.value || currentDate;
        const method = methodSelect.value;
        console.log('Phương thức đã chọn:', method);
        if (!validMethods.includes(method)) {
            alert('Phương thức không hợp lệ! Mặc định về revenue-booking.');
            methodSelect.value = 'revenue-booking';
            loadCharts(startDate, endDate, 'revenue-booking');
        } else {
            fetchRevenueData(startDate, endDate, method).then(data => {
                updateColumnChart(data.chartData, method);
            });
        }
    }, 300));

    document.getElementById('1monthAgo').addEventListener('click', () => {
        const endDate = currentDate;
        const startDate = new Date(new Date().setMonth(new Date().getMonth() - 1)).toISOString().split('T')[0];
        startDateInput.value = startDate;
        endDateInput.value = endDate;
        const method = methodSelect.value;
        loadCharts(startDate, endDate, method);
    });

    document.getElementById('1yearAgo').addEventListener('click', () => {
        const endDate = currentDate;
        const startDate = new Date(new Date().setFullYear(new Date().getFullYear() - 1)).toISOString().split('T')[0];
        startDateInput.value = startDate;
        endDateInput.value = endDate;
        const method = methodSelect.value;
        loadCharts(startDate, endDate, method);
    });

    // Tải dữ liệu ban đầu
    loadCharts(currentDate, currentDate, methodSelect.value);

    // Ngăn sự kiện resize gây vẽ lại biểu đồ
    window.addEventListener('resize', debounce(() => {
        if (!isLoading) {
            const startDate = startDateInput.value || currentDate;
            const endDate = endDateInput.value || currentDate;
            const method = methodSelect.value;
            loadCharts(startDate, endDate, method);
        }
    }, 500));
});