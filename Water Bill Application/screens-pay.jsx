/* global React, Badge, BILLS, PageHeader */
const { useState } = React;

// ─── Pay Bill flow ────────────────────────────
const PayBill = ({ onComplete, onCancel }) => {
  const [step, setStep] = useState(1);
  const [method, setMethod] = useState("upi");
  const [upiHandle, setUpiHandle] = useState("aarav@hdfcbank");
  const [savePaymentMode, setSavePaymentMode] = useState(true);
  const [setupAutopay, setSetupAutopay] = useState(false);
  const [amount, setAmount] = useState("1847");
  const [payMode, setPayMode] = useState("full"); // full | custom

  const next = () => setStep(s => Math.min(3, s + 1));
  const prev = () => setStep(s => Math.max(1, s - 1));

  return (
    <div className="page fade-in">
      <PageHeader
        title="Pay water bill"
        subtitle="Bill NJB-202603-84721 · Sector 50 · A-123"
        action={<button className="btn btn-ghost" onClick={onCancel}><i className="fa fa-times"></i> Cancel</button>}
      />

      <div className="steps">
        <div className={`step ${step >= 1 ? (step > 1 ? "done" : "active") : ""}`}>
          <div className="step-n">{step > 1 ? <i className="fa fa-check" style={{ fontSize: 10 }}/> : 1}</div>
          Review amount
        </div>
        <div className={`step ${step >= 2 ? (step > 2 ? "done" : "active") : ""}`}>
          <div className="step-n">{step > 2 ? <i className="fa fa-check" style={{ fontSize: 10 }}/> : 2}</div>
          Choose method
        </div>
        <div className={`step ${step >= 3 ? "active" : ""}`}>
          <div className="step-n">3</div>
          Confirm & pay
        </div>
      </div>

      <div style={{ display: "grid", gridTemplateColumns: "1.4fr 1fr", gap: 20 }}>
        <div className="card">
          {step === 1 && (
            <div className="card-body">
              <div className="card-title" style={{ marginBottom: 6 }}>Bill summary</div>
              <div style={{ fontSize: 13, color: "var(--ink3)" }}>Charges for billing cycle 01 Mar – 31 Mar 2026.</div>

              <div style={{ marginTop: 20, background: "var(--surface)", border: "1px solid var(--border)", borderRadius: 12, padding: 18 }}>
                <div className="kvp"><span className="k">Water consumption · 24.5 KL @ ₹65/KL</span><span className="v mono">₹1,592.50</span></div>
                <div className="kvp"><span className="k">Sewerage charges · flat</span><span className="v mono">₹185.00</span></div>
                <div className="kvp"><span className="k">Meter rent</span><span className="v mono">₹35.00</span></div>
                <div className="kvp"><span className="k">Service tax (CGST + SGST @ 18%)</span><span className="v mono">₹34.50</span></div>
                <div className="kvp"><span className="k">Late fee / arrears</span><span className="v mono" style={{ color: "var(--green)" }}>₹0.00</span></div>
                <div style={{ display: "flex", justifyContent: "space-between", paddingTop: 14, marginTop: 6, borderTop: "2px solid var(--ink)" }}>
                  <span style={{ fontSize: 14, fontWeight: 600 }}>Total payable</span>
                  <span style={{ fontSize: 22, fontWeight: 800 }} className="mono">₹1,847.00</span>
                </div>
              </div>

              <div style={{ marginTop: 20 }}>
                <div className="field-label" style={{ marginBottom: 10 }}>How much would you like to pay?</div>
                <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 10 }}>
                  <label className="card" style={{ padding: 14, cursor: "pointer", borderColor: payMode === "full" ? "var(--brand)" : "var(--border)", background: payMode === "full" ? "var(--brand-bg)" : "var(--white)" }} onClick={() => { setPayMode("full"); setAmount("1847"); }}>
                    <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                      <span style={{ fontSize: 13, fontWeight: 600 }}>Full amount</span>
                      <input type="radio" checked={payMode === "full"} readOnly style={{ accentColor: "var(--brand)" }}/>
                    </div>
                    <div style={{ fontSize: 20, fontWeight: 700, marginTop: 6 }} className="mono">₹1,847.00</div>
                    <div style={{ fontSize: 11.5, color: "var(--ink3)", marginTop: 2 }}>Settles bill in full</div>
                  </label>
                  <label className="card" style={{ padding: 14, cursor: "pointer", borderColor: payMode === "custom" ? "var(--brand)" : "var(--border)", background: payMode === "custom" ? "var(--brand-bg)" : "var(--white)" }} onClick={() => setPayMode("custom")}>
                    <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                      <span style={{ fontSize: 13, fontWeight: 600 }}>Partial</span>
                      <input type="radio" checked={payMode === "custom"} readOnly style={{ accentColor: "var(--brand)" }}/>
                    </div>
                    <div style={{ display: "flex", alignItems: "center", gap: 4, marginTop: 6 }}>
                      <span style={{ fontSize: 16, color: "var(--ink3)" }}>₹</span>
                      <input className="input mono" value={amount} onChange={e => setAmount(e.target.value)} style={{ fontSize: 18, fontWeight: 700, padding: "4px 8px" }} disabled={payMode !== "custom"}/>
                    </div>
                    <div style={{ fontSize: 11.5, color: "var(--ink3)", marginTop: 2 }}>Balance carries forward</div>
                  </label>
                </div>
              </div>
            </div>
          )}

          {step === 2 && (
            <div className="card-body">
              <div className="card-title">Select payment method</div>
              <div style={{ fontSize: 13, color: "var(--ink3)", marginBottom: 18 }}>All methods are secured by Razorpay & RBI-compliant.</div>

              {[
                { k: "upi", t: "UPI", s: "Pay via GPay, PhonePe, BHIM, or any UPI app", i: "qrcode", badge: "Instant · No charges" },
                { k: "card", t: "Debit / Credit Card", s: "Visa, Mastercard, RuPay accepted", i: "credit-card", badge: "Convenience fee ₹0" },
                { k: "netbanking", t: "Net Banking", s: "60+ Indian banks supported", i: "university", badge: "" },
                { k: "wallet", t: "Wallets", s: "Paytm, Amazon Pay, MobiKwik", i: "wallet", badge: "" },
              ].map(m => (
                <label key={m.k} style={{
                  display: "flex", alignItems: "center", gap: 14,
                  padding: 16, borderRadius: 12,
                  border: method === m.k ? "1.5px solid var(--brand)" : "1px solid var(--border)",
                  background: method === m.k ? "var(--brand-bg)" : "var(--white)",
                  marginBottom: 10, cursor: "pointer", transition: "var(--transition)"
                }} onClick={() => setMethod(m.k)}>
                  <div style={{ width: 42, height: 42, borderRadius: 10, background: "var(--surface)", color: "var(--ink2)", display: "grid", placeItems: "center", fontSize: 18 }}>
                    <i className={`fa fa-${m.i}`}></i>
                  </div>
                  <div style={{ flex: 1 }}>
                    <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
                      <span style={{ fontSize: 14, fontWeight: 600 }}>{m.t}</span>
                      {m.badge && <Badge tone="green">{m.badge}</Badge>}
                    </div>
                    <div style={{ fontSize: 12, color: "var(--ink3)", marginTop: 2 }}>{m.s}</div>
                  </div>
                  <input type="radio" checked={method === m.k} readOnly style={{ accentColor: "var(--brand)", width: 18, height: 18 }}/>
                </label>
              ))}

              {method === "upi" && (
                <div style={{ marginTop: 18, padding: 16, background: "var(--surface)", borderRadius: 12, border: "1px solid var(--border)" }}>
                  <div className="field-label" style={{ marginBottom: 8 }}>UPI ID</div>
                  <div className="input-with-icon">
                    <i className="fa fa-at"></i>
                    <input className="input mono" value={upiHandle} onChange={e => setUpiHandle(e.target.value)}/>
                  </div>
                  <div style={{ display: "flex", gap: 10, marginTop: 12 }}>
                    {["GPay", "PhonePe", "Paytm", "BHIM"].map(p => (
                      <button key={p} className="btn btn-outline" style={{ padding: "6px 12px", fontSize: 12 }}>{p}</button>
                    ))}
                  </div>
                </div>
              )}

              <label className="checkbox" style={{ marginTop: 18 }}>
                <input type="checkbox" checked={savePaymentMode} onChange={e => setSavePaymentMode(e.target.checked)}/>
                Save this method for faster checkout next time
              </label>
              <label className="checkbox" style={{ marginTop: 8 }}>
                <input type="checkbox" checked={setupAutopay} onChange={e => setSetupAutopay(e.target.checked)}/>
                Set up auto-pay — never miss a due date
              </label>
            </div>
          )}

          {step === 3 && (
            <div className="card-body">
              <div className="card-title">Confirm payment</div>
              <div style={{ fontSize: 13, color: "var(--ink3)" }}>Please review the details before completing your payment.</div>

              <div style={{ marginTop: 18, padding: 18, background: "var(--surface)", borderRadius: 12, border: "1px solid var(--border)" }}>
                <div className="kvp"><span className="k">Paying for</span><span className="v">Sector 50 · A-123</span></div>
                <div className="kvp"><span className="k">Bill</span><span className="v mono">NJB-202603-84721</span></div>
                <div className="kvp"><span className="k">Period</span><span className="v">March 2026</span></div>
                <div className="kvp"><span className="k">Payment method</span>
                  <span className="v" style={{ display: "flex", alignItems: "center", gap: 8 }}>
                    <i className="fa fa-qrcode" style={{ color: "var(--brand)" }}></i>
                    UPI · <span className="mono">{upiHandle}</span>
                  </span>
                </div>
              </div>

              <div style={{ marginTop: 18, padding: 22, background: "var(--brand-bg)", borderRadius: 14, border: "1px solid color-mix(in srgb, var(--brand) 25%, transparent)", display: "flex", alignItems: "center", justifyContent: "space-between" }}>
                <div>
                  <div style={{ fontSize: 11.5, color: "var(--ink2)", textTransform: "uppercase", letterSpacing: ".08em", fontWeight: 600 }}>Amount</div>
                  <div style={{ fontSize: 32, fontWeight: 800, color: "var(--brand-dk)", marginTop: 2 }} className="mono">₹{Number(amount).toLocaleString("en-IN")}.00</div>
                </div>
                <div style={{ textAlign: "right", fontSize: 12, color: "var(--ink3)" }}>
                  <div>Convenience fee</div>
                  <div style={{ color: "var(--green)", fontWeight: 600 }}>₹0.00</div>
                </div>
              </div>

              <div style={{ fontSize: 12, color: "var(--ink3)", marginTop: 16, display: "flex", alignItems: "center", gap: 6 }}>
                <i className="fa fa-shield-alt" style={{ color: "var(--green)" }}></i>
                Secured by 256-bit TLS · Backed by Noida Authority
              </div>
            </div>
          )}

          <div style={{ padding: 18, borderTop: "1px solid var(--border)", background: "var(--surface)", display: "flex", justifyContent: "space-between" }}>
            <button className="btn btn-outline" onClick={step === 1 ? onCancel : prev}>
              <i className="fa fa-arrow-left"></i> {step === 1 ? "Cancel" : "Back"}
            </button>
            {step < 3
              ? <button className="btn btn-primary" onClick={next}>Continue <i className="fa fa-arrow-right"></i></button>
              : <button className="btn btn-primary btn-lg" onClick={onComplete}><i className="fa fa-lock"></i> Pay ₹{Number(amount).toLocaleString("en-IN")}.00</button>}
          </div>
        </div>

        {/* Side summary */}
        <div className="card" style={{ height: "fit-content", position: "sticky", top: 80 }}>
          <div className="card-header">
            <div className="card-title">Order summary</div>
          </div>
          <div className="card-body">
            <div className="kvp"><span className="k">Bill amount</span><span className="v mono">₹1,847.00</span></div>
            <div className="kvp"><span className="k">Discount</span><span className="v mono" style={{ color: "var(--green)" }}>− ₹0.00</span></div>
            <div className="kvp"><span className="k">Convenience fee</span><span className="v mono" style={{ color: "var(--green)" }}>₹0.00</span></div>
            <div style={{ display: "flex", justifyContent: "space-between", paddingTop: 14, marginTop: 6, borderTop: "2px solid var(--ink)" }}>
              <span style={{ fontSize: 14, fontWeight: 600 }}>You pay</span>
              <span style={{ fontSize: 20, fontWeight: 800 }} className="mono">₹{Number(amount).toLocaleString("en-IN")}.00</span>
            </div>
            <div style={{ marginTop: 16, padding: 12, background: "var(--accent-bg)", color: "var(--accent)", borderRadius: 10, fontSize: 12, display: "flex", gap: 8, alignItems: "center" }}>
              <i className="fa fa-leaf"></i>
              <span style={{ color: "var(--ink2)" }}>By paying online you save 4 sheets of paper.</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

