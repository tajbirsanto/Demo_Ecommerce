// Demo E-commerce Store - ManyDial Integration
// Global State
let products = [];
let cart = [];
let orders = [];

// Initialize
document.addEventListener('DOMContentLoaded', () => {
    loadProducts();
    loadCart();
    updateCartCount();
});

// ==================== Navigation ====================
function showSection(section) {
    // Hide all sections
    document.querySelectorAll('.content-section').forEach(s => s.classList.add('d-none'));
    
    // Show selected section
    const sectionEl = document.getElementById(`${section}-section`);
    if (sectionEl) {
        sectionEl.classList.remove('d-none');
        sectionEl.classList.add('fade-in');
    }
    
    // Update nav active state
    document.querySelectorAll('.nav-link').forEach(link => link.classList.remove('active'));
    
    // Load section-specific data
    switch(section) {
        case 'orders':
            loadOrders();
            break;
        case 'webhooks':
            loadWebhookLogs();
            break;
        case 'cart':
            renderCart();
            break;
    }

    // Hide hero on non-product pages
    const hero = document.getElementById('hero');
    if (section !== 'products') {
        hero.classList.add('d-none');
    } else {
        hero.classList.remove('d-none');
    }
}

// ==================== Products ====================
async function loadProducts() {
    try {
        const response = await fetch('/api/products');
        products = await response.json();
        renderProducts(products);
    } catch (error) {
        console.error('Error loading products:', error);
        showToast('Failed to load products', 'danger');
    }
}

function renderProducts(productsToRender) {
    const grid = document.getElementById('products-grid');
    grid.innerHTML = productsToRender.map(product => `
        <div class="col-lg-3 col-md-4 col-sm-6">
            <div class="card product-card h-100 position-relative">
                <span class="badge bg-primary">${product.category}</span>
                <img src="${product.imageUrl}" class="card-img-top" alt="${product.name}" 
                     onerror="this.onerror=null; this.src='https://placehold.co/300x200/e9ecef/495057?text=No+Image'">
                <div class="card-body d-flex flex-column">
                    <h6 class="card-title">${product.name}</h6>
                    <p class="card-text small text-muted flex-grow-1">${product.description.substring(0, 60)}...</p>
                    <div class="d-flex justify-content-between align-items-center mt-auto">
                        <span class="price">৳${product.price.toFixed(2)}</span>
                        <span class="badge bg-${product.stock > 10 ? 'success' : 'warning'}">
                            ${product.stock} in stock
                        </span>
                    </div>
                </div>
                <div class="card-footer bg-transparent border-0">
                    <button class="btn btn-primary w-100 btn-add-cart" onclick="addToCart(${product.id})">
                        <i class="bi bi-cart-plus me-2"></i>Add to Cart
                    </button>
                </div>
            </div>
        </div>
    `).join('');
}

function filterProducts(category) {
    document.querySelectorAll('.btn-group .btn').forEach(btn => btn.classList.remove('active'));
    event.target.classList.add('active');
    
    if (category === 'all') {
        renderProducts(products);
    } else {
        const filtered = products.filter(p => p.category === category);
        renderProducts(filtered);
    }
}

// ==================== Cart ====================
function loadCart() {
    const savedCart = localStorage.getItem('cart');
    if (savedCart) {
        cart = JSON.parse(savedCart);
    }
}

function saveCart() {
    localStorage.setItem('cart', JSON.stringify(cart));
    updateCartCount();
}

