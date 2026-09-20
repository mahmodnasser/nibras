# ADR-0007: Generate PDFs with Gotenberg, from HTML

- **Status:** Proposed, awaiting product owner confirmation
- **Date:** 2026-09-19
- **Requirement IDs:** REQ-DOC-001

## Context

Report cards, certificates, invoices and receipts must render correctly in Arabic, which means correct shaping, right-to-left layout and bidirectional text. QuestPDF is revenue-gated and iText is AGPL for linked code, so both are excluded.

## Decision

Render HTML to PDF with Gotenberg as a container. Bundle Inter, IBM Plex Sans Arabic and a Noto Naskh fallback in both the Documents image and the Gotenberg container. Use PDFsharp or MigraDoc only for simple documents with no complex text.

A bilingual snapshot test compares every generated document against a committed baseline, so a shaping regression fails the build rather than reaching a parent.

## Alternatives considered

- **A .NET PDF library.** Rejected on licence, and separately because Arabic shaping in the available options is weaker than a browser engine.
- **A headless browser we operate ourselves.** Rejected: that is what Gotenberg already is, maintained.

## Consequences

- Document generation depends on a container being available, so Documents degrades rather than fails when it is not, and the job reports why.
- Templates are HTML and CSS, which means the design system and the document templates share a styling language. That is an advantage, and it is also why logical CSS properties are mandatory.