// ─── Receipt ──────────────────────────────────
const Receipt = ({ onClose, onNav }) => (
  <div className="page fade-in" style={{ maxWidth: 700 }}>
    <div className="card" style={{ overflow: "visible" }}>
      <div style={{ padding: "40px 32px 24px", textAlign: "center", background: "linear-gradient(180deg, var(--accent-bg) 0%, var(--white) 100%)", borderRadius: "var(--radius-lg) var(--radius-lg) 0 0" }}>
        <div style={{ width: 80, height: 80, borderRadius: "50%", background: "var(--accent)", color: "#fff", display: "grid", placeItems: "center", fontSize: 36, margin: "0 auto", boxShadow: "0 12px 28px rgba(16,185,129,.35)" }}>
          <i className="fa fa-check"></i>
        </div>
        <div style={{ fontSize: 26, fontWeight: 800, marginTop: 18, letterSpacing: "-.02em" }}>Payment successful</div>
        <div style={{ fontSize: 14, color: "var(--ink3)", marginTop: 6 }}>Your water bill for March 2026 has been paid.</div>
        <div style={{ fontSize: 36, fontWeight: 800, marginTop: 18 }} className="mono">₹1,847.00</div>
      </div>

      <div style={{ padding: "24px 32px" }}>
        <div className="kvp"><span className="k">Transaction ID</span><span className="v mono">NJTXN-26041182734</span></div>
        <div className="kvp"><span className="k">Bill number</span><span className="v mono">NJB-202603-84721</span></div>
        <div className="kvp"><span className="k">Paid to</span><span className="v">Noida Authority · Water Dept</span></div>
        <div className="kvp"><span className="k">Method</span><span className="v">UPI · aarav@hdfcbank</span></div>
        <div className="kvp"><span className="k">Date & time</span><span className="v mono">11 May 2026, 6:42 PM IST</span></div>
        <div className="kvp"><span className="k">Status</span><span className="v"><Badge tone="green" icon="check">Confirmed</Badge></span></div>

        <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 10, marginTop: 22 }}>
          <button className="btn btn-outline btn-block"><i className="fa fa-download"></i> Download receipt</button>
          <button className="btn btn-outline btn-block"><i className="fa fa-share-alt"></i> Share via WhatsApp</button>
        </div>

        <button className="btn btn-primary btn-lg btn-block" style={{ marginTop: 18 }} onClick={() => onNav("dashboard")}>
          Back to dashboard <i className="fa fa-arrow-right"></i>
        </button>
      </div>
    </div>
  </div>
);

