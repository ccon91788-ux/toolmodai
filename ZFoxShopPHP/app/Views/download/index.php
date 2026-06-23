<div style="padding: 40px 0;">
    <h1 style="text-align: center; margin-bottom: 8px; font-weight: 800; font-size: 2.5rem; letter-spacing: -0.04em;">Tải xuống</h1>
    <p style="text-align: center; margin-bottom: 60px; color: var(--text-muted); font-size: 1.1rem;">Phiên bản duy nhất. Tối đa sức mạnh.</p>

    <div style="max-width: 500px; margin: 0 auto;">
        <?php if($data['file']): ?>
            <div class="card" style="background: var(--input-bg); border: 1px solid var(--border-color); border-radius: var(--radius); padding: 40px 30px; text-align: center;">
                <div style="margin-bottom: 24px;">
                    <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="var(--text-color)" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
                        <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
                        <polyline points="7 10 12 15 17 10"></polyline>
                        <line x1="12" y1="15" x2="12" y2="3"></line>
                    </svg>
                </div>
                <h2 style="margin: 0 0 12px 0; font-size: 1.5rem; font-weight: 700;"><?= htmlspecialchars($data['file']->file_name) ?></h2>
                <p style="margin: 0 0 32px 0; color: var(--text-muted); line-height: 1.6;"><?= htmlspecialchars($data['file']->description) ?></p>
                
                <a href="<?= htmlspecialchars($data['file']->file_url) ?>" class="btn btn-primary" style="width: 100%; height: 56px; font-size: 1.1rem; border-radius: 12px;">Tải Gói (.zip) Về Máy</a>
            </div>
        <?php else: ?>
            <div style="text-align: center; padding: 60px 20px; border: 1px dashed var(--border-color); border-radius: var(--radius);">
                <h3 style="color: var(--text-muted); margin: 0 0 8px 0;">Hệ thống chưa có File</h3>
                <p style="color: var(--text-muted); font-size: 0.9rem; margin: 0;">Quản trị viên đang chuẩn bị bản cập nhật, bạn quay lại sau nhé!</p>
            </div>
        <?php endif; ?>
    </div>
</div>
