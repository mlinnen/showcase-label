### 2026-09-06T13-07-07: Issue #24 batch contract and implementation boundary
**By:** Mr. Pink
**What:** Issue #24 batch contract and implementation boundary
**References:** GitHub issue #24, GitHub issue #32, PR #33, Mr. Blonde (.NET implementation), Mr. Orange (test/QA)
**Why:** ### 2026-09-06: Issue #24 batch contract and implementation boundary
**By:** Mr. Pink
**What:** #24 adds an explicit default single-carver/range mode. The range is the inclusive Cartesian product of positive numeric carver and entry bounds, ordered carver-major then entry-ascending. A configurable `Maximum labels per batch` field defaults to 15; it is a preflight safety limit, not an alternate sequencing input. Reject a batch whose calculated count exceeds it before opening the printer. Equal start/end bounds are valid. Both modes create the same print-job sequence and use one USB open/write/close cycle.
**Why:** The user-supplied endpoints remain the sole source of the labels to generate, preserving the specified Cartesian-product semantics. Naming the configurable 15 setting as a maximum resolves the otherwise-conflicting independent quantity input and guards against accidental very large print jobs. Pure validation and job-sequencing helpers enable deterministic tests without printer I/O.
**Boundary:** Division removal is excluded from #24 and remains under #32/PR #33. #24 must neither add nor alter division UI, label text, QR data, or associated tests.