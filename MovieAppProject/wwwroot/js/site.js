
// CART FUNCTIONALITY
const CartManager = {
    init: function () {
        this.updateCartCount();
        this.setupCartEventListeners();
    },

    getAntiForgeryToken: function () {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value ||
            document.querySelector('[name=__RequestVerificationToken]')?.value ||
            '';
    },

    updateCartCount: async function () {
        try {
            const response = await fetch('/Cart/GetCartItemCount');
            if (response.ok) {
                const data = await response.json();
                const cartCountElement = document.querySelector('.cart-count');
                if (cartCountElement) {
                    cartCountElement.textContent = data.count;
                    cartCountElement.style.display = data.count > 0 ? 'block' : 'none';
                }
            }
        } catch (error) {
            console.error('Error fetching cart count:', error);
        }
    },

    addToCart: async function (movieId) {
        try {
            const token = this.getAntiForgeryToken();
            if (!token) {
                throw new Error('Anti-forgery token not found');
            }

            const formData = new URLSearchParams();
            formData.append("id", movieId);
            formData.append("__RequestVerificationToken", token);

            const response = await fetch('/Cart/AddToCart', {
                method: 'POST',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: formData
            });

            if (response.ok) {
                await this.updateCartCount();
                this.showNotification('Movie added to cart successfully!', 'success');
                return true;
            } else {
                throw new Error('Server returned error status');
            }
        } catch (error) {
            console.error('Error adding to cart:', error);
            this.showNotification('Error adding movie to cart.', 'error');
            return false;
        }
    },

    removeFromCart: async function (movieId) {
        try {
            const token = this.getAntiForgeryToken();
            if (!token) {
                throw new Error('Anti-forgery token not found');
            }
            const formData = new URLSearchParams();
            formData.append("id", movieId);
            formData.append("__RequestVerificationToken", token);

            const response = await fetch('/Cart/RemoveFromCart', {
                method: 'POST',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: formData
            });

            if (response.ok) {
                const data = await response.json();
                await this.updateCartCount();
                this.showNotification(data.message || 'Movie removed from cart.', 'success');

                // Always reload to see changes - simpler approach
                location.reload();
                return true;
            } else {
                throw new Error('Server returned error status');
            }
        } catch (error) {
            console.error('Error removing from cart:', error);
            this.showNotification('Error removing movie from cart.', 'error');
            return false;
        }
    },

    setupCartEventListeners: function () {
        document.addEventListener('click', (e) => {
            // Add to cart buttons
            const addToCartBtn = e.target.closest('[data-action="add-to-cart"]');
            if (addToCartBtn) {
                e.preventDefault();
                const movieId = addToCartBtn.dataset.movieId;
                this.addToCart(movieId);
            }

            // Remove from cart buttons
            const removeFromCartBtn = e.target.closest('[data-action="remove-from-cart"]');
            if (removeFromCartBtn) {
                e.preventDefault();
                const movieId = removeFromCartBtn.dataset.movieId;
                this.removeFromCart(movieId);
            }
        });
    },

    showNotification: function (message, type = 'info') {
        // Remove existing notifications
        const existingNotifications = document.querySelectorAll('.custom-notification');
        existingNotifications.forEach(notification => notification.remove());

        // Create new notification
        const notification = document.createElement('div');
        notification.className = `custom-notification alert alert-${type} alert-dismissible fade show`;
        notification.style.cssText = `
            position: fixed;
            top: 100px;
            right: 20px;
            z-index: 1060;
            min-width: 300px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        `;

        notification.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        document.body.appendChild(notification);

        // Auto remove after 5 seconds
        setTimeout(() => {
            if (notification.parentNode) {
                notification.remove();
            }
        }, 5000);
    }
};

