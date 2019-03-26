// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Xamarin.Cecil.Rocks
{
    public static class AcceptExtensions
    {
        public static void AcceptVisitor<TResult> (
            this IEnumerable<AssemblyDefinition> assemblyDefinitions,
            MetadataVisitor<TResult> visitor)
        {
            foreach (var assemblyDefinition in assemblyDefinitions)
                assemblyDefinition.AcceptVisitor (visitor);
        }

        public static void AcceptVisitor<TResult> (
            this AssemblyDefinition assemblyDefinition,
            MetadataVisitor<TResult> visitor)
        {
            if (!visitor.ShouldTraverseInto (visitor.VisitAssemblyDefinition (assemblyDefinition)))
                return;

            if (assemblyDefinition.HasCustomAttributes)
                assemblyDefinition.CustomAttributes.AcceptVisitor (visitor);

            foreach (var module in assemblyDefinition.Modules)
                module.AcceptVisitor (visitor);
        }

        public static void AcceptVisitor<TResult> (
            this ModuleReference moduleReference,
            MetadataVisitor<TResult> visitor)
        {
            if (!visitor.ShouldTraverseInto (visitor.VisitModuleReference (moduleReference)))
                return;
        }

        public static void AcceptVisitor<TResult> (
            this ModuleDefinition moduleDefinition,
            MetadataVisitor<TResult> visitor)
        {
            if (!visitor.ShouldTraverseInto (visitor.VisitModuleDefinition (moduleDefinition)))
                return;

            if (moduleDefinition.HasCustomAttributes)
                moduleDefinition.CustomAttributes.AcceptVisitor (visitor);

            foreach (var type in moduleDefinition.Types)
                type.AcceptVisitor (visitor);
        }

        public static void AcceptVisitor<TResult> (
            this TypeReference typeReference,
            MetadataVisitor<TResult> visitor)
        {
            if (!visitor.ShouldTraverseInto (visitor.VisitTypeReference (typeReference)))
                return;

            if (typeReference.IsGenericInstance &&
                typeReference is GenericInstanceType genericTypeReference &&
                genericTypeReference.HasGenericArguments) {
                foreach (var genericArgument in genericTypeReference.GenericArguments)
                    genericArgument.AcceptVisitor (visitor);
            }

            var elementTypeReference = typeReference.GetElementType ();
            if (elementTypeReference != null && elementTypeReference != typeReference)
                elementTypeReference.AcceptVisitor (visitor);

            typeReference.DeclaringType?.AcceptVisitor (visitor);
        }

        public static void AcceptVisitor<TResult> (
            this TypeDefinition typeDefinition,
            MetadataVisitor<TResult> visitor)
        {
            if (!visitor.ShouldTraverseInto (visitor.VisitTypeDefinition (typeDefinition)))
                return;

            typeDefinition.BaseType?.AcceptVisitor (visitor);

            if (typeDefinition.HasCustomAttributes)
                typeDefinition.CustomAttributes.AcceptVisitor (visitor);

            if (typeDefinition.HasGenericParameters)
                typeDefinition.GenericParameters.AcceptVisitor (visitor);

            if (typeDefinition.HasInterfaces) {
                foreach (var iface in typeDefinition.Interfaces)
                    iface.AcceptVisitor (visitor);
            }

            if (typeDefinition.HasNestedTypes) {
                foreach (var nestedType in typeDefinition.NestedTypes)
                    nestedType.AcceptVisitor (visitor);
            }

            if (typeDefinition.HasFields) {
                foreach (var field in typeDefinition.Fields)
                    field.AcceptVisitor (visitor);
            }

            if (typeDefinition.HasProperties) {
                foreach (var property in typeDefinition.Properties)
                    property.AcceptVisitor (visitor);
            }

            if (typeDefinition.HasEvents) {
                foreach (var @event in typeDefinition.Events)
                    @event.AcceptVisitor (visitor);
            }

            if (typeDefinition.HasMethods) {
                foreach (var method in typeDefinition.Methods)
                    method.AcceptVisitor (visitor);
            }
        }

        public static void AcceptVisitor<TResult> (
            this InterfaceImplementation interfaceImplementation,
            MetadataVisitor<TResult> visitor)
        {
            if (!visitor.ShouldTraverseInto (visitor.VisitInterfaceImplementation (interfaceImplementation)))
                return;

            if (interfaceImplementation.HasCustomAttributes)
                interfaceImplementation.CustomAttributes.AcceptVisitor (visitor);

            interfaceImplementation.InterfaceType.AcceptVisitor (visitor);
        }

        public static void AcceptVisitor<TResult> (
            this FieldReference fieldReference,
            MetadataVisitor<TResult> visitor)
        {
            if (!visitor.ShouldTraverseInto (visitor.VisitFieldReference (fieldReference)))
                return;

            fieldReference.FieldType.AcceptVisitor (visitor);
            fieldReference.DeclaringType.AcceptVisitor (visitor);
        }

        public static void AcceptVisitor<TResult> (
            this FieldDefinition fieldDefinition,
            MetadataVisitor<TResult> visitor)
        {
            if (!visitor.ShouldTraverseInto (visitor.VisitFieldDefinition (fieldDefinition)))
                return;

            if (fieldDefinition.HasCustomAttributes)
                fieldDefinition.CustomAttributes.AcceptVisitor (visitor);

            fieldDefinition.FieldType.AcceptVisitor (visitor);
        }

        public static void AcceptVisitor<TResult> (
            this ParameterDefinition parameterDefinition,
            MetadataVisitor<TResult> visitor)
        {
            if (!visitor.ShouldTraverseInto (visitor.VisitParameterDefinition (parameterDefinition)))
                return;

            if (parameterDefinition.HasCustomAttributes)
                parameterDefinition.CustomAttributes.AcceptVisitor (visitor);

            parameterDefinition.ParameterType.AcceptVisitor (visitor);
        }

        public static void AcceptVisitor<TResult> (
            this MethodReference methodReference,
            MetadataVisitor<TResult> visitor)
        {
            if (!visitor.ShouldTraverseInto (visitor.VisitMethodReference (methodReference)))
                return;

            if (methodReference.HasGenericParameters)
                methodReference.GenericParameters.AcceptVisitor (visitor);

            if (methodReference.HasParameters) {
                foreach (var parameter in methodReference.Parameters)
                    parameter.AcceptVisitor (visitor);
            }

            methodReference.ReturnType.AcceptVisitor (visitor);
            methodReference.DeclaringType.AcceptVisitor (visitor);
        }

        public static void AcceptVisitor<TResult> (
            this MethodDefinition methodDefinition,
            MetadataVisitor<TResult> visitor)
        {
            if (!visitor.ShouldTraverseInto (visitor.VisitMethodDefinition (methodDefinition)))
                return;

            if (methodDefinition.HasCustomAttributes)
                methodDefinition.CustomAttributes.AcceptVisitor (visitor);

            if (methodDefinition.HasGenericParameters)
                methodDefinition.GenericParameters.AcceptVisitor (visitor);

            if (methodDefinition.HasParameters) {
                foreach (var parameter in methodDefinition.Parameters)
                    parameter.AcceptVisitor (visitor);
            }

            methodDefinition.ReturnType.AcceptVisitor (visitor);

            if (methodDefinition.HasBody)
                methodDefinition.Body.AcceptVisitor (visitor);
        }

        public static void AcceptVisitor<TResult> (
            this MethodBody methodBody,
            MetadataVisitor<TResult> visitor)
        {
            if (!visitor.ShouldTraverseInto (visitor.VisitMethodBody (methodBody)))
                return;

            foreach (var instruction in methodBody.Instructions)
                instruction.AcceptVisitor (visitor);
        }

        public static void AcceptVisitor<TResult> (
            this Instruction instruction,
            MetadataVisitor<TResult> visitor)
        {
            if (!visitor.ShouldTraverseInto (visitor.VisitInstruction (instruction)))
                return;

            DynamicAccept (instruction.Operand, visitor);
        }

        public static void AcceptVisitor<TResult> (
            this PropertyReference propertyReference,
            MetadataVisitor<TResult> visitor)
        {
            if (!visitor.ShouldTraverseInto (visitor.VisitPropertyReference (propertyReference)))
                return;

            var parameters = propertyReference.Parameters;
            if (parameters != null) {
                foreach (var parameter in parameters)
                    parameter.AcceptVisitor (visitor);
            }

            propertyReference.PropertyType.AcceptVisitor (visitor);
            propertyReference.DeclaringType.AcceptVisitor (visitor);
        }

        public static void AcceptVisitor<TResult> (
            this PropertyDefinition propertyDefinition,
            MetadataVisitor<TResult> visitor)
        {
            if (!visitor.ShouldTraverseInto (visitor.VisitPropertyDefinition (propertyDefinition)))
                return;

            if (propertyDefinition.HasCustomAttributes)
                propertyDefinition.CustomAttributes.AcceptVisitor (visitor);

            propertyDefinition.PropertyType.AcceptVisitor (visitor);
            propertyDefinition.GetMethod?.AcceptVisitor (visitor);
            propertyDefinition.SetMethod?.AcceptVisitor (visitor);

            if (propertyDefinition.HasParameters) {
                foreach (var parameter in propertyDefinition.Parameters)
                    parameter.AcceptVisitor (visitor);
            }
        }

        public static void AcceptVisitor<TResult> (
            this EventReference eventReference,
            MetadataVisitor<TResult> visitor)
        {
            if (!visitor.ShouldTraverseInto (visitor.VisitEventReference (eventReference)))
                return;

            eventReference.EventType.AcceptVisitor (visitor);
            eventReference.DeclaringType.AcceptVisitor (visitor);
        }

        public static void AcceptVisitor<TResult> (
            this EventDefinition eventDefinition,
            MetadataVisitor<TResult> visitor)
        {
            if (!visitor.ShouldTraverseInto (visitor.VisitEventDefinition (eventDefinition)))
                return;

            if (eventDefinition.HasCustomAttributes)
                eventDefinition.CustomAttributes.AcceptVisitor (visitor);

            eventDefinition.EventType.AcceptVisitor (visitor);
            eventDefinition.AddMethod?.AcceptVisitor (visitor);
            eventDefinition.RemoveMethod?.AcceptVisitor (visitor);
            eventDefinition.InvokeMethod?.AcceptVisitor (visitor);
        }

        public static void AcceptVisitor<TResult> (
            this IEnumerable<GenericParameter> genericParameters,
            MetadataVisitor<TResult> visitor)
        {
            foreach (var genericParameter in genericParameters ?? Array.Empty<GenericParameter> ())
                genericParameter.AcceptVisitor (visitor);
        }

        public static void AcceptVisitor<TResult> (
            this GenericParameter genericParameter,
            MetadataVisitor<TResult> visitor)
        {
            if (!visitor.ShouldTraverseInto (visitor.VisitGenericParameter (genericParameter)))
                return;

            genericParameter.GetElementType ().AcceptVisitor (visitor);

            if (genericParameter.HasConstraints) {
                foreach (var constraintType in genericParameter.Constraints)
                    constraintType.AcceptVisitor (visitor);
            }
        }

        public static void AcceptVisitor<TResult> (
            this IEnumerable<CustomAttribute> customAttributes,
            MetadataVisitor<TResult> visitor)
        {
            foreach (var customAttribute in customAttributes ?? Array.Empty<CustomAttribute> ())
                customAttribute.AcceptVisitor (visitor);
        }

        public static void AcceptVisitor<TResult> (
            this CustomAttribute customAttribute,
            MetadataVisitor<TResult> visitor)
        {
            if (!visitor.ShouldTraverseInto (visitor.VisitCustomAttribute (customAttribute)))
                return;

            customAttribute.AttributeType.AcceptVisitor (visitor);
            customAttribute.Constructor.AcceptVisitor (visitor);

            if (customAttribute.HasConstructorArguments) {
                foreach (var argument in customAttribute.ConstructorArguments)
                    argument.AcceptVisitor (visitor);
            }

            if (customAttribute.HasFields) {
                foreach (var field in customAttribute.Fields)
                    field.AcceptVisitor (visitor);
            }

            if (customAttribute.HasProperties) {
                foreach (var property in customAttribute.Properties)
                    property.AcceptVisitor (visitor);
            }
        }

        public static void AcceptVisitor<TResult> (
            this CustomAttributeNamedArgument customAttributeNamedArgument,
            MetadataVisitor<TResult> visitor)
        {
            if (!visitor.ShouldTraverseInto (visitor.VisitCustomAttributeNamedArgument (customAttributeNamedArgument)))
                return;

            customAttributeNamedArgument.Argument.AcceptVisitor (visitor);
        }

        public static void AcceptVisitor<TResult> (
            this CustomAttributeArgument customAttributeArgument,
            MetadataVisitor<TResult> visitor)
        {
            if (!visitor.ShouldTraverseInto (visitor.VisitCustomAttributeArgument (customAttributeArgument)))
                return;

            customAttributeArgument.Type.AcceptVisitor (visitor);
            DynamicAccept (customAttributeArgument.Value, visitor);
        }

        static void DynamicAccept<TResult> (
            object target,
            MetadataVisitor<TResult> visitor)
        {
            switch (target) {
            case TypeReference typeReference:
                typeReference.AcceptVisitor (visitor);
                break;
            case FieldReference fieldReference:
                fieldReference.AcceptVisitor (visitor);
                break;
            case MethodReference methodReference:
                methodReference.AcceptVisitor (visitor);
                break;
            case VariableDefinition variableDefinition:
                variableDefinition.VariableType.AcceptVisitor (visitor);
                break;
            }
        }
    }
}