document.addEventListener("DOMContentLoaded", function () {
    setupServicesGalleryCarousel();
    setupReviewsMobileCarousel();

    function setupServicesGalleryCarousel() {
        const carousel = document.querySelector(".home-services-gallery__carousel");
        if (!carousel) return;

        const viewport = carousel.querySelector(".home-services-gallery__viewport");
        const track = carousel.querySelector(".home-services-gallery__track");
        const items = Array.from(carousel.querySelectorAll(".home-services-gallery__item"));
        const prevButton = carousel.querySelector(".home-carousel__arrow--prev");
        const nextButton = carousel.querySelector(".home-carousel__arrow--next");

        if (!viewport || !track || !items.length || !prevButton || !nextButton) return;

        let currentIndex = 0;

        function getVisibleItems() {
            if (window.innerWidth <= 768) return 1;
            if (window.innerWidth <= 1200) return 2;
            return 3;
        }

        function getGap() {
            if (window.innerWidth <= 768) return 0;
            return 18;
        }

        function getItemStep() {
            const firstItem = items[0];
            if (!firstItem) return 0;

            const itemWidth = firstItem.getBoundingClientRect().width;
            return itemWidth + getGap();
        }

        function getMaxIndex() {
            const visibleItems = getVisibleItems();
            return Math.max(items.length - visibleItems, 0);
        }

        function updateCarousel() {
            const maxIndex = getMaxIndex();

            if (currentIndex > maxIndex) {
                currentIndex = maxIndex;
            }

            const step = getItemStep();
            const translateX = currentIndex * step;

            track.style.transform = `translateX(-${translateX}px)`;

            prevButton.disabled = currentIndex === 0;
            nextButton.disabled = currentIndex >= maxIndex;
        }

        prevButton.addEventListener("click", function () {
            if (currentIndex > 0) {
                currentIndex--;
                updateCarousel();
            }
        });

        nextButton.addEventListener("click", function () {
            const maxIndex = getMaxIndex();

            if (currentIndex < maxIndex) {
                currentIndex++;
                updateCarousel();
            }
        });

        window.addEventListener("resize", updateCarousel);
        updateCarousel();
    }

    function setupReviewsMobileCarousel() {
        const carousel = document.querySelector(".home-reviews__carousel");
        if (!carousel) return;

        const viewport = carousel.querySelector(".home-reviews__viewport");
        const track = carousel.querySelector(".home-reviews__track");
        const cards = Array.from(carousel.querySelectorAll(".home-reviews__card"));
        const prevButton = carousel.querySelector(".home-carousel__arrow--prev");
        const nextButton = carousel.querySelector(".home-carousel__arrow--next");

        if (!viewport || !track || !cards.length || !prevButton || !nextButton) return;

        let currentIndex = 0;

        function isMobile() {
            return window.innerWidth <= 768;
        }

        function getCardWidth() {
            const firstCard = cards[0];
            if (!firstCard) return 0;
            return firstCard.getBoundingClientRect().width;
        }

        function getMaxIndex() {
            return Math.max(cards.length - 1, 0);
        }

        function updateCarousel() {
            if (isMobile()) {
                const cardWidth = getCardWidth();
                const translateX = currentIndex * cardWidth;

                track.style.transform = `translateX(-${translateX}px)`;
                prevButton.style.display = "inline-flex";
                nextButton.style.display = "inline-flex";
                prevButton.disabled = currentIndex === 0;
                nextButton.disabled = currentIndex === getMaxIndex();
            } else {
                currentIndex = 0;
                track.style.transform = "";
                prevButton.style.display = "none";
                nextButton.style.display = "none";
                prevButton.disabled = false;
                nextButton.disabled = false;
            }
        }

        prevButton.addEventListener("click", function () {
            if (!isMobile()) return;

            if (currentIndex > 0) {
                currentIndex--;
                updateCarousel();
            }
        });

        nextButton.addEventListener("click", function () {
            if (!isMobile()) return;

            if (currentIndex < getMaxIndex()) {
                currentIndex++;
                updateCarousel();
            }
        });

        window.addEventListener("resize", updateCarousel);
        updateCarousel();
    

        nextButton.addEventListener("click", function () {
            if (!isMobile()) return;

            if (currentIndex < getMaxIndex()) {
                currentIndex++;
                updateCarousel();
            }
        });

        window.addEventListener("resize", updateCarousel);
        updateCarousel();
    }
});