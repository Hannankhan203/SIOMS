// Mobile menu enhancements
$(document).ready(function() {
    // Auto-close mobile menu when clicking outside
    $(document).click(function(e) {
        if (!$(e.target).closest('.navbar-collapse').length && 
            $('.navbar-collapse').hasClass('show')) {
            $('.navbar-collapse').collapse('hide');
        }
    });
    
    // Smooth scroll for anchor links
    $('a[href^="#"]').on('click', function(e) {
        if (this.hash !== "") {
            e.preventDefault();
            const hash = this.hash;
            $('html, body').animate({
                scrollTop: $(hash).offset().top - 80
            }, 800);
        }
    });
    
    // Initialize tooltips
    $('[data-bs-toggle="tooltip"]').tooltip();
    
    // Initialize popovers
    $('[data-bs-toggle="popover"]').popover();
    
    // Back to top button
    $(window).scroll(function() {
        if ($(this).scrollTop() > 300) {
            $('#backToTop').fadeIn();
        } else {
            $('#backToTop').fadeOut();
        }
    });
    
    // Table row click for mobile
    $('tr[data-href]').on('click', function() {
        if ($(window).width() < 768) {
            window.location = $(this).data('href');
        }
    });
    
    // Form validation feedback
    $('form').on('submit', function() {
        $(this).find('.is-invalid').removeClass('is-invalid');
    });
    
    // Real-time input validation
    $('.form-control').on('blur', function() {
        if (!$(this).val()) {
            $(this).addClass('is-invalid');
        } else {
            $(this).removeClass('is-invalid');
        }
    });
    
    // Mobile swipe gestures
    let touchstartX = 0;
    let touchendX = 0;
    
    $('.table-responsive').on('touchstart', function(e) {
        touchstartX = e.changedTouches[0].screenX;
    });
    
    $('.table-responsive').on('touchend', function(e) {
        touchendX = e.changedTouches[0].screenX;
        handleSwipe();
    });
    
    function handleSwipe() {
        const swipeThreshold = 50;
        if (touchendX < touchstartX - swipeThreshold) {
            // Swipe left - show next columns
            $(this).scrollLeft($(this).scrollLeft() + 100);
        }
        if (touchendX > touchstartX + swipeThreshold) {
            // Swipe right - show previous columns
            $(this).scrollLeft($(this).scrollLeft() - 100);
        }
    }
});

// Offline detection
window.addEventListener('online', updateOnlineStatus);
window.addEventListener('offline', updateOnlineStatus);

function updateOnlineStatus() {
    const status = navigator.onLine ? 'online' : 'offline';
    showToast(`You are now ${status}`, status === 'online' ? 'success' : 'warning');
}

// Toast notifications
function showToast(message, type = 'info') {
    const toast = $(`
        <div class="toast align-items-center text-white bg-${type} border-0" role="alert">
            <div class="d-flex">
                <div class="toast-body">
                    <i class="fas fa-${getToastIcon(type)} me-2"></i>
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>
    `);
    
    $('#toastContainer').append(toast);
    const bsToast = new bootstrap.Toast(toast[0]);
    bsToast.show();
    
    toast.on('hidden.bs.toast', function() {
        $(this).remove();
    });
}

function getToastIcon(type) {
    const icons = {
        'success': 'check-circle',
        'error': 'exclamation-circle',
        'warning': 'exclamation-triangle',
        'info': 'info-circle'
    };
    return icons[type] || 'info-circle';
}

// Image lazy loading
if ('IntersectionObserver' in window) {
    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                img.src = img.dataset.src;
                img.classList.remove('lazy');
                observer.unobserve(img);
            }
        });
    });
    
    document.querySelectorAll('img.lazy').forEach(img => imageObserver.observe(img));
}