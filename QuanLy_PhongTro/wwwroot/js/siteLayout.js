document.addEventListener("DOMContentLoaded", function () {
    const userInfo = document.querySelector(".user-info");
    const dropdownMenu = document.querySelector(".dropdown-menu");
    const menuToggle = document.getElementById('menuToggle');
    const navMain = document.getElementById('navMain');
    const navAuth = document.getElementById('navAuth');


    if (userInfo && dropdownMenu) {
        userInfo.addEventListener("click", function () {
            dropdownMenu.style.display =
                dropdownMenu.style.display === "block" ? "none" : "block";
        });

        // Ẩn menu khi click ra ngoài
        document.addEventListener("click", function (event) {
            if (!userInfo.contains(event.target) && !dropdownMenu.contains(event.target)) {
                dropdownMenu.style.display = "none";
            }
        });
    }

    const currentPath = window.location.pathname.toLowerCase() || "/"; // Đảm bảo trang chủ là "/"
    const navLinks = document.querySelectorAll(".nav-main a, .nav-auth a");

    navLinks.forEach(link => {
        let linkPath = link.getAttribute("href")?.toLowerCase() || "/"; // Đảm bảo linkPath không null
        let isActive = false;

        // Kiểm tra nếu linkPath khớp với currentPath
        if (linkPath === currentPath) {
            isActive = true;
        } else {
            // Xử lý các trường hợp đặc biệt (ví dụ: /Admin/PhongTro/Index)
            if (currentPath.startsWith(linkPath) && linkPath !== "/") {
                isActive = true;
            }

            // Trường hợp đặc biệt cho link "Admin"
            if (link.textContent.trim() === "Admin") {
                if (currentPath.startsWith("/admin/")) {
                    isActive = true;
                }
            }

            // Trường hợp đặc biệt cho link "Lưu"
            if (linkPath.includes("luuphong")) {
                if (currentPath.includes("luuphong")) {
                    isActive = true;
                }
            }

            // Trường hợp đặc biệt cho link "Trang cá nhân"
            if (linkPath.includes("trangcanhan")) {
                if (currentPath.includes("trangcanhan")) {
                    isActive = true;
                }
            }
        }

        // Thêm class active nếu link khớp với trang hiện tại
        if (isActive) {
            link.classList.add("active");
        }
    });

    if (menuToggle) {
        menuToggle.addEventListener('click', function() {
            // Toggle active class for both navigation sections
            navMain.classList.toggle('nav-active');
            navAuth.classList.toggle('nav-active');
            
            // Change icon based on menu state
            const icon = menuToggle.querySelector('i');
            if (navMain.classList.contains('nav-active')) {
                icon.classList.remove('fa-bars');
                icon.classList.add('fa-times');
            } else {
                icon.classList.remove('fa-times');
                icon.classList.add('fa-bars');
            }
        });
    }
    
    // Close mobile menu when clicking outside
    document.addEventListener('click', function(event) {
        const header = document.querySelector('.header');
        const isClickInsideHeader = header.contains(event.target);
        
        if (!isClickInsideHeader && window.innerWidth <= 768) {
            navMain.classList.remove('nav-active');
            navAuth.classList.remove('nav-active');
            
            const icon = menuToggle.querySelector('i');
            icon.classList.remove('fa-times');
            icon.classList.add('fa-bars');
        }
    });
    
    // Close mobile menu when window is resized to desktop
    window.addEventListener('resize', function() {
        if (window.innerWidth > 768) {
            navMain.classList.remove('nav-active');
            navAuth.classList.remove('nav-active');
            
            const icon = menuToggle.querySelector('i');
            icon.classList.remove('fa-times');
            icon.classList.add('fa-bars');
        }
    });
    
    // Handle user menu dropdown (for desktop)
    const userMenu = document.querySelector('.user-menu');
    if (userMenu) {
        const userInfo = userMenu.querySelector('.user-info');
        const dropdownMenu = userMenu.querySelector('.dropdown-menu');
        
        // For mobile, toggle dropdown on click
        if (window.innerWidth <= 768) {
            userInfo.addEventListener('click', function(e) {
                e.preventDefault();
                e.stopPropagation();
                
                if (dropdownMenu.style.display === 'none' || dropdownMenu.style.display === '') {
                    dropdownMenu.style.display = 'block';
                } else {
                    dropdownMenu.style.display = 'none';
                }
            });
        }
    }
    
    // Handle window resize for user menu
    window.addEventListener('resize', function() {
        const userMenu = document.querySelector('.user-menu');
        if (userMenu) {
            const dropdownMenu = userMenu.querySelector('.dropdown-menu');
            if (window.innerWidth > 768) {
                dropdownMenu.style.display = 'none';
            }
        }
    });


});