// ─── Bill History ────────────────────────────
const BillHistory = ({ onPay }) => {
  const [filter, setFilter] = useState("all");
  const list = BILLS.filter(b => filter === "all" || b.status === filter);

  return (
    <div className="page fade-in">
      <PageHeader
        title="Bill history"
        subtitle="Download, share, or pay past bills."
        action={
          <div style={{ display: "flex", gap: 10 }}>
            <button className="btn btn-outline"><i className="fa fa-file-csv"></i> Export CSV</button>
            <button className="btn btn-outline"><i className="fa fa-print"></i> Print</button>
          </div>
        }
      />

      <div style={{ display: "flex", gap: 12, alignItems: "center", marginBottom: 16, flexWrap: "wrap" }}>
        <div className="tab-segments" style={{ width: "auto" }}>
          <div className={`tab-seg ${filter === "all" ? "active" : ""}`} onClick={() => setFilter("all")}>All bills</div>
          <div className={`tab-seg ${filter === "due" ? "active" : ""}`} onClick={() => setFilter("due")}>Due</div>
          <div className={`tab-seg ${filter === "paid" ? "active" : ""}`} onClick={() => setFilter("paid")}>Paid</div>
        </div>
        <div className="topbar-search" style={{ flex: "1 1 200px", maxWidth: 360, background: "var(--white)" }}>
          <i className="fa fa-search"></i>
          <input placeholder="Search by bill ID, period, amount..."/>
        </div>
        <select className="select" style={{ width: "auto" }}>
          <option>All connections</option>
          <option>Sector 50 · A-123</option>
          <option>Sector 18 · Shop S-7</option>
        </select>
      </div>

      <div className="card">
        <div className="scrollable">
          <table className="table">
            <thead>
              <tr>
                <th>Bill ID</th>
                <th>Period</th>
                <th>Units (KL)</th>
                <th>Amount</th>
                <th>Status</th>
                <th>Paid on / Due</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {list.map(b => (
                <tr key={b.id}>
                  <td><span className="mono" style={{ color: "var(--ink)", fontWeight: 600 }}>{b.id}</span></td>
                  <td style={{ color: "var(--ink)", fontWeight: 600 }}>{b.period}</td>
                  <td className="mono">{b.units}</td>
                  <td className="mono" style={{ color: "var(--ink)", fontWeight: 700 }}>₹{b.amount}</td>
                  <td>{b.status === "due" ? <Badge tone="orange" icon="clock">Due</Badge> : <Badge tone="green" icon="check">Paid</Badge>}</td>
                  <td style={{ color: "var(--ink3)" }}>
                    {b.status === "due"
                      ? <span>by {b.due}</span>
                      : <span>{b.paidOn} · <span style={{ color: "var(--ink2)" }}>{b.method}</span></span>}
                  </td>
                  <td style={{ textAlign: "right" }}>
                    {b.status === "due"
                      ? <button className="btn btn-primary" style={{ padding: "6px 12px" }} onClick={onPay}>Pay now</button>
                      : <button className="btn btn-ghost" style={{ padding: "6px 8px" }}><i className="fa fa-download"></i></button>}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginTop: 14, fontSize: 12.5, color: "var(--ink3)" }}>
        <span>Showing 1–{list.length} of {list.length} bills</span>
        <div style={{ display: "flex", gap: 6 }}>
          <button className="icon-btn"><i className="fa fa-chevron-left"></i></button>
          <button className="icon-btn" style={{ background: "var(--brand-bg)", color: "var(--brand)" }}>1</button>
          <button className="icon-btn"><i className="fa fa-chevron-right"></i></button>
        </div>
      </div>
    </div>
  );
};

window.PayBill = PayBill;
window.Receipt = Receipt;
window.BillHistory = BillHistory;
