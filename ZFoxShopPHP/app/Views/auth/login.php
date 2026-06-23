<div class="auth-wrapper">
    <div class="auth-card">
        <h2>Chào mừng trở lại</h2>
        <p class="subtitle">Đăng nhập vào tài khoản ZFoxShop của bạn</p>
        
        <?php if(isset($data['error'])): ?>
            <div class="error-msg">
                <?= htmlspecialchars($data['error']) ?>
            </div>
        <?php endif; ?>

        <form action="<?= BASE_URL ?>/auth/login" method="POST">
            <div class="form-group">
                <label for="username">Tên đăng nhập</label>
                <input type="text" name="username" id="username" class="form-control" placeholder="Nhập tên tài khoản" required autocomplete="off" autofocus>
            </div>
            <div class="form-group">
                <label for="password">Mật khẩu</label>
                <input type="password" name="password" id="password" class="form-control" placeholder="••••••••" required>
            </div>
            <button type="submit" class="btn btn-primary" style="width: 100%; margin-top: 8px;">Đăng Nhập</button>
        </form>
        
        <p style="text-align: center; margin-top: 24px; font-size: 0.875rem; color: var(--text-muted);">
            Chưa có tài khoản? <a href="<?= BASE_URL ?>/auth/register" style="color: var(--text-color); font-weight: 500;">Tạo mới ngay</a>
        </p>
    </div>
</div>
