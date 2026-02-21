// ============================
// State
// ============================
let products = [];
let cart = JSON.parse(localStorage.getItem('toto_cart') || '[]');
let allOrders = [];
let adminOrders = [];
let adminLoggedIn = false;
let callLog = [];

const API = '';

// GTM dataLayer
window.dataLayer = window.dataLayer || [];

// ============================
// Init
// ============================
document.addEventListener('DOMContentLoaded', () => {
    loadProducts();
    updateCartCount();
});

// ============================
// Navigation
// ============================
function showSection(section) {
    // Hide all
    document.querySelectorAll('.content-section').forEach(s => s.classList.add('d-none'));
    // Update nav active
    document.querySelectorAll('#navbarNav .nav-link').forEach(l => l.classList.remove('active'));

    if (section === 'products') {
        document.getElementById('products-section').classList.remove('d-none');
        document.getElementById('hero').classList.remove('d-none');
        document.querySelector('[onclick="showSection(\'products\')"]')?.classList.add('active');
    } else {
        document.getElementById('hero').classList.add('d-none');
        if (section === 'cart') {
            document.getElementById('cart-section').classList.remove('d-none');
            renderCart();
        } else if (section === 'orders') {
            document.getElementById('orders-section').classList.remove('d-none');
            document.querySelector('[onclick="showSection(\'orders\')"]')?.classList.add('active');
            loadOrders();
        } else if (section === 'admin') {
            document.getElementById('admin-section').classList.remove('d-none');
            document.querySelector('[onclick="showSection(\'admin\')"]')?.classList.add('active');
            if (adminLoggedIn) loadAdminDashboard();
        }
    }

    // Close mobile nav
    const navCollapse = document.getElementById('navbarNav');
    if (navCollapse.classList.contains('show')) {
        new bootstrap.Collapse(navCollapse).hide();
    }
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

// ============================
// Products
// ============================
async function loadProducts() {
    try {
        const res = await fetch(`${API}/api/products`);
        products = await res.json();
        renderProducts(products);
        renderCategoryFilters();
    } catch (e) {
        console.error('Failed to load products:', e);
        document.getElementById('products-grid').innerHTML = `
            <div class="col-12 text-center py-5">
                <i class="bi bi-exclamation-triangle fs-1 text-warning d-block mb-3"></i>
                <h5>পণ্য লোড করতে ব্যর্থ</h5>
                <button class="btn btn-primary mt-2" onclick="loadProducts()">আবার চেষ্টা করুন</button>
            </div>`;
    }
}

function renderProducts(list) {
    const grid = document.getElementById('products-grid');
    if (!list.length) {
        grid.innerHTML = `<div class="col-12 empty-state"><i class="bi bi-search d-block mb-3"></i><h5>কোনো পণ্য পাওয়া যায়নি</h5></div>`;
        return;
    }
    grid.innerHTML = list.map(p => `
        <div class="col-6 col-md-4 col-lg-3 fade-in">
            <div class="card product-card h-100">
                <div class="img-wrapper">
                    <span class="badge bg-primary category-badge">${p.category}</span>
                    <img src="${p.imageUrl}" class="card-img-top" alt="${p.name}" loading="lazy"
                         onerror="this.src='https://placehold.co/400x300/e2e8f0/64748b?text=${encodeURIComponent(p.name)}'">
                </div>
                <div class="card-body d-flex flex-column p-3">
                    <h6 class="fw-bold mb-1" title="${p.name}">${p.name}</h6>
                    <p class="text-muted small mb-2 flex-grow-1" style="display:-webkit-box;-webkit-line-clamp:2;-webkit-box-orient:vertical;overflow:hidden;">
                        ${p.description}
                    </p>
                    <div class="d-flex justify-content-between align-items-center mb-2">
                        <span class="price">৳${formatPrice(p.price)}</span>
                        <span class="badge ${p.stock > 0 ? 'bg-success-subtle text-success' : 'bg-danger-subtle text-danger'} stock-badge">
                            ${p.stock > 0 ? `স্টক: ${p.stock}` : 'স্টক আউট'}
                        </span>
                    </div>
                    <button class="btn ${p.stock > 0 ? 'btn-primary' : 'btn-secondary'} btn-sm btn-add-cart w-100" 
                            onclick="addToCart(${p.id})" ${p.stock <= 0 ? 'disabled' : ''}>
                        <i class="bi bi-cart-plus me-1"></i>${p.stock > 0 ? 'কার্টে যোগ করুন' : 'স্টক নেই'}
                    </button>
                </div>
            </div>
        </div>
    `).join('');
}

function renderCategoryFilters() {
    const cats = [...new Set(products.map(p => p.category))];
    const container = document.getElementById('category-filters');
    container.innerHTML = `<button class="btn btn-filter active" onclick="filterProducts('all', this)"><i class="bi bi-grid-3x3-gap"></i> সব</button>`;
    cats.forEach(c => {
        container.innerHTML += `<button class="btn btn-filter" onclick="filterProducts('${c}', this)">${c}</button>`;
    });
}

function filterProducts(category, btn) {
    if (btn) {
        document.querySelectorAll('#category-filters .btn-filter').forEach(b => b.classList.remove('active'));
        btn.classList.add('active');
    }
    const searchVal = document.getElementById('search-input')?.value?.toLowerCase() || '';
    let filtered = category === 'all' ? [...products] : products.filter(p => p.category === category);
    if (searchVal) filtered = filtered.filter(p => p.name.toLowerCase().includes(searchVal) || p.description.toLowerCase().includes(searchVal));
    renderProducts(filtered);
}

function searchProducts(val) {
    const activeFilter = document.querySelector('#category-filters .btn-filter.active');
    const cat = activeFilter?.textContent.trim();
    let filtered = products;
    if (cat && cat !== 'সব') filtered = products.filter(p => p.category === cat);
    if (val) filtered = filtered.filter(p => p.name.toLowerCase().includes(val.toLowerCase()) || p.description.toLowerCase().includes(val.toLowerCase()));
    renderProducts(filtered);
}

// ============================
// Cart
// ============================
function addToCart(productId) {
    const product = products.find(p => p.id === productId);
    if (!product || product.stock <= 0) return;

    const existing = cart.find(c => c.productId === productId);
    if (existing) {
        if (existing.quantity >= product.stock) {
            showToast('স্টক সীমা অতিক্রম!', 'warning');
            return;
        }
        existing.quantity++;
    } else {
        cart.push({
            productId: product.id,
            productName: product.name,
            price: product.price,
            quantity: 1,
            imageUrl: product.imageUrl
        });
    }
    saveCart();
    updateCartCount();
    showToast(`${product.name} কার্টে যোগ হয়েছে!`, 'success');

    // GTM: Add to Cart event
    dataLayer.push({
        event: 'add_to_cart',
        ecommerce: {
            currency: 'BDT',
            value: product.price,
            items: [{ item_id: product.id, item_name: product.name, price: product.price, quantity: 1, item_category: product.category }]
        }
    });
}

function removeFromCart(productId) {
    cart = cart.filter(c => c.productId !== productId);
    saveCart();
    updateCartCount();
    renderCart();
}

function updateQuantity(productId, delta) {
    const item = cart.find(c => c.productId === productId);
    if (!item) return;
    const product = products.find(p => p.id === productId);
    item.quantity += delta;
    if (item.quantity <= 0) { removeFromCart(productId); return; }
    if (product && item.quantity > product.stock) { item.quantity = product.stock; showToast('স্টক সীমা!', 'warning'); }
    saveCart();
    renderCart();
}

function renderCart() {
    const container = document.getElementById('cart-items');
    if (!cart.length) {
        container.innerHTML = `
            <div class="empty-state py-5">
                <i class="bi bi-cart-x d-block mb-3"></i>
                <h5>কার্ট খালি</h5>
                <p class="text-muted">শপিং শুরু করতে পণ্য যোগ করুন</p>
                <button class="btn btn-primary" onclick="showSection('products')"><i class="bi bi-shop me-1"></i>শপিং করুন</button>
            </div>`;
        updateCartSummary();
        return;
    }
    container.innerHTML = cart.map(item => `
        <div class="cart-item">
            <img src="${item.imageUrl}" alt="${item.productName}"
                 onerror="this.src='https://placehold.co/70x70/e2e8f0/64748b?text=P'">
            <div class="flex-grow-1">
                <h6 class="mb-1 fw-bold">${item.productName}</h6>
                <span class="text-primary fw-semibold">৳${formatPrice(item.price)}</span>
            </div>
            <div class="d-flex align-items-center gap-3">
                <div class="quantity-controls">
                    <button onclick="updateQuantity(${item.productId}, -1)"><i class="bi bi-dash"></i></button>
                    <span class="fw-bold px-1">${item.quantity}</span>
                    <button onclick="updateQuantity(${item.productId}, 1)"><i class="bi bi-plus"></i></button>
                </div>
                <span class="fw-bold text-nowrap">৳${formatPrice(item.price * item.quantity)}</span>
                <button class="btn btn-sm btn-outline-danger border-0" onclick="removeFromCart(${item.productId})"><i class="bi bi-trash"></i></button>
            </div>
        </div>`).join('');
    updateCartSummary();
}

function updateCartSummary() {
    const subtotal = cart.reduce((s, i) => s + i.price * i.quantity, 0);
    const delivery = cart.length > 0 ? 50 : 0;
    document.getElementById('cart-subtotal').textContent = `৳${formatPrice(subtotal)}`;
    document.getElementById('cart-total').textContent = `৳${formatPrice(subtotal + delivery)}`;
    document.getElementById('checkout-btn').disabled = cart.length === 0;
}

function updateCartCount() {
    const count = cart.reduce((s, i) => s + i.quantity, 0);
    document.querySelectorAll('.cart-count').forEach(el => {
        el.textContent = count;
        el.style.display = count > 0 ? '' : 'none';
    });
}

function saveCart() {
    localStorage.setItem('toto_cart', JSON.stringify(cart));
    updateCartCount();
}

// ============================
// Checkout
// ============================
function showCheckout() {
    if (!cart.length) return;
    new bootstrap.Modal(document.getElementById('checkoutModal')).show();

    // GTM: Begin Checkout event
    dataLayer.push({
        event: 'begin_checkout',
        ecommerce: {
            currency: 'BDT',
            value: cart.reduce((s, i) => s + i.price * i.quantity, 0) + 50,
            items: cart.map(i => ({ item_id: i.productId, item_name: i.productName, price: i.price, quantity: i.quantity }))
        }
    });
}

async function placeOrder() {
    const name = document.getElementById('customer-name').value.trim();
    const email = document.getElementById('customer-email').value.trim();
    const phone = document.getElementById('customer-phone').value.trim();
    const city = document.getElementById('customer-city').value;
    const address = document.getElementById('customer-address').value.trim();

    if (!name || !email || !phone || !city || !address) {
        showToast('সব ফিল্ড পূরণ করুন!', 'warning');
        return;
    }
    if (!/^1[0-9]{9}$/.test(phone)) {
        showToast('সঠিক ফোন নম্বর দিন (1XXXXXXXXX)', 'warning');
        return;
    }

    const btn = document.getElementById('place-order-btn');
    btn.disabled = true;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>অর্ডার হচ্ছে...';

    const subtotal = cart.reduce((s, i) => s + i.price * i.quantity, 0);
    const orderData = {
        customerName: name,
        customerEmail: email,
        customerPhone: `+880${phone}`,
        shippingAddress: `${address}, ${city}`,
        totalAmount: subtotal + 50,
        items: cart.map(c => ({
            productId: c.productId,
            productName: c.productName,
            price: c.price,
            quantity: c.quantity,
            imageUrl: c.imageUrl
        }))
    };

    try {
        const res = await fetch(`${API}/api/orders`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(orderData)
        });

        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        const order = await res.json();

        // Clear cart
        cart = [];
        saveCart();
        document.getElementById('checkout-form').reset();

        // Close checkout, show success
        bootstrap.Modal.getInstance(document.getElementById('checkoutModal'))?.hide();
        document.getElementById('success-order-id').textContent = order.id;
        new bootstrap.Modal(document.getElementById('orderSuccessModal')).show();

        // GTM: Purchase event
        dataLayer.push({
            event: 'purchase',
            ecommerce: {
                transaction_id: order.id,
                currency: 'BDT',
                value: orderData.totalAmount,
                shipping: 50,
                items: orderData.items.map(i => ({ item_id: i.productId, item_name: i.productName, price: i.price, quantity: i.quantity }))
            }
        });

    } catch (e) {
        console.error('Order failed:', e);
        showToast('অর্ডার ব্যর্থ হয়েছে! আবার চেষ্টা করুন।', 'danger');
    } finally {
        btn.disabled = false;
        btn.innerHTML = '<i class="bi bi-bag-check me-2"></i>অর্ডার করুন';
    }
}

