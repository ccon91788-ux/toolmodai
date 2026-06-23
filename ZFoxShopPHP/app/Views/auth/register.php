<div class="auth-wrapper">
    <div class="auth-card">
        <h2>Tạo tài khoản</h2>
        <p class="subtitle">Bắt đầu tự động hóa cùng ZFoxShop</p>
        
        <?php if(isset($data['error'])): ?>
            <div class="error-msg">
                <?= htmlspecialchars($data['error']) ?>
            </div>
        <?php endif; ?>

        <form action="<?= BASE_URL ?>/auth/register" method="POST">
            <div class="form-group">
                <label for="username">Tên đăng nhập</label>
                <input type="text" name="username" id="username" class="form-control" placeholder="nickname của bạn" required autocomplete="off" autofocus>
            </div>
            <div class="form-group">
                <label for="email">Địa chỉ Email (Tùy chọn)</label>
                <input type="email" name="email" id="email" class="form-control" placeholder="name@example.com">
            </div>
            <div class="form-group">
                <label for="password">Mật khẩu</label>
                <input type="password" name="password" id="password" class="form-control" placeholder="Mật khẩu trên 6 ký tự" required>
            </div>
            <button type="submit" class="btn btn-primary" style="width: 100%; margin-top: 8px;">Tiếp Tục</button>
        </form>
        
        <p style="text-align: center; margin-top: 24px; font-size: 0.875rem; color: var(--text-muted);">
            Đã có tài khoản? <a href="<?= BASE_URL ?>/auth/login" style="color: var(--text-color); font-weight: 500;">Đăng nhập</a>
        </p>
    </div>
</div>