// WISHLIST FUNCTIONALITY
const WishlistManager = {
    addToWishlist: async function (movieId) {
        try {
            const token = CartManager.getAntiForgeryToken();
            if (!token) {
                throw new Error('Anti-forgery token not found');
            }
            const formData = new URLSearchParams();
            formData.append("id", movieId);
            formData.append("__RequestVerificationToken", token);

            const response = await fetch('/Wishlist/AddToWishlist', {
                method: 'POST',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: formData
            });

            if (response.ok) {
                CartManager.showNotification('Movie added to wishlist successfully!', 'success');
                // Update button state
                this.updateWishlistButton(movieId, true);
                return true;
            } else {
                throw new Error('Server returned error status');
            }
        } catch (error) {
            console.error('Error adding to wishlist:', error);
            CartManager.showNotification('Error adding movie to wishlist.', 'error');
            return false;
        }
    },

    removeFromWishlist: async function (movieId) {
        try {
            const token = CartManager.getAntiForgeryToken();
            if (!token) {
                throw new Error('Anti-forgery token not found');
            }

            const formData = new URLSearchParams();
            formData.append("id", movieId);
            formData.append("__RequestVerificationToken", token);

            const response = await fetch('/Wishlist/RemoveFromWishlist', {
                method: 'POST',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: formData
            });

            if (response.ok) {
                CartManager.showNotification('Movie removed from wishlist.', 'success');
                // Update button state
                this.updateWishlistButton(movieId, false);

                const wishlistItem = document.querySelector(`[data-wishlist-item-id="${movieId}"]`);
                if (wishlistItem)
                {
                    wishlistItem.remove();
                }
                return true;
            } else {
                throw new Error('Server returned error status');
            }
        } catch (error) {
            console.error('Error removing from wishlist:', error);
            CartManager.showNotification('Error removing movie from wishlist.', 'error');
            return false;
        }
    },

    updateWishlistButton: function (movieId, isInWishlist) {
        const wishlistBtn = document.querySelector(`[data-action="wishlist"][data-movie-id="${movieId}"]`);
        if (wishlistBtn) {
            if (isInWishlist) {
                wishlistBtn.dataset.wishlistAction = 'remove';
                wishlistBtn.classList.remove('btn-outline-danger');
                wishlistBtn.classList.add('btn-danger');
                wishlistBtn.innerHTML = '<i class="bi bi-heart-fill"></i>';
            } else {
                wishlistBtn.dataset.wishlistAction = 'add';
                wishlistBtn.classList.remove('btn-danger');
                wishlistBtn.classList.add('btn-outline-danger');
                wishlistBtn.innerHTML = '<i class="bi bi-heart"></i>';
            }
        }
    }
};

// SEARCH FUNCTIONALITY
const SearchManager = {
    init: function () {
        this.setupSearchListeners();
    },

    setupSearchListeners: function () {
        const searchInput = document.getElementById('movieSearch');
        if (!searchInput) return;

        let searchTimeout;

        searchInput.addEventListener('input', (e) => {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                this.handleSearch(e.target.value);
            }, 300);
        });

        searchInput.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') {
                e.preventDefault();
                this.submitSearch(searchInput.value);
            }
        });

        // Close suggestions when clicking outside
        document.addEventListener('click', (e) => {
            if (!e.target.closest('#searchSuggestions') && e.target !== searchInput) {
                this.hideSuggestions();
            }
        });

        // Handle suggestion clicks
        document.addEventListener('click', (e) => {
            const suggestion = e.target.closest('#searchSuggestions li');
            if (suggestion) {
                searchInput.value = suggestion.textContent;
                this.hideSuggestions();
                this.submitSearch(searchInput.value);
            }
        });
    },

    handleSearch: function (query) {
        if (query.length > 1) {
            this.showSuggestions(query);
        } else {
            this.hideSuggestions();
        }
    },

    showSuggestions: function (query) {
        // In a real app, you would fetch from server using AJAX
        // For now, using sample data
        const sampleResults = [
            "Inception",
            "Interstellar",
            "The Dark Knight",
            "Avengers: Endgame",
            "The Shawshank Redemption",
            "Pulp Fiction",
            "The Godfather"
        ];

        const matches = sampleResults.filter(m =>
            m.toLowerCase().includes(query.toLowerCase())
        );

        const suggestionsContainer = document.getElementById('searchSuggestions');
        if (!suggestionsContainer) return;

        if (matches.length > 0) {
            suggestionsContainer.innerHTML = matches.map(m =>
                `<li class="list-group-item list-group-item-action">${m}</li>`
            ).join('');
            suggestionsContainer.classList.remove('d-none');
        } else {
            this.hideSuggestions();
        }
    },

    hideSuggestions: function () {
        const suggestionsContainer = document.getElementById('searchSuggestions');
        if (suggestionsContainer) {
            suggestionsContainer.classList.add('d-none');
        }
    },

    submitSearch: function (query) {
        if (query.trim()) {
            const form = document.querySelector('form[action*="Index"]');
            if (form) {
                const searchInput = form.querySelector('input[name="searchTerm"]');
                if (searchInput) {
                    searchInput.value = query;
                    form.submit();
                }
            }
        }
    }
};


