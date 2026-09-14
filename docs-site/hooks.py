"""Build hooks for the learning site.

TWO JOBS, BOTH SO THE DOCS FOLDER STAYS WRITTEN FOR GITHUB. The pages link into `src/` and `test/`
with relative paths, which work when the repository is browsed and point nowhere once the pages are
a site; and a page is L1 because it sits in `L1-novice/`, which a reader of the site cannot see.

1. A link that leaves `docs/` becomes a link to the same file on GitHub. A link to a folder inside
   `docs/` becomes a link to that folder's first page.
2. Every page in a level or section opens with a badge naming it, in that section's colour.
"""

import os
import posixpath
import re

REPO = "https://github.com/mlambda-net/MLambda.AI"
BRANCH = "master"

# `[text](target)` and `![alt](target)`; a target never contains a space or a closing parenthesis.
LINK = re.compile(r"(\]\()([^)\s]+)(\))")
FENCE = re.compile(r"^(```|~~~)")

# Folder -> (badge text, the page a bare folder link means).
SECTIONS = {
    "ideas": ("Ideas · Why it matters", "README.md"),
    "hilbert": ("Start here · How Hilbert works", "01-what-hilbert-is.md"),
    "L1-novice": ("Level 1 · Novice", "README.md"),
    "L2-practitioner": ("Level 2 · Practitioner", "README.md"),
    "L3-advanced": ("Level 3 · Advanced", "README.md"),
    "actuarial": ("Capstone · Actuarial", "README.md"),
}

_root = None


def on_config(config):
    global _root
    _root = os.path.dirname(os.path.abspath(config.config_file_path))
    return config


def _rewrite(target, page_dir):
    if re.match(r"^[a-zA-Z][a-zA-Z0-9+.-]*:", target) or target.startswith(("#", "/")):
        return target

    path, hash_, fragment = target.partition("#")
    if not path:
        return target

    resolved = posixpath.normpath(posixpath.join("docs", page_dir, path))
    on_disk = os.path.join(_root, *resolved.split("/"))

    if resolved == "docs" or resolved.startswith("docs/"):
        if os.path.isdir(on_disk):
            folder = resolved[len("docs/"):] if resolved != "docs" else ""
            first = SECTIONS.get(folder, (None, "README.md"))[1]
            relative = posixpath.relpath(posixpath.join("docs", folder, first), posixpath.join("docs", page_dir))
            return relative + (hash_ + fragment if fragment else "")
        return target

    kind = "tree" if os.path.isdir(on_disk) else "blob"
    return f"{REPO}/{kind}/{BRANCH}/{resolved}" + (hash_ + fragment if fragment else "")


def on_page_markdown(markdown, page, config, files):
    src = page.file.src_uri
    page_dir = posixpath.dirname(src)

    # LINKS INSIDE CODE ARE LEFT ALONE, so an example that shows markdown still shows it.
    out, fenced = [], False
    for line in markdown.split("\n"):
        if FENCE.match(line.lstrip()):
            fenced = not fenced
        elif not fenced:
            line = LINK.sub(lambda m: m.group(1) + _rewrite(m.group(2), page_dir) + m.group(3), line)
        out.append(line)
    markdown = "\n".join(out)

    # THE BADGE GOES UNDER THE TITLE, not above it: MkDocs names a page after its leading `# `
    # heading, and a badge in front of it would leave every page titled after its file name.
    section = src.split("/")[0] if "/" in src else None
    if section in SECTIONS:
        badge = f'<p class="ml-badge">{SECTIONS[section][0]}</p>'
        title = re.search(r"^# .*$", markdown, flags=re.MULTILINE)
        if title:
            markdown = markdown[: title.end()] + "\n\n" + badge + "\n" + markdown[title.end():]
        else:
            markdown = badge + "\n\n" + markdown

    return markdown
