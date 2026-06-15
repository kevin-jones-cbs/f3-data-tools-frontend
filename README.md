# F3 South Fork Tools Front End

This is the .NET Blazor WASM Front End

## Backend Actions

Backend calls go through `F3Wasm/Helpers/LambdaHelper.cs`. Set `FunctionInput.Action` with the shared `F3Core.LambdaActions` constants instead of string literals so the frontend stays aligned with backend dispatch and smoke-test coverage.

When `f3-data-tools-core` changes, rebuild `F3Core.dll` and copy it into `F3Wasm/Packages/F3Core.dll` before rebuilding this project.

<img width="461" alt="image" src="https://github.com/user-attachments/assets/3bd910f3-c062-4aa8-ba16-56a88b2e5733" />
<img width="456" alt="image" src="https://github.com/user-attachments/assets/5efecb67-73d3-4283-95d5-d15908e36c1b" />
<img width="383" alt="image" src="https://github.com/user-attachments/assets/d1289609-2899-463f-830a-014314323194" />
<img width="385" alt="image" src="https://github.com/user-attachments/assets/e4a4ace7-1df4-418b-8dd4-4ff769916009" />


