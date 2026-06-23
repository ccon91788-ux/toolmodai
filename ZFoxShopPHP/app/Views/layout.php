<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no">
    <title><?= isset($data['title']) ? $data['title'] : 'ZFoxShop' ?></title>
    <!-- Inter Font -->
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;700;800&display=swap" rel="stylesheet">
    <link rel="stylesheet" href="<?= BASE_URL ?>/assets/css/style.css">
</head>
<body>
    <nav class="navbar">
        <a href="<?= BASE_URL ?>" class="navbar-brand" style="display: flex; align-items: center; z-index: 1001; position: relative;">
            <img src="<?= BASE_URL ?>/assets/images/unnamed.png" alt="ZFoxShop Logo" style="height: 40px; width: auto; border-radius: 4px;">
        </a>

        <!-- Nút bật menu ở Mobile -->
        <button class="mobile-menu-btn" aria-label="Toggle menu" style="position: relative;">
            <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
                <line x1="3" y1="12" x2="21" y2="12"></line>
                <line x1="3" y1="6" x2="21" y2="6"></line>
                <line x1="3" y1="18" x2="21" y2="18"></line>
            </svg>
        </button>

        <div class="nav-links">
            <a href="<?= BASE_URL ?>">Trang Chủ</a>
            <a href="<?= BASE_URL ?>/download">Tải Tool</a>
            <a href="<?= BASE_URL ?>/services">Dịch Vụ</a>
            <a href="https://zalo.me/g/dtimiq161" target="_blank" style="color: #0068ff; font-weight: 600;">Cộng đồng</a>
            <?php if(isset($_SESSION['user_id'])): ?>
                <?php if(isset($_SESSION['user_role']) && $_SESSION['user_role'] === 'admin'): ?>
                    <a href="<?= BASE_URL ?>/admin">Admin</a>
                <?php endif; ?>
                <a href="<?= BASE_URL ?>/auth/logout" class="btn">Đăng xuất</a>
            <?php else: ?>
                <a href="<?= BASE_URL ?>/auth/login">Đăng nhập</a>
                <a href="<?= BASE_URL ?>/auth/register" class="btn">Bắt đầu miễn phí</a>
            <?php endif; ?>
        </div>
    </nav>

    <main>
        <?php 
            if(isset($contentView) && file_exists($contentView)) {
                require_once $contentView; 
            }
        ?>
    </main>

    <footer>
        <div class="container" style="display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 10px;">
            <div>&copy; <?= date('Y') ?> ZFox.</div>
            <div style="display: flex; gap: 15px;">
                <a href="https://zalo.me/g/dtimiq161" target="_blank" style="display: flex; align-items: center; gap: 5px;">
                    <i class="fa-solid fa-comment-dots"></i> Nhóm Zalo
                </a>
            </div>
        </div>
    </footer>

    <!-- Global Custom Confirm Modal -->
    <div class="modal-overlay" id="customConfirmModal">
        <div class="modal-box">
            <h3 class="modal-title" id="ccTitle">Xác nhận</h3>
            <p class="modal-desc" id="ccDesc">Bạn có chắc chắn muốn thực hiện hành động này?</p>
            <div class="modal-actions">
                <button class="modal-btn modal-btn-cancel" id="ccCancel">Hủy bỏ</button>
                <button class="modal-btn modal-btn-confirm" id="ccConfirm">Đồng ý</button>
            </div>
        </div>
    </div>

    <!-- Global Toast Notification -->
    <div id="global-toast" style="visibility: hidden; min-width: 250px; background: var(--input-bg); color: var(--text-color); text-align: center; border-radius: 8px; padding: 16px; position: fixed; z-index: 10000; left: 50%; bottom: 30px; transform: translateX(-50%); font-size: 0.95rem; box-shadow: 0 10px 15px -3px rgba(0,0,0,0.3); border: 1px solid var(--border-color); border-left: 4px solid var(--primary-color); opacity: 0; transition: opacity 0.3s, bottom 0.3s;">
        <i class="fas fa-info-circle" style="color: var(--primary-color); margin-right: 8px;"></i> <span id="toast-msg"></span>
    </div>

    <!-- FontAwesome Global -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">

    <script>
        // Global Toast Logic
        window.showToast = function(message) {
            var toast = document.getElementById("global-toast");
            document.getElementById("toast-msg").innerText = message;
            toast.style.visibility = "visible";
            toast.style.opacity = "1";
            toast.style.bottom = "50px";
            
            setTimeout(function(){ 
                toast.style.opacity = "0";
                toast.style.bottom = "30px";
                setTimeout(function(){ toast.style.visibility = "hidden"; }, 300);
            }, 3000);
        };

        // Custom Confirm Logic (Nâng cấp)
        window.customConfirm = function(titleOrMessage, messageOrCallback, callbackOrUndefined) {
            let title = 'Xác nhận';
            let message = '';
            let callback = null;

            if (typeof callbackOrUndefined === 'function') {
                title = titleOrMessage;
                message = messageOrCallback;
                callback = callbackOrUndefined;
            } else {
                message = titleOrMessage;
                callback = messageOrCallback;
            }

            const modal = document.getElementById('customConfirmModal');
            const titleEl = document.getElementById('ccTitle');
            const desc = document.getElementById('ccDesc');
            const btnCancel = document.getElementById('ccCancel');
            const btnConfirm = document.getElementById('ccConfirm');

            if (titleEl) titleEl.innerText = title;
            desc.innerText = message;
            modal.classList.add('active');

            const cleanup = () => {
                modal.classList.remove('active');
                btnCancel.removeEventListener('click', onCancel);
                btnConfirm.removeEventListener('click', onConfirm);
            };

            const onCancel = () => { cleanup(); if(typeof callback === 'function') callback(false); };
            const onConfirm = () => { 
                cleanup(); 
                btnConfirm.innerText = 'Đang xử lý...'; // Lock temporary
                setTimeout(() => { btnConfirm.innerText = 'Đồng ý'; }, 2000);
                if(typeof callback === 'function') callback(true); 
            };

            btnCancel.addEventListener('click', onCancel);
            btnConfirm.addEventListener('click', onConfirm);
        };

        document.addEventListener('DOMContentLoaded', () => {
            // Mobile Menu Logic
            const menuBtn = document.querySelector('.mobile-menu-btn');
            const navLinks = document.querySelector('.nav-links');
            
            if(menuBtn) {
                menuBtn.addEventListener('click', () => {
                    navLinks.classList.toggle('active');
                    const isActive = navLinks.classList.contains('active');
                    if(isActive) {
                        menuBtn.innerHTML = `<svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>`;
                        document.body.style.overflow = 'hidden'; 
                    } else {
                        menuBtn.innerHTML = `<svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><line x1="3" y1="12" x2="21" y2="12"></line><line x1="3" y1="6" x2="21" y2="6"></line><line x1="3" y1="18" x2="21" y2="18"></line></svg>`;
                        document.body.style.overflow = '';
                    }
                });
            }

            // Bind data-confirm handlers (Dành cho code cũ dùng data-confirm)
            document.querySelectorAll('[data-confirm]').forEach(el => {
                el.addEventListener('click', function(e) {
                    e.preventDefault();
                    const msg = this.getAttribute('data-confirm');
                    customConfirm('Xác nhận', msg, (result) => {
                        if(result) {
                            if(this.tagName === 'A') {
                                window.location.href = this.href;
                            } else if(this.tagName === 'BUTTON' && this.type === 'submit') {
                                this.closest('form').submit();
                            }
                        }
                    });
                });
            });
        });
    </script>
    
    <!-- Floating Zalo Button -->
    <a href="https://zalo.me/g/dtimiq161" class="zalo-float" target="_blank" title="Tham gia nhóm Zalo">
        <i class="fa-solid fa-comment-dots"></i>
    </a>
</body>
</html>
