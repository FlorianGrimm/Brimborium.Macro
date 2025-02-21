﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Macro;

public static class DocumentUtility {
    private static PropertyInfo? _PropertyInfoDocumentDocumentState;
    private static PropertyInfo? _PropertyInfoDocumentStateIsGenerated;
    private static Type? _TypeDocumentState;

    // happy life with internals

    public static bool GetIsGenerated(Document document) {
        if (_PropertyInfoDocumentDocumentState is null) {
            lock (typeof(BrainstormIdea)) {
                _PropertyInfoDocumentDocumentState = typeof(Microsoft.CodeAnalysis.Document).GetProperty("DocumentState", System.Reflection.BindingFlags.NonPublic);
                if (_PropertyInfoDocumentDocumentState is null) { return false; }
            }
        }

        var documentState = _PropertyInfoDocumentDocumentState.GetValue(document);
        if (documentState is null) { return false; }

        if (_TypeDocumentState is null) {
            lock (typeof(BrainstormIdea)) {
                _TypeDocumentState = documentState.GetType();
                if (_TypeDocumentState is null) { return false; }
            }
        }

        if (_PropertyInfoDocumentStateIsGenerated is null) {
            lock (typeof(BrainstormIdea)) {
                _PropertyInfoDocumentStateIsGenerated = _TypeDocumentState.GetProperty("IsGenerated", System.Reflection.BindingFlags.Public);
                if (_PropertyInfoDocumentStateIsGenerated is null) { return false; }
            }
        }

        if (_PropertyInfoDocumentStateIsGenerated.GetValue(documentState) is bool result) {
            return result;
        } else {
            return false;
        }
        /*
            if (0 < documentFolders.Count
                && documentFolders[0] == "obj") {
                // System.Console.WriteLine($"  Generated Document: {document.FilePath}");
            } else {
                System.Console.WriteLine($"  Code Document     : {document.FilePath}");
            }
         */
    }
}
