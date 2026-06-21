from pypdf import PdfReader
import re

pdf = r"C:\Users\吴一帆\Documents\BaiduSyncdisk\日常资料\待阅读\The Demand Driven Adaptive Enterprise.pdf"
reader = PdfReader(pdf)

ranges = [
    (100, 135, "Ch5 DDS&OP"),
    (136, 169, "Ch6 Adaptive S&OP"),
    (180, 196, "Ch8 Software/Compliance"),
    (216, 219, "Appendix A DDS&OP skill buffers"),
]

terms = [
    "six basic elements",
    "master settings",
    "variance analysis",
    "tactical exploitation",
    "strategic recommendation",
    "strategic projection",
    "relevant range",
    "buffer status",
    "capacity",
    "financial",
    "Adaptive S&OP process manages",
    "seven steps",
    "software must",
    "skill buffer",
]

for start, end, label in ranges:
    text = " ".join((reader.pages[i - 1].extract_text() or "").replace("\n", " ") for i in range(start, end + 1))
    print(f"\n--- {label} pages {start}-{end} chars={len(text)}")
    for term in terms:
        match = re.search(term, text, re.I)
        if not match:
            continue
        s = max(0, match.start() - 260)
        e = min(len(text), match.end() + 520)
        snippet = re.sub(r"\s+", " ", text[s:e])
        print(f"TERM {term}: {snippet[:1000]}")
