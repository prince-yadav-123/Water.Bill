(function () {
    const loader = document.getElementById("wbAppLoader");
    if (!loader) return;

    let pendingCount = 0;
    let showTimer = 0;
    let shownAt = 0;
    const showDelay = 120;
    const minVisible = 260;

    function show() {
        pendingCount += 1;
        window.clearTimeout(showTimer);
        showTimer = window.setTimeout(() => {
            shownAt = Date.now();
            document.body.classList.add("wb-loading");
            loader.classList.add("show");
            loader.setAttribute("aria-hidden", "false");
        }, showDelay);
    }

    function hide(force) {
        pendingCount = force ? 0 : Math.max(0, pendingCount - 1);
        if (pendingCount > 0) return;

        window.clearTimeout(showTimer);
        const elapsed = Date.now() - shownAt;
        const wait = loader.classList.contains("show") ? Math.max(0, minVisible - elapsed) : 0;

        window.setTimeout(() => {
            if (pendingCount > 0) return;
            document.body.classList.remove("wb-loading");
            loader.classList.remove("show");
            loader.setAttribute("aria-hidden", "true");
        }, wait);
    }

    window.WaterBillLoader = { show, hide: () => hide(false), hideAll: () => hide(true) };

    document.addEventListener("submit", (event) => {
        const form = event.target;
        if (!(form instanceof HTMLFormElement) || form.matches("[data-no-loader]")) return;

        window.setTimeout(() => {
            if (!event.defaultPrevented && form.checkValidity()) {
                show();
            }
        }, 0);
    });

    document.addEventListener("click", (event) => {
        const link = event.target.closest("a[href]");
        if (!link || link.matches("[data-no-loader]")) return;
        if (event.defaultPrevented || event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) return;
        if (link.target && link.target !== "_self") return;
        if (link.hasAttribute("download")) return;

        const href = link.getAttribute("href") || "";
        if (!href || href.startsWith("#") || href.startsWith("javascript:")) return;

        const url = new URL(href, window.location.href);
        if (url.origin !== window.location.origin) return;
        if (url.pathname === window.location.pathname && url.hash) return;

        show();
    });

    if (window.fetch) {
        const originalFetch = window.fetch.bind(window);
        window.fetch = function () {
            show();
            return originalFetch.apply(window, arguments).finally(() => hide(false));
        };
    }

    window.addEventListener("pageshow", () => hide(true));
    window.addEventListener("load", () => hide(true));
})();

(function () {
    const body = document.body;
    const collapseKey = body.dataset.sidebarStorageKey || "water-bill-admin-sidebar-collapsed";
    const mobileQuery = window.matchMedia("(max-width: 991.98px)");

    function isMobile() {
        return mobileQuery.matches;
    }

    function setCollapsed(collapsed) {
        if (isMobile()) {
            body.classList.remove("wb-sidebar-collapsed");
            return;
        }

        body.classList.toggle("wb-sidebar-collapsed", collapsed);
        localStorage.setItem(collapseKey, collapsed ? "true" : "false");
    }

    if (!isMobile() && localStorage.getItem(collapseKey) === "true") {
        body.classList.add("wb-sidebar-collapsed");
    }

    document.querySelectorAll("[data-menu-toggle]").forEach((button) => {
        button.addEventListener("click", () => {
            if (body.classList.contains("wb-sidebar-collapsed") && !isMobile()) {
                setCollapsed(false);
            }

            const group = button.closest(".wb-nav-group");
            if (!group) return;

            const isOpen = group.classList.contains("open");
            document.querySelectorAll(".wb-nav-group.open").forEach((openGroup) => {
                if (openGroup !== group) {
                    openGroup.classList.remove("open");
                    openGroup.querySelector("[data-menu-toggle]")?.setAttribute("aria-expanded", "false");
                }
            });

            group.classList.toggle("open", !isOpen);
            button.setAttribute("aria-expanded", String(!isOpen));
        });
    });

    document.querySelectorAll("[data-sidebar-toggle]").forEach((button) => {
        button.addEventListener("click", () => {
            if (isMobile()) {
                body.classList.toggle("wb-sidebar-open");
                return;
            }

            setCollapsed(!body.classList.contains("wb-sidebar-collapsed"));
        });
    });

    document.querySelector("[data-sidebar-dismiss]")?.addEventListener("click", () => {
        body.classList.remove("wb-sidebar-open");
    });

    document.querySelectorAll(".wb-nav-child, .wb-sidebar-bottom .wb-nav-link").forEach((link) => {
        link.addEventListener("click", () => body.classList.remove("wb-sidebar-open"));
    });

    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape") {
            body.classList.remove("wb-sidebar-open");
        }
    });

    mobileQuery.addEventListener("change", () => {
        body.classList.remove("wb-sidebar-open");
        if (isMobile()) {
            body.classList.remove("wb-sidebar-collapsed");
        } else if (localStorage.getItem(collapseKey) === "true") {
            body.classList.add("wb-sidebar-collapsed");
        }
    });
})();
