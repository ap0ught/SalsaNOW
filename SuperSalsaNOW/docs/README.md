# SuperSalsaNOW Documentation

Welcome to the SuperSalsaNOW documentation! This directory contains comprehensive guides for understanding, developing, and testing the SuperSalsaNOW mod manager.

---

## Documentation Index

### Core Documentation

| Document | Description | Audience |
|----------|-------------|----------|
| [MVP_SPEC.md](MVP_SPEC.md) | **MVP Specification** - Product requirements, user stories, success criteria | Product, Development, QA |
| [ARCHITECTURE.md](ARCHITECTURE.md) | **Architecture Guide** - System design, components, data flow, design decisions | Developers, Contributors |
| [DEVELOPMENT.md](DEVELOPMENT.md) | **Developer Guide** - Setup, workflows, conventions, common tasks | Developers, Contributors |
| [WINDOWS_TESTING.md](WINDOWS_TESTING.md) | **Windows Testing Procedures** - Testing checklist, validation, reporting | QA, Testers |

---

## Quick Navigation

### For New Users

**Start Here:** [../README.md](../README.md) (Project Overview)

1. **Understand What SuperSalsaNOW Does**
   - Read [MVP_SPEC.md](MVP_SPEC.md) → User Stories section

2. **Install and Use the Tool**
   - Follow [../README.md](../README.md) → Quick Start section

3. **Report Issues**
   - See [WINDOWS_TESTING.md](WINDOWS_TESTING.md) → Reporting Results section

### For Developers

**Start Here:** [DEVELOPMENT.md](DEVELOPMENT.md)

1. **Understand the Architecture**
   - Read [ARCHITECTURE.md](ARCHITECTURE.md) → System Overview

2. **Set Up Development Environment**
   - Follow [DEVELOPMENT.md](DEVELOPMENT.md) → Getting Started

3. **Make Your First Contribution**
   - Read [DEVELOPMENT.md](DEVELOPMENT.md) → Adding New Mods
   - See [../README.md](../README.md) → Contributing section

4. **Build and Test**
   - Follow [DEVELOPMENT.md](DEVELOPMENT.md) → Building and Testing
   - Use [WINDOWS_TESTING.md](WINDOWS_TESTING.md) for validation

### For Testers

**Start Here:** [WINDOWS_TESTING.md](WINDOWS_TESTING.md)

1. **Understand What to Test**
   - Read [WINDOWS_TESTING.md](WINDOWS_TESTING.md) → Features Requiring Windows Validation

2. **Set Up Test Environment**
   - Follow [WINDOWS_TESTING.md](WINDOWS_TESTING.md) → Testing Prerequisites

3. **Run Test Procedures**
   - Follow [WINDOWS_TESTING.md](WINDOWS_TESTING.md) → Testing Procedure

4. **Report Results**
   - Use [WINDOWS_TESTING.md](WINDOWS_TESTING.md) → Reporting Results templates

### For Product/Project Managers

**Start Here:** [MVP_SPEC.md](MVP_SPEC.md)

1. **Understand Product Vision**
   - Read [MVP_SPEC.md](MVP_SPEC.md) → Project Overview & Goals

2. **Review User Stories**
   - Read [MVP_SPEC.md](MVP_SPEC.md) → MVP User Stories

3. **Track Progress**
   - See [../README.md](../README.md) → Roadmap section

4. **Understand Technical Decisions**
   - Read [ARCHITECTURE.md](ARCHITECTURE.md) → Design Decisions

---

## Document Details

### MVP_SPEC.md

**Purpose:** Define what SuperSalsaNOW does and how it should work

**Contents:**
- Project overview and goals
- User stories with acceptance criteria
- Architecture diagrams
- Data models (manifest structure)
- Core interfaces
- MVP workflow (end-to-end process)
- Configuration details
- Testing strategy
- Extension points for future phases
- Success metrics

**When to Read:**
- Before starting development (understand requirements)
- When planning new features (check against MVP scope)
- When writing tests (verify acceptance criteria)

**Key Sections:**
- **User Stories** - What users can do with the tool
- **Workflow** - Step-by-step process flow
- **Extension Points** - Phase 2+ features (dependency resolution, conflict detection)

---

### ARCHITECTURE.md

**Purpose:** Explain how SuperSalsaNOW is built and why design decisions were made

**Contents:**
- High-level system architecture
- Project structure and file organization
- Core components (manifest system, mod installation pipeline, DI container)
- Data flow diagrams
- Interface contracts (IModInstaller, INexusClient, etc.)
- Extension points for future features
- Technology stack
- Design decisions with rationale
- Deployment architecture
- Security and performance considerations

**When to Read:**
- Before making code changes (understand system design)
- When adding new features (follow established patterns)
- When debugging (understand component interactions)
- During code review (validate design consistency)

**Key Sections:**
- **Core Components** - Manifest system, installation pipeline
- **Interface Contracts** - How services communicate
- **Design Decisions** - Why we chose .NET, separate Core/Windows libs, etc.

---

### DEVELOPMENT.md

