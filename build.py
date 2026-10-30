"""Genera web/puente-cuantico.html (artifact) inyectando el SQL inicial en la plantilla."""
from pathlib import Path

root = Path(__file__).parent
body = (root / "web/template.html").read_text()
seed = (root / "01_base_puente.sql").read_text()
assert "</script" not in seed.lower()
(root / "web/puente-cuantico.html").write_text(body.replace("__SEED_SQL__", seed))
print("ok")