// ============================
// Orders
// ============================
async function loadOrders() {
    const container = document.getElementById('orders-list');
    container.innerHTML = `<div class="text-center py-5"><div class="spinner-border text-primary"></div><p class="mt-3 text-muted">অর্ডার লোড হচ্ছে...</p></div>`;

    try {
        const res = await fetch(`${API}/api/orders`);
        allOrders = await res.json();
        renderOrders(allOrders);
    } catch (e) {
        console.error('Failed to load orders:', e);
        container.innerHTML = `<div class="text-center py-5"><i class="bi bi-exclamation-triangle fs-1 text-warning d-block mb-3"></i><h5>অর্ডার লোড ব্যর্থ</h5></div>`;
    }
}

function renderOrders(orders) {
    const container = document.getElementById('orders-list');
    if (!orders.length) {
        container.innerHTML = `
            <div class="empty-state">
                <i class="bi bi-receipt d-block mb-3"></i>
                <h5>কোনো অর্ডার নেই</h5>
                <p class="text-muted">শপিং করুন এবং প্রথম অর্ডার দিন!</p>
                <button class="btn btn-primary" onclick="showSection('products')"><i class="bi bi-shop me-1"></i>শপিং করুন</button>
            </div>`;
        return;
    }
    container.innerHTML = orders.map(o => {
        const statusClass = o.status === 'Confirmed' ? 'confirmed' : o.status === 'Cancelled' ? 'cancelled' : 'pending';
        const statusBadge = o.status === 'Confirmed' ? 'bg-success' : o.status === 'Cancelled' ? 'bg-danger' : 'bg-warning text-dark';
        const date = new Date(o.createdAt).toLocaleString('bn-BD');
        return `
            <div class="card order-card status-${statusClass} mb-3">
                <div class="card-body p-3 p-md-4">
                    <div class="d-flex flex-column flex-sm-row justify-content-between align-items-start gap-2 mb-3">
                        <div>
                            <h6 class="fw-bold mb-1">অর্ডার #${o.id.substring(0, 8)}</h6>
                            <small class="text-muted"><i class="bi bi-calendar me-1"></i>${date}</small>
                        </div>
                        <div class="text-sm-end">
                            <span class="badge ${statusBadge} mb-1">${o.status}</span>
                            ${o.callStatus ? `<br><small class="text-muted"><i class="bi bi-telephone me-1"></i>${o.callStatus}</small>` : ''}
                        </div>
                    </div>
                    <div class="row g-2 mb-3">
                        ${(o.items || []).map(item => `
                            <div class="col-auto">
                                <div class="d-flex align-items-center gap-2 bg-light rounded p-2">
                                    <img src="${item.imageUrl}" width="40" height="40" class="rounded" style="object-fit:cover"
                                         onerror="this.src='https://placehold.co/40x40/e2e8f0/64748b?text=P'">
                                    <div>
                                        <small class="fw-semibold d-block">${item.productName}</small>
                                        <small class="text-muted">x${item.quantity} — ৳${formatPrice(item.price * item.quantity)}</small>
                                    </div>
                                </div>
                            </div>`).join('')}
                    </div>
                    <div class="d-flex justify-content-between align-items-center">
                        <span class="text-muted small"><i class="bi bi-geo-alt me-1"></i>${o.shippingAddress || ''}</span>
                        <strong class="text-primary fs-5">৳${formatPrice(o.totalAmount)}</strong>
                    </div>
                </div>
            </div>`;
    }).join('');
}

