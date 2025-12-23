// wwwroot/js/cart-page.js
// Yêu cầu: site-cart-helpers.js đã được load trước file này (sử dụng window.siteCartHelpers.getCart/saveCart/updateCartCount/formatCurrency)

(function () {
    'use strict';

    // Helpers: sử dụng siteCartHelpers nếu có, ngược lại fallback nhẹ
    function getCart() {
        if (window.siteCartHelpers && typeof window.siteCartHelpers.getCart === 'function') {
            try { return window.siteCartHelpers.getCart(); } catch (e) { console.error(e); }
        }
        try { return JSON.parse(localStorage.getItem('staticCart_yenmay') || '[]'); } catch (e) { return []; }
    }

    function saveCart(cart) {
        if (window.siteCartHelpers && typeof window.siteCartHelpers.saveCart === 'function') {
            try { return window.siteCartHelpers.saveCart(cart); } catch (e) { console.error(e); }
        }
        try { localStorage.setItem('staticCart_yenmay', JSON.stringify(cart)); if (window.siteCartHelpers && typeof window.siteCartHelpers.updateCartCount === 'function') window.siteCartHelpers.updateCartCount(); else updateCartCountFallback(); } catch (e) { console.error(e); }
    }

    function formatCurrency(amount) {
        if (window.siteCartHelpers && typeof window.siteCartHelpers.formatCurrency === 'function') {
            try { return window.siteCartHelpers.formatCurrency(amount); } catch (e) { /* ignore */ }
        }
        try { return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(Number(amount) || 0); } catch (e) { return (Number(amount) || 0) + ' ₫'; }
    }

    function updateCartCountFallback() {
        try {
            const cart = getCart();
            const total = cart.reduce((s, it) => s + (parseInt(it.quantity, 10) || 0), 0);
            document.querySelectorAll('.cart-count-icon').forEach(el => {
                el.textContent = total;
                el.style.display = total > 0 ? 'inline-block' : 'none';
            });
        } catch (e) { /* ignore */ }
    }

    // Main render logic
    function renderCartPage() {
        const cartItemsBody = document.getElementById('cart-items-body');
        const cartSubtotalAmountEl = document.getElementById('cart-subtotal-amount');
        const emptyCartMessageEl = document.getElementById('empty-cart-message');
        const cartTable = document.querySelector('.cart-table');
        const cartSummaryDiv = document.querySelector('.cart-summary');

        if (!cartItemsBody) {
            console.warn('cart-page: #cart-items-body không tìm thấy');
            return;
        }

        const cart = getCart() || [];
        cartItemsBody.innerHTML = '';

        if (!cart || cart.length === 0) {
            if (emptyCartMessageEl) emptyCartMessageEl.style.display = 'block';
            if (cartTable) cartTable.style.display = 'none';
            if (cartSummaryDiv) cartSummaryDiv.style.display = 'none';
            if (cartSubtotalAmountEl) cartSubtotalAmountEl.textContent = formatCurrency(0);
            updateCartCountFallback();
            return;
        }

        if (emptyCartMessageEl) emptyCartMessageEl.style.display = 'none';
        if (cartTable) cartTable.style.display = 'table';
        if (cartSummaryDiv) cartSummaryDiv.style.display = 'flex';

        let subtotal = 0;

        cart.forEach(item => {
            const qty = parseInt(item.quantity, 10) || 0;
            const price = Number(item.price) || 0;
            const total = qty * price;
            subtotal += total;

            const tr = document.createElement('tr');
            tr.className = 'cart-item';
            tr.dataset.id = item.id;

            tr.innerHTML = `
        <td class="product-thumbnail" data-label="Ảnh"><img src="${item.img || 'https://placehold.co/80x80/eee/ccc?text=No+Image'}" alt="${escapeHtml(item.name || '')}" style="max-width:80px;"></td>
        <td class="product-name" data-label="Sản phẩm"><a href="product_detail.html?id=${encodeURIComponent(item.id)}">${escapeHtml(item.name || 'N/A')}</a></td>
        <td class="product-price text-end" data-label="Giá">${formatCurrency(price)}</td>
        <td class="product-quantity text-center" data-label="Số lượng">
          <div class="quantity-controls input-group input-group-sm justify-content-center">
            <button class="btn btn-outline-secondary quantity-button decrease-qty" type="button" data-id="${item.id}" aria-label="Giảm số lượng">-</button>
            <input type="number" class="quantity-input form-control text-center px-1" value="${qty}" min="1" data-id="${item.id}" readonly style="max-width: 55px; flex: 0 0 auto;">
            <button class="btn btn-outline-secondary quantity-button increase-qty" type="button" data-id="${item.id}" aria-label="Tăng số lượng">+</button>
          </div>
        </td>
        <td class="product-subtotal text-end" data-label="Tạm tính">${formatCurrency(total)}</td>
        <td class="product-remove text-center" data-label="Xóa">
          <button class="remove-item-button btn btn-sm btn-outline-danger" data-id="${item.id}" aria-label="Xóa sản phẩm ${escapeHtml(item.name || '')}"><i class="fas fa-trash-alt"></i></button>
        </td>
      `;

            cartItemsBody.appendChild(tr);
        });

        if (cartSubtotalAmountEl) cartSubtotalAmountEl.textContent = formatCurrency(subtotal);
        if (window.siteCartHelpers && typeof window.siteCartHelpers.updateCartCount === 'function') {
            try { window.siteCartHelpers.updateCartCount(); } catch (e) { updateCartCountFallback(); }
        } else {
            updateCartCountFallback();
        }
    }

    // Event delegation: xử lý tăng/giảm/xóa
    function attachCartEvents() {
        const cartItemsBody = document.getElementById('cart-items-body');
        if (!cartItemsBody) return;

        cartItemsBody.addEventListener('click', function (e) {
            const target = e.target;
            const btn = target.closest('button');
            if (!btn) return;

            const id = btn.dataset.id;
            if (!id) return;

            let cart = getCart();
            const idx = cart.findIndex(i => String(i.id) === String(id));
            if (idx === -1) return;

            let changed = false;

            if (btn.classList.contains('increase-qty')) {
                cart[idx].quantity = (parseInt(cart[idx].quantity, 10) || 0) + 1;
                changed = true;
            } else if (btn.classList.contains('decrease-qty')) {
                if ((parseInt(cart[idx].quantity, 10) || 0) > 1) {
                    cart[idx].quantity = (parseInt(cart[idx].quantity, 10) || 0) - 1;
                    changed = true;
                } else {
                    if (confirm(`Bạn có chắc muốn xóa "${cart[idx].name}" khỏi giỏ hàng?`)) {
                        cart.splice(idx, 1);
                        changed = true;
                    }
                }
            } else if (btn.classList.contains('remove-item-button')) {
                if (confirm(`Bạn có chắc muốn xóa "${cart[idx].name}" khỏi giỏ hàng?`)) {
                    cart.splice(idx, 1);
                    changed = true;
                }
            }

            if (changed) {
                saveCart(cart);
                renderCartPage();
            }
        });
    }

    // Escape HTML để tránh XSS khi hiển thị tên sản phẩm từ localStorage
    function escapeHtml(str) {
        return String(str || '').replace(/[&<>"'`=\/]/g, function (s) {
            return ({
                '&': '&amp;',
                '<': '&lt;',
                '>': '&gt;',
                '"': '&quot;',
                "'": '&#39;',
                '/': '&#x2F;',
                '`': '&#x60;',
                '=': '&#x3D;'
            })[s];
        });
    }

    // Khởi tạo khi DOM sẵn sàng
    document.addEventListener('DOMContentLoaded', function () {
        renderCartPage();
        attachCartEvents();
    });

    // Nếu file được load sau DOM
    if (document.readyState === 'complete' || document.readyState === 'interactive') {
        setTimeout(function () {
            renderCartPage();
            attachCartEvents();
        }, 0);
    }

})();