**Purpose:** Help developers set up, build, and contribute to the project

**Contents:**
- Prerequisites (.NET SDK, tools)
- Getting started (clone, restore, build, run)
- macOS development workflow (what works, what doesn't)
- Build, test, and publish commands
- Adding new mods to manifest
- Extending the system (new strategies, commands, platforms)
- Debugging with VS Code and Visual Studio
- Code conventions (naming, logging, error handling)
- Common development tasks
- Troubleshooting guide

**When to Read:**
- First time contributing (setup instructions)
- Adding new features (follow conventions)
- Stuck on a build/test issue (troubleshooting)
- Publishing a release (publish commands)

**Key Sections:**
- **Getting Started** - Step-by-step setup
- **Adding New Mods** - How to extend manifest
- **Code Conventions** - Naming, logging, async patterns
- **Troubleshooting** - Common errors and solutions

---

### WINDOWS_TESTING.md

**Purpose:** Guide testers through validating Windows-specific features

**Contents:**
- Why Windows testing is required
- Features requiring Windows validation
- Testing prerequisites (Windows version, accounts, software)
- Step-by-step testing procedure
- Validation checklist (track progress)
- Log collection procedures
- Reporting results (success/failure templates)
- Known macOS limitations
- Troubleshooting test failures

**When to Read:**
- Before testing on Windows (understand what to test)
- During testing (follow step-by-step procedures)
- After testing (report results using templates)
- When test fails (troubleshooting section)

**Key Sections:**
- **Testing Procedure** - 6 phases from config to game launch
- **Validation Checklist** - Track what's been tested
- **Reporting Results** - Templates for success/failure reports

---

## Documentation Conventions

### Markdown Formatting

- **Headers:** Use `#` for main sections, `##` for subsections
- **Code Blocks:** Use triple backticks with language identifier
  ```bash
  dotnet build
  ```
- **Tables:** Use for structured data (features, configurations, etc.)
- **Diagrams:** Use ASCII art for architecture diagrams
- **Links:** Use relative links between docs

### File Organization

```
docs/
├── README.md (this file)       # Documentation index
├── MVP_SPEC.md                 # Product specification
├── ARCHITECTURE.md             # Technical architecture
├── DEVELOPMENT.md              # Developer guide
└── WINDOWS_TESTING.md          # Testing procedures
```

### Document Structure

Each document follows this structure:

1. **Title and Introduction**
2. **Table of Contents** (for long docs)
3. **Main Content** (organized by sections)
4. **References/Resources** (if applicable)
5. **Last Updated Date**

---

## Contributing to Documentation

### When to Update Documentation

**Update docs when:**
- Adding new features (update ARCHITECTURE.md, DEVELOPMENT.md)
- Changing APIs/interfaces (update ARCHITECTURE.md)
- Modifying build process (update DEVELOPMENT.md)
- Discovering new issues (update DEVELOPMENT.md troubleshooting)
- Changing test procedures (update WINDOWS_TESTING.md)

### How to Update Documentation

1. **Edit Markdown Files**
   - Use any text editor
   - Follow existing formatting conventions

2. **Test Links**
   - Verify relative links work
   - Check code examples are valid

3. **Update "Last Updated" Date**
   - Add/update date at bottom of document

4. **Submit PR**
   - Include "Documentation update" in PR title
   - Explain what was changed and why

---

## Documentation Roadmap

### Phase 1 (Current)
- [x] MVP specification
- [x] Architecture guide
- [x] Development guide
- [x] Windows testing procedures
- [x] Documentation index (this file)

### Phase 2
- [ ] API reference (generated from XML doc comments)
- [ ] Troubleshooting FAQ
- [ ] Video tutorials (installation, configuration)
- [ ] Manifest authoring guide (for community contributors)

### Phase 3
- [ ] Performance tuning guide
- [ ] Security best practices
- [ ] Deployment guide (releases, distribution)
- [ ] Community contribution guide

---

## Additional Resources

### External Documentation

- **.NET Documentation**: https://learn.microsoft.com/en-us/dotnet/
- **Spectre.Console**: https://spectreconsole.net/
- **Nexus Mods API**: https://app.swaggerhub.com/apis-docs/NexusMods/nexus-mods_public_api_params_in_form_data/1.0
- **DepotDownloader**: https://github.com/SteamRE/DepotDownloader

### Project Links

- **GitHub Repository**: https://github.com/psingley/SuperSalsaNOW
- **Issue Tracker**: https://github.com/psingley/SuperSalsaNOW/issues
- **Discussions**: https://github.com/psingley/SuperSalsaNOW/discussions

---

## Documentation Feedback

Have suggestions for improving these docs?

- **Open an Issue**: [Documentation Improvement](https://github.com/psingley/SuperSalsaNOW/issues/new?labels=documentation)
- **Submit a PR**: Fix typos, add examples, clarify explanations
- **Ask Questions**: Use [GitHub Discussions](https://github.com/psingley/SuperSalsaNOW/discussions)

---

*Documentation Last Updated: 2025-11-09*