function addToCart(productId) {
    const product = products.find(p => p.id === productId);
    if (!product) return;
    
    const existingItem = cart.find(item => item.productId === productId);
    if (existingItem) {
        existingItem.quantity++;
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
    showToast(`${product.name} added to cart!`, 'success');
}

function removeFromCart(productId) {
    cart = cart.filter(item => item.productId !== productId);
    saveCart();
    renderCart();
}

function updateQuantity(productId, delta) {
    const item = cart.find(item => item.productId === productId);
    if (item) {
        item.quantity += delta;
        if (item.quantity <= 0) {
            removeFromCart(productId);
        } else {
            saveCart();
            renderCart();
        }
    }
}

function updateCartCount() {
    const count = cart.reduce((sum, item) => sum + item.quantity, 0);
    document.querySelectorAll('.cart-count').forEach(el => {
        el.textContent = count;
    });
}

function renderCart() {
    const cartItems = document.getElementById('cart-items');
    const checkoutBtn = document.getElementById('checkout-btn');
    
    if (cart.length === 0) {
        cartItems.innerHTML = `
            <div class="empty-state">
                <i class="bi bi-cart-x"></i>
                <p class="mt-3 text-muted">Your cart is empty</p>
                <button class="btn btn-primary" onclick="showSection('products')">
                    <i class="bi bi-bag me-2"></i>Continue Shopping
                </button>
            </div>
        `;
        checkoutBtn.disabled = true;
        updateCartTotals();
        return;
    }
    
    checkoutBtn.disabled = false;
    cartItems.innerHTML = cart.map(item => `
        <div class="cart-item d-flex align-items-center">
            <img src="${item.imageUrl}" alt="${item.productName}" class="me-3"
                 onerror="this.onerror=null; this.src='https://placehold.co/80x80/e9ecef/495057?text=Item'">
            <div class="flex-grow-1">
                <h6 class="mb-1">${item.productName}</h6>
                <span class="text-primary fw-bold">৳${item.price.toFixed(2)}</span>
            </div>
            <div class="quantity-controls me-3">
                <button class="btn btn-outline-secondary btn-sm" onclick="updateQuantity(${item.productId}, -1)">
                    <i class="bi bi-dash"></i>
                </button>
                <span class="fw-bold">${item.quantity}</span>
                <button class="btn btn-outline-secondary btn-sm" onclick="updateQuantity(${item.productId}, 1)">
                    <i class="bi bi-plus"></i>
                </button>
            </div>
            <div class="text-end me-3">
                <strong>৳${(item.price * item.quantity).toFixed(2)}</strong>
            </div>
            <button class="btn btn-outline-danger btn-sm" onclick="removeFromCart(${item.productId})">
                <i class="bi bi-trash"></i>
            </button>
        </div>
    `).join('');
    
    updateCartTotals();
}

function updateCartTotals() {
    const subtotal = cart.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    const shipping = cart.length > 0 ? 50 : 0;
    const total = subtotal + shipping;
    
    document.getElementById('cart-subtotal').textContent = `৳${subtotal.toFixed(2)}`;
    document.getElementById('cart-total').textContent = `৳${total.toFixed(2)}`;
}

// ==================== Checkout ====================
function showCheckout() {
    const modal = new bootstrap.Modal(document.getElementById('checkoutModal'));
    modal.show();
}

async function placeOrder() {
    const name = document.getElementById('customer-name').value;
    const email = document.getElementById('customer-email').value;
    const phone = document.getElementById('customer-phone').value;
    const city = document.getElementById('customer-city').value;
    const address = document.getElementById('customer-address').value;
    
    if (!name || !email || !phone || !city || !address) {
        showToast('Please fill in all required fields', 'danger');
        return;
    }
    
    // Validate phone number
    if (!/^1[0-9]{9}$/.test(phone)) {
        showToast('Please enter a valid phone number (e.g., 1XXXXXXXXX)', 'danger');
        return;
    }
    
    const order = {
        customerName: name,
        customerEmail: email,
        customerPhone: `+880${phone}`,
        shippingAddress: `${address}, ${city}`,
        items: cart,
        totalAmount: cart.reduce((sum, item) => sum + (item.price * item.quantity), 0) + 50
    };
    
    try {
        showToast('Processing your order...', 'info');
        
        const response = await fetch('/api/orders', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(order)
        });
        
        const result = await response.json();
        
        if (response.ok) {
            // Clear cart
            cart = [];
            saveCart();
            
            // Close checkout modal
            bootstrap.Modal.getInstance(document.getElementById('checkoutModal')).hide();
            
            // Show success modal
            document.getElementById('success-order-id').textContent = result.id;
            const successModal = new bootstrap.Modal(document.getElementById('orderSuccessModal'));
            successModal.show();
        } else {
            showToast(result.message || 'Failed to place order', 'danger');
        }
    } catch (error) {
        console.error('Error placing order:', error);
        showToast('Failed to place order. Please try again.', 'danger');
    }
}

// ==================== Orders ====================
async function loadOrders() {
    try {
        const response = await fetch('/api/orders');
        orders = await response.json();
        renderOrders();
    } catch (error) {
        console.error('Error loading orders:', error);
        showToast('Failed to load orders', 'danger');
    }
}

