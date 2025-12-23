// wwwroot/js/index-page.js
// Bắt sự kiện click cho mọi nút .add-to-cart-button trên trang
// Yêu cầu: site-cart-helpers.js đã được load trước file này và cung cấp window.siteCartHelpers.addProductToCart(...)

(function () {
    'use strict';

    function safeAddToCart(payload) {
        if (window.siteCartHelpers && typeof window.siteCartHelpers.addProductToCart === 'function') {
            return window.siteCartHelpers.addProductToCart(payload);
        }
        // Fallback nhẹ nếu helpers không tồn tại (không khuyến nghị lâu dài)
        try {
            const key = 'staticCart_yenmay';
            const raw = localStorage.getItem(key);
            const cart = raw ? JSON.parse(raw) : [];
            const idx = cart.findIndex(i => String(i.id) === String(payload.id));
            if (idx > -1) cart[idx].quantity = (parseInt(cart[idx].quantity, 10) || 0) + payload.quantity;
            else cart.push({ id: String(payload.id), name: payload.name, price: Number(payload.price) || 0, img: payload.img || '/images/no-image.png', quantity: payload.quantity });
            localStorage.setItem(key, JSON.stringify(cart));
            return true;
        } catch (e) {
            console.error('Fallback addToCart error', e);
            return false;
        }
    }

    // Event delegation: xử lý click cho mọi nút .add-to-cart-button
    document.addEventListener('click', function (e) {
        const btn = e.target.closest('.add-to-cart-button');
        if (!btn) return;

        // Tìm phần tử chứa dữ liệu sản phẩm (product-card hoặc product-detail-info)
        const card = btn.closest('.product-card') || btn.closest('.product-detail-info');
        if (!card) {
            console.error('index-page: Không tìm thấy product-card chứa nút add-to-cart');
            return;
        }

        // Lấy dữ liệu từ data-attributes (đảm bảo view đã render data-id, data-price, data-name, data-img)
        const id = card.dataset.id;
        const name = card.dataset.name || (card.querySelector('.product-title a')?.textContent || '').trim();
        const priceRaw = card.dataset.price || card.getAttribute('data-price') || '';
        const price = parseFloat(priceRaw.toString().replace(/[^0-9.-]+/g, '')) || 0;
        const img = card.dataset.img || (card.querySelector('img')?.src) || '/images/no-image.png';

        if (!id) {
            console.error('index-page: product-card thiếu data-id');
            return;
        }

        const added = safeAddToCart({ id, name, price, img, quantity: 1 });

        if (added) {
            // UI feedback: đổi nội dung nút tạm thời
            const originalHTML = btn.innerHTML;
            btn.disabled = true;
            btn.innerHTML = '<i class="fas fa-check me-1"></i> Đã thêm';
            setTimeout(() => {
                btn.disabled = false;
                btn.innerHTML = originalHTML;
            }, 1400);

            // Nếu helpers có updateCartCount, gọi để cập nhật icon
            if (window.siteCartHelpers && typeof window.siteCartHelpers.updateCartCount === 'function') {
                try { window.siteCartHelpers.updateCartCount(); } catch (e) { /* ignore */ }
            } else {
                // fallback: cập nhật các .cart-count-icon thủ công
                try {
                    const raw = localStorage.getItem('staticCart_yenmay');
                    const cart = raw ? JSON.parse(raw) : [];
                    const total = cart.reduce((s, it) => s + (parseInt(it.quantity, 10) || 0), 0);
                    document.querySelectorAll('.cart-count-icon').forEach(el => {
                        el.textContent = total;
                        el.style.display = total > 0 ? 'inline-block' : 'none';
                    });
                } catch (e) { /* ignore */ }
            }
        } else {
            alert('Không thể thêm sản phẩm vào giỏ. Vui lòng thử lại.');
        }
    });

    // Khi trang load, nếu helpers có updateCartCount thì gọi để hiển thị số lượng hiện tại
    document.addEventListener('DOMContentLoaded', function () {
        if (window.siteCartHelpers && typeof window.siteCartHelpers.updateCartCount === 'function') {
            try { window.siteCartHelpers.updateCartCount(); } catch (e) { /* ignore */ }
        }
    });

})();
