/* global React, ReactDOM, BrandMark */
const { useState, useEffect } = React;

const DEFAULTS = /*EDITMODE-BEGIN*/{
  "theme": "light",
  "palette": "aqua",
  "density": "spacious",
  "mobileNav": "bottom",
  "heroStyle": "gradient",
  "a11yLarge": false,
  "a11yHC": false
}/*EDITMODE-END*/;

const NAV = [
  { k: "dashboard", t: "Dashboard", i: "th-large" },
  { k: "pay", t: "Pay Bill", i: "bolt", badge: "Due" },
  { k: "history", t: "Bill History", i: "file-invoice" },
  { k: "analytics", t: "Usage Analytics", i: "chart-line" },
  { k: "newConnection", t: "New Connection", i: "plus-circle" },
  { k: "complaint", t: "Complaints", i: "exclamation-triangle" },
];
const NAV2 = [
  { k: "profile", t: "Profile & Connections", i: "user-circle" },
  { k: "notifications", t: "Notifications", i: "bell", badge: 3 },
  { k: "help", t: "Help & Support", i: "life-ring" },
];

function App() {
  const [tweaks, setTweaks] = window.useTweaks(DEFAULTS);
  const [route, setRoute] = useState("login"); // start at login
  const [loggedIn, setLoggedIn] = useState(false);
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [isMobile, setIsMobile] = useState(window.innerWidth < 980);

  useEffect(() => {
    const fn = () => setIsMobile(window.innerWidth < 980);
    window.addEventListener("resize", fn);
    return () => window.removeEventListener("resize", fn);
  }, []);

  useEffect(() => {
    document.documentElement.dataset.theme = tweaks.theme;
    document.documentElement.dataset.palette = tweaks.palette;
    document.documentElement.dataset.density = tweaks.density;
    document.body.classList.toggle("a11y-large", !!tweaks.a11yLarge);
    document.body.classList.toggle("a11y-hc", !!tweaks.a11yHC);
  }, [tweaks]);

  const onLogin = () => { setLoggedIn(true); setRoute("dashboard"); };
  const goTo = (r) => { setRoute(r); setDrawerOpen(false); window.scrollTo(0, 0); };

  if (!loggedIn || route === "login") {
    return (
      <>
        <LoginScreen onLogin={onLogin}/>
        <Tweaks tweaks={tweaks} setTweaks={setTweaks}/>
      </>
    );
  }

  const renderRoute = () => {
    switch (route) {
      case "dashboard": return <Dashboard heroStyle={tweaks.heroStyle} density={tweaks.density} onPay={() => goTo("pay")} onNav={goTo}/>;
      case "pay": return <PayBill onComplete={() => goTo("receipt")} onCancel={() => goTo("dashboard")}/>;
      case "receipt": return <Receipt onClose={() => goTo("dashboard")} onNav={goTo}/>;
      case "history": return <BillHistory onPay={() => goTo("pay")}/>;
      case "analytics": return <Analytics/>;
      case "newConnection": return <NewConnection onCancel={() => goTo("dashboard")}/>;
      case "complaint": return <Complaint onCancel={() => goTo("dashboard")}/>;
      case "profile": return <Profile/>;
      case "notifications": return <Notifications onNav={goTo}/>;
      case "help": return <Help/>;
      default: return null;
    }
  };

  const navMode = isMobile ? tweaks.mobileNav : "sidebar";
  const topbarTitle = (
    [...NAV, ...NAV2].find(n => n.k === route)?.t || "NoidaJal"
  );

  return (
    <div className="app" data-nav={navMode === "bottom" ? "bottom" : navMode === "drawer" ? "mobile" : "desktop"}>
      <Sidebar route={route} goTo={goTo}/>

      {drawerOpen && (
        <>
          <div className="mobile-drawer-mask" onClick={() => setDrawerOpen(false)}/>
          <div className="mobile-drawer">
            <Sidebar route={route} goTo={goTo} forceVisible/>
          </div>
        </>
      )}

      <div className="main">
        <div className="topbar">
          {isMobile && navMode === "drawer" && (
            <button className="icon-btn" onClick={() => setDrawerOpen(true)}><i className="fa fa-bars"></i></button>
          )}
          {!isMobile && <div className="topbar-title">{topbarTitle}</div>}
          {isMobile && (
            <div style={{ display: "flex", alignItems: "center", gap: 10, flex: 1 }}>
              <BrandMark size={32}/>
              <div style={{ lineHeight: 1.1 }}>
                <div style={{ fontSize: 14, fontWeight: 700 }}>NoidaJal</div>
                <div style={{ fontSize: 10, color: "var(--ink3)", textTransform: "uppercase", letterSpacing: ".08em" }}>{topbarTitle}</div>
              </div>
            </div>
          )}
          <div className="topbar-search">
            <i className="fa fa-search"></i>
            <input placeholder="Search bills, connections, help..."/>
            <kbd style={{ background: "var(--white)", border: "1px solid var(--border)", borderRadius: 6, padding: "2px 6px", fontSize: 11, color: "var(--ink3)", fontFamily: "var(--mono)" }}>⌘K</kbd>
          </div>
          <div className="topbar-actions">
            <button className="icon-btn" title="Switch to Hindi"><span style={{ fontSize: 13, fontWeight: 700 }}>हिं</span></button>
            <button className="icon-btn" title="Notifications" onClick={() => goTo("notifications")}>
              <i className="fa fa-bell"></i>
              <span className="dot"></span>
            </button>
            <button className="icon-btn" title="Toggle theme" onClick={() => setTweaks("theme", tweaks.theme === "light" ? "dark" : "light")}>
              <i className={`fa fa-${tweaks.theme === "light" ? "moon" : "sun"}`}></i>
            </button>
            <div className="avatar" onClick={() => goTo("profile")}>
              <div className="avatar-img">AS</div>
              <div className="avatar-info">
                <div className="avatar-name">Aarav Sharma</div>
                <div className="avatar-sub">NJL-20A-184721</div>
              </div>
            </div>
          </div>
        </div>

        {renderRoute()}
      </div>

      <div className="bottom-nav">
        <button className={`bnav-item ${route === "dashboard" ? "active" : ""}`} onClick={() => goTo("dashboard")}><i className="fa fa-th-large"></i>Home</button>
        <button className={`bnav-item ${route === "history" ? "active" : ""}`} onClick={() => goTo("history")}><i className="fa fa-file-invoice"></i>Bills</button>
        <button className="bnav-item center" onClick={() => goTo("pay")}><div className="pay-fab"><i className="fa fa-bolt"></i></div><span style={{ marginTop: 2 }}>Pay</span></button>
        <button className={`bnav-item ${route === "analytics" ? "active" : ""}`} onClick={() => goTo("analytics")}><i className="fa fa-chart-line"></i>Usage</button>
        <button className={`bnav-item ${route === "profile" ? "active" : ""}`} onClick={() => goTo("profile")}><i className="fa fa-user"></i>You</button>
      </div>

      <Tweaks tweaks={tweaks} setTweaks={setTweaks}/>
    </div>
  );
}