// STRIPE FUNCTIONALITY
const StripeManager = {
    init: function (stripeKey) {
        if (!document.getElementById('payment-form')) return;

        this.stripe = Stripe(stripeKey);
        this.elements = this.stripe.elements();
        this.cardElement = null;
        this.form = document.getElementById('payment-form');
        this.submitButton = this.form.querySelector('button[type="submit"]') || this.form.querySelector('#submit-button');
        this.cardErrors = this.form.querySelector('#card-errors');

        this.setupCardElement();
        this.setupPaymentForm();
    },

    setupCardElement: function () {
        if (!this.elements) return;

        this.cardElement = this.elements.create('card', {
            style: {
                base: {
                    fontSize: '16px',
                    color: '#ffffff',
                    '::placeholder': { color: '#aab7c4' },
                    backgroundColor: '#1a1a2e'
                }
            },
            hidePostalCode: true
        });

        const cardContainer = document.getElementById('card-element');
        if (!cardContainer) {
            console.error('Stripe card container not found!');
            return;
        }

        this.cardElement.mount('#card-element');

        this.cardElement.on('change', ({ error }) => {
            this.cardErrors.textContent = error ? error.message : '';
        });
    },

    setupPaymentForm: function () {
        this.form.addEventListener('submit', async (event) => {
            event.preventDefault();
            this.submitButton.disabled = true;
            this.submitButton.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Processing...';

            try {
                const token = CartManager.getAntiForgeryToken();
                console.log('Anti-forgery token:', token);

                // Create PaymentIntent on backend
                const response = await fetch('/Cart/CreatePaymentIntent', {
                    method: 'POST',
                    headers: { 'RequestVerificationToken': token }
                });

                const responseData = await response.json();
                console.log('PaymentIntent response:', responseData);

                if (!response.ok || !responseData.clientSecret) {
                    throw new Error(responseData.error || 'Failed to create payment intent');
                }

                if (!this.cardElement) {
                    throw new Error('Card element is not initialized');
                }

                // Confirm payment
                const confirmResult = await this.stripe.confirmCardPayment(responseData.clientSecret, {
                    payment_method: { card: this.cardElement }
                });

                console.log('Confirm payment result:', confirmResult);

                if (confirmResult.error) {
                    throw new Error(confirmResult.error.message);
                }

                // Notify backend of success
                const formData = new URLSearchParams();
                formData.append("paymentIntentId", confirmResult.paymentIntent.id);
                formData.append("__RequestVerificationToken", token);

                const processResponse = await fetch('/Cart/ProcessPaymentSuccess', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                    body: formData
                });

                const processResult = await processResponse.json();
                console.log('Process payment response:', processResult);

                if (processResult.success) {
                    this.showSuccess(processResult.purchaseId);
                } else {
                    throw new Error(processResult.message || 'Payment processing failed');
                }

            } catch (err) {
                console.error('Checkout Error:', err);
                this.cardErrors.textContent = `Stripe Error: ${err.message}`;
                this.submitButton.disabled = false;
                this.submitButton.innerHTML = '<i class="bi bi-lock-fill me-2"></i>Pay Now';
            }
        });
    },

    showSuccess: function (purchaseId) {
        const notification = document.createElement('div');
        notification.className = 'alert alert-success';
        notification.innerHTML = `<i class="bi bi-check-circle-fill me-2"></i>Payment successful! Redirecting...`;
        document.querySelector('.container').prepend(notification);

        setTimeout(() => {
            window.location.href = `/Cart/PurchaseComplete/${purchaseId}`;
        }, 2000);
    }
};


//  VIDEO PLAYER FUNCTIONALITY 
const VideoPlayerManager = {
    init: function (videoElement, movieId, isTrailer = false) {
        if (!videoElement) return;

        this.video = videoElement;
        this.movieId = movieId;
        this.isTrailer = isTrailer;

        this.setupVideoPlayer();
    },

    setupVideoPlayer: function () {
        if (!this.video) return;

        // Trailer duration limit
        if (this.isTrailer) {
            this.setupTrailerLimit();
        } else {
            // Resume playback for full movie
            this.restorePlaybackPosition();
        }

        // Save playback position on time update
        this.video.addEventListener('timeupdate', () => {
            if (!this.isTrailer) this.savePlaybackPosition();
        });

        this.setupKeyboardShortcuts();
        this.setupQualitySelector();
        this.video.addEventListener('error', handleVideoError);
    },

    setupTrailerLimit: function () {
        this.video.addEventListener('timeupdate', () => {
            if (this.video.currentTime > 180) { // 3 minutes(180s)
                this.video.pause();
                alert('Trailer ended. Purchase the movie to watch the full version.');
                this.video.currentTime = 0;
            }
        });
    },

    savePlaybackPosition: function () {
        localStorage.setItem(`movie_${this.movieId}_time`, this.video.currentTime);
        localStorage.setItem(`movie_${this.movieId}_last_played`, new Date().toISOString());
    },

    restorePlaybackPosition: function () {
        const savedTime = localStorage.getItem(`movie_${this.movieId}_time`);
        if (savedTime) {
            this.video.addEventListener('loadedmetadata', () => {
                this.video.currentTime = parseFloat(savedTime);
            }, { once: true });
        }
    },

    setupKeyboardShortcuts: function () {
        document.addEventListener('keydown', (e) => {
            if (e.target.tagName.toLowerCase() !== 'input' &&
                e.target.tagName.toLowerCase() !== 'textarea') {
                this.handleKeyboardShortcut(e);
            }
        });
    },

    handleKeyboardShortcut: function (e) {
        switch (e.code) {
            case 'Space':
                e.preventDefault();
                this.video.paused ? this.video.play() : this.video.pause();
                break;
            case 'ArrowRight':
                this.video.currentTime += 10;
                break;
            case 'ArrowLeft':
                this.video.currentTime -= 10;
                break;
            case 'ArrowUp':
                this.video.volume = Math.min(this.video.volume + 0.1, 1);
                break;
            case 'ArrowDown':
                this.video.volume = Math.max(this.video.volume - 0.1, 0);
                break;
            case 'KeyF':
                this.toggleFullscreen();
                break;
            case 'KeyM':
                e.preventDefault();
                this.video.muted = !this.video.muted;
                break;
        }
    },

    toggleFullscreen: function () {
        if (this.video.requestFullscreen) {
            this.video.requestFullscreen();
        } else if (this.video.webkitRequestFullscreen) {
            this.video.webkitRequestFullscreen();
        } else if (this.video.msRequestFullscreen) {
            this.video.msRequestFullscreen();
        }
    },

    setupQualitySelector: function () {
        const qualitySelect = document.querySelector('.quality-selector');
        if (qualitySelect) {
            qualitySelect.addEventListener('change', (e) => {
                this.changeQuality(e.target.value);
            });
        }
    },

    changeQuality: function (quality) {
        console.log('Changing quality to:', quality);
        // quality change if i had multiple sources for watching the movie
    }
};


