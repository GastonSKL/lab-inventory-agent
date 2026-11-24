# Lab Inventory Agent

An LLM agent that answers questions and performs actions on a laboratory inventory API (products, stock, lots, expiration dates, and purchase orders).

> **Status:** work in progress. Built in public, one phase at a time.
> All data in this repository is **synthetic**.

## What it will do

- **Read tools:** search products, check stock, list lots close to expiring.
- **Write tools:** create restock orders, adjust stock — always behind explicit human confirmation.
- **MCP server:** the same tools exposed over the Model Context Protocol (C#).
- **RAG:** questions over synthetic technical sheets and procedures.
- **Evals:** a regression suite (correct answers, refusals, prompt-injection attempts, wrong-tool cases) running in CI.

## Design principles

| Step | Kind | Note |
|---|---|---|
| Validate parameters, permissions, business rules | Deterministic | Never delegated to the LLM |
| Execute queries and writes | Deterministic | The LLM only proposes the call |
| Interpret user intent | Non-deterministic | LLM |
| Write the answer | Non-deterministic | LLM, grounded only on tool results |
| Write actions | Human in the loop | Explicit confirmation before executing |

## Stack

- .NET Minimal API + SQLite (SQL Server later)
- Anthropic API for tool calling
- React for a minimal chat UI

## Project journal

- `BREAKAGE.md` — what broke, in what order, why, and how it was fixed.
- `docs/adr/` — architecture decision records.
- `EVALS.md` — eval metrics per run.

## Roadmap

- [ ] Phase 1 — Inventory API with synthetic data and tests
- [ ] Phase 2 — Agent with read-only tool calling, token/latency/cost logging
- [ ] Phase 3 — Write tools with confirmation, iteration and cost limits, MCP server
- [ ] Phase 4 — RAG, evals in GitHub Actions, architecture docs
