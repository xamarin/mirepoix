// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Reflection;

using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;

namespace Xamarin.MSBuild.Tooling
{
    static class ConditionParser
    {
        static readonly Type parserType;
        static readonly Type parserOptionsType;
        static readonly MethodInfo parserParseMethod;

        static readonly Type equalExpressionNodeType;

        static readonly Type operatorExpressionNodeType;
        static readonly PropertyInfo operatorExpressionNodeLeftChildProperty;
        static readonly PropertyInfo operatorExpressionNodeRightChildProperty;

        static readonly Type stringExpressionNodeType;
        static readonly FieldInfo stringExpressionNodeValueField;

        static readonly BindingFlags bindingFlags =
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic;

        static ConditionParser ()
        {
            var msbuildAssembly = typeof (Project).Assembly;
            
            parserType = msbuildAssembly.GetType ("Microsoft.Build.Evaluation.Parser");
            parserOptionsType = msbuildAssembly.GetType ("Microsoft.Build.Evaluation.ParserOptions");
            parserParseMethod = parserType.GetMethod (
                "Parse",
                bindingFlags | BindingFlags.InvokeMethod,
                null,
                new [] {
                    typeof (string),
                    parserOptionsType,
                    typeof (ElementLocation)
                },
                null);

            equalExpressionNodeType = msbuildAssembly.GetType ("Microsoft.Build.Evaluation.EqualExpressionNode");
            
            operatorExpressionNodeType = msbuildAssembly.GetType ("Microsoft.Build.Evaluation.OperatorExpressionNode");

            operatorExpressionNodeLeftChildProperty = operatorExpressionNodeType.GetProperty ("LeftChild", bindingFlags);
            operatorExpressionNodeRightChildProperty = operatorExpressionNodeType.GetProperty ("RightChild", bindingFlags);
 
            stringExpressionNodeType = msbuildAssembly.GetType ("Microsoft.Build.Evaluation.StringExpressionNode");
            stringExpressionNodeValueField = stringExpressionNodeType.GetField ("_value", bindingFlags);
        }

        static object Parse (string expression, ElementLocation elementLocation = null)
        {
            if (string.IsNullOrEmpty (expression))
                return null;

            var parser = Activator.CreateInstance (parserType, nonPublic: true);

            return parserParseMethod.Invoke (
                parser,
                new object [] {
                    expression,
                    15, // ParserOptions.AllowAll
                    elementLocation
                });
        }

        public static bool TryGetStringEqualExpressionUnexpandedValues (
            string condition,
            ElementLocation elementLocation,
            out (string Left, string Right) expression)
        {
            var expressionNode = Parse (condition, elementLocation);

            if (expressionNode != null && equalExpressionNodeType.IsAssignableFrom (expressionNode.GetType ())) {
                var leftChild = operatorExpressionNodeLeftChildProperty.GetValue (expressionNode);
                var rightChild = operatorExpressionNodeRightChildProperty.GetValue (expressionNode);

                if (leftChild?.GetType () == stringExpressionNodeType &&
                    rightChild?.GetType () == stringExpressionNodeType) {
                    expression = (
                        (string)stringExpressionNodeValueField.GetValue (leftChild),
                        (string)stringExpressionNodeValueField.GetValue (rightChild));
                    return true;
                }
            }

            expression = default;
            return false;
        }
    }
}