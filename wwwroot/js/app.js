/**
 * EduManager RESTful API - Client-side SPA Application
 * Connects directly to ASP.NET Core Web API endpoints
 */

const API_BASE = '/api';

// Application State
const state = {
  currentTab: 'sinhvien',
  sinhVienQuery: {
    searchTerm: '',
    lopHocId: '',
    gioiTinh: '',
    trangThai: '',
    xepLoai: '',
    sortBy: 'Id',
    isDescending: false,
    pageIndex: 1,
    pageSize: 8
  },
  lopHocs: [],
  sinhVienData: null,
  thongKeData: null,
  editingStudentId: null,
  editingClassId: null,
  theme: localStorage.getItem('edumanager_theme') || 'dark'
};

// DOM Content Loaded Entrypoint
document.addEventListener('DOMContentLoaded', () => {
  initTheme();
  setupEventListeners();
  loadInitialData();
});

// Theme Initialization
function initTheme() {
  document.documentElement.setAttribute('data-theme', state.theme);
  const themeBtn = document.getElementById('themeToggleBtn');
  if (themeBtn) {
    themeBtn.innerHTML = state.theme === 'dark' ? '☀️ Sáng' : '🌙 Tối';
  }
}

function toggleTheme() {
  state.theme = state.theme === 'dark' ? 'light' : 'dark';
  localStorage.setItem('edumanager_theme', state.theme);
  initTheme();
}

// Initial Data Load
async function loadInitialData() {
  await Promise.all([
    loadLopHocs(),
    loadSinhViens(),
    loadThongKe()
  ]);
}

// Event Listeners Setup
function setupEventListeners() {
  // Theme toggle
  document.getElementById('themeToggleBtn')?.addEventListener('click', toggleTheme);

  // Refresh data
  document.getElementById('refreshBtn')?.addEventListener('click', () => {
    showToast('Đang làm mới dữ liệu từ API...', 'info');
    loadInitialData();
  });

  // Tab switching
  document.querySelectorAll('.tab-btn').forEach(btn => {
    btn.addEventListener('click', () => {
      const tabTarget = btn.getAttribute('data-tab');
      switchTab(tabTarget);
    });
  });

  // Search input debounce
  const searchInput = document.getElementById('searchStudentInput');
  let searchTimeout;
  searchInput?.addEventListener('input', (e) => {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
      state.sinhVienQuery.searchTerm = e.target.value;
      state.sinhVienQuery.pageIndex = 1;
      loadSinhViens();
    }, 350);
  });

  // Filters
  document.getElementById('filterLopHoc')?.addEventListener('change', (e) => {
    state.sinhVienQuery.lopHocId = e.target.value;
    state.sinhVienQuery.pageIndex = 1;
    loadSinhViens();
  });

  document.getElementById('filterGioiTinh')?.addEventListener('change', (e) => {
    state.sinhVienQuery.gioiTinh = e.target.value;
    state.sinhVienQuery.pageIndex = 1;
    loadSinhViens();
  });

  document.getElementById('filterXepLoai')?.addEventListener('change', (e) => {
    state.sinhVienQuery.xepLoai = e.target.value;
    state.sinhVienQuery.pageIndex = 1;
    loadSinhViens();
  });

  document.getElementById('filterTrangThai')?.addEventListener('change', (e) => {
    state.sinhVienQuery.trangThai = e.target.value;
    state.sinhVienQuery.pageIndex = 1;
    loadSinhViens();
  });

  // Student Form & Modals
  document.getElementById('openAddStudentModalBtn')?.addEventListener('click', () => openStudentModal());
  document.getElementById('closeStudentModalBtn')?.addEventListener('click', closeStudentModal);
  document.getElementById('cancelStudentModalBtn')?.addEventListener('click', closeStudentModal);
  document.getElementById('studentForm')?.addEventListener('submit', handleStudentFormSubmit);

  // Class Form & Modals
  document.getElementById('openAddClassModalBtn')?.addEventListener('click', () => openClassModal());
  document.getElementById('closeClassModalBtn')?.addEventListener('click', closeClassModal);
  document.getElementById('cancelClassModalBtn')?.addEventListener('click', closeClassModal);
  document.getElementById('classForm')?.addEventListener('submit', handleClassFormSubmit);

  // Confirm Modal
  document.getElementById('closeConfirmModalBtn')?.addEventListener('click', closeConfirmModal);
  document.getElementById('cancelConfirmModalBtn')?.addEventListener('click', closeConfirmModal);

  // Export Excel
  document.getElementById('exportExcelBtn')?.addEventListener('click', exportStudentsToExcel);

  // Table Column Sort
  document.querySelectorAll('th.sortable').forEach(th => {
    th.addEventListener('click', () => {
      const sortField = th.getAttribute('data-sort');
      if (state.sinhVienQuery.sortBy === sortField) {
        state.sinhVienQuery.isDescending = !state.sinhVienQuery.isDescending;
      } else {
        state.sinhVienQuery.sortBy = sortField;
        state.sinhVienQuery.isDescending = false;
      }
      updateSortIcons();
      loadSinhViens();
    });
  });

  // REST API Tester
  document.querySelectorAll('.endpoint-btn').forEach(btn => {
    btn.addEventListener('click', () => {
      document.querySelectorAll('.endpoint-btn').forEach(b => b.classList.remove('active'));
      btn.classList.add('active');

      const method = btn.getAttribute('data-method');
      const url = btn.getAttribute('data-url');
      const body = btn.getAttribute('data-body') || '';

      document.getElementById('apiTesterMethod').value = method;
      document.getElementById('apiTesterUrl').value = url;
      document.getElementById('apiTesterBody').value = body;
    });
  });

  document.getElementById('apiTesterSendBtn')?.addEventListener('click', executeApiTester);
}

