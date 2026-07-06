// FTT theme preference (matches ConeCalc / FCAControls.Wpf ThemeType: Dark | Light)
window.theme = {
    storageKey: 'aegis-theme',

    get() {
        try {
            const stored = localStorage.getItem(this.storageKey);
            if (stored === 'light' || stored === 'dark') {
                return stored;
            }
        } catch {
            // localStorage unavailable
        }
        return 'dark';
    },

    apply(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        try {
            localStorage.setItem(this.storageKey, theme);
        } catch {
            // localStorage unavailable
        }
    },

    isDark() {
        return this.get() === 'dark';
    },

    set(theme) {
        if (theme !== 'light' && theme !== 'dark') {
            return this.isDark();
        }
        this.apply(theme);
        return theme === 'dark';
    },

    toggle() {
        return this.set(this.isDark() ? 'light' : 'dark');
    },

    init() {
        this.apply(this.get());
    }
};

window.theme.init();

window.aegisGetTheme = function () {
    return window.theme.get();
};

window.aegisSetTheme = function (theme) {
    window.theme.set(theme);
};
