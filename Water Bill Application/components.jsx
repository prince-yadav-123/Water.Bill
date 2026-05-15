/* global React */
const { useState, useEffect, useRef, useMemo } = React;

// ─── Brand mark (mini logo) ────────────────────
const BrandMark = ({ size = 40 }) => (
  <div className="brand-mark" style={{ width: size, height: size }}>
    <svg viewBox="0 0 24 24" fill="none">
      {/* building silhouette + drop, simplified */}
      <path d="M3 21h18M5 21V10l3-2v13M11 21V6l4-3v18M18 21V12l3 1v8" stroke="#fff" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" opacity=".55"/>
      <path d="M12 8c0 0-4 4.5-4 7.5a4 4 0 0 0 8 0C16 12.5 12 8 12 8z" fill="#fff"/>
    </svg>
  </div>
);

// ─── Wave decoration ──────────────────────────
const WaveDeco = ({ className = "", opacity = 0.4 }) => (
  <svg className={className} viewBox="0 0 1200 120" preserveAspectRatio="none" style={{ width: "100%", height: 90, opacity }}>
    <path d="M0,60 C200,100 400,20 600,50 C800,80 1000,30 1200,60 L1200,120 L0,120 Z" fill="rgba(255,255,255,.4)"/>
    <path d="M0,80 C200,60 400,110 600,75 C800,50 1000,95 1200,75 L1200,120 L0,120 Z" fill="rgba(255,255,255,.55)"/>
    <path d="M0,100 C200,90 400,115 600,98 C800,85 1000,108 1200,95 L1200,120 L0,120 Z" fill="rgba(255,255,255,.7)"/>
  </svg>
);

// ─── Badges ───────────────────────────────────
const Badge = ({ tone = "gray", icon, children }) => (
  <span className={`badge b-${tone}`}>
    {icon && <i className={`fa fa-${icon}`}></i>}
    {children}
  </span>
);

// ─── KPI card ─────────────────────────────────
const StatCard = ({ label, value, meta, delta, deltaTone = "green", icon, accent }) => (
  <div className="stat-card">
    <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", gap: 12 }}>
      <div className="stat-label">{label}</div>
      {icon && (
        <div style={{
          width: 32, height: 32, borderRadius: 10,
          background: `var(--${accent || 'brand'}-bg)`,
          color: `var(--${accent || 'brand'})`,
          display: "grid", placeItems: "center", fontSize: 14,
        }}>
          <i className={`fa fa-${icon}`}></i>
        </div>
      )}
    </div>
    <div className="stat-value">{value}</div>
    <div className="stat-meta">
      {delta && (
        <span className={`stat-delta b-${deltaTone}`} style={{
          background: `var(--${deltaTone}-bg)`, color: `var(--${deltaTone})`
        }}>
          <i className={`fa fa-${delta.startsWith('-') ? 'arrow-down' : 'arrow-up'}`} style={{ fontSize: 9, marginRight: 3 }}></i>
          {delta.replace('-','')}
        </span>
      )}
      <span>{meta}</span>
    </div>
  </div>
);

