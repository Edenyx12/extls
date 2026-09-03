# extls CLI Program

*This is a personal project I’m developing for my own use.*

### Installation
> Requires .NET 9.0 or .NET 8.0
```bash
git clone https://github.com/Edenyx12/extls
cd extls

# Standard compilation
dotnet build

# Run with arguments directly
dotnet run --version
```

To use the tool via the `extls` command, you need to add the compiled binary to your PATH.
Use `compile.sh` on Linux. It will compile a ready-to-use binary right next to `compile.sh`. On Windows, you need to add the `.exe` file to your PATH.

---

#### Architecture
The `extls` utility is built on a modular architecture. Every module features its own name, `--version`, and `--help` flag. Running `extls` with the `help` argument lists available modules only.

#### Usage Examples
```bash
# List all available modules
extls modules

# Show help
extls help

# Scan the current directory in the terminal
extls dir scan .

# Uses the NCalc library under the hood
extls calc "2 + 2"
```

---

#### Supported Platforms
* **Linux:** *(Tested on Fedora 44)*
* **Windows:** *(Tested on Windows 11)*