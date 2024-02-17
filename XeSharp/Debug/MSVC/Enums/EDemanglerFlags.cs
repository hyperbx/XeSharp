namespace XeSharp.Debug.MSVC
{
    public enum EDemanglerFlags
    {
        /// <summary>
        /// Enable full undecoration.
        /// </summary>
        Complete = 0x0000,

        /// <summary>
        /// Remove leading underscores from Microsoft keywords.
        /// </summary>
        NoLeadingUnderscores = 0x0001,

        /// <summary>
        /// Disable expansion of Microsoft keywords.
        /// </summary>
        NoMicrosoftKeywords = 0x0002,

        /// <summary>
        /// Disable expansion of return types for primary declarations.
        /// </summary>
        NoFunctionReturns = 0x0004,

        /// <summary>
        /// Disable expansion of the declaration model.
        /// </summary>
        NoAllocationModel = 0x0008,

        /// <summary>
        /// Disable expansion of the declaration language specifier.
        /// </summary>
        NoAllocationLanguage = 0x0010,

        /// <summary>
        /// Disable expansion of Microsoft keywords on the this type for primary declaration.
        /// </summary>
        NoMicrosoftThisType = 0x0020,

        /// <summary>
        /// Disable expansion of CodeView modifiers on the this type for primary declaration.
        /// </summary>
        NoCodeViewThisType = 0x0040,

        /// <summary>
        /// Disable all modifiers on the "this" type.
        /// </summary>
        NoThisType = 0x0060,

        /// <summary>
        /// Disable expansion of access specifiers for members.
        /// </summary>
        NoAccessSpecifiers = 0x0080,

        /// <summary>
        /// Disable expansion of throw-signatures for functions and pointers to functions.
        /// </summary>
        NoThrowSignatures = 0x0100,

        /// <summary>
        /// Disable expansion of the static or virtual attribute of members.
        /// </summary>
        NoMemberType = 0x0200,

        /// <summary>
        /// Disable expansion of the Microsoft model for user-defined type returns.
        /// </summary>
        NoReturnUserDefinedTypeModel = 0x0400,

        /// <summary>
        /// Undecorate 32-bit decorated names.
        /// </summary>
        Undecorate32BitNames = 0x0800,

        /// <summary>
        /// Undecorate only the name for primary declaration. Returns [scope::]name. Does expand template parameters.
        /// </summary>
        NameOnly = 0x1000,

        /// <summary>
        /// Do not undecorate function arguments.
        /// </summary>
        NoArguments = 0x2000,

        /// <summary>
        /// Do not undecorate special names, such as vtable, vcall, vector, metatype, and so on.
        /// </summary>
        NoSpecialSymbols = 0x4000
    }
}
