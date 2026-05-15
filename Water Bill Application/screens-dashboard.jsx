/* global React, BrandMark, WaveDeco, Badge, StatCard, UsageChart, Donut, ConnectionPill, PageHeader */
const { useState } = React;

// Sample data
const USAGE_12M = [
  { label: "Apr", v: 18.2 }, { label: "May", v: 22.4 }, { label: "Jun", v: 27.8 },
  { label: "Jul", v: 31.2 }, { label: "Aug", v: 28.5 }, { label: "Sep", v: 24.1 },
  { label: "Oct", v: 22.6 }, { label: "Nov", v: 19.8 }, { label: "Dec", v: 18.4 },
  { label: "Jan", v: 20.1 }, { label: "Feb", v: 22.0 }, { label: "Mar", v: 24.5 },
];

const BILLS = [
  { id: "NJB-202603-84721", period: "Mar 2026", amount: "1,847", units: 24.5, status: "due", due: "15 May 2026" },
  { id: "NJB-202602-71204", period: "Feb 2026", amount: "1,654", units: 22.0, status: "paid", paidOn: "12 Mar 2026", method: "UPI · HDFC" },
  { id: "NJB-202601-58931", period: "Jan 2026", amount: "1,512", units: 20.1, status: "paid", paidOn: "11 Feb 2026", method: "Net Banking · SBI" },
  { id: "NJB-202512-46217", period: "Dec 2025", amount: "1,384", units: 18.4, status: "paid", paidOn: "09 Jan 2026", method: "UPI · GPay" },
  { id: "NJB-202511-34108", period: "Nov 2025", amount: "1,489", units: 19.8, status: "paid", paidOn: "13 Dec 2025", method: "Debit Card" },
  { id: "NJB-202510-21997", period: "Oct 2025", amount: "1,698", units: 22.6, status: "paid", paidOn: "14 Nov 2025", method: "UPI · PhonePe" },
];

