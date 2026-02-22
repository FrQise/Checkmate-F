# CHECKMATE-F: Forensic File Integrity & Audit Suite (v0.1)

CHECKMATE-F is a C# console application designed for directory monitoring and forensic integrity verification. It allows users to create state-snapshots of specific directories and perform audits to detect unauthorized modifications, deletions, or renames by comparing live data against a trusted baseline.

## Core Features
- **Dual-Mode Verification:** Supports both Deep Verification (cryptographic hash comparison) and Fast Check (metadata analysis of size and timestamps) to balance between security and execution speed.
- **Parallel Processing Engine:** Utilizes the Task Parallel Library to process file identities across all available CPU cores, optimizing disk I/O throughput during large-scale scans.
- **Intelligent Rename Tracking:** Analyzes file hashes to identify moved or renamed assets, distinguishing them from standard deletion and addition events.
- **Quantified Data Impact:** Calculates and reports the total volume of modified data in megabytes (MB), providing a metric for the scale of detected discrepancies.

## Technical Stack
- **Language:** C# / .NET
- **UI:** Custom Console Framework
- **Data Format:** .audit (JSON-encapsulated snapshot)
- **Architecture:** Concurrent collection architecture with SHA-256 integrity checks.

## Functional Overview

### Create Snapshot
Generates a baseline record of a target directory. The engine recursively scans the file system and generates a unique identity for every file, including its relative path, cryptographic hash, byte size, and last modified timestamp. This data is saved into a `.audit` container.

### Run Audit
Loads a previously created `.audit` file and compares it against the current state of the directory.
* **Deep Verification:** Performs a full binary read of every file to ensure bit-for-bit consistency. This is the primary method for detecting silent data corruption or manual file edits.
* **Fast Check:** Compares only file sizes and timestamps. This allows for near-instant auditing of massive data volumes where file content modification is less likely than accidental deletion or replacement.

### Reporting and Logging
Upon completion, the tool generates a formatted audit report. This report categorizes every discrepancy found and provides a summary including the count of added, modified, renamed, and deleted files. A session log is automatically saved to the snapshot's directory for permanent record-keeping.

## Installation & Usage

### Option 1: Quick Start
1. Download the latest `CheckmateF_vX.X.X.zip` from the Releases page.
2. Extract the archive and execute `CheckmateF.exe`.
3. Select "Create Snapshot" to define your initial baseline.

### Option 2: Build from Source
1. Clone the repository:
   `git clone https://github.com/FrQise/Checkmate-F.git`
2. Build and Run:
   Use Visual Studio or the dotnet CLI:
   `dotnet run`

## Controls
- **Arrow Keys:** Navigate the menu system.
- **Enter:** Confirm selection and proceed through the audit wizard.

## File Format Specification (.audit)
The `.audit` format utilizes a structured JSON schema for portability and forensic transparency:

- **Header:** Contains the source directory path, creation timestamp, and a user-defined description.
- **File Identities:** A list of records containing the relative path, SHA-256 hash, file size in bytes, and the OS-reported last modified date.

## IMPORTANT NOTES AND DISCLAIMERS

### Detection Limits
The "Fast Check" mode relies on operating system metadata. While efficient, it is possible for sophisticated software or forensic tools to modify a file and subsequently "timestomp" the metadata to match the original. For critical security audits, Deep Verification is the only way to ensure total integrity.

### Resource Intensity
During a Deep Verification audit, the application will saturate available CPU and Disk I/O resources to complete the scan as quickly as possible. Users should expect a temporary increase in system resource consumption during these operations.

### User Responsibility
This software is provided "as is." It is a monitoring tool and does not provide "Real-Time" protection or file locking. It is intended to detect changes after they have occurred. Ensure that your baseline snapshots are stored in a secure, read-only location to prevent the baseline itself from being compromised.
