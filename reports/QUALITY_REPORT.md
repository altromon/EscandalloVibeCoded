# 📊 Formal Quality and Release Gate Report (AI-SDLC)

> **Generation Date:** 2026-10-05T21:36:46.742Z
> **Release Gate Verdict:** 🟢 APPROVED (RELEASE READY)
> **Global Rating:** **`F`** (MI Index: 36.8/100, Average CC: 13)

---

## 1. Executive Metric Summary

| Key Metric | Measured Value | Policy Threshold | Compliance |
| :--- | :---: | :---: | :---: |
| **Analyzed Files** | `7` | N/A | ℹ️ |
| **Evaluated Functions** | `7` | N/A | ℹ️ |
| **Lines of Code (LOC)** | `915` | N/A | ℹ️ |
| **Cyclomatic Complexity (Average)** | `13` | $\le 10$ | ❌ EXCEEDED |
| **Cognitive Complexity (Average)** | `14.9` | $\le 15$ | ✅ COMPLIANT |
| **Maintainability Index (SEI MI)** | `36.8 / 100` | $\ge 50$ | ❌ INSUFFICIENT |
| **Functions in Violation** | `7` | $0$ (Mode PERMISSIVE) | ❌ BLOCKED |

---

## 2. Polyglot Breakdown by Language Ecosystem

| Language | Functions | Total LOC | Average MI | Average CC | Rating |
| :--- | :---: | :---: | :---: | :---: | :---: |
| **C#** | `7` | `915` | `36.8` | `13` | `F` |

---

## 3. Quality Gate Violations Log

- **`EscandalloWeb/Data/AppDbContext.cs`** [`main_module`]: Maintainability Index 47.2 below minimum of 50
- **`EscandalloWeb/Data/ExcelTemplateSeeder.cs`** [`main_module`]: Maintainability Index 35.7 below minimum of 50, Function lines 133 exceeds maximum of 40, Función extensa (133 líneas > límite 40)
- **`EscandalloWeb/Models/Entities.cs`** [`main_module`]: Cyclomatic Complexity 16 exceeds threshold of 10, Maintainability Index 42.5 below minimum of 50, Function lines 115 exceeds maximum of 40, Función extensa (115 líneas > límite 40)
- **`EscandalloWeb/Program.cs`** [`main_module`]: Maintainability Index 44.9 below minimum of 50, Function lines 70 exceeds maximum of 40, Función extensa (70 líneas > límite 40)
- **`EscandalloWeb/Services/ExportadorEscandallo.cs`** [`main_module`]: Cyclomatic Complexity 29 exceeds threshold of 10, Cognitive Complexity 51 exceeds threshold of 15, Maintainability Index 21.3 below minimum of 50, Function lines 290 exceeds maximum of 40, Función extensa (290 líneas > límite 40)
- **`EscandalloWeb/Services/SesionUsuarioService.cs`** [`main_module`]: Cyclomatic Complexity 35 exceeds threshold of 10, Cognitive Complexity 45 exceeds threshold of 15, Maintainability Index 19.7 below minimum of 50, Function lines 182 exceeds maximum of 40, Función extensa (182 líneas > límite 40)
- **`EscandalloWeb.Tests/UnitTest1.cs`** [`main_module`]: Maintainability Index 46.6 below minimum of 50, Function lines 93 exceeds maximum of 40, Función extensa (93 líneas > límite 40)

---

## 4. Evaluation Criteria and Standards
- **McCabe Cyclomatic Complexity (CC)**: Number of linearly independent paths.
- **Maintainability Index (SEI MI)**: Normalized formula [0 - 100] combining Halstead Volume, CC, and LOC.
- **Clean Code Guardrails**: Prohibition of implicit `any` typing, function length limits ($le 40$ lines), and zero unjustified suppressions.