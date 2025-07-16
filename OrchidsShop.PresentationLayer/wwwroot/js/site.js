// =============================================================================
// ORCHID SHOP - MAIN JAVASCRIPT FILE
// =============================================================================

// Global variables and constants
window.OrchidShop = window.OrchidShop || {};

// Configuration
OrchidShop.config = {
    apiBaseUrl: '/api/',
    defaultPageSize: 12,
    maxCartItems: 50,
    sessionTimeout: 30 * 60 * 1000, // 30 minutes
    animationDuration: 300
};

// =============================================================================
// UTILITY FUNCTIONS
// =============================================================================

// Debounce function for search input
function debounce(func, wait, immediate) {
    let timeout;
    return function executedFunction(...args) {
        const later = function() {
            timeout = null;
            if (!immediate) func(...args);
        };
        const callNow = immediate && !timeout;
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
        if (callNow) func(...args);
    };
}

// Format currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD'
    }).format(amount || 0);
}

// Format date
function formatDate(dateString) {
    return new Date(dateString).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
    });
}

// Validate email
function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

// Generate random ID
function generateId() {
    return Date.now().toString(36) + Math.random().toString(36).substr(2);
}

// =============================================================================
// ANIMATION AND UI HELPERS
// =============================================================================

// Smooth scroll to element
function scrollToElement(elementId, offset = 0) {
    const element = document.getElementById(elementId);
    if (element) {
        const elementPosition = element.offsetTop - offset;
        window.scrollTo({
            top: elementPosition,
            behavior: 'smooth'
        });
    }
}

// Add fade-in animation to elements
function addFadeInAnimation(elements) {
    if (typeof elements === 'string') {
        elements = document.querySelectorAll(elements);
    }
    
    elements.forEach((el, index) => {
        el.style.opacity = '0';
        el.style.transform = 'translateY(20px)';
        
        setTimeout(() => {
            el.style.transition = 'opacity 0.5s ease, transform 0.5s ease';
            el.style.opacity = '1';
            el.style.transform = 'translateY(0)';
        }, index * 100);
    });
}

// Loading state management
function setLoadingState(element, isLoading, originalText = '') {
    if (typeof element === 'string') {
        element = document.querySelector(element);
    }
    
    if (!element) return;
    
    if (isLoading) {
        element.disabled = true;
        element.dataset.originalText = element.innerHTML;
        element.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Loading...';
    } else {
        element.disabled = false;
        element.innerHTML = element.dataset.originalText || originalText;
    }
}

// =============================================================================
// CART MANAGEMENT
// =============================================================================

OrchidShop.Cart = {
    // Get cart from localStorage
    getCart: function() {
        try {
            return JSON.parse(localStorage.getItem('orchidCart') || '[]');
        } catch (error) {
            console.error('Error reading cart:', error);
            return [];
        }
    },

    // Save cart to localStorage
    saveCart: function(cart) {
        try {
            localStorage.setItem('orchidCart', JSON.stringify(cart));
            this.updateBadge();
            return true;
        } catch (error) {
            console.error('Error saving cart:', error);
            return false;
        }
    },

    // Add item to cart
    addItem: function(orchidId, name, price, imageUrl, quantity = 1) {
        const cart = this.getCart();
        
        // Check if item already exists
        const existingItem = cart.find(item => item.orchidId === orchidId);
        
        if (existingItem) {
            existingItem.quantity += quantity;
        } else {
            cart.push({
                orchidId: orchidId,
                name: name,
                price: parseFloat(price) || 0,
                imageUrl: imageUrl || '/assets/logos/orchid-openai.png',
                quantity: parseInt(quantity) || 1,
                addedAt: new Date().toISOString()
            });
        }
        
        // Check cart limit
        if (cart.length > OrchidShop.config.maxCartItems) {
            showAlert('warning', `Maximum ${OrchidShop.config.maxCartItems} different items allowed in cart`);
            return false;
        }
        
        if (this.saveCart(cart)) {
            showAlert('success', `${name} added to cart!`);
            return true;
        }
        
        return false;
    },

    // Remove item from cart
    removeItem: function(orchidId) {
        const cart = this.getCart();
        const filteredCart = cart.filter(item => item.orchidId !== orchidId);
        return this.saveCart(filteredCart);
    },

    // Update item quantity
    updateQuantity: function(orchidId, quantity) {
        const cart = this.getCart();
        const item = cart.find(item => item.orchidId === orchidId);
        
        if (item) {
            if (quantity <= 0) {
                return this.removeItem(orchidId);
            }
            item.quantity = parseInt(quantity) || 1;
            return this.saveCart(cart);
        }
        
        return false;
    },

    // Clear cart
    clear: function() {
        localStorage.removeItem('orchidCart');
        this.updateBadge();
    },

    // Get cart total
    getTotal: function() {
        const cart = this.getCart();
        return cart.reduce((total, item) => total + (item.price * item.quantity), 0);
    },

    // Get cart item count
    getItemCount: function() {
        const cart = this.getCart();
        return cart.reduce((count, item) => count + item.quantity, 0);
    },

    // Update cart badge in navbar
    updateBadge: function() {
        const badge = document.getElementById('cartBadge');
        if (badge) {
            const count = this.getItemCount();
            if (count > 0) {
                badge.textContent = count;
                badge.style.display = 'block';
            } else {
                badge.style.display = 'none';
            }
        }
    }
};

