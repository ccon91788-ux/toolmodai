import os
import re
import threading
import urllib.request
import urllib.error
import concurrent.futures
import tkinter as tk
from tkinter import ttk, scrolledtext, messagebox

BASE_URL = "https://electroheavenvn.github.io/DataNRO/TeaMobi"
HEADERS = {'User-Agent': 'Mozilla/5.0'}

JSON_LINKS = {
    "Maps": f"{BASE_URL}/Server1/Maps.json",
    "ItemTemplates": f"{BASE_URL}/Server1/ItemTemplates.json",
    "ItemOptionTemplates": f"{BASE_URL}/Server1/ItemOptionTemplates.json",
    "MobTemplates": f"{BASE_URL}/Server1/MobTemplates.json",
    "NpcTemplates": f"{BASE_URL}/Server1/NpcTemplates.json",
    "NClasses": f"{BASE_URL}/Server1/NClasses.json",
    "Parts": f"{BASE_URL}/Server1/Parts.json",
    "LastUpdated": f"{BASE_URL}/Server1/LastUpdated"
}

IMAGE_PATHS = {
    "Icons": f"{BASE_URL}/Icons/{{0}}.png",
    "Maps": f"{BASE_URL}/Maps/{{0}}.png",
    "NPCs": f"{BASE_URL}/NPCs/{{0}}.png",
    "Monsters": f"{BASE_URL}/Monsters/{{0}}.png"
}

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
DATA_DIR = os.path.join(SCRIPT_DIR, "Data")
JSON_DIR = os.path.join(DATA_DIR, "JSON")
IMAGE_DIR = os.path.join(DATA_DIR, "Images")



