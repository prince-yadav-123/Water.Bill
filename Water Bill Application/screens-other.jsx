/* global React, Badge, PageHeader */
const { useState } = React;

// ─── Profile + Connections ────────────────────
const Profile = () => {
  const [tab, setTab] = useState("connections");
  return (
    <div className="page fade-in">
      <PageHeader title="Profile & connections" subtitle="Manage your account, properties, and preferences."/>

      <div className="card" style={{ marginBottom: 20, overflow: "visible" }}>
        <div style={{ padding: 22, display: "flex", alignItems: "center", gap: 18 }}>
          <div style={{ width: 72, height: 72, borderRadius: "50%", background: "var(--hero-grad)", color: "#fff", display: "grid", placeItems: "center", fontSize: 28, fontWeight: 700 }}>AS</div>
          <div style={{ flex: 1 }}>
            <div style={{ fontSize: 20, fontWeight: 700 }}>Aarav Sharma</div>
            <div style={{ fontSize: 13, color: "var(--ink3)", marginTop: 2 }}>
              <span className="mono">+91 98765 43210</span> · aarav.sharma@gmail.com
            </div>
            <div style={{ display: "flex", gap: 8, marginTop: 10, flexWrap: "wrap" }}>
              <Badge tone="green" icon="check-circle">Mobile verified</Badge>
              <Badge tone="green" icon="check-circle">Email verified</Badge>
              <Badge tone="blue" icon="star">Loyal customer · 4 yrs</Badge>
            </div>
          </div>
          <button className="btn btn-outline"><i className="fa fa-pen"></i> Edit profile</button>
        </div>
      </div>

      <div className="tab-segments" style={{ width: "fit-content", marginBottom: 20 }}>
        {[
          { k: "connections", t: "My connections" },
          { k: "payment", t: "Payment methods" },
          { k: "prefs", t: "Preferences" },
          { k: "security", t: "Security" },
        ].map(t => (
          <div key={t.k} className={`tab-seg ${tab === t.k ? "active" : ""}`} onClick={() => setTab(t.k)} style={{ padding: "10px 16px" }}>{t.t}</div>
        ))}
      </div>

      {tab === "connections" && (
        <>
          <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 16, marginBottom: 16 }}>
            {[
              { primary: true, name: "Sector 50 · A-123", addr: "Block A, House 123, Sector 50, Noida 201301", id: "NJL-20A-184721", type: "Domestic", since: "Aug 2022", lastBill: "₹1,847 · due 15 May", status: "active", icon: "home" },
              { primary: false, name: "Sector 18 · Shop S-7", addr: "Shop S-7, Brahmaputra Market, Sector 18, Noida 201301", id: "NJL-18C-329106", type: "Commercial", since: "Jan 2024", lastBill: "₹3,420 · paid 28 Apr", status: "active", icon: "store" },
            ].map(c => (
              <div key={c.id} className="card">
                <div style={{ padding: 18 }}>
                  <div style={{ display: "flex", alignItems: "center", gap: 12 }}>
                    <div style={{ width: 44, height: 44, borderRadius: 12, background: "var(--brand-bg)", color: "var(--brand)", display: "grid", placeItems: "center", fontSize: 18 }}>
                      <i className={`fa fa-${c.icon}`}></i>
                    </div>
                    <div style={{ flex: 1 }}>
                      <div style={{ fontWeight: 700, fontSize: 15 }}>{c.name}</div>
                      <div className="mono" style={{ fontSize: 11.5, color: "var(--ink3)" }}>{c.id}</div>
                    </div>
                    {c.primary && <Badge tone="blue" icon="star">Primary</Badge>}
                  </div>
                  <div style={{ marginTop: 14, fontSize: 13, color: "var(--ink2)" }}>{c.addr}</div>

                  <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr", gap: 12, marginTop: 16, paddingTop: 14, borderTop: "1px solid var(--border)" }}>
                    <div>
                      <div style={{ fontSize: 10, color: "var(--ink3)", textTransform: "uppercase", fontWeight: 700, letterSpacing: ".08em" }}>Type</div>
                      <div style={{ fontSize: 13, fontWeight: 600, marginTop: 2 }}>{c.type}</div>
                    </div>
                    <div>
                      <div style={{ fontSize: 10, color: "var(--ink3)", textTransform: "uppercase", fontWeight: 700, letterSpacing: ".08em" }}>Since</div>
                      <div style={{ fontSize: 13, fontWeight: 600, marginTop: 2 }}>{c.since}</div>
                    </div>
                    <div>
                      <div style={{ fontSize: 10, color: "var(--ink3)", textTransform: "uppercase", fontWeight: 700, letterSpacing: ".08em" }}>Status</div>
                      <div style={{ marginTop: 2 }}><Badge tone="green" icon="circle">Active</Badge></div>
                    </div>
                  </div>
                  <div style={{ marginTop: 12, padding: 12, background: "var(--surface)", borderRadius: 10, fontSize: 12.5, color: "var(--ink2)" }}>
                    Latest: {c.lastBill}
                  </div>

                  <div style={{ display: "flex", gap: 8, marginTop: 14 }}>
                    <button className="btn btn-outline" style={{ flex: 1 }}><i className="fa fa-chart-line"></i> View usage</button>
                    <button className="btn btn-ghost"><i className="fa fa-cog"></i></button>
                  </div>
                </div>
              </div>
            ))}
            <button className="card" style={{ padding: 18, border: "2px dashed var(--border2)", background: "transparent", display: "grid", placeItems: "center", textAlign: "center", minHeight: 260, cursor: "pointer" }}>
              <div>
                <div style={{ width: 48, height: 48, borderRadius: "50%", background: "var(--brand-bg)", color: "var(--brand)", display: "grid", placeItems: "center", margin: "0 auto", fontSize: 20 }}>
                  <i className="fa fa-plus"></i>
                </div>
                <div style={{ marginTop: 14, fontWeight: 600, color: "var(--ink)" }}>Link another connection</div>
                <div style={{ fontSize: 12, color: "var(--ink3)", marginTop: 4 }}>Add a new or existing meter</div>
              </div>
            </button>
          </div>
        </>
      )}

      {tab === "payment" && (
        <div className="card">
          <div className="card-header"><div className="card-title">Saved payment methods</div><button className="btn btn-primary"><i className="fa fa-plus"></i> Add new</button></div>
          <div style={{ padding: 0 }}>
            {[
              { i: "qrcode", t: "UPI · aarav@hdfcbank", s: "Default · linked to HDFC Savings", primary: true },
              { i: "credit-card", t: "HDFC Credit Card ••3412", s: "Expires 09/28 · auto-pay enabled" },
              { i: "university", t: "Net Banking · SBI", s: "Used Jan 2026" },
            ].map(p => (
              <div key={p.t} style={{ display: "flex", alignItems: "center", gap: 14, padding: "16px 22px", borderBottom: "1px solid var(--border)" }}>
                <div style={{ width: 42, height: 42, borderRadius: 10, background: "var(--surface)", color: "var(--ink2)", display: "grid", placeItems: "center", fontSize: 16 }}><i className={`fa fa-${p.i}`}></i></div>
                <div style={{ flex: 1 }}>
                  <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
                    <span style={{ fontWeight: 600, fontSize: 13.5 }}>{p.t}</span>
                    {p.primary && <Badge tone="blue">Default</Badge>}
                  </div>
                  <div style={{ fontSize: 12, color: "var(--ink3)", marginTop: 2 }}>{p.s}</div>
                </div>
                <button className="btn btn-ghost"><i className="fa fa-ellipsis-h"></i></button>
              </div>
            ))}
          </div>
        </div>
      )}

      {tab === "prefs" && (
        <div className="card">
          <div className="card-body">
            <div style={{ marginBottom: 18, fontWeight: 600, color: "var(--ink2)", textTransform: "uppercase", fontSize: 11, letterSpacing: ".08em" }}>Notifications</div>
            {[
              { t: "Bill ready for payment", e: true, s: true, w: true },
              { t: "Due-date reminders (3 days, 1 day)", e: true, s: true, w: false },
              { t: "Successful payment receipt", e: true, s: true, w: true },
              { t: "Possible leak / anomaly alerts", e: true, s: true, w: true },
              { t: "Outage & maintenance notices", e: false, s: true, w: false },
              { t: "Promotional offers", e: false, s: false, w: false },
            ].map(p => (
              <div key={p.t} style={{ display: "grid", gridTemplateColumns: "1fr 80px 80px 80px", alignItems: "center", padding: "12px 0", borderBottom: "1px solid var(--border)" }}>
                <div style={{ fontSize: 13.5 }}>{p.t}</div>
                <Toggle on={p.e}/> <Toggle on={p.s}/> <Toggle on={p.w}/>
              </div>
            ))}
            <div style={{ display: "grid", gridTemplateColumns: "1fr 80px 80px 80px", marginTop: 6, fontSize: 11, color: "var(--ink3)", fontWeight: 600, textTransform: "uppercase", letterSpacing: ".08em" }}>
              <div></div><div>Email</div><div>SMS</div><div>WhatsApp</div>
            </div>
          </div>
        </div>
      )}

      {tab === "security" && (
        <div className="card">
          <div className="card-body" style={{ padding: 0 }}>
            {[
              { i: "key", t: "Change password", s: "Last changed 3 months ago" },
              { i: "mobile-alt", t: "Two-factor authentication", s: "OTP on every login", on: true },
              { i: "fingerprint", t: "Biometric login", s: "Use Face ID / Fingerprint on this device", on: false },
              { i: "history", t: "Active sessions", s: "3 devices · last seen 11 May" },
              { i: "download", t: "Download my data", s: "Export profile, bills, complaints" },
              { i: "user-times", t: "Close account", s: "Permanently remove all data", danger: true },
            ].map(x => (
              <div key={x.t} style={{ display: "flex", alignItems: "center", gap: 14, padding: "16px 22px", borderBottom: "1px solid var(--border)" }}>
                <div style={{ width: 38, height: 38, borderRadius: 10, background: x.danger ? "var(--red-bg)" : "var(--surface)", color: x.danger ? "var(--red)" : "var(--ink2)", display: "grid", placeItems: "center" }}><i className={`fa fa-${x.i}`}></i></div>
                <div style={{ flex: 1 }}>
                  <div style={{ fontWeight: 600, fontSize: 13.5, color: x.danger ? "var(--red)" : "var(--ink)" }}>{x.t}</div>
                  <div style={{ fontSize: 12, color: "var(--ink3)" }}>{x.s}</div>
                </div>
                {typeof x.on === "boolean" ? <Toggle on={x.on}/> : <i className="fa fa-chevron-right" style={{ color: "var(--ink3)" }}></i>}
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};

const Toggle = ({ on: initial = false }) => {
  const [on, setOn] = useState(initial);
  return (
    <button onClick={() => setOn(!on)} style={{
      width: 42, height: 24, borderRadius: 999,
      background: on ? "var(--brand)" : "var(--border2)",
      position: "relative", transition: "var(--transition)",
      cursor: "pointer",
    }}>
      <span style={{
        position: "absolute", top: 3, left: on ? 21 : 3,
        width: 18, height: 18, borderRadius: "50%", background: "#fff",
        transition: "var(--transition)",
        boxShadow: "0 1px 3px rgba(0,0,0,.2)"
      }}/>
    </button>
  );
};

// ─── Notifications ─────────────────────────────
const Notifications = ({ onNav }) => {
  const items = [
    { i: "file-invoice", c: "orange", t: "March 2026 bill is ready", s: "₹1,847 due by 15 May 2026 · for A-123 Sector 50", time: "2 hrs ago", unread: true, action: "Pay now" },
    { i: "exclamation-triangle", c: "red", t: "Possible leak detected", s: "Overnight flow on 8 Apr was 2.4× your baseline", time: "Yesterday", unread: true, action: "Investigate" },
    { i: "check-circle", c: "green", t: "Complaint NJT-26041-0421 update", s: "Technician will visit between 11 AM – 1 PM today", time: "Yesterday", unread: true },
    { i: "tint-slash", c: "blue", t: "Scheduled supply interruption", s: "Sector 50: 14 May, 9 AM – 1 PM for pipeline maintenance", time: "2 days ago" },
    { i: "check", c: "green", t: "Payment confirmed for Feb 2026 bill", s: "₹1,654 via UPI · HDFC" , time: "12 Mar"},
    { i: "trophy", c: "purple", t: "You saved 6% water in March", s: "Top 22% of Sector 50 households", time: "01 Apr"},
  ];

  return (
    <div className="page fade-in">
      <PageHeader title="Notifications" subtitle="All updates from NoidaJal in one place."
        action={
          <div style={{ display: "flex", gap: 10 }}>
            <button className="btn btn-outline"><i className="fa fa-check-double"></i> Mark all read</button>
            <button className="btn btn-outline"><i className="fa fa-cog"></i> Preferences</button>
          </div>
        }
      />

      <div className="tab-segments" style={{ width: "fit-content", marginBottom: 16 }}>
        <div className="tab-seg active">All <span className="badge b-red" style={{ marginLeft: 6, padding: "1px 6px" }}>3</span></div>
        <div className="tab-seg">Bills</div>
        <div className="tab-seg">Service</div>
        <div className="tab-seg">Alerts</div>
      </div>

      <div className="card">
        {items.map((n, i) => (
          <div key={i} style={{ display: "flex", gap: 14, padding: "16px 22px", borderBottom: i < items.length - 1 ? "1px solid var(--border)" : "none", background: n.unread ? "var(--brand-bg)" : "transparent", position: "relative" }}>
            {n.unread && <span style={{ position: "absolute", left: 8, top: "50%", width: 6, height: 6, borderRadius: "50%", background: "var(--brand)" }}/>}
            <div style={{ width: 40, height: 40, borderRadius: 12, background: `var(--${n.c}-bg)`, color: `var(--${n.c})`, display: "grid", placeItems: "center", flexShrink: 0 }}><i className={`fa fa-${n.i}`}></i></div>
            <div style={{ flex: 1 }}>
              <div style={{ display: "flex", justifyContent: "space-between", gap: 10 }}>
                <div style={{ fontSize: 14, fontWeight: 600, color: "var(--ink)" }}>{n.t}</div>
                <div style={{ fontSize: 11.5, color: "var(--ink3)", whiteSpace: "nowrap" }}>{n.time}</div>
              </div>
              <div style={{ fontSize: 12.5, color: "var(--ink2)", marginTop: 4 }}>{n.s}</div>
              {n.action && <button className="btn btn-outline" style={{ marginTop: 10, padding: "5px 12px", fontSize: 12 }} onClick={() => n.action === "Pay now" && onNav("pay")}>{n.action} <i className="fa fa-arrow-right" style={{ fontSize: 10 }}/></button>}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

// ─── Help & FAQs ──────────────────────────────
const Help = () => {
  const [open, setOpen] = useState(0);
  const faqs = [
    { q: "How is my water bill calculated?", a: "Your bill = (units consumed × applicable tariff) + sewerage charges + meter rent + applicable taxes. For domestic connections in Noida the tariff slabs are: ₹40/KL for the first 15 KL, ₹65/KL up to 30 KL, ₹90/KL beyond 30 KL." },
    { q: "When do I receive my bill?", a: "Bills are generated on the 1st of every month for the previous billing cycle and are pushed to your registered email, SMS, and WhatsApp within 24 hours. They are always available on this portal under 'Bill History'." },
    { q: "What are the accepted payment methods?", a: "UPI (all apps), Debit/Credit cards (Visa, Mastercard, RuPay), Net banking (60+ banks), and major wallets. There is no convenience fee for any online payment." },
    { q: "What happens if I miss the due date?", a: "A 2% late fee on the outstanding amount is added after 15 days. Connections with arrears beyond 60 days are subject to disconnection notice. You can pay partial amounts to avoid escalation." },
    { q: "How do I set up auto-pay?", a: "Go to Profile → Payment methods → 'Set up auto-pay'. Authorise a card or UPI mandate and bills will be paid automatically 3 days before the due date. You can cancel anytime." },
    { q: "I'm seeing higher consumption than usual — what should I do?", a: "First check the Usage Analytics screen for daily readings. Spikes after midnight often indicate hidden leaks. You can raise a 'Pipe leak' complaint and an inspector will visit within 24 hours." },
  ];

  return (
    <div className="page fade-in">
      <PageHeader title="Help & support" subtitle="Find answers fast or reach our 24×7 team."/>

      <div className="topbar-search" style={{ flex: "0", maxWidth: 600, background: "var(--white)", padding: "12px 16px", marginBottom: 22 }}>
        <i className="fa fa-search" style={{ fontSize: 16 }}></i>
        <input placeholder="Search help articles, e.g. 'how to read meter'" style={{ fontSize: 14 }}/>
      </div>

      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill,minmax(180px,1fr))", gap: 14, marginBottom: 28 }}>
        {[
          { i: "file-invoice", t: "Billing", n: 12 },
          { i: "credit-card", t: "Payments", n: 8 },
          { i: "tachometer-alt", t: "Meter & readings", n: 6 },
          { i: "plus-circle", t: "New connection", n: 9 },
          { i: "exclamation-triangle", t: "Complaints", n: 5 },
          { i: "user-shield", t: "Account & security", n: 7 },
        ].map(c => (
          <button key={c.t} className="card" style={{ padding: 18, textAlign: "left", cursor: "pointer" }}>
            <div style={{ width: 38, height: 38, borderRadius: 10, background: "var(--brand-bg)", color: "var(--brand)", display: "grid", placeItems: "center" }}><i className={`fa fa-${c.i}`}></i></div>
            <div style={{ fontSize: 14, fontWeight: 600, marginTop: 14 }}>{c.t}</div>
            <div style={{ fontSize: 12, color: "var(--ink3)", marginTop: 2 }}>{c.n} articles</div>
          </button>
        ))}
      </div>

      <div style={{ display: "grid", gridTemplateColumns: "1.5fr 1fr", gap: 20 }}>
        <div className="card">
          <div className="card-header"><div className="card-title">Frequently asked questions</div></div>
          <div style={{ padding: 0 }}>
            {faqs.map((f, i) => (
              <div key={i} style={{ borderBottom: i < faqs.length - 1 ? "1px solid var(--border)" : "none" }}>
                <button onClick={() => setOpen(open === i ? -1 : i)} style={{ width: "100%", textAlign: "left", padding: "16px 22px", display: "flex", alignItems: "center", justifyContent: "space-between", gap: 14, cursor: "pointer" }}>
                  <span style={{ fontSize: 14, fontWeight: 600, color: "var(--ink)" }}>{f.q}</span>
                  <i className={`fa fa-chevron-${open === i ? "up" : "down"}`} style={{ color: "var(--ink3)", fontSize: 12 }}></i>
                </button>
                {open === i && (
                  <div style={{ padding: "0 22px 18px", fontSize: 13.5, color: "var(--ink2)", lineHeight: 1.7 }}>{f.a}</div>
                )}
              </div>
            ))}
          </div>
        </div>

        <div style={{ display: "flex", flexDirection: "column", gap: 16 }}>
          <div className="card">
            <div className="card-header"><div className="card-title">Talk to us</div></div>
            <div className="card-body">
              {[
                { i: "headset", t: "Helpline", s: "1800 200 3344 · 24×7", c: "brand", tag: "Toll-free" },
                { i: "whatsapp", t: "WhatsApp support", s: "+91 88820 12345 · 8 AM – 8 PM", c: "green", tag: "Fastest" },
                { i: "envelope", t: "Email us", s: "support@noidajal.gov.in", c: "purple" },
                { i: "map-marker-alt", t: "Visit office", s: "Sector 6, Noida · Mon–Fri 10–5", c: "orange" },
              ].map(x => (
                <div key={x.t} style={{ display: "flex", alignItems: "center", gap: 12, padding: "10px 0", borderBottom: "1px solid var(--border)" }}>
                  <div style={{ width: 36, height: 36, borderRadius: 10, background: `var(--${x.c}-bg)`, color: `var(--${x.c})`, display: "grid", placeItems: "center" }}><i className={`fa${x.i === "whatsapp" ? "b" : ""} fa-${x.i}`}></i></div>
                  <div style={{ flex: 1 }}>
                    <div style={{ fontSize: 13.5, fontWeight: 600 }}>{x.t}</div>
                    <div style={{ fontSize: 12, color: "var(--ink3)" }}>{x.s}</div>
                  </div>
                  {x.tag && <Badge tone="green">{x.tag}</Badge>}
                </div>
              ))}
              <button className="btn btn-primary btn-block" style={{ marginTop: 14 }}><i className="fa fa-comments"></i> Chat with NoidaJal</button>
            </div>
          </div>

          <div className="card" style={{ background: "var(--hero-grad)", color: "#fff", border: "none" }}>
            <div style={{ padding: 22, position: "relative", overflow: "hidden" }}>
              <i className="fa fa-lightbulb" style={{ fontSize: 28, opacity: .85 }}></i>
              <div style={{ fontSize: 16, fontWeight: 700, marginTop: 14 }}>Citizen charter</div>
              <div style={{ fontSize: 13, opacity: .85, marginTop: 6, lineHeight: 1.6 }}>Read about your rights, response time guarantees, and refund policies.</div>
              <button className="btn" style={{ background: "#fff", color: "var(--brand-dk)", marginTop: 14 }}>Read charter <i className="fa fa-arrow-right"></i></button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

window.Profile = Profile;
window.Notifications = Notifications;
window.Help = Help;