function renderOrders() {
    const ordersList = document.getElementById('orders-list');
    
    if (orders.length === 0) {
        ordersList.innerHTML = `
            <div class="empty-state">
                <i class="bi bi-receipt"></i>
                <p class="mt-3 text-muted">No orders yet</p>
                <button class="btn btn-primary" onclick="showSection('products')">
                    <i class="bi bi-bag me-2"></i>Start Shopping
                </button>
            </div>
        `;
        return;
    }
    
    ordersList.innerHTML = orders.map(order => {
        const statusClass = order.status.toLowerCase() === 'confirmed' ? 'confirmed' : 
                          order.status.toLowerCase() === 'cancelled' ? 'cancelled' : 'pending';
        
        return `
            <div class="card order-card status-${statusClass} mb-3">
                <div class="card-body">
                    <div class="d-flex justify-content-between align-items-start mb-3">
                        <div>
                            <h5 class="mb-1">Order #${order.id.substring(0, 8)}...</h5>
                            <small class="text-muted">${new Date(order.createdAt).toLocaleString()}</small>
                        </div>
                        <div class="text-end">
                            <span class="badge bg-${statusClass === 'confirmed' ? 'success' : statusClass === 'cancelled' ? 'danger' : 'warning'} mb-1">
                                ${order.status}
                            </span>
                            <br>
                            ${order.callStatus ? `<span class="badge bg-info call-status-badge"><i class="bi bi-telephone me-1"></i>${order.callStatus}</span>` : ''}
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-6">
                            <p class="mb-1"><strong>Customer:</strong> ${order.customerName}</p>
                            <p class="mb-1"><strong>Phone:</strong> ${order.customerPhone}</p>
                            <p class="mb-1"><strong>Address:</strong> ${order.shippingAddress}</p>
                        </div>
                        <div class="col-md-6">
                            <p class="mb-1"><strong>Items:</strong> ${order.items.length} product(s)</p>
                            <p class="mb-1"><strong>Total:</strong> <span class="text-primary fw-bold">৳${order.totalAmount.toFixed(2)}</span></p>
                        </div>
                    </div>
                    <hr>
                    <div class="d-flex justify-content-between align-items-center">
                        <div>
                            ${order.items.map(item => `
                                <span class="badge bg-light text-dark me-1">${item.productName} x${item.quantity}</span>
                            `).join('')}
                        </div>
                        <button class="btn btn-sm btn-outline-primary" onclick="resendCall('${order.id}')">
                            <i class="bi bi-telephone-outbound me-1"></i>Resend Call
                        </button>
                    </div>
                </div>
            </div>
        `;
    }).join('');
}

async function resendCall(orderId) {
    try {
        const response = await fetch(`/api/orders/${orderId}/resend-call`, {
            method: 'POST'
        });
        const result = await response.json();
        
        if (response.ok) {
            showToast('Confirmation call resent successfully!', 'success');
            loadOrders();
        } else {
            showToast(result.message || 'Failed to resend call', 'danger');
        }
    } catch (error) {
        console.error('Error resending call:', error);
        showToast('Failed to resend call', 'danger');
    }
}

// ==================== Call Center ====================
function loadCallCenter() {
    const email = document.getElementById('agent-email').value;
    const callerId = document.getElementById('cc-caller-id').value;
    
    if (!email || !callerId) {
        showToast('Please enter agent email and caller ID', 'warning');
        return;
    }
    
    const iframeUrl = `https://callcenter.manydial.com?email=${encodeURIComponent(email)}&callerId=${encodeURIComponent(callerId)}`;
    
    document.getElementById('call-center-placeholder').classList.add('d-none');
    const iframe = document.getElementById('call-center-iframe');
    iframe.src = iframeUrl;
    iframe.classList.remove('d-none');
    
    showToast('Call Center loaded successfully!', 'success');
}

function openCallCenterFullscreen() {
    const email = document.getElementById('agent-email').value;
    const callerId = document.getElementById('cc-caller-id').value;
    const url = `https://callcenter.manydial.com?email=${encodeURIComponent(email)}&callerId=${encodeURIComponent(callerId)}`;
    window.open(url, '_blank');
}

