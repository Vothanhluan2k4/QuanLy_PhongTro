document.addEventListener("DOMContentLoaded", function () {
    var swiper = new Swiper('.swiper', {
        slidesPerView: 1, // Hiển thị 1 slide tại 1 thời điểm
        spaceBetween: 10, // Khoảng cách giữa các slide
        loop: true, // Lặp lại slide
        centeredSlides: true, // Căn giữa slide hiện tại
        autoplay: {
            delay: 2500, // Chuyển slide sau 2.5 giây
            disableOnInteraction: false, // Không dừng khi người dùng tương tác
        },
        speed: 1000, // Tăng tốc độ chuyển đổi slide
        effect: 'slide', // Hiệu ứng chuyển slide mượt mà
        pagination: {
            el: '.swiper-pagination',
            clickable: true, // Cho phép bấm vào pagination
        },
        navigation: {
            nextEl: '.swiper-button-next',
            prevEl: '.swiper-button-prev',
        },
    });
});

function toggleContent() {
    var sections = ["extraContent1", "extraContent2", "extraContent3"];
    var button = document.querySelector(".toggle-btn");

    var allHidden = sections.every(id => document.getElementById(id).style.display === "none"
        || document.getElementById(id).style.display === "");

    sections.forEach(id => {
        var section = document.getElementById(id);
        section.style.display = allHidden ? "block" : "none";
    });
    if (allHidden) {
        document.getElementById("extraContent3").appendChild(button);
        button.textContent = "Ẩn bớt";
    } else {
        document.querySelector(".advantage").appendChild(button);
        button.textContent = "Xem thêm...";
    }
}
