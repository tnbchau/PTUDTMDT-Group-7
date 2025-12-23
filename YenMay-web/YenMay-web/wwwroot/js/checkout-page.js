// wwwroot/js/checkout-page.js
(function () {
    'use strict';

    const CART_KEY = 'staticCart_yenmay';

    /* ---------- Helpers ---------- */
    function formatCurrency(amount) {
        try {
            return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(Number(amount) || 0);
        } catch (e) {
            return (Number(amount) || 0) + ' ₫';
        }
    }

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

    function getCartRaw() {
        try {
            return JSON.parse(localStorage.getItem(CART_KEY) || '[]');
        } catch (e) {
            console.error('checkout-page: parse cart error', e);
            return [];
        }
    }

    /* ---------- Render cart on checkout ---------- */
    function renderCheckoutItems() {
        const items = getCartRaw();
        const container = document.getElementById('checkout-items-list');
        const subtotalEl = document.getElementById('checkout-subtotal-amount');
        const shippingEl = document.getElementById('checkout-shipping-fee');
        const discountRow = document.getElementById('checkout-discount-row');
        const discountAmountEl = document.getElementById('checkout-discount-amount');
        const grandTotalEl = document.getElementById('checkout-grand-total');

        if (!container) return;

        container.innerHTML = '';
        if (!items || items.length === 0) {
            container.innerHTML = '<div class="text-center text-muted py-4">Giỏ hàng đang trống</div>';
            if (subtotalEl) subtotalEl.textContent = formatCurrency(0);
            if (shippingEl) shippingEl.textContent = formatCurrency(0);
            if (grandTotalEl) grandTotalEl.textContent = formatCurrency(0);
            if (discountRow) discountRow.style.display = 'none';
            return;
        }

        let subtotal = 0;
        items.forEach(it => {
            // Map nhiều dạng trường (tùy nguồn lưu)
            const qty = parseInt(it.quantity ?? it.Quantity ?? it.qty ?? 0, 10) || 0;
            const price = Number(it.price ?? it.Price ?? it.unitPrice ?? 0) || 0;
            const name = it.name ?? it.Name ?? it.title ?? 'Sản phẩm';
            const img = it.img ?? it.Img ?? it.image ?? '/images/no-image.png';
            const line = qty * price;
            subtotal += line;

            const row = document.createElement('div');
            row.className = 'd-flex align-items-center mb-3';
            row.innerHTML = `
        <img src="${escapeHtml(img)}" alt="${escapeHtml(name)}" style="width:72px;height:72px;object-fit:cover;border-radius:6px;" />
        <div class="ms-3 flex-grow-1">
          <div class="fw-semibold text-truncate" style="max-width:220px;">${escapeHtml(name)}</div>
          <div class="small text-muted">x ${qty}</div>
        </div>
        <div class="text-end">${formatCurrency(line)}</div>
      `;
            container.appendChild(row);
        });

        // Business rules (mẫu) - chỉnh theo yêu cầu
        let shipping = 0;
        let discount = 0;
        if (subtotal >= 1000000) discount = Math.round(subtotal * 0.10);
        if (subtotal > 0 && subtotal < 500000) shipping = 30000;

        if (subtotalEl) subtotalEl.textContent = formatCurrency(subtotal);
        if (shippingEl) shippingEl.textContent = formatCurrency(shipping);
        if (discount > 0) {
            if (discountRow) discountRow.style.display = 'block';
            if (discountAmountEl) discountAmountEl.textContent = `- ${formatCurrency(discount)}`;
        } else {
            if (discountRow) discountRow.style.display = 'none';
        }
        if (grandTotalEl) grandTotalEl.textContent = formatCurrency(subtotal + shipping - discount);
    }

    /* ---------- Render invoice from server order object ---------- */
    function renderInvoiceFromOrder(order) {
        if (!order) return;

        // Map server keys to view ids
        document.getElementById('invoice-order-id').textContent = order.orderId || order.OrderId || '';
        document.getElementById('invoice-order-date').textContent = order.orderDate ? new Date(order.orderDate).toLocaleString() : (order.orderDateString || '');
        document.getElementById('invoice-customer-name').textContent = order.customerName || order.CustomerName || '';
        document.getElementById('invoice-customer-phone').textContent = order.customerPhone || order.CustomerPhone || '';
        document.getElementById('invoice-customer-email').textContent = order.customerEmail || order.CustomerEmail || '-';
        document.getElementById('invoice-customer-address').textContent = order.customerAddress || order.CustomerAddress || '';
        document.getElementById('invoice-order-notes').textContent = order.notes || order.Notes || '[Không có ghi chú]';
        document.getElementById('invoice-payment-method').textContent = order.paymentMethod || order.PaymentMethod || '';

        const tbody = document.getElementById('invoice-items-body');
        tbody.innerHTML = '';
        const items = order.items || order.Items || [];
        items.forEach((it, idx) => {
            const name = it.name || it.Name || '';
            const qty = it.quantity || it.Quantity || 0;
            const price = it.price || it.Price || 0;
            const lineTotal = it.lineTotal || it.LineTotal || (qty * price);
            const tr = document.createElement('tr');
            tr.innerHTML = `
        <td class="text-center">${idx + 1}</td>
        <td>${escapeHtml(name)}</td>
        <td class="text-center">${qty}</td>
        <td class="text-end">${formatCurrency(price)}</td>
        <td class="text-end">${formatCurrency(lineTotal)}</td>
      `;
            tbody.appendChild(tr);
        });

        document.getElementById('invoice-subtotal').textContent = formatCurrency(order.subtotal || order.Subtotal || 0);
        document.getElementById('invoice-shipping-fee').textContent = formatCurrency(order.shippingFee || order.ShippingFee || 0);
        const discountVal = order.discount || order.Discount || 0;
        if (discountVal > 0) {
            document.getElementById('invoice-discount-row').style.display = 'block';
            document.getElementById('invoice-discount-amount').textContent = `- ${formatCurrency(discountVal)}`;
        } else {
            document.getElementById('invoice-discount-row').style.display = 'none';
        }
        document.getElementById('invoice-grand-total-value').textContent = formatCurrency(order.grandTotal || order.GrandTotal || 0);

        // Show invoice, hide checkout form
        const formArea = document.getElementById('checkout-form-area');
        const invoiceArea = document.getElementById('order-invoice-section');
        if (formArea) formArea.style.display = 'none';
        if (invoiceArea) invoiceArea.style.display = 'block';
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }

    /* ---------- Submit order to server ---------- */
    async function submitOrder() {
        const btn = document.getElementById('checkout-submit-button');
        if (btn) {
            btn.disabled = true;
            btn.dataset.orig = btn.innerHTML;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Đang xử lý...';
        }

        try {
            const cart = getCartRaw();
            if (!cart || cart.length === 0) {
                alert('Giỏ hàng trống.');
                return;
            }

            const payload = {
                FullName: document.getElementById('billing_name')?.value?.trim() || '',
                Phone: document.getElementById('billing_phone')?.value?.trim() || '',
                Email: document.getElementById('billing_email')?.value?.trim() || '',
                Address: document.getElementById('billing_address')?.value?.trim() || '',
                Notes: document.getElementById('order_notes')?.value?.trim() || '',
                PaymentMethod: document.querySelector('input[name="payment_method"]:checked')?.value || 'cod',
                Cart: cart.map(it => ({
                    Id: it.id ?? it.Id ?? it.productId ?? '',
                    Name: it.name ?? it.Name ?? it.title ?? '',
                    Price: Number(it.price ?? it.Price ?? it.unitPrice ?? 0) || 0,
                    Quantity: parseInt(it.quantity ?? it.Quantity ?? it.qty ?? 0, 10) || 1,
                    Img: it.img ?? it.Img ?? it.image ?? ''
                }))
            };

            // Basic client validation
            if (!payload.FullName) { alert('Vui lòng nhập họ và tên'); return; }
            if (!payload.Phone) { alert('Vui lòng nhập số điện thoại'); return; }
            if (!payload.Address) { alert('Vui lòng nhập địa chỉ nhận hàng'); return; }

            // Anti-forgery token (nếu view có @Html.AntiForgeryToken())
            const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
            const headers = { 'Content-Type': 'application/json' };
            if (tokenInput) headers['RequestVerificationToken'] = tokenInput.value;

            const res = await fetch('/Checkout/PlaceOrder', {
                method: 'POST',
                headers,
                body: JSON.stringify(payload)
            });

            if (!res.ok) {
                const txt = await res.text();
                console.error('checkout-page: PlaceOrder failed', res.status, txt);
                alert('Có lỗi khi gửi đơn hàng. Vui lòng thử lại.');
                return;
            }

            const data = await res.json();
            if (data && data.success && data.order) {
                // Clear cart
                localStorage.removeItem(CART_KEY);
                // Render invoice
                renderInvoiceFromOrder(data.order);
            } else {
                console.error('checkout-page: PlaceOrder response error', data);
                alert(data && data.message ? data.message : 'Đặt hàng thất bại. Vui lòng thử lại.');
            }
        } catch (e) {
            console.error('checkout-page: submitOrder error', e);
            alert('Có lỗi khi gửi đơn hàng. Vui lòng thử lại.');
        } finally {
            if (btn) {
                btn.disabled = false;
                btn.innerHTML = btn.dataset.orig || 'XÁC NHẬN ĐẶT HÀNG';
            }
        }
    }

    /* ---------- Payment method toggles & print ---------- */
    function initPaymentMethodToggles() {
        const bankInfo = document.getElementById('bank-transfer-info');
        const ewalletInfo = document.getElementById('ewallet-info');
        document.querySelectorAll('input[name="payment_method"]').forEach(r => {
            r.addEventListener('change', function () {
                if (this.value === 'bank_transfer') {
                    if (bankInfo) bankInfo.style.display = 'block';
                    if (ewalletInfo) ewalletInfo.style.display = 'none';
                } else if (this.value === 'e_wallet') {
                    if (bankInfo) bankInfo.style.display = 'none';
                    if (ewalletInfo) ewalletInfo.style.display = 'block';
                } else {
                    if (bankInfo) bankInfo.style.display = 'none';
                    if (ewalletInfo) ewalletInfo.style.display = 'none';
                }
            });
        });
    }

    function initPrintInvoice() {
        document.querySelectorAll('.btn-print-invoice').forEach(btn => {
            btn.addEventListener('click', function () {
                window.print();
            });
        });
    }

    /* ---------- Init ---------- */
    document.addEventListener('DOMContentLoaded', function () {
        try {
            renderCheckoutItems();
            initPaymentMethodToggles();
            initPrintInvoice();

            // Hook submit button
            const submitBtn = document.getElementById('checkout-submit-button');
            if (submitBtn) submitBtn.addEventListener('click', submitOrder);

            // Update when cart changes in other tabs
            window.addEventListener('storage', function (e) {
                if (e.key === CART_KEY) renderCheckoutItems();
            });
        } catch (e) {
            console.error('checkout-page init error', e);
        }
    });

    // Expose for debugging
    window.__checkout_render = renderCheckoutItems;
    window.__checkout_submit = submitOrder;
})();
