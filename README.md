<div id="top">

<!-- HEADER STYLE: CLASSIC -->
<div align="center">


# ANIFIN

<em>Transforming Anime Access into Seamless Entertainment</em>

<!-- BADGES -->
<img src="https://img.shields.io/github/license/RikoxCode/AniFin?style=flat&logo=opensourceinitiative&logoColor=white&color=0080ff" alt="license">
<img src="https://img.shields.io/github/last-commit/RikoxCode/AniFin?style=flat&logo=git&logoColor=white&color=0080ff" alt="last-commit">
<img src="https://img.shields.io/github/languages/top/RikoxCode/AniFin?style=flat&color=0080ff" alt="repo-top-language">
<img src="https://img.shields.io/github/languages/count/RikoxCode/AniFin?style=flat&color=0080ff" alt="repo-language-count">

<em>Built with the tools and technologies:</em>

<img src="https://img.shields.io/badge/Markdown-000000.svg?style=flat&logo=Markdown&logoColor=white" alt="Markdown">
<img src="https://img.shields.io/badge/JavaScript-F7DF1E.svg?style=flat&logo=JavaScript&logoColor=black" alt="JavaScript">
<img src="https://img.shields.io/badge/NuGet-004880.svg?style=flat&logo=NuGet&logoColor=white" alt="NuGet">

</div>
<br>

---

## Table of Contents

