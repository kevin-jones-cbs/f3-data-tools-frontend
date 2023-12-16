# F3 South Fork Tools

## Basic Structure
- F3Wasm - This it the .NET Blazor Front End
- F3Lambda - This is the AWS Lambda function(s) Backend
- F3Core - .NET dll for shared functionality between FE and BE

- Both the wasm and Lambda apps will auto deploy when committed to the `main` branch.
- The python lambda function will only deploy if the word `python` is in the commit.
- The data coming from the Spreadsheet is cached using Momento. 