function handleVideoError() {
    console.error('Video playback error');
    const videoContainer = document.querySelector('.video-player-wrapper');
    if (videoContainer) {
        videoContainer.innerHTML = `
            <div class="alert alert-danger d-flex align-items-center justify-content-center h-100">
                <div class="text-center">
                    <i class="bi bi-exclamation-triangle-fill display-4 text-danger mb-3"></i>
                    <h4>Video Playback Error</h4>
                    <p class="mb-3">The video could not be loaded. Please try again later.</p>
                    <button onclick="location.reload()" class="btn btn-primary">
                        <i class="bi bi-arrow-clockwise me-2"></i> Retry
                    </button>
                </div>
            </div>
        `;
    }
}


// MAIN INITIALIZATION 
document.addEventListener('DOMContentLoaded', function () {
    CartManager.init();
    SearchManager.init();

    const videoElement = document.getElementById('moviePlayer');
    if (videoElement) {
        const movieId = videoElement.dataset.movieId;
        const isTrailer = !!videoElement.closest('.video-container').querySelector('.alert-info'); // trailer detection
        videoElement.addEventListener('error', function () {
            handleVideoError();
        });

        VideoPlayerManager.init(videoElement, movieId, isTrailer);
    }


    document.addEventListener('click', (e) => {
        const wishlistBtn = e.target.closest('[data-action="wishlist"]');
        if (wishlistBtn) {
            e.preventDefault();
            const movieId = wishlistBtn.dataset.movieId;
            const action = wishlistBtn.dataset.wishlistAction;

            if (action === 'add') {
                WishlistManager.addToWishlist(movieId);
            } else if (action === 'remove') {
                WishlistManager.removeFromWishlist(movieId);
            }
        }
    });

   
    const stripeKeyElement = document.getElementById('stripe-publishable-key');
    if (stripeKeyElement) {
        const stripeKey = stripeKeyElement.value;
        StripeManager.init(stripeKey);
    }


    const cartPageButtons = document.querySelectorAll('[data-action="remove-from-cart"]');
    if (cartPageButtons.length > 0) {
        document.addEventListener('click', (e) => {
            const removeBtn = e.target.closest('[data-action="remove-from-cart"]');
            if (removeBtn) {
                e.preventDefault();
                const movieId = removeBtn.dataset.movieId;
                CartManager.removeFromCart(movieId);
            }
        });
    }


    
    // Smooth scrolling for # links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            const href = this.getAttribute('href');

           
            if (href === '#' || href === '#!' || href === '#0') {
                return true; 
            }

            try {
                const target = document.querySelector(href);
                if (target) {
                    e.preventDefault();
                    target.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });

                   
                    history.pushState(null, null, href);
                }
            } catch (error) {
                console.warn('Smooth scroll failed for selector:', href, error);
            }
        });
    });

});

//  ERROR HANDLING 
window.addEventListener('error', function (e) {
    console.error('Global error:', e.error);
});

// Handle unhandled promise rejections
window.addEventListener('unhandledrejection', function (e) {
    console.error('Unhandled promise rejection:', e.reason);
});