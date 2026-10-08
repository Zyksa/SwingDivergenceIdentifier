"""Create deterministic parity fixtures directly from the user's reference implementation.

Only imports core/divergence.py (stdlib-only), never core/config.py or its .env loader.
"""
import importlib.util
import json
import random
import sys
from pathlib import Path
from types import SimpleNamespace
import hashlib

if len(sys.argv) != 2:
    raise SystemExit("Usage: python tests/generate_reference.py /path/to/reference/divergence.py")
source = Path(sys.argv[1]).resolve()
if not source.is_file():
    raise SystemExit("Reference implementation not found.")
spec = importlib.util.spec_from_file_location("reference_divergence", source)
module = importlib.util.module_from_spec(spec)
sys.modules[spec.name] = module
spec.loader.exec_module(module)
rng = random.Random(431)
rows, candles = [], []
previous, cvd = 100.0, 0.0
for index in range(600):
    close = previous + rng.randint(-16, 16) * 0.25
    high = max(previous, close) + rng.randint(0, 7) * 0.25
    low = min(previous, close) - rng.randint(0, 7) * 0.25
    delta = float(rng.randint(-50, 50))
    max_delta = max(0, delta) + rng.randint(0, 30)
    min_delta = min(0, delta) - rng.randint(0, 30)
    rows.append(dict(index=index, open=previous, high=high, low=low, close=close,
                     delta=delta, maxDelta=max_delta, minDelta=min_delta))
    candles.append(SimpleNamespace(time=1700000000 + index * 30, open=previous,
        high=high, low=low, close=close, cvd_open=cvd, cvd_close=cvd+delta,
        cvd_high=cvd+max_delta, cvd_low=cvd+min_delta))
    previous, cvd = close, cvd + delta

tracker = module.SwingTracker(1.4, 3, 0.25)
swings, previous_type = [], {}
for index, candle in enumerate(candles):
    before_kind = tracker.kind
    report = tracker.feed(candles, index)
    if report is not None and report.ended is not None:
        high = before_kind == module.HIGH
        price = candles[report.ended].high if high else candles[report.ended].low
        old = previous_type.get(high)
        label = ("H" if high else "L") if old is None else (
            "HH" if price > old + 0.25 else "LH") if high else (
            "LL" if price < old - 0.25 else "HL")
        swings.append(dict(index=report.ended, confirmedAt=index, price=price, high=high, label=label))
        previous_type[high] = price

atr = module.calculate_atr(candles, 3)
filtered = module._structure_for_scale(candles, len(candles), module.MAJOR, 2, 2, 0.25, atr, 0.5)
detector = module.DivergenceDetector(module.DivergenceConfig(
    swing_atr=0.75, swing_atr_period=3, require_confirmed_swing=True,
    pivot_right_bars=0, max_bars_between=11, include_weak_signals=False,
    require_clear_price_line=True, require_clear_cvd_line=True,
), tick_size=0.25)
detector.update(candles)
default_detector = module.DivergenceDetector(module.DivergenceConfig(), tick_size=0.25)
default_detector.update(candles)
def setup_rows(d):
    return [dict(first=s.first_index, second=s.second_index, confirmedAt=s.confirmed_at_index,
        type=s.type, price1=s.price1, price2=s.price2, cvd1=s.cvd1, cvd2=s.cvd2)
        for s in d.setups]
payload = dict(source="phidias-dxfeed/core/divergence.py", sourceSha256=hashlib.sha256(source.read_bytes()).hexdigest(),
    bars=rows, atr=atr, rsi=module.calculate_rsi(candles, 2),
    macd=module.calculate_macd(candles, 3, 5, 2)["histogram"],
    atrSwings=swings,
    cvdSetups=setup_rows(detector), defaultCvdSetups=setup_rows(default_detector),
    filtered=[dict(index=p.index, confirmedAt=p.confirmed_at_index, price=p.price,
        high=p.kind==module.HIGH, label={"high":"H", "low":"L"}.get(p.label,p.label)) for p in filtered])
output = Path(__file__).parent / "fixtures" / "reference.json"
output.parent.mkdir(exist_ok=True)
output.write_text(json.dumps(payload, separators=(",", ":")), encoding="utf-8")
print(f"Reference fixture: {len(rows)} bars, {len(swings)} ATR swings, {len(filtered)} filtered pivots, {len(detector.setups)} CVD setups.")
