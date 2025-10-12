# Contributing to AniFin

First off, thank you for considering contributing to AniFin! It's people like you that make AniFin such a great tool.

## Code of Conduct

This project and everyone participating in it is governed by our Code of Conduct. By participating, you are expected to uphold this code.

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check the existing issues as you might find out that you don't need to create one. When you are creating a bug report, please include as many details as possible using our bug report template.

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion, please include:

- A clear and descriptive title
- A detailed description of the proposed feature
- Explain why this enhancement would be useful
- List any alternatives you've considered

### Pull Requests

1. Fork the repo and create your branch from `main`
2. If you've added code that should be tested, add tests
3. Ensure the test suite passes
4. Make sure your code follows the existing style
5. Write a clear commit message
6. Open a pull request!

## Development Setup

### Prerequisites

- .NET 6.0 SDK or higher
- Jellyfin 10.8.0 or higher (for testing)
- AniWorld CLI installed

### Building the Plugin
```bash
# Clone your fork
git clone https://github.com/YOUR-USERNAME/AniFin.git
cd AniFin

# Build the project
dotnet build

# Run tests (if available)
dotnet test
```