function Sidebar({ route, goTo, forceVisible }) {
  return (
    <aside className="sidebar" style={forceVisible ? { display: "flex", position: "static", height: "100%", width: "100%" } : null}>
      <div className="brand">
        <BrandMark size={40}/>
        <div className="brand-text">
          <div className="brand-name">NoidaJal</div>
          <div className="brand-sub">Water Bill System</div>
        </div>
      </div>

      <div className="nav-section">
        <div className="nav-section-label">Main</div>
        {NAV.map(n => (
          <button key={n.k} className={`nav-item ${route === n.k ? "active" : ""}`} onClick={() => goTo(n.k)}>
            <i className={`fa fa-${n.i}`}></i>
            <span>{n.t}</span>
            {n.badge && <span className="badge-dot" style={{ background: "var(--orange)" }}>{n.badge}</span>}
          </button>
        ))}
      </div>

      <div className="nav-section" style={{ borderTop: "1px solid var(--border)" }}>
        <div className="nav-section-label">Account</div>
        {NAV2.map(n => (
          <button key={n.k} className={`nav-item ${route === n.k ? "active" : ""}`} onClick={() => goTo(n.k)}>
            <i className={`fa fa-${n.i}`}></i>
            <span>{n.t}</span>
            {n.badge && <span className="badge-dot">{n.badge}</span>}
          </button>
        ))}
      </div>

      <div className="sidebar-footer">
        <div className="sidebar-card">
          <div style={{ display: "flex", alignItems: "center", gap: 8, marginBottom: 8 }}>
            <i className="fa fa-headset" style={{ color: "var(--brand)" }}></i>
            <div className="sidebar-card-title">24×7 helpline</div>
          </div>
          <div className="sidebar-card-sub">
            <span className="mono" style={{ color: "var(--ink2)", fontWeight: 600 }}>1800 200 3344</span><br/>
            Toll-free · Hindi + English
          </div>
        </div>
      </div>
    </aside>
  );
}

// Tweaks panel
function Tweaks({ tweaks, setTweaks }) {
  const { TweaksPanel, TweakSection, TweakRadio, TweakColor, TweakToggle, TweakSelect } = window;
  return (
    <TweaksPanel title="Tweaks">
      <TweakSection title="Appearance">
        <TweakRadio label="Theme" value={tweaks.theme} onChange={v => setTweaks("theme", v)} options={[
          { value: "light", label: "Light" },
          { value: "dark", label: "Dark" },
        ]}/>
        <TweakRadio label="Palette" value={tweaks.palette} onChange={v => setTweaks("palette", v)} options={[
          { value: "aqua", label: "Aqua" },
          { value: "civic", label: "Civic" },
          { value: "authority", label: "Navy" },
        ]}/>
        <TweakRadio label="Density" value={tweaks.density} onChange={v => setTweaks("density", v)} options={[
          { value: "spacious", label: "Spacious" },
          { value: "compact", label: "Compact" },
        ]}/>
        <TweakRadio label="Hero card" value={tweaks.heroStyle} onChange={v => setTweaks("heroStyle", v)} options={[
          { value: "gradient", label: "Gradient" },
          { value: "flat", label: "Flat" },
          { value: "illustrated", label: "Waves" },
        ]}/>
      </TweakSection>
      <TweakSection title="Mobile">
        <TweakRadio label="Navigation" value={tweaks.mobileNav} onChange={v => setTweaks("mobileNav", v)} options={[
          { value: "bottom", label: "Bottom nav" },
          { value: "drawer", label: "Hamburger" },
        ]}/>
      </TweakSection>
      <TweakSection title="Accessibility">
        <TweakToggle label="Larger text" value={tweaks.a11yLarge} onChange={v => setTweaks("a11yLarge", v)}/>
        <TweakToggle label="High contrast" value={tweaks.a11yHC} onChange={v => setTweaks("a11yHC", v)}/>
      </TweakSection>
    </TweaksPanel>
  );
}

ReactDOM.createRoot(document.getElementById("root")).render(<App/>);
