# Rules

## 1. Macro Region Syntax Rules

### 1.1 C# Comment Style

```csharp
/* Macro MacroName Parameter1 Parameter2 #LineNumber */
content
/* MacroEnd #LineNumber */
```

- The opening marker must start with `/* Macro`
- MacroName is required and must be a valid identifier
- Parameters are optional and space-separated
- LineNumber must be preceded by `#`
- The closing marker must be `/* MacroEnd #LineNumber */`. The #LineNumber is optional.

### 1.2 C# Region Style

```csharp
#region Macro MacroName Parameter1 Parameter2 #LineNumber
content
#endregion MacroEnd #LineNumber
```

- Must start with `#region Macro`
- Same naming rules as comment style
- Must end with `#endregion MacroEnd #LineNumber`. The #LineNumber is optional.

### 1.3 C# Attribute Style

```csharp
[Macro(MacroName, Parameter1, Parameter2)]
content
```

- Must use the `Macro` attribute
- Parameters are comma-separated
- No line numbers required
- Only valid in C# code files

## 2. MacroTemplateCode Rules

### 2.1 File Location Resolution

1. For C# attribute macros:
   - Must be in TargetMacroCli
   - Must implement required interfaces

2. For comment/region macros:
   - Search order:
     1. TargetMacroCli
     2. Local Macros folder
     3. Parent folders' Macros folders up to solution root
   - File naming: `{MacroName}.scriban` or `{MacroName}.tt`

### 2.2 Template Types

1. C# Templates:
   - Required for attribute-style macros
   - Must follow C# syntax rules
   - Must implement macro interfaces

2. Scriban Templates:
   - Valid for comment/region style
   - Must follow Scriban syntax
   - Access to macro parameters via template variables

3. T4 Templates:
   - Valid for comment/region style
   - Must follow T4 syntax
   - Standard T4 directives supported

## 3. Execution Rules

### 3.1 Processing Order

1. Solution level processing
2. Project level processing
3. File level processing
4. Macro region processing (top-down)
5. Nested macro processing (depth-first)

### 3.2 Error Handling

1. Validation errors stop all processing
2. No partial updates on failure
3. All changes must be atomic
4. Error reporting must include:
   - File location
   - Line numbers
   - Error type
   - Context information

### 3.3 Content Replacement

1. Entire macro region content is replaced
2. Original formatting must be preserved
3. Line numbers must be maintained
4. Nested macros are processed after parent content generation

## 4. Project Structure Rules

### 4.1 Required Folders

1. Solution root must contain:
   - TargetMacroCli (optional)
   - Macros folder (optional)

2. Projects may contain:
   - Local Macros folder
   - Custom macro implementations

### 4.2 File Organization

1. Macro template files:
   - Must be in a Macros folder
   - Must match macro name
   - Must have correct extension (.scriban/.tt)

2. C# macro implementations:
   - Must be in correct namespace
   - Must follow C# naming conventions
   - Must implement required interfaces

## 5. Parameter Rules

### 5.1 Parameter Naming

1. Must be valid identifiers
2. Case-sensitive
3. No duplicates allowed
4. No reserved keywords

### 5.2 Parameter Usage

1. All parameters must be used
2. Type checking where applicable
3. Optional parameters must have defaults
4. Parameter count must match template

## 6. Security Rules

### 6.1 Template Execution

1. No arbitrary code execution
2. Sandboxed environment
3. Limited system access
4. Resource limits enforced

### 6.2 File Access

1. Limited to project scope
2. No system file access
3. No network access
4. Validated paths only

## 7. Performance Rules

### 7.1 Processing Limits

1. Maximum nesting depth
2. Maximum macro count per file
3. Maximum parameter count
4. Maximum content size

### 7.2 Resource Usage

1. Memory limits
2. Processing time limits
3. File size limits
4. Template size limits