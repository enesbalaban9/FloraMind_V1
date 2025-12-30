function updateCountdowns() {
    const timers = document.querySelectorAll('.countdown');

    timers.forEach(timer => {
        const targetStr = timer.getAttribute('data-target');
        if (!targetStr) return;

        const targetDate = new Date(targetStr).getTime();
        const now = new Date().getTime();
        const distance = targetDate - now;

        if (distance < 0) {
            timer.innerHTML = "SULAMA VAKTİ!";
            timer.classList.add('text-danger');
            timer.classList.remove('text-primary');
        } else {
            const days = Math.floor(distance / (1000 * 60 * 60 * 24));
            const hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
            const minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
            const seconds = Math.floor((distance % (1000 * 60)) / 1000);

            timer.innerHTML = `${days}g ${hours}s ${minutes}d ${seconds}sn`;
        }
    });
}

document.addEventListener('DOMContentLoaded', () => {
    setInterval(updateCountdowns, 1000);
    updateCountdowns();
});