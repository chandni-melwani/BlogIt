window.themeInterop = {
    setDarkMode: function (isDark) {
        document.documentElement.classList.toggle('dark-theme', isDark);
        localStorage.setItem('blogit-dark-mode', isDark ? '1' : '0');
    },
    getDarkMode: function () {
        return localStorage.getItem('blogit-dark-mode') === '1';
    }
};