// ============================
// Admin
// ============================
function adminLogin() {
    const pwd = document.getElementById('admin-password').value;
    if (pwd === 'admin123') {
        adminLoggedIn = true;
        document.getElementById('admin-login-gate').classList.add('d-none');
        document.getElementById('admin-dashboard').classList.remove('d-none');
        loadAdminDashboard();
        showToast('অ্যাডমিন লগইন সফল!', 'success');
    } else {
        showToast('ভুল পাসওয়ার্ড!', 'danger');
        document.getElementById('admin-password').classList.add('is-invalid');
        setTimeout(() => document.getElementById('admin-password').classList.remove('is-invalid'), 2000);
    }
}

function adminLogout() {
    adminLoggedIn = false;
    document.getElementById('admin-dashboard').classList.add('d-none');
    document.getElementById('admin-login-gate').classList.remove('d-none');
    document.getElementById('admin-password').value = '';
    showToast('লগআউট হয়েছে', 'info');
}

async function loadAdminDashboard() {
    try {
        const res = await fetch(`${API}/api/admin/dashboard`);
        const data = await res.json();

        document.getElementById('stat-total-orders').textContent = data.totalOrders;
        document.getElementById('stat-pending').textContent = data.pendingOrders;
        document.getElementById('stat-confirmed').textContent = data.confirmedOrders;
        document.getElementById('stat-revenue').textContent = `৳${formatPrice(data.totalRevenue)}`;

        adminOrders = data.recentOrders || [];
        renderAdminOrders(adminOrders);
    } catch (e) {
        console.error('Admin dashboard error:', e);
        showToast('ড্যাশবোর্ড লোড ব্যর্থ', 'danger');
    }
}

