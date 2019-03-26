// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Xamarin.Cecil.Rocks
{
    public abstract class MetadataVisitor : MetadataVisitor<bool>
    {
        public override bool ShouldTraverseInto (bool visitResult)
            => visitResult;

        protected override bool VisitDefault (object any)
            => true;
    }

    public abstract class MetadataVisitor<TResult>
    {
        public virtual bool ShouldTraverseInto (TResult visitResult)
            => true;

        protected virtual TResult VisitDefault (object any)
            => default;

        public virtual TResult VisitAssemblyDefinition (
            AssemblyDefinition assemblyDefinition)
            => VisitDefault (assemblyDefinition);

        public virtual TResult VisitModuleReference (
            ModuleReference moduleReference)
            => VisitDefault (moduleReference);

        public virtual TResult VisitModuleDefinition (
            ModuleDefinition moduleDefinition)
            => VisitDefault (moduleDefinition);

        public virtual TResult VisitTypeReference (
            TypeReference typeReference)
            => VisitDefault (typeReference);

        public virtual TResult VisitTypeDefinition (
            TypeDefinition typeDefinition)
            => VisitDefault (typeDefinition);

        public virtual TResult VisitInterfaceImplementation (
            InterfaceImplementation interfaceImplementation)
            => VisitDefault (interfaceImplementation);

        public virtual TResult VisitFieldReference (
            FieldReference fieldReference)
            => VisitDefault (fieldReference);

        public virtual TResult VisitFieldDefinition (
            FieldDefinition fieldDefinition)
            => VisitDefault (fieldDefinition);

        public virtual TResult VisitParameterDefinition (
            ParameterDefinition parameterDefinition)
            => VisitDefault (parameterDefinition);

        public virtual TResult VisitMethodReference (
            MethodReference methodReference)
            => VisitDefault (methodReference);

        public virtual TResult VisitMethodDefinition (
            MethodDefinition methodDefinition)
            => VisitDefault (methodDefinition);

        public virtual TResult VisitMethodBody (
            MethodBody methodBody)
            => VisitDefault (methodBody);

        public virtual TResult VisitInstruction (
            Instruction instruction)
            => VisitDefault (instruction);

        public virtual TResult VisitPropertyReference (
            PropertyReference propertyReference)
            => VisitDefault (propertyReference);

        public virtual TResult VisitPropertyDefinition (
            PropertyDefinition propertyDefinition)
            => VisitDefault (propertyDefinition);

        public virtual TResult VisitEventReference (
            EventReference eventReference)
            => VisitDefault (eventReference);

        public virtual TResult VisitEventDefinition (
            EventDefinition eventDefinition)
            => VisitDefault (eventDefinition);

        public virtual TResult VisitGenericParameter (
            GenericParameter genericParameter)
            => VisitDefault (genericParameter);

        public virtual TResult VisitCustomAttribute (
            CustomAttribute customAttribute)
            => VisitDefault (customAttribute);

        public virtual TResult VisitCustomAttributeNamedArgument (
            CustomAttributeNamedArgument customAttributeNamedArgument)
            => VisitDefault (customAttributeNamedArgument);

        public virtual TResult VisitCustomAttributeArgument (
            CustomAttributeArgument customAttributeArgument)
            => VisitDefault (customAttributeArgument);
    }
}