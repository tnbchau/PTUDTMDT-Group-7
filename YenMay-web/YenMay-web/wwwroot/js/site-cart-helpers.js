// wwwroot/js/site-cart-helpers.js
const LOCAL_STORAGE_CART_KEY = 'staticCart_yenmay';

function getCart() {
    try {
        const raw = localStorage.getItem(LOCAL_STORAGE_CART_KEY);
        return raw ? JSON.parse(raw) : [];
    } catch (e) {
        console.error('getCart parse error', e);
        return [];
    }
}

function saveCart(cart) {
    try {
        if (!Array.isArray(cart)) return;
        localStorage.setItem(LOCAL_STORAGE_CART_KEY, JSON.stringify(cart));
        updateCartCount();
    } catch (e) {
        console.error('saveCart error', e);
    }
}

function updateCartCount() {
    try {
        const cart = getCart();
        const total = cart.reduce((s, it) => s + (parseInt(it.quantity, 10) || 0), 0);
        document.querySelectorAll('.cart-count-icon').forEach(el => {
            el.textContent = total;
            el.style.display = total > 0 ? 'inline-block' : 'none';
        });
    } catch (e) {
        console.error('updateCartCount error', e);
    }
}

function formatCurrency(amount) {
    if (typeof amount !== 'number') amount = Number(amount) || 0;
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
}

function addProductToCart({ id, name, price, img, quantity = 1 }) {
    if (!id) return false;
    try {
        const cart = getCart();
        const idx = cart.findIndex(item => String(item.id) === String(id));
        if (idx > -1) {
            cart[idx].quantity = (parseInt(cart[idx].quantity, 10) || 0) + quantity;
        } else {
            cart.push({
                id: String(id),
                name: name || 'Sản phẩm',
                price: Number(price) || 0,
                img: img || '/images/no-image.png',
                quantity: quantity
            });
        }
        saveCart(cart);
        return true;
    } catch (e) {
        console.error('addProductToCart error', e);
        return false;
    }
}

// Export to window for pages that expect global functions
window.siteCartHelpers = {
    getCart,
    saveCart,
    updateCartCount,
    formatCurrency,
    addProductToCart
};
