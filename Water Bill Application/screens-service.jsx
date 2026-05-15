/* global React, Badge, PageHeader */
const { useState } = React;

// ─── New Connection ───────────────────────────
const NewConnection = ({ onCancel }) => {
  const [step, setStep] = useState(1);
  const [type, setType] = useState("domestic");
  return (
    <div className="page fade-in">
      <PageHeader
        title="Apply for new connection"
        subtitle="Get a NoidaJal water connection in 7–10 working days."
        action={<button className="btn btn-ghost" onClick={onCancel}><i className="fa fa-times"></i> Cancel</button>}
      />

      <div className="steps">
        {["Connection type", "Property details", "Documents", "Review & submit"].map((s, i) => (
          <div key={s} className={`step ${i + 1 === step ? "active" : i + 1 < step ? "done" : ""}`}>
            <div className="step-n">{i + 1 < step ? <i className="fa fa-check" style={{ fontSize: 10 }}/> : i + 1}</div>
            {s}
          </div>
        ))}
      </div>

      <div style={{ display: "grid", gridTemplateColumns: "1.5fr 1fr", gap: 20 }}>
        <div className="card">
          {step === 1 && (
            <div className="card-body">
              <div className="card-title">What type of connection?</div>
              <div style={{ fontSize: 13, color: "var(--ink3)", marginBottom: 18 }}>Tariff and document requirements vary by category.</div>
              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
                {[
                  { k: "domestic", i: "home", t: "Domestic", s: "Residential household", r: "₹65 / KL" },
                  { k: "commercial", i: "store", t: "Commercial", s: "Shop / office", r: "₹120 / KL" },
                  { k: "industrial", i: "industry", t: "Industrial", s: "Factory / unit", r: "₹185 / KL" },
                  { k: "institutional", i: "university", t: "Institutional", s: "School / hospital", r: "₹90 / KL" },
                ].map(o => (
                  <label key={o.k} className="card" style={{
                    padding: 18, cursor: "pointer",
                    borderColor: type === o.k ? "var(--brand)" : "var(--border)",
                    background: type === o.k ? "var(--brand-bg)" : "var(--white)"
                  }} onClick={() => setType(o.k)}>
                    <div style={{ width: 38, height: 38, borderRadius: 10, background: "var(--white)", color: "var(--brand)", display: "grid", placeItems: "center" }}>
                      <i className={`fa fa-${o.i}`}></i>
                    </div>
                    <div style={{ fontWeight: 600, fontSize: 14, marginTop: 12 }}>{o.t}</div>
                    <div style={{ fontSize: 12, color: "var(--ink3)" }}>{o.s}</div>
                    <div className="mono" style={{ fontSize: 12, color: "var(--brand)", fontWeight: 600, marginTop: 8 }}>{o.r}</div>
                  </label>
                ))}
              </div>
              <div className="divider" style={{ margin: "22px 0" }}></div>
              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 14 }}>
                <div className="field"><label className="field-label">Required pipe size</label>
                  <select className="select"><option>1/2 inch (default)</option><option>3/4 inch</option><option>1 inch</option></select>
                </div>
                <div className="field"><label className="field-label">Estimated daily usage</label>
                  <select className="select"><option>Up to 1 KL/day</option><option>1–3 KL/day</option><option>Above 3 KL/day</option></select>
                </div>
              </div>
            </div>
          )}

          {step === 2 && (
            <div className="card-body">
              <div className="card-title">Property details</div>
              <div style={{ fontSize: 13, color: "var(--ink3)", marginBottom: 18 }}>Where should the new connection be installed?</div>
              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 14 }}>
                <div className="field" style={{ gridColumn: "1/-1" }}><label className="field-label">Property address (Line 1)</label><input className="input" placeholder="House / Flat number, Street"/></div>
                <div className="field" style={{ gridColumn: "1/-1" }}><label className="field-label">Property address (Line 2)</label><input className="input" placeholder="Society / Landmark"/></div>
                <div className="field"><label className="field-label">Sector / Block</label><input className="input" placeholder="e.g. Sector 50"/></div>
                <div className="field"><label className="field-label">PIN code</label><input className="input mono" placeholder="201301"/></div>
                <div className="field"><label className="field-label">Property type</label><select className="select"><option>Owned</option><option>Rented</option><option>Builder-allotted</option></select></div>
                <div className="field"><label className="field-label">Built-up area (sq.ft.)</label><input className="input mono" placeholder="1450"/></div>
                <div className="field"><label className="field-label">Number of occupants</label><input className="input mono" placeholder="4"/></div>
                <div className="field"><label className="field-label">Bathrooms / kitchens</label><input className="input mono" placeholder="3 / 1"/></div>
              </div>
            </div>
          )}

          {step === 3 && (
            <div className="card-body">
              <div className="card-title">Upload documents</div>
              <div style={{ fontSize: 13, color: "var(--ink3)", marginBottom: 18 }}>Clear photos or PDFs · max 5 MB each.</div>
              {[
                { t: "Aadhaar card", s: "Front + back", req: true, done: true },
                { t: "Property ownership proof", s: "Allotment letter / sale deed", req: true, done: true },
                { t: "Recent electricity bill", s: "Same address, under 3 months", req: true, done: false },
                { t: "NOC from society", s: "If applicable", req: false, done: false },
                { t: "Site photograph", s: "Where meter will be installed", req: true, done: false },
              ].map(d => (
                <div key={d.t} style={{ display: "flex", alignItems: "center", gap: 14, padding: 14, border: "1px solid var(--border)", borderRadius: 12, marginBottom: 10, background: d.done ? "var(--accent-bg)" : "var(--white)" }}>
                  <div style={{ width: 38, height: 38, borderRadius: 10, background: d.done ? "var(--accent)" : "var(--surface)", color: d.done ? "#fff" : "var(--ink3)", display: "grid", placeItems: "center" }}>
                    <i className={`fa fa-${d.done ? "check" : "file-upload"}`}></i>
                  </div>
                  <div style={{ flex: 1 }}>
                    <div style={{ fontSize: 13.5, fontWeight: 600 }}>{d.t} {d.req && <span style={{ color: "var(--red)", fontWeight: 700 }}> *</span>}</div>
                    <div style={{ fontSize: 12, color: "var(--ink3)" }}>{d.s}</div>
                  </div>
                  {d.done
                    ? <Badge tone="green" icon="check">Uploaded</Badge>
                    : <button className="btn btn-outline" style={{ padding: "6px 12px", fontSize: 12 }}><i className="fa fa-paperclip"></i> Browse</button>}
                </div>
              ))}
            </div>
          )}

          {step === 4 && (
            <div className="card-body">
              <div className="card-title">Review your application</div>
              <div style={{ marginTop: 14, padding: 18, background: "var(--surface)", borderRadius: 12, border: "1px solid var(--border)" }}>
                <div className="kvp"><span className="k">Connection type</span><span className="v" style={{ textTransform: "capitalize" }}>{type}</span></div>
                <div className="kvp"><span className="k">Pipe size</span><span className="v">1/2 inch</span></div>
                <div className="kvp"><span className="k">Property</span><span className="v">A-456, Sector 50, Noida 201301</span></div>
                <div className="kvp"><span className="k">Built-up area</span><span className="v">1,450 sq.ft.</span></div>
                <div className="kvp"><span className="k">Documents uploaded</span><span className="v">3 of 4 required</span></div>
                <div className="kvp"><span className="k">Estimated security deposit</span><span className="v mono">₹2,500.00</span></div>
                <div className="kvp"><span className="k">Application fee</span><span className="v mono">₹500.00</span></div>
              </div>

              <div style={{ marginTop: 16, padding: 14, background: "var(--brand-bg)", borderRadius: 10, fontSize: 13, color: "var(--ink2)", display: "flex", gap: 10 }}>
                <i className="fa fa-info-circle" style={{ color: "var(--brand)", fontSize: 16, paddingTop: 1 }}></i>
                <div>After submission, a NoidaJal inspector will visit the property within 3 working days to verify details and install the meter.</div>
              </div>

              <label className="checkbox" style={{ marginTop: 16 }}>
                <input type="checkbox" defaultChecked/>
                I declare that all information provided is accurate and I accept NoidaJal's <a href="#">terms of service</a>.
              </label>
            </div>
          )}

          <div style={{ padding: 18, borderTop: "1px solid var(--border)", background: "var(--surface)", display: "flex", justifyContent: "space-between" }}>
            <button className="btn btn-outline" onClick={() => step > 1 ? setStep(s => s - 1) : onCancel()}>
              <i className="fa fa-arrow-left"></i> {step > 1 ? "Back" : "Cancel"}
            </button>
            {step < 4
              ? <button className="btn btn-primary" onClick={() => setStep(s => s + 1)}>Continue <i className="fa fa-arrow-right"></i></button>
              : <button className="btn btn-primary btn-lg" onClick={onCancel}><i className="fa fa-paper-plane"></i> Submit application</button>}
          </div>
        </div>

        <div style={{ display: "flex", flexDirection: "column", gap: 16 }}>
          <div className="card">
            <div className="card-header"><div className="card-title">What you'll pay</div></div>
            <div className="card-body">
              <div className="kvp"><span className="k">Application fee</span><span className="v mono">₹500</span></div>
              <div className="kvp"><span className="k">Security deposit</span><span className="v mono">₹2,500</span></div>
              <div className="kvp"><span className="k">Meter installation</span><span className="v mono">₹0 (free)</span></div>
              <div style={{ display: "flex", justifyContent: "space-between", paddingTop: 12, marginTop: 4, borderTop: "1px solid var(--ink)" }}>
                <span style={{ fontWeight: 600 }}>Total today</span>
                <span style={{ fontWeight: 800, fontSize: 18 }} className="mono">₹3,000</span>
              </div>
            </div>
          </div>

          <div className="card">
            <div className="card-header"><div className="card-title">Timeline</div></div>
            <div className="card-body">
              {[
                { t: "Submit application", s: "Today", done: true },
                { t: "Document verification", s: "Within 24 hours", done: false },
                { t: "Site inspection", s: "Day 2–3", done: false },
                { t: "Approval & meter install", s: "Day 7–10", done: false },
                { t: "First bill generated", s: "After 1 cycle", done: false },
              ].map((x, i, arr) => (
                <div key={x.t} style={{ display: "flex", gap: 12 }}>
                  <div style={{ display: "flex", flexDirection: "column", alignItems: "center" }}>
                    <div style={{ width: 18, height: 18, borderRadius: "50%", background: x.done ? "var(--accent)" : "var(--white)", border: `2px solid ${x.done ? "var(--accent)" : "var(--border2)"}`, display: "grid", placeItems: "center", color: "#fff" }}>
                      {x.done && <i className="fa fa-check" style={{ fontSize: 8 }}/>}
                    </div>
                    {i < arr.length - 1 && <div style={{ width: 2, flex: 1, background: "var(--border)", margin: "2px 0" }}></div>}
                  </div>
                  <div style={{ paddingBottom: 14, flex: 1 }}>
                    <div style={{ fontSize: 13, fontWeight: 600, color: "var(--ink)" }}>{x.t}</div>
                    <div style={{ fontSize: 11.5, color: "var(--ink3)" }}>{x.s}</div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

// ─── Complaint ────────────────────────────────
const Complaint = ({ onCancel }) => {
  const [type, setType] = useState("leak");
  const types = [
    { k: "leak", i: "tint", t: "Pipe leak", c: "blue" },
    { k: "pressure", i: "compress-arrows-alt", t: "Low pressure", c: "orange" },
    { k: "quality", i: "vial", t: "Water quality", c: "purple" },
    { k: "billing", i: "file-invoice-dollar", t: "Billing dispute", c: "red" },
    { k: "meter", i: "tachometer-alt", t: "Meter issue", c: "green" },
    { k: "outage", i: "ban", t: "No water supply", c: "red" },
  ];

  return (
    <div className="page fade-in">
      <PageHeader
        title="Raise a complaint"
        subtitle="Most issues are resolved within 24 hours."
        action={<button className="btn btn-ghost" onClick={onCancel}><i className="fa fa-times"></i> Cancel</button>}
      />

      <div style={{ display: "grid", gridTemplateColumns: "1.6fr 1fr", gap: 20 }}>
        <div className="card">
          <div className="card-body">
            <div className="card-title" style={{ marginBottom: 14 }}>What's the issue?</div>
            <div style={{ display: "grid", gridTemplateColumns: "repeat(3,1fr)", gap: 10 }}>
              {types.map(t => (
                <label key={t.k} className="card" style={{
                  padding: 14, cursor: "pointer", textAlign: "center",
                  borderColor: type === t.k ? "var(--brand)" : "var(--border)",
                  background: type === t.k ? "var(--brand-bg)" : "var(--white)"
                }} onClick={() => setType(t.k)}>
                  <div style={{ width: 38, height: 38, borderRadius: 10, background: `var(--${t.c}-bg)`, color: `var(--${t.c}, var(--brand))`, display: "grid", placeItems: "center", margin: "0 auto" }}>
                    <i className={`fa fa-${t.i}`}></i>
                  </div>
                  <div style={{ fontSize: 12.5, fontWeight: 600, marginTop: 10 }}>{t.t}</div>
                </label>
              ))}
            </div>

            <div className="divider"></div>

            <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 14 }}>
              <div className="field"><label className="field-label">Connection</label>
                <select className="select"><option>Sector 50 · A-123 (NJL-20A-184721)</option><option>Sector 18 · Shop S-7</option></select>
              </div>
              <div className="field"><label className="field-label">Urgency</label>
                <select className="select"><option>Normal — within 48 hours</option><option>Urgent — within 24 hours</option><option>Emergency — within 4 hours</option></select>
              </div>
              <div className="field" style={{ gridColumn: "1/-1" }}><label className="field-label">Describe the issue</label>
                <textarea className="textarea" rows="4" placeholder="e.g. Water has been leaking from the main pipe outside our gate since yesterday evening. The pressure inside the house has also dropped." defaultValue="Noticed a steady drip from the main valve at the meter point starting around 7 PM yesterday. Pooling has reached the driveway."></textarea>
              </div>
              <div className="field" style={{ gridColumn: "1/-1" }}><label className="field-label">Attach photo / video (optional)</label>
                <div style={{ border: "2px dashed var(--border2)", borderRadius: 12, padding: 22, textAlign: "center", color: "var(--ink3)", background: "var(--surface)" }}>
                  <i className="fa fa-cloud-upload-alt" style={{ fontSize: 28, color: "var(--ink4)" }}></i>
                  <div style={{ marginTop: 10, fontSize: 13 }}>Drop files here or <a href="#">browse</a></div>
                  <div style={{ fontSize: 11.5, marginTop: 4 }}>PNG, JPG, MP4 — max 25 MB total</div>
                </div>
              </div>
              <div className="field"><label className="field-label">Preferred contact time</label>
                <select className="select"><option>Any time</option><option>Morning (8–12)</option><option>Afternoon (12–5)</option><option>Evening (5–9)</option></select>
              </div>
              <div className="field"><label className="field-label">Mobile</label>
                <input className="input mono" defaultValue="+91 98765 43210"/>
              </div>
            </div>
          </div>
          <div style={{ padding: 18, borderTop: "1px solid var(--border)", background: "var(--surface)", display: "flex", justifyContent: "space-between" }}>
            <button className="btn btn-outline" onClick={onCancel}>Cancel</button>
            <button className="btn btn-primary" onClick={onCancel}><i className="fa fa-paper-plane"></i> Submit complaint</button>
          </div>
        </div>

        <div style={{ display: "flex", flexDirection: "column", gap: 16 }}>
          <div className="card">
            <div className="card-header"><div className="card-title">Service guarantee</div></div>
            <div className="card-body">
              {[
                { t: "Acknowledgement", s: "Instant SMS + email" },
                { t: "First response", s: "Within 4 hours" },
                { t: "Resolution", s: "24–48 hours" },
                { t: "Escalation", s: "Auto-escalate after 72 hrs" },
              ].map(x => (
                <div key={x.t} style={{ display: "flex", alignItems: "center", gap: 10, padding: "8px 0" }}>
                  <i className="fa fa-check-circle" style={{ color: "var(--accent)" }}></i>
                  <div style={{ flex: 1, fontSize: 13 }}>{x.t}</div>
                  <span style={{ fontSize: 12, color: "var(--ink3)" }}>{x.s}</span>
                </div>
              ))}
            </div>
          </div>

          <div className="card">
            <div className="card-header"><div className="card-title">Open tickets</div></div>
            <div className="card-body" style={{ padding: 0 }}>
              <div style={{ padding: "14px 20px", borderBottom: "1px solid var(--border)" }}>
                <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                  <span className="mono" style={{ fontSize: 12, fontWeight: 600, color: "var(--brand)" }}>NJT-26041-0421</span>
                  <Badge tone="orange" icon="clock">In progress</Badge>
                </div>
                <div style={{ fontSize: 13.5, fontWeight: 600, marginTop: 6 }}>Low pressure in upper floor</div>
                <div style={{ fontSize: 12, color: "var(--ink3)", marginTop: 2 }}>Raised 09 May · technician assigned</div>
              </div>
              <div style={{ padding: "14px 20px" }}>
                <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                  <span className="mono" style={{ fontSize: 12, fontWeight: 600, color: "var(--ink3)" }}>NJT-26032-0188</span>
                  <Badge tone="green" icon="check">Resolved</Badge>
                </div>
                <div style={{ fontSize: 13.5, fontWeight: 600, marginTop: 6 }}>Meter glass cracked</div>
                <div style={{ fontSize: 12, color: "var(--ink3)", marginTop: 2 }}>Closed 04 Apr · meter replaced</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

window.NewConnection = NewConnection;
window.Complaint = Complaint;
