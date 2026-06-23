<style>
/* Utilities cho lưới Card thay vì Table */
.my-keys-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 20px; }
.key-card { background: var(--input-bg); border: 1px solid var(--border-color); border-radius: 12px; padding: 20px; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1); position: relative; }
.key-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px; border-bottom: 1px solid var(--border-color); padding-bottom: 12px; }
.key-string { font-family: monospace; font-size: 1.1rem; color: var(--primary-color); font-weight: 700; word-break: break-all;}
.btn-copy { background: transparent; border: none; color: var(--text-muted); cursor: pointer; padding: 4px; border-radius: 4px; transition: color 0.2s; }
.btn-copy:hover { color: var(--primary-color); background: rgba(255,102,0,0.1); }
.key-field { margin-bottom: 8px; font-size: 0.95rem; display: flex; justify-content: space-between; align-items: center;}
.key-field span.lbl { color: var(--text-muted); }
.badge { padding: 4px 10px; border-radius: 6px; font-size: 0.8rem; font-weight: 600; }
.badge-active { background: rgba(52, 211, 153, 0.15); color: #34d399; }
.badge-expired { background: rgba(248, 113, 113, 0.15); color: #f87171; }
.key-actions { display: flex; gap: 10px; margin-top: 20px; }
.key-actions form { flex: 1; margin: 0; }
.btn-outline { width: 100%; padding: 8px; border-radius: 8px; font-size: 0.9rem; font-weight: 600; cursor: pointer; transition: 0.2s; text-align: center; }
.btn-renew { background: var(--primary-color); border: 1px solid var(--primary-color); color: #fff; }
.btn-renew:hover { filter: brightness(1.1); }
.btn-reset { background: transparent; border: 1px solid var(--border-color); color: var(--text-color); }
.btn-reset:hover { border-color: #f87171; color: #f87171; }
.btn-reset:disabled { opacity: 0.5; cursor: not-allowed; border-color: var(--border-color) !important; color: var(--text-muted) !important;}
.nickname-row { display: flex; align-items: center; gap: 8px; margin-bottom: 12px; padding: 8px 10px; background: rgba(99,102,241,0.06); border-radius: 8px; border: 1px dashed rgba(99,102,241,0.25); }
.nickname-input { flex: 1; background: transparent; border: none; outline: none; font-size: 0.95rem; color: var(--text-color); font-weight: 600; }
.nickname-input::placeholder { color: var(--text-muted); font-weight: 400; font-style: italic; }
.btn-save-nick { background: transparent; border: none; color: var(--text-muted); cursor: pointer; padding: 4px 8px; border-radius: 4px; transition: 0.2s; font-size: 0.85rem; }
.btn-save-nick:hover { color: #6366f1; background: rgba(99,102,241,0.1); }
.ip-display { font-family: monospace; font-size: 0.85rem; color: #60a5fa; background: rgba(96,165,250,0.08); padding: 2px 8px; border-radius: 4px; }
</style>

<div style="padding: 20px 0;">
    <h1 style="text-align: center; margin-bottom: 12px; font-weight: 800; font-size: 2.2rem;">Dịch Vụ Thuê Auto</h1>
    <p style="text-align: center; margin-bottom: 30px; color: var(--text-muted); font-size: 1rem; max-width: 600px; margin-left: auto; margin-right: auto;">
        Cày cuốc 24/7 hoàn toàn tự động. Tự do chuyển đổi thiết bị bất cứ lúc nào (Reset PC).
    </p>

    <?php if(isset($_SESSION['user_id'])): ?>
    <div style="text-align: center; margin-bottom: 30px;">
        <span style="background: var(--input-bg); padding: 12px 24px; border-radius: 9999px; border: 1px solid var(--border-color); font-size: 1.05rem; box-shadow: 0 2px 10px rgba(0,0,0,0.2);">
            Số dư tài khoản: <strong style="color: var(--primary-color); font-size: 1.2rem;"><?= number_format($data['userBalance']) ?> VNĐ</strong>
        </span>
    </div>
    <?php endif; ?>

    <div class="container" style="max-width: 1000px;">
        <?php if(!empty($data['success'])): ?>
            <div style="background: rgba(52, 211, 153, 0.1); color: #34d399; padding: 16px; border-radius: 12px; margin-bottom: 24px; border: 1px solid rgba(52, 211, 153, 0.3); text-align: center; font-weight: 500;">
                <i class="fas fa-check-circle" style="margin-right: 8px;"></i> <?= htmlspecialchars($data['success']) ?>
            </div>
        <?php endif; ?>

        <?php if(!empty($data['error'])): ?>
            <div style="background: rgba(248, 113, 113, 0.1); color: #f87171; padding: 16px; border-radius: 12px; margin-bottom: 24px; border: 1px solid rgba(248, 113, 113, 0.3); text-align: center; font-weight: 500;">
                <i class="fas fa-exclamation-triangle" style="margin-right: 8px;"></i> <?= htmlspecialchars($data['error']) ?>
            </div>
        <?php endif; ?>

        <!-- BUY KEY SECTION -->
        <h2 style="margin-bottom: 20px; font-weight: 700; font-size: 1.5rem; border-left: 4px solid var(--primary-color); padding-left: 12px;">Đăng Ký Mới</h2>
        <div style="display: grid; grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap: 24px; margin-bottom: 48px;">
            <?php foreach($data['packages'] as $pkg): ?>
            <div style="background: var(--input-bg); border: 1px solid var(--border-color); border-radius: 16px; padding: 26px; display: flex; flex-direction: column; justify-content: space-between; position: relative; overflow: hidden;">
                <!-- Decor stripe -->
                <div style="position: absolute; top: 0; left: 0; width: 100%; height: 4px; background: var(--primary-color);"></div>
                <div>
                    <h3 style="margin-top: 0; font-size: 1.4rem; font-weight: 700;"><?= htmlspecialchars($pkg->name) ?></h3>
                    <p style="color: var(--text-muted); font-size: 0.95rem; margin-bottom: 20px; min-height: 45px;"><?= htmlspecialchars($pkg->description) ?></p>
                    <div style="font-size: 2rem; font-weight: 800; color: var(--primary-color); margin-bottom: 24px;">
                        <?= number_format($pkg->price) ?> <span style="font-size: 1.1rem; color: var(--text-color); font-weight: 600;">VNĐ</span>
                    </div>
                    <ul style="list-style: none; padding: 0; margin-bottom: 24px; color: var(--text-color); font-size: 0.95rem;">
                        <li style="margin-bottom: 12px;"><span style="color: var(--primary-color); margin-right: 8px;">✔</span> Thời gian SD: <strong><?= $pkg->duration_days ?> ngày</strong></li>
                        <li style="margin-bottom: 12px;"><span style="color: var(--primary-color); margin-right: 8px;">✔</span> Số máy tối đa: <strong><?= $pkg->device_limit ?> PC</strong></li>
                        <li style="margin-bottom: 12px;"><span style="color: var(--primary-color); margin-right: 8px;">✔</span> Được phép Đổi Máy tự do</li>
                    </ul>
                </div>
                <!-- Sửa lại phần Form Mua: Dùng JS gọi Modal API Toàn cục -->
                <form action="<?= BASE_URL ?>/services/buy/<?= $pkg->id ?>" method="POST" style="margin: 0;">
                    <button type="button" class="btn btn-primary" style="width: 100%; border-radius: 12px; padding: 12px; font-size: 1rem; font-weight: 700; display: flex; justify-content: center; align-items: center;" onclick="confirmAction(this, 'Xác Nhận Thanh Toán', 'Trừ ngay <?= number_format($pkg->price) ?> VNĐ từ số dư để mua gói <?= htmlspecialchars($pkg->name) ?>. Bạn có chắc chắn không?');">
                        <i class="fas fa-shopping-cart" style="margin-right: 8px;"></i> Mua Ngay
                    </button>
                </form>
            </div>
            <?php endforeach; ?>
        </div>

        <!-- MY KEYS SECTION -->
        <h2 style="margin-bottom: 20px; font-weight: 700; font-size: 1.5rem; border-left: 4px solid #3b82f6; padding-left: 12px;">Tool Đang Thuê Của Bạn</h2>
        
        <?php if(empty($data['licenses'])): ?>
            <div style="background: var(--input-bg); border: 1px dashed var(--border-color); border-radius: 16px; padding: 40px 20px; text-align: center;">
                <i class="fas fa-box-open" style="font-size: 3rem; color: var(--border-color); margin-bottom: 16px;"></i>
                <h3 style="color: var(--text-muted); margin: 0;">Bạn chưa thuê gói Auto nào</h3>
                <p style="color: var(--text-muted); font-size: 0.9rem; margin-top: 8px;">Hãy chọn một gói phù hợp ở phía trên để bắt đầu treo máy nhé.</p>
            </div>
        <?php else: ?>
            <div class="my-keys-grid">
                <?php foreach($data['licenses'] as $lic): 
                      $isActive = ($lic->status === 'active' && strtotime($lic->expires_at) > time());
                ?>
                <div class="key-card">
                    <div class="key-header">
                        <span class="key-string" id="key-<?= $lic->id ?>"><?= htmlspecialchars($lic->license_key) ?></span>
                        <button class="btn-copy" onclick="copyKey('key-<?= $lic->id ?>')" title="Copy Key">
                            <i class="fas fa-copy" style="font-size: 1.2rem;"></i>
                        </button>
                    </div>
                    
                    <div class="key-field">
                        <span class="lbl"><i class="fas fa-cube"></i> Gói cước:</span>
                        <strong><?= htmlspecialchars($lic->package_name) ?></strong>
                    </div>

                    <div class="key-field">
                        <span class="lbl"><i class="fas fa-clock"></i> Hạn dùng:</span>
                        <strong><?= date('H:i - d/m/Y', strtotime($lic->expires_at)) ?></strong>
                    </div>

                    <div class="key-field">
                        <span class="lbl"><i class="fas fa-laptop"></i> Trạng thái PC:</span>
                        <?php if(empty($lic->bound_hdd)): ?>
                            <span style="color: #60a5fa; font-size: 0.9rem;">Chưa gắn PC (Sẵn sàng)</span>
                        <?php else: ?>
                            <span style="color: #34d399; font-size: 0.9rem;">Đang chạy (Đã gắn PC)</span>
                        <?php endif; ?>
                    </div>

                    <?php if(!empty($lic->client_ip)): ?>
                    <div class="key-field">
                        <span class="lbl"><i class="fas fa-network-wired"></i> IP máy đang dùng:</span>
                        <span class="ip-display"><?= htmlspecialchars($lic->client_ip) ?></span>
                    </div>
                    <?php endif; ?>

                    <div class="nickname-row">
                        <i class="fas fa-tag" style="color: #6366f1; font-size: 0.9rem;"></i>
                        <form action="<?= BASE_URL ?>/services/update_nickname/<?= $lic->id ?>" method="POST" style="display:flex; flex:1; gap:6px; margin:0; align-items:center;">
                            <input type="text" name="nickname" class="nickname-input" value="<?= htmlspecialchars($lic->nickname ?? '') ?>" placeholder="Ghi chú (VD: VPS 1 - Farm Namek)" maxlength="100">
                            <button type="submit" class="btn-save-nick" title="Lưu ghi chú"><i class="fas fa-check"></i> Lưu</button>
                        </form>
                    </div>

                    <div class="key-field" style="margin-top: 12px;">
                        <?php if($isActive): ?>
                            <span class="badge badge-active"><i class="fas fa-check"></i> Đang Hoạt Động</span>
                        <?php else: ?>
                            <span class="badge badge-expired"><i class="fas fa-times"></i> Hết Hạn / Bị Khóa</span>
                        <?php endif; ?>
                    </div>

                    <div class="key-actions">
                        <!-- Gia hạn -->
                        <form action="<?= BASE_URL ?>/services/extend/<?= $lic->id ?>" method="POST">
                            <button type="button" class="btn-outline btn-renew" title="Gia hạn tự động" onclick="confirmAction(this, 'Gia Hạn Bản Quyền', 'Hệ thống sẽ dùng số dư hiện tại của bạn để mua thêm thời gian sử dụng cho gói này. Tiếp tục chứ?');">
                                <i class="fas fa-sync-alt" style="margin-right: 4px;"></i> Gia Hạn
                            </button>
                        </form>
                        
                        <!-- Reset HWID -->
                        <form action="<?= BASE_URL ?>/services/reset_hwid/<?= $lic->id ?>" method="POST">
                            <button type="button" class="btn-outline btn-reset" title="Nhấn để đổi PC sử dụng Auto" onclick="confirmAction(this, 'Đổi Máy (Reset PC)', 'Sau khi đổi máy, key sẽ ngắt kết nối VPS cũ và có thể dán vào VPS mới. Tiến hành đổi?');" <?= (empty($lic->bound_hdd)) ? 'disabled' : '' ?>>
                                <i class="fas fa-unlink" style="margin-right: 4px;"></i> Đổi Máy (PC)
                            </button>
                        </form>
                    </div>
                </div>
                <?php endforeach; ?>
            </div>
        <?php endif; ?>
    </div>
</div>

<script>
// Logic nối lệnh với Global Modal trong layout.php
function confirmAction(btn, title, msg) {
    if(window.customConfirm) {
        window.customConfirm(title, msg, function(result) {
            if(result) btn.closest('form').submit();
        });
    } else {
        if(confirm(msg)) btn.closest('form').submit();
    }
}

function copyKey(elementId) {
    var keyText = document.getElementById(elementId).innerText;
    navigator.clipboard.writeText(keyText).then(function() {
        if(window.showToast) window.showToast("Đã chép: " + keyText);
        else alert("Đã chép: " + keyText);
    }, function(err) {
        if(window.showToast) window.showToast("Copy thất bại. Vui lòng thử lại!");
        else alert("Copy thất bại!");
    });
}
</script>