// Switch Tab
function switchTab(tabId) {
  state.currentTab = tabId;
  document.querySelectorAll('.tab-btn').forEach(b => b.classList.toggle('active', b.getAttribute('data-tab') === tabId));
  document.querySelectorAll('.tab-content').forEach(c => c.classList.toggle('active', c.id === `tab-${tabId}`));

  if (tabId === 'thongke') {
    loadThongKe();
  }
}

// ==========================================
// 1. SINH VIEN (STUDENTS) LOGIC
// ==========================================
async function loadSinhViens() {
  const tableBody = document.getElementById('sinhVienTableBody');
  if (tableBody) {
    tableBody.innerHTML = `<tr><td colspan="7" class="empty-state"><span class="loading-spinner"></span> Đang tải danh sách sinh viên...</td></tr>`;
  }

  try {
    const params = new URLSearchParams();
    if (state.sinhVienQuery.searchTerm) params.append('searchTerm', state.sinhVienQuery.searchTerm);
    if (state.sinhVienQuery.lopHocId) params.append('lopHocId', state.sinhVienQuery.lopHocId);
    if (state.sinhVienQuery.gioiTinh) params.append('gioiTinh', state.sinhVienQuery.gioiTinh);
    if (state.sinhVienQuery.trangThai) params.append('trangThai', state.sinhVienQuery.trangThai);
    if (state.sinhVienQuery.xepLoai) params.append('xepLoai', state.sinhVienQuery.xepLoai);
    params.append('sortBy', state.sinhVienQuery.sortBy);
    params.append('isDescending', state.sinhVienQuery.isDescending);
    params.append('pageIndex', state.sinhVienQuery.pageIndex);
    params.append('pageSize', state.sinhVienQuery.pageSize);

    const res = await fetch(`${API_BASE}/sinhvien?${params.toString()}`);
    const result = await res.json();

    if (result.success && result.data) {
      state.sinhVienData = result.data;
      renderSinhVienTable(result.data.items);
      renderPagination(result.data);
    } else {
      showToast(result.message || 'Không thể tải danh sách sinh viên', 'error');
    }
  } catch (err) {
    console.error('Error fetching students:', err);
    showToast('Lỗi kết nối API lấy danh sách sinh viên', 'error');
    if (tableBody) {
      tableBody.innerHTML = `<tr><td colspan="7" class="empty-state">Không thể kết nối đến máy chủ API. Vui lòng kiểm tra lại dịch vụ!</td></tr>`;
    }
  }
}