// ─── Usage area chart ─────────────────────────
const UsageChart = ({ data, height = 220, accent = "brand" }) => {
  const w = 600, h = height;
  const pad = { l: 36, r: 12, t: 18, b: 28 };
  const innerW = w - pad.l - pad.r;
  const innerH = h - pad.t - pad.b;
  const max = Math.max(...data.map(d => d.v)) * 1.2;
  const stepX = innerW / (data.length - 1);
  const pts = data.map((d, i) => [pad.l + i * stepX, pad.t + innerH - (d.v / max) * innerH]);
  const linePath = pts.map((p, i) => (i === 0 ? `M${p[0]},${p[1]}` : `L${p[0]},${p[1]}`)).join(" ");
  const areaPath = `${linePath} L${pts[pts.length-1][0]},${pad.t+innerH} L${pts[0][0]},${pad.t+innerH} Z`;
  const yTicks = 4;

  return (
    <svg viewBox={`0 0 ${w} ${h}`} style={{ width: "100%", height: "auto", display: "block" }}>
      <defs>
        <linearGradient id={`area-${accent}`} x1="0" y1="0" x2="0" y2="1">
          <stop offset="0%" stopColor={`var(--${accent})`} stopOpacity=".35"/>
          <stop offset="100%" stopColor={`var(--${accent})`} stopOpacity="0"/>
        </linearGradient>
      </defs>
      {/* grid */}
      {[...Array(yTicks)].map((_, i) => {
        const y = pad.t + (innerH / yTicks) * i;
        const val = Math.round((max - (max / yTicks) * i) * 10) / 10;
        return (
          <g key={i}>
            <line className="chart-grid" x1={pad.l} x2={w - pad.r} y1={y} y2={y}/>
            <text x={pad.l - 8} y={y + 3} textAnchor="end" style={{ fill: "var(--ink3)", fontSize: 10 }}>{val}</text>
          </g>
        );
      })}
      {/* area */}
      <path d={areaPath} fill={`url(#area-${accent})`}/>
      <path d={linePath} fill="none" stroke={`var(--${accent})`} strokeWidth="2.5" strokeLinejoin="round" strokeLinecap="round"/>
      {/* points */}
      {pts.map((p, i) => (
        <g key={i}>
          <circle cx={p[0]} cy={p[1]} r="4" fill="var(--white)" stroke={`var(--${accent})`} strokeWidth="2"/>
        </g>
      ))}
      {/* x labels */}
      {data.map((d, i) => (
        <text key={i} x={pad.l + i * stepX} y={h - 8} textAnchor="middle" style={{ fill: "var(--ink3)", fontSize: 10 }}>
          {d.label}
        </text>
      ))}
    </svg>
  );
};

// ─── Donut ────────────────────────────────────
const Donut = ({ value, total, size = 100, color = "brand", label }) => {
  const r = size / 2 - 8;
  const c = 2 * Math.PI * r;
  const pct = Math.min(1, value / total);
  return (
    <div style={{ position: "relative", width: size, height: size }}>
      <svg width={size} height={size} style={{ transform: "rotate(-90deg)" }}>
        <circle cx={size/2} cy={size/2} r={r} fill="none" stroke="var(--surface2)" strokeWidth="8"/>
        <circle cx={size/2} cy={size/2} r={r} fill="none" stroke={`var(--${color})`} strokeWidth="8"
          strokeDasharray={`${c * pct} ${c}`} strokeLinecap="round"/>
      </svg>
      <div style={{
        position: "absolute", inset: 0, display: "grid", placeItems: "center",
        textAlign: "center", lineHeight: 1.1
      }}>
        <div>
          <div style={{ fontSize: 18, fontWeight: 700, color: "var(--ink)" }}>{value}</div>
          <div style={{ fontSize: 10, color: "var(--ink3)" }}>{label}</div>
        </div>
      </div>
    </div>
  );
};

// ─── Connection switcher ──────────────────────
const ConnectionPill = ({ conn, onClick }) => (
  <button className="conn-pill" onClick={onClick}>
    <div className="conn-icon">
      <i className={`fa fa-${conn.icon || 'home'}`}></i>
    </div>
    <div className="conn-info">
      <div className="name">{conn.name}</div>
      <div className="id">{conn.id}</div>
    </div>
    <i className="fa fa-chevron-down" style={{ color: "var(--ink3)", fontSize: 11, marginLeft: 6 }}></i>
  </button>
);

// ─── Generic top section: "Hello, Aarav" ──────
const PageHeader = ({ title, subtitle, action }) => (
  <div className="page-header" style={{ display: "flex", alignItems: "flex-end", justifyContent: "space-between", gap: 16, flexWrap: "wrap" }}>
    <div>
      <div className="page-title">{title}</div>
      {subtitle && <div className="page-subtitle">{subtitle}</div>}
    </div>
    {action}
  </div>
);

// expose
Object.assign(window, { BrandMark, WaveDeco, Badge, StatCard, UsageChart, Donut, ConnectionPill, PageHeader });
