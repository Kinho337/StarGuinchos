document.addEventListener("DOMContentLoaded", function () {
    setupScrollCarousel({
        carouselSelector: ".home-services-gallery__carousel",
        trackSelector: ".home-services-gallery__track",
        itemSelector: ".home-services-gallery__item",
        prevSelector: ".home-carousel__arrow--prev",
        nextSelector: ".home-carousel__arrow--next",
        loop: true
    });

    setupScrollCarousel({
        carouselSelector: ".home-reviews__carousel",
        trackSelector: ".home-reviews__track",
        itemSelector: ".home-reviews__card",
        prevSelector: ".home-carousel__arrow--prev",
        nextSelector: ".home-carousel__arrow--next",
        loop: true
    });
});

function setupScrollCarousel(config) {
    const carousels = document.querySelectorAll(config.carouselSelector);

    carousels.forEach(function (carousel) {
        const track = carousel.querySelector(config.trackSelector);
        const prevButton = carousel.querySelector(config.prevSelector);
        const nextButton = carousel.querySelector(config.nextSelector);

        if (!track) return;

        function getGap() {
            const styles = window.getComputedStyle(track);
            return parseFloat(styles.columnGap || styles.gap || 0);
        }

        function getItems() {
            return Array.from(track.querySelectorAll(config.itemSelector));
        }

        function getScrollAmount() {
            const firstItem = track.querySelector(config.itemSelector);

            if (!firstItem) {
                return track.clientWidth * 0.85;
            }

            return firstItem.getBoundingClientRect().width + getGap();
        }

        function getMaxScrollLeft() {
            return Math.max(track.scrollWidth - track.clientWidth, 0);
        }

        function isAtStart() {
            return track.scrollLeft <= 4;
        }

        function isAtEnd() {
            return track.scrollLeft >= getMaxScrollLeft() - 4;
        }

        function scrollToStart() {
            track.scrollTo({
                left: 0,
                behavior: "smooth"
            });
        }

        function scrollToEnd() {
            track.scrollTo({
                left: getMaxScrollLeft(),
                behavior: "smooth"
            });
        }

        function updateButtons() {
            if (!prevButton || !nextButton) return;

            if (config.loop) {
                prevButton.disabled = false;
                nextButton.disabled = false;
                return;
            }

            prevButton.disabled = isAtStart();
            nextButton.disabled = isAtEnd();
        }

        if (prevButton) {
            prevButton.addEventListener("click", function () {
                if (config.loop && isAtStart()) {
                    scrollToEnd();
                    return;
                }

                track.scrollBy({
                    left: -getScrollAmount(),
                    behavior: "smooth"
                });
            });
        }

        if (nextButton) {
            nextButton.addEventListener("click", function () {
                if (config.loop && isAtEnd()) {
                    scrollToStart();
                    return;
                }

                track.scrollBy({
                    left: getScrollAmount(),
                    behavior: "smooth"
                });
            });
        }

        track.addEventListener("scroll", updateButtons, { passive: true });
        window.addEventListener("resize", updateButtons);

        updateButtons();
    });
}