function renderSinhVienTable(students) {
  const tableBody = document.getElementById('sinhVienTableBody');
  if (!tableBody) return;

  if (!students || students.length === 0) {
    tableBody.innerHTML = `
      <tr>
        <td colspan="7" class="empty-state">
          <div class="empty-icon">📂</div>
          <p>Không tìm thấy sinh viên nào phù hợp với bộ lọc hiện tại.</p>
        </td>
      </tr>`;
    return;
  }

  tableBody.innerHTML = students.map(s => {
    // Score Badge Styling
    let scoreClass = 'score-low';
    if (s.diemTrungBinh >= 9.0) scoreClass = 'score-high';
    else if (s.diemTrungBinh >= 8.0) scoreClass = 'score-high';
    else if (s.diemTrungBinh >= 6.5) scoreClass = 'score-mid';
    else if (s.diemTrungBinh >= 5.0) scoreClass = 'score-avg';

    // Status Badge Styling
    let statusBadge = 'badge-success';
    if (s.trangThai === 'Bảo lưu') statusBadge = 'badge-warning';
    else if (s.trangThai === 'Đình chỉ') statusBadge = 'badge-danger';
    else if (s.trangThai === 'Đã tốt nghiệp') statusBadge = 'badge-purple';

    const formattedDob = s.ngaySinh ? new Date(s.ngaySinh).toLocaleDateString('vi-VN') : '—';
    const firstLetter = s.hoTen ? s.hoTen.trim().split(' ').pop().charAt(0) : 'S';

    return `
      <tr>
        <td><span class="badge badge-code">${escapeHtml(s.maSV)}</span></td>
        <td>
          <div class="student-cell">
            <div class="avatar-badge">${escapeHtml(firstLetter)}</div>
            <div class="student-info-meta">
              <span class="student-name">${escapeHtml(s.hoTen)}</span>
              <span class="student-contact">${escapeHtml(s.email)} • ${escapeHtml(s.soDienThoai)}</span>
            </div>
          </div>
        </td>
        <td>
          <div>${formattedDob}</div>
          <small style="color: var(--text-dim);">${escapeHtml(s.gioiTinh)}</small>
        </td>
        <td>
          <div style="font-weight: 600;">${escapeHtml(s.tenLop || s.maLop || 'Chưa xếp lớp')}</div>
          <small style="color: var(--text-dim);">${escapeHtml(s.khoa || '')}</small>
        </td>
        <td>
          <div style="display: flex; align-items: center; gap: 8px;">
            <span class="score-pill ${scoreClass}">${s.diemTrungBinh.toFixed(1)}</span>
            <span class="badge ${scoreClass === 'score-high' ? 'badge-info' : 'badge-subtle'}">${escapeHtml(s.xepLoai)}</span>
          </div>
        </td>
        <td><span class="badge ${statusBadge}">${escapeHtml(s.trangThai)}</span></td>
        <td>
          <div class="action-buttons">
            <button class="btn-table-action edit" title="Chỉnh sửa" onclick="editStudent(${s.id})">✏️</button>
            <button class="btn-table-action delete" title="Xóa" onclick="confirmDeleteStudent(${s.id}, '${escapeHtml(s.hoTen)}')">🗑️</button>
          </div>
        </td>
      </tr>
    `;
  }).join('');
}

function renderPagination(paged) {
  const infoEl = document.getElementById('paginationInfo');
  const controlsEl = document.getElementById('paginationControls');
  if (!infoEl || !controlsEl) return;

  const startRecord = paged.totalCount === 0 ? 0 : (paged.pageIndex - 1) * paged.pageSize + 1;
  const endRecord = Math.min(paged.pageIndex * paged.pageSize, paged.totalCount);

  infoEl.innerText = `Hiển thị ${startRecord} - ${endRecord} trên tổng số ${paged.totalCount} sinh viên`;

  let buttonsHtml = `
    <button class="page-btn" ${!paged.hasPreviousPage ? 'disabled' : ''} onclick="goToPage(${paged.pageIndex - 1})">❮</button>
  `;

  for (let i = 1; i <= paged.totalPages; i++) {
    // Show first, last, and window around current page
    if (i === 1 || i === paged.totalPages || (i >= paged.pageIndex - 1 && i <= paged.pageIndex + 1)) {
      buttonsHtml += `<button class="page-btn ${i === paged.pageIndex ? 'active' : ''}" onclick="goToPage(${i})">${i}</button>`;
    } else if (i === paged.pageIndex - 2 || i === paged.pageIndex + 2) {
      buttonsHtml += `<span style="padding: 0 4px; color: var(--text-dim);">...</span>`;
    }
  }

  buttonsHtml += `
    <button class="page-btn" ${!paged.hasNextPage ? 'disabled' : ''} onclick="goToPage(${paged.pageIndex + 1})">❯</button>
  `;

  controlsEl.innerHTML = buttonsHtml;
}

window.goToPage = function(page) {
  if (page < 1 || (state.sinhVienData && page > state.sinhVienData.totalPages)) return;
  state.sinhVienQuery.pageIndex = page;
  loadSinhViens();
};

