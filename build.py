"""Genera web/puente-cuantico.html (artifact) e index.html (GitHub Pages) inyectando el SQL inicial."""
from pathlib import Path

root = Path(__file__).parent
body = (root / "web/template.html").read_text()
seed = (root / "01_base_puente.sql").read_text()
assert "</script" not in seed.lower()
body = body.replace("__SEED_SQL__", seed)
(root / "web/puente-cuantico.html").write_text(body)

# GitHub Pages necesita el documento completo (el artifact lo envuelve solo)
style_end = body.index("</style>") + len("</style>")
page = ('<!doctype html>\n<html lang="es">\n<head>\n' + body[:style_end]
        + "\n</head>\n<body>\n" + body[style_end:] + "\n</body>\n</html>\n")
(root / "index.html").write_text(page)
print("ok")
