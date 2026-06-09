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
    const stacks = Array.from(document.querySelectorAll(".wb-toast-stack"));
    if (!stacks.length) return;

    const primaryStack = stacks[0];
    stacks.slice(1).forEach((stack) => {
        stack.querySelectorAll(".wb-toast-message").forEach((toast) => primaryStack.appendChild(toast));
        stack.remove();
    });

    primaryStack.querySelectorAll(".wb-toast-message").forEach((toast) => {
        window.setTimeout(() => {
            toast.classList.add("hide");
            window.setTimeout(() => {
                toast.remove();
                if (!primaryStack.querySelector(".wb-toast-message")) {
                    primaryStack.remove();
                }
            }, 260);
        }, 4200);
    });
})();

(function () {
    document.querySelectorAll("[data-export-table]").forEach((button) => {
        button.addEventListener("click", () => {
            const table = document.querySelector(button.dataset.exportTable);
            if (!table) return;

            const rows = Array.from(table.querySelectorAll("tr"));
            const csv = rows.map((row) => {
                const cells = Array.from(row.querySelectorAll("th,td"));
                return cells.map((cell) => {
                    const value = (cell.innerText || "").replace(/\s+/g, " ").trim().replace(/"/g, '""');
                    return `"${value}"`;
                }).join(",");
            }).join("\n");

            const blob = new Blob([csv], { type: "text/csv;charset=utf-8" });
            const url = URL.createObjectURL(blob);
            const link = document.createElement("a");
            link.href = url;
            link.download = button.dataset.exportName || "export.csv";
            document.body.appendChild(link);
            link.click();
            link.remove();
            URL.revokeObjectURL(url);
        });
    });
})();

(function () {
    const body = document.body;
    const collapseKey = body.dataset.sidebarStorageKey || "water-bill-sidebar-collapsed";
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

    const loginForm = document.querySelector("[data-consumer-login-form]");
    if (!loginForm) return;

    const methodInput = loginForm.querySelector("[data-login-method]");
    const tabs = loginForm.querySelectorAll("[data-login-tab]");
    const panes = loginForm.querySelectorAll("[data-login-pane]");

    function setLoginMethod(method) {
        methodInput.value = method;

        tabs.forEach((tab) => {
            const isActive = tab.dataset.loginTab === method;
            tab.classList.toggle("active", isActive);
            tab.setAttribute("aria-selected", String(isActive));
            tab.tabIndex = isActive ? 0 : -1;
        });

        panes.forEach((pane) => {
            const isActive = pane.dataset.loginPane === method;
            pane.classList.toggle("active", isActive);
            pane.hidden = !isActive;
            pane.querySelectorAll("[data-login-input]").forEach((input) => {
                input.disabled = !isActive;
                if (!isActive) {
                    input.classList.remove("input-validation-error");
                }
            });
        });

        loginForm.querySelectorAll(".wb-client-error").forEach((error) => {
            error.textContent = "";
        });
    }

    function setFieldError(name, message) {
        const error = loginForm.querySelector(`[data-login-error-for="${name}"]`);
        if (error) error.textContent = message;
    }

    function validateSelectedMethod() {
        let isValid = true;
        const method = methodInput.value;

        loginForm.querySelectorAll(".wb-client-error").forEach((error) => {
            error.textContent = "";
        });

        if (method === "mobile-otp") {
            const input = loginForm.querySelector("[name='MobileNumber']");
            const digits = (input.value || "").replace(/\D/g, "");
            if (!digits) {
                setFieldError("MobileNumber", "Enter your mobile number.");
                isValid = false;
            } else if (digits.length !== 10) {
                setFieldError("MobileNumber", "Enter a valid 10 digit mobile number.");
                isValid = false;
            }
        }

        if (method === "consumer-id") {
            const input = loginForm.querySelector("[name='ConsumerId']");
            if (!input.value.trim()) {
                setFieldError("ConsumerId", "Enter your consumer ID.");
                isValid = false;
            }
        }

        if (method === "username-email") {
            const username = loginForm.querySelector("[name='UsernameOrEmail']");
            const password = loginForm.querySelector("[name='Password']");
            if (!username.value.trim()) {
                setFieldError("UsernameOrEmail", "Enter your username or email.");
                isValid = false;
            }
            if (!password.value.trim()) {
                setFieldError("Password", "Enter your password.");
                isValid = false;
            }
        }

        return isValid;
    }

    tabs.forEach((tab) => {
        tab.addEventListener("click", () => setLoginMethod(tab.dataset.loginTab));
    });

    loginForm.addEventListener("submit", (event) => {
        if (!validateSelectedMethod()) {
            event.preventDefault();
        }
    });

    setLoginMethod(methodInput.value || "mobile-otp");
})();
