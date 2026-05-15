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
