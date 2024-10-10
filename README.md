# Brimborium.Macro

Roslyn Codefix that do Macro  / Snippet

Not ready yet

## Idea

yourfile.cs

```c#
#region Macro {{ MacroName Arguments }}
#endregion
```

plus Macro.MacroName.scriban-cs

```Scriban
...content..
```

Running Codefix

```c#
#region Macro {{ MacroName Arguments }}
...content...
#endregion
```

## Macro vs snippet vs Source Generator

Snippet can be  modified, but not updated after.
Source Generator can update the content, but you cannot change the source geneator (or bette only not so easily - restart Visual Studio).
Changing the Macro and rerun - changes all content. (I hope).

## Syntax

```c#
#region Macro {{ MacroName Arguments }}
...content...
#endregion
```

or comment

```c#
/* Macro {{ MacroName Arguments }} */
...content...
/* EndMacro*/
```

if analyser allow this

## Macro Language

May be ..

- Scriban
- c#
- t4 with https://github.com/mono/t4

## Challange

- Specify regions that can be customized AND updated.
- Spanning Macros over many regions.
- Performance / Caching