// ─── Dashboard ────────────────────────────────
const Dashboard = ({ heroStyle, density, onPay, onNav }) => {
  return (
    <div className="page fade-in">
      <PageHeader
        title="Good evening, Aarav"
        subtitle="Here's a quick look at your water connection."
        action={
          <div style={{ display: "flex", gap: 10, alignItems: "center" }}>
            <ConnectionPill conn={{ name: "Sector 50 · A-123", id: "NJL-20A-184721", icon: "home" }}/>
          </div>
        }
      />

      {/* HERO BILL CARD */}
      <div className="hero-card" data-style={heroStyle} style={{ marginBottom: 24 }}>
        <div className="blob"></div>
        {heroStyle === "illustrated" && <WaveDeco className="waves"/>}

        <div style={{ display: "grid", gridTemplateColumns: "1.4fr 1fr", gap: 24, position: "relative", zIndex: 2, alignItems: "center" }}>
          <div>
            <div style={{ display: "inline-flex", alignItems: "center", gap: 8, padding: "5px 12px", borderRadius: 999, background: "rgba(255,255,255,.18)", fontSize: 11.5, fontWeight: 600, letterSpacing: ".06em", textTransform: "uppercase" }}>
              <span style={{ width: 6, height: 6, borderRadius: "50%", background: "#FCD34D" }}></span>
              Bill Due · March 2026
            </div>
            <div style={{ fontSize: 12, opacity: .8, marginTop: 18, fontWeight: 500, letterSpacing: ".08em", textTransform: "uppercase" }}>Amount Payable</div>
            <div style={{ display: "flex", alignItems: "baseline", gap: 4, marginTop: 6 }}>
              <span style={{ fontSize: 22, fontWeight: 600, opacity: .85 }}>₹</span>
              <span style={{ fontSize: 52, fontWeight: 800, letterSpacing: "-.03em", lineHeight: 1 }}>1,847</span>
              <span style={{ fontSize: 16, opacity: .75, marginLeft: 4 }}>.00</span>
            </div>
            <div style={{ fontSize: 13, opacity: .85, marginTop: 8 }}>
              Due by <strong>15 May 2026</strong> · 4 days remaining
            </div>

            <div style={{ display: "flex", gap: 10, marginTop: 22, flexWrap: "wrap" }}>
              <button className="btn btn-lg" style={{ background: "#fff", color: "var(--brand-dk)", fontWeight: 700 }} onClick={onPay}>
                <i className="fa fa-bolt"></i> Pay ₹1,847 now
              </button>
              <button className="btn btn-lg" style={{ background: "rgba(255,255,255,.16)", color: "#fff", border: "1px solid rgba(255,255,255,.25)" }}>
                <i className="fa fa-download"></i> Download bill
              </button>
            </div>
          </div>

          <div style={{ background: "rgba(255,255,255,.10)", border: "1px solid rgba(255,255,255,.18)", backdropFilter: "blur(8px)", borderRadius: 16, padding: 18 }}>
            <div style={{ fontSize: 11.5, opacity: .8, textTransform: "uppercase", letterSpacing: ".08em", fontWeight: 600 }}>This month's usage</div>
            <div style={{ display: "flex", alignItems: "flex-end", gap: 6, marginTop: 10 }}>
              <span style={{ fontSize: 38, fontWeight: 800, letterSpacing: "-.02em" }}>24.5</span>
              <span style={{ fontSize: 14, opacity: .85, paddingBottom: 6 }}>kilolitres</span>
            </div>
            <div style={{ fontSize: 12, opacity: .85, marginTop: 6 }}>
              <i className="fa fa-arrow-up" style={{ fontSize: 10, marginRight: 4 }}></i>
              11.4% vs Feb · ₹193 higher
            </div>
            <div style={{ marginTop: 14, height: 6, borderRadius: 999, background: "rgba(255,255,255,.18)", overflow: "hidden" }}>
              <div style={{ height: "100%", width: "62%", background: "linear-gradient(90deg, #FCD34D, #fff)", borderRadius: 999 }}></div>
            </div>
            <div style={{ fontSize: 11, opacity: .8, marginTop: 8, display: "flex", justifyContent: "space-between" }}>
              <span>0 KL</span><span>Target 40 KL</span>
            </div>
          </div>
        </div>
      </div>

      {/* ALERT BAR */}
      <div style={{ display: "flex", alignItems: "center", gap: 14, background: "var(--orange-bg)", border: "1px solid color-mix(in srgb, var(--orange) 25%, transparent)", padding: "12px 16px", borderRadius: 12, marginBottom: 24 }}>
        <div style={{ width: 32, height: 32, borderRadius: 10, background: "var(--orange)", color: "#fff", display: "grid", placeItems: "center" }}>
          <i className="fa fa-exclamation"></i>
        </div>
        <div style={{ flex: 1, fontSize: 13.5, color: "var(--ink2)" }}>
          <strong style={{ color: "var(--ink)" }}>Possible leak detected.</strong> Your overnight flow on 8 Apr was 2.4× the usual baseline. Consider checking valves.
        </div>
        <button className="btn btn-outline" style={{ borderColor: "var(--orange)", color: "var(--orange)" }}>Investigate</button>
        <button className="icon-btn"><i className="fa fa-times"></i></button>
      </div>

      {/* QUICK STATS */}
      <div className="stat-grid" style={{ marginBottom: 24 }}>
        <StatCard label="YTD Spend" value="₹18,420" meta="Apr 2025 — Mar 2026" delta="-4.2%" deltaTone="green" icon="rupee-sign" accent="brand"/>
        <StatCard label="Avg. Daily Usage" value="0.82 KL" meta="Last 30 days" delta="+11.4%" deltaTone="orange" icon="tint" accent="brand"/>
        <StatCard label="Auto-pay Status" value="Active" meta="HDFC ••3412 · 3rd of month" icon="check-circle" accent="green"/>
        <StatCard label="Open Tickets" value="1" meta="Low pressure · in progress" icon="headset" accent="orange"/>
      </div>

      {/* USAGE + RECENT */}
      <div style={{ display: "grid", gridTemplateColumns: "1.5fr 1fr", gap: 20 }}>
        <div className="card">
          <div className="card-header">
            <div>
              <div className="card-title">Monthly water consumption</div>
              <div style={{ fontSize: 12, color: "var(--ink3)", marginTop: 2 }}>Last 12 billing cycles, in kilolitres (KL)</div>
            </div>
            <div className="tab-segments" style={{ width: "auto" }}>
              <div className="tab-seg">6M</div>
              <div className="tab-seg active">1Y</div>
              <div className="tab-seg">All</div>
            </div>
          </div>
          <div className="card-body">
            <UsageChart data={USAGE_12M} accent="brand"/>
            <div style={{ display: "grid", gridTemplateColumns: "repeat(3,1fr)", gap: 16, marginTop: 16, paddingTop: 16, borderTop: "1px solid var(--border)" }}>
              <div>
                <div className="ds-label" style={{ fontSize: 10, fontWeight: 700, color: "var(--ink3)", textTransform: "uppercase", letterSpacing: ".08em" }}>Peak month</div>
                <div style={{ fontSize: 18, fontWeight: 700, marginTop: 4 }}>July <span style={{ fontSize: 13, color: "var(--ink3)", fontWeight: 500 }}>· 31.2 KL</span></div>
              </div>
              <div>
                <div className="ds-label" style={{ fontSize: 10, fontWeight: 700, color: "var(--ink3)", textTransform: "uppercase", letterSpacing: ".08em" }}>Average</div>
                <div style={{ fontSize: 18, fontWeight: 700, marginTop: 4 }}>23.3 KL <span style={{ fontSize: 13, color: "var(--green)", fontWeight: 600 }}>↓ 8% vs city</span></div>
              </div>
              <div>
                <div className="ds-label" style={{ fontSize: 10, fontWeight: 700, color: "var(--ink3)", textTransform: "uppercase", letterSpacing: ".08em" }}>YoY trend</div>
                <div style={{ fontSize: 18, fontWeight: 700, marginTop: 4, color: "var(--green)" }}>−4.8% <span style={{ fontSize: 13, color: "var(--ink3)", fontWeight: 500 }}>· vs FY24</span></div>
              </div>
            </div>
          </div>
        </div>

        <div className="card">
          <div className="card-header">
            <div className="card-title">Recent bills</div>
            <button className="btn btn-ghost" style={{ padding: "4px 8px", fontSize: 12.5, color: "var(--brand)" }} onClick={() => onNav("history")}>
              View all <i className="fa fa-arrow-right" style={{ fontSize: 10 }}></i>
            </button>
          </div>
          <div>
            {BILLS.slice(0, 4).map(b => (
              <div key={b.id} style={{ display: "flex", alignItems: "center", gap: 12, padding: "14px 20px", borderBottom: "1px solid var(--border)" }}>
                <div style={{
                  width: 38, height: 38, borderRadius: 10,
                  background: b.status === "due" ? "var(--orange-bg)" : "var(--green-bg)",
                  color: b.status === "due" ? "var(--orange)" : "var(--green)",
                  display: "grid", placeItems: "center"
                }}>
                  <i className={`fa fa-${b.status === "due" ? "file-invoice" : "check"}`}></i>
                </div>
                <div style={{ flex: 1, minWidth: 0 }}>
                  <div style={{ fontWeight: 600, fontSize: 13.5, color: "var(--ink)" }}>{b.period}</div>
                  <div style={{ fontSize: 11.5, color: "var(--ink3)" }} className="mono">{b.id}</div>
                </div>
                <div style={{ textAlign: "right" }}>
                  <div style={{ fontWeight: 700, fontSize: 14 }}>₹{b.amount}</div>
                  <div style={{ fontSize: 11, color: b.status === "due" ? "var(--orange)" : "var(--ink3)" }}>
                    {b.status === "due" ? "Due " + b.due : "Paid"}
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* QUICK ACTIONS */}
      <div style={{ marginTop: 24 }}>
        <div style={{ fontSize: 16, fontWeight: 700, marginBottom: 14 }}>Quick actions</div>
        <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(180px, 1fr))", gap: 14 }}>
          {[
            { i: "credit-card", t: "Pay bill", s: "UPI / Card / NetBank", c: "brand", to: "pay" },
            { i: "sync-alt", t: "Set auto-pay", s: "Never miss a due", c: "green" },
            { i: "plus-circle", t: "New connection", s: "Apply online", c: "purple", to: "newConnection" },
            { i: "exclamation-triangle", t: "Report issue", s: "Leak, low pressure", c: "orange", to: "complaint" },
            { i: "chart-line", t: "Usage analytics", s: "Monthly breakdown", c: "brand", to: "analytics" },
            { i: "file-pdf", t: "Bill history", s: "Download as PDF", c: "red", to: "history" },
          ].map(a => (
            <button key={a.t} className="card" onClick={() => a.to && onNav(a.to)} style={{ padding: 16, textAlign: "left", cursor: "pointer", transition: "var(--transition)" }}>
              <div style={{ width: 38, height: 38, borderRadius: 10, background: `var(--${a.c}-bg)`, color: `var(--${a.c})`, display: "grid", placeItems: "center", fontSize: 15 }}>
                <i className={`fa fa-${a.i}`}></i>
              </div>
              <div style={{ fontSize: 14, fontWeight: 600, marginTop: 12, color: "var(--ink)" }}>{a.t}</div>
              <div style={{ fontSize: 12, color: "var(--ink3)", marginTop: 2 }}>{a.s}</div>
            </button>
          ))}
        </div>
      </div>
    </div>
  );
};

window.Dashboard = Dashboard;
window.BILLS = BILLS;
window.USAGE_12M = USAGE_12M;
