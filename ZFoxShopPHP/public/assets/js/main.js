document.addEventListener('DOMContentLoaded', () => {
    // Add minor fade-in to cards dynamically if not set
    const cards = document.querySelectorAll('.card');
    cards.forEach((card, index) => {
        card.style.animationDelay = `${index * 0.1}s`;
        if(!card.classList.contains('fade-in')) {
            card.classList.add('fade-in');
        }
    });
});
