/**
 * CART.JS - PHIÊN BẢN DEBUG & FIXED
 * Thêm console.log để debug và sửa lỗi
 */

document.addEventListener('DOMContentLoaded', function () {
    console.log('✅ Cart.js loaded');

    // 1. Khởi tạo
    updateHeaderCartCount();

    // 2. Kiểm tra có token không
    const token = getToken();
    if (!token) {
        console.error('❌ Anti-forgery token NOT FOUND! Check if @Html.AntiForgeryToken() exists in the page');
    } else {
        console.log('✅ Anti-forgery token found');
    }

    // 3. Event Delegation
    document.body.addEventListener('click', function (e) {
        // A. Nút "Thêm vào giỏ"
        const btnAdd = e.target.closest('.add-to-cart-btn');
        if (btnAdd) {
            console.log('🛒 Add to cart button clicked', btnAdd);
            e.preventDefault();
            handleAddToCart(btnAdd);
            return;
        }

        // B. Nút "Xóa"
        const btnRemove = e.target.closest('.btn-remove-item');
        if (btnRemove) {
            e.preventDefault();
            handleRemoveItem(btnRemove);
            return;
        }

        // C. Nút +/-
        const btnQty = e.target.closest('.btn-qty-action');
        if (btnQty) {
            e.preventDefault();
            handleQuantityButton(btnQty);
            return;
        }
    });
});

// ==========================================
// THÊM VÀO GIỎ HÀNG
// ==========================================
function handleAddToCart(btn) {
    console.log('📦 handleAddToCart called');

    if (btn.disabled) {
        console.log('⚠️ Button is disabled');
        return;
    }

    const productId = btn.getAttribute('data-id');
    const productName = btn.getAttribute('data-name') || 'Sản phẩm';

    console.log('📌 Product Info:', { productId, productName });

    if (!productId) {
        console.error('❌ Product ID is missing! Check data-id attribute');
        showToast('danger', 'Thiếu thông tin sản phẩm');
        return;
    }

    let quantity = 1;

    // Lấy số lượng từ input nếu có
    const qtyInputDetail = document.getElementById('product-quantity');
    if (qtyInputDetail && btn.id === 'btn-add-to-cart-detail') {
        quantity = parseInt(qtyInputDetail.value) || 1;
        console.log('📊 Quantity from input:', quantity);
    }

    const originalContent = btn.innerHTML;
    btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang thêm...';
    btn.disabled = true;

    const token = getToken();
    if (!token) {
        console.error('❌ Cannot get anti-forgery token');
        showToast('danger', 'Lỗi bảo mật! Vui lòng tải lại trang.');
        btn.innerHTML = originalContent;
        btn.disabled = false;
        return;
    }

    const formData = new FormData();
    formData.append('productId', productId);
    formData.append('quantity', quantity);
    formData.append('__RequestVerificationToken', token);

    console.log('📤 Sending request to /Cart/Add with:', {
        productId: productId,
        quantity: quantity,
        hasToken: !!token
    });

    fetch('/Cart/Add', {
        method: 'POST',
        body: formData
    })
        .then(res => {
            console.log('📥 Response status:', res.status);
            if (!res.ok) {
                throw new Error(`HTTP error! status: ${res.status}`);
            }
            return res.json();
        })
        .then(data => {
            console.log('✅ Response data:', data);

            if (data.success) {
                updateHeaderBadge(data.totalItems);
                showToast('success', `Đã thêm <b>${productName}</b> vào giỏ!`);

                btn.classList.add('btn-success');
                btn.innerHTML = '<i class="fas fa-check"></i> Đã thêm';
            } else {
                console.error('❌ Add to cart failed:', data.message);
                showToast('danger', data.message || 'Không thể thêm vào giỏ hàng');
                btn.innerHTML = originalContent;
            }
        })
        .catch(err => {
            console.error('❌ Fetch error:', err);
            showToast('danger', 'Lỗi kết nối! ' + err.message);
            btn.innerHTML = originalContent;
        })
        .finally(() => {
            setTimeout(() => {
                btn.innerHTML = originalContent;
                btn.classList.remove('btn-success');
                btn.disabled = false;
            }, 2000);
        });
}

// ==========================================
// XỬ LÝ SỐ LƯỢNG
// ==========================================
function handleQuantityButton(btn) {
    const action = btn.getAttribute('data-action');
    const input = btn.parentElement.querySelector('.cart-qty-input');

    if (!input) {
        console.error('❌ Quantity input not found');
        return;
    }

    const maxStock = parseInt(input.getAttribute('max')) || 999;
    let currentValue = parseInt(input.value) || 1;
    let newValue = currentValue;

    if (action === 'increase') {
        if (currentValue < maxStock) {
            newValue = currentValue + 1;
        } else {
            showToast('warning', 'Đã đạt giới hạn kho hàng.');
            return;
        }
    } else if (action === 'decrease') {
        if (currentValue > 1) {
            newValue = currentValue - 1;
        } else {
            return;
        }
    }

    input.value = newValue;
    triggerChange(input);
}

