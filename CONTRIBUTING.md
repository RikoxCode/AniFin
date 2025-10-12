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

### Testing Your Changes

1. Copy the compiled DLL to your Jellyfin plugins directory
2. Restart Jellyfin
3. Test your changes thoroughly
4. Check the logs for any errors

## Style Guidelines

### Git Commit Messages

- Use the present tense ("Add feature" not "Added feature")
- Use the imperative mood ("Move cursor to..." not "Moves cursor to...")
- Limit the first line to 72 characters or less
- Reference issues and pull requests liberally after the first line

Example:
```
Add support for batch downloads

- Implement queue system for multiple series
- Add UI controls for batch operations
- Update documentation

Fixes #123
```

### C# Style Guide

- Follow Microsoft's C# Coding Conventions
- Use meaningful variable and method names
- Add XML documentation comments to public methods
- Keep methods focused and small
- Use async/await for asynchronous operations

### Documentation

- Update the README.md if you change functionality
- Comment your code where necessary
- Update API documentation for any endpoint changes

## Project Structure

```
AniFin/
├── Jellyfin.Plugin.AniFin/
│   ├── Api/                  # API Controllers
│   ├── Configuration/        # Plugin configuration
│   ├── Modules/             # Core functionality
│   │   ├── DownloadExecuter.cs
│   │   └── DownloadQueue.cs
│   ├── Web/                 # Web interface files
│   └── Plugin.cs            # Main plugin class
└── README.md
```

## Testing

### Manual Testing Checklist

- [ ] Plugin installs correctly
- [ ] Configuration page loads and saves settings
- [ ] Downloads can be added to queue
- [ ] Queue processes downloads correctly
- [ ] Files are organized properly
- [ ] Browser extension works (if modified)
- [ ] Logs are generated correctly

## Questions?

Feel free to open an issue with the label `question` if you have any questions about contributing!

## License

By contributing, you agree that your contributions will be licensed under the same license as the project.