class DownloaderApp:
    def __init__(self, root):
        self.root = root
        self.root.title("NRO Data Downloader - Giao Diện Kéo Data Cực Tốc")
        self.root.geometry("750x620")
        self.root.configure(bg="#2E3440")
        
        # Thiết lập style (Theme Dark Mode cực xịn)
        style = ttk.Style()
        style.theme_use("clam")
        style.configure("TFrame", background="#2E3440")
        style.configure("TLabel", background="#2E3440", foreground="#D8DEE9", font=("Segoe UI", 10))
        style.configure("Header.TLabel", font=("Segoe UI", 16, "bold"), foreground="#88C0D0")
        style.configure("TButton", background="#4C566A", foreground="#ECEFF4", font=("Segoe UI", 10, "bold"), padding=8)
        style.map("TButton", background=[("active", "#5E81AC"), ("disabled", "#3B4252")])
        style.configure("Horizontal.TProgressbar", background="#A3BE8C", troughcolor="#3B4252", bordercolor="#2E3440")

        # Cấu trúc UI chính
        main_frame = ttk.Frame(self.root, padding=20)
        main_frame.pack(fill=tk.BOTH, expand=True)

        title = ttk.Label(main_frame, text="🐉 NRO DATA FETCHER - JSON & IMAGES ONLINE", style="Header.TLabel")
        title.pack(pady=(0, 20))

        # Lưu danh sách button để dễ khoá/mở
        self.buttons = []
        
        # Lưới Button
        btn_frame = ttk.Frame(main_frame)
        btn_frame.pack(fill=tk.X, pady=10)
        
        configs = [
            ("1. Tải toàn bộ JSON (Bắt buộc chạy trước)", self.download_json_files, 0, 0, 3),
            ("2. Kéo ảnh Icons (Items, Skills, ...)", self.download_icons, 1, 0, 1),
            ("3. Kéo ảnh Maps (Bản đồ)", self.download_maps, 1, 1, 1),
            ("4. Kéo ảnh NPCs", self.download_npcs, 2, 0, 1),
            ("5. Kéo ảnh Mobs (Quái vật)", self.download_monsters, 2, 1, 1),
            ("6. 🔥 TẢI TẤT CẢ A-Z 🔥", self.download_all, 1, 2, 1, 2) # rowspan=2 cho nổi bật
        ]
        
        for cfg in configs:
            if len(cfg) == 5:
                text, func, r, c, cspan = cfg
                rspan = 1
            else:
                text, func, r, c, cspan, rspan = cfg
                
            btn = ttk.Button(btn_frame, text=text, command=lambda f=func: self.start_task(f))
            btn.grid(row=r, column=c, columnspan=cspan, rowspan=rspan, padx=5, pady=5, sticky="nsew")
            self.buttons.append(btn)
            
        for i in range(3):
            btn_frame.columnconfigure(i, weight=1)
            
        # Thanh trạng thái (Progress Bar)
        self.progress_var = tk.DoubleVar()
        self.progress_bar = ttk.Progressbar(main_frame, variable=self.progress_var, maximum=100, length=100, style="Horizontal.TProgressbar")
        self.progress_bar.pack(fill=tk.X, pady=(20, 5))
        
        self.status_label = ttk.Label(main_frame, text="✨ Trạng thái: Đang chờ...", font=("Segoe UI", 10, "italic"), foreground="#EBCB8B")
        self.status_label.pack(anchor="w")
        
        # Khung Log hiển thị tiến độ
        self.log_text = scrolledtext.ScrolledText(main_frame, bg="#242933", fg="#A3BE8C", font=("Consolas", 10), height=14)
        self.log_text.pack(fill=tk.BOTH, expand=True, pady=(15, 0))
        self.log_text.config(state=tk.DISABLED)
        
    def log(self, text, color="#A3BE8C"):
        self.log_text.config(state=tk.NORMAL)
        self.log_text.insert(tk.END, text + "\n")
        self.log_text.see(tk.END)
        self.log_text.config(state=tk.DISABLED)

    def update_status(self, text, progress=None):
        self.status_label.config(text=f"✨ {text}")
        if progress is not None:
            self.progress_var.set(progress)

    def start_task(self, target_func):
        for btn in self.buttons:
            btn.config(state=tk.DISABLED)
        threading.Thread(target=self.run_task_wrapper, args=(target_func,), daemon=True).start()

    def run_task_wrapper(self, target_func):
        try:
            target_func()
        except Exception as e:
            self.log(f"LỖI TOÁN CỤC: {e}", color="#BF616A")
        finally:
            self.root.after(0, self.enable_buttons)
            
    def enable_buttons(self):
        for btn in self.buttons:
            btn.config(state=tk.NORMAL)

    def download_file(self, url, target_path):
        try:
            req = urllib.request.Request(url, headers=HEADERS)
            with urllib.request.urlopen(req) as response:
                with open(target_path, 'wb') as f:
                    f.write(response.read())
            return True, url
        except urllib.error.HTTPError as e:
            return False, f"HTTP {e.code}"
        except Exception as e:
            return False, str(e)

    def download_json_files(self):
        self.log("\n--- BẮT ĐẦU TẢI DỮ LIỆU JSON ---")
        os.makedirs(JSON_DIR, exist_ok=True)
        self.update_status("Đang tải dữ liệu JSON (0%)...", 0)
        
        total = len(JSON_LINKS)
        completed = 0
        with concurrent.futures.ThreadPoolExecutor(max_workers=5) as executor:
            futures = {}
            for name, url in JSON_LINKS.items():
                ext = ".json" if name != "LastUpdated" else ""
                target = os.path.join(JSON_DIR, f"{name}{ext}")
                futures[executor.submit(self.download_file, url, target)] = name
                
            for future in concurrent.futures.as_completed(futures):
                name = futures[future]
                completed += 1
                success, msg = future.result()
                if success:
                    self.log(f"[OK] Đã lưu \t{name}")
                else:
                    self.log(f"[LỖI] {name}: {msg}")
                self.update_status(f"Đang tải dữ liệu JSON... ({completed}/{total})", int((completed/total)*100))
                
        self.update_status("Hoàn tất tải dữ liệu JSON!", 100)

    def extract_ids(self, filename, regex_pattern):
        ids = set()
        path = os.path.join(JSON_DIR, filename)
        if not os.path.exists(path):
            return ids
        try:
            with open(path, 'r', encoding='utf-8') as f:
                content = f.read()
                matches = re.findall(regex_pattern, content)
                for match in matches:
                    ids.add(int(match))
        except Exception as e:
            self.log(f"Lỗi đọc {filename}: {e}")
        return ids

    def download_images(self, category, id_set):
        target_dir = os.path.join(IMAGE_DIR, category)
        os.makedirs(target_dir, exist_ok=True)
        url_template = IMAGE_PATHS[category]
        
        # Bỏ qua các file đã tải rồi
        to_download = []
        for uid in id_set:
            file_path = os.path.join(target_dir, f"{uid}.png")
            if not os.path.exists(file_path) or os.path.getsize(file_path) == 0:
                to_download.append(uid)
                
        self.log(f"\n--- BẮT ĐẦU KÉO ẢNH [{category.upper()}] ---")
        self.log(f"Tổng số ID nhận diện: {len(id_set)}")
        self.log(f"Cần tải mới (Skip trùng): {len(to_download)}")
        
        if len(to_download) == 0:
            self.update_status(f"Đã cập nhật đủ ảnh {category}!", 100)
            return

        success_count = 0
        fail_count = 0
        total = len(to_download)
        
        # Tốc độ tải cực nhanh với 30 luồng
        with concurrent.futures.ThreadPoolExecutor(max_workers=30) as executor:
            futures = {}
            for uid in to_download:
                url = url_template.format(uid)
                target = os.path.join(target_dir, f"{uid}.png")
                futures[executor.submit(self.download_file, url, target)] = uid
                
            for cnt, future in enumerate(concurrent.futures.as_completed(futures), 1):
                success, msg = future.result()
                if success:
                    success_count += 1
                else:
                    fail_count += 1
                    
                if cnt % 50 == 0 or cnt == total:
                    pct = int((cnt/total)*100)
                    self.update_status(f"Đang tải {category}... ({cnt}/{total})", pct)

        self.update_status(f"Hoàn tất kéo {category}!", 100)
        self.log(f"Thành công: {success_count} ảnh | Bỏ qua/Lỗi: {fail_count} ảnh")

    # ----- WRAPPERS THỰC HIỆN -----
    def download_icons(self):
        icons = set()
        if os.path.exists(JSON_DIR):
            for file in os.listdir(JSON_DIR):
                if file.endswith(".json"):
                    icons.update(self.extract_ids(file, r'"icon"\s*:\s*(\d+)'))
                    icons.update(self.extract_ids(file, r'"iconId"\s*:\s*(\d+)'))
        if not icons:
            self.log("⚠️ KHÔNG TÌM THẤY ICONS! Vui lòng tải JSON (Nút 1) trước.", color="#BF616A")
            return
        self.download_images("Icons", icons)

    def download_maps(self):
        ids = self.extract_ids("Maps.json", r'"id"\s*:\s*(\d+)')
        if not ids:
            self.log("⚠️ Không tìm thấy Map ID. Vui lòng tải JSON (Nút 1) trước.")
            return
        self.download_images("Maps", ids)

    def download_npcs(self):
        ids = self.extract_ids("NpcTemplates.json", r'"(?:id|npcTemplateId)"\s*:\s*(\d+)')
        if not ids:
            self.log("⚠️ Không tìm thấy NPC ID. Vui lòng tải JSON (Nút 1) trước.")
            return
        self.download_images("NPCs", ids)

    def download_monsters(self):
        ids = self.extract_ids("MobTemplates.json", r'"(?:id|mobTemplateId)"\s*:\s*(\d+)')
        if not ids:
            self.log("⚠️ Không tìm thấy Mob ID. Vui lòng tải JSON (Nút 1) trước.")
            return
        self.download_images("Monsters", ids)

    def download_all(self):
        self.log("🚀 KHỞI ĐỘNG CHUỖI TẢI TOÀN BỘ DATA 🚀")
        self.download_json_files()
        
        # Quét icons
        icons = set()
        if os.path.exists(JSON_DIR):
            for file in os.listdir(JSON_DIR):
                if file.endswith(".json"):
                    icons.update(self.extract_ids(file, r'"icon"\s*:\s*(\d+)'))
                    icons.update(self.extract_ids(file, r'"iconId"\s*:\s*(\d+)'))
        self.download_images("Icons", icons)
        
        map_ids = self.extract_ids("Maps.json", r'"(?:id|mapTemplateId)"\s*:\s*(\d+)')
        self.download_images("Maps", map_ids)
        
        npc_ids = self.extract_ids("NpcTemplates.json", r'"(?:id|npcTemplateId)"\s*:\s*(\d+)')
        self.download_images("NPCs", npc_ids)
        
        mob_ids = self.extract_ids("MobTemplates.json", r'"(?:id|mobTemplateId)"\s*:\s*(\d+)')
        self.download_images("Monsters", mob_ids)
        
        self.log("\n🎉 HOÀN TẤT TẢI TOÀN BỘ. MỌI DỮ LIỆU ĐÃ CÓ TRONG THƯ MỤC 'Data'. 🎉")
        self.update_status("THÀNH CÔNG VÀ SẴN SÀNG!", 100)


def main():
    root = tk.Tk()
    app = DownloaderApp(root)
    root.mainloop()

if __name__ == "__main__":
    main()
