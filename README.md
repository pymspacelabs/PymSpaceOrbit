# ☄️ PymGameOrbit - A decision card game about space exploration and survivalism  

## 👨🏻‍🚀 About
This project was created to NASA Space Apps Challenge 2026. The target challenge is to build a Junior Mission Trainer game app for children 7-12 years old
## ⚡ Getting Started
Steps to use this project:
1.  Clone the repository
2. Install the [dotnet sdk](https://dotnet.microsoft.com/en-us/download/dotnet/11.0)  
3. Install the maui workload with following command
```bash
dotnet workload install maui
```

## 🔧 Building and Running

After the setup of dotnet dependencies (sdk, runtime, maui and blazor wasm workload) run the following set of commands:

### 🔨 Build the Project


1. At the repository root folder, run the following commands;

2. Restore the project dependencies

```bash
dotnet restore
```

3. Build the solution: 

```bash
dotnet build
``` 

### ▶ Running and Settings

1. Run the project: 
- **Android**:

```bash
dotnet run --project ./src/PymSpaceOrbit/PymSpaceOrbit.csproj -f net11.0-android
```   
   
- **iOS**:

```bash
dotnet run --project ./src/PymSpaceOrbit/PymSpaceOrbit.csproj -f net11.0-ios
```

- **MacCatalyst**:

```bash
dotnet run --project ./src/PymSpaceOrbit/PymSpaceOrbit.csproj -f net11.0-maccatalyst
```   
   
- **Web (Blazor WASM)**:

```bash
dotnet run --project ./src/PymSpaceOrbit.Web/PymSpaceOrbit.Web.csproj  
```

- **Windows**:

```bash
dotnet run --project ./src/PymSpaceOrbit/PymSpaceOrbit.csproj -f net11.0-windows10.0.19041.0
```   
  
