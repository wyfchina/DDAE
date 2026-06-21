from pypdf import PdfReader
import re

pdf = r"C:\Users\吴一帆\Documents\BaiduSyncdisk\日常资料\待阅读\The Demand Driven Adaptive Enterprise.pdf"
reader = PdfReader(pdf)
text = "\n".join((reader.pages[i - 1].extract_text() or "") for i in range(105, 134))

patterns = [
    "The Tactical Relevant Range",
    "Model Performance",
    "Tactical Reconciliation",
    "Tactical Exploitation",
    "Strategic Recommendation",
    "Strategic Projection",
    "Variance Analysis",
    "DDOM Master",
    "Projected Buffer",
]

for pattern in patterns:
    print(f"\nPAT {pattern}")
    match = re.search(pattern, text, re.I)
    if match:
        start = max(0, match.start() - 700)
        end = min(len(text), match.end() + 1200)
        print(re.sub(r"\s+", " ", text[start:end])[:1800])
