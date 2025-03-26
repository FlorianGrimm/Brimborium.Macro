# Intend

Brimborium.Macro.Cli will process a TargetSolution. It will process all TargetProjects in the TargetSolution. It will process all TargetFiles in the TargetProject. It will process all MacroRegions in the TargetFile. It will replace the content of the MacroRegionBlock with the calculated content of the invocation of the MacroTemplateCode defined by the MacroRegionBlock and the defined Parameters.
The TargetSolution is defined by the command line parameter.
The TargetSolution contains the SourceCode for the MacroTemplateCode.
MacroTemplateCode is C# code, scriban or T4.

The MacroTemplateCode is defined in the TargetSolution.
The MacroTemplateCode is defined in a MacroFile, in a folder called Macros.

A MarcoRegion is a region in a TargetFile. It defines the name of the macro, it's parameters and the content.
A MarcoRegion can contain other MacroRegions.
The MarcoRegions of a TargetFile define a tree.

Example:

```csharp
// file Example.cs
namespace ExampleNamespace;
class ExampleClass {
    /* Macro NameOfMacro Param1 Param2 #LineNumberOfMacro */
    public Param2 Param1 { get; set; }
    /* MacroEnd #LineNumberOfMacro*/
} 
```

```csharp
// file NameOfMacro.cs
namespace ExampleNamespace;
class NameOfMacro : IMacro {
    public void Execute(MacroContext context){
        // TODO...
    }
} 
```

## Names / Components

* Brimborium.Macro.Cli - Command Line Interface for Brimborium.Macro
* TargetSolution - The solution that should be processed.
* TargetProject - The project that should be processed.
* TargetFile - The file that should be processed.
* MacroRegionBlock - The region in the TargetFile, which content should be replaced, defined by the invocation of the macro.
* MacroTemplateCode - The code that calculates the content of the MacroRegionBlock.
* CustomData - The TargetSolution can contain custom data.
* Transform - The CustomData, TypeDefinitions from the TargetSolution can be transformed to a better suitable data for the MacroTemplateCode.
* TargetMacroCli - The TargetSolution can contain a TargetMacroCli. This contains the customized Brimborium.Macro.Cli. It's allows to host the MacroTemplateCode and the Transform.

## Macro Syntax

Used in the TargetFile.

for c# you can use:

1. Comments

    ```csharp
        /* Macro MacroName MacroParameter1 MacroParameter2 #LineNumber */
        CONTENT
        /* MacroEnd #LineNumber */
    ```

2. Regions

    ```csharp
        #region Macro MacroName MacroParameter1 MacroParameter2 #LineNumber
        CONTENT
        #endregion MacroEnd #LineNumber
    ```

3. Attributes

    ```csharp
        [Macro(MacroName, MacroParameter1, MacroParameter2)]
        CONTENT
    ```

## MacroTemplateCode

Supported template languages:

* C# - must be used if MacroAttribute is used - implemented in MacroTemplateAttributeCode.
* C# - can be used if Comment or Region is used - implemented in MacroTemplateRegionCode.
* Scriban - can be used if Comment or Region is used - implemented in MacroTemplateScribanCode.
* T4 template specifics - can be used if Comment or Region is used - implemented in MacroTemplateT4Code.

## Resolution of the MacroTemplateCode

The MacroName in the MacroRegionBlock in the TargetFile is used to find the MacroTemplateCode defined by these rules:

1. If the MacroRegionBlock is defined by CSharp Code (MacroAttribute or Comment) then TargetMacroCli contains the MacroTemplateCode Code.
2. If the MacroRegionBlock is defined by a Comment or a Region then filename is "{MacroName}.scriban" or "{MacroName}.tt".
3. If 2. applies then the TargetMacroCli can contains the file with the MacroTemplateCode.
4. If 2. applies and 3. does not apply then the TargetFile's folder can have a subfolder called Macros which can contains the file with the MacroTemplateCode.
5. If 2. applies and 3. and 4. does not apply then the TargetFile's folder can have a subfolder called Macros which can contains the file with the MacroTemplateCode.
6. Continue with the parent folder until the root folder is reached. - The root folder is the folder of the TargetSolution.
7. If 2. applies and it's not found then an error is thrown.

## Execution of a Macro

The MacroRegionBlock defines the MacroName and the MacroParameters.
Try to find the MacroTemplateCode as defined above.
The MacroTemplateCode is loaded, depended on the type of the MacroTemplateCode.
The MacroTemplateCode is executed with the MacroParameters the results are stored in the MacroRegionResult.
The MacroRegionResult can be shown to the user - optional.
The MacroRegionResult is used to replace the content of the MacroRegionBlock.
The child MacroRegionBlock are processed as well.
If a error occurs the processing is stopped and nothing will be changed.

## Transform

The TargetMacroCli contains the CSharp code for the transformation of the CustomData.
The some CustomData can be loaded from JSON files.
Some of the CustomData is based on the TypeDefinitions of the TargetSolution.