// =============================================================================
// API COMMUNICATION
// =============================================================================

OrchidShop.Api = {
    // Generic API call function
    call: async function(endpoint, options = {}) {
        const defaultOptions = {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            }
        };
        
        const config = { ...defaultOptions, ...options };
        
        try {
            const response = await fetch(endpoint, config);
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            return await response.json();
        } catch (error) {
            console.error('API call failed:', error);
            throw error;
        }
    },

    // Search orchids
    searchOrchids: async function(searchParams) {
        const params = new URLSearchParams(searchParams);
        return await this.call(`/Index?handler=Orchids&${params.toString()}`);
    },

    // Get categories
    getCategories: async function() {
        return await this.call('/Index?handler=Categories');
    }
};

// =============================================================================
// SEARCH AND FILTERING
// =============================================================================

OrchidShop.Search = {
    currentFilters: {},
    
    // Initialize search functionality
    init: function() {
        this.bindEvents();
        this.loadFiltersFromUrl();
    },

    // Bind search events
    bindEvents: function() {
        // Search input with debounce
        const searchInput = document.getElementById('searchInput');
        if (searchInput) {
            searchInput.addEventListener('input', debounce(this.performSearch.bind(this), 500));
        }

        // Filter change events
        const filterElements = document.querySelectorAll('#categoryFilter, #sortSelect, input[name="typeFilter"], #minPrice, #maxPrice');
        filterElements.forEach(element => {
            element.addEventListener('change', this.performSearch.bind(this));
        });
    },

    // Perform search with current filters
    performSearch: function() {
        this.collectFilters();
        this.updateUrl();
        this.loadResults();
    },

    // Collect current filter values
    collectFilters: function() {
        this.currentFilters = {
            search: this.getValue('#searchInput'),
            category: this.getValue('#categoryFilter'),
            isNatural: this.getValue('input[name="typeFilter"]:checked'),
            minPrice: this.getValue('#minPrice'),
            maxPrice: this.getValue('#maxPrice'),
            sortBy: this.getValue('#sortSelect')?.split(',')[0],
            sortDir: this.getValue('#sortSelect')?.split(',')[1],
            page: 1
        };
    },

    // Get element value safely
    getValue: function(selector) {
        const element = document.querySelector(selector);
        return element ? element.value : '';
    },

    // Update URL with current filters
    updateUrl: function() {
        const params = new URLSearchParams();
        
        Object.keys(this.currentFilters).forEach(key => {
            const value = this.currentFilters[key];
            if (value && value !== '' && value !== 'undefined') {
                params.append(key, value);
            }
        });
        
        const newUrl = `${window.location.pathname}?${params.toString()}`;
        window.history.pushState(null, '', newUrl);
    },

    // Load filters from URL parameters
    loadFiltersFromUrl: function() {
        const params = new URLSearchParams(window.location.search);
        
        params.forEach((value, key) => {
            const element = document.querySelector(`#${key}, input[name="${key}"][value="${value}"]`);
            if (element) {
                if (element.type === 'radio') {
                    element.checked = true;
                } else {
                    element.value = value;
                }
            }
        });
    },

    // Load search results
    loadResults: async function() {
        try {
            showLoading();
            const response = await OrchidShop.Api.searchOrchids(this.currentFilters);
            
            if (response.success) {
                this.renderResults(response.data, response.pagination);
            } else {
                showAlert('danger', response.message || 'Failed to load results');
            }
        } catch (error) {
            console.error('Search failed:', error);
            showAlert('danger', 'Error loading search results');
        } finally {
            hideLoading();
        }
    },

    // Render search results
    renderResults: function(orchids, pagination) {
        const grid = document.getElementById('orchidGrid');
        if (!grid) return;

        if (orchids && orchids.length > 0) {
            let html = '';
            orchids.forEach(orchid => {
                html += this.createOrchidCard(orchid);
            });
            grid.innerHTML = html;
            
            // Add fade-in animation
            addFadeInAnimation(grid.querySelectorAll('.orchid-card'));
        } else {
            grid.innerHTML = this.createEmptyState();
        }
    },

    // Create orchid card HTML
    createOrchidCard: function(orchid) {
        const imageUrl = orchid.url || '/assets/logos/orchid-openai.png';
        const description = orchid.description || 'Beautiful orchid variety';
        const typeClass = orchid.isNatural ? 'natural-badge' : 'artificial-badge';
        const typeText = orchid.isNatural ? 'Natural' : 'Artificial';
        
        return `
            <div class="col-xl-3 col-lg-4 col-md-6 mb-4">
                <div class="card orchid-card">
                    <div class="position-relative">
                        <img src="${imageUrl}" class="card-img-top orchid-image" alt="${orchid.name}"
                             onerror="this.src='/assets/logos/orchid-openai.png'" loading="lazy">
                        <span class="position-absolute top-0 start-0 m-2 ${typeClass}">${typeText}</span>
                    </div>
                    <div class="card-body">
                        <h6 class="card-title fw-bold text-truncate" title="${orchid.name}">${orchid.name}</h6>
                        <p class="card-text text-muted small" style="height: 3em; overflow: hidden;">${description}</p>
                        <div class="d-flex justify-content-between align-items-center">
                            <span class="price-badge">${formatCurrency(orchid.price)}</span>
                            <button class="btn btn-primary btn-sm" onclick="OrchidShop.Cart.addItem('${orchid.id}', '${orchid.name.replace(/'/g, "\\'")}', ${orchid.price}, '${imageUrl.replace(/'/g, "\\'")}')">
                                <i class="bi bi-cart-plus me-1"></i>Add to Cart
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;
    },

    // Create empty state HTML
    createEmptyState: function() {
        return `
            <div class="col-12 text-center py-5">
                <div class="text-muted">
                    <i class="bi bi-flower1" style="font-size: 4rem; opacity: 0.3;"></i>
                    <h4 class="mt-3">No orchids found</h4>
                    <p>Try adjusting your filters or search terms.</p>
                    <button class="btn btn-outline-primary" onclick="OrchidShop.Search.clearFilters()">Clear Filters</button>
                </div>
            </div>
        `;
    },

    // Clear all filters
    clearFilters: function() {
        // Reset form elements
        const elements = document.querySelectorAll('#searchInput, #categoryFilter, #minPrice, #maxPrice');
        elements.forEach(el => el.value = '');
        
        // Reset radio buttons
        const typeAll = document.querySelector('input[name="typeFilter"][value=""]');
        if (typeAll) typeAll.checked = true;
        
        // Reset sort
        const sortSelect = document.getElementById('sortSelect');
        if (sortSelect) sortSelect.value = 'Name,Asc';
        
        // Perform search
        this.performSearch();
    }
};

// =============================================================================
// INITIALIZATION AND EVENT HANDLERS
// =============================================================================

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    // Initialize cart badge
    OrchidShop.Cart.updateBadge();
    
    // Initialize search if on main page
    if (document.getElementById('orchidGrid')) {
        OrchidShop.Search.init();
    }
    
    // Initialize tooltips (if Bootstrap is available)
    if (typeof bootstrap !== 'undefined') {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function(tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }
    
    // Mobile filter sidebar
    initMobileFilters();
    
    // Session timeout warning
    initSessionTimeout();
});

// Mobile filter sidebar functionality
function initMobileFilters() {
    const filterToggle = document.getElementById('filterToggle');
    const filterSidebar = document.querySelector('.filter-sidebar');
    const filterOverlay = document.querySelector('.filter-overlay');
    
    if (filterToggle && filterSidebar) {
        filterToggle.addEventListener('click', function() {
            filterSidebar.classList.add('show');
            if (filterOverlay) filterOverlay.classList.add('show');
        });
        
        if (filterOverlay) {
            filterOverlay.addEventListener('click', function() {
                filterSidebar.classList.remove('show');
                filterOverlay.classList.remove('show');
            });
        }
    }
}

// Session timeout functionality
function initSessionTimeout() {
    let sessionTimer;
    
    function resetSessionTimer() {
        clearTimeout(sessionTimer);
        sessionTimer = setTimeout(() => {
            if (window.isLoggedIn) {
                showAlert('warning', 'Your session will expire in 5 minutes due to inactivity.', 10000);
            }
        }, OrchidShop.config.sessionTimeout - 5 * 60 * 1000); // 5 minutes before expiry
    }
    
    // Reset timer on user activity
    ['mousedown', 'mousemove', 'keypress', 'scroll', 'touchstart'].forEach(event => {
        document.addEventListener(event, resetSessionTimer, { passive: true });
    });
    
    resetSessionTimer();
}

// =============================================================================
// GLOBAL HELPER FUNCTIONS (already defined in layout)
// =============================================================================

// Make global functions available for backward compatibility
window.addToCart = function(orchidId, name, price, imageUrl, quantity = 1) {
    return OrchidShop.Cart.addItem(orchidId, name, price, imageUrl, quantity);
};

window.updateCartBadge = function() {
    return OrchidShop.Cart.updateBadge();
};

// Export for module use
if (typeof module !== 'undefined' && module.exports) {
    module.exports = OrchidShop;
}