function renderAdminOrders(orders) {
    const tbody = document.getElementById('admin-orders-table');
    if (!orders.length) {
        tbody.innerHTML = `<tr><td colspan="6" class="text-center text-muted py-5"><i class="bi bi-inbox fs-1 d-block mb-2"></i>কোনো অর্ডার নেই</td></tr>`;
        return;
    }
    tbody.innerHTML = orders.map(o => {
        const statusBadge = o.status === 'Confirmed' ? 'bg-success' : o.status === 'Cancelled' ? 'bg-danger' : 'bg-warning text-dark';
        return `
            <tr>
                <td><small class="fw-semibold">#${o.id.substring(0, 8)}</small></td>
                <td class="d-none d-md-table-cell">${o.customerName}</td>
                <td><small>${o.customerPhone}</small></td>
                <td class="d-none d-sm-table-cell fw-bold">৳${formatPrice(o.totalAmount)}</td>
                <td><span class="badge ${statusBadge}">${o.status}</span></td>
                <td>
                    <div class="d-flex gap-1">
                        <button class="btn btn-sm btn-success" onclick="openAdminCallModal('${o.id}', '${escapeHtml(o.customerName)}', '${o.customerPhone}')" title="কল করুন">
                            <i class="bi bi-telephone"></i>
                        </button>
                        <div class="dropdown">
                            <button class="btn btn-sm btn-outline-secondary dropdown-toggle" data-bs-toggle="dropdown"><i class="bi bi-three-dots-vertical"></i></button>
                            <ul class="dropdown-menu dropdown-menu-end">
                                <li><a class="dropdown-item text-success" href="#" onclick="updateOrderStatus('${o.id}','Confirmed')"><i class="bi bi-check-circle me-2"></i>কনফার্ম</a></li>
                                <li><a class="dropdown-item text-danger" href="#" onclick="updateOrderStatus('${o.id}','Cancelled')"><i class="bi bi-x-circle me-2"></i>বাতিল</a></li>
                                <li><hr class="dropdown-divider"></li>
                                <li><a class="dropdown-item text-danger" href="#" onclick="deleteOrder('${o.id}')"><i class="bi bi-trash me-2"></i>মুছুন</a></li>
                            </ul>
                        </div>
                    </div>
                </td>
            </tr>`;
    }).join('');
}

