window.postViewInterop = {
    scrollToCommentsAndFocus: function () {
        const section = document.getElementById('comments');
        if (section) {
            section.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
        setTimeout(function () {
            const input = document.getElementById('comment-input');
            if (input) input.focus();
        }, 400);
    }
};