async function initiateClickToCall() {
    const number = document.getElementById('click-call-number').value;
    const email = document.getElementById('agent-email').value;
    const callerId = document.getElementById('cc-caller-id').value;
    
    if (!number || !email) {
        showToast('Please enter customer phone and agent email', 'warning');
        return;
    }
    
    try {
        const response = await fetch('/api/manydial/click-to-call', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                callerId: callerId,
                email: email,
                number: number.startsWith('0') ? number : `0${number}`,
                payload: JSON.stringify({ source: 'demo-store', timestamp: new Date().toISOString() })
            })
        });
        
        const result = await response.json();
        
        if (result.success) {
            showToast('Call initiated successfully!', 'success');
        } else {
            showToast(result.message || 'Failed to initiate call', 'danger');
        }
    } catch (error) {
        console.error('Error initiating call:', error);
        showToast('Failed to initiate call', 'danger');
    }
}

// ==================== Webhooks ====================
async function loadWebhookLogs() {
    try {
        const response = await fetch('/api/webhooks/logs');
        const logs = await response.json();
        renderWebhookLogs(logs);
    } catch (error) {
        console.error('Error loading webhook logs:', error);
        showToast('Failed to load webhook logs', 'danger');
    }
}

function renderWebhookLogs(logs) {
    const container = document.getElementById('webhook-logs');
    
    if (logs.length === 0) {
        container.innerHTML = `
            <div class="empty-state">
                <i class="bi bi-broadcast"></i>
                <p class="mt-3 text-muted">No webhook events received yet</p>
                <p class="small text-muted">Place an order to see call delivery webhooks</p>
            </div>
        `;
        return;
    }
    
    container.innerHTML = logs.map(log => `
        <div class="webhook-log">
            <div class="d-flex justify-content-between mb-2">
                <span class="log-type">[${log.type.toUpperCase()}]</span>
                <span class="log-time">${new Date(log.receivedAt).toLocaleString()}</span>
            </div>
            <pre class="mb-0">${formatJSON(log.payload)}</pre>
        </div>
    `).join('');
}

async function clearWebhookLogs() {
    try {
        await fetch('/api/webhooks/logs', { method: 'DELETE' });
        loadWebhookLogs();
        showToast('Webhook logs cleared', 'success');
    } catch (error) {
        console.error('Error clearing logs:', error);
    }
}

// ==================== API Demo ====================
async function testCallAutomation() {
    const number = document.getElementById('demo-call-number').value;
    const welcomeMsg = document.getElementById('demo-welcome-msg').value;
    
    if (!number) {
        showToast('Please enter a phone number', 'warning');
        return;
    }
    
    const request = {
        callPayload: `demo-${Date.now()}`,
        number: `+880${number}`,
        perCallDuration: '3',
        messages: {
            welcome: welcomeMsg,
            repeat: '2',
            sms: 'Thank you for trying our demo!',
            menuMessage1: 'You selected option 1. This is a demo of ManyDial call automation. Thank you!',
            sms1: 'You selected Product Info. Visit our website for more details.',
            menuMessage2: 'You selected option 2. Our support team will contact you soon. Thank you!',
            sms2: 'You selected Support. Our team will reach out shortly.'
        },
        buttons: [
            { id: 'menuMessage1', key: '1', value: 'Product Info' },
            { id: 'menuMessage2', key: '2', value: 'Support' }
        ]
    };
    
    try {
        logApiResponse('Dispatching call...', 'info');
        
        const response = await fetch('/api/manydial/call/dispatch', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(request)
        });
        
        const result = await response.json();
        logApiResponse(JSON.stringify(result, null, 2), result.success ? 'success' : 'error');
        
        if (result.success) {
            showToast('Test call dispatched successfully!', 'success');
        } else {
            showToast(result.message || 'Failed to dispatch call', 'danger');
        }
    } catch (error) {
        logApiResponse(`Error: ${error.message}`, 'error');
        showToast('Failed to dispatch call', 'danger');
    }
}

async function testCreateCallCenter() {
    const callerId = document.getElementById('demo-cc-callerid').value;
    const totalAgents = document.getElementById('demo-cc-agents').value;
    
    const request = {
        callerId: callerId,
        callPrefix: '1000',
        totalAgents: totalAgents,
        domainUrl: window.location.origin
    };
    
    try {
        logApiResponse('Creating call center...', 'info');
        
        const response = await fetch('/api/manydial/call-center', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(request)
        });
        
        const result = await response.json();
        logApiResponse(JSON.stringify(result, null, 2), result.success ? 'success' : 'error');
        
        showToast(result.message, result.success ? 'success' : 'danger');
    } catch (error) {
        logApiResponse(`Error: ${error.message}`, 'error');
        showToast('Failed to create call center', 'danger');
    }
}

