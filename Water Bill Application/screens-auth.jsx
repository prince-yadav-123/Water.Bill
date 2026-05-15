/* global React, BrandMark, WaveDeco, Badge */
const { useState } = React;

// ─── Login Screen ─────────────────────────────
const LoginScreen = ({ onLogin }) => {
  const [method, setMethod] = useState("mobile"); // mobile | consumer | email
  const [step, setStep] = useState(1); // 1=enter, 2=otp
  const [mobile, setMobile] = useState("98765 43210");
  const [consumerId, setConsumerId] = useState("NJL-20A-184721");
  const [email, setEmail] = useState("aarav.sharma@gmail.com");
  const [password, setPassword] = useState("••••••••");
  const [otp, setOtp] = useState(["", "", "", "", "", ""]);

  const handleOtp = (i, v) => {
    if (!/^\d?$/.test(v)) return;
    const next = [...otp]; next[i] = v; setOtp(next);
    if (v && i < 5) document.getElementById(`otp-${i+1}`)?.focus();
  };

  const submitFirstStep = () => {
    if (method === "email") onLogin();
    else setStep(2);
  };

  return (
    <div className="login-shell">
      <div className="login-art">
        <div style={{ display: "flex", alignItems: "center", gap: 14, position: "relative", zIndex: 2 }}>
          <BrandMark size={46}/>
          <div style={{ lineHeight: 1.1 }}>
            <div style={{ fontSize: 20, fontWeight: 800, letterSpacing: "-.01em" }}>NoidaJal</div>
            <div style={{ fontSize: 11, opacity: .8, textTransform: "uppercase", letterSpacing: ".12em", marginTop: 4 }}>Water Bill System</div>
          </div>
        </div>

        <div style={{ position: "relative", zIndex: 2, maxWidth: 480 }}>
          <div style={{ fontSize: 36, fontWeight: 700, letterSpacing: "-.02em", lineHeight: 1.15, marginBottom: 16 }}>
            Pay your water bill in under 30 seconds.
          </div>
          <div style={{ fontSize: 15, opacity: .85, lineHeight: 1.6 }}>
            Track usage across all your connections, set up auto-pay, raise complaints, and download bills — anytime, from anywhere.
          </div>

          <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 14, marginTop: 36 }}>
            {[
              { i: "bolt", t: "Instant", s: "Real-time bill view" },
              { i: "shield-alt", t: "Secure", s: "256-bit encrypted" },
              { i: "leaf", t: "Paperless", s: "All bills as PDF" },
              { i: "headset", t: "24×7 support", s: "Hindi + English" },
            ].map(f => (
              <div key={f.t} style={{
                background: "rgba(255,255,255,.10)",
                backdropFilter: "blur(8px)",
                border: "1px solid rgba(255,255,255,.15)",
                padding: "14px 14px",
                borderRadius: 12,
              }}>
                <i className={`fa fa-${f.i}`} style={{ fontSize: 16, opacity: .9 }}></i>
                <div style={{ fontSize: 13.5, fontWeight: 600, marginTop: 8 }}>{f.t}</div>
                <div style={{ fontSize: 11.5, opacity: .75, marginTop: 2 }}>{f.s}</div>
              </div>
            ))}
          </div>
        </div>

        <div style={{ position: "relative", zIndex: 2, fontSize: 11.5, opacity: .65 }}>
          A Government of Uttar Pradesh initiative · Noida Authority
        </div>

        <WaveDeco className="wave-deco"/>
      </div>

      <div className="login-form-wrap">
        <div className="login-card">
          <div style={{ display: "flex", alignItems: "center", gap: 12, marginBottom: 6 }}>
            <BrandMark size={46}/>
            <div style={{ lineHeight: 1.1 }}>
              <div style={{ fontSize: 18, fontWeight: 700 }}>Sign in</div>
              <div style={{ fontSize: 12, color: "var(--ink3)", marginTop: 4 }}>Welcome back to NoidaJal</div>
            </div>
          </div>

          {step === 1 && (
            <>
              <div className="tab-segments" style={{ marginTop: 26 }}>
                <div className={`tab-seg ${method==="mobile"?"active":""}`} onClick={() => setMethod("mobile")}>Mobile + OTP</div>
                <div className={`tab-seg ${method==="consumer"?"active":""}`} onClick={() => setMethod("consumer")}>Consumer ID</div>
                <div className={`tab-seg ${method==="email"?"active":""}`} onClick={() => setMethod("email")}>Email</div>
              </div>

              <div style={{ marginTop: 22, display: "flex", flexDirection: "column", gap: 16 }}>
                {method === "mobile" && (
                  <div className="field">
                    <label className="field-label">Mobile number</label>
                    <div className="input-with-icon">
                      <i className="fa fa-mobile-alt"></i>
                      <input className="input" value={mobile} onChange={e => setMobile(e.target.value)} placeholder="98765 43210"/>
                    </div>
                  </div>
                )}
                {method === "consumer" && (
                  <div className="field">
                    <label className="field-label">Consumer ID</label>
                    <div className="input-with-icon">
                      <i className="fa fa-id-card"></i>
                      <input className="input mono" value={consumerId} onChange={e => setConsumerId(e.target.value)} placeholder="NJL-XX-XXXXXX"/>
                    </div>
                    <div style={{ fontSize: 11.5, color: "var(--ink3)" }}>Find this on your last water bill, top-right corner.</div>
                  </div>
                )}
                {method === "email" && (
                  <>
                    <div className="field">
                      <label className="field-label">Email address</label>
                      <div className="input-with-icon">
                        <i className="fa fa-envelope"></i>
                        <input className="input" value={email} onChange={e => setEmail(e.target.value)}/>
                      </div>
                    </div>
                    <div className="field">
                      <label className="field-label">Password</label>
                      <div className="input-with-icon">
                        <i className="fa fa-lock"></i>
                        <input className="input" type="password" value={password} onChange={e => setPassword(e.target.value)}/>
                      </div>
                    </div>
                  </>
                )}

                <button className="btn btn-primary btn-lg btn-block" onClick={submitFirstStep}>
                  {method === "email" ? "Sign in" : "Send OTP"}
                  <i className="fa fa-arrow-right"></i>
                </button>

                {method !== "email" && (
                  <div style={{ fontSize: 12, color: "var(--ink3)", textAlign: "center" }}>
                    By continuing you agree to NoidaJal's <a href="#">Terms</a> & <a href="#">Privacy Policy</a>.
                  </div>
                )}
              </div>
            </>
          )}

          {step === 2 && (
            <>
              <div style={{ marginTop: 26 }}>
                <div style={{ fontSize: 13.5, color: "var(--ink2)" }}>
                  Enter the 6-digit code sent to{" "}
                  <strong className="mono">+91 {mobile}</strong>
                  <button className="btn btn-ghost" style={{ padding: "2px 6px", fontSize: 12 }} onClick={() => setStep(1)}>
                    <i className="fa fa-pen" style={{ fontSize: 10 }}></i> Edit
                  </button>
                </div>

                <div style={{ display: "flex", gap: 10, marginTop: 22 }}>
                  {otp.map((v, i) => (
                    <input key={i} id={`otp-${i}`}
                      className="input mono"
                      style={{ flex: 1, textAlign: "center", fontSize: 22, fontWeight: 600, padding: "14px 0", letterSpacing: "0" }}
                      maxLength={1}
                      value={v}
                      onChange={e => handleOtp(i, e.target.value)}
                    />
                  ))}
                </div>

                <div style={{ fontSize: 12.5, color: "var(--ink3)", marginTop: 14, textAlign: "center" }}>
                  Didn't receive the code? <a href="#">Resend in 0:24</a>
                </div>

                <button className="btn btn-primary btn-lg btn-block" style={{ marginTop: 24 }} onClick={onLogin}>
                  Verify & continue
                  <i className="fa fa-arrow-right"></i>
                </button>
              </div>
            </>
          )}

          <div className="divider" style={{ margin: "26px 0" }}></div>
          <div style={{ fontSize: 12.5, color: "var(--ink3)", textAlign: "center" }}>
            New to NoidaJal? <a href="#" style={{ fontWeight: 600 }}>Register your connection</a>
          </div>
        </div>
      </div>
    </div>
  );
};

window.LoginScreen = LoginScreen;