function filterAdminOrders(status, btn) {
    if (btn) {
        btn.closest('.d-flex').querySelectorAll('.btn').forEach(b => b.classList.remove('active'));
        btn.classList.add('active');
    }
    if (status === 'all') {
        renderAdminOrders(adminOrders);
    } else {
        renderAdminOrders(adminOrders.filter(o => o.status === status));
    }
}

// Admin Call Modal
function openAdminCallModal(orderId, name, phone) {
    document.getElementById('modal-call-customer').value = name;
    document.getElementById('modal-call-phone').value = phone;
    document.getElementById('modal-call-message').value = '';
    document.getElementById('modal-call-orderid').value = orderId;
    new bootstrap.Modal(document.getElementById('adminCallModal')).show();
}

async function executeAdminCall() {
    const orderId = document.getElementById('modal-call-orderid').value;
    const message = document.getElementById('modal-call-message').value.trim();
    const phone = document.getElementById('modal-call-phone').value;

    const btn = event.target;
    btn.disabled = true;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>কল হচ্ছে...';

    try {
        const body = {};
        if (message) body.message = message;

        const res = await fetch(`${API}/api/admin/call-customer/${orderId}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body)
        });
        const data = await res.json();

        if (data.success) {
            showToast(`কল পাঠানো হয়েছে: ${phone}`, 'success');
            addCallLog(phone, 'সফল', 'অর্ডার কল');
        } else {
            showToast(`কল ব্যর্থ: ${data.message}`, 'danger');
            addCallLog(phone, 'ব্যর্থ', 'অর্ডার কল');
        }

        bootstrap.Modal.getInstance(document.getElementById('adminCallModal'))?.hide();
    } catch (e) {
        console.error('Admin call error:', e);
        showToast('কল পাঠাতে ব্যর্থ!', 'danger');
        addCallLog(phone, 'ত্রুটি', 'অর্ডার কল');
    } finally {
        btn.disabled = false;
        btn.innerHTML = '<i class="bi bi-telephone-outbound me-2"></i>কল পাঠান';
    }
}

async function adminDirectCall() {
    const phone = document.getElementById('admin-direct-phone').value.trim();
    const message = document.getElementById('admin-direct-message').value.trim();

    if (!phone || !/^1[0-9]{9}$/.test(phone)) {
        showToast('সঠিক ফোন নম্বর দিন (1XXXXXXXXX)', 'warning');
        return;
    }

    const btn = event.target;
    btn.disabled = true;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>কল হচ্ছে...';

    try {
        const body = { phone };
        if (message) body.message = message;

        const res = await fetch(`${API}/api/admin/call-direct`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body)
        });
        const data = await res.json();

        if (data.success) {
            showToast(`কল পাঠানো হয়েছে: +880${phone}`, 'success');
            addCallLog(`+880${phone}`, 'সফল', 'ডাইরেক্ট কল');
            document.getElementById('admin-direct-phone').value = '';
            document.getElementById('admin-direct-message').value = '';
        } else {
            showToast(`কল ব্যর্থ: ${data.message}`, 'danger');
            addCallLog(`+880${phone}`, 'ব্যর্থ', 'ডাইরেক্ট কল');
        }
    } catch (e) {
        console.error('Direct call error:', e);
        showToast('কল পাঠাতে ব্যর্থ!', 'danger');
        addCallLog(`+880${phone}`, 'ত্রুটি', 'ডাইরেক্ট কল');
    } finally {
        btn.disabled = false;
        btn.innerHTML = '<i class="bi bi-telephone-outbound me-1"></i> কল করুন';
    }
}

// Call Log (local)
function addCallLog(phone, status, type) {
    const time = new Date().toLocaleTimeString('bn-BD');
    callLog.unshift({ phone, status, type, time });
    if (callLog.length > 20) callLog.pop();
    renderCallLog();
}

function renderCallLog() {
    const container = document.getElementById('admin-call-log');
    if (!callLog.length) {
        container.innerHTML = `<div class="text-center text-muted p-4"><i class="bi bi-telephone fs-1 d-block mb-2"></i>কোনো কল লগ নেই</div>`;
        return;
    }
    container.innerHTML = callLog.map(l => {
        const statusClass = l.status === 'সফল' ? 'text-success' : l.status === 'ব্যর্থ' ? 'text-danger' : 'text-warning';
        const icon = l.status === 'সফল' ? 'bi-check-circle-fill' : l.status === 'ব্যর্থ' ? 'bi-x-circle-fill' : 'bi-exclamation-circle-fill';
        return `
            <div class="call-log-item d-flex justify-content-between align-items-center">
                <div>
                    <i class="bi ${icon} ${statusClass} me-2"></i>
                    <span class="fw-semibold">${l.phone}</span>
                    <small class="text-muted ms-2">${l.type}</small>
                </div>
                <div class="text-end">
                    <span class="call-status ${statusClass}">${l.status}</span>
                    <div class="call-time">${l.time}</div>
                </div>
            </div>`;
    }).join('');
}

// Admin order actions
async function updateOrderStatus(orderId, status) {
    try {
        const res = await fetch(`${API}/api/admin/orders/${orderId}/status`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ status })
        });
        if (res.ok) {
            showToast(`অর্ডার স্ট্যাটাস: ${status}`, 'success');
            loadAdminDashboard();
        } else {
            showToast('স্ট্যাটাস আপডেট ব্যর্থ', 'danger');
        }
    } catch (e) {
        showToast('ত্রুটি হয়েছে!', 'danger');
    }
}

async function deleteOrder(orderId) {
    if (!confirm('এই অর্ডার মুছে ফেলতে চান?')) return;
    try {
        const res = await fetch(`${API}/api/admin/orders/${orderId}`, { method: 'DELETE' });
        if (res.ok) {
            showToast('অর্ডার মুছে ফেলা হয়েছে', 'success');
            loadAdminDashboard();
        } else {
            showToast('মুছতে ব্যর্থ', 'danger');
        }
    } catch (e) {
        showToast('ত্রুটি হয়েছে!', 'danger');
    }
}

// ============================
// Utilities
// ============================
function formatPrice(num) {
    return Number(num).toLocaleString('en-IN', { minimumFractionDigits: 0, maximumFractionDigits: 2 });
}

function escapeHtml(str) {
    return String(str).replace(/'/g, "\\'").replace(/"/g, '&quot;');
}

function showToast(message, type = 'info') {
    const toast = document.getElementById('toast');
    const body = document.getElementById('toast-message');
    body.textContent = message;

    // Color the header icon
    const header = toast.querySelector('.toast-header i');
    header.className = 'bi me-2';
    if (type === 'success') { header.classList.add('bi-check-circle-fill', 'text-success'); }
    else if (type === 'danger') { header.classList.add('bi-x-circle-fill', 'text-danger'); }
    else if (type === 'warning') { header.classList.add('bi-exclamation-triangle-fill', 'text-warning'); }
    else { header.classList.add('bi-bell-fill', 'text-primary'); }

    new bootstrap.Toast(toast, { delay: 3000 }).show();
}