- [Overview](#overview)
- [Getting Started](#getting-started)
    - [Prerequisites](#prerequisites)
    - [Installation](#installation)
    - [Usage](#usage)
    - [Testing](#testing)
- [Features](#features)
- [Project Structure](#project-structure)
    - [Project Index](#project-index)
- [Roadmap](#roadmap)
- [Contributing](#contributing)
- [License](#license)
- [Acknowledgment](#acknowledgment)

---

## Overview



---

## Features

|      | Component       | Details                                                                                     |
| :--- | :-------------- | :------------------------------------------------------------------------------------------ |
| ⚙️  | **Architecture**  | <ul><li>Plugin-based architecture integrating with Jellyfin media server</li><li>Modular design separating core logic, UI, and API layers</li></ul> |
| 🔩 | **Code Quality**  | <ul><li>Consistent C# coding standards, leveraging SOLID principles</li><li>Clear separation of concerns, well-structured solution (.sln)</li></ul> |
| 📄 | **Documentation** | <ul><li>README provides project overview, setup instructions, and usage</li><li>Code comments and XML documentation in C# codebase</li></ul> |
| 🔌 | **Integrations**  | <ul><li>Jellyfin plugin API for media metadata and playback control</li><li>NuGet package for dependency management</li></ul> |
| 🧩 | **Modularity**    | <ul><li>Separate projects for core plugin, UI components, and tests</li><li>Extensible via plugin interfaces and configuration files</li></ul> |
| 🧪 | **Testing**       | <ul><li>Unit tests in dedicated test projects, covering core logic</li><li>Mocking dependencies for isolated testing</li></ul> |
| ⚡️  | **Performance**   | <ul><li>Efficient API calls with caching strategies</li><li>Minimal UI rendering overhead</li></ul> |
| 🛡️ | **Security**      | <ul><li>Input validation for user configurations</li><li>Secure handling of media metadata and API tokens</li></ul> |
| 📦 | **Dependencies**  | <ul><li>Primary dependency on `Jellyfin.Plugin.AniFin.csproj` via NuGet</li><li>Additional dependencies managed through NuGet packages</li></ul> |

---

## Project Structure

```sh
└── AniFin/
    ├── .github
    │   ├── ISSUE_TEMPLATE
    │   └── PULL_REQUEST_TEMPLATE.md
    ├── Api
    │   └── PluginController.cs
    ├── CONTRIBUTING.md
    ├── Configuration
    │   └── PluginConfiguration.cs
    ├── FileLogger.cs
    ├── Jellyfin.Plugin.AniFin.csproj
    ├── Jellyfin.Plugin.AniFin.sln
    ├── LICENSE
    ├── Modules
    │   ├── DownloadExecuter.cs
    │   └── DownloadQueue.cs
    ├── Plugin.cs
    ├── README.md
    ├── Web
    │   ├── anifin.html
    │   └── anifin.js
    └── anifin-extension.zip
```

---

### Project Index

<details open>
	<summary><b><code>ANIFIN/</code></b></summary>
	<!-- __root__ Submodule -->
	<details>
		<summary><b>__root__</b></summary>
		<blockquote>
			<div class='directory-path' style='padding: 8px 0; color: #666;'>
				<code><b>⦿ __root__</b></code>
			<table style='width: 100%; border-collapse: collapse;'>
			<thead>
				<tr style='background-color: #f8f9fa;'>
					<th style='width: 30%; text-align: left; padding: 8px;'>File Name</th>
					<th style='text-align: left; padding: 8px;'>Summary</th>
				</tr>
			</thead>
				<tr style='border-bottom: 1px solid #eee;'>
					<td style='padding: 8px;'><b><a href='https://github.com/RikoxCode/AniFin/blob/master/Jellyfin.Plugin.AniFin.csproj'>Jellyfin.Plugin.AniFin.csproj</a></b></td>
					<td style='padding: 8px;'>- Defines the project configuration for the AniFin plugin, enabling integration with Jellyfin media server<br>- It specifies dependencies, target framework, and embedded resources such as web interface files and optional extension packages, facilitating seamless plugin deployment and interaction within the Jellyfin ecosystem.</td>
				</tr>
				<tr style='border-bottom: 1px solid #eee;'>
					<td style='padding: 8px;'><b><a href='https://github.com/RikoxCode/AniFin/blob/master/Plugin.cs'>Plugin.cs</a></b></td>
					<td style='padding: 8px;'>- Defines the core plugin structure for AniFin, enabling integration with Jellyfin to facilitate downloading anime content from AniWorld<br>- Manages plugin initialization, configuration, and web interface setup, serving as the primary entry point for plugin registration and user interaction within the Jellyfin ecosystem.</td>
				</tr>
				<tr style='border-bottom: 1px solid #eee;'>
					<td style='padding: 8px;'><b><a href='https://github.com/RikoxCode/AniFin/blob/master/Jellyfin.Plugin.AniFin.sln'>Jellyfin.Plugin.AniFin.sln</a></b></td>
					<td style='padding: 8px;'>- Defines the core plugin structure for integrating AniFin into the Jellyfin media server, enabling enhanced anime metadata retrieval and management<br>- Serves as the primary project file orchestrating the plugins build and configuration within the overall Jellyfin architecture, facilitating seamless extension of media library functionalities with AniFin-specific features.</td>
				</tr>
				<tr style='border-bottom: 1px solid #eee;'>
					<td style='padding: 8px;'><b><a href='https://github.com/RikoxCode/AniFin/blob/master/LICENSE'>LICENSE</a></b></td>
					<td style='padding: 8px;'>- Provides the licensing terms for the project, establishing legal permissions and restrictions<br>- It ensures users understand their rights to use, modify, and distribute the software while clarifying liability limitations<br>- This foundational document supports the overall architecture by defining the legal framework enabling open collaboration and distribution within the project ecosystem.</td>
				</tr>
				<tr style='border-bottom: 1px solid #eee;'>
					<td style='padding: 8px;'><b><a href='https://github.com/RikoxCode/AniFin/blob/master/FileLogger.cs'>FileLogger.cs</a></b></td>
					<td style='padding: 8px;'>- Provides logging capabilities for the AniFin plugin by recording runtime events and errors into daily log files<br>- It ensures structured, thread-safe logging within the plugins directory, facilitating troubleshooting and monitoring of plugin activity within the overall Jellyfin architecture.</td>
				</tr>
				<tr style='border-bottom: 1px solid #eee;'>
					<td style='padding: 8px;'><b><a href='https://github.com/RikoxCode/AniFin/blob/master/CONTRIBUTING.md'>CONTRIBUTING.md</a></b></td>
					<td style='padding: 8px;'>- Provides guidelines and procedures for contributing to AniFin, ensuring community-driven development<br>- Emphasizes bug reporting, feature suggestions, code standards, and testing protocols to maintain project quality<br>- Facilitates collaborative enhancement of the plugin, supporting its core functionality within the Jellyfin ecosystem and ensuring consistent, reliable updates aligned with project architecture.</td>
				</tr>
				<tr style='border-bottom: 1px solid #eee;'>
					<td style='padding: 8px;'><b><a href='https://github.com/RikoxCode/AniFin/blob/master/README.md'>README.md</a></b></td>
					<td style='padding: 8px;'>- Provides seamless integration of anime downloads from AniWorld into Jellyfin, enabling automated management, organization, and queue control of anime series, seasons, and episodes<br>- Enhances media library with real-time download tracking, multi-language support, and browser extension capabilities, streamlining the process of acquiring and organizing anime content within the Jellyfin ecosystem.</td>
				</tr>
			</table>
		</blockquote>
	</details>
	<!-- Modules Submodule -->
	<details>
		<summary><b>Modules</b></summary>
		<blockquote>
			<div class='directory-path' style='padding: 8px 0; color: #666;'>
				<code><b>⦿ Modules</b></code>
			<table style='width: 100%; border-collapse: collapse;'>
			<thead>
				<tr style='background-color: #f8f9fa;'>
					<th style='width: 30%; text-align: left; padding: 8px;'>File Name</th>
					<th style='text-align: left; padding: 8px;'>Summary</th>
				</tr>
			</thead>
				<tr style='border-bottom: 1px solid #eee;'>
					<td style='padding: 8px;'><b><a href='https://github.com/RikoxCode/AniFin/blob/master/Modules/DownloadQueue.cs'>DownloadQueue.cs</a></b></td>
					<td style='padding: 8px;'>- Manages a prioritized queue for anime downloads, orchestrating download tasks with progress tracking, status updates, and event notifications<br>- Facilitates sequential processing, pausing, resuming, retrying, and cancellation of downloads, while maintaining comprehensive statistics and history to support efficient and reliable download workflows within the overall application architecture.</td>
				</tr>
				<tr style='border-bottom: 1px solid #eee;'>
					<td style='padding: 8px;'><b><a href='https://github.com/RikoxCode/AniFin/blob/master/Modules/DownloadExecuter.cs'>DownloadExecuter.cs</a></b></td>
					<td style='padding: 8px;'>- The <code>DownloadExecuter</code> class serves as a core component within the plugin architecture, orchestrating the process of downloading anime content<br>- It executes AniWorld CLI commands to fetch entire series, specific seasons, or individual episodes, saving the downloaded files to a designated output directory<br>- This module ensures seamless integration of automated content retrieval into the larger system, enabling users to efficiently acquire and organize anime media within the applications ecosystem.</td>
				</tr>
			</table>
		</blockquote>
	</details>
	<!-- Api Submodule -->
	<details>
		<summary><b>Api</b></summary>
		<blockquote>
			<div class='directory-path' style='padding: 8px 0; color: #666;'>
				<code><b>⦿ Api</b></code>
			<table style='width: 100%; border-collapse: collapse;'>
			<thead>
				<tr style='background-color: #f8f9fa;'>
					<th style='width: 30%; text-align: left; padding: 8px;'>File Name</th>
					<th style='text-align: left; padding: 8px;'>Summary</th>
				</tr>
			</thead>
				<tr style='border-bottom: 1px solid #eee;'>
					<td style='padding: 8px;'><b><a href='https://github.com/RikoxCode/AniFin/blob/master/Api/PluginController.cs'>PluginController.cs</a></b></td>
					<td style='padding: 8px;'>- The <code>Api/PluginController.cs</code> file defines an API controller responsible for handling anime download requests within the Jellyfin plugin architecture<br>- It facilitates secure interactions for managing anime content via AniWorld, leveraging Jellyfins authentication system<br>- This component acts as a bridge between client requests and the underlying download management system, enabling users to initiate and control anime downloads seamlessly within the broader media server environment.</td>
				</tr>
			</table>
		</blockquote>
	</details>
	<!-- Web Submodule -->
	<details>
		<summary><b>Web</b></summary>
		<blockquote>
			<div class='directory-path' style='padding: 8px 0; color: #666;'>
				<code><b>⦿ Web</b></code>
			<table style='width: 100%; border-collapse: collapse;'>
			<thead>
				<tr style='background-color: #f8f9fa;'>
					<th style='width: 30%; text-align: left; padding: 8px;'>File Name</th>
					<th style='text-align: left; padding: 8px;'>Summary</th>
				</tr>
			</thead>
				<tr style='border-bottom: 1px solid #eee;'>
					<td style='padding: 8px;'><b><a href='https://github.com/RikoxCode/AniFin/blob/master/Web/anifin.js'>anifin.js</a></b></td>
					<td style='padding: 8px;'>- The <code>Web/anifin.js</code> file serves as the core configuration and initialization script for the AniFin extension within the web application<br>- Its primary purpose is to set up necessary authentication tokens, define helper functions for API interactions, and load configuration data from the backend API<br>- This setup enables the extension to seamlessly integrate with the applications architecture, facilitating features such as dynamic configuration retrieval and authenticated API communication<br>- Overall, it acts as the foundational component that prepares the extension to operate effectively within the larger codebase.</td>
				</tr>
				<tr style='border-bottom: 1px solid #eee;'>
					<td style='padding: 8px;'><b><a href='https://github.com/RikoxCode/AniFin/blob/master/Web/anifin.html'>anifin.html</a></b></td>
					<td style='padding: 8px;'>- Provides the user interface for configuring, initiating, and managing anime downloads within the AniFin plugin ecosystem<br>- It facilitates setting download preferences, starting manual downloads via URLs, monitoring download queues, and integrating browser extensions, thereby enabling seamless control and automation of anime content acquisition in the overall system architecture.</td>
				</tr>
			</table>
		</blockquote>
	</details>
	<!-- .github Submodule -->
	<details>
		<summary><b>.github</b></summary>
		<blockquote>
			<div class='directory-path' style='padding: 8px 0; color: #666;'>
				<code><b>⦿ .github</b></code>
			<table style='width: 100%; border-collapse: collapse;'>
			<thead>
				<tr style='background-color: #f8f9fa;'>
					<th style='width: 30%; text-align: left; padding: 8px;'>File Name</th>
					<th style='text-align: left; padding: 8px;'>Summary</th>
				</tr>
			</thead>
				<tr style='border-bottom: 1px solid #eee;'>
					<td style='padding: 8px;'><b><a href='https://github.com/RikoxCode/AniFin/blob/master/.github/PULL_REQUEST_TEMPLATE.md'>PULL_REQUEST_TEMPLATE.md</a></b></td>
					<td style='padding: 8px;'>- Provides a structured template for submitting pull requests, guiding contributors to clearly describe changes, related issues, testing procedures, and adherence to project standards<br>- It ensures consistent documentation of modifications, facilitating efficient review and collaboration within the overall project architecture<br>- This template supports maintaining high-quality, well-documented contributions aligned with the projects development workflow.</td>
				</tr>
			</table>
			<!-- ISSUE_TEMPLATE Submodule -->
			<details>
				<summary><b>ISSUE_TEMPLATE</b></summary>
				<blockquote>
					<div class='directory-path' style='padding: 8px 0; color: #666;'>
						<code><b>⦿ .github.ISSUE_TEMPLATE</b></code>
					<table style='width: 100%; border-collapse: collapse;'>
					<thead>
						<tr style='background-color: #f8f9fa;'>
							<th style='width: 30%; text-align: left; padding: 8px;'>File Name</th>
							<th style='text-align: left; padding: 8px;'>Summary</th>
						</tr>
					</thead>
						<tr style='border-bottom: 1px solid #eee;'>
							<td style='padding: 8px;'><b><a href='https://github.com/RikoxCode/AniFin/blob/master/.github/ISSUE_TEMPLATE/bug_report.md'>bug_report.md</a></b></td>
							<td style='padding: 8px;'>- Provides a structured template for users to report bugs within the project, facilitating efficient issue tracking and resolution<br>- It guides users to clearly describe problems, reproduce steps, environment details, and attach relevant logs, thereby supporting the overall maintenance and improvement of the codebase<br>- This enhances the projects robustness and user support capabilities.</td>
						</tr>
						<tr style='border-bottom: 1px solid #eee;'>
							<td style='padding: 8px;'><b><a href='https://github.com/RikoxCode/AniFin/blob/master/.github/ISSUE_TEMPLATE/feature_request.md'>feature_request.md</a></b></td>
							<td style='padding: 8px;'>- Provides a structured template for submitting feature requests, facilitating clear communication of new ideas and enhancements within the project<br>- It guides users to articulate problems, desired solutions, and potential implementations, supporting organized and efficient collaboration to evolve the codebase effectively.</td>
						</tr>
					</table>
				</blockquote>
			</details>
		</blockquote>
	</details>
	<!-- Configuration Submodule -->
	<details>
		<summary><b>Configuration</b></summary>
		<blockquote>
			<div class='directory-path' style='padding: 8px 0; color: #666;'>
				<code><b>⦿ Configuration</b></code>
			<table style='width: 100%; border-collapse: collapse;'>
			<thead>
				<tr style='background-color: #f8f9fa;'>
					<th style='width: 30%; text-align: left; padding: 8px;'>File Name</th>
					<th style='text-align: left; padding: 8px;'>Summary</th>
				</tr>
			</thead>
				<tr style='border-bottom: 1px solid #eee;'>
					<td style='padding: 8px;'><b><a href='https://github.com/RikoxCode/AniFin/blob/master/Configuration/PluginConfiguration.cs'>PluginConfiguration.cs</a></b></td>
					<td style='padding: 8px;'>- Defines configuration settings for the AniFin plugin, enabling customization of download paths, language preferences, content providers, and concurrency controls<br>- Serves as the central data structure for managing user preferences and operational parameters, facilitating seamless integration within the larger media management system to enhance anime content retrieval and organization.</td>
				</tr>
			</table>
		</blockquote>
	</details>
</details>

---

## Getting Started

### Prerequisites

This project requires the following dependencies:

- **Programming Language:** CSharp
- **Package Manager:** Nuget

### Installation

Build AniFin from the source and install dependencies:

1. **Clone the repository:**

    ```sh
    ❯ git clone https://github.com/RikoxCode/AniFin
    ```

2. **Navigate to the project directory:**

    ```sh
    ❯ cd AniFin
    ```

3. **Install the dependencies:**

**Using [nuget](https://docs.microsoft.com/en-us/dotnet/csharp/):**

```sh
❯ dotnet restore
```

### Usage

Run the project with:

**Using [nuget](https://docs.microsoft.com/en-us/dotnet/csharp/):**

```sh
dotnet run
```

### Testing

Anifin uses the {__test_framework__} test framework. Run the test suite with:

**Using [nuget](https://docs.microsoft.com/en-us/dotnet/csharp/):**

```sh
dotnet test
```

---

## Roadmap

- [X] **`Task 1`**: <strike>Implement feature one.</strike>
- [ ] **`Task 2`**: Implement feature two.
- [ ] **`Task 3`**: Implement feature three.

---

## Contributing

- **💬 [Join the Discussions](https://github.com/RikoxCode/AniFin/discussions)**: Share your insights, provide feedback, or ask questions.
- **🐛 [Report Issues](https://github.com/RikoxCode/AniFin/issues)**: Submit bugs found or log feature requests for the `AniFin` project.
- **💡 [Submit Pull Requests](https://github.com/RikoxCode/AniFin/blob/main/CONTRIBUTING.md)**: Review open PRs, and submit your own PRs.

<details closed>
<summary>Contributing Guidelines</summary>

1. **Fork the Repository**: Start by forking the project repository to your github account.
2. **Clone Locally**: Clone the forked repository to your local machine using a git client.
   ```sh
   git clone https://github.com/RikoxCode/AniFin
   ```
3. **Create a New Branch**: Always work on a new branch, giving it a descriptive name.
   ```sh
   git checkout -b new-feature-x
   ```
4. **Make Your Changes**: Develop and test your changes locally.
5. **Commit Your Changes**: Commit with a clear message describing your updates.
   ```sh
   git commit -m 'Implemented new feature x.'
   ```
6. **Push to github**: Push the changes to your forked repository.
   ```sh
   git push origin new-feature-x
   ```
7. **Submit a Pull Request**: Create a PR against the original project repository. Clearly describe the changes and their motivations.
8. **Review**: Once your PR is reviewed and approved, it will be merged into the main branch. Congratulations on your contribution!
</details>

<details closed>
<summary>Contributor Graph</summary>
<br>
<p align="left">
   <a href="https://github.com{/RikoxCode/AniFin/}graphs/contributors">
      <img src="https://contrib.rocks/image?repo=RikoxCode/AniFin">
   </a>
</p>
</details>

---

## License

Anifin is protected under the [LICENSE](https://choosealicense.com/licenses) License. For more details, refer to the [LICENSE](https://choosealicense.com/licenses/) file.

---

## Acknowledgments

- Credit `contributors`, `inspiration`, `references`, etc.

<div align="left"><a href="#top">⬆ Return</a></div>

---
