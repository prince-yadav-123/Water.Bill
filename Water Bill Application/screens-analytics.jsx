/* global React, Badge, USAGE_12M, UsageChart, Donut, PageHeader, StatCard */
const { useState } = React;

// ─── Usage Analytics ──────────────────────────
const Analytics = () => {
  const [range, setRange] = useState("12m");

  return (
    <div className="page fade-in">
      <PageHeader
        title="Usage analytics"
        subtitle="Understand your water consumption patterns and save money."
        action={
          <div style={{ display: "flex", gap: 10 }}>
            <select className="select" style={{ width: "auto" }}>
              <option>Sector 50 · A-123</option>
              <option>Sector 18 · Shop S-7</option>
            </select>
            <button className="btn btn-outline"><i className="fa fa-file-download"></i> Export</button>
          </div>
        }
      />

      <div className="stat-grid" style={{ marginBottom: 20 }}>
        <StatCard label="Avg / day" value="0.82 KL" meta="Last 30 days" delta="+11.4%" deltaTone="orange" icon="tint" accent="brand"/>
        <StatCard label="Highest day" value="1.61 KL" meta="08 Apr · suspected leak" delta="2.0×" deltaTone="red" icon="exclamation" accent="red"/>
        <StatCard label="Lowest day" value="0.42 KL" meta="22 Mar · weekend trip" icon="moon" accent="green"/>
        <StatCard label="Estimated next bill" value="₹2,140" meta="If trend continues" delta="+15.9%" deltaTone="orange" icon="receipt" accent="brand"/>
      </div>

      <div style={{ display: "grid", gridTemplateColumns: "1.6fr 1fr", gap: 20, marginBottom: 20 }}>
        <div className="card">
          <div className="card-header">
            <div>
              <div className="card-title">Daily consumption · April 2026</div>
              <div style={{ fontSize: 12, color: "var(--ink3)", marginTop: 2 }}>30 most recent meter readings</div>
            </div>
            <div className="tab-segments" style={{ width: "auto" }}>
              <div className={`tab-seg ${range==="30d"?"active":""}`} onClick={() => setRange("30d")}>30D</div>
              <div className={`tab-seg ${range==="6m"?"active":""}`} onClick={() => setRange("6m")}>6M</div>
              <div className={`tab-seg ${range==="12m"?"active":""}`} onClick={() => setRange("12m")}>1Y</div>
            </div>
          </div>
          <div className="card-body">
            <BarChart/>
          </div>
        </div>

        <div className="card">
          <div className="card-header"><div className="card-title">Where your water goes</div></div>
          <div className="card-body">
            <div style={{ display: "flex", alignItems: "center", justifyContent: "center", padding: "8px 0 20px" }}>
              <DonutBig/>
            </div>
            <div style={{ display: "flex", flexDirection: "column", gap: 10 }}>
              {[
                { c: "#0891B2", t: "Bathing & hygiene", v: "9.8 KL", p: "40%" },
                { c: "#10B981", t: "Cooking & drinking", v: "3.2 KL", p: "13%" },
                { c: "#7C3AED", t: "Washing clothes", v: "5.4 KL", p: "22%" },
                { c: "#F59E0B", t: "Cleaning & misc", v: "3.1 KL", p: "13%" },
                { c: "#EF4444", t: "Garden / outdoor", v: "3.0 KL", p: "12%" },
              ].map(x => (
                <div key={x.t} style={{ display: "flex", alignItems: "center", gap: 10 }}>
                  <span style={{ width: 10, height: 10, borderRadius: 3, background: x.c }}></span>
                  <span style={{ flex: 1, fontSize: 13, color: "var(--ink2)" }}>{x.t}</span>
                  <span style={{ fontSize: 12.5, color: "var(--ink3)" }} className="mono">{x.v}</span>
                  <span style={{ fontSize: 12, fontWeight: 600, color: "var(--ink)", minWidth: 36, textAlign: "right" }}>{x.p}</span>
                </div>
              ))}
            </div>
            <div style={{ marginTop: 16, padding: 12, background: "var(--surface)", borderRadius: 10, fontSize: 12, color: "var(--ink3)" }}>
              Estimated from household profile + IoT-meter flow patterns.
            </div>
          </div>
        </div>
      </div>

      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 20, marginBottom: 20 }}>
        <div className="card">
          <div className="card-header"><div className="card-title">You vs neighbourhood</div></div>
          <div className="card-body">
            <CompareBar label="Your home" value={0.82} color="var(--brand)" max={1.4}/>
            <CompareBar label="Sector 50 average" value={0.91} color="var(--ink3)" max={1.4}/>
            <CompareBar label="Most efficient 10%" value={0.58} color="var(--accent)" max={1.4}/>
            <CompareBar label="Noida city average" value={1.05} color="var(--orange)" max={1.4}/>
            <div style={{ marginTop: 16, padding: 14, background: "var(--accent-bg)", borderRadius: 10, fontSize: 13, color: "var(--ink2)", display: "flex", gap: 10 }}>
              <i className="fa fa-trophy" style={{ color: "var(--accent)", fontSize: 18 }}></i>
              <div>
                <strong style={{ color: "var(--ink)" }}>You're in the top 22%</strong> for your sector. Keep it up — small wins compound.
              </div>
            </div>
          </div>
        </div>

        <div className="card">
          <div className="card-header"><div className="card-title">Smart saving tips</div></div>
          <div className="card-body" style={{ padding: 0 }}>
            {[
              { i: "shower", t: "Cut shower time by 2 min", s: "Could save up to ~6 KL/month", save: "₹390" },
              { i: "wrench", t: "Fix overnight leak", s: "Detected on 8 Apr · 2.4× baseline", save: "₹240" },
              { i: "tshirt", t: "Run washer at full load", s: "Combines 3 weekly cycles into 2", save: "₹180" },
              { i: "seedling", t: "Water plants after sunset", s: "Reduces evaporation loss by ~30%", save: "₹95" },
            ].map(tip => (
              <div key={tip.t} style={{ display: "flex", alignItems: "center", gap: 14, padding: "14px 20px", borderBottom: "1px solid var(--border)" }}>
                <div style={{ width: 38, height: 38, borderRadius: 10, background: "var(--brand-bg)", color: "var(--brand)", display: "grid", placeItems: "center" }}>
                  <i className={`fa fa-${tip.i}`}></i>
                </div>
                <div style={{ flex: 1 }}>
                  <div style={{ fontSize: 13.5, fontWeight: 600 }}>{tip.t}</div>
                  <div style={{ fontSize: 12, color: "var(--ink3)" }}>{tip.s}</div>
                </div>
                <Badge tone="green" icon="rupee-sign">Save {tip.save}</Badge>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};

const BarChart = () => {
  const data = Array.from({ length: 30 }, (_, i) => 0.5 + Math.sin(i / 3) * 0.2 + Math.random() * 0.25);
  data[7] = 1.61; // spike
  const max = Math.max(...data) * 1.1;
  const w = 600, h = 220, pad = { l: 32, r: 10, t: 10, b: 24 };
  const bw = (w - pad.l - pad.r) / data.length;
  return (
    <svg viewBox={`0 0 ${w} ${h}`} style={{ width: "100%", height: "auto", display: "block" }}>
      {[0, 0.5, 1, 1.5].map((v, i) => {
        const y = pad.t + (h - pad.t - pad.b) - (v / max) * (h - pad.t - pad.b);
        return (<g key={i}>
          <line x1={pad.l} x2={w - pad.r} y1={y} y2={y} className="chart-grid"/>
          <text x={pad.l - 6} y={y + 3} textAnchor="end" style={{ fill: "var(--ink3)", fontSize: 10 }}>{v}</text>
        </g>);
      })}
      {data.map((v, i) => {
        const x = pad.l + i * bw + 1;
        const bh = (v / max) * (h - pad.t - pad.b);
        const y = h - pad.b - bh;
        const isSpike = i === 7;
        return <rect key={i} x={x} y={y} width={bw - 2} height={bh} rx={3} fill={isSpike ? "var(--red)" : "var(--brand)"} opacity={isSpike ? 1 : 0.85}/>;
      })}
      {[1, 8, 15, 22, 29].map(i => (
        <text key={i} x={pad.l + i * bw + bw / 2} y={h - 6} textAnchor="middle" style={{ fill: "var(--ink3)", fontSize: 10 }}>{i + 1}</text>
      ))}
    </svg>
  );
};

const DonutBig = () => {
  const segs = [
    { c: "#0891B2", p: 40 }, { c: "#7C3AED", p: 22 }, { c: "#10B981", p: 13 },
    { c: "#F59E0B", p: 13 }, { c: "#EF4444", p: 12 },
  ];
  const r = 70, c = 2 * Math.PI * r;
  let acc = 0;
  return (
    <div style={{ position: "relative", width: 180, height: 180 }}>
      <svg width="180" height="180" style={{ transform: "rotate(-90deg)" }}>
        <circle cx="90" cy="90" r={r} fill="none" stroke="var(--surface2)" strokeWidth="22"/>
        {segs.map((s, i) => {
          const len = (s.p / 100) * c;
          const off = -acc;
          acc += len;
          return <circle key={i} cx="90" cy="90" r={r} fill="none" stroke={s.c} strokeWidth="22" strokeDasharray={`${len} ${c}`} strokeDashoffset={off}/>;
        })}
      </svg>
      <div style={{ position: "absolute", inset: 0, display: "grid", placeItems: "center", textAlign: "center" }}>
        <div>
          <div style={{ fontSize: 28, fontWeight: 800, color: "var(--ink)" }}>24.5</div>
          <div style={{ fontSize: 11, color: "var(--ink3)", letterSpacing: ".08em", textTransform: "uppercase", fontWeight: 600 }}>KL · Apr</div>
        </div>
      </div>
    </div>
  );
};

const CompareBar = ({ label, value, max, color }) => (
  <div style={{ marginBottom: 14 }}>
    <div style={{ display: "flex", justifyContent: "space-between", fontSize: 13, marginBottom: 6 }}>
      <span style={{ color: "var(--ink2)" }}>{label}</span>
      <span className="mono" style={{ fontWeight: 600 }}>{value} KL/day</span>
    </div>
    <div style={{ height: 10, background: "var(--surface)", borderRadius: 999, overflow: "hidden" }}>
      <div style={{ width: `${(value / max) * 100}%`, height: "100%", background: color, borderRadius: 999 }}></div>
    </div>
  </div>
);

window.Analytics = Analytics;