async function testCreateAgent() {
    const name = document.getElementById('demo-agent-name').value;
    const email = document.getElementById('demo-agent-email').value;
    const phone = document.getElementById('demo-agent-phone').value;
    const permission = document.getElementById('demo-agent-permission').value;
    const callerId = document.getElementById('demo-cc-callerid').value;
    
    if (!name || !email || !phone) {
        showToast('Please fill in all agent fields', 'warning');
        return;
    }
    
    const request = {
        callerId: callerId,
        name: name,
        email: email,
        phone: phone,
        password: 'demo123456',
        callPermission: permission,
        phoneType: 'WEBPHONE',
        expireDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]
    };
    
    try {
        logApiResponse('Creating agent...', 'info');
        
        const response = await fetch('/api/manydial/agent', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(request)
        });
        
        const result = await response.json();
        logApiResponse(JSON.stringify(result, null, 2), result.success ? 'success' : 'error');
        
        showToast(result.message, result.success ? 'success' : 'danger');
    } catch (error) {
        logApiResponse(`Error: ${error.message}`, 'error');
        showToast('Failed to create agent', 'danger');
    }
}

async function submitCallerIdRequest() {
    // Gather form data
    const formData = {
        ownerName: document.getElementById('cid-owner-name').value,
        businessName: document.getElementById('cid-business-name').value,
        email: document.getElementById('cid-email').value,
        phone: document.getElementById('cid-phone').value,
        nid: document.getElementById('cid-nid').value,
        dob: document.getElementById('cid-dob').value,
        gender: document.getElementById('cid-gender').value,
        fatherName: document.getElementById('cid-father').value,
        motherName: document.getElementById('cid-mother').value,
        division: document.getElementById('cid-division').value,
        district: document.getElementById('cid-district').value,
        upazilaOrThana: document.getElementById('cid-upazila').value,
        postCode: document.getElementById('cid-postcode').value,
        flatNo: document.getElementById('cid-flat').value || 'N/A',
        houseNoOrName: document.getElementById('cid-house').value,
        roadNoOrMoholla: document.getElementById('cid-road').value,
        areaOrVillage: document.getElementById('cid-area').value,
        date: new Date().toISOString().split('T')[0],
        smsEnabled: 'Yes',
        callerIdPayload: `demo-request-${Date.now()}`,
        // Placeholder base64 images for demo
        passportSizeImage: 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==',
        signature: 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==',
        seal: 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg=='
    };
    
    try {
        logApiResponse('Submitting Caller ID request...', 'info');
        
        const response = await fetch('/api/manydial/caller-id', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(formData)
        });
        
        const result = await response.json();
        logApiResponse(JSON.stringify(result, null, 2), result.success ? 'success' : 'error');
        
        bootstrap.Modal.getInstance(document.getElementById('callerIdModal')).hide();
        showToast(result.message, result.success ? 'success' : 'danger');
    } catch (error) {
        logApiResponse(`Error: ${error.message}`, 'error');
        showToast('Failed to submit request', 'danger');
    }
}

function logApiResponse(message, type = 'info') {
    const logEl = document.getElementById('api-response-log');
    const timestamp = new Date().toLocaleTimeString();
    const color = type === 'success' ? '#4ec9b0' : type === 'error' ? '#f14c4c' : '#dcdcaa';
    logEl.innerHTML += `\n<span style="color: ${color}">[${timestamp}] ${message}</span>`;
    logEl.scrollTop = logEl.scrollHeight;
}

// ==================== Utilities ====================
function showToast(message, type = 'info') {
    const toast = document.getElementById('toast');
    const toastMessage = document.getElementById('toast-message');
    
    toast.className = `toast bg-${type === 'danger' ? 'danger' : type === 'success' ? 'success' : type === 'warning' ? 'warning' : 'info'} text-white`;
    toastMessage.textContent = message;
    
    const bsToast = new bootstrap.Toast(toast);
    bsToast.show();
}

function formatJSON(jsonString) {
    try {
        const obj = JSON.parse(jsonString);
        return JSON.stringify(obj, null, 2);
    } catch {
        return jsonString;
    }
}
