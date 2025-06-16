## Overview

Monobuilder is cli tool that improves the process of working out what to build based on what changed in a monorepo.
- Setup what folder is dependent on what in the config
- Feed the tool a set of changes. 
  - From a file or from standard in. This covers all the bases but primarily supports `git diff --name-only` so you can diff between tags, branches and specific commits.
- The tool will print to standard out the exec scripts that need to be built.

> config.schema.json contains the schema for the config file with some documentation for each field

## TODOS
- Discovery will struggle id the project file has the same name every time like package.json in JS projects.

## Example

#### Repo Structure
```
/src
  /SomeApplication
    /Build.ps1
    /<Source code for the application>
  /Core
    /LibraryCode
      /<Common Library code used by many applications>
    /<Maybe other libraries>
```

#### Config.json 
```JSON
{
  "Projects": [
    {
      "Name": "SomeApplication",
      "Path": "src/SomeApplication",
      "Dependencies": [ "SomeLibrary" ],
      "Exec": "./src/SomeApplication/Build.ps1"
    },
    {
      "Name": "SomeLibrary",
      "Path": "src/Core/LibraryCode"
    }
  ]
}
```

With the above config any changes to in the `src/Core/LibraryCode` folder or the `src/SomeApplication` will get `./src/SomeApplication/Build.ps1` printed in standard out so you know to build it.

The `exec` field doesnt need to point to a script the expectation is you pipe it into what ever tools you have to build. This just demoes a basic example of each exec being piped into powershell.

## Usage
This application is setup to compile to an all in one AOT exe for portability but the recommendation would be vet the codebase and then add it to the monorepo and run from source as part of a pipeline.

## Discovery
In complex projects it can be annoying to add all the applications and libraries to config.json. The application supports `Discoverers` to dynamically find dependencies and map them. The only one implemented in the repo now
is a C# one because its what the tool is built in so its easy to test. As part of porting the system into your monorepo you can add what you need.

### Example

The following config is taken from the root of this repo to demo how the c sharp discoverer works by looking in the csproj files to find dependencies

#### Repo Structure of test projects
```
/TestApi
/TestConsole
/TestLib
/NestedTestLib
```

#### Dependencies
```
TestLib -> NestedTestLib
TestApi -> TestLib
TestConsole -> NestedTestLib
TestConsole -> TestLib
```

TestConsole has redundant dependencies to prove deduplication checks in the tool

#### config.json
```JSON
{
  "Projects": [
    {
      "Name": "TestApi",
      "Path": "TestApi",
      "Dependencies": [ "CSharpDiscoverer" ],
      "Exec": "test api output"
    },
    {
      "Name": "TestConsole",
      "Path": "TestConsole",
      "Dependencies": [ "CSharpDiscoverer" ],
      "Exec": "test console output"
    }
  ]
}
```

In this system changes to code in each folder will trigger output as below

#### TestApi
```
test api output
```

#### TestConsole
```
test console output
```

#### TestLib
```
test api output
test console output
```

#### NestedTestLib
```
test api output
test console output
```

## Dev

Monobuilder is the core project with the main code. ConsoleRunner is adaption to System.Commandline. Its pulled out if for somereason someone wanted to interact with the core code in a different way.

- run.ps1 build the tool in AOT and copies it to the root for testing with the TestProjects
- send.bat is a script for pushing changes