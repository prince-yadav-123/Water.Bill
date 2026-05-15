// NoidaJal portal — small helpers for the Razor surface
(function () {
    function setTheme(t) {
        document.documentElement.dataset.theme = t;
        try { localStorage.setItem('njTheme', t); } catch (e) { }
        var icon = document.querySelector('.icon-btn .fa-moon, .icon-btn .fa-sun');
        if (icon) icon.className = 'fa fa-' + (t === 'dark' ? 'sun' : 'moon');
    }
    window.njToggleTheme = function () {
        var cur = document.documentElement.dataset.theme || 'light';
        setTheme(cur === 'light' ? 'dark' : 'light');
    };
    try {
        var saved = localStorage.getItem('njTheme');
        if (saved) setTheme(saved);
    } catch (e) { }

    // Pay-bill stepper
    window.njGoStep = function (n) {
        document.querySelectorAll('[data-step]').forEach(function (el) {
            el.style.display = (el.getAttribute('data-step') === String(n)) ? '' : 'none';
        });
        document.querySelectorAll('[data-stepind]').forEach(function (el) {
            var i = parseInt(el.getAttribute('data-stepind'), 10);
            el.classList.remove('active', 'done');
            if (i === n) el.classList.add('active');
            else if (i < n) el.classList.add('done');
        });
    };

    // Tabs (generic)
    window.njSwitchTab = function (group, key) {
        document.querySelectorAll('[data-tabgroup="' + group + '"]').forEach(function (el) {
            el.classList.toggle('active', el.getAttribute('data-tabkey') === key);
        });
        document.querySelectorAll('[data-tabpanel="' + group + '"]').forEach(function (el) {
            el.style.display = (el.getAttribute('data-tabkey') === key) ? '' : 'none';
        });
    };

    // FAQ accordion
    window.njToggleFaq = function (i) {
        var el = document.getElementById('faq-' + i);
        var ic = document.getElementById('faq-ic-' + i);
        if (!el) return;
        var open = el.style.display !== 'none';
        el.style.display = open ? 'none' : '';
        if (ic) ic.className = 'fa fa-chevron-' + (open ? 'down' : 'up');
    };
})();
