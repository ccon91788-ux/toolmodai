<div>
    <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem;">
        <h2 style="color: var(--text-color); font-weight: 700;">Quản Trị Hệ Thống</h2>
        <span style="background: var(--text-color); color: var(--bg-color); padding: 6px 16px; border-radius: 20px; font-size: 0.85rem; font-weight: 600;">Administrator</span>
    </div>

    <div class="card" style="margin-bottom: 2rem; background: var(--input-bg); border: 1px solid var(--border-color); padding: 30px; border-radius: var(--radius);">
        <h3 style="margin-top: 0; margin-bottom: 8px;">Tải Lên Bản Cập Nhật Mới (chỉ .zip)</h3>
        <p style="color: var(--text-muted); font-size: 0.9rem; margin-bottom: 24px;">
            Hệ thống chỉ giữ 1 release duy nhất: upload bản mới sẽ tự đổi tên file mới, xóa file zip cũ và ghi đè dữ liệu release cũ.
        </p>

        <?php if (!empty($data['success'])): ?>
            <div style="background: #022c22; color: #34d399; padding: 12px 16px; border-radius: var(--radius); margin-bottom: 24px; border: 1px solid #064e3b; font-size: 0.875rem;">
                <?= htmlspecialchars($data['success']) ?>
            </div>
        <?php endif; ?>
        <?php if (!empty($data['error'])): ?>
            <div class="error-msg">
                <?= htmlspecialchars($data['error']) ?>
            </div>
        <?php endif; ?>

        <form action="<?= BASE_URL ?>/admin/upload" method="POST" enctype="multipart/form-data">
            <div style="display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 16px;">
                <div class="form-group">
                    <label for="upload_version">Phiên bản (vd: 1.0.2)</label>
                    <input type="text" name="upload_version" id="upload_version" class="form-control" value="" placeholder="1.0.2" required>
                </div>
                <div class="form-group">
                    <label for="channel">Kênh phát hành</label>
                    <select name="channel" id="channel" class="form-control">
                        <option value="stable" selected>stable</option>
                        <option value="beta">beta</option>
                    </select>
                </div>
                <div class="form-group">
                    <label for="mandatory">Bắt buộc cập nhật</label>
                    <select name="mandatory" id="mandatory" class="form-control">
                        <option value="0" selected>Không</option>
                        <option value="1">Có</option>
                    </select>
                </div>
            </div>

            <div class="form-group">
                <label for="zipfile">Chọn file Tool đóng gói dạng (.zip)</label>
                <input type="file" name="zipfile" id="zipfile" class="form-control" style="padding-top: 10px;" accept=".zip" required>
            </div>
            <div class="form-group">
                <label for="description">Mô tả cập nhật</label>
                <input type="text" name="description" id="description" class="form-control" placeholder="Ví dụ: cập nhật auto train + goback" autocomplete="off">
            </div>
            <button type="submit" class="btn btn-primary" style="margin-top: 10px;">Upload & Tạo Release Mới</button>
        </form>
    </div>

    <div class="card" style="margin-bottom: 2rem; background: var(--input-bg); border: 1px solid var(--border-color); padding: 30px; border-radius: var(--radius);">
        <h3 style="margin-top: 0; margin-bottom: 8px;">Tải Lên Bản Cập Nhật Demo (chỉ .zip)</h3>
        <p style="color: var(--text-muted); font-size: 0.9rem; margin-bottom: 24px;">
            Upload bản demo sẽ tự đổi tên file mới kèm ngày giờ để bảo mật. Link tải sẽ được giấu, phục vụ riêng cho ứng dụng cập nhật tự động.
        </p>

        <form action="<?= BASE_URL ?>/admin/upload_demo" method="POST" enctype="multipart/form-data">
            <div style="display: grid; grid-template-columns: 1fr; gap: 16px;">
                <div class="form-group">
                    <label for="demozip">Chọn file Demo (.zip)</label>
                    <input type="file" name="demozip" id="demozip" class="form-control" style="padding-top: 10px;" accept=".zip" required>
                </div>
            </div>
            <button type="submit" class="btn btn-primary" style="margin-top: 10px;">Tải lên Bản Demo Mới</button>
        </form>
    </div>

    <div class="card" style="margin-bottom: 2rem; background: var(--input-bg); border: 1px solid var(--border-color); padding: 30px; border-radius: var(--radius);">
        <h3 style="margin-top: 0; margin-bottom: 8px;">🔄 Cập Nhật Website</h3>
        <p style="color: var(--text-muted); font-size: 0.9rem; margin-bottom: 24px;">Upload file ZIP chứa source web mới. Hệ thống sẽ ghi đè file trùng tên, trừ các file bảo vệ.</p>

        <form action="<?= BASE_URL ?>/admin/update_web" method="POST" enctype="multipart/form-data">
            <div class="form-group">
                <label for="webzip">Chọn file ZIP mã nguồn web (.zip)</label>
                <input type="file" name="webzip" id="webzip" class="form-control" style="padding-top: 10px;" accept=".zip" required>
            </div>
            <button type="submit" class="btn btn-primary" style="margin-top: 10px;" data-confirm="Xác nhận cập nhật website? File trùng tên sẽ bị ghi đè (trừ config.php)!">Cập Nhật Web</button>
        </form>
    </div>

    <div style="display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 2rem;">
        <div class="card" style="background: var(--input-bg); border: 1px solid var(--border-color); padding: 30px; border-radius: var(--radius); text-align: center;">
            <p style="color: var(--text-muted); margin: 0 0 10px 0; font-size: 0.875rem; font-weight: 500;">Tổng Thành Viên</p>
            <div style="font-size: 3rem; font-weight: 800; color: var(--text-color);"><?= $data['totalUsers'] ?></div>
        </div>

        <div class="card" style="background: var(--input-bg); border: 1px solid var(--border-color); padding: 30px; border-radius: var(--radius); text-align: center;">
            <p style="color: var(--text-muted); margin: 0 0 10px 0; font-size: 0.875rem; font-weight: 500;">Tổng Release</p>
            <div style="font-size: 3rem; font-weight: 800; color: var(--text-color);"><?= $data['totalFiles'] ?></div>
        </div>
    </div>
</div>
