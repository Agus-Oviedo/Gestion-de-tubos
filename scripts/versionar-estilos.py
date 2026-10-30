"""Versiona los CSS de una publicación según su contenido, sin cambiar el código fuente."""
import hashlib
from pathlib import Path
import re
import sys

root = Path(sys.argv[1]).resolve()
index = root / "index.html"


def versionar(match):
    relative = match.group(1)
    stylesheet = (root / relative).resolve()
    if not stylesheet.is_relative_to(root) or not stylesheet.is_file():
        raise ValueError(f"Hoja de estilos no encontrada en la publicación: {relative}")
    digest = hashlib.sha256(stylesheet.read_bytes()).hexdigest()[:16]
    return f'href="{relative}?v={digest}"'


html, cantidad = re.subn(r'href="([^"?:]+\.css)(?:\?[^" ]*)?"', versionar,
                         index.read_text(encoding="utf-8"))
if cantidad != 3:
    raise ValueError(f"Se esperaban 3 hojas de estilos y se encontraron {cantidad}")
index.write_text(html, encoding="utf-8")
print(f"{cantidad} hojas de estilos versionadas según su contenido.")
