# Installation

## Prerequisites

- **Dyalog APL v20.0 or later** — required to use the generated client code
    - Download from [dyalog.com](https://www.dyalog.com)

## Download

Download the latest binary for your platform from the [GitHub Releases page](https://github.com/Dyalog/OpenAPI/releases/latest).

=== "Windows"

    | Architecture | File |
    |---|---|
    | x64 (most common) | `openapidyalog-win-x64.exe` |
    | ARM64 | `openapidyalog-win-arm64.exe` |

=== "Linux"

    | Architecture | File |
    |---|---|
    | x64 (most common) | `openapidyalog-linux-x64` |
    | ARM64 | `openapidyalog-linux-arm64` |

=== "macOS"

    | Architecture | File |
    |---|---|
    | Apple Silicon (M-series) | `openapidyalog-osx-arm64` |
    | Intel | `openapidyalog-osx-x64` |

## Setup

=== "Windows"

    No additional setup is required. You can run the binary directly from any terminal.

    To make it available from anywhere, move it to a directory on your `PATH`, for example `C:\Windows\System32`, or add its containing folder to your `PATH` in System Settings.

=== "Linux"

    Mark the binary as executable:

    ```bash
    chmod +x openapidyalog-linux-x64
    ```

    Optionally, move it onto your `PATH` so it can be run from anywhere:

    ```bash
    mv openapidyalog-linux-x64 /usr/local/bin/openapidyalog
    ```

=== "macOS"

    macOS quarantines binaries downloaded from the internet. Remove the quarantine attribute before running:

    ```bash
    xattr -d com.apple.quarantine openapidyalog-osx-arm64
    ```

    Then mark it as executable:

    ```bash
    chmod +x openapidyalog-osx-arm64
    ```

    Optionally, move it onto your `PATH` so it can be run from anywhere:

    ```bash
    mv openapidyalog-osx-arm64 /usr/local/bin/openapidyalog
    ```

## Verify

Confirm the tool is working:

=== "Windows"

    ```
    openapidyalog-win-x64.exe --help
    ```

=== "Linux"

    ```bash
    ./openapidyalog-linux-x64 --help
    ```

=== "macOS"

    ```bash
    ./openapidyalog-osx-arm64 --help
    ```

## Next Steps

- [Usage Guide](usage-guide.md) - Learn how to generate and use clients
- [Examples](examples.md) - See practical examples