function updateSortIcons() {
  document.querySelectorAll('th.sortable').forEach(th => {
    const field = th.getAttribute('data-sort');
    const icon = th.querySelector('.sort-icon');
    if (icon) {
      if (state.sinhVienQuery.sortBy === field) {
        icon.innerText = state.sinhVienQuery.isDescending ? ' ▼' : ' ▲';
      } else {
        icon.innerText = ' ⇅';
      }
    }
  });
}

// Student Modal & Form Actions
function openStudentModal(student = null) {
  const modal = document.getElementById('studentModal');
  const title = document.getElementById('studentModalTitle');
  const form = document.getElementById('studentForm');
  if (!modal || !form) return;

  form.reset();
  populateClassSelect('studentLopHocSelect');

  if (student) {
    state.editingStudentId = student.id;
    title.innerText = 'Chỉnh Sửa Thông Tin Sinh Viên';
    document.getElementById('studentMaSV').value = student.maSV;
    document.getElementById('studentHoTen').value = student.hoTen;
    document.getElementById('studentNgaySinh').value = student.ngaySinh ? student.ngaySinh.split('T')[0] : '';
    document.getElementById('studentGioiTinh').value = student.gioiTinh;
    document.getElementById('studentEmail').value = student.email;
    document.getElementById('studentPhone').value = student.soDienThoai;
    document.getElementById('studentDiaChi').value = student.diaChi || '';
    document.getElementById('studentDiem').value = student.diemTrungBinh;
    document.getElementById('studentTrangThai').value = student.trangThai;
    document.getElementById('studentLopHocSelect').value = student.lopHocId;
  } else {
    state.editingStudentId = null;
    title.innerText = 'Thêm Mới Sinh Viên';
    // Generate suggested Student ID
    document.getElementById('studentMaSV').value = 'SV' + new Date().getFullYear().toString() + Math.floor(100 + Math.random() * 900);
    document.getElementById('studentDiem').value = '8.0';
    document.getElementById('studentTrangThai').value = 'Đang học';
  }

  modal.classList.add('active');
}

function closeStudentModal() {
  const modal = document.getElementById('studentModal');
  modal?.classList.remove('active');
  state.editingStudentId = null;
}

window.editStudent = async function(id) {
  try {
    const res = await fetch(`${API_BASE}/sinhvien/${id}`);
    const result = await res.json();
    if (result.success && result.data) {
      openStudentModal(result.data);
    } else {
      showToast(result.message || 'Không tìm thấy sinh viên', 'error');
    }
  } catch (err) {
    console.error('Error loading student detail:', err);
    showToast('Lỗi khi tải thông tin chi tiết sinh viên', 'error');
  }
};

