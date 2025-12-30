function updateCountdowns() {
    document.querySelectorAll('.countdown').forEach(function (element) {
        const targetDate = new Date(element.getAttribute('data-target')).getTime();
        const now = new Date().getTime();
        const distance = targetDate - now;

        if (distance < 0) {
            element.innerHTML = "SULAMA ZAMANI!";
            return;
        }

        const hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
        const minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
        const seconds = Math.floor((distance % (1000 * 60)) / 1000);

        element.innerHTML = hours + "s " + minutes + "d " + seconds + "s ";
    });
}

setInterval(updateCountdowns, 1000);
updateCountdowns();