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