async function handleStudentFormSubmit(e) {
  e.preventDefault();

  const payload = {
    maSV: document.getElementById('studentMaSV').value.trim(),
    hoTen: document.getElementById('studentHoTen').value.trim(),
    ngaySinh: document.getElementById('studentNgaySinh').value,
    gioiTinh: document.getElementById('studentGioiTinh').value,
    email: document.getElementById('studentEmail').value.trim(),
    soDienThoai: document.getElementById('studentPhone').value.trim(),
    diaChi: document.getElementById('studentDiaChi').value.trim(),
    diemTrungBinh: parseFloat(document.getElementById('studentDiem').value) || 0,
    trangThai: document.getElementById('studentTrangThai').value,
    lopHocId: parseInt(document.getElementById('studentLopHocSelect').value)
  };

  if (!payload.lopHocId) {
    showToast('Vui lòng chọn lớp học cho sinh viên!', 'error');
    return;
  }

  const isEdit = !!state.editingStudentId;
  const url = isEdit ? `${API_BASE}/sinhvien/${state.editingStudentId}` : `${API_BASE}/sinhvien`;
  const method = isEdit ? 'PUT' : 'POST';

  try {
    const res = await fetch(url, {
      method: method,
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    const result = await res.json();
    if (result.success) {
      showToast(result.message || (isEdit ? 'Cập nhật thành công!' : 'Thêm mới thành công!'), 'success');
      closeStudentModal();
      loadSinhViens();
      loadThongKe();
    } else {
      const errDetail = result.errors && result.errors.length ? result.errors.join('; ') : result.message;
      showToast(errDetail || 'Không thể lưu thông tin sinh viên', 'error');
    }
  } catch (err) {
    console.error('Error saving student:', err);
    showToast('Lỗi kết nối máy chủ API', 'error');
  }
}

window.confirmDeleteStudent = function(id, name) {
  openConfirmModal(
    'Xác Nhận Xóa Sinh Viên',
    `Bạn có chắc chắn muốn xóa sinh viên <strong>${escapeHtml(name)}</strong> khỏi hệ thống không? Dữ liệu này sẽ không thể khôi phục.`,
    async () => {
      try {
        const res = await fetch(`${API_BASE}/sinhvien/${id}`, { method: 'DELETE' });
        const result = await res.json();
        if (result.success) {
          showToast(result.message || 'Đã xóa sinh viên thành công!', 'success');
          loadSinhViens();
          loadThongKe();
        } else {
          showToast(result.message || 'Lỗi khi xóa sinh viên', 'error');
        }
      } catch (err) {
        showToast('Lỗi khi gọi API xóa sinh viên', 'error');
      }
    }
  );
};

// ==========================================
// 2. LOP HOC (CLASSES) LOGIC
// ==========================================
async function loadLopHocs() {
  try {
    const res = await fetch(`${API_BASE}/lophoc`);
    const result = await res.json();
    if (result.success && result.data) {
      state.lopHocs = result.data;
      renderClassGrid(result.data);
      populateClassSelect('filterLopHoc', true);
      populateClassSelect('studentLopHocSelect');
    }
  } catch (err) {
    console.error('Error fetching classes:', err);
  }
}

function renderClassGrid(classes) {
  const container = document.getElementById('classGridContainer');
  if (!container) return;

  if (!classes || classes.length === 0) {
    container.innerHTML = `
      <div class="empty-state" style="grid-column: 1 / -1;">
        <div class="empty-icon">🏫</div>
        <p>Chưa có lớp học nào. Hãy nhấn "Thêm Lớp Mới" để tạo lớp đầu tiên!</p>
      </div>`;
    return;
  }

  container.innerHTML = classes.map(c => `
    <div class="class-card">
      <div>
        <div class="class-card-header">
          <span class="badge badge-code">${escapeHtml(c.maLop)}</span>
          <span class="badge badge-info">${c.soLuongSinhVien} sinh viên</span>
        </div>
        <h3 class="class-card-title">${escapeHtml(c.tenLop)}</h3>
        <div class="class-dept">🏢 ${escapeHtml(c.khoa)}</div>
        <p class="class-desc">${escapeHtml(c.moTa || 'Chưa có mô tả bổ sung cho lớp học này.')}</p>
      </div>
      <div class="class-footer">
        <button class="btn btn-secondary btn-sm" onclick="filterByClass(${c.id})">🔍 Xem sinh viên</button>
        <div class="action-buttons">
          <button class="btn-table-action edit" title="Sửa lớp" onclick="editClass(${c.id})">✏️</button>
          <button class="btn-table-action delete" title="Xóa lớp" onclick="confirmDeleteClass(${c.id}, '${escapeHtml(c.tenLop)}')">🗑️</button>
        </div>
      </div>
    </div>
  `).join('');
}

function populateClassSelect(elementId, hasAllOption = false) {
  const select = document.getElementById(elementId);
  if (!select) return;

  let options = hasAllOption ? '<option value="">Tất cả lớp học</option>' : '<option value="">-- Chọn lớp học --</option>';
  options += state.lopHocs.map(c => `<option value="${c.id}">${escapeHtml(c.tenLop)} (${escapeHtml(c.maLop)})</option>`).join('');
  select.innerHTML = options;
}

window.filterByClass = function(classId) {
  state.sinhVienQuery.lopHocId = classId;
  const filterSelect = document.getElementById('filterLopHoc');
  if (filterSelect) filterSelect.value = classId;
  switchTab('sinhvien');
  loadSinhViens();
};

function openClassModal(cls = null) {
  const modal = document.getElementById('classModal');
  const title = document.getElementById('classModalTitle');
  const form = document.getElementById('classForm');
  if (!modal || !form) return;

  form.reset();

  if (cls) {
    state.editingClassId = cls.id;
    title.innerText = 'Chỉnh Sửa Lớp Học';
    document.getElementById('classMaLop').value = cls.maLop;
    document.getElementById('classTenLop').value = cls.tenLop;
    document.getElementById('classKhoa').value = cls.khoa;
    document.getElementById('classMoTa').value = cls.moTa || '';
  } else {
    state.editingClassId = null;
    title.innerText = 'Thêm Mới Lớp Học';
  }

  modal.classList.add('active');
}

function closeClassModal() {
  const modal = document.getElementById('classModal');
  modal?.classList.remove('active');
  state.editingClassId = null;
}

window.editClass = async function(id) {
  try {
    const res = await fetch(`${API_BASE}/lophoc/${id}`);
    const result = await res.json();
    if (result.success && result.data) {
      openClassModal(result.data);
    } else {
      showToast(result.message || 'Không tìm thấy lớp học', 'error');
    }
  } catch (err) {
    showToast('Lỗi khi tải thông tin lớp học', 'error');
  }
};

async function handleClassFormSubmit(e) {
  e.preventDefault();

  const payload = {
    maLop: document.getElementById('classMaLop').value.trim(),
    tenLop: document.getElementById('classTenLop').value.trim(),
    khoa: document.getElementById('classKhoa').value.trim(),
    moTa: document.getElementById('classMoTa').value.trim()
  };

  const isEdit = !!state.editingClassId;
  const url = isEdit ? `${API_BASE}/lophoc/${state.editingClassId}` : `${API_BASE}/lophoc`;
  const method = isEdit ? 'PUT' : 'POST';

  try {
    const res = await fetch(url, {
      method: method,
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    const result = await res.json();
    if (result.success) {
      showToast(result.message || 'Thao tác lớp học thành công!', 'success');
      closeClassModal();
      await loadLopHocs();
      loadThongKe();
    } else {
      showToast(result.message || 'Không thể lưu lớp học', 'error');
    }
  } catch (err) {
    showToast('Lỗi kết nối máy chủ API', 'error');
  }
}

window.confirmDeleteClass = function(id, name) {
  openConfirmModal(
    'Xác Nhận Xóa Lớp Học',
    `Bạn có chắc chắn muốn xóa lớp học <strong>${escapeHtml(name)}</strong>? Lưu ý: Nếu lớp đã có sinh viên thì hệ thống sẽ từ chối xóa để đảm bảo toàn vẹn dữ liệu.`,
    async () => {
      try {
        const res = await fetch(`${API_BASE}/lophoc/${id}`, { method: 'DELETE' });
        const result = await res.json();
        if (result.success) {
          showToast(result.message || 'Đã xóa lớp học thành công!', 'success');
          await loadLopHocs();
          loadThongKe();
        } else {
          showToast(result.message || 'Không thể xóa lớp học', 'error');
        }
      } catch (err) {
        showToast('Lỗi khi gọi API xóa lớp học', 'error');
      }
    }
  );
};

// ==========================================
// 3. THONG KE (STATISTICS & CHARTS)
// ==========================================
async function loadThongKe() {
  try {
    const res = await fetch(`${API_BASE}/thongke/tong-quan`);
    const result = await res.json();
    if (result.success && result.data) {
      state.thongKeData = result.data;
      renderStatCards(result.data);
      renderCharts(result.data);
    }
  } catch (err) {
    console.error('Error fetching statistics:', err);
  }
}

function renderStatCards(stats) {
  // Top Hero Cards
  document.getElementById('statTotalStudents').innerText = stats.tongSinhVien;
  document.getElementById('statTotalClasses').innerText = stats.tongLopHoc;
  document.getElementById('statAvgScore').innerText = stats.diemTrungBinhChung.toFixed(2);

  const goodCount = stats.soLuongXuatSac + stats.soLuongGioi;
  const goodPercent = stats.tongSinhVien > 0 ? Math.round((goodCount / stats.tongSinhVien) * 100) : 0;
  document.getElementById('statGoodRate').innerText = `${goodPercent}%`;
  document.getElementById('statGoodSub').innerText = `${goodCount}/${stats.tongSinhVien} SV Khá - Giỏi`;
}

function renderCharts(stats) {
  const container = document.getElementById('academicRankingBars');
  if (!container) return;

  const total = stats.tongSinhVien || 1;
  const rankings = [
    { label: 'Xuất sắc (>= 9.0)', count: stats.soLuongXuatSac, color: '#10b981' },
    { label: 'Giỏi (8.0 - 8.9)', count: stats.soLuongGioi, color: '#06b6d4' },
    { label: 'Khá (6.5 - 7.9)', count: stats.soLuongKha, color: '#6366f1' },
    { label: 'Trung bình (5.0 - 6.4)', count: stats.soLuongTrungBinh, color: '#f59e0b' },
    { label: 'Yếu (< 5.0)', count: stats.soLuongYeu, color: '#f43f5e' }
  ];

  container.innerHTML = rankings.map(r => {
    const pct = Math.round((r.count / total) * 100);
    return `
      <div class="bar-item">
        <div class="bar-label-group">
          <span>${r.label}</span>
          <span style="font-weight: 600; color: ${r.color};">${r.count} sinh viên (${pct}%)</span>
        </div>
        <div class="progress-track">
          <div class="progress-fill" style="width: ${pct}%; background: ${r.color};"></div>
        </div>
      </div>
    `;
  }).join('');

  // Status breakdown
  const statusContainer = document.getElementById('statusBreakdownBars');
  if (statusContainer) {
    const statuses = [
      { label: 'Đang theo học', count: stats.soLuongDangHoc, color: '#10b981' },
      { label: 'Đang bảo lưu', count: stats.soLuongBaoLuu, color: '#f59e0b' },
      { label: 'Đã tốt nghiệp', count: stats.soLuongDaTotNghiep, color: '#8b5cf6' },
      { label: 'Đình chỉ học vụ', count: stats.soLuongDinhChi, color: '#f43f5e' }
    ];

    statusContainer.innerHTML = statuses.map(s => {
      const pct = Math.round((s.count / total) * 100);
      return `
        <div class="bar-item">
          <div class="bar-label-group">
            <span>${s.label}</span>
            <span style="font-weight: 600; color: ${s.color};">${s.count} SV (${pct}%)</span>
          </div>
          <div class="progress-track">
            <div class="progress-fill" style="width: ${pct}%; background: ${s.color};"></div>
          </div>
        </div>
      `;
    }).join('');
  }

  // Class statistics table
  const classTable = document.getElementById('classStatsTableBody');
  if (classTable && stats.thongKeTheoLop) {
    classTable.innerHTML = stats.thongKeTheoLop.map(cl => `
      <tr>
        <td><strong>${escapeHtml(cl.tenLop)}</strong> <small style="color:var(--text-dim)">(${escapeHtml(cl.maLop)})</small></td>
        <td>${escapeHtml(cl.khoa)}</td>
        <td><span class="badge badge-info">${cl.soLuongSinhVien} SV</span></td>
        <td><strong style="color: var(--accent-emerald);">${cl.diemTrungBinhLop.toFixed(2)}</strong></td>
      </tr>
    `).join('');
  }
}

// ==========================================
// 4. REST API TESTER CONSOLE
// ==========================================
async function executeApiTester() {
  const method = document.getElementById('apiTesterMethod').value;
  const url = document.getElementById('apiTesterUrl').value;
  const bodyText = document.getElementById('apiTesterBody').value.trim();
  const outputEl = document.getElementById('apiTesterResponse');

  outputEl.innerText = 'Đang gửi yêu cầu...';

  const startTime = performance.now();

  try {
    const options = {
      method: method,
      headers: { 'Content-Type': 'application/json' }
    };

    if (['POST', 'PUT'].includes(method) && bodyText) {
      options.body = bodyText;
    }

    const res = await fetch(url, options);
    const duration = Math.round(performance.now() - startTime);
    const json = await res.json();

    outputEl.innerText = `// HTTP Status: ${res.status} ${res.statusText} (${duration} ms)\n\n` + JSON.stringify(json, null, 2);
  } catch (err) {
    outputEl.innerText = `// Lỗi khi thực hiện request: ${err.message}`;
  }
}

// ==========================================
// 5. HELPER UTILITIES
// ==========================================
function openConfirmModal(title, message, onConfirm) {
  const modal = document.getElementById('confirmModal');
  const titleEl = document.getElementById('confirmModalTitle');
  const msgEl = document.getElementById('confirmModalMessage');
  const confirmBtn = document.getElementById('confirmActionBtn');
  if (!modal) return;

  titleEl.innerText = title;
  msgEl.innerHTML = message;

  const handleConfirm = async () => {
    confirmBtn.removeEventListener('click', handleConfirm);
    closeConfirmModal();
    if (onConfirm) await onConfirm();
  };

  confirmBtn.onclick = handleConfirm;
  modal.classList.add('active');
}

function closeConfirmModal() {
  document.getElementById('confirmModal')?.classList.remove('active');
}

function showToast(message, type = 'info') {
  const container = document.getElementById('toastContainer');
  if (!container) return;

  const icons = {
    success: '✅',
    error: '❌',
    info: 'ℹ️'
  };

  const toast = document.createElement('div');
  toast.className = `toast toast-${type}`;
  toast.innerHTML = `<span>${icons[type] || 'ℹ️'}</span> <span>${escapeHtml(message)}</span>`;

  container.appendChild(toast);

  setTimeout(() => {
    toast.style.opacity = '0';
    toast.style.transform = 'translateX(100%)';
    toast.style.transition = 'all 0.3s ease-out';
    setTimeout(() => toast.remove(), 300);
  }, 3500);
}

async function exportStudentsToExcel() {
  if (typeof XLSX === 'undefined') {
    showToast('Thư viện Excel chưa sẵn sàng, vui lòng thử lại sau...', 'warning');
    return;
  }

  if (!state.sinhVienData || !state.sinhVienData.items || state.sinhVienData.items.length === 0) {
    showToast('Không có dữ liệu sinh viên để xuất', 'info');
    return;
  }

  let exportList = state.sinhVienData.items;

  // Nếu có nhiều sinh viên hơn số lượng đang hiển thị trên trang hiện tại, lấy toàn bộ danh sách khớp bộ lọc
  if (state.sinhVienData.totalCount > state.sinhVienData.items.length) {
    try {
      showToast('Đang chuẩn bị dữ liệu xuất toàn bộ sinh viên...', 'info');
      const params = new URLSearchParams();
      if (state.sinhVienQuery.searchTerm) params.append('searchTerm', state.sinhVienQuery.searchTerm);
      if (state.sinhVienQuery.lopHocId) params.append('lopHocId', state.sinhVienQuery.lopHocId);
      if (state.sinhVienQuery.gioiTinh) params.append('gioiTinh', state.sinhVienQuery.gioiTinh);
      if (state.sinhVienQuery.trangThai) params.append('trangThai', state.sinhVienQuery.trangThai);
      if (state.sinhVienQuery.xepLoai) params.append('xepLoai', state.sinhVienQuery.xepLoai);
      params.append('sortBy', state.sinhVienQuery.sortBy);
      params.append('isDescending', state.sinhVienQuery.isDescending);
      params.append('pageIndex', '1');
      params.append('pageSize', String(Math.min(state.sinhVienData.totalCount, 100)));

      const res = await fetch(`${API_BASE}/sinhvien?${params.toString()}`);
      const result = await res.json();
      if (result.success && result.data && result.data.items) {
        exportList = result.data.items;
      }
    } catch (err) {
      console.warn('Không thể tải toàn bộ danh sách, xuất các dòng trên trang hiện tại:', err);
    }
  }

  const formatDate = (dateStr) => {
    if (!dateStr) return '';
    const d = new Date(dateStr);
    if (isNaN(d.getTime())) return dateStr;
    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    return `${day}/${month}/${year}`;
  };

  const excelRows = exportList.map((sv, idx) => ({
    'STT': idx + 1,
    'Mã SV': sv.maSV,
    'Họ và tên': sv.hoTen,
    'Ngày sinh': formatDate(sv.ngaySinh),
    'Giới tính': sv.gioiTinh,
    'Lớp': sv.tenLop ? `${sv.tenLop} (${sv.maLop})` : (sv.maLop || ''),
    'Khoa': sv.khoa || '',
    'Điểm TB': Number(sv.diemTrungBinh ? sv.diemTrungBinh.toFixed(2) : 0),
    'Xếp loại': sv.xepLoai || '',
    'Trạng thái': sv.trangThai || '',
    'Email': sv.email || '',
    'Số điện thoại': sv.soDienThoai || '',
    'Địa chỉ': sv.diaChi || ''
  }));

  const worksheet = XLSX.utils.json_to_sheet(excelRows);

  // Tự động căn chỉnh độ rộng cột
  const colWidths = Object.keys(excelRows[0]).map(key => {
    let maxLen = key.length;
    excelRows.forEach(row => {
      const val = row[key] !== null && row[key] !== undefined ? String(row[key]) : '';
      if (val.length > maxLen) maxLen = val.length;
    });
    return { wch: Math.min(Math.max(maxLen + 3, 10), 40) };
  });
  worksheet['!cols'] = colWidths;

  const workbook = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(workbook, worksheet, 'Danh sách sinh viên');

  const today = new Date().toISOString().slice(0, 10);
  XLSX.writeFile(workbook, `danh_sach_sinh_vien_${today}.xlsx`);

  showToast(`Đã xuất ${excelRows.length} sinh viên sang file Excel thành công!`, 'success');
}

function escapeHtml(str) {
  if (!str) return '';
  return String(str)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#039;');
}