function handleUpdateQuantityAjax(inputElement) {
    const cartItemId = inputElement.getAttribute('data-id');
    const newQuantity = parseInt(inputElement.value);

    console.log('🔄 Updating quantity:', { cartItemId, newQuantity });

    if (newQuantity < 1) {
        showToast('warning', 'Số lượng tối thiểu là 1');
        return;
    }

    const formData = new FormData();
    formData.append('cartItemId', cartItemId);
    formData.append('quantity', newQuantity);
    formData.append('__RequestVerificationToken', getToken());

    fetch('/Cart/UpdateQuantity', {
        method: 'POST',
        body: formData
    })
        .then(res => res.json())
        .then(data => {
            console.log('✅ Update response:', data);
            if (data.success) {
                updateHeaderBadge(data.totalItems);
                showToast('success', 'Đã cập nhật số lượng');
                // Reload để cập nhật tổng tiền
                setTimeout(() => window.location.reload(), 500);
            } else {
                showToast('danger', data.message);
            }
        })
        .catch(err => {
            console.error('❌ Update error:', err);
            showToast('danger', 'Lỗi cập nhật!');
        });
}

// ==========================================
// XÓA SẢN PHẨM
// ==========================================
function handleRemoveItem(btn) {
    if (!confirm('Bạn có chắc muốn xóa sản phẩm này?')) return;

    const cartItemId = btn.getAttribute('data-id');
    const row = document.getElementById(`row-${cartItemId}`);

    console.log('🗑️ Removing item:', cartItemId);

    const formData = new FormData();
    formData.append('cartItemId', cartItemId);
    formData.append('__RequestVerificationToken', getToken());

    fetch('/Cart/Remove', {
        method: 'POST',
        body: formData
    })
        .then(res => res.json())
        .then(data => {
            console.log('✅ Remove response:', data);
            if (data.success) {
                if (row) row.remove();
                updateHeaderBadge(data.totalItems);

                if (data.totalItems === 0) {
                    setTimeout(() => window.location.reload(), 300);
                } else {
                    showToast('success', 'Đã xóa sản phẩm');
                    setTimeout(() => window.location.reload(), 500);
                }
            } else {
                showToast('danger', data.message);
            }
        })
        .catch(err => {
            console.error('❌ Remove error:', err);
            showToast('danger', 'Lỗi xóa sản phẩm!');
        });
}

// ==========================================
// HELPER FUNCTIONS
// ==========================================
function getToken() {
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    const token = tokenInput ? tokenInput.value : null;

    if (!token) {
        console.error('❌ Anti-forgery token not found in DOM');
    }

    return token;
}

function triggerChange(element) {
    const event = new Event('change');
    element.dispatchEvent(event);
}

function updateHeaderCartCount() {
    console.log('🔄 Updating header cart count...');

    fetch('/Cart/Count')
        .then(res => {
            if (!res.ok) throw new Error('Failed to get cart count');
            return res.json();
        })
        .then(data => {
            console.log('📊 Cart count:', data);
            if (data.success) {
                updateHeaderBadge(data.totalItems);
            }
        })
        .catch(err => {
            console.error('❌ Error getting cart count:', err);
        });
}

function updateHeaderBadge(count) {
    console.log('🔢 Updating badge to:', count);

    const badges = document.querySelectorAll('.cart-count, .badge-cart');
    badges.forEach(el => {
        el.innerText = count > 99 ? '99+' : count;
        el.style.display = count > 0 ? 'inline-block' : 'none';

        // Animation
        el.classList.remove('animate__bounceIn');
        void el.offsetWidth; // Trigger reflow
        el.classList.add('animate__animated', 'animate__bounceIn');
    });
}

function showToast(type, message) {
    let container = document.querySelector('.toast-container');
    if (!container) {
        container = document.createElement('div');
        container.className = 'toast-container position-fixed top-0 end-0 p-3';
        container.style.zIndex = '10000';
        document.body.appendChild(container);
    }

    const bgClass = {
        'success': 'text-bg-success',
        'danger': 'text-bg-danger',
        'warning': 'text-bg-warning',
        'info': 'text-bg-info'
    }[type] || 'text-bg-info';

    const icon = {
        'success': 'fa-check-circle',
        'danger': 'fa-times-circle',
        'warning': 'fa-exclamation-triangle',
        'info': 'fa-info-circle'
    }[type] || 'fa-info-circle';

    const toastId = 'toast-' + Date.now();
    const html = `
        <div id="${toastId}" class="toast align-items-center ${bgClass} border-0 shadow-lg" role="alert">
            <div class="d-flex">
                <div class="toast-body">
                    <i class="fas ${icon} me-2"></i>${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>
    `;

    container.insertAdjacentHTML('beforeend', html);
    const toastEl = document.getElementById(toastId);

    if (window.bootstrap && bootstrap.Toast) {
        const toast = new bootstrap.Toast(toastEl, { delay: 3500 });
        toast.show();
        toastEl.addEventListener('hidden.bs.toast', () => toastEl.remove());
    } else {
        toastEl.classList.add('show');
        setTimeout(() => toastEl.remove(), 3500);
    }
}